using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EventQueue 
{
    private static List<InputEvent> _events;

    public static void AddToEventQueue(InputEvent newEvent)
    {
        EventQueue._events.Add (newEvent);
    }

    public static List<InputEvent> GetEventQueue()
    {
        if(_events == null)
        {
            _events = new List<InputEvent>();
        }
        return _events;
    }

    public static void RemoveEvents(List<InputEvent> inactiveEvents)
    {
        foreach(InputEvent inEvent in inactiveEvents)
        {
            _events.Remove(inEvent);
        }
        
        if(inactiveEvents.Count > 0)
		{
		    //Debug.Log(string.Format("{0} events will be removed",inactiveEvents.Count)); 
	        //Debug.Log(string.Format("{0} events remaining after removal", _events.Count));
		}
    }
}
