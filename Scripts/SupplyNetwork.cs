using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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

        // create an initial node at startPos. 
        this.InstantiateNodeObject(startPos);
    } 

    /// <summary>
    /// Adds the edge to the supply network at position endNodePosition
    /// </summary>
    /// <param name='endNodePosition'>
    /// End node position.
    /// </param>
    /// <returns>
    /// the x/y position that the new edge terminated at. 
    /// </returns>
    public Vector3 AddEdge(Vector3 endNodePosition)
    {

        Vector3 startPosition = Vector3.zero;
        // any node which is within this distance to an added
        // node will automatically be 'bridged'. 
        float maxDistanceForBridge = 0.25f;

        List<int> nearestNodeId = this.nearestNeighborNode(endNodePosition, 2);

        if( nearestNodeId.Count > 0 )
        {
            startPosition = this.NetworkNodes[nearestNodeId[0]].Position;
        }

        float distanceToNewEdge = SupplyEdge.calculateScaleForEdge(startPosition,endNodePosition);
        float theta = SupplyEdge.calculateRotationForEdge(startPosition,endNodePosition);
        float thetaInRadians =  theta * (float)(System.Math.PI/180f);

        Vector3 endNodePositionTransformed = new Vector3(
                (float)(startPosition.x + distanceToNewEdge * System.Math.Cos(thetaInRadians)),
                (float)(startPosition.y + distanceToNewEdge * System.Math.Sin(thetaInRadians)),
                endNodePosition.z);

        int newId = NetworkNodes.Count;

        // the nearest node will be the first element returned from this.nearesNeighborNode. 
        SupplyNetwork.SupplyNode nearestNode = this.NetworkNodes[nearestNodeId[0]];

        // if there are more than one nodes present, we need to 
        // start checking for bridging. 
        if( nearestNodeId.Count > 1 )
        {
            SupplyNetwork.SupplyNode secondNearestNode = this.NetworkNodes[nearestNodeId[1]];

            float lengthFromNewNodeToSecondNearestNode = Vector3.Distance(secondNearestNode.Position,endNodePositionTransformed);
            if(lengthFromNewNodeToSecondNearestNode < maxDistanceForBridge)
            {
                Debug.Log("bridge");
                AddConnection(nearestNodeId[1], newId);
                this.InstantiateEdgeObject(secondNearestNode.Position, endNodePositionTransformed);
            }
        }

        SupplyNetwork.SupplyNode newNode = new SupplyNetwork.SupplyNode(endNodePositionTransformed);
        NetworkNodes[newId] = newNode;

        AddConnection(nearestNodeId[0], newId);

        this.InstantiateEdgeObject(nearestNode.Position, endNodePositionTransformed);
        this.InstantiateNodeObject(endNodePositionTransformed);
        return endNodePositionTransformed;
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

        /// instaniating these for debugging since the unity debugger doesn't 
        /// make it easy to look up dictionary values. 
        SupplyNetwork.SupplyNode debugNodeStart;
        SupplyNetwork.SupplyNode debugNodeEnd;

        if(this.NetworkNodes.ContainsKey(startNodeId) && this.NetworkNodes.ContainsKey(endNodeId))
        {
            debugNodeStart = this.NetworkNodes[startNodeId];
            debugNodeEnd = this.NetworkNodes[endNodeId]; 
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

    /// <summary>
    /// Returns the position of the Network node that
    /// is closest to input position in 2D space
    /// </summary>
    /// <returns>
    /// The neighbor node.
    /// </returns>
    /// <param name='ADPosition'>
    /// AD position.
    /// </param>
    public int nearestNeighborNode(Vector3 position)
    {
        //Debug.Log(string.Format("calculating nearest neighbor for position {0:F02},{1:F02} = {2:F02},{3:F02}", position.x,position.y, nearestNode.Position.x,nearestNode.Position.y));
        List<int> list = nearestNeighborNode(position, 1);
        return list.First();
    }   

    public List<int> nearestNeighborNode(Vector3 position, int numberOfNodes)
    {
        SortedDictionary<float,int> distanceToIdMap = new SortedDictionary<float, int>();

        int keyForNearestNeighbor = 999;

        SupplyNetwork.SupplyNode nearestNode = new SupplyNetwork.SupplyNode(Vector3.zero);
        float minDistance = float.MaxValue;

        // nearest neighbor search algorithms are numerous but intricate.  We'll be 
        // sticking to a naive linear search for now. 
        foreach(int nodeKey in this.NetworkNodes.Keys)
        {
            SupplyNetwork.SupplyNode node = this.NetworkNodes[nodeKey];

            // for purpose of finding the min distance, I'm assuming that we don't need to 
            // calculate d^2 = (x^2 + y^2).  we'll try to avoid some calculations by computing
            // d = x + y. 
            float simplifiedDistance = 
                System.Math.Abs(position.x - node.Position.x) +  
                System.Math.Abs (position.y - node.Position.y);

            distanceToIdMap[simplifiedDistance] = nodeKey;
            if(simplifiedDistance < minDistance)
            {
                nearestNode = node;
                minDistance = simplifiedDistance;
                keyForNearestNeighbor = nodeKey;
            }
        }

        var a = distanceToIdMap.Take(numberOfNodes);
        List<int> nearestIds = distanceToIdMap.Take(numberOfNodes).Select( kvp => kvp.Value ).ToList();

        //Debug.Log(string.Format("calculating nearest neighbor for position {0:F02},{1:F02} = {2:F02},{3:F02}", position.x,position.y, nearestNode.Position.x,nearestNode.Position.y));
        return nearestIds;
    }

    public SupplyNetwork.SupplyNode NodeForId(int id)
    {
        return this.NetworkNodes[id];
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

    private void InstantiateEdgeObject(Vector3 startPosition, Vector3 endPosition)
    {
        Object edgeResource = Resources.Load(@"SupplyEdge");
        GameObject edgeTest = Object.Instantiate(edgeResource, Vector3.zero, Quaternion.identity) as GameObject;
        SupplyEdge edge = edgeTest.GetComponent(typeof(SupplyEdge)) as SupplyEdge;
        edge.Initialize(
                startPosition,
                endPosition);
    }
}
