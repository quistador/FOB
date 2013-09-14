using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LevelV0 : MonoBehaviour 
{
    public Material material;
    public Material buildingMaterial;
    public Material cityBlockMaterial;

    private GamePlayState gamePlayState;

    private List<Building> buildings;

    // Use this for initialization
    void Start () 
    {
        buildings = new List<Building>();
        gamePlayState = new GamePlayState();

        Object obj = Resources.Load (@"Building");
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

            GameObject blockObject = new GameObject("block");
            blockObject.transform.position = currentBlockPosition;
            Mesh block = PlaneMeshTools.CreatePlane(
                    blockWidth,blockWidth,
                    2,2, 
                    cityBlockMaterial, 
                    blockObject);
        }
    }

    // Update is called once per frame
    void Update () 
    {
        this.gamePlayState.CurrentMouseWorldCoordinate = new Vector3((float)Input.mousePosition.x, (float)Input.mousePosition.y, 0f);
        this.gamePlayState.UpdateState();
    }
}
