using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
public class checkpoints : MonoBehaviour {
	public TextAsset T;
	public GameObject sphere;
	public Shader dirshader;
	public Shader veloshader;
	System.IO.StreamReader reader;
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
	LineRenderer nextpoint;
	Vector3 leaporigin;
	Vector3 preRightvect=Vector3.zero;
	float speed;
	List<GameObject> checkpointlist;
	float timer=0;
	Vector3 linestart;
	Vector3 cameraforward;
	GameObject countdownText;
	GameObject stopWatchText;
	GameObject nextpointtext;
	GameObject leapspace;
	int freezecounter=5;
	float totaltime;
	int currentpoint=0;
	int pinchframe=0;
	bool end=false;
	// Use this for initialization
	void Start () {
		controller= new Leap.Controller ();
		campus = GameObject.FindGameObjectWithTag ("campus");
		reader = new StreamReader (toStream (T.text));
		string[] data;
		checkpointlist = new List<GameObject> ();
		while (!reader.EndOfStream) {
			data = reader.ReadLine ().Split (' ');
			GameObject temp=GameObject.Instantiate (sphere);
			temp.transform.parent = campus.transform;
			temp.transform.localPosition = new Vector3 (float.Parse(data [0])*0.0254f, float.Parse(data [1])*0.0254f, float.Parse(data [2])*0.0254f);
			temp.tag = "checkpoint";
			checkpointlist.Add (temp);
		}
		player = GameObject.FindGameObjectWithTag ("Player");
		lefto=GameObject.Find("leftorigin");
		righto = GameObject.Find ("rightorigin");
		velocity = player.transform.forward;
		direction = DrawLine (Vector3.zero, Vector3.zero, Color.blue,veloshader);
		nextpoint = DrawLine (Vector3.zero, Vector3.zero, Color.red,dirshader);
		countdownText = GameObject.Find ("countdownText");
		stopWatchText = GameObject.Find ("stopwatch");
		nextpointtext = GameObject.Find ("nextPoint");
		leapspace = player.transform.GetChild (0).transform.GetChild (0).gameObject;
	}
	
	// Update is called once per frame
	void Update () {
		updatevariables ();
		countdown (freezecounter);
		stopwatch (freezecounter);
		connectToHands ();
		decideDirection ();
		decideSpeed ();
		if (!end) {
			detectCollision ();
		}
	}

	void FixedUpdate(){
		if (lefthand != null && timer>freezecounter) {
			player.transform.position += speed*velocity;
		}
	}

	void updatevariables(){
		timer += Time.deltaTime;
		rightorigin = righto.transform.position;
		leftorigin = lefto.transform.position;
		leaporigin = leapspace.transform.position;
		cameraforward = leapspace.transform.forward;
		linestart = leaporigin + cameraforward;
		nextpoint.SetPosition (0, linestart);
		nextpoint.SetPosition (1, checkpointlist [currentpoint].transform.position);
	}

	void countdown(float cliptime){
		if (timer < cliptime) {
			countdownText.SetActive (true);
			countdownText.GetComponent<UnityEngine.UI.Text> ().text = "Count down: " +System.Environment.NewLine+ (5 - Mathf.Floor (timer));
			countdownText.transform.position = linestart;
			countdownText.transform.forward = cameraforward;
		} else {
			countdownText.SetActive (false);
		}
	}

	void stopwatch(float cliptime){
		if (timer > cliptime) {
			stopWatchText.SetActive (true);
			stopWatchText.GetComponent<UnityEngine.UI.Text> ().text = Mathf.Floor (totaltime).ToString();
			totaltime += Time.deltaTime;
			stopWatchText.transform.position = linestart - leapspace.transform.up*0.4f;
			stopWatchText.transform.forward = cameraforward;
		} else {
			stopWatchText.SetActive (false);
		}
	}
	//compute direction of velocity and draw a line
	void decideDirection(){
		if (righthand != null) {
			Vector3 rightvect =  Vector3.Normalize(leapToUnity (righthand.PalmPosition / 1000.0f));
			//nextpoint.SetPosition (0, leaporigin);
			//nextpoint.SetPosition (1, leaporigin+rightvect);
			float deg = Vector3.Dot (preRightvect, rightvect);
			Vector3 axis = Vector3.Normalize(Vector3.Cross (preRightvect,rightvect));
			if (deg < 0.999 && isFist (righthand)) {
				velocity = Quaternion.AngleAxis (deg * 3, axis) * velocity;
				preRightvect = rightvect;
				pinchframe = 0;
			} else if (!isFist (righthand) && righthand.PinchStrength > 0.9) {
				pinchframe++;
				if (pinchframe > 6) {
					velocity = cameraforward;
					pinchframe = 0;
				}
			} else {
				pinchframe = 0;
			}
		}
		direction.SetPosition (0, linestart);
		direction.SetPosition (1, linestart+velocity*10);
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
		if (badfinger > 3) {
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

	void detectCollision(){
		GameObject point = checkpointlist [currentpoint];
		float dist = (player.transform.position - point.transform.position).magnitude;
		nextpointtext.GetComponent<UnityEngine.UI.Text>().text = Mathf.Floor(dist).ToString ();	
		nextpoint.transform.position = point.transform.position;
		nextpoint.transform.forward = cameraforward;
		if (dist < 10) {
			currentpoint++;
			if (currentpoint == checkpointlist.Count) {
				gameEnd ();
			}
		}
	}

	void gameEnd(){
		bool end = true;
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

	LineRenderer DrawLine(Vector3 start, Vector3 end,Color color,Shader shad)
	{
		GameObject myline = new GameObject ();
		myline.transform.position = start;
		myline.AddComponent<LineRenderer> ();
		LineRenderer lr = myline.GetComponent<LineRenderer> ();
		lr.material = new Material (shad);
		lr.SetWidth (0.02f, 0.02f);
		lr.positionCount = 2;
		lr.SetPosition (0, start);
		lr.SetPosition (1, end);
		return lr;
	}
}
