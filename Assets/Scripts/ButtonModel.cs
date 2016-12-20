using UnityEngine;
using UnityEngine.Events;

namespace CustomPieMenu {

    [System.Serializable]
    public class ButtonModel {

        public enum EState { CREATED = 0, MOVING, IN_PLACE, ACTIVE, WAITING }

        public string name = "";
        public Sprite icon;

        [SerializeField] private string id;
        public UnityEvent ActionOnClick;

        private EState state         = EState.CREATED;
        private EState previousState = EState.CREATED;
        private Vector3 startPoint;
        private Vector3 targetPoint;

        public Vector3 StartPoint {
            get { return startPoint; }
            set { startPoint = value; }
        }

        public Vector3 TargetPoint {
            get { return targetPoint; }
            set { targetPoint = value; }
        }

        public EState State {
            get { return state; }
            set {
                previousState = state;
                state = value;
                }
        }

        public string Id {
            get { return id; }
        }


        public ButtonModel(string _id) {
            id          = _id;

            if (_id.Equals("@")) {
                state = EState.IN_PLACE;
            }

            name        = "New menu level " + GetLevel();
        }

        public void Reset() {
            state           = EState.CREATED;
            previousState   = EState.CREATED;
            startPoint      = Vector3.zero;
            targetPoint     = Vector3.zero;
        }

        public byte GetLevel() {
            return (byte)(id.Length - 1);
        }

        public bool IsChildOf(string _id) {
            return id.Contains(_id) && !id.Equals(_id);
        }

        //public void SetNewStartPoint(Vector3 _startPoint) {
        //    startPoint = _startPoint;
        //}

        public void SetTargetPoint(Vector3 _targetPoint) {
            targetPoint = _targetPoint;
        }


        public override string ToString() {
            return GetType() + " (ID : " + Id + ", Level : " + (Id.Length - 1) + ", parentID : " + Id.Substring(0, Id.Length - 1) + ")";
        }

        public void HandleClickAction() {
            Debug.Log("Click on " + id);
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
