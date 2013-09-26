using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
		}
    }

    // Update is called once per frame
    void Update () 
    {
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
    }

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
