using UnityEngine;
using System.Collections;

public class InputEvent  
{
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
    }

    public InputEvent(Vector3 worldPosition, Vector3 forward, InputEvent.EventType interfaceEvent, Vector3 cameraPosition) : this(worldPosition,forward,interfaceEvent)
    {
        this.CameraPosition = cameraPosition;
    }

    public InputEvent(Vector3 worldPosition, Vector3 forward, InputEvent.EventType interfaceEvent) : this(worldPosition,forward)
    {
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

    /// <summary>
    /// the camera position when the input event was added. 
    /// </summary>
    public Vector3 CameraPosition
    {
        get; set;
    }
}
