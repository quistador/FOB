using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Building : MonoBehaviour 
{
    public Material material;

    /// <summary>
    /// Probably not-traditional and potentially confusing, but here's the deal with 'isInitialized'. 
    /// Start() is typically called at a point not entirely in our control. 
    /// problem:  
    /// 1) level.cs instantiates a building. 
    /// 2) somewhere later in level.cs, level.cs uses some data from the instantiated building. 
    /// 3) however, building.Start hasn't been called yet.  (it's not called at time of instantiation, it's only 
    ///    called sometime before the first building.update() function call. 
    /// 4) unexpected behavior is therefore encountered. 
    /// 
    /// my solution is to call start() explicitly, however, this means it might be called twice.  We use the 'isInitialized'
    /// variable to prevent double instantiation of member variables. 
    /// </summary> 
    private bool isInitialized = false;

    // Use this for initialization
    public void Start () 
    {
        if(isInitialized == false)
        {
            //PlaneMeshTools.CreatePlane(10f, 10f, 1, 1, material, this.gameObject);
            PlaneMeshTools.CreatePlane(
                    buildingWidth,buildingWidth,
                    2,2,
                    this.material,
                    this.gameObject);

            this.gameObject.AddComponent<MeshCollider>();
            MeshCollider collider = this.gameObject.GetComponent<MeshCollider>();
            MeshFilter mesh = this.gameObject.GetComponent<MeshFilter>();
            collider.sharedMesh = mesh.sharedMesh;
            this._entryPointPositions = new List<Vector3>();

            Object doorResource = Resources.Load(@"Door");
            Vector3 doorPosition = new Vector3(
                    this.transform.position.x + mesh.sharedMesh.bounds.center.x,
                    this.transform.position.y,
                    -0.01f);

            this._entryPointPositions.Add(doorPosition);

            GameObject doorObject = Object.Instantiate(
                    doorResource, 
                    doorPosition, 
                    Quaternion.identity) as GameObject;

            Door door = doorObject.GetComponent(typeof(SupplyEdge)) as Door;
            isInitialized = true;

            Transform cube = this.transform.FindChild("cube");
            cube.transform.localScale = new Vector3(
                    buildingWidth,
                    buildingWidth,
                    cube.transform.localScale.z
                    );
            cube.transform.position = new Vector3(
                    cube.transform.position.x + buildingWidth/2,
                    cube.transform.position.y + buildingWidth/2,
                    cube.transform.position.z
                    );
        }
    }

    // Update is called once per frame
    void Update () 
    {
        // check for input events
        List<InputEvent> events = EventQueue.GetEventQueue();
        if(events == null)
        { 
            // if events isn't set, then there's simply nothing to do here. 
            // (no mouseclicks or other things yet)
            return;
        }
        if(events.Count > 0)
        {
            if(Physics.Raycast(events[0].mouseClickForEvent))
            {
                //Debug.Log(string.Format("{0} was hit, setting color to magenta",this.gameObject.name));
                //this.material.color = Color.white;
            }
        }

        // check for any events that indicate that a unit arrived in this building. 
        List<GamePlayEvent> unitArrivedInThisBuilding = GamePlayEvents.Events.Where( 
                gameEvent => gameEvent.eventKind == GamePlayEvent.EventKind.UnitArrived )
            .ToList()
            .Where(
                    unitArrivedEvent => this.nodeIdsForEntryPoints.Contains(unitArrivedEvent.nodeId))
            .ToList();

		// if a unit has arrived in the building, remove the 'unitarrived' event from our
		// event list and do something. 
        if(unitArrivedInThisBuilding.Count > 0)
        {
        	GamePlayEvents.Events.Remove(unitArrivedInThisBuilding.First());
            Debug.Log("unit arrived in building " + this.GetInstanceID() );
        }
    }

    /// <summary>
    /// Sets the level adjusted entry point position.
    /// </summary>
    /// <value>
    /// The level adjusted entry point position.
    /// </value>
    public List<Vector3> LevelAdjustedEntryPointPosition()
    {
        // problem:  if we use the door position as the entry point position (which initially seems like the 
        // logical thing to do), then our 'supplyEdgeBeingPlaced' will intersect with the building (since it starts
        // at the entry point positions that we return from this function).  If it intersects, then the edge will 
        // be marked as 'invalid', because we don't want to allow edges that intersect with buildings. So, how do we solve?
        // we'll start with a naive approach, where we move the entry point outward from the bounds-center of the building, 
        // along the vector that travels from the bounds center to the door position:  
        //
        //                               o  <---(adjusted door position)
        //                             /
        //                           /
        // |---------------------|-x-|-|    <---(original door position)
        // |                     /     |
        // |                   /       |
        // |                 /         |
        // |               /           |
        // |             x             |
        // |        (bounds center)    |
        // |                           |
        // |                           |
        // |                           |
        // |___________________________|
        //
        // this *will* look weird for certain door positions and this shouldn't be a permanent solution. 
        MeshCollider collider = this.gameObject.GetComponent<MeshCollider>();
        Vector3 buildingCenter = collider.bounds.center;

        List<Vector3> adjustedDoors = this.EntryPointPositions.Select(door =>
                {
                    // get the vector from the center to the door. 
                    Vector3 centerToDoor = buildingCenter - door;
                    centerToDoor.Normalize();
                    centerToDoor = centerToDoor * 0.01f;
                    return door - centerToDoor;
                }).ToList();

        return adjustedDoors; 
    }

    /// <summary>
    /// The node identifiers for entry points.  In order to link our buildings into the supplyNetwork, 
    /// we need to be able to reference node Ids in the network that 
    /// </summary>
    public List<int> nodeIdsForEntryPoints{ get; set; }

    public Bounds AxisAlignedBoundingBox()
    {
        MeshFilter mesh = this.gameObject.GetComponent<MeshFilter>();
        return mesh.sharedMesh.bounds;
    }

    public static float buildingWidth = 0.6f;

    private List<Vector3> _entryPointPositions;
    public List<Vector3> EntryPointPositions
    {
        get
        {
            return this._entryPointPositions;
        }
    }
}
