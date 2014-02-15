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

    /// <summary>
    /// static dictionary mapping all squad guids to current squad state. 
    /// as game play progresses, updates to the state of different squads should be made to this structure, 
    /// not to the Army objects (which are generally used for persisting army state between games, not in-game 
    /// updates). 
    /// </summary>
    private static Dictionary<Guid, Squad> squadIdToSquadInfo;

    public static Guid squadIdForStartDrag = new Guid();
    private static List<int> buildingsConnectedToStartDragBuilding = new List<int>();

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

        squadIdToSquadInfo = new Dictionary<Guid, Squad>();

        this.redTeam.Squads.ForEach( squad => squadIdToSquadInfo[squad.id] = squad );
        this.blueTeam.Squads.ForEach( squad => squadIdToSquadInfo[squad.id] = squad );

        this.CurrentGameMode = GameMode.BlankState;
        this.CurrentInputState = InputState.BlankState;
    
        this.buildingIdToBuildingInfo = new Dictionary<Guid, Building>();

        List<Building> buildings = this.LevelData.Buildings;

        GamePlayState.supplyLines = new SupplyNetwork(this.blueTeam, this.LevelData, this);
        foreach(Building building in buildings)
        {
            this.buildingIdToBuildingInfo.Add(building.buildingId, building);

            if(building.isStartingPosition)
            {
                // if this building is a starting point for all of our units, then 
                // we need to let our supply network 'know', so that it knows where to 
                // position our units in the initial configuration. 
                GamePlayState.supplyLines.MarkAsStartingPoint(building.nodeIdsForEntryPoints.First());
            }
        }

        // link up event handlers with the camera. 
        if(Camera.allCameras.Length != 1)
        {
            throw new System.ArgumentException("we only expected one active camera");
        }
        
        Camera activeCamera = Camera.allCameras[0];
        TopDownCamera camera = activeCamera.GetComponentsInChildren<TopDownCamera>().First() as TopDownCamera;
        camera.ActionModeButtonPressed += this.ActionModeButtonPressed;
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

    /// <summary>
    /// controls high level game aspects:
    /// generally, keeping track of command mode or action mode. 
    /// </summary>
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
        ActionMode
    }


    /// <summary>
    /// defines different input states, generally overlapping with the command state. 
    /// this class keeps track of the different states we'll be in as the user 
    /// starts and stops clicking to do things like define supply lines or unit movement orders. 
    ///
    /// InputState state can change for a variety of reasons, sometimes clicking on a UI button will cause a change, 
    /// sometimes clicking somewhere in the play area will cause a change. 
    /// </summary>
    public enum InputState
    {
        /// <summary>
        /// starting and default input mode. 
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
        /// Starts drag from a squad to another point. 
        /// </summary>
        SquadStartDrag
    }

    public GamePlayState()
    {

    }

    public InputState CurrentInputState
    {
        get;
        set;
    }

    public GameMode CurrentGameMode
    {
        get;
        set;
    }

    public Vector3 CurrentMouseWorldCoordinate
    {
        get; set;
    }

    public List<IHousable> Housables
    {
        get
        {
            List<IHousable> houses = this.LevelData.Buildings.Cast<IHousable>().ToList();

            // converting all of our network nodes to 'IHousable' is a bit complicated.
            // Start by querying all gameObjects tagged with 'SupplyNode'. 
            // then, call 'GetComponent' on the game object to get the TacticsGame.SupplyNode, 
            // then cast it to an IHousable.
            GameObject[] supplyNodesObjects = GameObject.FindGameObjectsWithTag("SupplyNode");
            List<TacticsGame.SupplyNode> supplyNodes = new List<TacticsGame.SupplyNode>();
            foreach(GameObject obj in supplyNodesObjects)
            {
                TacticsGame.SupplyNode[] nodes = obj.GetComponentsInChildren<TacticsGame.SupplyNode>() as TacticsGame.SupplyNode[];

                // not sure why but there appears to be an object that's tagged with 'SupplyNode' that
                // doesn't have a supplyNode component on it.  Until I figure out why, I'll need to perform this check
                if(nodes.Length == 1)
                {
                    supplyNodes.Add (nodes.First());
                }
                else
                {
                    Debug.Log("we expect each supplynode to have a respective component");
                }
            }
            List<IHousable> HousableNodes = supplyNodes.Cast<IHousable>().ToList();
            houses.AddRange(HousableNodes);
            return houses;
        }
    }

    public void DelegateInputEvent(InputEvent inputEventInfo)
    {
        // this function looks at the inputevent, and the current inputstate, to determine
        // any actions to take, as well as transitioning to the next input state. 

        if( this.CurrentInputState == InputState.BlankState)
        {
            // ok, we're in the 'blank' state. 
            if(inputEventInfo.interfaceEvent == InputEvent.EventType.WaypointButtonPressed)
            {
                // if the user clicked on the 'waypoint', put us in a supplyLinesStart state. 
                this.CurrentInputState = InputState.DefineSupplyLinesStart;
            }
            else if(inputEventInfo.interfaceEvent == InputEvent.EventType.ClicksOnSquad)
            {
                // find the building that contains the squad. 
                IHousable housingWithSelectedSquadInIt = this.Housables.Find( housable =>
                    housable.ContainsSquadId(GamePlayState.squadIdForStartDrag) );

                // get all node entry points for that building. 
                List<int> doorsForBuilding = housingWithSelectedSquadInIt.NodeIdsInHousing();
                List<int> buildingIdsForConnectedBuildings = GamePlayState.GetSupplyLines().GetConnectedBuildings( doorsForBuilding );
                GamePlayState.buildingsConnectedToStartDragBuilding = buildingIdsForConnectedBuildings;

                // the user clicked down on a squad... enter the ClicksOnSquad state (which should 
                // be kept until the user releases the mouse button). 
                this.CurrentInputState = InputState.SquadStartDrag;
            }
        }
        else if (this.CurrentInputState == InputState.DefineSupplyLinesStart)
        {
            if(inputEventInfo.interfaceEvent == InputEvent.EventType.WaypointButtonPressed)
            {
                // if the user clicked on the 'waypoint' button and we are already 
                // defining supply lines, deactivate our intermediate edge and 
                // go back to the blank state. 
                this.CurrentInputState = InputState.BlankState;
                this.IntermediateEdge.gameObject.SetActive(false);
                return;
            }

            UnityEngine.Object edgeResource = Resources.Load(@"SupplyEdgeBeingPlaced");

            GameObject edgeTest = UnityEngine.Object.Instantiate(edgeResource, Vector3.zero, Quaternion.identity) as GameObject;
            this.IntermediateEdge = edgeTest.GetComponent(typeof(SupplyEdge)) as SupplyEdgeBeingPlaced;
            this.IntermediateEdge.startPos = Vector3.zero;
            this.CurrentInputState = InputState.DefineSupplyLine;
        }
        else if (this.CurrentInputState == InputState.DefineSupplyLine)
        {
            if(inputEventInfo.interfaceEvent == InputEvent.EventType.WaypointButtonPressed)
            {
                // if the user clicked on the 'waypoint' button and we are already 
                // defining supply lines, deactivate our intermediate edge and 
                // go back to the blank state. 
                this.CurrentInputState = InputState.BlankState;
                this.IntermediateEdge.gameObject.SetActive(false);
                return;
            }

            // only add nodes when the mouse button is released, not when it's first pressed. 
            if(inputEventInfo.interfaceEvent == InputEvent.EventType.ReleasesMouseDown)
            {
                if(this.IntermediateEdge.isValid)
                {
                    Vector2 mousePosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
                    Vector3 worldPositionForMouse = TopDownCamera.ScreenToWorldPoint(mousePosition);
                    Vector3 endPointOfLastEdge = GamePlayState.supplyLines.AddEdge(worldPositionForMouse);
                    this.IntermediateEdge.startPos = endPointOfLastEdge ;
                }
            }
        }
        else if (this.CurrentInputState == InputState.SquadStartDrag)
        {
            if(inputEventInfo.interfaceEvent == InputEvent.EventType.ReleasesMouseDown)
            {
                Vector3 worldCoordsOfClick = inputEventInfo.worldPosition;
                GamePlayState.buildingsConnectedToStartDragBuilding = new List<int>();
                this.CurrentInputState = InputState.BlankState;
                RaycastHit hit;

                Vector3 cameraPositionAtTimeOfRelease = inputEventInfo.CameraPosition;
                Debug.DrawRay(worldCoordsOfClick, (worldCoordsOfClick - cameraPositionAtTimeOfRelease ));

                /// did the mouse release land on a collider?
                if (Physics.Raycast(worldCoordsOfClick, (worldCoordsOfClick - inputEventInfo.CameraPosition), out hit, 100f))
                {
                    Debug.Log("Release mouse on" + hit.collider.gameObject.name);

                    Building clickedOnBuilding = null;
                    TacticsGame.SupplyNode clickedOnSupplyNode = null;
                    bool IsOrderValid = false;

                    Vector3 startPositionOfUnit = Vector3.zero;
                    Vector3 endPositionOfUnit = Vector3.zero;

                    // default to obscure value to avoid 'use of uninitialized variable' compilation errors. 
                    int endNodeIdOfPath = -8008; 

                    // find the housing that contains the squad. 
                    IHousable housingWithSelectedSquadInIt = this.Housables.Find( housing =>
                        housing.ContainsSquadId(GamePlayState.squadIdForStartDrag) );
                    int startNodeIdOfPath = housingWithSelectedSquadInIt.NodeIdsInHousing().First();

                    // did the mouse release land on a node?
                    if(TacticsGame.SupplyNode.IsRaycastHittingSupplyNode(hit.collider.gameObject, ref clickedOnSupplyNode))
                    {
                        endNodeIdOfPath = clickedOnSupplyNode.nodeId;
                        startPositionOfUnit = housingWithSelectedSquadInIt.UnitsInHousing().GetPositionOfSquad(GamePlayState.squadIdForStartDrag);
                        endPositionOfUnit = clickedOnSupplyNode.transform.position;
                        IsOrderValid = true;

                    }
                    // did the mouse release land on a building?
                    else if(Building.IsRaycastHittingBuilding(hit.collider.gameObject, ref clickedOnBuilding))
                    {
                        endNodeIdOfPath = clickedOnBuilding.nodeIdsForEntryPoints.First();
                        startPositionOfUnit = housingWithSelectedSquadInIt.UnitsInHousing().GetPositionOfSquad(GamePlayState.squadIdForStartDrag);
                        endPositionOfUnit = clickedOnBuilding.BuildingCenter;
                        IsOrderValid = true;
                    }

                    if(IsOrderValid == true)
                    {
                        Order movementOrder = new Order()
                        { 
                            command = Orders.OrderCommand.MoveOrder,
                            SquadGuid = GamePlayState.squadIdForStartDrag,
                            Path = GamePlayState.supplyLines.shortestPath(startNodeIdOfPath,endNodeIdOfPath),
                            StartPosition = startPositionOfUnit,
                            EndPosition = endPositionOfUnit
                        };

                        // now that we've issued an order, clear out the 
                        // field that identifies the unit that the order has been issued to. 
                        GamePlayState.squadIdForStartDrag = Guid.Empty;
                        Orders.AddOrder(movementOrder);
                    }
                }
            }
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

        foreach(InputEvent inEvent in events)
        {
            if(inEvent is CommandEvent)
            {
                CommandEvent commandEvent = (CommandEvent)inEvent;
                this.CurrentGameMode = commandEvent.Command;
                //DelegateInputEvent(inEvent.worldPosition);
                eventsToRemove.Add (inEvent);
            }
            else
            {
                // theres been a general mouseclick.  Take action on it, depending on our 
                // current state. 
                InputEvent inputEvent = (InputEvent)inEvent;
                DelegateInputEvent(inputEvent);
                eventsToRemove.Add(inputEvent);
            }
        }

        EventQueue.RemoveEvents(eventsToRemove);
    }

    public void ActionModeButtonPressed(int param)
    {
        Debug.Log("get this message logged");

        // clear out any existing input state. 
        this.CurrentInputState = InputState.BlankState;
        if(this.IntermediateEdge != null)
        {
            this.IntermediateEdge.gameObject.SetActive(false);
        }

        List<Order> orders = Orders.ReadAndClearOrders();
        orders.ForEach( order =>
        {
            GamePlayState.supplyLines.Requisition(
                order.Path.First(),
                order.Path[order.Path.Count-1],
                order.SquadGuid);
        });

    }

    public static bool IsBuildingPotentialDestinationForUnit(List<int> doorEntryPointIds)
    {
        // intersect the node ids of all connected buildings with the node id's of the entry points. 
        // if the count is greater than zero, then return true. 
        List<int> connections = GamePlayState.buildingsConnectedToStartDragBuilding.Intersect( doorEntryPointIds ).ToList();
        if(connections.Count() > 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public static SupplyNetwork GetSupplyLines()
    {
        return GamePlayState.supplyLines;
    }

    public static Squad GetSquadById(Guid id)
    {
        return squadIdToSquadInfo[id];
    }

    public Guid GetSquadStartDrag()
    {
        return GamePlayState.squadIdForStartDrag;
    }

    public void OnUnitArrived(GamePlayEvent eventInfo)
    {       
        // new code
        IHousable arrivalHousing = this.Housables.Single( housing =>
                housing.ContainsNode(eventInfo.nodeId) );

        arrivalHousing.HandleUnitArrived(eventInfo);

        Debug.Log("unit arrived!!!!");
    }

    public void OnUnitDeparted(GamePlayEvent eventInfo)
    {
        // refactored code
        IHousable departureHousing = this.Housables.Single( housing =>
                housing.ContainsNode(eventInfo.nodeId) );

        departureHousing.HandleUnitDeparted(eventInfo);

        Debug.Log("unit departed!!!!");
    }
}
