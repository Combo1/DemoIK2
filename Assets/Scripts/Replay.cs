using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class Replay : MonoBehaviour
{
    public GameObject LeftIKTarget;
    public GameObject RightIKTarget;
    public GameObject LookAtIKTarget;

	StreamReader sr = new StreamReader("Recording.csv");

	// Start is called before the first frame update
	void Start()
    {
        LeftIKTarget = this.transform.GetChild(0).gameObject;
        RightIKTarget = this.transform.GetChild(1).gameObject;
        LookAtIKTarget = this.transform.GetChild(2).gameObject;
    }

    // Update is called once per frame
    void Update()
    {
		//Replay functionality
		string line = sr.ReadLine();
		string[] subs = line.Split(',');
		Vector3 pos = new Vector3(float.Parse(subs[1]), float.Parse(subs[2]), float.Parse(subs[3]));
		LookAtIKTarget.transform.position = pos;

		line = sr.ReadLine();
		subs = line.Split(',');
		Quaternion rot = new Quaternion(float.Parse(subs[1]), float.Parse(subs[2]), float.Parse(subs[3]), float.Parse(subs[4]));
		LookAtIKTarget.transform.rotation = rot;

		line = sr.ReadLine();
		subs = line.Split(',');
		pos = new Vector3(float.Parse(subs[1]), float.Parse(subs[2]), float.Parse(subs[3]));
		LeftIKTarget.transform.position = pos;

		line = sr.ReadLine();
		subs = line.Split(',');
		rot = new Quaternion(float.Parse(subs[1]), float.Parse(subs[2]), float.Parse(subs[3]), float.Parse(subs[4]));
		LeftIKTarget.transform.rotation = rot;

		line = sr.ReadLine();
		subs = line.Split(',');
		pos = new Vector3(float.Parse(subs[1]), float.Parse(subs[2]), float.Parse(subs[3]));
		RightIKTarget.transform.position = pos;

		line = sr.ReadLine();
		subs = line.Split(',');
		rot = new Quaternion(float.Parse(subs[1]), float.Parse(subs[2]), float.Parse(subs[3]), float.Parse(subs[4]));
		RightIKTarget.transform.rotation = rot;
	}
}
