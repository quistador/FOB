using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Unit : MonoBehaviour 
{
    private Vector2 destinationPosition;
    private float speed = 0.01f;
    Vector2 directionToDestination;
    Guid identifier;

    /// <summary>
    /// The path to follow, assuming that the unit is travelling along supply lines in 
    /// our supply network, they'll travel along the network nodes identified by these integers. 
    /// </summary>
    List<SupplyNetwork.SupplyNode> pathToFollow;

    /// <summary>
    /// The current index on path.
    /// </summary>
    int currentIndexOnPath;

    // Use this for initialization
    void Start () 
    {
        destinationPosition = new Vector2(0f,0f);
        currentIndexOnPath = 0;
        this.identifier = Guid.NewGuid();
    }

    // Update is called once per frame
    void Update () 
    { 
        // update the positions of this unit, if it's moving along a supply line. 
        this.MoveUnitAlongSupplyLine();
    }
    
    private void MoveUnitAlongSupplyLine()
    {
        float threshold = 0.01f;

        // if we don't have a path to follow, then
        // there aren't any steps to take here. 
        if(this.pathToFollow != null && this.pathToFollow.Count > 0)
        {
            float distanceToNextNode = Vector2.Distance(
                    new Vector2(this.transform.position.x,this.transform.position.y),
                    this.pathToFollow[currentIndexOnPath].Position);

            // Check and see if we are close enough to our next node. 
            // we have to treat the last leg of the journey as a special case, so check 
            // for that first. 
            if(currentIndexOnPath < this.pathToFollow.Count - 1)
            {
                // we aren't on the last leg.  This is the normal case.
                // now, check to see if we are 'close enough' to intermediate node that 
                // we are travelling to. if we are sufficiently close start moving to the next node.
                if( distanceToNextNode < threshold )
                {
                    currentIndexOnPath = currentIndexOnPath + 1;
                }
            }
            else
            {
                // we are at the last leg!  now, when we're really close 
                // to the nextNode, add an event to the queue so that our building 
                // knows that we're here. 
                // if we are sufficiently close start moving to the next node. 
                if( distanceToNextNode < threshold )
                {
                    // return here.  We don't want the node to continue updating it's position. 
                    GamePlayEvents.Events.Add( 
                        new GamePlayEvent()
                        {
                            nodeId = this.pathToFollow[currentIndexOnPath].NodeId,
                         eventKind = GamePlayEvent.EventKind.UnitArrived
                        }
                    );
                    this.SetPath(new List<SupplyNetwork.SupplyNode>());
                    this.gameObject.SetActive(false);
                    return;
                }
            }

            destinationPosition = this.pathToFollow[currentIndexOnPath].Position;

            directionToDestination = new Vector2(
                    destinationPosition.x - this.transform.position.x,
                    destinationPosition.y - this.transform.position.y);

            directionToDestination.Normalize();
            Vector3 newPosition = new Vector3(
                    this.transform.position.x + this.directionToDestination.x * speed,
                    this.transform.position.y + this.directionToDestination.y * speed,
                    this.transform.position.z);

            //Debug.Log(System.String.Format("travelling distance: {0}", Vector3.Distance(this.transform.position, newPosition)));

            // ok, now we actually move our unit along the path. 
            this.transform.position = newPosition;
        }
        
        return;
    }

    public void SetPath(List<SupplyNetwork.SupplyNode> path)
    {
        this.pathToFollow = path;
    }
}
