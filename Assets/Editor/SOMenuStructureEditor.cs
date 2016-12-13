using System;
using UnityEditor;
using UnityEngine;

namespace CustomPieMenu {
    [CustomEditor(typeof(SOMenuStructure))]
    [CanEditMultipleObjects]
    public class SOMenuStructureEditor : Editor {

        private SOMenuStructure managerScript;

        GUIStyle foldoutStyle1, foldoutStyle2;
        bool     isSetup = false;
        bool[]   showDetails;
        bool[]   showSubMenus;
        int      buttonCount;

        void OnEnable() {
            managerScript = (SOMenuStructure)target;
        }

        bool SetupReferences() {

            buttonCount = managerScript.Buttons.Count;

            Array.Resize(ref showDetails, buttonCount);
            Array.Resize(ref showSubMenus, buttonCount);

            foldoutStyle1 = new GUIStyle(EditorStyles.foldout);
            foldoutStyle2 = new GUIStyle(EditorStyles.foldout);

            foldoutStyle1.fixedWidth = 50;
            foldoutStyle1.fontStyle = FontStyle.Bold;
            foldoutStyle2.fontStyle = FontStyle.Bold;

            return true;
        }

        // Use this for initialization
        public override void OnInspectorGUI() {

            if (!isSetup) {
                isSetup = SetupReferences();
            }

            if (buttonCount == 0) {
                return;
            }

            DrawDefaultInspector();
            EditorGUILayout.LabelField("Level 0 : " + managerScript.Root.SubNodes.Count);

            if (GUILayout.Button("Clear")) {
                managerScript.Buttons = new System.Collections.Generic.List<ButtonModel>(1) { new ButtonModel() };
                managerScript.Root.SubNodes.Clear();
                isSetup = SetupReferences();
            }

            DisplayMenuByNode(managerScript.Root);

        }


        private void DisplayMenuByNode(SOMenuStructure.Node _node) {

            if (!isSetup) {
                isSetup = SetupReferences();
                return;
            }

            ButtonModel button = managerScript.GetButtonFromNode(_node);

            int  nodeCount  = _node.SubNodes.Count;
            byte level      = _node.Level;
            byte id         = _node.ID;
            int  foldId     = managerScript.Buttons.IndexOf(button);

            if (foldId < 0)
                return;

            EditorGUILayout.BeginHorizontal();
            showDetails[foldId] = EditorGUILayout.Foldout(showDetails[foldId], "Level " + level, foldoutStyle1);
            managerScript.GetButtonFromNode(_node).name = EditorGUILayout.TextField(button.name);
            if (level > 0 && GUILayout.Button("-")) {
                managerScript.RemoveSubMenu(_node);
                isSetup = false;
                return;
            }
            EditorGUILayout.EndHorizontal();

            if (showDetails[foldId]) {
                EditorGUI.indentLevel++;
                _node.ID       = (byte)EditorGUILayout.IntField("Id ", _node.ID);
                _node.ParentID = (byte)EditorGUILayout.IntField("Parent Id ", _node.ParentID);
                managerScript.GetButtonFromNode(_node).icon    = (Sprite)EditorGUILayout.ObjectField("Sprite", managerScript.GetButtonFromNode(_node).icon, typeof(Sprite), false);
                //button.index       = (byte)EditorGUILayout.IntField("Id ", button.index);
                //button.parentIndex = (byte)EditorGUILayout.IntField("Parent Id ", button.parentIndex);
                //button.icon        = (Sprite)EditorGUILayout.ObjectField("Sprite", button.icon, typeof(Sprite), false);

                EditorGUILayout.BeginHorizontal();
                showSubMenus[foldId] = EditorGUILayout.Foldout(showSubMenus[foldId], "Related Sub-menus", foldoutStyle2);
                if (GUILayout.Button("+")) {
                    managerScript.AddNewSubMenu(_node);
                    isSetup = false;
                    return;
                }
                EditorGUILayout.EndHorizontal();

                if (showSubMenus[foldId]) {
                    for (int i = 0; i < _node.SubNodes.Count; i++) {
                        DisplayMenuByNode(_node.SubNodes[i]);
                    }
                }

                EditorGUI.indentLevel--;

            }
        }

        private void OnDisable() {
            isSetup = false;
        }
    }
}
