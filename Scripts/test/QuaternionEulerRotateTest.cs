using UnityEngine;
using System.Collections;

public class QuaternionEulerRotateTest : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () 
	{
		this.transform.rotation = Quaternion.Euler(Vector3.zero);
	}
}
