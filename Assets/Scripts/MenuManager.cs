using UnityEngine;
using System.Collections.Generic;

namespace CustomPieMenu {

    public class MenuManager : MonoBehaviour {

        enum ETweenMode { LINEAR, EASE_IN, CURVE }
        private static MenuManager instance;

        //[SerializeField] SO_MenuStructure   menu;
        [SerializeField] ButtonsManager     buttonsManagerRef = null;
        [SerializeField] GameObject         buttonPrefab      = null;
        [SerializeField] ETweenMode         TweeningMode;
        [SerializeField] [Range(1.0f, 5.0f)]  float spacing     = 1.5f;
        [SerializeField] [Range(1.0f, 10.0f)] float tweenSpeed  = 3.5f;
        [SerializeField] AnimationCurve TweeningCurve = AnimationCurve.Linear(0.0f, 0.0f, 1.0f,1.0f);

        public static MenuManager Instance {
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

            if(buttonsManagerRef == null) {
                buttonsManagerRef = GetComponent<ButtonsManager>();
            }
            buttonsManagerRef.OnButtonsUnfolding += UnfoldMenu;
            buttonsManagerRef.OnButtonsFoldingUp += FoldUpMenu;


            //if(menu == null) {
            //    Debug.LogError("No context for menu");
            //    return;
            //}

            GameObject newBtnGo = Instantiate(buttonPrefab, Vector3.zero, Quaternion.identity, transform) as GameObject;
            PieButton  newBtn   = newBtnGo.GetComponent<PieButton>();

            if (newBtn != null) {
                //newBtn.OnClicked += HandleClick;
                newBtn.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;
                //newBtn.Init(_node : menu.Root, _angularPos : 0, _parentAngle: 0, _isLinked : false);
                newBtn.Init(_btnModel: buttonsManagerRef.GetRootButton(), _angularPos: 0, _isLinked: false);
            }

        }

        public void CreateNewMenu() {
            Debug.Log("<b>ButtonManager</b> CreateNewMenu");

            SO_MenuStructure.CreateInstance("SO_MenuStructure");
        }

        //public void UnsuscribeButtonEvent(PieButton _button) {
        //    _button.OnClicked       -= HandleClick;
        //    _button.OnButtonInPlace -= HandleEndOfAnimation;
        //}

        

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

        private void UnfoldMenu(ButtonModel _button, ButtonModel[] _subButtons) {
            //Debug.Log("<b>MenuManager</b> UnfoldMenu from " + _button.ToString());

            int len = _subButtons.Length;
            if (len == 0)
                return;

            float   baseAngle  = _button.BaseAngle;
            Vector3 startPoint = _button.EndPoint;
            float   angularPos = 0.0f;

            for (int i = 0; i < len; i++) {
                angularPos = GetAngleByIndex(i, len, baseAngle);
                _subButtons[i].State = ButtonModel.EState.DEPLOYING;
                CreateButtonObject(_subButtons[i], startPoint, angularPos);
            }
        }

        private void FoldUpMenu(ButtonModel _button, ButtonModel[] _subButtons) {
            //Debug.Log("<b>MenuManager</b> FoldUpMenu to " + _button.ToString());

            int len = _subButtons.Length;
            for (int i = 0; i < len; i++) {

                if(_subButtons[i].State != ButtonModel.EState.IN_PLACE) {
                    continue;
                }

                _subButtons[i].State = ButtonModel.EState.RETRACTING;
                _subButtons[i].StartAnimation();
            }
        }

        private void CreateButtonObject(ButtonModel _button, Vector3 _startPoint, float _angularPos, string _idToIgnore = "&") {
            //Debug.Log("<b>MenuManager</b> CreateSubButtons from " + _node.Id);

            if (_button.Id.Equals(_idToIgnore))
                return;

            _button.BaseAngle = _angularPos;

            GameObject newBtn = Instantiate(buttonPrefab, Vector3.zero, Quaternion.identity) as GameObject;

            newBtn.gameObject.name = "btn " + _button.Id;
            newBtn.gameObject.transform.SetParent(transform);
            newBtn.GetComponent<RectTransform>().anchoredPosition = _startPoint;

            PieButton btnComp = newBtn.GetComponent<PieButton>();
            btnComp.Init(_button, _angularPos, true);
            //btnComp.OnClicked += HandleClick;
            btnComp.OnButtonInPlace += HandleEndOfAnimation;

            btnComp.StartAnimation(_button);
        }

