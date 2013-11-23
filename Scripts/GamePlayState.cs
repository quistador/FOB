using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GamePlayState : MonoBehaviour
{
    private static SupplyNetwork supplyLines;
    private SupplyEdgeBeingPlaced IntermediateEdge;

    private Profile blueTeamProfile;
    private Profile redTeamProfile;

    private Army blueTeam;
    private Army redTeam;

    private Dictionary<Guid, Building> buildingIdToBuildingInfo;

    private LevelV0 LevelData;

    // Use this for initialization
    void Start () 
    {
        UnityEngine.Object levelV0 = Resources.Load(@"LevelV0");
        GameObject levelV0Object = UnityEngine.Object.Instantiate(levelV0, Vector3.zero, Quaternion.identity) as GameObject;
        this.LevelData = levelV0Object.GetComponent(typeof(LevelV0)) as LevelV0;
        this.LevelData.Initialize();

        this.blueTeamProfile = new Profile("testProfile");
        this.redTeamProfile = new Profile("testEnemyTeam");

        this.redTeam = new Army(this.redTeamProfile);
        this.blueTeam = new Army(this.blueTeamProfile);

        this.CurrentGameMode = GameMode.BlankState;
        this.buildingIdToBuildingInfo = new Dictionary<Guid, Building>();
  
        int armyStartingPointNodeId = -1;

        List<Building> buildings = this.LevelData.Buildings;

        // care needs to be taken on *where* we instantiate the supply lines:
        // note that the SupplyNetwork constructor performs writes to a static class field. 
        // This isn't preferable, and it's something that should be refactored soon. 
        GamePlayState.supplyLines = new SupplyNetwork(this.blueTeam, this.LevelData);
        foreach(Building building in buildings)
        {
            this.buildingIdToBuildingInfo.Add(building.buildingId, building);

            if(building.isStartingPosition)
            {
                armyStartingPointNodeId = building.nodeIdsForEntryPoints.First();

                // if this building is a starting point for all of our units, then 
                // we need to let our supply network 'know', so that it knows where to 
                // position our units in the initial configuration. 
                GamePlayState.supplyLines.MarkAsStartingPoint(building.nodeIdsForEntryPoints.First());
            }
        }
    }

    // Update is called once per frame
    void Update () 
    {
        this.CurrentMouseWorldCoordinate = new Vector3(
            (float)Input.mousePosition.x, 
            (float)Input.mousePosition.y, 
            0f);

        this.ProcessInputEvents();
    }

    public enum GameMode
    {
        /// <summary>
        /// State when level just loads:  no commands have been issued.  
        /// </summary>
        BlankState,

        /// <summary>
        /// game mode where player enters commands. 
        /// </summary>
        CommandMode,

        /// <summary>
        /// game mode where units act based on provided commands. Unlike other modes,
        /// commands are not entered in this mode. 
        /// </summary>
        ActionMode,

        /// <summary>
        /// state when we've just started DefineSupplyLine state (UI button has just been pushed)
        /// </summary>
        DefineSupplyLinesStart,

        /// <summary>
        /// 
        /// </summary>
        DefineSupplyLine,

        /// <summary>
        /// Orders unit movement
        /// </summary>
        OrderUnitMovementMode
    }

    public GamePlayState()
    {

    }

    private GameMode _CurrentGameMode;
    public GameMode CurrentGameMode
    {
        get
        {
            return this._CurrentGameMode;
        } 
        set
        {
            this._CurrentGameMode = value;
        }
    }

    public Vector3 CurrentMouseWorldCoordinate
    {
        get; set;
    }

    public void DelegateClick(Vector3 worldCoordOfClick)
    {
        if( this.CurrentGameMode == GameMode.BlankState)
        {
        }
        else if (this.CurrentGameMode == GameMode.DefineSupplyLinesStart)
        {
            UnityEngine.Object edgeResource = Resources.Load(@"SupplyEdgeBeingPlaced");

            GameObject edgeTest = UnityEngine.Object.Instantiate(edgeResource, Vector3.zero, Quaternion.identity) as GameObject;
            this.IntermediateEdge = edgeTest.GetComponent(typeof(SupplyEdge)) as SupplyEdgeBeingPlaced;
            this.IntermediateEdge.startPos = Vector3.zero;
            this.CurrentGameMode = GameMode.DefineSupplyLine;
        }
        else if (this.CurrentGameMode == GameMode.DefineSupplyLine)
        {
            if(this.IntermediateEdge.isValid)
            {
                Vector2 mousePosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
                Vector3 worldPositionForMouse = TopDownCamera.ScreenToWorldPoint(mousePosition);
                Vector3 endPointOfLastEdge = GamePlayState.supplyLines.AddEdge(worldPositionForMouse);
                this.IntermediateEdge.startPos = endPointOfLastEdge ;
            }
        }
        else if (this.CurrentGameMode == GameMode.OrderUnitMovementMode)
        {
            this.IntermediateEdge.gameObject.SetActive(false);
            Debug.Log("Requisition");
            GamePlayState.supplyLines.Requisition(
                    this.LevelData.GetOriginBuilding(),
                    this.LevelData.GetDestinationBuilding());
        }
    }

    /// <summary>
    /// Updates the state:  this causes the State to look for relevent events in the queue
    /// and do other update logic.  
    /// </summary>
    private void ProcessInputEvents()
    {
        List<InputEvent> events = EventQueue.GetEventQueue();
        List<InputEvent> eventsToRemove = new List<InputEvent>();

        foreach(InputEvent inputEvent in events)
        {
            if(inputEvent is CommandEvent)
            {
                CommandEvent commandEvent = (CommandEvent)inputEvent;
                this.CurrentGameMode = commandEvent.Command;
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
