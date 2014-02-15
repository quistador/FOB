using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace TacticsGame
{
    public class SupplyNode : MonoBehaviour, IHousable
    {
        private UnitListControl SquadsInNodeChildObject;
        private List<Squad> SquadsInNode;

        // Use this for initialization
        void Start () 
        {
            SquadsInNode = new List<Squad>();

            /// we'll be referencing this child component regularly, so store a local copy. 
            this.SquadsInNodeChildObject = this.gameObject.GetComponentInChildren<UnitListControl>() as UnitListControl;
            Orders.OrderAdded += this.SquadsInNodeChildObject.OnOrderAdded;
        }

        // Update is called once per frame
        void Update () 
        {
            // our child object is responsible for visually depicting the units, update that field so that 
            // it knows that updates have occurred. 
            this.SquadsInNodeChildObject.squads = this.SquadsInNode;
        }

        public static bool IsRaycastHittingSupplyNode(GameObject intersectedObject, ref SupplyNode node)
        {
            SupplyNode refNode = intersectedObject.transform.parent.gameObject.GetComponent<SupplyNode>() as SupplyNode;

            if(refNode == null)
            {
                node = null;
                return false;
            }

            node = refNode;
            return true;
        }

        public bool ContainsNode(int nodeId)
        {
            return (this.nodeId == nodeId);
        }

        public bool ContainsSquadId(Guid squadId)
        {
            List<Guid> squadGuids = this.SquadsInNode.Select( squad => squad.id).ToList();
            return (squadGuids.Contains(squadId));
        }

        public void HandleUnitDeparted(GamePlayEvent ev)
        {
            this.SquadsInNode.Remove(GamePlayState.GetSquadById(ev.squadId));
        }

        public void HandleUnitArrived(GamePlayEvent ev)
        {
            this.SquadsInNode.Add (GamePlayState.GetSquadById(ev.squadId));
        }

        /// <summary>
        /// returns the UnitListControl object (that is responsible for
        /// tracking and visually depicting the units in this building in a list like format). 
        /// </summary>
        public UnitListControl UnitsInHousing()
        {
            return this.SquadsInNodeChildObject;
        }

        public List<int> NodeIdsInHousing()
        {
            // supplynodes only have one node id in their 'housing', just return it 
            // in a size one list. 
            List<int> returnValue = new List<int>();
            returnValue.Add(this.nodeId);
            return returnValue;
        }

        public int nodeId { get; set; }
    }
}
