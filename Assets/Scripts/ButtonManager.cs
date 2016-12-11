using UnityEngine;
using UnityEngine.UI;

namespace CustomPieMenu {

    public class ButtonManager : MonoBehaviour {

        private static ButtonManager instance;

        enum ETweenMode { LINEAR, EASE_IN, CURVE }

        [SerializeField] GameObject buttonPrefab = null;
        [SerializeField] ETweenMode TweeningMode;
        [SerializeField] [Range(1.0f, 5.0f)]  float spacing     = 1.5f;
        [SerializeField] [Range(1.0f, 10.0f)] float tweenSpeed  = 3.5f;
        [SerializeField] AnimationCurve TweeningCurve = AnimationCurve.Linear(0.0f, 0.0f, 1.0f,1.0f);

        public static ButtonManager Instance {
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

            GameObject newBtnGo = Instantiate(buttonPrefab, Vector3.zero, Quaternion.identity, transform) as GameObject;
            PieButton  newBtn   = newBtnGo.GetComponent<PieButton>();

            if (newBtn != null) {
                newBtn.OnClicked += HandleClick;
                newBtn.Init(Vector2.zero);
                newBtn.StartAnimation();
            }

        }
	
	    // Update is called once per frame
	    void Update () {
		
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

            //if (_button.BtnModel.State != ButtonModel.EState.UNFOLDED) 
            {
                uint nbSubBtn = (uint)Random.Range(1, 6);
                float baseAngle = _button.BaseAngle;
                Vector3 startPoint = _button.BtnModel.TargetPoint;

                for (uint i = 0; i < nbSubBtn; i++) {
                    float angularPos = GetAngleByIndex(i, nbSubBtn, baseAngle);
                    CreateButton(i, startPoint, angularPos, baseAngle);
                }
            }

            return true;
        }

        private void CreateButton(uint _index, Vector3 startPoint, float _angularPos, float _parentAngle) {
            //Debug.Log("<b>PieButton</b> CreateButton " + _index);

            GameObject newBtn = Instantiate(buttonPrefab, Vector3.zero, Quaternion.identity) as GameObject;

            newBtn.gameObject.name = "btn " + _index;
            newBtn.gameObject.transform.SetParent(transform);

            newBtn.GetComponent<RectTransform>().anchoredPosition = startPoint;

            PieButton btnComp = newBtn.GetComponent<PieButton>();
            btnComp.Init(_index, _angularPos, _parentAngle, true);
            btnComp.OnClicked += HandleClick;

            btnComp.StartAnimation();
        }

        private float GetAngleByIndex(uint _index, uint _btnNumber, float _baseAngle) {

            float angularInc = 360.0f / _btnNumber;

            int sign = _index % 2 == 0 ? -1 : 1;
            int inc = Mathf.CeilToInt(_index * 0.5f) * sign;

            return inc * angularInc * 0.8f + _baseAngle;
        }

    }
}
