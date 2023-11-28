using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Valve.Newtonsoft.Json;

public class VisAMASS : MonoBehaviour
{
    public float[] Rodrigues_joints;
    public NetworkingAvatarPoser NAP;
    public int frameCount = 0;
    // Start is called before the first frame update
    void Start()
    {
        NAP = this.GetComponent<NetworkingAvatarPoser>();
        Rodrigues_joints = LoadJson();
    }

    // Update is called once per frame
    void Update()
    {
        PoseAvatar(Rodrigues_joints[(189 * frameCount)..(189 * (frameCount+1))]); //skip to frame n
        frameCount++;
    }

    public void PoseAvatar(float[] Rodrigues_joints)
    {
        NAP.references.spine.transform.localRotation = RotationMatrixToQuaternion(Rodrigues_joints, 2);
        NAP.references.chest.transform.localRotation = RotationMatrixToQuaternion(Rodrigues_joints, 8);
        NAP.references.neck.transform.localRotation = RotationMatrixToQuaternion(Rodrigues_joints, 11);
        NAP.references.head.transform.localRotation = RotationMatrixToQuaternion(Rodrigues_joints, 14);

        NAP.references.leftShoulder.transform.localRotation = RotationMatrixToQuaternion(Rodrigues_joints, 12);
        NAP.references.leftUpperArm.transform.localRotation = RotationMatrixToQuaternion(Rodrigues_joints, 15);
        NAP.references.leftForearm.transform.localRotation = RotationMatrixToQuaternion(Rodrigues_joints, 17);
        NAP.references.leftHand.transform.localRotation = RotationMatrixToQuaternion(Rodrigues_joints, 19);

        NAP.references.rightShoulder.transform.localRotation = RotationMatrixToQuaternion(Rodrigues_joints, 13);
        NAP.references.rightUpperArm.transform.localRotation = RotationMatrixToQuaternion(Rodrigues_joints, 16);
        NAP.references.rightForearm.transform.localRotation = RotationMatrixToQuaternion(Rodrigues_joints, 18);
        NAP.references.rightHand.transform.localRotation = RotationMatrixToQuaternion(Rodrigues_joints, 20);

        NAP.references.leftThigh.transform.localRotation = RotationMatrixToQuaternion(Rodrigues_joints, 0);
        NAP.references.leftCalf.transform.localRotation = RotationMatrixToQuaternion(Rodrigues_joints, 3);
        NAP.references.leftFoot.transform.localRotation = RotationMatrixToQuaternion(Rodrigues_joints, 6);
        NAP.references.leftToes.transform.localRotation = RotationMatrixToQuaternion(Rodrigues_joints, 9);

        NAP.references.rightThigh.transform.localRotation = RotationMatrixToQuaternion(Rodrigues_joints, 1);
        NAP.references.rightCalf.transform.localRotation = RotationMatrixToQuaternion(Rodrigues_joints, 4);
        NAP.references.rightFoot.transform.localRotation = RotationMatrixToQuaternion(Rodrigues_joints, 7);
        NAP.references.rightToes.transform.localRotation = RotationMatrixToQuaternion(Rodrigues_joints, 10);
    }

    static Quaternion RotationMatrixToQuaternion(float[] rotation, int joint)
    {
        return Quaternion.LookRotation(
            new Vector3(-rotation[2 + 9 * joint], rotation[5 + 9 * joint], rotation[8 + 9 * joint]),
            new Vector3(-rotation[1 + 9 * joint], rotation[4 + 9 * joint], rotation[7 + 9 * joint])
        );
    }



    public float[] LoadJson()
    {
        using (StreamReader r = new StreamReader("25.json"))
        {
            string json = r.ReadToEnd();
            Item items = JsonConvert.DeserializeObject<Item>(json);
            return(items.Rodrigues_joints);
        }
    }
}

public class Item
{
    public float[] Rodrigues_joints;
}