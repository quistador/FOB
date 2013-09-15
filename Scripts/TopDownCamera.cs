using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TopDownCamera : MonoBehaviour 
{

    // Use this for initialization
    void Start () 
    {
    }

    // Update is called once per frame
    void Update () 
    {

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        this.transform.Translate(new Vector3(
                    horizontal * Time.deltaTime,
                    vertical * Time.deltaTime,
                    0 ) );

        // check for left click.
        if(Input.GetMouseButtonDown(0))
        {
            //Debug.Log(string.Format("clickin on screen coordinates -- x:{0},y:{1},arctan(x/y):{2}", Input.mousePosition.x,Input.mousePosition.y,System.Math.Atan(Input.mousePosition.x/Input.mousePosition.y) * (180f/Mathf.PI)));
            RaycastHit hit;

            // I'm not sure what's going on with the z coordinate here:  when it's negative, things seem to work out the way that I want it to
            // though I have no idea why.  It's also necessary to reset the z-coord to the camera transform position. 
            Vector3 worldCoordsOfClick = camera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -transform.position.z));
            worldCoordsOfClick.z = transform.position.z;

            //Debug.Log(string.Format ("worldCoordsOf Click: [{0},{1},{2}]",worldCoordsOfClick.x, worldCoordsOfClick.y, worldCoordsOfClick.z));
            //Debug.Log (string.Format("worldCoordsOf Camer: [{0},{1},{2}]",transform.position.x,transform.position.y,transform.position.z));
            if (Physics.Raycast(worldCoordsOfClick, transform.forward, out hit, 10f))
            {
                //Debug.Log("Clicked on" + hit.collider.gameObject.name);
                MeshRenderer mesh = hit.collider.gameObject.GetComponent<MeshRenderer>() as MeshRenderer;
                if(mesh.material.color == Color.yellow)
                {
                    mesh.material.color = Color.cyan;
                }
                else
                {
                    mesh.material.color = Color.yellow;
                }
            }

            EventQueue.AddToEventQueue(new InputEvent(worldCoordsOfClick, transform.forward));
        }

        // draw debug rays for each click. 
        EventQueue.GetEventQueue().ForEach( e => Debug.DrawRay(e.worldPosition, e.forward * 100.0f, Color.white));
    }

    /// <summary>
    /// Handles the GUI event.
    /// </summary>
    void OnGUI()
    {
        if(GUILayout.Button("Waypoint", new GUILayoutOption[]{GUILayout.Width(100), GUILayout.Height(30)}))
        {
            EventQueue.AddToEventQueue(new CommandEvent(GamePlayState.CommandState.DefineSupplyLinesStart));
        }
        if(GUILayout.Button ("Requisition",new GUILayoutOption[]{GUILayout.Width(100), GUILayout.Height(30)}))   
        {
            EventQueue.AddToEventQueue(new CommandEvent(GamePlayState.CommandState.RequisitionSoldiers));
        }
    }

    /// <summary>
    /// Screens to world point.
    /// </summary>
    /// <returns>
    /// The to world point.
    /// </returns>
    /// <param name='screenPoint'>
    /// Screen point.
    /// </param>
    /// <exception cref='System.ArgumentException'>
    /// Is thrown when the argument exception.
    /// </exception>
    public static Vector3 ScreenToWorldPoint(Vector2 screenPoint)
    {
        if(Camera.allCameras.Length != 1)
        {
            throw new System.ArgumentException("we only expected one active camera");
        }

        Camera activeCamera = Camera.allCameras[0];

        // still not sure what's happening with the z coord here, why it needs to be negative. 
        // however, this does appear to give me points that are consistent with other points in world
        // space.  If I don't negate it, then points appear 'flipped' about the x and y axis. 
        Vector3 screenPointWithZ = new Vector3(screenPoint.x,screenPoint.y,-activeCamera.transform.position.z);
        Vector3 untranslatedWorldPoint = activeCamera.ScreenToWorldPoint(screenPointWithZ);

        return untranslatedWorldPoint;
    }
}