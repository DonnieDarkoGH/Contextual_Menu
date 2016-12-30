using UnityEngine;
using UnityEngine.Events;
using ContextualMenuData;

namespace ContextualMenu {

    public class MenuManager : MonoBehaviour {

        enum ETweenMode { LINEAR, EASE_IN, CURVE }
        private static MenuManager instance;

        public UnityAction<ButtonModel, ButtonsManager.EButtonActionState> ButtonsManagerAction;

        [SerializeField] private ScriptableMenuStructure menu;
        [SerializeField] private ButtonsManager    buttonsManagerRef = null;
        [SerializeField] private GameObject        buttonPrefab      = null;
        [SerializeField] private ETweenMode TweeningMode;
        [SerializeField] [Range(1.0f, 5.0f)]  private float spacing     = 1.5f;
        [SerializeField] [Range(1.0f, 10.0f)] private float tweenSpeed  = 3.5f;
        [SerializeField] private AnimationCurve TweeningCurve = AnimationCurve.Linear(0.0f, 0.0f, 1.0f,1.0f);

        public static MenuManager Instance {
            get {
                if (!instance) {
                    instance = FindObjectOfType<MenuManager>();

                    if (!instance) {
                        Debug.LogError("A MenuManager script is needed in the scene !");
                    }
                }

                return instance;
            }
        }

        public float Spacing {
            get {
                if(buttonsManagerRef == null || buttonsManagerRef.CurrentLevel > 0) {
                    return spacing;
                }
                else {
                    return spacing * 0.8f;
                }
                
            }
        }

        public float TweenSpeed {
            get {
                return tweenSpeed;
            }
        }

        // Use this for initialization
        void Awake() {

            if (instance == null) {
                instance = this;
            }

            if (MenuEventManager.Instance) { };

            if (ButtonsManagerAction == null) {
                ButtonsManagerAction = new UnityAction<ButtonModel, ButtonsManager.EButtonActionState>(HandleButtonsManagerAction);
            }

            if (buttonsManagerRef == null) {
                buttonsManagerRef = GetComponent<ButtonsManager>();
            }
            buttonsManagerRef.Init(menu);

            GameObject newBtnGo = Instantiate(buttonPrefab, Vector3.zero, Quaternion.identity, transform) as GameObject;
            PieButton  newBtn   = newBtnGo.GetComponent<PieButton>();

            if (newBtn != null) {
                newBtn.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;
                newBtn.Init(_btnModel: buttonsManagerRef.GetRootButton(), _angularPos: 0, _isLinked: false);
            }

        }

        public void CreateNewMenu() {
            Debug.Log("<b>ButtonManager</b> CreateNewMenu");
            ScriptableMenuStructure.CreateInstance("ScriptableMenuStructure");
        }
        
        public Vector3 TweenPosition(Vector3 _startPoint, Vector3 _targetPoint, float delta) {

            Vector3 movingPosition = Vector3.zero;

            switch (TweeningMode) 
            {
                    case ETweenMode.LINEAR:
                        movingPosition = Linear(_startPoint, _targetPoint, delta);
                        break;

                    case ETweenMode.EASE_IN:
                        movingPosition = EaseIn(_startPoint, _targetPoint, delta);
                        break;

                    case ETweenMode.CURVE:
                        movingPosition = Curve(_startPoint, _targetPoint, delta);
                        break;

                    default:
                        break;
            }

            return movingPosition;
        }

        private void HandleButtonsManagerAction(ButtonModel _btnModel, ButtonsManager.EButtonActionState _btnStateAction) {
            //Debug.Log("<b>MenuManager</b> HandleButtonsManagerAction of " + _btnModel.ToString() + "(" + _btnStateAction + ")");

            switch (_btnStateAction) {

                case ButtonsManager.EButtonActionState.RETRACTING:
                    FoldUpMenu(_btnModel, buttonsManagerRef.CurrentSubButtons);
                    break;

                case ButtonsManager.EButtonActionState.EXPANDING:
                    UnfoldMenu(_btnModel, buttonsManagerRef.CurrentSubButtons);
                    break;

                default:
                    break;
            }

        }

        private void UnfoldMenu(ButtonModel _button, ButtonModel[] _subButtons) {
            //Debug.Log("<b>MenuManager</b> UnfoldMenu from " + _button.ToString());

            int len = _subButtons.Length;
            if (len == 0)
                return;

            float   baseAngle  = _button.BaseAngle;
            Vector3 startPoint = _button.EndPoint;
            float   angularPos = 0.0f;

            for (int i = 0; i < len; i++) {
                angularPos = GetAngleByIndex(i, len, baseAngle);
                _subButtons[i].State = ButtonModel.EState.DEPLOYING;
                CreateButtonObject(_subButtons[i], startPoint, angularPos);
            }
        }

        private void FoldUpMenu(ButtonModel _button, ButtonModel[] _subButtons) {
            //Debug.Log("<b>MenuManager</b> FoldUpMenu to " + _button.ToString());

            int len = _subButtons.Length;
            for (int i = 0; i < len; i++) {

                if(_subButtons[i].State != ButtonModel.EState.IN_PLACE) {
                    continue;
                }

                _subButtons[i].State = ButtonModel.EState.RETRACTING;
                MenuEventManager.TriggerEvent(_subButtons[i], ButtonsManager.EButtonActionState.ANIMATION_STARTED);
            }
        }

        private void CreateButtonObject(ButtonModel _button, Vector3 _startPoint, float _angularPos, string _idToIgnore = "&") {
            //Debug.Log("<b>MenuManager</b> CreateSubButtons from " + _node.Id);

            if (_button.Id.Equals(_idToIgnore))
                return;

            _button.BaseAngle = _angularPos;

            GameObject newBtn = Instantiate(buttonPrefab, Vector3.zero, Quaternion.identity) as GameObject;

            newBtn.gameObject.name = "btn " + _button.Id;
            newBtn.gameObject.transform.SetParent(transform);
            newBtn.GetComponent<RectTransform>().anchoredPosition = _startPoint;

            PieButton btnComp = newBtn.GetComponent<PieButton>();
            btnComp.Init(_button, _angularPos, true);

        }

        private float GetAngleByIndex(int _index, int _btnNumber, float _baseAngle) {
            //Debug.Log("<b>MenuManager</b> GetAngleByIndex for " + _btnNumber + " buttons");

            float baseInc = buttonsManagerRef.CurrentLevel > 0 ? 30.0f : 45.0f;

            // Angle value is 45° for up to 7 buttons, then it is divided by 2 for each full range of 8 buttons
            float angularInc = baseInc / (1 + _btnNumber / 8);
            // 1st index is in line with the parent, then right, then left and so on... 
            int sign = _index % 2 == 0 ? -1 : 1;
            int inc = Mathf.CeilToInt(_index * 0.5f) * sign;

            return inc * angularInc + _baseAngle;
        }

        Vector3 Linear(Vector3 _startPoint, Vector3 _targetPoint, float delta) {
            return Vector3.Lerp(_startPoint, _targetPoint, delta);
        }

        Vector3 EaseIn(Vector3 _startPoint, Vector3 _targetPoint, float delta) {
            return Vector3.Lerp(_startPoint, _targetPoint, delta * delta);
        }

        Vector3 Curve(Vector3 _startPoint, Vector3 _targetPoint, float delta) {
            return (_targetPoint - _startPoint) * TweeningCurve.Evaluate(delta) + _startPoint;
        }

          
    }

}
