using UnityEditor;
using UnityEngine;

namespace CustomPieMenu {
    [CustomEditor(typeof(ButtonManager))]
    [CanEditMultipleObjects]
    public class ButtonManagerEditor : Editor {

        private ButtonManager managerScript;

        void OnEnable() {
            managerScript = (ButtonManager)target;
        }

        // Use this for initialization
        public override void OnInspectorGUI() {

            DrawDefaultInspector();
            
            if(GUILayout.Button("Create new menu")) {
                managerScript.CreateNewMenu();
            }


        }
    }
}
