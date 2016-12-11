using System;
using UnityEngine;
using System.Collections.Generic;

public class ButtonModel {

    public enum EState { CREATED = 0, INITIALIZED, FOLDED, MOVING, UNFOLDED }

    public readonly uint index;
    public readonly uint levelInMenu;

    private EState  state         = EState.CREATED;
    private EState  previousState = EState.CREATED;
    private Vector3 startPoint;
    private Vector3 targetPoint;

    private uint parentIndex;
    private uint subButtonNumber = 1;
    private bool isInPlace = false;
    private bool isActive  = false;

    public bool IsInPlace {
        get { return isInPlace; }
        set {
            isInPlace = value;

            if (isInPlace) 
            {
                switch (previousState) 
                {
                    case EState.CREATED:
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
    }

    public Vector3 TargetPoint {
        get { return targetPoint; }
        set { targetPoint = value; }
    }

    public uint SubButtonNumber {
        get { return subButtonNumber; }
        set { subButtonNumber = value; }
    }

    public EState State {
        get { return state; }
        set { state = value; }
    }

    public ButtonModel(uint _index, uint _level, Vector3 _startPoint, uint _parentIndex) {
        index       = _index;
        levelInMenu = _level;
        startPoint  = _startPoint;
        parentIndex = _parentIndex;
    }

    public ButtonModel() : this(0, 0, Vector3.zero, 0) {

    }

    public void SetNewStartPoint(Vector3 _startPoint) {
        startPoint = _startPoint;
    }

    public void SetTargetPoint(Vector3 _targetPoint) {
        targetPoint = _targetPoint;
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
