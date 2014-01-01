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

            // we need to identify all the nodes to remove from 'this.squadsInBuildingSet'. Let's use a complicated, O(n^2)
            // method to do this, since I can't think of a better method right now. 
            List<Guid> squadIdsToRemove = this.squadsInBuildingSet.Keys.Where( squadGuid => 
            {
                Squad foundSquad = this.squads.Find( squad => squad.id == squadGuid );
                if(foundSquad != null)
                {
                    return false;
                }
                return true;
            }).ToList();

            squadIdsToRemove.ForEach( squad => this.squadsInBuildingSet.Remove(squad) );

            foreach(TextSlot slot in this.TextSlots)
            {
                if(squadIdsToRemove.Contains(slot.squadForSlot.id))
                {
                    slot.gameObject.SetActive(false);
                }
            }
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

    /// <summary>
    /// event handler, called when an order is added for one of the contained units.
    /// </summary>
    /// <param name="order">Order.</param>
    public void OnOrderAdded(Order order)
    {
        // check to see if the targeted move squad is in this building. 
        if(
            // make sure this building contains the clicked-on/moving squad
            this.squadsInBuildingSet.ContainsKey(order.SquadGuid)

            &&

            // make sure the path is valid
            order.Path != null

            &&

            // make sure the path is valid
            order.Path.Count > 0 )
        {
            int startNode = order.Path.First();
            int endNode = order.Path[order.Path.Count-1];

            Vector3 startArrowPosition = this.GetPositionOfSquad(order.SquadGuid);
            Vector3 endArrowPosition = order.EndPosition;

            float theta = SupplyEdge.calculateRotationForEdge(startArrowPosition,endArrowPosition);

            // lerp between the text slot position of the moved squad and the building position that the 
            // unit will be moving to. 
            Vector3 orderPreviewPosition = Vector3.Lerp( 
                    startArrowPosition,
                    endArrowPosition,
                    0.5f);

            // reset the z position:  we want the order preview to appear just above the 
            // building roof. 
            orderPreviewPosition.z = this.GetPositionOfSquad(order.SquadGuid).z;

            Squad movingSquad = GamePlayState.GetSquadById(order.SquadGuid);

            UnityEngine.Object edgeResource = Resources.Load(@"OrderPreviewObject");
            GameObject previewObject = UnityEngine.Object.Instantiate(edgeResource, orderPreviewPosition, Quaternion.identity) as GameObject;

            previewObject.transform.RotateAround(
                orderPreviewPosition,
                new Vector3(0,0,1),
                theta );

            float xScale = (startArrowPosition - endArrowPosition).magnitude;

            // scale the arrow along the x axis to stretch it from it's start to destination. 
            previewObject.transform.localScale = new Vector3(
                previewObject.transform.localScale.x,
                previewObject.transform.localScale.y,
                previewObject.transform.localScale.z );

            OrderPreview preview = previewObject.GetComponent(typeof(OrderPreview)) as OrderPreview;
            MeshRenderer previewArrowMesh = previewObject.GetComponentsInChildren(typeof(MeshRenderer)).Single() as MeshRenderer;

 
            Debug.Log (String.Format("preview length {3} creating a waypoint from building {0} to building {1} for squad {2}",startNode, endNode, movingSquad.squadTypeDisplayName, previewArrowMesh.bounds.extents.magnitude));
        }
    }

    /// <summary>
    /// Gets the position of squad that's in this unit list. 
    /// </summary>
    /// <returns>The position of squad.</returns>
    /// <param name="squadId">Squad identifier.</param>
    public Vector3 GetPositionOfSquad(Guid squadId)
    {
        // get our child textslot objects.  Iterate through each one to see 
        // which one matches, and return the position of it. 
        TextSlot slotForMatchingSquad = this.TextSlots.Single( slot  => slot.squadForSlot.id == squadId );
        return slotForMatchingSquad.transform.position;
    }

    public TextSlot[] TextSlots
    {
        get
        {
            TextSlot[] slots = this.GetComponentsInChildren<TextSlot>() as TextSlot[];
            return slots;
        }
    }

    public int GetUnitsCount
    {
        get
        {
            return this.TextSlots.Length;
        }
    }
}
