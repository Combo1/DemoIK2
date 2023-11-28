using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test_Rotation_LeftHand : MonoBehaviour
{
    public GameObject leftController;
    public GameObject leftHand;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //Multiply the rotation by the rotation of the coordinate system
        //transform.rotation = leftController.transform.rotation * Quaternion.Euler(0.135f, 118.178f, -128.216f);
        transform.rotation = leftController.transform.rotation;
    }

    //Correct result! 
    public static Quaternion QuaternionFromMatrix(Matrix4x4 m)
    {
        // Adapted from: http://www.euclideanspace.com/maths/geometry/rotations/conversions/matrixToQuaternion/index.htm
        Quaternion q = new Quaternion();
        q.w = Mathf.Sqrt(Mathf.Max(0, 1 + m[0, 0] + m[1, 1] + m[2, 2])) / 2;
        q.x = Mathf.Sqrt(Mathf.Max(0, 1 + m[0, 0] - m[1, 1] - m[2, 2])) / 2;
        q.y = Mathf.Sqrt(Mathf.Max(0, 1 - m[0, 0] + m[1, 1] - m[2, 2])) / 2;
        q.z = Mathf.Sqrt(Mathf.Max(0, 1 - m[0, 0] - m[1, 1] + m[2, 2])) / 2;
        q.x *= Mathf.Sign(q.x * (m[2, 1] - m[1, 2]));
        q.y *= Mathf.Sign(q.y * (m[0, 2] - m[2, 0]));
        q.z *= Mathf.Sign(q.z * (m[1, 0] - m[0, 1]));
        return q;
    }


    
    private static Matrix4x4 QuaternionToRotationMatrix(Quaternion q)
    {
        //Covert a quaternion into a full three-dimensional rotation matrix.

        //Input
        //:param Q: A 4 element array representing the quaternion(q0, q1, q2, q3)


        //Output
        //:return: A 3x3 element matrix representing the full 3D rotation matrix.
        //         This rotation matrix converts a point in the local reference
        //         frame to a point in the global reference frame.
        // Extract the values from Q
        float s = q.w;
        float x = q.x;
        float y = q.y;
        float z = q.z;

        // First row of the rotation matrix
        float r00 = 1 - 2 * y * y - 2 * z * z;
        float r01 = 2 * x * y - 2 * s * z;
        float r02 = 2 * x * z + 2 * s * y;

        // Second row of the rotation matrix
        float r10 = 2 * x * y + 2 * s * z;
        float r11 = 1 - 2 * x * x - 2 * z * z;
        float r12 = 2 * y * z - 2 * s * x;

        // Third row of the rotation matrix
        float r20 = 2 * x * z - 2 * s * y;
        float r21 = 2 * y * z + 2 * s * x;
        float r22 = 1 - 2 * x * x - 2 * y * y;

        // 4x4 rotation matrix
        Matrix4x4 rot_matrix = new Matrix4x4(new Vector4(r00, r10, r20, 0), new Vector4(r01, r11, r21, 0), new Vector4(r02, r12, r22, 0), new Vector4(0, 0, 0, 1));

        return rot_matrix;
    }

    private static Quaternion ChangeCoordinateSystem(Quaternion q)
    {
        return new Quaternion(q.x, q.z, q.y, q.w);
    }
}
