using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
public class checkpoints : MonoBehaviour {
	public TextAsset T;
	System.IO.StreamReader reader;
	// Use this for initialization
	void Start () {
		reader = new StreamReader (toStream (T.text));
		string[] data;
		while (!reader.EndOfStream) {
			data = reader.ReadLine ().Split (' '); 
			foreach (string s in data) {
			}
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
