using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class MotionRecorderToCsv : MonoBehaviour
{

    public Transform headPosition;
    public Transform leftHandPosition;
    public Transform rightHandPosition;
    public string csvFileName;
    private StreamWriter writer;

    void Start()
    {
        writer = new StreamWriter(csvFileName);
    }

    void Update()
    {
        //Write position and rotation
        writer.WriteLine(Time.time + "," + headPosition.position.x + "," + headPosition.position.y + "," + headPosition.position.z);
        writer.WriteLine(Time.time + "," + headPosition.rotation.x + "," + headPosition.rotation.y + "," + headPosition.rotation.z + "," + headPosition.rotation.w);
        writer.WriteLine(Time.time + "," + leftHandPosition.position.x + "," + leftHandPosition.position.y + "," + leftHandPosition.position.z);
        writer.WriteLine(Time.time + "," + leftHandPosition.rotation.x + "," + leftHandPosition.rotation.y + "," + leftHandPosition.rotation.z + "," + leftHandPosition.rotation.w);
        writer.WriteLine(Time.time + "," + rightHandPosition.position.x + "," + rightHandPosition.position.y + "," + rightHandPosition.position.z);
        writer.WriteLine(Time.time + "," + rightHandPosition.rotation.x + "," + rightHandPosition.rotation.y + "," + rightHandPosition.rotation.z + "," + rightHandPosition.rotation.w);
    }

    void OnApplicationQuit()
    {
        writer.Close();
    }

}
