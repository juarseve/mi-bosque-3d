using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

public class EventManager : MonoBehaviour {

    private Dictionary <string, UnityEvent> eventDictionary= new Dictionary<string, UnityEvent>();

    public static EventManager eventManager;

    private void Awake() {
        if(eventManager==null){
            eventManager=this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }
    
    public static void StartListening (EventsDictionary eD)
    {
        if(!eventManager.eventDictionary.ContainsKey(eD.eventName))
            eventManager.eventDictionary.Add (eD.eventName, eD.eventToAdd);
    }

    public static void StopListening (EventsDictionary eD)
    {
        if (eventManager == null) return;
        if (eventManager.eventDictionary.ContainsKey (eD.eventName))
            eventManager.eventDictionary.Remove(eD.eventName);
    }

    public static void StopListening(string eventName){
        if (eventManager.eventDictionary.ContainsKey (eventName))
            eventManager.eventDictionary.Remove(eventName);
    }

    public  void TriggerEvent2 (string eventName)
    {
        UnityEvent thisEvent = null;
        if (eventManager.eventDictionary.TryGetValue (eventName, out thisEvent))
        {
            thisEvent.Invoke ();
        }
    }

    public void TriggerEvent(string eventName)
    {
        UnityEvent thisEvent;

        if (eventManager.eventDictionary.TryGetValue(eventName, out thisEvent))
        {
            Debug.Log($"Invoking event: {eventName}");
            Debug.Log($"Listener count: {thisEvent.GetPersistentEventCount()}");

            for (int i = 0; i < thisEvent.GetPersistentEventCount(); i++)
            {
                Debug.Log(
                    $"Listener {i}: " +
                    thisEvent.GetPersistentTarget(i) + " -> " +
                    thisEvent.GetPersistentMethodName(i)
                );
            }

            thisEvent.Invoke();
        }
    }

}