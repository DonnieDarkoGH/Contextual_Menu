using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using ContextualMenuData;

namespace ContextualMenu {

    public class ButtonsManager : MonoBehaviour {

        public enum EButtonActionState {
            NONE = 0,
            CLICKED,
            RELEASED,
            ANIMATION_STARTED,
            ANIMATION_ENDED,
            RETRACTING,
            EXPANDING,
            DESTROYED
        }

        private UnityAction<ButtonModel, EButtonActionState> ButtonModelAction;

        [SerializeField] private ScriptableMenuStructure menu;
        private ButtonModel   currentActiveButton = null;
        private ButtonModel[] currentSubButtons;
        private Stack<string> activePathIdInMenu  = new Stack<string>();
        
        public int CurrentLevel {
            get { return activePathIdInMenu.Count; }
        }

        public ButtonModel[] CurrentSubButtons {
            get { return currentSubButtons; }
        }

        public void Init(ScriptableMenuStructure _menu) {
            //Debug.Log("<b>ButtonsManager</b> Init");

            if (_menu == null) {
                Debug.LogError("No context for menu");
                return;
            }
            menu = _menu;

            ResetAllButtons();

            if (ButtonModelAction == null) {
                ButtonModelAction = new UnityAction<ButtonModel, EButtonActionState>(HandleButtonAction);
            }

            int len = menu.Buttons.Count;
            for (int i = 0; i < len; i++) {
                MenuEventManager.StartListening(menu.Buttons[i], EButtonActionState.CLICKED            , ButtonModelAction);
                //MenuEventManager.StartListening(menu.Buttons[i], EButtonActionState.ANIMATION_STARTED, ButtonModelAction);
                MenuEventManager.StartListening(menu.Buttons[i], EButtonActionState.ANIMATION_ENDED    , ButtonModelAction);
                MenuEventManager.StartListening(menu.Buttons[i], EButtonActionState.RETRACTING         , MenuManager.Instance.ButtonsManagerAction);
                MenuEventManager.StartListening(menu.Buttons[i], EButtonActionState.EXPANDING          , MenuManager.Instance.ButtonsManagerAction);

            }

            activePathIdInMenu.Clear();
        }

        public ButtonModel GetRootButton() {
            //Debug.Log("<b>ButtonsManager</b> GetRootButton");

            return menu.Buttons[0];
        }

        public void ResetAllButtons() {
            //Debug.Log("<b>ButtonsManager</b> ResetAllButtons");

            StopListening();

            int len = menu.Buttons.Count;
            for (int i = 0; i < len; i++) {
                menu.Buttons[i].Reset();
            }
        }

        public void HandleEndOfAnimation(ButtonModel _btnModel) {
            //Debug.Log("<b>ButtonsManager</b> HandleEndOfAnimation for " + _btnModel.ToString());

            switch (_btnModel.State) {

                case ButtonModel.EState.RETRACTING:
                    _btnModel.Reset();
                    if(currentActiveButton.State == ButtonModel.EState.FOLDING || currentActiveButton.State == ButtonModel.EState.UNFOLDING) {
                        SwitchMenu();
                    }
                    break;

                case ButtonModel.EState.DEPLOYING:
                    _btnModel.State = ButtonModel.EState.IN_PLACE;

                    if (!activePathIdInMenu.Contains(currentActiveButton.Id)) {
                        activePathIdInMenu.Push(currentActiveButton.Id);
                    }

                    break;

                default:
                    break;
            }

            if (!IsAnimationProcessing() && currentActiveButton != null) {
                currentActiveButton.State = ButtonModel.EState.IN_PLACE;
            }
        }

