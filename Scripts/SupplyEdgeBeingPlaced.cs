using UnityEngine;
using System;
using System.Collections;

/// <summary>
/// Supply edge being placed:  it's the supply edge that is rendered 
/// when the user is defining supply lines. 
/// </summary>
public class SupplyEdgeBeingPlaced : SupplyEdge
{

    private SupplyNetwork _networkReference;
    private bool _isValid;

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
        this._networkReference = GamePlayState.GetSupplyLines();
        this.gameObject.AddComponent<BoxCollider>();
        BoxCollider collider = this.gameObject.GetComponent<BoxCollider >();

        collider.enabled = true;
        collider.isTrigger = true;

        collider.size = new Vector3(
                collider.bounds.extents.x,
                collider.bounds.extents.y,
                10f);

        MeshFilter mesh = this.gameObject.GetComponent<MeshFilter>();

        this.gameObject.AddComponent<Rigidbody>();
        Rigidbody rigidBody = this.gameObject.GetComponent<Rigidbody>();
        rigidBody.useGravity = false;

        this._isValid = true;
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

            float theta = SupplyEdge.calculateRotationForEdge(startPos,endPos);
            float lengthFromOriginToEndpoint = SupplyEdge.calculateScaleForEdge(startPos,endPos);

            MeshFilter mesh = this.gameObject.GetComponent<MeshFilter>();
            //this.transform.localPosition = new Vector3(0f,-0.05f,0f);
            this.transform.RotateAround(
                    startPos,
                    new Vector3(0,0,1),
                    theta
                    );       

            mesh.transform.localScale = new Vector3(
                    lengthFromOriginToEndpoint, 
                    mesh.transform.localScale.y, 
                    mesh.transform.localScale.z);

            MeshRenderer meshRenderer = this.gameObject.GetComponent<MeshRenderer>();

            this.transform.position = new Vector3(startPos.x,startPos.y, -0.03f);
        }
        catch(Exception)
        {
            Debug.Log ("Exception, fire up the debugger");
            throw;
        }
    }

    public bool isValid
    {
        get { return this._isValid; }
    }

    void OnTriggerEnter(Collider test)
    {
        MeshRenderer meshRenderer = this.gameObject.GetComponent<MeshRenderer>();
        meshRenderer.material.color = Color.black;


        // since other collidable objects exist within the building, we've got to only
        // update our 'isValid' reference when we collide enter/exit with buildings. 
        //
        // NOTE: something is wrong with the below code, the spirit is correct but
        // there's a bug in it. 
        if(!test.gameObject.name.Contains("BuildingSlotDescriptor"));
        {
            Debug.Log("trigger enter  " + test.gameObject.name);
            this._isValid = false;
        }
    }

    void OnTriggerExit(Collider test)
    {
        MeshRenderer meshRenderer = this.gameObject.GetComponent<MeshRenderer>();
        meshRenderer.material.color = Color.white;

        // since other collidable objects exist within the building, we've got to only
        // update our 'isValid' reference when we collide enter/exit with buildings. 
        //
        // NOTE: something is wrong with the below code, the spirit is correct but
        // there's a bug in it. 
        if(!test.gameObject.name.Contains("BuildingSlotDescriptor"));
        {
            Debug.Log("trigger exit  " + test.gameObject.name);
            this._isValid = true;
        }
    }
}