        private void HandleEndOfAnimation(PieButton _button) {
            //Debug.Log("<b>MenuManager</b> HandleEndOfAnimation for " + _button.gameObject.name);

            ButtonModel btnModel = _button.BtnModel;

            switch (btnModel.State) {

                case ButtonModel.EState.RETRACTING :
                    _button.OnButtonInPlace -= HandleEndOfAnimation;
                    Destroy(_button.gameObject);
                    break;

                default:
                    break;
            }

            buttonsManagerRef.HandleEndOfAnimation(btnModel);

        }


        private void OnDisable() {
            //Debug.Log("<b>MenuManager</b> OnDisable");
            //menu.ResetAllButtons();

            if (buttonsManagerRef == null) {
                buttonsManagerRef.OnButtonsUnfolding -= UnfoldMenu;
                buttonsManagerRef.OnButtonsFoldingUp -= FoldUpMenu;
            }
        }

        private float GetAngleByIndex(int _index, int _btnNumber, float _baseAngle) {
            //Debug.Log("<b>MenuManager</b> GetAngleByIndex for " + _btnNumber + " buttons");

            // Angle value is 45° for up to 7 buttons, then it is divided by 2 for each full range of 8 buttons
            float angularInc = 45.0f / (1 + _btnNumber / 8);
            // 1st index is in line with the parent, then right, then left and so on... 
            int sign = _index % 2 == 0 ? -1 : 1;
            int inc = Mathf.CeilToInt(_index * 0.5f) * sign;

            return inc * angularInc + _baseAngle;
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


        //private PieButton GetPieButtonObjectFromId(string _btnModelId) {
        //    //Debug.Log("<b>ButtonManager</b> GetPieButtonObjectFromId from " + _btnModelId);

        //    PieButton[] childrenPB = GetComponentsInChildren<PieButton>();
        //    int len = childrenPB.Length;

        //    for (uint i = 0; i < len; i++) {
        //        if (childrenPB[i].BtnModel.Id.Equals(_btnModelId)) {
        //            return childrenPB[i];
        //        }
        //    }

        //    return null;
        //}

        //private ButtonModel[] GetSubButtons(ButtonModel _btnModel) {
        //    //Debug.Log("<b>ButtonManager</b> GetSubButtons from " + _btnModel.ToString());

        //    ButtonModel[] subButtons = new ButtonModel[0];

        //    if (_btnModel != null) {
        //        Node node  = menu.Root.GetNode(_btnModel.Id);
        //        if(node != null) {
        //            subButtons = menu.GetSubMenuFromNode(node);
        //        }
        //    }

        //    return subButtons;
        //}

        /*private bool HandleClick(PieButton _button) {
            //Debug.Log("<b>ButtonManager</b> HandleClick " + _button.gameObject.name);

            if (_button == null)
                return false;

            ButtonModel btnModel = _button.BtnModel;
            string      nodeId   = btnModel.Id;
            Node        node     = menu.Root.GetNode(nodeId);

            if(node == null) {
                return false;
            }

            string  parentId = node.GetParentId();
            uint    nbSubBtn = (uint)node.SubNodes.Count;
            //Debug.Log(btnModel.ToString() + " : " + btnModel.State + "with " + nbSubBtn + " children");

            if (nbSubBtn > 0) {
                if (btnModel.State == ButtonModel.EState.IN_PLACE || btnModel.State == ButtonModel.EState.WAITING) {

                    btnModel.State = ButtonModel.EState.ACTIVE;

                    CreateSubButtons(_button, node);
                    
                    RetractAll(parentId, nodeId);

                }
                else {
                    btnModel.State = ButtonModel.EState.WAITING;
                    RetractAll(nodeId);

                    if(nodeId != "@") {
                        Node      parentNode = menu.Root.GetNode(parentId);
                        PieButton parentBtn  = GetPieButtonObjectFromId(parentId);
                        CreateSubButtons(parentBtn, parentNode, nodeId);
                    }
                }
            }

            return true;
        }

        private bool HandleEndOfAnimation(PieButton _button) {
            //Debug.Log("<b>ButtonManager</b> HandleEndOfAnimation for " + _button.gameObject.name);

            if (_button.ShouldBeDestroyed) {

                if(_button.BtnModel.GetLevel() > currentLevel && currentLevel > targetLevel) {
                    RetractAll();
                }
                else if(currentLevel == targetLevel) {
                    targetLevel     = -1;
                    retractToNodeId = "&";
                    exceptNodeId    = "&";
                }

                _button.BtnModel.Reset();
                Destroy(_button.gameObject);

            } 
            else {
                currentLevel = (uint)_button.BtnModel.GetLevel();
            }

            return true;
        }

        private void CreateSubButtons(PieButton _button, Node _node, string _idToIgnore ="&") {
            //Debug.Log("<b>ButtonManager</b> CreateSubButtons from " + _node.Id);

            uint    nbSubBtn    = (uint)_node.SubNodes.Count;
            float   baseAngle   = _button.BaseAngle;
            Vector3 startPoint  = _button.BtnModel.EndPoint;

            GameObject newBtn;
            PieButton  btnComp;

            for (byte i = 0; i < nbSubBtn; i++) {

                if (_node.SubNodes[i].Id.Equals(_idToIgnore))
                    continue;

                float angularPos = GetAngleByIndex(i, nbSubBtn, baseAngle);

                newBtn = Instantiate(buttonPrefab, Vector3.zero, Quaternion.identity) as GameObject;

                newBtn.gameObject.name = "btn " + _node.SubNodes[i].Id;
                newBtn.gameObject.transform.SetParent(transform);

                newBtn.GetComponent<RectTransform>().anchoredPosition = startPoint;

                btnComp = newBtn.GetComponent<PieButton>();
                btnComp.Init(_node.SubNodes[i], angularPos, baseAngle, true);
                btnComp.OnClicked       += HandleClick;
                btnComp.OnButtonInPlace += HandleEndOfAnimation;

                btnComp.StartAnimation();
            }

        }

        //private void RetractAll() {
        //    //Debug.Log("<b>ButtonManager</b> RetractAll");
        //    RetractAll(retractToNodeId, exceptNodeId, targetLevel);
        //}

        private void RetractAll(string _id, string _exceptId = "&", int _upToLevel = -1) {
            //Debug.Log("<b>ButtonManager</b> RetractAll from " + _id);

            PieButton[] pBtn = GetComponentsInChildren<PieButton>();
            if (pBtn == null) {
                return;
            }

            if(_upToLevel < 0) {
                _upToLevel  = _id.Length - 1;
                targetLevel = _upToLevel;
            }

            retractToNodeId = _id;
            exceptNodeId    = _exceptId;

            int len = pBtn.Length;
            ButtonModel btnModel;

            //Debug.Log("Level : " + currentLevel);
            for (int i = 0; i < len; i++) {
                btnModel = pBtn[i].BtnModel;

                if (!btnModel.Id.Contains(_exceptId) && btnModel.IsChildOf(_id) && btnModel.GetLevel() == (byte)currentLevel)
                {
                    //Debug.Log(pBtn[i].BtnModel.ToString());
                    pBtn[i].StartAnimation(true);
                }
            }

            if(currentLevel > 0)
                currentLevel--;
        }

        private void CreateButton(Node _node, Vector3 startPoint, float _angularPos, float _parentAngle) {
            //Debug.Log("<b>PieButton</b> CreateButton related to " + _node.ToString() + " at " + startPoint);

            GameObject newBtn = Instantiate(buttonPrefab, Vector3.zero, Quaternion.identity) as GameObject;

            newBtn.gameObject.name = "btn " + _node.Id;
            newBtn.gameObject.transform.SetParent(transform);

            newBtn.GetComponent<RectTransform>().anchoredPosition = startPoint;

            PieButton btnComp = newBtn.GetComponent<PieButton>();
            btnComp.Init(_node, _angularPos, _parentAngle, true);
            btnComp.OnClicked       += HandleClick;
            btnComp.OnButtonInPlace += HandleEndOfAnimation;

            btnComp.StartAnimation();
        }



        }*/

    }
}
