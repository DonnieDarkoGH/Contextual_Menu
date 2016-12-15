using System;
using UnityEngine;
using System.Collections.Generic;

namespace CustomPieMenu {

    [System.Serializable]
    public class ButtonModel {

        public enum EState { CREATED = 0, INITIALIZED, FOLDED, MOVING, UNFOLDED }

        public string name = "";
        public Sprite icon;

        [SerializeField] private string id;
        private EState state = EState.CREATED;
        private EState previousState = EState.CREATED;
        private Vector3 startPoint;
        private Vector3 targetPoint;

        //[SerializeField] private byte childrenNb;
        private bool isInPlace;
        private bool isActive;

        public bool IsInPlace {
            get { return isInPlace; }
            set {
                isInPlace = value;

                if (isInPlace) {
                    switch (previousState) {
                        case EState.CREATED :
                        case EState.INITIALIZED:
                            state = EState.UNFOLDED;
                            break;

                        case EState.UNFOLDED:
                            state = EState.FOLDED;
                            break;

                        default:
                            break;
                    }
                    previousState = state;
                }
            }
        }

        public bool IsActive {
            get { return isActive; }
            set { isActive = value; }
        }

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
            set { state = value; }
        }

        public string Id {
            get { return id; }
        }



        public ButtonModel(string _id, bool _isStatic) {
            id          = _id;
            name        = "New menu level " + GetLevel();
            isInPlace   = _isStatic;
        }

        public ButtonModel(string _id) : this(_id, false) {
            isInPlace = true;
        }


        public ButtonModel() : this("@") {

        }

        //public ButtonModel(byte _index, byte _level, Vector3 _startPoint, byte _parentIndex) {
        //    index       = _index;
        //    levelInMenu = _level;
        //    startPoint  = _startPoint;
        //    parentIndex = _parentIndex;

        //    name = "New menu level " + _level;
        //}

        public void Reset() {
            state           = EState.CREATED;
            previousState   = EState.CREATED;
            startPoint      = Vector3.zero;
            targetPoint     = Vector3.zero;

            isInPlace = GetLevel() == 0 ? true : false;
            isActive  = false;
        }

        public int GetLevel() {
            return id.Length - 1;
        }

        //public void SetNewStartPoint(Vector3 _startPoint) {
        //    startPoint = _startPoint;
        //}

        public void SetTargetPoint(Vector3 _targetPoint) {
            targetPoint = _targetPoint;
        }

        public override string ToString() {
            return GetType() + " (ID : " + Id + ", Level : " + Id.Length + ", parentID : " + Id.Substring(0, Id.Length - 1) + ")";
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
