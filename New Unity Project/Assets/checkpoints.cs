using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
public class checkpoints : MonoBehaviour {
	public TextAsset T;
	System.IO.StreamReader reader;
	public GameObject sphere;
	GameObject campus;
	Leap.Hand lefthand;
	Leap.Hand righthand;
	Leap.Controller controller;
	GameObject player;
	// Use this for initialization
	void Start () {
		controller= new Leap.Controller ();
		campus = GameObject.FindGameObjectWithTag ("campus");
		reader = new StreamReader (toStream (T.text));
		string[] data;
		while (!reader.EndOfStream) {
			data = reader.ReadLine ().Split (' '); 
			GameObject temp=GameObject.Instantiate (sphere);
			temp.transform.parent = campus.transform;
			temp.transform.position = new Vector3 (float.Parse(data [0])*0.0254f, float.Parse(data [1])*0.0254f, float.Parse(data [2])*0.0254f);
		}
		player = GameObject.FindGameObjectWithTag ("Player");
	}
	
	// Update is called once per frame
	void Update () {
		connectToHands ();
		if (lefthand != null) {
			
		}
	}

	void connectToHands(){
		if (controller.Frame ().Hands.Count == 1) {
			Leap.Hand temp = controller.Frame ().Hands [0];
			if (temp.IsLeft) {
				lefthand = temp;
				righthand = null;
			} else {
				righthand = temp;
				lefthand = null;
			}
		} else if (controller.Frame ().Hands.Count == 2) {
			Leap.Hand temp = controller.Frame ().Hands [0];
			if (temp.IsLeft) {
				lefthand = temp;
				righthand = controller.Frame ().Hands [1];
			} else {
				righthand = temp;
				lefthand = controller.Frame ().Hands [1];
			}

		} else {
			lefthand = null;
			righthand = null;
		}
	}
	Stream toStream(string str)
	{
		MemoryStream stream = new MemoryStream ();
		StreamWriter writer = new StreamWriter (stream);
		writer.Write (str);
		writer.Flush ();
		stream.Position = 0;
		return stream;
	}
}
