﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace CustomPieMenu {

    [CreateAssetMenu(fileName = "NewMenu", menuName = "Tools/ContextualMenu/NewMenu", order = 1)]
    public class SO_MenuStructure : ScriptableObject {

        public List<ButtonModel> Buttons = new List<ButtonModel>(1) { new ButtonModel()};
        public Node Root = new Node();

        public ButtonModel GetButtonFromNode(Node _node) {
            //Debug.Log("<b>SOMenuStructure</b> GetButtonFromNode " + _node.ToString());

            byte len    = (byte)Buttons.Count;
            string id   = _node.Id;

            for (byte i = 0; i < len; i++) {
                if (Buttons[i].Id.Equals(id)) {
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
            Debug.Log("<b>SOMenuStructure</b> AddNewSubMenu to " + _node.ToString());

            if (Buttons.Count == 255)
                return false;

            Node        newNode = _node.AddSubNode();
            ButtonModel newBtn  = new ButtonModel(newNode.Id);
            //ButtonModel newBtn = new ButtonModel(newId, (byte)(level+1), position, id);
            //Debug.Log(newBtn.ToString());
            Buttons.Add(newBtn);

            return true;
        }

        public bool RemoveSubMenu(Node _node) {
            //Debug.Log("<b>SOMenuStructure</b> RemoveSubMenu from node " + _node.ToString());

            string nodeId     = _node.Id;
            string parentId   = _node.GetParentId();
            Node   parentNode = Root.GetNode(parentId);

            if (parentNode == null) {
                return false;
            }

            parentNode.RemoveSubNode(_node);

            List<ButtonModel> updatedBtnList = new List<ButtonModel>(0);

            int buttonsCount = Buttons.Count;
            for(int i = 0; i < buttonsCount; i++) {
                if (!Buttons[i].Id.StartsWith(nodeId)) {
                    updatedBtnList.Add(Buttons[i]);
                }
            }

            Buttons.Clear();
            Buttons.AddRange(updatedBtnList);

            return true;
        }

        public void ResetAllButtons() {

            int len = Buttons.Count;
            for(int i = 0; i < len; i++) {
                Buttons[i].Reset();
            }
        }

        public bool ChangeButtonIndex(ButtonModel _button, byte _newIndex) {
            Debug.Log("<b>SOMenuStructure</b> ChangeButtonIndex of " + _button.name + " to " + _newIndex);


            return true;
        }


    }

    [System.Serializable]
    public class Node {

        [SerializeField] private string id;
        [SerializeField] private List<Node> subNodes;

        public byte Level {
            get {
                if (Id.Length == 0) {
                    throw new Exception("Id for this node is not initialized");
                }
                return (byte)(Id.Length - 1);
            }
        }

        public List<Node> SubNodes {
            get { return subNodes; }
        }

        public string Id {
            get { return id;}
        }

        public Node(string _parentID, byte _index) {

            byte start = Convert.ToByte('a');
            char indexToChar = Convert.ToChar(start + _index);

            id       = _parentID + indexToChar;
            subNodes = new List<Node>();
        }

        public Node() : this("", 0) {
            id      = "@";

            subNodes = new List<Node>();
        }

        public Node GetNode(string _id) {

            int len = subNodes.Count;

            if (id.Equals(_id))
                return this;

            for (int i = 0; i < len; i++) {
                if(subNodes[i].GetNode(_id) != null) {
                    return subNodes[i];
                }
            }

            return null;
        }

        public string GetParentId() {

            if (Level == 0) {
                return string.Empty;
            }

            return id.Substring(0, Level);
        }
        
        public bool IsChildOf(Node _node) {
            return id.StartsWith(_node.Id);
        }

        public Node AddSubNode() {

            byte nodesCount = (byte)SubNodes.Count;
            byte newIndex   = nodesCount;

            for (byte i = 0; i < nodesCount; i++) {

                byte byteIndex = IdToByteIndex(SubNodes[i].Id);
                Debug.Log("i = " + i + ", byteIndex = " + byteIndex);

                if (i < byteIndex) {
                    newIndex = i;
                    break;
                }
            }

            Debug.Log("newIndex = " + newIndex);
            Node subNode = new Node(id, newIndex);
            //Debug.Log(subNode.ToString());

            SubNodes.Add(subNode);

            return subNode;
        }

        public bool RemoveSubNode(Node _subNode) {

            try {
                for (int i = 0; i < _subNode.SubNodes.Count; i++) {
                    _subNode.RemoveSubNode(_subNode.SubNodes[i]);
                }
                SubNodes.Remove(_subNode);

            }
            catch {
                return false;
            }

            return true;
        }

        public override string ToString() {
            return GetType() + " (ID : " + id + ", Level : " + Level + ", parentID : " + GetParentId() + ")";
        }

        byte IdToByteIndex(string _id) {
            int  len = _id.Length;
            char lastChar = _id.ToCharArray()[len - 1];

            return Convert.ToByte(lastChar - 'a');
        }

    }
}
