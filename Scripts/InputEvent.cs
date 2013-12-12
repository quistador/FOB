using UnityEngine;
using System.Collections;

public class InputEvent  
{
    private Ray mouseClickPosition;

    /// <summary>
    /// has something special happened hear?  such as a button click or a click 
    /// on a game object that needs to be handled specially? this enumeration
    /// defines such possibilities. 
    /// </summary>
    public enum EventType
    {
        /// <summary>
        /// most events will probably just be a mouse down event. 
        /// </summary>
        MouseDown,
        WaypointButtonPressed,
        ClicksOnSquad,
        ReleasesMouseDown,
    }

    public InputEvent()
    {
        this.worldPosition = Vector3.zero;
        this.forward = Vector3.zero;
        this.interfaceEvent = InputEvent.EventType.MouseDown;
    }

    public InputEvent(Vector3 worldPosition, Vector3 forward, InputEvent.EventType interfaceEvent)
    {
        this.worldPosition = worldPosition;
        this.forward = forward;
        this.interfaceEvent = interfaceEvent;
    }


    public InputEvent(Vector3 worldPosition, Vector3 forward)
    {
        this.worldPosition = worldPosition;
        this.forward = forward;
    }

    public InputEvent(InputEvent.EventType newMode)
    {
        this.interfaceEvent = newMode;
    }

    public Vector3 worldPosition
    {
        get; set;
    }

    public Vector3 forward
    {
        get; set;
    }

    public InputEvent.EventType interfaceEvent
    {
        get; set;
    }

    public Ray mouseClickForEvent
    {
        get {return mouseClickPosition;}
    }
}
