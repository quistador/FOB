using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SupplyNetwork
{
    /// <summary>
    /// The network connections: mapping node ID x to a list of node IDs that are connected to x.
    /// </summary>
    private Dictionary<int, List<int>> NetworkConnections;

    /// <summary>
    /// The network nodes: mapping node ID x to information about the node at x. 
    /// </summary>
    private SortedDictionary<int, SupplyNetwork.SupplyNode> NetworkNodes;

    public SupplyNetwork(Vector3 startPos)
    {
        NetworkNodes = new SortedDictionary<int, SupplyNode>();
        NetworkConnections = new Dictionary<int, List<int>>();

        // the first node always has id=0. 
        this.NetworkNodes.Add (
                0, 
                new SupplyNetwork.SupplyNode(startPos));


		this.InstantiateNodeObject(startPos);
    } 

    /// <summary>
    /// Adds the edge to the supply network at position endNodePosition
    /// </summary>
    /// <param name='endNodePosition'>
    /// End node position.
    /// </param>
    public void AddEdge(Vector3 endNodePosition)
    {
        SupplyNetwork.SupplyNode newNode = new SupplyNetwork.SupplyNode(endNodePosition);
        SupplyNetwork.SupplyNode previousNode = this.NetworkNodes[NetworkNodes.Count - 1];
        
		Vector3 startPosition = Vector3.zero;
		
		if( previousNode != null )
		{
			startPosition = previousNode.Position;
		}
		
        int newId = NetworkNodes.Count;
        NetworkNodes[newId] = newNode;
        AddConnection(newId - 1, newId);

        Object edgeResource = Resources.Load(@"SupplyEdge");
        GameObject edgeTest = Object.Instantiate(edgeResource, Vector3.zero, Quaternion.identity) as GameObject;
        
        SupplyEdge edge = edgeTest.GetComponent(typeof(SupplyEdge)) as SupplyEdge;
        edge.Initialize(
                startPosition,
                endNodePosition);
        this.InstantiateNodeObject(endNodePosition);
    }

    private void AddConnection(int startNodeId, int endNodeId)
    {
        // first, make sure that the entries have properly initialized lists. 
        if( !NetworkConnections.ContainsKey(startNodeId))
        {
            NetworkConnections[startNodeId] = new List<int>();
        }

        if( !NetworkConnections.ContainsKey(endNodeId))
        {
            NetworkConnections[endNodeId] = new List<int>();
        }

        // second, if we aren't already connected, add a connection. 
        if(!NetworkConnections[startNodeId].Contains(endNodeId))
        {
            NetworkConnections[startNodeId].Add (endNodeId);
        }

        if(!NetworkConnections[endNodeId].Contains(startNodeId))
        {
            NetworkConnections[endNodeId].Add (startNodeId);
        } 
    }

    /// <summary>
    /// Adds the edge to the supply network at position endNodePosition, linked to whatever node 
    /// is closest to startNodePosition
    /// </summary>
    /// <param name='startNodePosition'>
    /// Start node position.
    /// </param>
    /// <param name='endNodePosition'>
    /// End node position.
    /// </param>
    public void AddEdge(Vector3 startNodePosition, Vector3 endNodePosition)
    {
        //SupplyNetwork.SupplyNode newNode = new SupplyNode(endNodePosition);
        throw new System.ArgumentException("not yet implemented");
    }

    /// <summary>
    /// Supply node: internal node representation.  
    /// This class deals with the data and algorithms associated with the network, not the presentation. 
    /// </summary>
    public class SupplyNode
    {
        public SupplyNode(Vector3 position)
        {
            this.Position = position;
        }
        
		public Vector3 Position { get; set; }
    }
    
	private void InstantiateNodeObject(Vector3 position)
	{
        Object node = Resources.Load(@"SupplyNode");
        
        if(node == null)
        {
            throw new System.ArgumentException("prefab not loaded");
        }
        
        Object.Instantiate(node, position, Quaternion.identity);
	}
}
