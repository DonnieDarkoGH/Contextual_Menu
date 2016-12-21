using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomPieMenu {
    public class ButtonsManager : MonoBehaviour {

        public delegate void ButtonsManagerEvent(ButtonModel btnModel, ButtonModel[] subButtons);
        public event ButtonsManagerEvent OnButtonsUnfolding;
        public event ButtonsManagerEvent OnButtonsFoldingUp;

        [SerializeField] SO_MenuStructure menu;
        [SerializeField] string[] activePathIdToArray;

        private ButtonModel   currentActiveButton = null;
        private Stack<string> activePathIdInMenu  = new Stack<string>();


        void Awake() {
            //Debug.Log("<b>ButtonsManager</b> Awake");
             
            if (menu == null) {
                Debug.LogError("No context for menu");
                return;
            }

            ResetAllButtons();

            int len = menu.Buttons.Count;
            for (int i = 0; i < len; i++) {
                menu.Buttons[i].OnClick          += HandleClick;
                menu.Buttons[i].OnAnimationEnded += HandleEndOfAnimation;
            }

            activePathIdInMenu.Clear();
        }

        public ButtonModel GetRootButton() {
            //Debug.Log("<b>ButtonsManager</b> GetRootButton");

            ButtonModel rootButton = GetButtonFromNode(menu.Root);
            rootButton.State       = ButtonModel.EState.IN_PLACE;

            return GetButtonFromNode(menu.Root);
        }

        public ButtonModel GetButtonFromNode(Node _node) {
            //Debug.Log("<b>ButtonsManager</b> GetButtonFromNode " + _node.ToString());

            byte len = (byte)menu.Buttons.Count;
            string id = _node.Id;

            for (byte i = 0; i < len; i++) {
                if (menu.Buttons[i].Id.Equals(id)) {
                    return menu.Buttons[i];
                }
            }

            return null;
        }

        public ButtonModel[] GetSubMenuFromNode(Node _node) {
            //Debug.Log("<b>ButtonsManager</b> GetSubMenuFromNode " + _node.ToString());

            if (_node == null) {
                return null;
            }

            byte len = (byte)_node.SubNodes.Count;
            ButtonModel[] subMenus = new ButtonModel[len];

            for (byte i = 0; i < len; i++) {
                subMenus[i] = GetButtonFromNode(_node.SubNodes[i]);
            }

            return subMenus;
        }

        public ButtonModel GetParentButton(ButtonModel _btnModel) {
            //Debug.Log("<b>ButtonsManager</b> GetParentButton " + _btnModel.ToString());

            Node btnNode    = menu.Root.GetNode(_btnModel.Id);
            Node parentNode = null;

            if (btnNode != null) {
                parentNode = menu.Root.GetNode(btnNode.GetParentId());
            }

            return parentNode == null ? null : GetButtonFromNode(parentNode);
        }

        public void ResetAllButtons() {
            //Debug.Log("<b>ButtonsManager</b> ResetAllButtons");

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

            if (!IsAnimationProcessing()) {
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

            Node btnNode  = menu.Root.GetNode(activePathIdInMenu.Pop());
            ButtonModel btnModel = GetButtonFromNode(btnNode);

            if (btnNode.SubNodes.Count > 0) {
                OnButtonsFoldingUp(btnModel, GetSubMenuFromNode(btnNode));
            }
            else {
                btnModel.State = ButtonModel.EState.IN_PLACE;
                SwitchMenu();
            }

            return true;
        }

        private void UnfoldMenu() {
            //Debug.Log("<b>ButtonsManager</b> UnfoldMenu from " + _btnModel.ToString());

            Node btnNode = menu.Root.GetNode(currentActiveButton.Id);

            if (btnNode.SubNodes.Count > 0) {
                OnButtonsUnfolding(currentActiveButton, GetSubMenuFromNode(btnNode));
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

        private void LateUpdate() {
            activePathIdToArray = activePathIdInMenu.ToArray();
        }

        private void OnDisable() {
            //Debug.Log("<b>ButtonsManager</b> OnDisable");

            int len = menu.Buttons.Count;
            for (int i = 0; i < len; i++) {
                menu.Buttons[i].OnClick          -= HandleClick;
                menu.Buttons[i].OnAnimationEnded -= HandleEndOfAnimation;
            }
        }

    }
}
