using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class TopDownCamera : MonoBehaviour 
{
    private int leftPaneWidth = 100;
    private bool unitPaneShown = false;
    private List<InputEvent> allEvents;

    // Use this for initialization
    void Start () 
    {
        this.allEvents = new List<InputEvent>();
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
        if( Input.GetMouseButtonDown(0) 

                && 

                // we don't process clicks that happen in the left pane (where our buttons are). 
            Input.mousePosition.x > this.leftPaneWidth)
        {
            //Debug.Log(string.Format("clickin on screen coordinates -- x:{0},y:{1},arctan(x/y):{2}", Input.mousePosition.x,Input.mousePosition.y,System.Math.Atan(Input.mousePosition.x/Input.mousePosition.y) * (180f/Mathf.PI)));
            RaycastHit hit;

            Vector3 worldCoordsOfClick = camera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, camera.nearClipPlane));

            Debug.Log(string.Format ("worldCoordsOf Click: [{0},{1},{2}]",worldCoordsOfClick.x, worldCoordsOfClick.y, worldCoordsOfClick.z));
            Debug.Log (string.Format("worldCoordsOf Camer: [{0},{1},{2}]",transform.position.x,transform.position.y,transform.position.z));

            /// please NOTE:  it's uncertain if this is the correct place to be raycasting. 
            /// don't assume too much as it's possible that this could change. 
            if (Physics.Raycast(worldCoordsOfClick, (worldCoordsOfClick - camera.transform.position), out hit, 100f))
            {
                // plan:  we will store the guid of the clicked on squad as a static variable to facilitate cross class communication
                // the gameplay state contains a list of all buildings via LevelVo, and it should be able to figure out which building 
                // houses the squad.  
                //
                // Gameplaystate will then store another static list of the IDs of all building that are connected to that building. 
                // in each building update stage, they will render themselves as a possible target after checking the static connected building ID list. 
                Debug.Log("Clicked on" + hit.collider.gameObject.name);
                Squad clickedOnSquadSlot = UnitsInBuilding.getSquadForClick(hit.collider.gameObject);

                MeshRenderer mesh = hit.collider.gameObject.GetComponent<MeshRenderer>() as MeshRenderer;
                if(mesh.material.color == Color.white)
                {
                    mesh.material.color = Color.cyan;
                }
                else if(mesh.material.color == Color.cyan)
                {
                    mesh.material.color = Color.red;
                }
                else
                {
                    mesh.material.color = Color.white;
                }

                if(clickedOnSquadSlot != null)
                {
                    GamePlayState.squadIdForStartDrag = clickedOnSquadSlot.id;
                    Debug.Log("clicked on squad id " + clickedOnSquadSlot.id);
                    EventQueue.AddToEventQueue(new InputEvent(worldCoordsOfClick, transform.forward, InputEvent.EventType.ClicksOnSquad));
                    return;
                }
            }

            EventQueue.AddToEventQueue(new InputEvent(worldCoordsOfClick, transform.forward));
            this.allEvents.Add (new InputEvent(worldCoordsOfClick, transform.forward));
        }
        else if(Input.GetMouseButtonUp(0))
        {
            Vector3 worldCoordsOfClick = camera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, camera.nearClipPlane));
            EventQueue.AddToEventQueue(new InputEvent(worldCoordsOfClick, transform.forward, InputEvent.EventType.ReleasesMouseDown, camera.transform.position));
        }

        // draw debug rays for each click. 
        this.allEvents.ForEach( e => Debug.DrawRay(e.worldPosition, (e.worldPosition - camera.transform.position) * 100.0f, Color.white));
        this.allEvents.ForEach( e => Debug.DrawLine(e.worldPosition, e.worldPosition + new Vector3(0.1f, 0.1f, 0.0f), Color.red) );
    }

    /// <summary>
    /// Handles the GUI event.
    /// </summary>
    void OnGUI()
    {
        //GUILayout.BeginArea(new Rect(0,0,this.leftPaneWidth,Screen.width));
        GUILayout.BeginVertical("box");
        if(GUILayout.Button ("Go!",new GUILayoutOption[]{GUILayout.Width(100), GUILayout.Height(30)}))   
        {
            /// when the "go!" button is pressed, all orders that have been added to our queue are issued. 
            EventQueue.AddToEventQueue(new CommandEvent(GamePlayState.GameMode.ActionMode));
        }
        if(GUILayout.Button("Waypoint", new GUILayoutOption[]{GUILayout.Width(100), GUILayout.Height(30)}))
        {
            // enters our game into a state where the user can define supply lines. 
            EventQueue.AddToEventQueue(new InputEvent(InputEvent.EventType.WaypointButtonPressed));
        }
        if(GUILayout.Button ("Requisition",new GUILayoutOption[]{GUILayout.Width(100), GUILayout.Height(30)}))   
        {
            // enters a game state where the users can issue movement orders to units. 
            //
            this.unitPaneShown = !this.unitPaneShown;

            // NOTE: we are removing this for now, implement the 'ActionMode' state, which 
            // issues all orders to units instead.  
            //EventQueue.AddToEventQueue(new CommandEvent(GamePlayState.GameMode.OrderUnitMovementMode));
        }
        GUILayout.EndVertical();
        //GUILayout.EndArea();
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
