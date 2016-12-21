using UnityEngine;
using UnityEngine.Events;

namespace CustomPieMenu {

    [System.Serializable]
    public class ButtonModel {

        public enum EState { CREATED = 0, INITIALIZED, DEPLOYING, IN_PLACE, UNFOLDING, FOLDING, RETRACTING }

        public delegate void ButtonModelEvent(ButtonModel thisButton);
        public event ButtonModelEvent OnClick;
        public event ButtonModelEvent OnAnimationStarted;
        public event ButtonModelEvent OnAnimationEnded;
        //public UnityEvent ActionOnClick;

        public string name = "New menu level ";
        public Sprite icon;

        [SerializeField] private string id;

        private EState  state         = EState.CREATED;
        private EState  previousState = EState.CREATED;
        private Vector3 originPoint   = Vector3.zero;
        private Vector3 endPoint      = Vector3.zero;
        private float   baseAngle     = 0.0f;

        public string Id {
            get { return id; }
        }

        public EState State {
            get { return state; }
            set {
                previousState = state;
                state = value;
            }
        }

        public Vector3 OriginPoint {
            get { return originPoint; }
            set { originPoint = value; }
        }

        public Vector3 EndPoint {
            get { return endPoint; }
            set { endPoint = value; }
        }

        public float BaseAngle {
            get { return baseAngle; }
            set { baseAngle = value; }
        }

        public bool IsRoot {
            get { return id.Equals("@"); }
        }

        public bool IsMoving {
            get { return state == EState.RETRACTING || state == EState.DEPLOYING; }
        }

        public ButtonModel(string _id, string _name = "") {
            id    = _id;

            if (string.IsNullOrEmpty(_name)) {
                name = "New menu level " + GetLevel();
            }
            else {
                name = _name;
            }
            
            if (IsRoot) {
                state = EState.IN_PLACE;
            }
            else {
                state = EState.INITIALIZED;
            }

            //if (ActionOnClick == null) {
            //    ActionOnClick = new UnityEvent();
            //}
            //ActionOnClick.AddListener(HandleClickAction);
        }

        public void Reset() {
            if (IsRoot) {
                state         = EState.IN_PLACE;
                previousState = EState.INITIALIZED;
            }
            else {
                state         = EState.CREATED;
                previousState = EState.CREATED;
            }

            originPoint     = Vector3.zero;
            endPoint        = Vector3.zero;
        }

        public byte GetLevel() {
            return (byte)(id.Length - 1);
        }

        public bool IsChildOf(string _id) {
            return id.Contains(_id) && !id.Equals(_id);
        }

        public void SetEndPoint(Vector3 _targetPoint) {
            endPoint = _targetPoint;
        }

        public override string ToString() {
            return GetType() + " (ID : " + Id + ", Level : " + (Id.Length - 1) + ", parentID : " + Id.Substring(0, Id.Length - 1) + ", State : " + state + ")";
        }

        public void HandleClickAction() {
            //Debug.Log("Click on " + id);
            OnClick(this);
        }

        public void StartAnimation() {
            //Debug.Log("<b>ButtonModel</b> StartAnimation");

            OnAnimationStarted(this);
        }

        public void EndAnimation() {
            //Debug.Log("<b>ButtonModel</b> EndAnimation");

            OnAnimationEnded(this);
        }

    }
}

/*namespace CustomVector {
    public struct Vector3 {

        public static Vector3 zero = new Vector3(0, 0, 0);
        public static Vector3 one  = new Vector3(1, 1, 1);

        float x;
        float y;
        float z;

        public Vector3(float _x, float _y, float _z) {
            this.x = _x;
            this.y = _y;
            this.z = _z;
        }

        public override string ToString() {
            return (String.Format("({0},{1},{2})", x, y, z));
        }

        public Vector3 Set(float _x, float _y, float _z) {
            this.x = _x;
            this.y = _y;
            this.z = _z;

            return this;
        }

    }
}*/
