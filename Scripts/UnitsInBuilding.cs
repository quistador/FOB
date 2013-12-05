using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class UnitsInBuilding : MonoBehaviour 
{
    private SortedList<Guid, DateTime> squadsInBuildingSet;

    private UnityEngine.Object textSlotResource;
    // Use this for initialization
    void Start () 
    {
        squadsInBuildingSet = new SortedList<Guid, DateTime>();

        this.textSlotResource = Resources.Load(@"BuildingSlotDescriptor");

        BoxCollider[] colliders = this.GetComponentsInChildren<BoxCollider>() as BoxCollider[];
        this.squads = new List<Squad>();

        foreach(BoxCollider collider in colliders)
        {
            Debug.DrawLine(
                collider.bounds.min,
                collider.bounds.max);
        }
    }

    // Update is called once per frame
    void Update () 
    {
        // start by comparing the squads in our SortedSet with those that have just entered the building. 
        foreach(Squad squad in this.squads)
        {
            if( !this.squadsInBuildingSet.ContainsKey(squad.id))
            {
                // first, add it to our hash set. 
                this.squadsInBuildingSet[squad.id] = DateTime.Now;
                Debug.Log(String.Format("added new squad {0} to building", squad.id));

                // if this squad isn't yet in our hash set, then we need to make a new text slot for it. 
                Vector3 textSlotPosition = this.transform.position;
                textSlotPosition.z = -0.2f;
                textSlotPosition.y = textSlotPosition.y - (this.squadsInBuildingSet.Count * 0.05f);

                GameObject textSlotObject = UnityEngine.Object.Instantiate(
                        this.textSlotResource, 
                        textSlotPosition, 
                        Quaternion.identity) as GameObject;

                TextSlot slot = textSlotObject.GetComponent<TextSlot>() as TextSlot;
                slot.squadForSlot = squad;

                textSlotObject.transform.parent = this.transform;
            }
        };

        // check to see if a unit has left since our last update. 
        if( this.squads.Count() < this.squadsInBuildingSet.Count() )
        {
            // if someone has left, then we've got to figure out what the squad id of the 
            // departed unit is.  Destroy the textSlot of the departed unit. 
            Debug.Log("squad has left the building, we'll need to remove it");
        }

        BoxCollider[] colliders = this.GetComponentsInChildren<BoxCollider>() as BoxCollider[];

        foreach(BoxCollider collider in colliders)
        {
            Debug.DrawLine(
                    collider.bounds.min,
                    collider.bounds.max);
        }
    }

    private List<Squad> _squads;
    public List<Squad> squads 
    { 
        get { return _squads; }
        set 
        { 
            _squads = value; 
        } 
    }

    /// <summary>
    /// when the user clicks on something, this function will return:
    /// 1) null, if the user didn't click on a squad text slot. 
    /// 2) the Squad text slot object that was clicked on.  
    /// </summary>
    /// <returns>The squad for click.</returns>
    public static Squad getSquadForClick(GameObject clickedOnObject)
    {
        TextSlot slotForClickedObject = clickedOnObject.GetComponent<TextSlot>() as TextSlot;

        if(slotForClickedObject == null)
        {
            return null;
        }

        return slotForClickedObject.squadForSlot;
    }
}
