using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
public class checkpoints : MonoBehaviour {
	public TextAsset T;
	public GameObject sphere;
	public Shader dirshader;
	public Shader veloshader;
	public Shader farshader;
	System.IO.StreamReader reader;
	GameObject campus;
	Leap.Hand lefthand;
	Leap.Hand righthand;
	Leap.Controller controller;
	GameObject player;
	Vector3 velocity;
	LineRenderer direction;
	LineRenderer nextpoint;
	LineRenderer farpoint;
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
		velocity = player.transform.forward;
		direction = DrawLine (Vector3.zero, Vector3.zero, Color.blue,veloshader,0.02f);
		nextpoint = DrawLine (Vector3.zero, Vector3.zero, Color.red,dirshader,0.02f);
		farpoint=DrawLine (Vector3.zero, Vector3.zero, Color.red,farshader,1.0f);
		countdownText = GameObject.Find ("countdownText");
		stopWatchText = GameObject.Find ("stopwatch");
		nextpointtext = GameObject.Find ("nextPoint");
		leapspace = player.transform.GetChild (0).transform.GetChild (0).gameObject;
		player.transform.position = checkpointlist [0].transform.position ;
		player.transform.forward = checkpointlist [1].transform.position-player.transform.position;
		velocity = player.transform.forward;
	}
	
	// Update is called once per frame
	void Update () {
		updatevariables ();
		if (!end) {
			countdown (freezecounter);
			stopwatch (freezecounter);
		}
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
		leaporigin = leapspace.transform.position;
		cameraforward = leapspace.transform.forward;
		linestart = leaporigin + cameraforward;
		if (!end) {
			nextpoint.SetPosition (0, linestart);
			nextpoint.SetPosition (1, checkpointlist [currentpoint].transform.position);
		}if (currentpoint < checkpointlist.Count - 1) {
			farpoint.SetPosition (0, checkpointlist [currentpoint].transform.position);
			farpoint.SetPosition (1, checkpointlist [currentpoint+1].transform.position);
		}
	}

	void countdown(float cliptime){
		if (timer < cliptime) {
			countdownText.SetActive (true);
			countdownText.GetComponent<UnityEngine.UI.Text> ().text = "Count down: " +System.Environment.NewLine+ (freezecounter - Mathf.Floor (timer));
			countdownText.transform.position = linestart;
			countdownText.transform.forward = cameraforward;
		} else {
			countdownText.SetActive (false);
		}
	}

	void stopwatch(float cliptime){
		if (timer > cliptime) {
			stopWatchText.SetActive (true);
			stopWatchText.GetComponent<UnityEngine.UI.Text> ().text = currentpoint+"/"+checkpointlist.Count+System.Environment.NewLine+Mathf.Floor (totaltime).ToString()+"S";
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
			Camera c = leapspace.GetComponent<Camera> ();
			Matrix4x4 m= c.cameraToWorldMatrix;
			Vector3 rightvect =  Vector3.Normalize(leapToUnity (righthand.PalmPosition / 1000.0f));
			float deg = Vector3.Dot (preRightvect, rightvect);
			Vector3 axis = Vector3.Normalize(Vector3.Cross (preRightvect,rightvect));
			Vector4 tmp = m*new Vector4 (axis.x,axis.y,-axis.z, 0);
			axis.x = tmp.x;
			axis.y = tmp.y;
			axis.z = tmp.z;
			if (deg < 0.999 && isFist (righthand)) {
				velocity = Quaternion.AngleAxis (deg * 3.5f, axis) * velocity;
				preRightvect = rightvect;
				pinchframe = 0;
			} else if (!isFist (righthand) && righthand.PinchStrength > 0.94) {
				pinchframe++;
				if (pinchframe > 4) {
					Vector3 temp2 = Vector3.Normalize (leapToUnity (righthand.PalmPosition / 1000.0f));
					Vector4 res = m*new Vector4 (temp2.x, temp2.y, -temp2.z, 0);
					temp2.x = res.x;
					temp2.y = res.y;
					temp2.z = res.z;
					velocity = Vector3.Normalize(temp2);
					velocity = 0.5f * velocity + 0.5f * cameraforward;
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
			if(speed<1){
				speed=1;
			}
			speed += 0.95f*leapToUnity (lefthand.PalmPosition / 1000.0f).magnitude;
			if (isFist(lefthand)) {
				speed = 0;
			}
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
		nextpointtext.GetComponent<UnityEngine.UI.Text>().text = Mathf.Floor(dist).ToString ()+" Meters";	
		nextpointtext.transform.position = point.transform.position+Vector3.up*dist/10.0f;
		nextpointtext.transform.forward = cameraforward;
		nextpointtext.transform.localScale = new Vector3 (dist / 200.0f,dist / 200.0f,dist / 200.0f);
		if (dist < 9.4488f) {
			checkpointlist [currentpoint].SetActive (false);
			AudioSource arrive;
			if (currentpoint == 0) {
				arrive = player.GetComponents<AudioSource> () [1];
			} else if (currentpoint == checkpointlist.Count-1) {
				arrive = player.GetComponents<AudioSource> () [2];
			} else {
				arrive= player.GetComponents<AudioSource> ()[0];
			}
			arrive.Play ();
			currentpoint++;
			if (currentpoint == checkpointlist.Count) {
				gameEnd ();
			}
		}
	}

	void gameEnd(){
		end = true;
		stopWatchText.GetComponent<UnityEngine.UI.Text> ().text = "Total Time: " + System.Environment.NewLine + Mathf.Floor (totaltime)+"S";
		nextpointtext.SetActive (false);
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

	LineRenderer DrawLine(Vector3 start, Vector3 end,Color color,Shader shad,float width)
	{
		GameObject myline = new GameObject ();
		myline.transform.position = start;
		myline.AddComponent<LineRenderer> ();
		LineRenderer lr = myline.GetComponent<LineRenderer> ();
		lr.material = new Material (shad);
		lr.SetWidth (width, width);
		lr.positionCount = 2;
		lr.SetPosition (0, start);
		lr.SetPosition (1, end);
		return lr;
	}

	void OnTriggerEnter(Collider col) {
		if (col.gameObject.tag != "checkpoint") {
			freezecounter = 3;
			totaltime += 3;
			timer = 0;
		}
		player.transform.position += Vector3.up;
	}
}
