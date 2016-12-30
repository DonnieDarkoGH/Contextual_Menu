using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using ContextualMenu;
using UnityEngine.EventSystems;


public class EventTests : MonoBehaviour {

    public ButtonModelEvent ButtonEvents;
    public Object     Target;
    public MethodInfo Method;

	// Use this for initialization
	void Start () {

        Debug.Log("ToString                :" + ButtonEvents.ToString());
        Debug.Log("GetPersistentEventCount :" + ButtonEvents.GetPersistentEventCount());
        Debug.Log("GetPersistentMethodName :" + ButtonEvents.GetPersistentMethodName(0));
        Debug.Log("GetPersistentTarget     :" + ButtonEvents.GetPersistentTarget(0));
        Debug.Log("GetPersistentTargetID   :" + ButtonEvents.GetPersistentTarget(0).GetInstanceID());

        Target      = ButtonEvents.GetPersistentTarget(0);
        ButtonEvents.GetPersistentTarget(0).GetType().GetMethod(ButtonEvents.GetPersistentMethodName(0));

        Method = ButtonEvents.GetPersistentTarget(0).GetType().GetMethod(ButtonEvents.GetPersistentMethodName(0));
        

        MemberInfo[] infos = typeof(ButtonModelEvent).GetMembers();
        foreach(var info in infos) {
            Debug.Log(info.Name);
        }
        



    }
	
}
