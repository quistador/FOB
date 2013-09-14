using UnityEngine;
using System.Collections;

public class RotateAroundTest : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
		Vector3 around = new Vector3(
			2.2f,
			2.2f,
			0);
			
		transform.RotateAround (
			around,
			new Vector3(0,0,1), 
			20 * Time.deltaTime);
	}
}
