using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GamePlayState 
{
    private static SupplyNetwork supplyLines;
    private SupplyEdgeBeingPlaced IntermediateEdge;

    private Profile blueTeam;
    private Profile redTeam;

    public enum CommandState
    {
        /// <summary>
        /// State when level just loads:  no commands have been issued.  
        /// </summary>
        BlankState,

        /// <summary>
        /// state when we've just started DefineSupplyLine state (UI button has just been pushed)
        /// </summary>
        DefineSupplyLinesStart,

        /// <summary>
        /// 
        /// </summary>
        DefineSupplyLine,

        /// <summary>
        /// Constant requisition soldiers.
        /// </summary>
        RequisitionSoldiers
    }

    public GamePlayState()
    {
        GamePlayState.supplyLines = new SupplyNetwork(Vector3.zero);
        this.CurrentInputState = CommandState.BlankState;
    }

    private CommandState _CurrentInputState;
    public CommandState CurrentInputState
    {
        get
        {
            return this._CurrentInputState;
        } 
        set
        {
            this._CurrentInputState = value;
        }
    }

    public Vector3 CurrentMouseWorldCoordinate
    {
        get; set;
    }

    public void DelegateClick(Vector3 worldCoordOfClick)
    {
        if( this.CurrentInputState == CommandState.BlankState)
        {
        }
        else if (this.CurrentInputState == CommandState.DefineSupplyLinesStart)
        {
            //Debug.Log("CommandState.DefineSupplyLinesStart");
            Object edgeResource = Resources.Load(@"SupplyEdgeBeingPlaced");

            //todo we should add a supply edge here. 
            GameObject edgeTest = Object.Instantiate(edgeResource, Vector3.zero, Quaternion.identity) as GameObject;
            this.IntermediateEdge = edgeTest.GetComponent(typeof(SupplyEdge)) as SupplyEdgeBeingPlaced;
            this.IntermediateEdge.startPos = Vector3.zero;
            this.CurrentInputState = CommandState.DefineSupplyLine;
        }
        else if (this.CurrentInputState == CommandState.DefineSupplyLine)
        {
            //Debug.Log("CommandState.DefineSupplyLine");

            if(this.IntermediateEdge.isValid)
            {
                Vector2 mousePosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
                Vector3 worldPositionForMouse = TopDownCamera.ScreenToWorldPoint(mousePosition);
                Vector3 endPointOfLastEdge = GamePlayState.supplyLines.AddEdge(worldPositionForMouse);
                this.IntermediateEdge.startPos = endPointOfLastEdge ;
            }
        }
        else if (this.CurrentInputState == CommandState.RequisitionSoldiers)
        {
            this.IntermediateEdge.gameObject.SetActive(false);
            Debug.Log("Requisition");
            GamePlayState.supplyLines.Requisition();
        }
    }

    /// <summary>
    /// Updates the state:  this causes the State to look for relevent events in the queue
    /// and do other update logic.  
    /// </summary>
    public void UpdateState()
    {
        List<InputEvent> events = EventQueue.GetEventQueue();
        List<InputEvent> eventsToRemove = new List<InputEvent>();

        foreach(InputEvent inputEvent in events)
        {
            if(inputEvent is CommandEvent)
            {
                CommandEvent commandEvent = (CommandEvent)inputEvent;
                this.CurrentInputState = commandEvent.Command;
                DelegateClick(inputEvent.worldPosition);
                eventsToRemove.Add (inputEvent);
            }
            else
            {
                // theres been a general mouseclick.  Take action on it, depending on our 
                // current state. 
                DelegateClick(inputEvent.worldPosition);
                eventsToRemove.Add(inputEvent);
            }
        }

        EventQueue.RemoveEvents(eventsToRemove);
    }

    public static SupplyNetwork GetSupplyLines()
    {
        return GamePlayState.supplyLines;
    }
}
