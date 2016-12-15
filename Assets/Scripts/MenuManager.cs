using UnityEngine;
using UnityEngine.UI;

namespace CustomPieMenu {

    public class MenuManager : MonoBehaviour {

        private static MenuManager instance;

        enum ETweenMode { LINEAR, EASE_IN, CURVE }

        [SerializeField] SO_MenuStructure   menu;
        [SerializeField] GameObject         buttonPrefab = null;
        [SerializeField] ETweenMode         TweeningMode;
        [SerializeField] [Range(1.0f, 5.0f)]  float spacing     = 1.5f;
        [SerializeField] [Range(1.0f, 10.0f)] float tweenSpeed  = 3.5f;
        [SerializeField] AnimationCurve TweeningCurve = AnimationCurve.Linear(0.0f, 0.0f, 1.0f,1.0f);

        public static MenuManager Instance {
            get {
                 return instance;
            }
        }

        public float Spacing {
            get {
                return spacing;
            }
        }

        public float TweenSpeed {
            get {
                return tweenSpeed;
            }
        }

        // Use this for initialization
        void Awake () {

            if (instance == null) {
                instance = this;
            }

            if(menu == null) {
                Debug.LogError("No context for menu");
                return;
            }

            GameObject newBtnGo = Instantiate(buttonPrefab, Vector3.zero, Quaternion.identity, transform) as GameObject;
            PieButton  newBtn   = newBtnGo.GetComponent<PieButton>();

            if (newBtn != null) {
                newBtn.OnClicked += HandleClick;
                newBtn.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;
                newBtn.Init(_node : menu.Root, _angularPos : 0, _parentAngle: 0, _isLinked : false);
                newBtn.BtnModel.State = ButtonModel.EState.FOLDED;
                //newBtn.StartAnimation();
            }

        }
	
        public ButtonModel GetBtnModel(Node _node) {
            Debug.Log("<b>ButtonManager</b> GetBtnModel from " + _node.ToString());

            return menu.GetButtonFromNode(_node);
        }

        public void CreateNewMenu() {
            Debug.Log("<b>ButtonManager</b> CreateNewMenu");

            SO_MenuStructure.CreateInstance("SO_MenuStructure");
        }

        public void UnsuscribeButtonEvent(PieButton _button) {
            _button.OnClicked -= HandleClick;
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

        Vector3 Linear(Vector3 _startPoint, Vector3 _targetPoint, float delta) {
            return Vector3.Lerp(_startPoint, _targetPoint, delta);
        }

        Vector3 EaseIn(Vector3 _startPoint, Vector3 _targetPoint, float delta) {
            return Vector3.Lerp(_startPoint, _targetPoint, delta * delta);
        }

        Vector3 Curve(Vector3 _startPoint, Vector3 _targetPoint, float delta) {
            return (_targetPoint - _startPoint) * TweeningCurve.Evaluate(delta) + _startPoint;
        }

        private bool HandleClick(PieButton _button) {
            //Debug.Log("<b>ButtonManager</b> HandleClick " + _button.gameobject.name);

            if (_button == null)
                return false;

            string nodeId = _button.BtnModel.Id;
            Node   node   = menu.Root.GetNode(nodeId);

            if (_button.BtnModel.State != ButtonModel.EState.UNFOLDED) 
            {
                uint  nbSubBtn  = (uint)node.SubNodes.Count;
                float baseAngle = _button.BaseAngle;
                Vector3 startPoint = _button.BtnModel.TargetPoint;

                for (byte i = 0; i < nbSubBtn; i++) {
                    float angularPos = GetAngleByIndex(i, nbSubBtn, baseAngle);
                    CreateButton(node.SubNodes[i], startPoint, angularPos, baseAngle);
                }
            }

            return true;
        }

        private void CreateButton(Node _node, Vector3 startPoint, float _angularPos, float _parentAngle) {
            //Debug.Log("<b>PieButton</b> CreateButton " + _index);

            GameObject newBtn = Instantiate(buttonPrefab, Vector3.zero, Quaternion.identity) as GameObject;

            newBtn.gameObject.name = "btn " + _node.Id;
            newBtn.gameObject.transform.SetParent(transform);

            newBtn.GetComponent<RectTransform>().anchoredPosition = startPoint;

            PieButton btnComp = newBtn.GetComponent<PieButton>();
            btnComp.Init(_node, _angularPos, _parentAngle, true);
            btnComp.OnClicked += HandleClick;

            btnComp.StartAnimation();
        }

        private float GetAngleByIndex(uint _index, uint _btnNumber, float _baseAngle) {
            //Debug.Log("<b>PieButton</b> GetAngleByIndex for " + _btnNumber + " buttons");

            // Angle value is 45° for up to 7 buttons, then it is divided by 2 for each full range of 8 buttons
            float angularInc = 45.0f / (1 + _btnNumber / 8); 
            // 1st index is in line with the parent, then right, then left and so on... 
            int sign = _index % 2 == 0 ? -1 : 1;     
            int inc  = Mathf.CeilToInt(_index * 0.5f) * sign;

            return inc * angularInc + _baseAngle;
        }

        private void OnDisable() {
            //Debug.Log("<b>PieButton</b> OnDisable");
            menu.ResetAllButtons();
        }

    }
}