        private void HandleClick(ButtonModel _btnModel) {
            //Debug.Log("<b>ButtonsManager</b> HandleClick on " + _btnModel.ToString());

            if (IsAnimationProcessing())
                return;

            currentActiveButton = _btnModel;

            if (activePathIdInMenu.Contains(_btnModel.Id)) {
                _btnModel.State = ButtonModel.EState.FOLDING;
            }
            else {
                _btnModel.State = ButtonModel.EState.UNFOLDING;
            }

            SwitchMenu();

            MenuEventManager.Instance.TryButtonAction(menu.Buttons.IndexOf(_btnModel));

        }

        private void SwitchMenu() {
            //Debug.Log("<b>ButtonsManager</b> SwitchMenu");

            if (currentActiveButton == null)
                return;

            if (currentActiveButton.GetLevel() < activePathIdInMenu.Count) {
                RetractSubMenus();
            }
            else {
                if (currentActiveButton.State == ButtonModel.EState.UNFOLDING) {
                    UnfoldMenu();
                    activePathIdInMenu.Push(currentActiveButton.Id);
                }
                currentActiveButton.State = ButtonModel.EState.IN_PLACE;
            }

        }

        private bool RetractSubMenus() {
            //Debug.Log("<b>ButtonsManager</b> RetractSubMenus");

            ButtonModel btnModel = menu.GetButtonfromId(activePathIdInMenu.Pop());
            currentSubButtons    = menu.GetChildren(btnModel);

            if(currentSubButtons.Length > 0) {
                MenuEventManager.TriggerEvent(btnModel, EButtonActionState.RETRACTING);
            }
            else {
                btnModel.State = ButtonModel.EState.IN_PLACE;
                SwitchMenu();
            }

            return true;
        }

        private void UnfoldMenu() {
            //Debug.Log("<b>ButtonsManager</b> UnfoldMenu from " + _btnModel.ToString());

            currentSubButtons = menu.GetChildren(currentActiveButton);

            if (currentSubButtons.Length > 0) {
                    MenuEventManager.TriggerEvent(currentActiveButton, EButtonActionState.EXPANDING);
            }
        }

        private bool IsAnimationProcessing() {
            //Debug.Log("<b>ButtonsManager</b> IsAnimationProcessing");

            bool isProcessing = false;

            int len = menu.Buttons.Count;
            for (int i = 0; i < len; i++) {
                isProcessing = isProcessing || menu.Buttons[i].IsMoving;
            }

            //Debug.Log("IsAnimationProcessing : " + isProcessing);
            return isProcessing;
        }

        private void StopListening() {
            //Debug.Log("<b>ButtonsManager</b> StopListening");
            int len = menu.Buttons.Count;
            for (int i = 0; i < len; i++) {
                MenuEventManager.StopListening(menu.Buttons[i], EButtonActionState.CLICKED            , ButtonModelAction);
                //MenuEventManager.StopListening(menu.Buttons[i], EButtonActionState.ANIMATION_STARTED, ButtonModelAction);
                MenuEventManager.StopListening(menu.Buttons[i], EButtonActionState.ANIMATION_ENDED    , ButtonModelAction);

                MenuEventManager.StopListening(menu.Buttons[i], EButtonActionState.RETRACTING, MenuManager.Instance.ButtonsManagerAction);
                MenuEventManager.StopListening(menu.Buttons[i], EButtonActionState.EXPANDING, MenuManager.Instance.ButtonsManagerAction);
            }

        }

        private void OnDisable() {
            //Debug.Log("<b>ButtonsManager</b> OnDisable");
            StopListening();
        }

        private void OnDestroy() {
            //Debug.Log("<b>ButtonsManager</b> OnDestroy");
            StopListening();
        }

        private void HandleButtonAction(ButtonModel _btnModel, EButtonActionState _btnActionState) {
            //Debug.Log("HandleButtonAction on " + _btnModel.Id + " : " + _btnActionState.ToString());

            switch (_btnActionState) {

                case EButtonActionState.CLICKED:
                    HandleClick(_btnModel);
                    break;

                case EButtonActionState.ANIMATION_ENDED:
                    HandleEndOfAnimation(_btnModel);
                    break;

                default:
                    break;
            }
        }

    }
}
