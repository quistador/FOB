using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class UnitsInBuilding : MonoBehaviour 
{

    public TextMesh[] unitListDisplay;
    
    public Material debugMaterial;
    
	// Use this for initialization
	void Start () 
    {
        BoxCollider[] colliders = this.GetComponentsInChildren<BoxCollider>() as BoxCollider[];
        
        foreach(BoxCollider collider in colliders)
        {
            Debug.DrawLine(
                collider.bounds.min,
                collider.bounds.max);
                
            GameObject debugPlaneObject = new GameObject("debug");
            debugPlaneObject.transform.position = this.gameObject.transform.position;
            PlaneMeshTools.CreatePlane(
                collider.bounds.max.x - collider.bounds.min.x,
                collider.bounds.max.y - collider.bounds.min.y,
                2, 2,
                debugMaterial,
                debugPlaneObject);
        }
	}
	
	// Update is called once per frame
	void Update () 
    {
        BoxCollider[] colliders = this.GetComponentsInChildren<BoxCollider>() as BoxCollider[];
        
        foreach(BoxCollider collider in colliders)
        {
            Debug.DrawLine(
                collider.bounds.min,
                collider.bounds.max);
        }
	}
 
    public void Initialize(List<Unit> units, Vector3 buildingPosition)
    {
    }
}
