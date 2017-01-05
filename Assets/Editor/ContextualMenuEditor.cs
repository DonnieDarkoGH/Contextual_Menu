using System;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using ContextualMenu;
using System.Collections.Generic;

namespace ContextualMenuData {
    [CustomEditor(typeof(ScriptableMenuStructure))]
    [CanEditMultipleObjects]
    public class ContextualMenuEditor : Editor {

        private ScriptableMenuStructure targetScript;
        private ReorderableList     nodesListProp;
        private SerializedProperty  buttonsListProp;
        private SerializedProperty  contextProp;

        private float lineHeight     = EditorGUIUtility.singleLineHeight * 1.5f;
        private float nameFieldWidth = 150f;
        private float idFieldWidth   = 50f;
        private float iconFieldWidth = 200.0f;
        private GUIStyle titleStyle  = new GUIStyle();

        private bool   isReodering   = false;
        private int    selectedIndex = -1;
        private int[]  nodeIndexesToReorder;
        private string nodeIndexesToReorderToString;

        private void OnEnable() {

            targetScript = (ScriptableMenuStructure)target;

            contextProp  = serializedObject.FindProperty("Context");

            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.fontSize = 14;

            nodesListProp   = new ReorderableList(serializedObject, serializedObject.FindProperty("serializedNodes"), true, true, true, true);
            buttonsListProp = serializedObject.FindProperty("Buttons");
            
            nodesListProp.headerHeight  = lineHeight;
            nodesListProp.elementHeight = lineHeight;
            nodesListProp.footerHeight  = lineHeight;

            nodesListProp.drawHeaderCallback  = (Rect rect) => { EditorGUI.LabelField(rect, "Buttons in menu", titleStyle); };

            nodesListProp.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
                var button = buttonsListProp.GetArrayElementAtIndex(index);
                var node   = nodesListProp.serializedProperty.GetArrayElementAtIndex(index);
                rect.y += 2;
                rect.x += 10 * (node.FindPropertyRelative("Id").stringValue.Length - 1);
                previewIcon(rect, index);
                DrawField(button, rect, nameFieldWidth, "name");
                DrawField(button, rect, idFieldWidth  , "id", nameFieldWidth);
                DrawField(node  , rect, idFieldWidth  , "Id", nameFieldWidth + idFieldWidth);
                DrawField(button, rect, iconFieldWidth, "Icon", nameFieldWidth + idFieldWidth * 2);
            };

            nodesListProp.onAddCallback = (ReorderableList rlist) => {
                Debug.Log(rlist.index);
                if (rlist.index < 0) {
                    targetScript.AddNode(targetScript.serializedNodes[0]);
                }
                else {
                    targetScript.AddNode(targetScript.serializedNodes[rlist.index]);
                }
            };

            nodesListProp.onRemoveCallback = (ReorderableList rlist) => {
                if (EditorUtility.DisplayDialog("Warning!", "Are you sure you want to delete this button ?", "Yes", "No")) {
                    targetScript.RemoveButton(targetScript.Buttons[rlist.index]);
                    serializedObject.ApplyModifiedProperties();
                }
            };

            nodesListProp.onReorderCallback = (ReorderableList rlist) => {
                //Debug.Log("onReorderCallback " + rlist.index);

                isReodering = selectedIndex != rlist.index;
            };

            nodesListProp.onSelectCallback = (ReorderableList rlist) => {
                //Debug.Log("onSelectCallback " + rlist.index);

                selectedIndex = rlist.index;
                

                var node = targetScript.serializedNodes[rlist.index];

                nodeIndexesToReorder = targetScript.GetNodeAndSubChildrenId(node);

                nodeIndexesToReorderToString = "";
                for (int i = 0; i < nodeIndexesToReorder.Length; i++) {
                    nodeIndexesToReorderToString += nodeIndexesToReorder[i] + ",";
                }
                //Debug.Log(targetScript.GetNodeInfo(node) + " has " + node.ChildCount + " children : " + s);
            };

            nodesListProp.onChangedCallback = (ReorderableList rlist) => {
                //Debug.Log("onChangedCallback " + rlist.index);

                int ind = rlist.index;
                if (isReodering) {
                    ind = targetScript.ReorderElements(nodeIndexesToReorder, ind);
                    isReodering = false;
                }

                nodesListProp.index = ind;
            };

        }

         
        public override void OnInspectorGUI() {
            serializedObject.Update();

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Selected Index       : " + selectedIndex);
            EditorGUILayout.LabelField("nodeIndexesToReorder : " + nodeIndexesToReorderToString);
            GUILayout.EndHorizontal();

            EditorGUILayout.PropertyField(contextProp);

            if (GUILayout.Button("Clear")) {
                targetScript.ClearAll();
                nodesListProp.index  = -1;
                nodeIndexesToReorder = new int[0];
                serializedObject.ApplyModifiedProperties();
                return;
            }

            nodesListProp.DoLayoutList();

            serializedObject.ApplyModifiedProperties();
        }

        private bool DrawField(SerializedProperty _prop, Rect _rectangle, float _width, string _fieldName, float _previousWidth = 0.0f) {

            Rect  rect   = new Rect (lineHeight + _rectangle.x + _previousWidth,
                                     _rectangle.y,
                                     _width,
                                     lineHeight);
            SerializedProperty findProp = _prop.FindPropertyRelative(_fieldName);

            return EditorGUI.PropertyField(rect, findProp, GUIContent.none);
        }

        private void previewIcon(Rect _rectangle, int _index) {

            Texture texture = targetScript.Buttons[_index].Icon ? targetScript.Buttons[_index].Icon.texture : Texture2D.blackTexture;

            Rect rect = new Rect(_rectangle.x, _rectangle.y, lineHeight, lineHeight);

            EditorGUI.DrawTextureTransparent(rect, texture);

        }
    }
}
