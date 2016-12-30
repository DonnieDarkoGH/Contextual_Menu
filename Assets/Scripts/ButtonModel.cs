using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace ContextualMenu {

    [System.Serializable]
    public class ButtonModelEvent: UnityEvent<ButtonModel> {

    }


    [System.Serializable]
    public class ButtonModel {

        public enum EState { CREATED = 0, INITIALIZED, DEPLOYING, IN_PLACE, UNFOLDING, FOLDING, RETRACTING }

        public string name = "New menu level ";
        public Sprite Icon;

        [SerializeField] private string id;

        private EState  state         = EState.CREATED;
        private Vector3 originPoint   = Vector3.zero;
        private Vector3 endPoint      = Vector3.zero;
        private float   baseAngle     = 0.0f;

        public string Id {
            get { return id; }
            set { id = value; }
        }

        public EState State {
            get { return state; }
            set {
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
           
        }

        public void Reset() {
            if (IsRoot) {
                state         = EState.IN_PLACE;
            }
            else {
                state         = EState.CREATED;
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

    }
}
