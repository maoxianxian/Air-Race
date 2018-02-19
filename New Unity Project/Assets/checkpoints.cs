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
	Vector3 velocity;
	Vector3 leftorigin;
	Vector3 rightorigin;
	GameObject righto;
	GameObject lefto;
	LineRenderer direction;
	LineRenderer rightline2;
	Vector3 leaporigin;
	Vector3 preRightvect=Vector3.zero;
	float speed;

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
		//GameObject tGameObject.Instantiate (sphere);
		lefto=GameObject.Find("leftorigin");
		righto = GameObject.Find ("rightorigin");
		//lefto.SetActive (false);
		//righto.SetActive (false);
		velocity = player.transform.forward;
		direction = DrawLine (Vector3.zero, Vector3.zero, Color.blue);
		rightline2 = DrawLine (Vector3.zero, Vector3.zero, Color.red);
	}
	
	// Update is called once per frame
	void Update () {
		rightorigin = righto.transform.position;
		leftorigin = lefto.transform.position;
		leaporigin = player.transform.GetChild (0).transform.GetChild (0).transform.position;

		connectToHands ();
		decideDirection ();
		decideSpeed ();

		if (lefthand != null) {
			player.transform.position += speed*velocity;
		}
	}

	void decideDirection(){
		if (righthand != null) {
			Vector3 rightvect =  Vector3.Normalize(leapToUnity (righthand.PalmPosition / 1000.0f));
			float deg = Vector3.Dot (preRightvect, rightvect);
			Vector3 axis = Vector3.Normalize(Vector3.Cross (preRightvect,rightvect));
			if (deg < 0.999 && isFist(righthand)) {
				velocity=Quaternion.AngleAxis(deg*3, axis) * velocity;
				direction.SetPosition (0, leaporigin+player.transform.forward);
				direction.SetPosition (1, leaporigin+velocity*10+player.transform.forward);
				preRightvect = rightvect;
			}
		} 
	}

	void decideSpeed(){
		if (lefthand != null) {
			speed += 0.03f*leapToUnity (lefthand.PalmPosition / 1000.0f).magnitude;
			if (isFist(lefthand)) {
				speed = 0;
			}
		} else {
			speed = 0;
		}
	}

	Vector3 leapToUnity(Leap.Vector v)
	{
		Vector3 result = new Vector3(0,0,0);
		result.x = -v.x;
		result.y = -v.z;
		result.z = v.y;
		return result;
	}

	bool isFist(Leap.Hand h){
		int badfinger = 0;
		foreach (Leap.Finger fig in h.Fingers) {
			if (!fig.IsExtended) {
				badfinger++;
			}
		}
		if (badfinger > 1) {
			return true;
		}
		return false;
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

	LineRenderer DrawLine(Vector3 start, Vector3 end,Color color)
	{
		GameObject myline = new GameObject ();
		myline.transform.position = start;
		myline.AddComponent<LineRenderer> ();
		LineRenderer lr = myline.GetComponent<LineRenderer> ();
		//lr.material = new Material (Shader.Find("Particles/Additive"));
		lr.SetColors (color,color);
		lr.SetWidth (0.02f, 0.02f);
		lr.positionCount = 2;
		lr.SetPosition (0, start);
		lr.SetPosition (1, end);
		return lr;
	}
}
