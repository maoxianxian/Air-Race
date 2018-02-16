using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
public class checkpoints : MonoBehaviour {
	public TextAsset T;
	System.IO.StreamReader reader;
	public GameObject sphere;
	GameObject campus;
	// Use this for initialization
	void Start () {
		campus = GameObject.FindGameObjectWithTag ("campus");
		reader = new StreamReader (toStream (T.text));
		string[] data;
		while (!reader.EndOfStream) {
			data = reader.ReadLine ().Split (' '); 
			GameObject temp=GameObject.Instantiate (sphere);
			Debug.Log (temp);
			temp.transform.parent = campus.transform;
			temp.transform.position = new Vector3 (float.Parse(data [0])*0.0254f, float.Parse(data [1])*0.0254f, float.Parse(data [2])*0.0254f);
			//temp.transform.localScale = new Vector3 (0.0254f, 0.0254f, 0.0254f);
		}
	}
	
	// Update is called once per frame
	void Update () {
		
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
