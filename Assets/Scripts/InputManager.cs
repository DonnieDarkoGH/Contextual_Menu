using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


namespace ContextualMenu {


    public class InputManager : MonoBehaviour {

        private Vector2 InputPosition;
        //private bool    isLockedOnPosition = false;
        private float   timerStart;
        private float   distance;

        private Vector3 HitCamLoc;
        private Vector3 TargetCamLoc;

        // Use this for initialization
        public void Init() {

        }

        // Update is called once per frame
        void Update() {

            if (Input.GetKeyUp(KeyCode.Mouse1)) {
                InputPosition = Input.mousePosition;
                IsContextObjectSelected();
            }

            //HitLoc = GetCameraTargetPoint();
        }

        private bool IsContextObjectSelected() {

            Ray inputRay = Camera.main.ScreenPointToRay(InputPosition);
            RaycastHit hit;

            if (Physics.Raycast(inputRay, out hit, Mathf.Infinity)) {
                ISelectable IObj = hit.collider.gameObject.GetComponent<ISelectable>();
                if(IObj != null) {
                    CenterObjectInScreen(hit.collider.transform, IObj);
                }
            }

            return false;
        }


        private void CenterObjectInScreen(Transform _objToCenter, ISelectable _selectableObj) {

            MenuManager.Instance.KillMenu();

            HitCamLoc    = GetProjectionOnGround(Camera.main.transform, Camera.main.transform.forward);
            TargetCamLoc = GetProjectionOnGround(_objToCenter, -Vector3.up);
            Vector3 Offset = TargetCamLoc - HitCamLoc;
            Vector3 TargetCamPos = Camera.main.transform.position + Offset;

            timerStart = Time.time;

            StartCoroutine(MoveObject(Camera.main.transform, TargetCamPos, _selectableObj));

        }

        private Vector3 GetProjectionOnGround(Transform _transformToProject, Vector3 _projDir) {// Transform _objTransform) {

            Transform camTr       = Camera.main.transform;
            Ray       camRay      = new Ray(_transformToProject.position, _projDir);
            Plane     groundPlane = new Plane(Vector3.up, Vector3.zero);
            float     rayDistance;

            if(groundPlane.Raycast(camRay,out rayDistance)) {
                return camRay.GetPoint(rayDistance);
            }

            return Vector3.zero;
        }

        IEnumerator MoveObject(Transform _objTr, Vector3 _targetPosition, ISelectable _selectableObj) {

            Vector3 startPosition = _objTr.position;
            float frac = 0.0f;

            do {
                frac = (Time.time - timerStart) * (Time.time - timerStart) * 10.0f;
                _objTr.position = Vector3.Lerp(startPosition, _targetPosition, frac);
                yield return 0;
            } while (frac <= 1);

            _objTr.position = _targetPosition;
            _selectableObj.HandleSelection(_targetPosition);
        }

        private void OnDrawGizmos() {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(HitCamLoc, 0.25f);
            Gizmos.DrawSphere(TargetCamLoc, 0.25f);
        }

    }
}

