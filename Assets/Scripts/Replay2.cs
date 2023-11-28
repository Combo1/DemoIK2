using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class Replay2 : MonoBehaviour
{
	public GameObject LeftController;
	public GameObject RightController;
	public GameObject HMD;

	StreamReader sr = new StreamReader("Recording.csv");

	// Start is called before the first frame update
	void Start()
	{
	}

	// Update is called once per frame
	void Update()
	{
		//Replay functionality
		string line = sr.ReadLine();
		string[] subs = line.Split(',');
		Vector3 pos = new Vector3(float.Parse(subs[1]), float.Parse(subs[2]), float.Parse(subs[3]));
		HMD.transform.position = pos;

		line = sr.ReadLine();
		subs = line.Split(',');
		Quaternion rot = new Quaternion(float.Parse(subs[1]), float.Parse(subs[2]), float.Parse(subs[3]), float.Parse(subs[4]));
		HMD.transform.rotation = rot;

		line = sr.ReadLine();
		subs = line.Split(',');
		pos = new Vector3(float.Parse(subs[1]), float.Parse(subs[2]), float.Parse(subs[3]));
		LeftController.transform.position = pos;

		line = sr.ReadLine();
		subs = line.Split(',');
		rot = new Quaternion(float.Parse(subs[1]), float.Parse(subs[2]), float.Parse(subs[3]), float.Parse(subs[4]));
		LeftController.transform.rotation = rot;

		line = sr.ReadLine();
		subs = line.Split(',');
		pos = new Vector3(float.Parse(subs[1]), float.Parse(subs[2]), float.Parse(subs[3]));
		RightController.transform.position = pos;

		line = sr.ReadLine();
		subs = line.Split(',');
		rot = new Quaternion(float.Parse(subs[1]), float.Parse(subs[2]), float.Parse(subs[3]), float.Parse(subs[4]));
		RightController.transform.rotation = rot;
	}
}
