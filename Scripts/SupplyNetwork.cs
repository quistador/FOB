using UnityEngine;
using System;
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

    public SupplyNetwork(Army army, LevelV0 level)
    {
        NetworkNodes = new SortedDictionary<int, SupplyNode>();
        NetworkConnections = new Dictionary<int, List<int>>();

        // add nodes for all the entry points in our level. 
        // caution is needed when changing, since we are calling a static method
        // that does a lot of writing to various locations.  If this constructor
        // is ever called twice in one succession, disaster might arise. 
        level.AddBuildingEntryPointsToNetwork(this);
        
        int startingPointId = level.Buildings.Single(building => building.isStartingPosition == true).nodeIdsForEntryPoints.First();
        this.NetworkNodes[startingPointId].SquadsInNode = army.Squads;

        // instead of adding new functions to 'initialize' the units in there initial starting building, 
        // we'll reuse the Event Queue, adding 'unit arrived' events. 
        army.Squads.ForEach( squad =>
            GamePlayEvents.AddEvent( 
            
                startingPointId,
                
                new GamePlayEvent()
                {
                    nodeId = startingPointId ,
                    eventKind = GamePlayEvent.EventKind.UnitArrived
                }
            ));

    } 

    /// <summary>
    /// Adds the nodes from building entry points.
    /// </summary>
    /// <returns>
    /// The nodeIds from the building entry points.
    /// </returns>
    /// <param name='entryPointPositions'>
    /// positions of the doors for this building.
    /// </param>
    /// <param name='building'>
    /// Building.
    /// </param>
    public List<int> AddNodeFromBuildingEntryPoints(List<Vector3> entryPointPositions)
    {
        List<int> nodeIdsForThisBuilding = new List<int>();

        entryPointPositions.ForEach( position =>
                {
                    int newId;
                    if(this.NetworkNodes.Keys.Count == 0)
                    {
                        newId = 0;
                    }
                    else
                    {
                        newId = this.NetworkNodes.Keys.Max () + 1;
                    }

                    this.NetworkNodes.Add (newId,new SupplyNetwork.SupplyNode(){Position = position, NodeId = newId});
                    nodeIdsForThisBuilding.Add(newId);
                });

        return nodeIdsForThisBuilding;
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

        SupplyNetwork.SupplyNode newNode = new SupplyNetwork.SupplyNode(){Position = endNodePositionTransformed, NodeId  = newId};
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
    /// the ID specified by startingPointNode is where our units begin at game start time. 
    /// </summary>
    private int startingPointNode { get; set; }

    public void MarkAsStartingPoint(int nodeIdOfStartPoint)
    {
        this.startingPointNode = nodeIdOfStartPoint;
    }

    /// <summary>
    /// Supply node: internal node representation.  
    /// This class deals with the data and algorithms associated with the network, not the presentation. 
    /// </summary>
    public class SupplyNode
    {
        public SupplyNode()
        {
            this.SquadsInNode = new List<Squad>();
        }

        public SupplyNode(Vector3 position)
        {
            this.Position = position;
        }

        public Vector3 Position { get; set; }
        public int NodeId { get; set; }
        public List<Squad> SquadsInNode {get; set;}
    }

    public List<int> shortestPath(int startId, int endId)
    {
        if(this.NetworkNodes.Count == 0 )
        {
            return null;
        }
        SupplyNode end = this.NetworkNodes[endId];

        Dictionary<int, int> cameFrom = new Dictionary<int, int>();
        List<int> openList = new List<int>();
        List<int> closedList = new List<int>();
        List<int> neighbors = new List<int>();

        Dictionary<int,float> gCost = new Dictionary<int,float>();
        Dictionary<int,float> hCost = new Dictionary<int,float>();
        Dictionary<int,float> fCost = new Dictionary<int,float>();

        gCost[startId] = 0;
        hCost[startId] = this.mapHeuristic(startId,endId);
        System.Diagnostics.Debug.Assert(hCost[startId] >= 0);
        fCost[startId] = hCost[startId];

        openList.Add(startId);

        while( openList.Count() > 0 )
        {
            // get the point in the open list with the lowest f-cost. 
            int p = openList.Aggregate(openList[0], (currentMin, x) => (fCost[x] < fCost[currentMin]) ? x : currentMin);
            if(p == endId)
            {
                List<int> path = new List<int>();
                path = reconstructPath(cameFrom, endId, path);
                path.Add (endId);
                //List<SupplyNode> pathInGameCoords = path.Select(point => new Vector2(point.Position.x,point.Position.y)).ToList();
                return path;
            }

            openList.Remove(p);
            closedList.Add(p);

            neighbors = this.NetworkConnections[p];

            // limit neighboring points to those not already in the 
            // open and closed list. 
            neighbors = neighbors.Where(neighbor =>
                    !openList.Contains(neighbor) && !closedList.Contains(neighbor)
                    ).ToList();


            foreach (int possibleLink in neighbors)
            {
                bool tentativeIsBetter;
                SupplyNode nodeForPossibleLink = this.NetworkNodes[possibleLink];
                SupplyNode nodeForP = this.NetworkNodes[p];
                float tentativeGCost = gCost[p] + ((nodeForP.Position.x == nodeForPossibleLink.Position.x || nodeForP.Position.y == nodeForPossibleLink.Position.y) ? 10 : 14);
                if (!openList.Contains(possibleLink))
                {
                    openList.Add(possibleLink);
                    tentativeIsBetter = true;
                }
                else if (tentativeGCost < gCost[possibleLink])
                {
                    tentativeIsBetter = true;
                }
                else
                {
                    tentativeIsBetter = false;
                }

                if (tentativeIsBetter == true)
                {
                    cameFrom[possibleLink] = p;
                    gCost[possibleLink] = tentativeGCost;
                    hCost[possibleLink] = this.mapHeuristic(possibleLink, endId);
                    fCost[possibleLink] = gCost[possibleLink] + hCost[possibleLink];
                }
            }
        }

        return null;
    }

    private float mapHeuristic(int start, int end)
    {
        SupplyNode startNode = this.NetworkNodes[start];
        SupplyNode endNode = this.NetworkNodes[end];
        return 10 * (Math.Abs(startNode.Position.x - endNode.Position.x) + Math.Abs(startNode.Position.y - endNode.Position.y));
    }

    private List<int> reconstructPath(Dictionary<int, int> cameFrom, int currentNode, List<int> path)
    {
        if (cameFrom.ContainsKey(currentNode))
        {
            path.Insert(0, cameFrom[currentNode]);
            return reconstructPath(cameFrom, cameFrom[currentNode], path);
        }
        else
        {
            return path;
        }
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
        //Debug.Log(string.Format("calculating nearest neighbor for position {0:F02},{1:F02} = {2:F02},{3:F02}; ID = {4}", position.x,position.y, nearestNode.Position.x,nearestNode.Position.y, nearestIds.First()));
        return nearestIds;
    }

    public SupplyNetwork.SupplyNode NodeForId(int id)
    {
        return this.NetworkNodes[id];
    }

    public void Requisition(Building originBuilding, Building destinationBuilding)
    {
        UnityEngine.Object unitResource = Resources.Load(@"Unit");

        // just take the first door of the selected building right now.  We aren't currently supporting 
        // multiple doors, this will need to be changed and appropriately refactored at a future point. 
        int idForDestination = destinationBuilding.nodeIdsForEntryPoints.First();
        int idForOrigin = originBuilding.nodeIdsForEntryPoints.First();
        
        List<int> shortestPath = this.shortestPath(idForOrigin, idForDestination);

        List<SupplyNetwork.SupplyNode> shortestPathCoords = shortestPath.Select(nodeId => 
                new SupplyNetwork.SupplyNode()
                {
                    Position = new Vector2(this.NetworkNodes[nodeId].Position.x,this.NetworkNodes[nodeId].Position.y),
                    NodeId = nodeId
                }).ToList();

        Vector3 startPosition = this.NetworkNodes[shortestPath[0]].Position;
        GameObject unitTest = UnityEngine.Object.Instantiate( unitResource, startPosition, Quaternion.identity) as GameObject;
        Unit unit = unitTest.GetComponent(typeof(Unit)) as Unit;
        unit.SetPath(shortestPathCoords);
    }

    private void InstantiateNodeObject(Vector3 position)
    {
        UnityEngine.Object node = Resources.Load(@"SupplyNode");

        if(node == null)
        {
            throw new System.ArgumentException("prefab not loaded");
        }

        UnityEngine.Object.Instantiate(node, position, Quaternion.identity);
    }

    private void InstantiateEdgeObject(Vector3 startPosition, Vector3 endPosition)
    {
        UnityEngine.Object edgeResource = Resources.Load(@"SupplyEdge");
        GameObject edgeTest = UnityEngine.Object.Instantiate(edgeResource, Vector3.zero, Quaternion.identity) as GameObject;
        SupplyEdge edge = edgeTest.GetComponent(typeof(SupplyEdge)) as SupplyEdge;
        edge.Initialize(
                startPosition,
                endPosition);
    }
}
