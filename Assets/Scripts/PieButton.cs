using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;

namespace CustomPieMenu {

    public class PieButton : MonoBehaviour {

        public delegate bool PieButtonEvent(PieButton thisButton);
        public event PieButtonEvent OnClicked;
        public event PieButtonEvent OnButtonInPlace;

        [SerializeField] private ButtonModel btnModel;

        [SerializeField] RectTransform linkRef = null;

        [SerializeField] Image   icon = null;

        Vector3 movingPosition = Vector3.zero;
        Vector3 origin         = Vector3.zero;

        Vector3 startPoint      = Vector3.zero;
        Vector3 targetPoint     = Vector3.zero;

        public bool  IsInPlace = false;
        private bool shouldBeDestroyed = false;

        RectTransform rectTransform;

        float width     = 0;
        float height    = 0;
        float baseAngle = 0.0f;
        float startTime = 0;
        float distance  = 0.0f;

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

        public bool ShouldBeDestroyed { get { return shouldBeDestroyed; } }

        // Use this for initialization
        public void Init(Node _node, float _angularPos, float _parentAngle, bool _isLinked = false) {
            //Debug.Log("<b>PieButton</b> Init from node : " + _node.ToString());
            //Debug.Log("<b>PieButton</b> Init : " + name + ", to reach " + _endPosition);
            //Debug.Log(transform.position + ", "+  transform.localPosition);

            btnModel = MenuManager.Instance.GetBtnModel(_node);

            if (_node.Id.Equals("@")) {
                btnModel.State = ButtonModel.EState.IN_PLACE;
            }
            else {
                btnModel.State = ButtonModel.EState.CREATED;
            }

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

            btnModel.StartPoint = rectTransform.anchoredPosition;

            CalculateTrajectory();
            
            if (_isLinked) {
                //CreateLink(_angularPos);
            }

            startTime = Time.time;

        }

        IEnumerator MoveButton(Vector3 _start, Vector3 _end) {

            while (TimeRatio() < 1) 
            {
                btnModel.State = ButtonModel.EState.MOVING;
                movingPosition = MenuManager.Instance.TweenPosition(_start, _end, TimeRatio());
                rectTransform.anchoredPosition = movingPosition;

                yield return null;
            }

            btnModel.State = ButtonModel.EState.IN_PLACE;

            movingPosition                 = _end;
            rectTransform.anchoredPosition = movingPosition;

            Vector3 tmp = startPoint;
            startPoint  = targetPoint;
            targetPoint = tmp;

            OnButtonInPlace(this);
        }

        public void HandleClick() {
            //Debug.Log("<b>PieButton</b> HandleClick");

            OnClicked(this);

            icon.gameObject.transform.SetAsLastSibling();
            gameObject.transform.SetAsLastSibling();
        }

        private void CalculateTrajectory() {
            //Debug.Log("<b>PieButton</b> CalculateTrajectory from " + btnModel.StartPoint);

            if(btnModel == null) {
                throw new System.Exception("Button model must be set in PieButton to calculate the trajectory");
            }

            startPoint = btnModel.StartPoint;

            if (btnModel.State == ButtonModel.EState.IN_PLACE) {
                targetPoint = btnModel.StartPoint;
            }
            else {
                float spacing   = MenuManager.Instance.Spacing;
                targetPoint     = btnModel.StartPoint + new Vector3(height * spacing * Mathf.Sin(baseAngle * Mathf.Deg2Rad),
                                                                    width * spacing * Mathf.Cos(baseAngle * Mathf.Deg2Rad),
                                                                    0);
            }

            btnModel.SetTargetPoint(targetPoint);

            startPoint =  btnModel.StartPoint;
            targetPoint = btnModel.TargetPoint;
        }

        public bool StartAnimation(bool _shouldBeDestroyed = false) {
            //Debug.Log("<b>PieButton</b> StartAnimation");

            if (btnModel.State == ButtonModel.EState.MOVING)
                return false;

            shouldBeDestroyed = _shouldBeDestroyed;

            startTime = Time.time;

            StartCoroutine(MoveButton(startPoint, targetPoint));

            return true;
        }

        private void CreateLink(float _rotation) {
            //Debug.Log("<b>PieButton</b> CreateLink");

            linkRef.localRotation = Quaternion.Euler(0, 0, 180 - _rotation);

            float x = height * 0.5f * Mathf.Sin(_rotation * Mathf.Deg2Rad);
            float y = width * 0.5f * Mathf.Cos(_rotation * Mathf.Deg2Rad);

            origin = btnModel.StartPoint + new Vector3(x, y, 0);

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
            if(OnClicked != null) {
                MenuManager.Instance.UnsuscribeButtonEvent(this);
            }
        }

    }
}
