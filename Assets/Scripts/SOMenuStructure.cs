using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomPieMenu {

    [CreateAssetMenu(fileName = "NewMenu", menuName = "Tools/ContextualMenu/NewMenu", order = 1)]
    public class SOMenuStructure : ScriptableObject {

        public List<ButtonModel> Buttons = new List<ButtonModel>(1) { new ButtonModel()};
        public Node              Root    = new Node();

        [System.Serializable]
        public class Node {
            public byte ID;
            public byte Level;
            public byte ParentID;

            public List<Node> SubNodes;

            public Node(byte _id, byte _level, byte _parentID) {
                ID       = _id;
                Level    = _level;
                ParentID = _parentID;

                SubNodes = new List<Node>();
            }

            public Node() : this(0, 0, 0) {

            }

            public Node GetNode(byte _level, byte _id) {

                byte len = (byte)SubNodes.Count;
                Node node = null;

                if(_level == Level && _id == ID) {
                    return this;
                }

                for (byte i = 0; i < len; i++) {
                    if(SubNodes[i].Level == _level && SubNodes[i].ID == _id) {
                        node = SubNodes[i];
                        break;
                    }
                    else {
                        node = SubNodes[i].GetNode(_level, _id);
                        if(node != null) {
                            break;
                        }
                    }
                }

                return node;
            }

            public Node AddSubNode() {
                byte nodesCount = (byte)SubNodes.Count;
                byte subLevel   = (byte)(Level + 1);

                Node subNode    = new Node(nodesCount, subLevel, ID);
                Debug.Log(subNode.ToString());

                SubNodes.Add(subNode);

                return subNode;
            }

            public bool RemoveSubNode(Node _subNode) {

                try {
                    for(int i = 0; i < _subNode.SubNodes.Count; i++) {
                        _subNode.RemoveSubNode(_subNode.SubNodes[i]);
                    }
                    SubNodes.Remove(_subNode);
                    ReoderSubNodes();
                }
                catch {
                    return false;
                }

                return true;
            }

            private bool ReoderSubNodes() {

                try {
                    for (byte i = 0; i < SubNodes.Count; i++) {
                        SubNodes[i].ID = i;
                        for (byte j = 0; j < SubNodes[i].SubNodes.Count; j++) {
                            SubNodes[i].SubNodes[j].ParentID = i;
                        }
                     }
                }
                catch {
                    return false;
                }

                return true;
            }

            public override string ToString() {
                return GetType() + " (ID : " + ID + ", Level : " + Level + ", parentID : " + ParentID + ")";
            }

        }

        public ButtonModel GetButtonFromNode(Node _node) {

            byte len    = (byte)Buttons.Count;
            byte level  = _node.Level;
            byte id     = _node.ID;

            for (byte i = 0; i < len; i++) {
                if (Buttons[i].levelInMenu == level && Buttons[i].index == id) {
                    return Buttons[i];
                }
            }

            return null;
        }

        public ButtonModel[] GetSubMenuFromNode(Node _node) {

            byte          len      = (byte)_node.SubNodes.Count;
            ButtonModel[] subMenus = new ButtonModel[len];

            for (byte i = 0; i < len; i++) {
                subMenus[i] = GetButtonFromNode(_node.SubNodes[i]);
            }

            return subMenus;
        }

        public bool AddNewSubMenu(Node _node) {
            //Debug.Log("<b>SOMenuStructure</b> AddNewSubMenu to " + _menu.ToString());

            if (Buttons.Count == 255)
                return false;

            byte    id        = _node.ID;
            byte    level     = _node.Level;
            //Vector3 position  = _menu.TargetPoint;
            Vector3 position = Vector3.zero;

            Node newNode = _node.AddSubNode();
            byte newId   = newNode.ID;

            ButtonModel newBtn = new ButtonModel(newId, (byte)(level+1), position, id);
            //Debug.Log(newBtn.ToString());
            Buttons.Add(newBtn);

            return true;
        }

        public bool RemoveSubMenu(Node _node) {
            //Debug.Log("<b>SOMenuStructure</b> RemoveSubMenu from node " + _node.ToString());

            //Buttons.Remove(menuToRemove);

            byte parentId   = _node.ParentID;
            byte level      = (byte)(_node.Level - 1);
            Node parentNode = Root.GetNode(level, parentId);

            ButtonModel     menuToRemove = GetButtonFromNode(_node);
            ButtonModel[]   menusInLevel = GetSubMenuFromNode(parentNode);

            for (int i = 0; i < menusInLevel.Length; i++) {
                if(menusInLevel[i].index.CompareTo(menuToRemove.index) > 0) {
                    menusInLevel[i].index--;
                }
            }
            Buttons.Remove(menuToRemove);

            //Debug.Log("Get node at level " + level + " from Id :" + parentId);
            //Debug.Log(" --> " + parentNode.ToString());
            parentNode.RemoveSubNode(_node);

            return true;
        }

        public bool ChangeButtonIndex(ButtonModel _button, byte _newIndex) {
            Debug.Log("<b>SOMenuStructure</b> ChangeButtonIndex of " + _button.name + " to " + _newIndex);


            return true;
        }


    }
}
