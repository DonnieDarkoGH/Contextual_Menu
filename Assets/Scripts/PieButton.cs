using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace CustomPieMenu {

    public class PieButton : MonoBehaviour {

        public delegate bool PieButtonEvent(PieButton thisButton);
        public event PieButtonEvent OnClicked;

        private ButtonModel btnModel;

        ButtonModel.EState state;

        [SerializeField] RectTransform linkRef = null;

        [SerializeField] Image icon = null;

        [SerializeField] Vector3 movingPosition = Vector3.zero;
        [SerializeField] Vector3 origin = Vector3.zero;

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

        // Use this for initialization
        public void Init(uint _index, float _angularPos, float _parentAngle, bool _isLinked = false) {
            //Debug.Log("<b>PieButton</b> Init : " + name + ", to reach " + _endPosition);
            //Debug.Log(transform.position + ", "+  transform.localPosition);

            baseAngle = _angularPos;

            if (rectTransform == null) {
                rectTransform = GetComponent<RectTransform>();
            }
            rectTransform.localScale = Vector3.one;

            if (icon == null) {
                icon = GetComponentsInChildren<Image>()[2];
            }

            width  = rectTransform.rect.width;
            height = rectTransform.rect.height;

            btnModel = new ButtonModel(_index, 0, rectTransform.anchoredPosition, 0);

            CalculateTrajectory();

            if (_isLinked) {
                CreateLink(_angularPos);
            }

            startTime = Time.time;

            //StartAnimation();

            //Type myType = typeof(RectTransform);
            //PropertyInfo[] fieldsInfo = myType.GetProperties();

            //foreach (var pi in fieldsInfo) {
            //    Debug.Log(pi.ToString() + " : " + pi.GetValue(rectTransform, null));

            //}

        }

        IEnumerator MoveButton(Vector3 _start, Vector3 _end) {

            float dt = TimeRatio();

            while (dt < 1) 
            {
                btnModel.State = ButtonModel.EState.MOVING;
                movingPosition = ButtonManager.Instance.TweenPosition(_start, _end, TimeRatio());
                rectTransform.anchoredPosition = movingPosition;

                yield return null;
            }

            movingPosition = btnModel.TargetPoint;
            rectTransform.anchoredPosition = movingPosition;

            btnModel.IsInPlace = true;
        }

        public void Init(Vector2 _position, bool _isInPlace = true) {
            Debug.Log("<b>PieButton</b> Init at " + _position);

            Init(0, 0, 0);

            SetPositionInScreen(_position);

            btnModel.IsInPlace = _isInPlace;

            CalculateTrajectory();
        }

        public bool SetPositionInScreen(Vector2 _position) {
            //Debug.Log("<b>PieButton</b> SetPositionInScreen to " + _position);

            if (rectTransform != null) {
                rectTransform.anchoredPosition = _position;
                btnModel.SetNewStartPoint(_position);
                //startPoint = _position;
                return true;
            }

            return false;
        }


        public void HandleClick() {
            //Debug.Log("<b>PieButton</b> HandleClick");

            OnClicked(this);

            btnModel.IsActive = !btnModel.IsActive;
            icon.gameObject.transform.SetAsLastSibling();
        }

        private void CalculateTrajectory() {
            Debug.Log("<b>PieButton</b> CalculateTrajectory from " + btnModel.StartPoint);

            if(btnModel == null) {
                throw new System.Exception("Button model must be set in PieButton to calculate the trajectory");
            }

            float   spacing     = ButtonManager.Instance.Spacing;
            Vector3 targetPoint = Vector3.zero;
            //startPoint = rectTransform.anchoredPosition;

            if (btnModel.IsInPlace) {
                targetPoint = btnModel.StartPoint;
            }
            else {
                targetPoint = btnModel.StartPoint + new Vector3(height * spacing * Mathf.Sin(baseAngle * Mathf.Deg2Rad),
                                                                width * spacing * Mathf.Cos(baseAngle * Mathf.Deg2Rad),
                                                                0);
            }

            btnModel.SetTargetPoint(targetPoint);
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

        public bool StartAnimation() {
            Debug.Log("<b>PieButton</b> StartAnimation");

            Vector3 start = btnModel.StartPoint;
            Vector3 end   = btnModel.TargetPoint;

            switch (btnModel.State) 
            {
                case ButtonModel.EState.MOVING:
                    return false;

                case ButtonModel.EState.UNFOLDED:
                    start = btnModel.TargetPoint;
                    end   = btnModel.StartPoint;
                    break;

                default:
                    break;
            }

            Debug.Log("from " + start + " to " + end);
            StartCoroutine(MoveButton(start,end));

            return true;
        }
        //private void Update() {

        //    state = btnModel.State;

        //    if (isInPlace)
        //        return;

            //    if(rectTransform != null) {
            //        rectTransform.anchoredPosition = movingPosition;
            //        UpdateLink();
            //    }

        //}

        float TimeRatio() {
            return (Time.time - startTime) * ButtonManager.Instance.TweenSpeed * (1.0f - 0.1f * btnModel.index);
        }

    }
}
