using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ContextualMenuData;

namespace ContextualMenu {
    
    public enum EContext {
        None = 0,
        Pawn,
        Board,
        Other,
    }

    public interface ISelectable {

        void       HandleSelection(Vector3 inputPosition);
        GameObject GetGameObject();
    }

    [RequireComponent(typeof(Collider))]
    public class Selectable : MonoBehaviour, ISelectable {

        public EContext context;

        public void HandleSelection(Vector3 _inputPosition) {
            //Debug.Log("<b>Selectable</b> HandleSelection in " + _inputPosition);

            ScriptableMenuStructure[] menus = Resources.FindObjectsOfTypeAll<ScriptableMenuStructure>();

            int len = menus.Length;
            for(int i = 0; i < len; i++) {
                if (menus[i].Context == context) {
                    MenuManager.Instance.InitializeMenuContext(menus[i]);
                    return;
                }
            }

        }

        public GameObject GetGameObject() {
            return gameObject;
        }

    }
}
