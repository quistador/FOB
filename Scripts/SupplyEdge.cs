using UnityEngine;
using System.Collections;

public class SupplyEdge : MonoBehaviour 
{

    public Material material;

    // Use this for initialization
    void Start () 
    {
    	this.startPos = Vector3.zero;
    }

    /// <summary>
    /// Initialize this instance. 
    /// </summary>
    public void Initialize(Vector3 startPos, Vector3 endPos)
    {
    	//Debug.Log(string.Format("setting startPos for new edge: {0}", startPos));
    	//this.transform.position = startPos;
    	this.startPos = startPos;
		this.endPos = endPos;
        Mesh plane = PlaneMeshTools.CreatePlane(
            1.0f,0.01f,
            2,2,
            this.material,
            this.gameObject);

        this.transform.Translate(new Vector3(0f,0f,-0.03f));
        float x = endPos.x - startPos.x;
        float y = endPos.y - startPos.y;
        float theta = -((float)System.Math.Atan(x/y) * (180f / Mathf.PI) - 90f);

        float lengthFromOriginToEndpoint = Vector3.Distance(startPos,endPos);

        // inverse tangent range is from -pi/2 to pi/2, which doesn't cover the full
        // 360 degrees of rotation that we need. 
        if(endPos.y < startPos.y)
        {
            theta = theta + 180f;
        }

        MeshFilter mesh = this.gameObject.GetComponent<MeshFilter>();
        //this.transform.localPosition = new Vector3(0f,-0.05f,0f);
        this.transform.RotateAround(
                new Vector3(0f,0.0f,0f),
                new Vector3(0,0,1),
                theta
                );       
                
        mesh.transform.localScale = new Vector3(lengthFromOriginToEndpoint, mesh.transform.localScale.y, mesh.transform.localScale.z);
        this.transform.position = new Vector3(startPos.x,startPos.y, -0.03f);
    }

    // Update is called once per frame
    void Update () 
    {
    }
    
    private Vector3 _startPos;
	public Vector3 startPos 
	{ 
		get
		{
			if( float.IsNaN(this._startPos.x) || 
				float.IsNaN(this._startPos.y) || 
				float.IsNaN(this._startPos.z) )
			{
				this._startPos = Vector3.zero;
			}
			return this._startPos;
		}
		set
		{
			this._startPos = value;
		}
	}
	public Vector3 endPos { get; set; }
}
