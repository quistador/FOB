using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class LevelV0 : MonoBehaviour 
{
    public Material material;
    public Material buildingMaterial;
    public Material cityBlockMaterial;

    private List<Building> buildings;

    public void Initialize()
    {
        buildings = new List<Building>();

        UnityEngine.Object obj = Resources.Load (@"Building");
        if(obj == null)
        {
            throw new System.ArgumentException("couldn't load building resource");
        }

        // create the stone base layer
        Mesh background;
        GameObject levelBaseLayer = new GameObject("levelBaseLayer");
        levelBaseLayer.transform.position = Vector3.zero;
        background = PlaneMeshTools.CreatePlane(
                1000f, 1000f,
                2,2, 
                material, 
                levelBaseLayer);

        float buildingWidth = 0.6f;
        float buildingGap = 0.08f;
        float streetWidth = 0.4f;
        float sideWalkWidth = 0.3f;
        float buildingHeight = -0.001f;

        // this is weird, but I'm doing it now since I can't instantiate unity objects using 'new'.
        // I'm going to do this until I figure out something better. 
        // Generally, instead of passing input parameters to a constructor, we'll set some public variables of the 
        // class. 
        Building.buildingWidth = buildingWidth;

        //  sw = sideWalkWidth
        //  bw = blockWidth
        //  bg = buildingGap
        //  bld = buildingWidth
        //
        //  <------ bw ------->
        //        
        //  |------------------|            |------------------|         
        //  |                  |            |                  |
        //  |<sw>|--|  |--|<sw>|            |    |--|  |--|    |  
        //  |    |b3|  |b4|    |            |    |b3|  |b4|    |
        //  |                  |<--street-->|                  |
        //  |                  |   width    |                  |
        //  |    |--|bg|--|    |            |    |--|  |--|    |
        //  |    |b1|  |b2|    |            |    |b1|  |b2|    |
        //  |          <-->    |            |                  |
        //  |____________|_____|            |__________________|
        //               |
        //               bld
        //
        // (diagram above hopefully explains the below equation) 
        float blockWidth = (sideWalkWidth*2) + (buildingWidth*2) + buildingGap;

        int cityBlockCount = 4;

        Vector3 initialBlockPosition = Vector3.zero;

        for(int i = 0; i < cityBlockCount; i++)
        {
            Vector3 currentBlockPosition = new Vector3( 
                    initialBlockPosition.x + (blockWidth + streetWidth)*i,
                    initialBlockPosition.y,
                    buildingHeight);

            Vector3 building1Position = new Vector3(
                    currentBlockPosition.x + sideWalkWidth,
                    currentBlockPosition.y + sideWalkWidth,
                    buildingHeight*2);

            Vector3 building2Position = new Vector3(
                    currentBlockPosition.x + sideWalkWidth + buildingGap + buildingWidth,
                    currentBlockPosition.y + sideWalkWidth,
                    building1Position.z);

            Vector3 building3Position = new Vector3(
                    currentBlockPosition.x + sideWalkWidth,
                    currentBlockPosition.y + sideWalkWidth + buildingGap + buildingWidth,
                    building1Position.z);

            Vector3 building4Position = new Vector3(
                    currentBlockPosition.x + sideWalkWidth + buildingGap + buildingWidth,
                    currentBlockPosition.y + sideWalkWidth + buildingGap + buildingWidth,
                    building1Position.z);

            GameObject tempObj; 

            tempObj = Instantiate(obj, building1Position, Quaternion.identity) as GameObject;
            Building b1 = tempObj.GetComponent(typeof(Building)) as Building;
            b1.gameObject.name = string.Format("building{0}1",i);

            tempObj = Instantiate(obj, building2Position, Quaternion.identity) as GameObject;
            Building b2 = tempObj.GetComponent(typeof(Building)) as Building;
            b2.gameObject.name = string.Format("building{0}2",i);

            tempObj = Instantiate(obj, building3Position, Quaternion.identity) as GameObject;
            Building b3 = tempObj.GetComponent(typeof(Building)) as Building;
            b3.gameObject.name = string.Format("building{0}3",i);

            tempObj = Instantiate(obj, building4Position, Quaternion.identity) as GameObject;
            Building b4 = tempObj.GetComponent(typeof(Building)) as Building;
            b4.gameObject.name = string.Format("building{0}4",i);

            buildings.Add(b1);
            buildings.Add(b2);
            buildings.Add(b3);
            buildings.Add(b4);

            buildings.ForEach(b=>b.Start());
            GameObject blockObject = new GameObject("block");
            blockObject.transform.position = currentBlockPosition;
            Mesh block = PlaneMeshTools.CreatePlane(
                    blockWidth,blockWidth,
                    2,2, 
                    cityBlockMaterial, 
                    blockObject);
        }
        
        // for our v0 level, we have to specify a starting position somehow (we
        // we currently don't have a user input way of doing it). Choose the first 
        // building, change this methodology in later levels. 
        buildings.First().isStartingPosition = true;
    }

    // Use this for initialization
    void Start () 
    {
    
    }

    // Update is called once per frame
    void Update () 
    {
    }

    public Building GetDestinationBuilding()
    {
        return GetBuildingByColor(Color.cyan);
    }

    public Building GetOriginBuilding()
    {
        return GetBuildingByColor(Color.red);
    }

    /// <summary>
    /// Gets the selected building: one building in the level can be 'selected' at any time. 
    /// this function returns that one building. 
    /// </summary>
    /// <returns>
    /// The selected building.
    /// </returns>
    public Building GetBuildingByColor(Color color)
    {
        Building returnBuilding = null;
        try
        {
            returnBuilding = buildings.Single(building =>
                {
                    MeshRenderer[] meshRenderer = building.GetComponentsInChildren<MeshRenderer>() as MeshRenderer[];
                    Color buildingColor = meshRenderer[1].material.color;
                    if(buildingColor.Equals(color))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                });
        }
        catch(System.Exception e)
        {
            Debug.Log("exception due to non-singular building colors.  Swallowing this exception until dev progresses...");
        }

        return returnBuilding;
    }

    public void AddBuildingEntryPointsToNetwork(SupplyNetwork network)
    {
        buildings.ForEach( building => 
                {
                    // get the adjusted entry positions for this building. 
                    List<Vector3> adjustedEntryPositions = building.LevelAdjustedEntryPointPosition();

                    // pass these off to our supply network. 
                    List<int> nodeIds = network.AddNodeFromBuildingEntryPoints(adjustedEntryPositions); 

                    // make sure that the building keeps a reference to the ids that were created. 
                    building.nodeIdsForEntryPoints = nodeIds;
                });
    }

    public Building GetBuildingContainingSquadId(Guid squadId)
    {
        Building buildingWithSquadInIt = this.buildings.Single( building =>
        {
            return building.SquadIdsInThisBuilding.Contains(squadId);
        });

        return buildingWithSquadInIt;
    }

    public List<Vector3> GetEntryPointPositionsInLevel()
    {
        List<Vector3> doorPositions = this.buildings.SelectMany( building => 
                {
                // problem:  if we use the door position as the entry point position (which initially seems like the 
                // logical thing to do), then our 'supplyEdgeBeingPlaced' will intersect with the building (since it starts
                // at the entry point positions that we return from this function).  If it intersects, then the edge will 
                // be marked as 'invalid', because we don't want to allow edges that intersect with buildings. So, how do we solve?
                // we'll start with a naive approach, where we move the entry point outward from the bounds-center of the building, 
                // along the vector that travels from the bounds center to the door position:  
                //
                //                               o  <---(adjusted door position)
                //                             /
                //                           /
                // |---------------------|-x-|-|    <---(original door position)
                // |                     /     |
                // |                   /       |
                // |                 /         |
                // |               /           |
                // |             x             |
                // |        (bounds center)    |
                // |                           |
                // |                           |
                // |                           |
                // |___________________________|
                //
                // this *will* look weird for certain door positions and this shouldn't be a permanent solution. 
                    MeshCollider collider = building.gameObject.GetComponent<MeshCollider>();
                    Vector3 buildingCenter = collider.bounds.center;
                    List<Vector3> originalDoorPositions = building.EntryPointPositions;

                    List<Vector3> adjustedDoorPositions = originalDoorPositions.Select(door => 
                            {
                            // get the vector from the center to the door. 
                            Vector3 centerToDoor = buildingCenter - door;
                            centerToDoor.Normalize();
                            centerToDoor = centerToDoor * 0.01f;
                            return door - centerToDoor;

                            }).ToList();

                    return adjustedDoorPositions;
                }).ToList();

        return doorPositions;
    }

    public List<Building> Buildings
    {
        get { return this.buildings; }
    }
}
