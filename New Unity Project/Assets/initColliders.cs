using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class initColliders : MonoBehaviour {

	// Use this for initialization
	void Start () {
		MeshCollider[] meshes = this.GetComponentsInChildren<MeshCollider> (); 
		foreach (MeshCollider m in meshes) {
			m.convex = true;
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
