using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GamePlayEvents 
{
    //public static List<GamePlayEvent> Events = new List<GamePlayEvent>();
    

    /// <summary>
    /// The events, indexed by key, where key is the supply node id of our network. 
    /// </summary>
    private static Dictionary<int,List<GamePlayEvent>> Events = new Dictionary<int,List<GamePlayEvent>>();
    public static void AddEvent(int nodeId, GamePlayEvent newEvent)
    {
        if(!Events.ContainsKey(nodeId))
        {
            Events[nodeId] = new List<GamePlayEvent>();
        }
        
        Events[nodeId].Add(newEvent);
        Debug.Log(string.Format ("added Event to node {0}, count now: {1}",nodeId,Events[nodeId].Count));
    }

    public static List<GamePlayEvent> GetEventsForId(int id)
    {
        if(!Events.ContainsKey(id))
        {
            // return an empty list if there are no events. 
            Events[id] = new List<GamePlayEvent>();
        }

        List<GamePlayEvent> events = Events[id];
        
        // we need some way to deactivate events.  
        // for now, we'll just remove them once they are requested.  This will
        // probably need to change at a later point. 
        //Events[id].Clear();
        
        return events;
    }

    /// <summary>
    /// Removes Event 
    /// </summary>
    public static void RemoveEvent(int nodeId, GamePlayEvent eventToRemove)
    {
        Events[nodeId].Remove(eventToRemove);
    }
    
    public static void RemoveEvents(int nodeId, List<GamePlayEvent> eventsToRemove)
    {
        eventsToRemove.ForEach( ev => Events[nodeId].Remove(ev) );
    }
}
