using UnityEngine;
using System;
using System.Collections;

/// <summary>
/// Supply edge being placed:  it's the supply edge that is rendered 
/// when the user is defining supply lines. 
/// </summary>
public class SupplyEdgeBeingPlaced : SupplyEdge
{

    // Use this for initialization
    void Start () 
    {
        PlaneMeshTools.CreatePlane(
                1.0f,0.01f,
                2,2,
                this.material,
                this.gameObject);

        this.transform.position = Vector3.zero;
        this.transform.Translate(new Vector3(0f,0f,-0.03f));
        this._networkReference = new SupplyNetwork(Vector3.zero);
    }

    // Update is called once per frame
    void Update () 
    {
	    try
		{
	        this.transform.rotation = UnityEngine.Quaternion.identity;

	        //each update, we need to:
	        // 1) figure out the startNode for this edge. 
	        // 2) figure out the mouse hover position (which we use as the endNode). 
	        // 3) figure out if the edge is in a valid position (not intersecting with buildings, etc)
	        // 4) render the edge appropriately. 
	        Vector2 mousePosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
	
	        Vector3 endPos = TopDownCamera.ScreenToWorldPoint(mousePosition);
	
	        // since we're in 2d land, we don't need the z coord.  Additionally, ScreenToWorldPoint
	        // might have set this to some non-zero value, which will screw up our distance calculations later. 
	        endPos.z = 0;
	
	        Vector3 startPos = this.startPos;
	        int nearestNodeId = this._networkReference.nearestNeighborNode(endPos);
	        startPos = this._networkReference.NodeForId(nearestNodeId).Position;
	        
	        this.transform.position = new Vector3(startPos.x,startPos.y, -0.03f);
	        
	        float x = endPos.x - startPos.x;
	        float y = endPos.y - startPos.y;
	        float theta = -((float)System.Math.Atan(x/y) * (180f / Mathf.PI) - 90f);
	        
			// in the event that endPos == startPos, we'll get NaN values for theta. 
	        if(float.IsNaN(theta))
			{
				theta = 0.0f;
			}
	
	        float lengthFromOriginToEndpoint = Vector3.Distance(startPos,endPos);
	
	        // inverse tangent range is from -pi/2 to pi/2, which doesn't cover the full
	        // 360 degrees of rotation that we need. 
	        if(endPos.y < startPos.y)
	        {
	            theta = theta + 180f;
	        }
	
	        MeshFilter mesh = this.gameObject.GetComponent<MeshFilter>();
	        //this.transform.localPosition = new Vector3(0f,-0.05f,0f);
	        this.transform.rotation = Quaternion.identity;
	        this.transform.RotateAround(
	                startPos,
	                new Vector3(0,0,1),
	                theta
	                );       
	                
	        mesh.transform.localScale = new Vector3(
				lengthFromOriginToEndpoint, 
				mesh.transform.localScale.y, 
				mesh.transform.localScale.z);
		}
		catch(Exception)
		{
			Debug.Log ("Exception, fire up the debugger");
			throw;
		}
    }
    
    private SupplyNetwork _networkReference;
	public SupplyNetwork networkReference
	{
		set{ this._networkReference = value; }
	}
}
