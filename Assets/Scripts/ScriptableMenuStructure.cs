using System;
using System.Collections.Generic;
using UnityEngine;
using ContextualMenu;

namespace ContextualMenuData {
    [CreateAssetMenu(fileName = "NewMenuStructure", menuName = "ContextualMenu/NewMenuStructure", order = 1)]
    [Serializable]
    public class ScriptableMenuStructure : ScriptableTreeObject {

        public List<ButtonModel> Buttons = new List<ButtonModel>(1) { new ButtonModel("@") };

        [SerializeField]private List<bool> areDetailsVisible  = new List<bool>(1) { true };

        public List<bool> AreDetailsVisible {
            get { return areDetailsVisible; }
            set { areDetailsVisible = value; }
        }

        public override void Init() {
            base.Init();

            Buttons             = new List<ButtonModel>(1) { new ButtonModel("@") };
            areDetailsVisible   = new List<bool>(1) { true };

            //OnNodeRemoved += HandleNodeRemoved;
        }

        public override void ClearAll() {
            base.ClearAll();

            Buttons.Clear();
            areDetailsVisible.Clear();
        }

        public override int AddNode(SerializableNode _parentNode) {
            //Debug.Log("<b>ScriptableMenuStructure</b> AddNode from " + GetNodeInfo(_parentNode));

            int index     = base.AddNode(_parentNode);
            string nodeId = serializedNodes[index].Id;

            ButtonModel newBtn = new ButtonModel(nodeId);

            Buttons.Insert(index, newBtn);
            areDetailsVisible.Insert(index, false);

            return index;
        }


        public void RemoveButton(ButtonModel _btnModel) {
            //Debug.Log("<b>ScriptableMenuStructure</b> RemoveButton " + _btnModel.ToString());

            SerializableNode node       = GetNodeFromButton(_btnModel);
            SerializableNode parentNode = GetParent(node);

            UpdateLists(RemoveNode(node, true), parentNode);
  
            return;
        }

        public ButtonModel GetButtonfromId(string _id) {
            //Debug.Log("<b>ScriptableMenuStructure</b> GetButtonfromId " + _id);

            for (int i = 0; i < Buttons.Count; i++) {
                if (Buttons[i].Id.Equals(_id)){
                    return Buttons[i];
                }
            }

            return null;
        }

        public ButtonModel GetButtonFromNode(SerializableNode _node) {
            //Debug.Log("<b>ScriptableMenuStructure</b> GetButtonFromNode " + _node.ToString());

            byte   len = (byte)Buttons.Count;
            string id  = _node.Id;

            for (byte i = 0; i < len; i++) {
                if (Buttons[i].Id.Equals(id)) {
                    return Buttons[i];
                }
            }

            return null;
        }

        public SerializableNode GetNodeFromButton(ButtonModel _btnModel) {
            //Debug.Log("<b>ScriptableMenuStructure</b> GetNodeFromButton of " + _btnModel.ToString());
            return serializedNodes[Buttons.IndexOf(_btnModel)];
        }

        public ButtonModel[] GetChildren(ButtonModel _btnModel) {
            //Debug.Log("<b>ScriptableMenuStructure</b> GetChildren of " + _btnModel.ToString());

            SerializableNode   node     = GetNodeFromButton(_btnModel);
            SerializableNode[] subNodes = GetChildren(node);
            
            int len = subNodes.Length;
            //Debug.Log(node.ToString() + " : has " + len + " children");
            ButtonModel[] subButtons = new ButtonModel[len];

            int index;

            for (int i = 0; i < len; i++) {
                //Debug.Log("i = " + i + ", subNodes[i] = " + subNodes[i].ToString());
                index = subNodes[i].IndexOfFirstChild - 1;
                //Debug.Log("subButtons[i] : Buttons " + index);
                subButtons[i] = Buttons[index];
            }

            return subButtons;
        }

        protected override void UpdateLists(List<int> _indexesToRemove, SerializableNode _parentNode = null) {
            //Debug.Log("<b>ScriptableMenuStructure</b> UpdateLists ");

            base.UpdateLists(_indexesToRemove, _parentNode);

            List<ButtonModel> updatedBtnList  = new List<ButtonModel>();
            List<bool>        updatedBoolList = new List<bool>();

            int lastIndex = _indexesToRemove[_indexesToRemove.Count - 1];
            int len       = Buttons.Count;

            for (int i = 0; i < len; i++) {
                if (!_indexesToRemove.Contains(i)) {
                    updatedBtnList.Add(Buttons[i]);
                    updatedBoolList.Add(areDetailsVisible[i]);
                }
            }

            Buttons.Clear();
            Buttons = updatedBtnList;

            areDetailsVisible.Clear();
            areDetailsVisible = updatedBoolList;

            len = Buttons.Count;
            for (int i = 0; i <len; i++) {
                Buttons[i].Id = serializedNodes[i].Id;
            }
        }

    }
}
