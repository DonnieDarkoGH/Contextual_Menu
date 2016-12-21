using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;

namespace CustomPieMenu {

    public class PieButton : MonoBehaviour {

        public delegate void PieButtonEvent(PieButton thisButton);
        public event PieButtonEvent OnButtonInPlace;

        [SerializeField] private ButtonModel   btnModel;
        [SerializeField] private RectTransform linkRef = null;
        [SerializeField] private Image         icon = null;

        private Vector3 movingPosition  = Vector3.zero;
        private Vector3 origin          = Vector3.zero;
        private Vector3 startPoint      = Vector3.zero;
        private Vector3 targetPoint     = Vector3.zero;

        private RectTransform rectTransform;

        private float width     = 0;
        private float height    = 0;
        private float baseAngle = 0.0f;
        private float startTime = 0;
        private float distance  = 0.0f;

        public float BaseAngle {
            get { return baseAngle; }
        }

        public ButtonModel BtnModel {
            get { return btnModel; }
        }

        public Vector3 StartPoint {
            get {
                return startPoint;
            }

            set {
                startPoint = value;
            }
        }

        public Vector3 TargetPoint {
            get {
                return targetPoint;
            }

            set {
                targetPoint = value;
            }
        }

        // Use this for initialization
        public void Init(ButtonModel _btnModel, float _angularPos, bool _isLinked = false) {
            //Debug.Log("<b>PieButton</b> Init from model : " + _btnModel.ToString());

            btnModel = _btnModel;
            btnModel.OnAnimationStarted += StartAnimation;

            if (rectTransform == null) {
                rectTransform = GetComponent<RectTransform>();
            }
            rectTransform.localScale = Vector3.one;

            if (icon == null) {
                icon = GetComponentsInChildren<Image>()[2];
            }
            icon.overrideSprite = btnModel.icon;

            width     = rectTransform.rect.width;
            height    = rectTransform.rect.height;
            baseAngle = _angularPos;

            btnModel.OriginPoint = rectTransform.anchoredPosition;

            CalculateTrajectory();
            
            if (_isLinked) {
                //CreateLink(_angularPos);
            }

            startTime = Time.time;

        }

        IEnumerator MoveButton(Vector3 _start, Vector3 _end) {

            while (TimeRatio() < 1) 
            {
                movingPosition = MenuManager.Instance.TweenPosition(_start, _end, TimeRatio());
                rectTransform.anchoredPosition = movingPosition;

                yield return null;
            }

            movingPosition                 = _end;
            rectTransform.anchoredPosition = movingPosition;

            Vector3 tmp = startPoint;
            startPoint  = targetPoint;
            targetPoint = tmp;

            OnButtonInPlace(this);

        }

        public void HandleClick() {
            //Debug.Log("<b>PieButton</b> HandleClick");

            if (btnModel.State != ButtonModel.EState.IN_PLACE)
                return;

            btnModel.HandleClickAction();

            icon.gameObject.transform.SetAsLastSibling();
            gameObject.transform.SetAsLastSibling();
        }

        private void CalculateTrajectory() {
            //Debug.Log("<b>PieButton</b> CalculateTrajectory from " + btnModel.ToString());

            if(btnModel == null) {
                throw new System.Exception("Button model must be set in PieButton to calculate the trajectory");
            }

            startPoint = btnModel.OriginPoint;

            if (btnModel.State == ButtonModel.EState.IN_PLACE) {
                targetPoint = btnModel.OriginPoint;
            }
            else {
                float spacing   = MenuManager.Instance.Spacing;
                targetPoint     = btnModel.OriginPoint + new Vector3(height * spacing * Mathf.Sin(baseAngle * Mathf.Deg2Rad),
                                                                    width * spacing * Mathf.Cos(baseAngle * Mathf.Deg2Rad),
                                                                    0);
            }

            btnModel.SetEndPoint(targetPoint);

            startPoint =  btnModel.OriginPoint;
            targetPoint = btnModel.EndPoint;
        }

        //public bool StartAnimation(bool _shouldBeDestroyed = false) {
        public void StartAnimation(ButtonModel _btnModel) {
            //Debug.Log("<b>PieButton</b> StartAnimation for " + _btnModel.ToString());

            startTime = Time.time;

            StartCoroutine(MoveButton(startPoint, targetPoint));
        }

        private void CreateLink(float _rotation) {
            //Debug.Log("<b>PieButton</b> CreateLink");

            linkRef.localRotation = Quaternion.Euler(0, 0, 180 - _rotation);

            float x = height * 0.5f * Mathf.Sin(_rotation * Mathf.Deg2Rad);
            float y = width * 0.5f * Mathf.Cos(_rotation * Mathf.Deg2Rad);

            origin = btnModel.OriginPoint + new Vector3(x, y, 0);

        }

        private void UpdateLink() {
            //Debug.Log("<b>PieButton</b> UpdateLink");

            float distance = Vector3.Distance(origin, movingPosition);

            linkRef.sizeDelta = new Vector2(2, distance);
        }

        private float TimeRatio() {
            return (Time.time - startTime) * MenuManager.Instance.TweenSpeed ;// * (1.0f - 0.1f * btnModel.index);
        }

        //private void Start() {
        //    if (btnModel.ButtonAction == null) {
        //        btnModel.ButtonAction = new UnityEngine.Events.UnityEvent();
        //    }

        //    btnModel.ButtonAction.AddListener(btnModel.HandleClickAction);
        //}

        private void OnDisable() {

            btnModel.OnAnimationStarted -= StartAnimation;
        }

    }
}
