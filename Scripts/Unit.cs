using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Unit : MonoBehaviour 
{
    private Vector2 destinationPosition;
    private float speed = 0.001f;
    Vector2 directionToDestination;

    /// <summary>
    /// The path to follow, assuming that the unit is travelling along supply lines in 
    /// our supply network, they'll travel along the network nodes identified by these integers. 
    /// </summary>
    List<Vector2> pathToFollow;

    /// <summary>
    /// The current index on path.
    /// </summary>
    int currentIndexOnPath;

    // Use this for initialization
    void Start () 
    {
        destinationPosition = new Vector2(0f,0f);

        currentIndexOnPath = 0;
    }

    // Update is called once per frame
    void Update () 
    { 
        if(this.pathToFollow != null && this.pathToFollow.Count > 0)
        {
            // Check and see if we are close enough to our next node. 
            // we have to treat the last leg of the journey as a special case, so check 
            // for that first. 
            if(currentIndexOnPath < this.pathToFollow.Count - 1)
            {
                // we aren't on the last leg.  This is the normal case.
                // now, check to see if we are 'close enough' to intermediate node that 
                // we are travelling to
                float distanceToNextNode = Vector2.Distance(
                        new Vector2(this.transform.position.x,this.transform.position.y),
                        this.pathToFollow[currentIndexOnPath]);

                // if we are sufficiently close start moving to the next node. 
                if( distanceToNextNode < 0.01f )
                {
                    currentIndexOnPath = currentIndexOnPath + 1;
                }
            }

            destinationPosition = this.pathToFollow[currentIndexOnPath];

            directionToDestination = new Vector2(
                    destinationPosition.x - this.transform.position.x,
                    destinationPosition.y - this.transform.position.y);

            directionToDestination.Normalize();

            // ok, now we actually move our unit along the path. 
            this.transform.position = new Vector3(
                    this.transform.position.x + this.directionToDestination.x * speed,
                    this.transform.position.y + this.directionToDestination.y * speed,
                    this.transform.position.z);
        }

    }

    public void SetPath(List<Vector2> path)
    {
        this.pathToFollow = path;
    }
}
