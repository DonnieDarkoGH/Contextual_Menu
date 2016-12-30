using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;


namespace ContextualMenu {

    [System.Serializable]
    public class ButtonManagerEvent : UnityEvent<ButtonModel, ButtonsManager.EButtonActionState> {

    }

    public class MenuEventManager : MonoBehaviour {

        public GameObject TargetForEventData;
        public List<UnityEvent> ActionEvents;

        private Dictionary<string, ButtonManagerEvent> eventDictionnary;

        private static MenuEventManager instance;
        public  static MenuEventManager Instance {
            get {
                if (!instance) {
                    instance = FindObjectOfType<MenuEventManager>();

                    if (!instance) {
                        Debug.LogError("You need a MenuEventManager script in your scene !");
                    }
                    else {
                        instance.Init();
                    }
                }
                return instance;
            }
        }

        private void Init() {

            if( eventDictionnary == null) {
                eventDictionnary = new Dictionary<string, ButtonManagerEvent>();
            }
        }

        public static void StartListening(ButtonModel _btnModel, ButtonsManager.EButtonActionState _actionName, UnityAction<ButtonModel, ButtonsManager.EButtonActionState> _listener) {
            //Debug.Log("<b>MenuEventManager</b> StartListening to " + _btnModel.Id);
            ButtonManagerEvent btnModelEvent   = null;
            string             btnModelEventId = _actionName + _btnModel.Id;

            if (instance.eventDictionnary.TryGetValue(btnModelEventId, out btnModelEvent)) {
                btnModelEvent.AddListener(_listener);
            }
            else {

                btnModelEvent = new ButtonManagerEvent();
                btnModelEvent.AddListener(_listener);
                instance.eventDictionnary.Add(btnModelEventId, btnModelEvent);
            }

        }

        public static void StopListening(ButtonModel _btnModel, ButtonsManager.EButtonActionState _actionName, UnityAction<ButtonModel, ButtonsManager.EButtonActionState> _listener) {
            //Debug.Log("<b>MenuEventManager</b> StopListening to " + _btnModel.Id);

            if (instance == null)
                return;

            ButtonManagerEvent btnModelEvent   = null;
            string             btnModelEventId = _actionName + _btnModel.Id;

            if (instance.eventDictionnary.TryGetValue(btnModelEventId, out btnModelEvent)) {
                btnModelEvent.RemoveListener(_listener);
            }
        }

        public static void TriggerEvent(ButtonModel _btnModel, ButtonsManager.EButtonActionState _actionName) {

            ButtonManagerEvent btnModelEvent   = null;
            string             btnModelEventId = _actionName + _btnModel.Id;

            if (instance.eventDictionnary.TryGetValue(btnModelEventId, out btnModelEvent)) {
                btnModelEvent.Invoke(_btnModel, _actionName);
            }

        }

        public bool TryButtonAction(int _index) {
            //Debug.Log("<b>MenuEventManager</b> TryButtonAction " + _index);

            if (_index < 0 || ActionEvents.Count <= _index || ActionEvents[_index].GetPersistentEventCount() == 0) {
                return false;
            }

            ActionEvents[_index].Invoke();

            return true;
        }

    }

}
