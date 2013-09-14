using UnityEngine;
using System.Collections;

public class InputEvent  
{
    private Ray mouseClickPosition;

    public InputEvent()
    {
        this.worldPosition = Vector3.zero;
        this.forward = Vector3.zero;
    }

    public InputEvent(Vector3 worldPosition, Vector3 forward)
    {
        this.worldPosition = worldPosition;
        this.forward = forward;
    }

    public Vector3 worldPosition
    {
        get; set;
    }

    public Vector3 forward
    {
        get; set;
    }

    public Ray mouseClickForEvent
    {
        get {return mouseClickPosition;}
    }
}
