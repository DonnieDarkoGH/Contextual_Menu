using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace CustomPieMenu {
    [CustomEditor(typeof(SO_MenuStructure))]
    [CanEditMultipleObjects]
    public class SO_MenuStructureEditor : Editor {

        private SO_MenuStructure managerScript;

        GUIStyle foldoutStyle1, foldoutStyle2;
        bool     isSetup = false;
        bool[]   showDetails;
        bool[]   showSubMenus;
        int      buttonCount;

        void OnEnable() {
            managerScript = (SO_MenuStructure)target;
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

            if (GUILayout.Button("Clear")) {
                Undo.RecordObject(target, "Clear data");
                managerScript.Buttons = new System.Collections.Generic.List<ButtonModel>(1) { new ButtonModel("@") };
                managerScript.Root.SubNodes.Clear();
                isSetup = SetupReferences();
                serializedObject.ApplyModifiedProperties();
            }

            DisplayMenuByNode(managerScript.Root);

            if (GUI.changed) {
                EditorUtility.SetDirty(target);
            }
        }

        private void DisplayMenuByNode(Node _node) {

            if (!isSetup) {
                isSetup = SetupReferences();
                return;
            }

            byte level = _node.Level;

            ButtonModel button = managerScript.GetButtonFromNode(_node);
            int         foldId = managerScript.Buttons.IndexOf(button);

            if (foldId < 0)
                return;

            /** Header (Current Button) **/
            EditorGUILayout.BeginHorizontal();
            showDetails[foldId] = EditorGUILayout.Foldout(showDetails[foldId], "Level " + level, foldoutStyle1);
            button.name         = EditorGUILayout.TextField(button.name);

            if (level > 0 && GUILayout.Button("-")) {
                managerScript.RemoveSubMenu(_node);
                isSetup = false;
                return;
            }

            EditorGUILayout.EndHorizontal();
            /** End of the header **/

            /** Details of the Button **/
            if (showDetails[foldId]) {
            EditorGUI.indentLevel++;
                // Base data (Ids) and related sprite
                EditorGUILayout.LabelField("Id "+ _node.Id);
                EditorGUILayout.LabelField("ParentId " + _node.GetParentId());
                
                EditorGUI.BeginChangeCheck();
                Sprite icon = (Sprite)EditorGUILayout.ObjectField("Sprite", button.icon, typeof(Sprite), false);
                if (EditorGUI.EndChangeCheck()) {
                    button.icon = icon;
                }

                EditorGUILayout.Separator();
                GUILayout.Space(10);

                // Foldout label to hide/show sub-menus
                EditorGUILayout.BeginHorizontal();
                showSubMenus[foldId] = EditorGUILayout.Foldout(showSubMenus[foldId], "Related Sub-menus", foldoutStyle2);
                // and '+' button to create new ones
                if (GUILayout.Button("+")) {
                    managerScript.AddNewSubMenu(_node);
                    isSetup = false;
                    return;
                }
                EditorGUILayout.EndHorizontal();
                
                // List related sub-menus
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
