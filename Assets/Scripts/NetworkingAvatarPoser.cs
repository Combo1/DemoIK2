using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using System.Linq;
using RootMotion;
using System.Diagnostics;
using System.Threading;
using Unity.Jobs;
using Unity.Burst;

public class NetworkingAvatarPoser : MonoBehaviour
{
    private string json_result;
    public References references = new References();
    private PoseResult poseResult;

    public GameObject head;
    public GameObject leftController;
    public GameObject rightController;

    private Vector3 prevHead;
    private Vector3 prevLeftHand;
    private Vector3 prevRightHand;

    private Matrix4x4 prevRotationHead;
    private Matrix4x4 prevRotationLeftHand;
    private Matrix4x4 prevRotationRightHand;

    private Vector3 prevVeloHead;
    private Vector3 prevVeloLeftHand;
    private Vector3 prevVeloRightHand;

    private double[][] prevRotVeloHead;
    private double[][] prevRotVeloLeftHand;
    private double[][] prevRotVeloRightHand;

    public GameObject[] spherels;

    [SerializeField]
    public string url = "http://127.0.0.1:8080";

    // Update is called once per frame
    void Start()
    {

        references.pelvisRot = references.pelvis.rotation.normalized;
        references.spineRot = references.spine.rotation.normalized;
        references.chestRot = references.chest.rotation.normalized;
        references.neckRot = references.neck.rotation.normalized;
        references.headRot = references.head.rotation.normalized;
        references.leftShoulderRot = references.leftShoulder.rotation.normalized;
        references.leftUpperArmRot = references.leftUpperArm.rotation.normalized;
        references.leftForearmRot = references.leftForearm.rotation.normalized;
        references.leftHandRot = references.leftHand.rotation.normalized;
        references.rightShoulderRot = references.rightShoulder.rotation.normalized;
        references.rightUpperArmRot = references.rightUpperArm.rotation.normalized;
        references.rightForearmRot = references.rightForearm.rotation.normalized;
        references.rightHandRot = references.rightHand.rotation.normalized;
        references.leftThighRot = references.leftThigh.rotation.normalized;
        references.leftCalfRot = references.leftCalf.rotation.normalized;
        references.leftFootRot = references.leftFoot.rotation.normalized;
        references.leftToesRot = references.leftToes.rotation.normalized;
        references.rightThighRot = references.rightThigh.rotation.normalized;
        references.rightCalfRot = references.rightCalf.rotation.normalized;
        references.rightFootRot = references.rightFoot.rotation.normalized;
        references.rightToesRot = references.rightToes.rotation.normalized;
        /*
        references.pelvisRot = references.pelvis.localRotation.normalized;
        references.spineRot = references.spine.localRotation.normalized;
        references.chestRot = references.chest.localRotation.normalized;
        references.neckRot = references.neck.localRotation.normalized;
        references.headRot = references.head.localRotation.normalized;
        references.leftShoulderRot = references.leftShoulder.localRotation.normalized;
        references.leftUpperArmRot = references.leftUpperArm.localRotation.normalized;
        references.leftForearmRot = references.leftForearm.localRotation.normalized;
        references.leftHandRot = references.leftHand.localRotation.normalized;
        references.rightShoulderRot = references.rightShoulder.localRotation.normalized;
        references.rightUpperArmRot = references.rightUpperArm.localRotation.normalized;
        references.rightForearmRot = references.rightForearm.localRotation.normalized;
        references.rightHandRot = references.rightHand.localRotation.normalized;
        references.leftThighRot = references.leftThigh.localRotation.normalized;
        references.leftCalfRot = references.leftCalf.localRotation.normalized;
        references.leftFootRot = references.leftFoot.localRotation.normalized;
        references.leftToesRot = references.leftToes.localRotation.normalized;
        references.rightThighRot = references.rightThigh.localRotation.normalized;
        references.rightCalfRot = references.rightCalf.localRotation.normalized;
        references.rightFootRot = references.rightFoot.localRotation.normalized;
        references.rightToesRot = references.rightToes.localRotation.normalized;
        */


        prevHead = head.transform.position;
        prevLeftHand = leftController.transform.position;
        prevRightHand = rightController.transform.position;

        Quaternion q = head.transform.rotation;
        Quaternion q2 = leftController.transform.rotation;
        Quaternion q3 = rightController.transform.rotation;

        prevRotVeloHead = new double[3][] {
            new double[3]{ 0.0, 0.0, 0.0},
            new double[3]{ 0.0, 0.0, 0.0},
            new double[3]{ 0.0, 0.0, 0.0},
        };
        prevRotVeloLeftHand = new double[3][] {
            new double[3]{ 0.0, 0.0, 0.0},
            new double[3]{ 0.0, 0.0, 0.0},
            new double[3]{ 0.0, 0.0, 0.0},
        };
        prevRotVeloRightHand = new double[3][] {
            new double[3]{ 0.0, 0.0, 0.0},
            new double[3]{ 0.0, 0.0, 0.0},
            new double[3]{ 0.0, 0.0, 0.0},
        };

        prevVeloHead = new Vector3(0.0f, 0.0f, 0.0f);
        prevVeloLeftHand = new Vector3(0.0f, 0.0f, 0.0f);
        prevVeloRightHand = new Vector3(0.0f, 0.0f, 0.0f);

        prevRotationHead = QuaternionToRotationMatrix(q); // ChangeCoordinateSystem(q));//new Quaternion(q.z, q.x, q.y, q.w)); //head.transform.localToWorldMatrix;
        prevRotationLeftHand = QuaternionToRotationMatrix(q2); // ChangeCoordinateSystem(q2));//new Quaternion(q2.z, q2.x, q2.y, q2.w));//leftHand.transform.localToWorldMatrix;
        prevRotationRightHand = QuaternionToRotationMatrix(q3); // ChangeCoordinateSystem(q3));//new Quaternion(q3.z, q3.x, q3.y, q3.w)); //rightHand.transform.localToWorldMatrix;

        //Set x, z position of the avatar to the head position


    }

    private void Update()
    {
        Quaternion q = head.transform.rotation;
        Quaternion q2 = leftController.transform.rotation;
        Quaternion q3 = rightController.transform.rotation;

        Matrix4x4 rotationMatrixHead = QuaternionToRotationMatrix(q);//ChangeCoordinateSystem(q)); //head.transform.localToWorldMatrix; //seemed wrong, 1. we don't take into account initial orientation, 2. the rotation matrix does not represent the rotation of the transform but a matrix that transforms a point from local space into world space (Read Only).
        Matrix4x4 rotationMatrixLeftHand = QuaternionToRotationMatrix(q2);// ChangeCoordinateSystem(q2));//leftHand.transform.localToWorldMatrix;
        Matrix4x4 rotationMatrixRightHand = QuaternionToRotationMatrix(q3); // ChangeCoordinateSystem(q3)); //rightHand.transform.localToWorldMatrix;

        //R3x3: rotation matrix, changed data order
        
        double[][] mHead = new double[][] { new double[] { rotationMatrixHead.m00, rotationMatrixHead.m01, rotationMatrixHead.m02 }, new double[] { rotationMatrixHead.m20, rotationMatrixHead.m22, rotationMatrixHead.m21 }, new double[] { rotationMatrixHead.m10, rotationMatrixHead.m12, rotationMatrixHead.m11 } }; 
        double[][] mLeftHand = new double[][] { new double[] { rotationMatrixLeftHand.m00, rotationMatrixLeftHand.m01, rotationMatrixLeftHand.m02 }, new double[] { rotationMatrixLeftHand.m20, rotationMatrixLeftHand.m22, rotationMatrixLeftHand.m21 }, new double[] { rotationMatrixLeftHand.m10, rotationMatrixLeftHand.m12, rotationMatrixLeftHand.m11 } };
        double[][] mRightHand = new double[][] { new double[] { rotationMatrixRightHand.m00, rotationMatrixRightHand.m01, rotationMatrixRightHand.m02 }, new double[] { rotationMatrixRightHand.m20, rotationMatrixRightHand.m22, rotationMatrixRightHand.m21 }, new double[] { rotationMatrixRightHand.m10, rotationMatrixRightHand.m12, rotationMatrixRightHand.m11 } };

        //R_{t-1} 3x3: rotation matrix from one frame before, changed data order
        double[][] mPrevHead = new double[][] { new double[] { prevRotationHead.m00, prevRotationHead.m01, prevRotationHead.m02 }, new double[] { prevRotationHead.m20, prevRotationHead.m22, prevRotationHead.m21 }, new double[] { prevRotationHead.m10, prevRotationHead.m12, prevRotationHead.m11 } };
        double[][] mPrevLeftHand = new double[][] { new double[] { prevRotationLeftHand.m00, prevRotationLeftHand.m01, prevRotationLeftHand.m02 }, new double[] { prevRotationLeftHand.m20, prevRotationLeftHand.m22, prevRotationLeftHand.m21 }, new double[] { prevRotationLeftHand.m10, prevRotationLeftHand.m12, prevRotationLeftHand.m11 } };
        double[][] mPrevRightHand = new double[][] { new double[] { prevRotationRightHand.m00, prevRotationRightHand.m01, prevRotationRightHand.m02 }, new double[] { prevRotationRightHand.m20, prevRotationRightHand.m22, prevRotationRightHand.m21 }, new double[] { prevRotationRightHand.m10, prevRotationRightHand.m12, prevRotationRightHand.m11 } };

        /*
        double[][] mHead = new double[][] { new double[] { rotationMatrixHead.m00, rotationMatrixHead.m01, rotationMatrixHead.m02 }, new double[] { rotationMatrixHead.m10, rotationMatrixHead.m11, rotationMatrixHead.m12 }, new double[] { rotationMatrixHead.m20, rotationMatrixHead.m21, rotationMatrixHead.m22 } };
        double[][] mLeftHand = new double[][] { new double[] { rotationMatrixLeftHand.m00, rotationMatrixLeftHand.m01, rotationMatrixLeftHand.m02 }, new double[] { rotationMatrixLeftHand.m10, rotationMatrixLeftHand.m11, rotationMatrixLeftHand.m12 }, new double[] { rotationMatrixLeftHand.m20, rotationMatrixLeftHand.m21, rotationMatrixLeftHand.m22 } };
        double[][] mRightHand = new double[][] { new double[] { rotationMatrixRightHand.m00, rotationMatrixRightHand.m01, rotationMatrixRightHand.m02 }, new double[] { rotationMatrixRightHand.m10, rotationMatrixRightHand.m11, rotationMatrixRightHand.m12 }, new double[] { rotationMatrixRightHand.m20, rotationMatrixRightHand.m21, rotationMatrixRightHand.m22 } };

        //R_{t-1} 3x3: rotation matrix from one frame before, changed data order
        double[][] mPrevHead = new double[][] { new double[] { prevRotationHead.m00, prevRotationHead.m01, prevRotationHead.m02 }, new double[] { prevRotationHead.m10, prevRotationHead.m11, prevRotationHead.m12 }, new double[] { prevRotationHead.m20, prevRotationHead.m21, prevRotationHead.m22 } };
        double[][] mPrevLeftHand = new double[][] { new double[] { prevRotationLeftHand.m00, prevRotationLeftHand.m01, prevRotationLeftHand.m02 }, new double[] { prevRotationLeftHand.m10, prevRotationLeftHand.m11, prevRotationLeftHand.m12 }, new double[] { prevRotationLeftHand.m20, prevRotationLeftHand.m21, prevRotationLeftHand.m22 } };
        double[][] mPrevRightHand = new double[][] { new double[] { prevRotationRightHand.m00, prevRotationRightHand.m01, prevRotationRightHand.m02 }, new double[] { prevRotationRightHand.m10, prevRotationRightHand.m11, prevRotationRightHand.m12 }, new double[] { prevRotationRightHand.m20, prevRotationRightHand.m21, prevRotationRightHand.m22 } };
        */

        //R_{t-1}^{-1}
        //Invert the previous time step's rotation matrix
        mPrevHead = MatrixInverse(mPrevHead); //Works! Verified!
        mPrevLeftHand = MatrixInverse(mPrevLeftHand);
        mPrevRightHand = MatrixInverse(mPrevRightHand);

        //Multiply the previous time step's rotation matrix with the current time step rotation matrix
        double[][] rotvelHead = MatrixProduct(mPrevHead, mHead); //correct! Verified!
        double[][] rotvelLeftHand = MatrixProduct(mPrevLeftHand, mLeftHand);
        double[][] rotvelRightHand = MatrixProduct(mPrevRightHand, mRightHand);


        //Write dots instead of commas for floats
        System.Globalization.CultureInfo customCulture = (System.Globalization.CultureInfo) System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
        customCulture.NumberFormat.NumberDecimalSeparator = ".";
        System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;
        //1. write a struct for data
        //2. Serialize the struct
        //3. Send struct through GetRequestWithBody
        /*
        PoseData poseData = new PoseData(new float[108] //Replace with real pose data this.pose = pose, this.head_global_trans_list = head_global_trans_list;
        {
            rotationMatrixHead.m00, rotationMatrixHead.m10, rotationMatrixHead.m20, rotationMatrixHead.m01, rotationMatrixHead.m11, rotationMatrixHead.m21, rotationMatrixLeftHand.m00, rotationMatrixLeftHand.m10, rotationMatrixLeftHand.m20, rotationMatrixLeftHand.m01, rotationMatrixLeftHand.m11, rotationMatrixLeftHand.m21, rotationMatrixRightHand.m00, rotationMatrixRightHand.m10, rotationMatrixRightHand.m20, rotationMatrixRightHand.m01, rotationMatrixRightHand.m11, rotationMatrixRightHand.m21, //rotation (00, 10, 20, 01, 11, 21)
                (float) rotvelHead[0][0], (float) rotvelHead[1][0], (float) rotvelHead[2][0], (float) rotvelHead[0][1], (float)rotvelHead[1][1], (float)rotvelHead[2][1], (float)rotvelLeftHand[0][0], (float)rotvelLeftHand[1][0], (float)rotvelLeftHand[2][0], (float)rotvelLeftHand[0][1], (float)rotvelLeftHand[1][1] , (float)rotvelLeftHand[2][1], (float)rotvelRightHand[0][0], (float)rotvelRightHand[1][0], (float)rotvelRightHand[2][0], (float)rotvelRightHand[0][1], (float)rotvelRightHand[1][1], (float)rotvelRightHand[2][1], //rotational velocity (00, 10, 20, 01, 11, 21) of the final rotation velocity matrix
                head.transform.position.x, head.transform.position.z, head.transform.position.y, leftController.transform.position.x, leftController.transform.position.z, leftController.transform.position.y, rightController.transform.position.x, rightController.transform.position.z, rightController.transform.position.y, //head.transform.position.z + ";" + head.transform.position.x + ";" + head.transform.position.y + ";" + leftHand.transform.position.z + ";" + leftHand.transform.position.x + ";" + leftHand.transform.position.y + ";" + rightHand.transform.position.z + ";" + rightHand.transform.position.x + ";" + rightHand.transform.position.y + ";" + //position //head.transform.position.x + ";" + head.transform.position.y + ";" + head.transform.position.z + ";" + leftHand.transform.position.x + ";" + leftHand.transform.position.y + ";" + leftHand.transform.position.z + ";" + rightHand.transform.position.x + ";" + rightHand.transform.position.y + ";" + rightHand.transform.position.z + ";" + 
                (head.transform.position.x - prevHead.x), (head.transform.position.z - prevHead.z), (head.transform.position.y - prevHead.y), (leftController.transform.position.x - prevLeftHand.x), (leftController.transform.position.z - prevLeftHand.z), (leftController.transform.position.y - prevLeftHand.y), (rightController.transform.position.x - prevRightHand.x), (rightController.transform.position.z - prevRightHand.z), (rightController.transform.position.y - prevRightHand.y), //velocity//(head.transform.position.x - prevHead.x) + ";" + (head.transform.position.y - prevHead.y) + ";" + (head.transform.position.z - prevHead.z) + ";" + (leftHand.transform.position.x - prevLeftHand.x) + ";" + (leftHand.transform.position.y - prevLeftHand.y) + ";" + (leftHand.transform.position.z - prevLeftHand.z) + ";" + (rightHand.transform.position.x - prevRightHand.x) + ";" + (rightHand.transform.position.y - prevRightHand.y) + ";" + (rightHand.transform.position.z - prevRightHand.z) //velocity
            //To delete
            rotationMatrixHead.m00, rotationMatrixHead.m10, rotationMatrixHead.m20, rotationMatrixHead.m01, rotationMatrixHead.m11, rotationMatrixHead.m21, rotationMatrixLeftHand.m00, rotationMatrixLeftHand.m10, rotationMatrixLeftHand.m20, rotationMatrixLeftHand.m01, rotationMatrixLeftHand.m11, rotationMatrixLeftHand.m21, rotationMatrixRightHand.m00, rotationMatrixRightHand.m10, rotationMatrixRightHand.m20, rotationMatrixRightHand.m01, rotationMatrixRightHand.m11, rotationMatrixRightHand.m21, //rotation (00, 10, 20, 01, 11, 21)
                (float) rotvelHead[0][0], (float) rotvelHead[1][0], (float) rotvelHead[2][0], (float) rotvelHead[0][1], (float)rotvelHead[1][1], (float)rotvelHead[2][1], (float)rotvelLeftHand[0][0], (float)rotvelLeftHand[1][0], (float)rotvelLeftHand[2][0], (float)rotvelLeftHand[0][1], (float)rotvelLeftHand[1][1] , (float)rotvelLeftHand[2][1], (float)rotvelRightHand[0][0], (float)rotvelRightHand[1][0], (float)rotvelRightHand[2][0], (float)rotvelRightHand[0][1], (float)rotvelRightHand[1][1], (float)rotvelRightHand[2][1], //rotational velocity (00, 10, 20, 01, 11, 21) of the final rotation velocity matrix
                head.transform.position.x, head.transform.position.z, head.transform.position.y, leftController.transform.position.x, leftController.transform.position.z, leftController.transform.position.y, rightController.transform.position.x, rightController.transform.position.z, rightController.transform.position.y, //head.transform.position.z + ";" + head.transform.position.x + ";" + head.transform.position.y + ";" + leftHand.transform.position.z + ";" + leftHand.transform.position.x + ";" + leftHand.transform.position.y + ";" + rightHand.transform.position.z + ";" + rightHand.transform.position.x + ";" + rightHand.transform.position.y + ";" + //position //head.transform.position.x + ";" + head.transform.position.y + ";" + head.transform.position.z + ";" + leftHand.transform.position.x + ";" + leftHand.transform.position.y + ";" + leftHand.transform.position.z + ";" + rightHand.transform.position.x + ";" + rightHand.transform.position.y + ";" + rightHand.transform.position.z + ";" + 
                (head.transform.position.x - prevHead.x), (head.transform.position.z - prevHead.z), (head.transform.position.y - prevHead.y), (leftController.transform.position.x - prevLeftHand.x), (leftController.transform.position.z - prevLeftHand.z), (leftController.transform.position.y - prevLeftHand.y), (rightController.transform.position.x - prevRightHand.x), (rightController.transform.position.z - prevRightHand.z), (rightController.transform.position.y - prevRightHand.y), //velocity//(head.transform.position.x - prevHead.x) + ";" + (head.transform.position.y - prevHead.y) + ";" + (head.transform.position.z - prevHead.z) + ";" + (leftHand.transform.position.x - prevLeftHand.x) + ";" + (leftHand.transform.position.y - prevLeftHand.y) + ";" + (leftHand.transform.position.z - prevLeftHand.z) + ";" + (rightHand.transform.position.x - prevRightHand.x) + ";" + (rightHand.transform.position.y - prevRightHand.y) + ";" + (rightHand.transform.position.z - prevRightHand.z) //velocity
        }, new float[32]
        {
            rotationMatrixHead[0, 0], rotationMatrixHead[0, 1], rotationMatrixHead[0, 2], head.transform.position.x, //head.transform.position.z + ";" +
                rotationMatrixHead[1, 0], rotationMatrixHead[1, 1], rotationMatrixHead[1, 2],head.transform.position.z, //head.transform.position.x + ";" +
                rotationMatrixHead[2, 0], rotationMatrixHead[2, 1], rotationMatrixHead[2, 2], head.transform.position.y, //head.transform.position.y + ";" +
                0,0,0,1,
            //To delete
            rotationMatrixHead[0, 0], rotationMatrixHead[0, 1], rotationMatrixHead[0, 2], head.transform.position.x, //head.transform.position.z + ";" +
                rotationMatrixHead[1, 0], rotationMatrixHead[1, 1], rotationMatrixHead[1, 2],head.transform.position.z, //head.transform.position.x + ";" +
                rotationMatrixHead[2, 0], rotationMatrixHead[2, 1], rotationMatrixHead[2, 2], head.transform.position.y, //head.transform.position.y + ";" +
                0,0,0,1,
        });
        */

        //Play around with this at home! 
        //rotation_global_matrot = local2global_pose(rotation_local_matrot, bm.kintree_table[0].long()) # rotation of joints relative to the origin
        //rotation_velocity_global_6d = utils_transform.matrot2sixd(rotation_velocity_global_matrot.reshape(-1,3,3)).reshape(rotation_velocity_global_matrot.shape[0],-1,6)
        PoseData poseData = new PoseData(new float[108] //Replace with real pose data this.pose = pose, this.head_global_trans_list = head_global_trans_list;
        {
            prevRotationHead.m00, prevRotationHead.m20, prevRotationHead.m10, prevRotationHead.m01, prevRotationHead.m22, prevRotationHead.m12, prevRotationLeftHand.m00, prevRotationLeftHand.m20, prevRotationLeftHand.m10, prevRotationLeftHand.m02, prevRotationLeftHand.m21, prevRotationLeftHand.m11, prevRotationRightHand.m00, prevRotationRightHand.m20, prevRotationRightHand.m10, prevRotationRightHand.m01, prevRotationRightHand.m22, prevRotationRightHand.m12, //rotation (00, 10, 20, 01, 11, 21)
                (float) prevRotVeloHead[0][0], (float) prevRotVeloHead[2][0], (float) prevRotVeloHead[1][0], (float) prevRotVeloHead[0][1], (float)prevRotVeloHead[2][2], (float)prevRotVeloHead[1][2], (float) prevRotVeloLeftHand[0][0], (float)prevRotVeloLeftHand[2][0], (float)prevRotVeloLeftHand[1][0], (float)prevRotVeloLeftHand[0][2], (float)prevRotVeloLeftHand[2][1] , (float)prevRotVeloLeftHand[1][1], (float)prevRotVeloRightHand[0][0], (float)prevRotVeloRightHand[2][0], (float)prevRotVeloRightHand[1][0], (float)prevRotVeloRightHand[0][1], (float)prevRotVeloRightHand[2][2], (float)prevRotVeloRightHand[1][2], //rotational velocity (00, 10, 20, 01, 11, 21) of the final rotation velocity matrix
                prevHead.x, prevHead.z, prevHead.y, prevLeftHand.x, prevLeftHand.z, prevLeftHand.y, prevRightHand.x, prevRightHand.z, prevRightHand.y, //head.transform.position.z + ";" + head.transform.position.x + ";" + head.transform.position.y + ";" + leftHand.transform.position.z + ";" + leftHand.transform.position.x + ";" + leftHand.transform.position.y + ";" + rightHand.transform.position.z + ";" + rightHand.transform.position.x + ";" + rightHand.transform.position.y + ";" + //position //head.transform.position.x + ";" + head.transform.position.y + ";" + head.transform.position.z + ";" + leftHand.transform.position.x + ";" + leftHand.transform.position.y + ";" + leftHand.transform.position.z + ";" + rightHand.transform.position.x + ";" + rightHand.transform.position.y + ";" + rightHand.transform.position.z + ";" + 
                prevVeloHead.x, prevVeloHead.z, prevVeloHead.y, prevVeloLeftHand.x, prevVeloLeftHand.z, prevVeloLeftHand.y, prevVeloRightHand.x, prevVeloLeftHand.z, prevVeloLeftHand.y, //velocity//(head.transform.position.x - prevHead.x) + ";" + (head.transform.position.y - prevHead.y) + ";" + (head.transform.position.z - prevHead.z) + ";" + (leftHand.transform.position.x - prevLeftHand.x) + ";" + (leftHand.transform.position.y - prevLeftHand.y) + ";" + (leftHand.transform.position.z - prevLeftHand.z) + ";" + (rightHand.transform.position.x - prevRightHand.x) + ";" + (rightHand.transform.position.y - prevRightHand.y) + ";" + (rightHand.transform.position.z - prevRightHand.z) //velocity
            //To delete
            rotationMatrixHead.m00, rotationMatrixHead.m20, rotationMatrixHead.m10, rotationMatrixHead.m01, rotationMatrixHead.m22, rotationMatrixHead.m12, rotationMatrixLeftHand.m00, rotationMatrixLeftHand.m20, rotationMatrixLeftHand.m10, rotationMatrixLeftHand.m02, rotationMatrixLeftHand.m21, rotationMatrixLeftHand.m11, rotationMatrixRightHand.m00, rotationMatrixRightHand.m20, rotationMatrixRightHand.m10, rotationMatrixRightHand.m01, rotationMatrixRightHand.m22, rotationMatrixRightHand.m12, //rotation (00, 10, 20, 01, 11, 21)
                (float) rotvelHead[0][0], (float) rotvelHead[2][0], (float) rotvelHead[1][0], (float) rotvelHead[0][1], (float)rotvelHead[2][2], (float)rotvelHead[1][2], (float)rotvelLeftHand[0][0], (float)rotvelLeftHand[2][0], (float)rotvelLeftHand[1][0], (float)rotvelLeftHand[0][2], (float)rotvelLeftHand[2][1] , (float)rotvelLeftHand[1][1], (float)rotvelRightHand[0][0], (float)rotvelRightHand[2][0], (float)rotvelRightHand[1][0], (float)rotvelRightHand[0][1], (float)rotvelRightHand[2][2], (float)rotvelRightHand[1][2], //rotational velocity (00, 10, 20, 01, 11, 21) of the final rotation velocity matrix
                head.transform.position.x, head.transform.position.z, head.transform.position.y, leftController.transform.position.x, leftController.transform.position.z, leftController.transform.position.y, rightController.transform.position.x, rightController.transform.position.z, rightController.transform.position.y, //head.transform.position.z + ";" + head.transform.position.x + ";" + head.transform.position.y + ";" + leftHand.transform.position.z + ";" + leftHand.transform.position.x + ";" + leftHand.transform.position.y + ";" + rightHand.transform.position.z + ";" + rightHand.transform.position.x + ";" + rightHand.transform.position.y + ";" + //position //head.transform.position.x + ";" + head.transform.position.y + ";" + head.transform.position.z + ";" + leftHand.transform.position.x + ";" + leftHand.transform.position.y + ";" + leftHand.transform.position.z + ";" + rightHand.transform.position.x + ";" + rightHand.transform.position.y + ";" + rightHand.transform.position.z + ";" + 
                (head.transform.position.x - prevHead.x), (head.transform.position.z - prevHead.z), (head.transform.position.y - prevHead.y), (leftController.transform.position.x - prevLeftHand.x), (leftController.transform.position.z - prevLeftHand.z), (leftController.transform.position.y - prevLeftHand.y), (rightController.transform.position.x - prevRightHand.x), (rightController.transform.position.z - prevRightHand.z), (rightController.transform.position.y - prevRightHand.y), //velocity//(head.transform.position.x - prevHead.x) + ";" + (head.transform.position.y - prevHead.y) + ";" + (head.transform.position.z - prevHead.z) + ";" + (leftHand.transform.position.x - prevLeftHand.x) + ";" + (leftHand.transform.position.y - prevLeftHand.y) + ";" + (leftHand.transform.position.z - prevLeftHand.z) + ";" + (rightHand.transform.position.x - prevRightHand.x) + ";" + (rightHand.transform.position.y - prevRightHand.y) + ";" + (rightHand.transform.position.z - prevRightHand.z) //velocity
        }, new float[32]
        {
            prevRotationHead[0, 0], prevRotationHead[0, 1], prevRotationHead[0, 2], prevHead.x, //head.transform.position.z + ";" +
                prevRotationHead[2, 0], prevRotationHead[2, 2], prevRotationHead[2, 1], prevHead.z, //head.transform.position.x + ";" +
                prevRotationHead[1, 0], prevRotationHead[1, 2], prevRotationHead[1, 1], prevHead.y, //head.transform.position.y + ";" +
                0,0,0,1,
            //To delete
            rotationMatrixHead[0, 0], rotationMatrixHead[0, 1], rotationMatrixHead[0, 2], head.transform.position.x, //head.transform.position.z + ";" +
                rotationMatrixHead[2, 0], rotationMatrixHead[2, 2], rotationMatrixHead[2, 1], head.transform.position.z, //head.transform.position.x + ";" +
                rotationMatrixHead[1, 0], rotationMatrixHead[1, 2], rotationMatrixHead[1, 1], head.transform.position.y, //head.transform.position.y + ";" +
                0,0,0,1,
                /*
            rotationMatrixHead.m00, rotationMatrixHead.m20, rotationMatrixHead.m10, rotationMatrixHead.m02, rotationMatrixHead.m22, rotationMatrixHead.m12, rotationMatrixLeftHand.m00, rotationMatrixLeftHand.m20, rotationMatrixLeftHand.m10, rotationMatrixLeftHand.m02, rotationMatrixLeftHand.m22, rotationMatrixLeftHand.m12, rotationMatrixRightHand.m00, rotationMatrixRightHand.m20, rotationMatrixRightHand.m10, rotationMatrixRightHand.m02, rotationMatrixRightHand.m22, rotationMatrixRightHand.m12, //rotation (00, 10, 20, 01, 11, 21)
                (float) rotvelHead[0][0], (float) rotvelHead[2][0], (float) rotvelHead[1][0], (float) rotvelHead[0][2], (float)rotvelHead[2][2], (float)rotvelHead[1][2], (float)rotvelLeftHand[0][0], (float)rotvelLeftHand[2][0], (float)rotvelLeftHand[1][0], (float)rotvelLeftHand[0][2], (float)rotvelLeftHand[2][2] , (float)rotvelLeftHand[1][2], (float)rotvelRightHand[0][0], (float)rotvelRightHand[2][0], (float)rotvelRightHand[1][0], (float)rotvelRightHand[0][2], (float)rotvelRightHand[2][2], (float)rotvelRightHand[1][2], //rotational velocity (00, 10, 20, 01, 11, 21) of the final rotation velocity matrix
                head.transform.position.x, head.transform.position.z, head.transform.position.y, leftController.transform.position.x, leftController.transform.position.z, leftController.transform.position.y, rightController.transform.position.x, rightController.transform.position.z, rightController.transform.position.y, //head.transform.position.z + ";" + head.transform.position.x + ";" + head.transform.position.y + ";" + leftHand.transform.position.z + ";" + leftHand.transform.position.x + ";" + leftHand.transform.position.y + ";" + rightHand.transform.position.z + ";" + rightHand.transform.position.x + ";" + rightHand.transform.position.y + ";" + //position //head.transform.position.x + ";" + head.transform.position.y + ";" + head.transform.position.z + ";" + leftHand.transform.position.x + ";" + leftHand.transform.position.y + ";" + leftHand.transform.position.z + ";" + rightHand.transform.position.x + ";" + rightHand.transform.position.y + ";" + rightHand.transform.position.z + ";" + 
                (head.transform.position.x - prevHead.x), (head.transform.position.z - prevHead.z), (head.transform.position.y - prevHead.y), (leftController.transform.position.x - prevLeftHand.x), (leftController.transform.position.z - prevLeftHand.z), (leftController.transform.position.y - prevLeftHand.y), (rightController.transform.position.x - prevRightHand.x), (rightController.transform.position.z - prevRightHand.z), (rightController.transform.position.y - prevRightHand.y), //velocity//(head.transform.position.x - prevHead.x) + ";" + (head.transform.position.y - prevHead.y) + ";" + (head.transform.position.z - prevHead.z) + ";" + (leftHand.transform.position.x - prevLeftHand.x) + ";" + (leftHand.transform.position.y - prevLeftHand.y) + ";" + (leftHand.transform.position.z - prevLeftHand.z) + ";" + (rightHand.transform.position.x - prevRightHand.x) + ";" + (rightHand.transform.position.y - prevRightHand.y) + ";" + (rightHand.transform.position.z - prevRightHand.z) //velocity
            //To delete
            rotationMatrixHead.m00, rotationMatrixHead.m20, rotationMatrixHead.m10, rotationMatrixHead.m02, rotationMatrixHead.m22, rotationMatrixHead.m12, rotationMatrixLeftHand.m00, rotationMatrixLeftHand.m20, rotationMatrixLeftHand.m10, rotationMatrixLeftHand.m02, rotationMatrixLeftHand.m22, rotationMatrixLeftHand.m12, rotationMatrixRightHand.m00, rotationMatrixRightHand.m20, rotationMatrixRightHand.m10, rotationMatrixRightHand.m02, rotationMatrixRightHand.m22, rotationMatrixRightHand.m12, //rotation (00, 10, 20, 01, 11, 21)
                (float) rotvelHead[0][0], (float) rotvelHead[2][0], (float) rotvelHead[1][0], (float) rotvelHead[0][2], (float)rotvelHead[2][2], (float)rotvelHead[1][2], (float)rotvelLeftHand[0][0], (float)rotvelLeftHand[2][0], (float)rotvelLeftHand[1][0], (float)rotvelLeftHand[0][2], (float)rotvelLeftHand[2][2] , (float)rotvelLeftHand[1][2], (float)rotvelRightHand[0][0], (float)rotvelRightHand[2][0], (float)rotvelRightHand[1][0], (float)rotvelRightHand[0][2], (float)rotvelRightHand[2][2], (float)rotvelRightHand[1][2], //rotational velocity (00, 10, 20, 01, 11, 21) of the final rotation velocity matrix
                head.transform.position.x, head.transform.position.z, head.transform.position.y, leftController.transform.position.x, leftController.transform.position.z, leftController.transform.position.y, rightController.transform.position.x, rightController.transform.position.z, rightController.transform.position.y, //head.transform.position.z + ";" + head.transform.position.x + ";" + head.transform.position.y + ";" + leftHand.transform.position.z + ";" + leftHand.transform.position.x + ";" + leftHand.transform.position.y + ";" + rightHand.transform.position.z + ";" + rightHand.transform.position.x + ";" + rightHand.transform.position.y + ";" + //position //head.transform.position.x + ";" + head.transform.position.y + ";" + head.transform.position.z + ";" + leftHand.transform.position.x + ";" + leftHand.transform.position.y + ";" + leftHand.transform.position.z + ";" + rightHand.transform.position.x + ";" + rightHand.transform.position.y + ";" + rightHand.transform.position.z + ";" + 
                (head.transform.position.x - prevHead.x), (head.transform.position.z - prevHead.z), (head.transform.position.y - prevHead.y), (leftController.transform.position.x - prevLeftHand.x), (leftController.transform.position.z - prevLeftHand.z), (leftController.transform.position.y - prevLeftHand.y), (rightController.transform.position.x - prevRightHand.x), (rightController.transform.position.z - prevRightHand.z), (rightController.transform.position.y - prevRightHand.y), //velocity//(head.transform.position.x - prevHead.x) + ";" + (head.transform.position.y - prevHead.y) + ";" + (head.transform.position.z - prevHead.z) + ";" + (leftHand.transform.position.x - prevLeftHand.x) + ";" + (leftHand.transform.position.y - prevLeftHand.y) + ";" + (leftHand.transform.position.z - prevLeftHand.z) + ";" + (rightHand.transform.position.x - prevRightHand.x) + ";" + (rightHand.transform.position.y - prevRightHand.y) + ";" + (rightHand.transform.position.z - prevRightHand.z) //velocity
        }, new float[32]
        {
            rotationMatrixHead[0, 0], rotationMatrixHead[0, 2], rotationMatrixHead[0, 1], head.transform.position.x, //head.transform.position.z + ";" +
                rotationMatrixHead[2, 0], rotationMatrixHead[2, 2], rotationMatrixHead[2, 1], head.transform.position.z, //head.transform.position.x + ";" +
                rotationMatrixHead[1, 0], rotationMatrixHead[1, 2], rotationMatrixHead[1, 1], head.transform.position.y, //head.transform.position.y + ";" +
                0,0,0,1,
            //To delete
            rotationMatrixHead[0, 0], rotationMatrixHead[0, 2], rotationMatrixHead[0, 1], head.transform.position.x, //head.transform.position.z + ";" +
                rotationMatrixHead[2, 0], rotationMatrixHead[2, 2], rotationMatrixHead[2, 1], head.transform.position.z, //head.transform.position.x + ";" +
                rotationMatrixHead[1, 0], rotationMatrixHead[1, 2], rotationMatrixHead[1, 1], head.transform.position.y, //head.transform.position.y + ";" +
                0,0,0,1,
                */
        });
        string json = JsonUtility.ToJson(poseData);

        //Update the previous rotation and position values. Needs to be here, since we update the pose afterwards
        prevHead = head.transform.position;
        prevLeftHand = leftController.transform.position;
        prevRightHand = rightController.transform.position;

        prevRotationHead = rotationMatrixHead;
        prevRotationLeftHand = rotationMatrixLeftHand;
        prevRotationRightHand = rotationMatrixRightHand;

        prevRotVeloHead = rotvelHead;
        prevRotVeloLeftHand = rotvelLeftHand;
        prevRotVeloRightHand = rotvelRightHand;

        prevVeloHead = head.transform.position - prevHead;
        prevVeloLeftHand = leftController.transform.position - prevLeftHand;
        prevVeloRightHand = rightController.transform.position - prevRightHand;



        string result = SendPostRequest(url, json); //Pose gets changed in here
    }

    static double[][] MatrixProduct(double[][] matrixA, double[][] matrixB)
    {
        int aRows = matrixA.Length; int aCols = matrixA[0].Length;
        int bRows = matrixB.Length; int bCols = matrixB[0].Length;
        if (aCols != bRows)
            throw new Exception("Non-conformable matrices in MatrixProduct");

        double[][] result = MatrixCreate(aRows, bCols);

        for (int i = 0; i < aRows; ++i) // each row of A
            for (int j = 0; j < bCols; ++j) // each col of B
                for (int k = 0; k < aCols; ++k) // could use k less-than bRows
                    result[i][j] += matrixA[i][k] * matrixB[k][j];

        return result;
    }

    static double[][] MatrixInverse(double[][] matrix)
    {
        int n = matrix.Length;
        double[][] result = MatrixDuplicate(matrix);

        int[] perm;
        int toggle;
        double[][] lum = MatrixDecompose(matrix, out perm,
          out toggle);
        if (lum == null)
            throw new Exception("Unable to compute inverse");

        double[] b = new double[n];
        for (int i = 0; i < n; ++i)
        {
            for (int j = 0; j < n; ++j)
            {
                if (i == perm[j])
                    b[j] = 1.0;
                else
                    b[j] = 0.0;
            }

            double[] x = HelperSolve(lum, b);

            for (int j = 0; j < n; ++j)
                result[j][i] = x[j];
        }
        return result;
    }

    static double[][] MatrixDuplicate(double[][] matrix)
    {
        // allocates/creates a duplicate of a matrix.
        double[][] result = MatrixCreate(matrix.Length, matrix[0].Length);
        for (int i = 0; i < matrix.Length; ++i) // copy the values
            for (int j = 0; j < matrix[i].Length; ++j)
                result[i][j] = matrix[i][j];
        return result;
    }

    //Taken from: https://stackoverflow.com/questions/46836908/how-to-invert-double-in-c-sharp
    static double[][] MatrixCreate(int rows, int cols)
    {
        double[][] result = new double[rows][];
        for (int i = 0; i < rows; ++i)
            result[i] = new double[cols];
        return result;
    }

    static double[] HelperSolve(double[][] luMatrix, double[] b)
    {
        // before calling this helper, permute b using the perm array
        // from MatrixDecompose that generated luMatrix
        int n = luMatrix.Length;
        double[] x = new double[n];
        b.CopyTo(x, 0);

        for (int i = 1; i < n; ++i)
        {
            double sum = x[i];
            for (int j = 0; j < i; ++j)
                sum -= luMatrix[i][j] * x[j];
            x[i] = sum;
        }

        x[n - 1] /= luMatrix[n - 1][n - 1];
        for (int i = n - 2; i >= 0; --i)
        {
            double sum = x[i];
            for (int j = i + 1; j < n; ++j)
                sum -= luMatrix[i][j] * x[j];
            x[i] = sum / luMatrix[i][i];
        }

        return x;
    }

    static double[][] MatrixDecompose(double[][] matrix, out int[] perm, out int toggle)
    {
        // Doolittle LUP decomposition with partial pivoting.
        // rerturns: result is L (with 1s on diagonal) and U;
        // perm holds row permutations; toggle is +1 or -1 (even or odd)
        int rows = matrix.Length;
        int cols = matrix[0].Length; // assume square
        if (rows != cols)
            throw new Exception("Attempt to decompose a non-square m");

        int n = rows; // convenience

        double[][] result = MatrixDuplicate(matrix);

        perm = new int[n]; // set up row permutation result
        for (int i = 0; i < n; ++i) { perm[i] = i; }

        toggle = 1; // toggle tracks row swaps.
                    // +1 -greater-than even, -1 -greater-than odd. used by MatrixDeterminant

        for (int j = 0; j < n - 1; ++j) // each column
        {
            double colMax = Math.Abs(result[j][j]); // find largest val in col
            int pRow = j;
            //for (int i = j + 1; i less-than n; ++i)
            //{
            //  if (result[i][j] greater-than colMax)
            //  {
            //    colMax = result[i][j];
            //    pRow = i;
            //  }
            //}

            // reader Matt V needed this:
            for (int i = j + 1; i < n; ++i)
            {
                if (Math.Abs(result[i][j]) > colMax)
                {
                    colMax = Math.Abs(result[i][j]);
                    pRow = i;
                }
            }
            // Not sure if this approach is needed always, or not.

            if (pRow != j) // if largest value not on pivot, swap rows
            {
                double[] rowPtr = result[pRow];
                result[pRow] = result[j];
                result[j] = rowPtr;

                int tmp = perm[pRow]; // and swap perm info
                perm[pRow] = perm[j];
                perm[j] = tmp;

                toggle = -toggle; // adjust the row-swap toggle
            }

            // --------------------------------------------------
            // This part added later (not in original)
            // and replaces the 'return null' below.
            // if there is a 0 on the diagonal, find a good row
            // from i = j+1 down that doesn't have
            // a 0 in column j, and swap that good row with row j
            // --------------------------------------------------

            if (result[j][j] == 0.0)
            {
                // find a good row to swap
                int goodRow = -1;
                for (int row = j + 1; row < n; ++row)
                {
                    if (result[row][j] != 0.0)
                        goodRow = row;
                }

                if (goodRow == -1)
                    throw new Exception("Cannot use Doolittle's method");

                // swap rows so 0.0 no longer on diagonal
                double[] rowPtr = result[goodRow];
                result[goodRow] = result[j];
                result[j] = rowPtr;

                int tmp = perm[goodRow]; // and swap perm info
                perm[goodRow] = perm[j];
                perm[j] = tmp;

                toggle = -toggle; // adjust the row-swap toggle
            }
            // --------------------------------------------------
            // if diagonal after swap is zero . .
            //if (Math.Abs(result[j][j]) less-than 1.0E-20) 
            //  return null; // consider a throw

            for (int i = j + 1; i < n; ++i)
            {
                result[i][j] /= result[j][j];
                for (int k = j + 1; k < n; ++k)
                {
                    result[i][k] -= result[i][j] * result[j][k];
                }
            }


        } // main j column loop

        return result;
    }
    private static Quaternion ChangeCoordinateSystem(Quaternion q)
    {
        return new Quaternion(q.x, q.z, q.y, q.w);
    }



    //Quaternion needs to be normalized
    //Formula: https://www.songho.ca/opengl/gl_quaternion.html
    //Correct!
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

    public string SendPostRequest(string url, string _json)
    {
        float timeNow = Time.realtimeSinceStartup;
        var request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(_json);
        request.uploadHandler = (UploadHandler) new UploadHandlerRaw(bodyRaw);//bodyRaw);
        request.downloadHandler = (DownloadHandler) new DownloadHandlerBuffer();
        request.disposeUploadHandlerOnDispose = true;
        request.disposeDownloadHandlerOnDispose = true;
        request.SetRequestHeader("Content-Type", "application/json");
        var answer = request.SendWebRequest();
        
        while (!answer.isDone)
        {
        }
        if (request.result == UnityWebRequest.Result.ConnectionError)
        {
            UnityEngine.Debug.Log(request.error);
        }
        else
        {
            if (request.isDone)
            {
                string jsonResult = System.Text.Encoding.UTF8.GetString(request.downloadHandler.data);
                //Workaround: We remove the last couple of characters in the string "POST request /    "
                jsonResult = jsonResult.Remove(jsonResult.Length - 18);
                poseResult = JsonUtility.FromJson<PoseResult>(jsonResult);
                PoseAvatar(poseResult); //Coordinate System Transform is done by the server
                //Funktioniert, kann auf die Werte in den Variablen zugreifen. Rodrigues ist nun ein-dimensional
                request.uploadHandler.Dispose();
                request.downloadHandler.Dispose();
            }
        }
        UnityEngine.Debug.Log("Status Code: " + request.responseCode);
        UnityEngine.Debug.Log("This operation took: " + (Time.realtimeSinceStartup - timeNow) + "seconds.");
        return "";
    }

    public void PoseAvatar(PoseResult poseData)
    {
        //var r = Quaternion.Euler(180, 0, 0) * RotationMatrixToQuaternion(poseData.Rodrigues_joints, 0);
        //references.pelvis.transform.localRotation = RotationMatrixToQuaternion(poseData.Rodrigues_joints, 0);
        //UnityEngine.Debug.Log(RotationMatrixToQuaternion(poseData.Rodrigues_joints, 0).eulerAngles);

        references.spine.transform.localRotation = RotationMatrixToQuaternion(poseData.Rodrigues_joints, 2 + 1);
        references.chest.transform.localRotation = RotationMatrixToQuaternion(poseData.Rodrigues_joints, 8 + 1);
        references.neck.transform.localRotation = RotationMatrixToQuaternion(poseData.Rodrigues_joints, 11 + 1);
        references.head.transform.localRotation = RotationMatrixToQuaternion(poseData.Rodrigues_joints, 14 + 1);

        references.leftShoulder.transform.localRotation = RotationMatrixToQuaternion(poseData.Rodrigues_joints, 12 + 1);
        references.leftUpperArm.transform.localRotation = RotationMatrixToQuaternion(poseData.Rodrigues_joints, 15 + 1);
        references.leftForearm.transform.localRotation = RotationMatrixToQuaternion(poseData.Rodrigues_joints, 17 + 1);
        references.leftHand.transform.localRotation = RotationMatrixToQuaternion(poseData.Rodrigues_joints, 19 + 1);

        references.rightShoulder.transform.localRotation = RotationMatrixToQuaternion(poseData.Rodrigues_joints, 13 + 1);
        references.rightUpperArm.transform.localRotation = RotationMatrixToQuaternion(poseData.Rodrigues_joints, 16 + 1);
        references.rightForearm.transform.localRotation = RotationMatrixToQuaternion(poseData.Rodrigues_joints, 18 + 1);
        references.rightHand.transform.localRotation = RotationMatrixToQuaternion(poseData.Rodrigues_joints, 20 + 1);
        
        references.leftThigh.transform.localRotation = RotationMatrixToQuaternion(poseData.Rodrigues_joints, 0 + 1);
        references.leftCalf.transform.localRotation = RotationMatrixToQuaternion(poseData.Rodrigues_joints, 3 + 1);
        references.leftFoot.transform.localRotation = RotationMatrixToQuaternion(poseData.Rodrigues_joints, 6 + 1);
        references.leftToes.transform.localRotation = RotationMatrixToQuaternion(poseData.Rodrigues_joints, 9 + 1);

        references.rightThigh.transform.localRotation = RotationMatrixToQuaternion(poseData.Rodrigues_joints, 1 + 1);
        references.rightCalf.transform.localRotation = RotationMatrixToQuaternion(poseData.Rodrigues_joints, 4 + 1);
        references.rightFoot.transform.localRotation = RotationMatrixToQuaternion(poseData.Rodrigues_joints, 7 + 1);
        references.rightToes.transform.localRotation = RotationMatrixToQuaternion(poseData.Rodrigues_joints, 10 + 1);



        //Position
        references.pelvis.transform.position = FloatToVector3(poseData.E_global_position, 0);
        
        

        //UpdateSpheres(poseData);

    }


    void UpdateSpheres(PoseResult poseData)
    {
        spherels[0].transform.position = FloatToVector3(poseData.E_global_position, 0);
        for (int i = 1; i < 22; i++)
        {
            spherels[i].transform.position = FloatToVector3(poseData.E_joint_position, i);
        }

        //rotation 
        //UnityEngine.Debug.Log(poseData.Global_matrot);
        /*
        UnityEngine.Debug.Log(poseData.Rodrigues_joints[0 + 15 * 9]);
        UnityEngine.Debug.Log(poseData.Rodrigues_joints[1 + 15 * 9]);
        UnityEngine.Debug.Log(poseData.Rodrigues_joints[2 + 15 * 9]);
        UnityEngine.Debug.Log(poseData.Rodrigues_joints[3 + 15 * 9]);
        UnityEngine.Debug.Log(poseData.Rodrigues_joints[4 + 15 * 9]);
        UnityEngine.Debug.Log(poseData.Rodrigues_joints[5 + 15 * 9]);
        UnityEngine.Debug.Log(poseData.Rodrigues_joints[6 + 15 * 9]);
        UnityEngine.Debug.Log(poseData.Rodrigues_joints[7 + 15 * 9]);
        UnityEngine.Debug.Log(poseData.Rodrigues_joints[8 + 15 * 9]);
        
        for (int i = 1; i < 22; i++)
        {
            //https://github.com/mkocabas/VIBE/issues/26
            Quaternion quat = Quaternion.LookRotation(
                
                  new Vector3(-poseData.Rodrigues_joints[0 + 15 * 9], poseData.Rodrigues_joints[2 + 15 * 9], poseData.Rodrigues_joints[1 + 15 * 9]),
                  new Vector3(-poseData.Rodrigues_joints[6 + 15 * 9], poseData.Rodrigues_joints[8 + 15 * 9], poseData.Rodrigues_joints[7 + 15 * 9])
                
                
                  new Vector3(-poseData.Rodrigues_joints[2 + 15 * 9], poseData.Rodrigues_joints[5 + 15 * 9], poseData.Rodrigues_joints[8 + 15 * 9]),
                  new Vector3(-poseData.Rodrigues_joints[1 + 15 * 9], poseData.Rodrigues_joints[4 + 15 * 9], poseData.Rodrigues_joints[7 + 15 * 9])
                
              );
        
            
            // Quaternions 
            spherels[15].transform.localRotation = quat;
        
        
            //spherels[i].transform.rotation = RotationMatrixToQuaternion(poseData.Global_matrot, i - 1);
        }
        */
        
    }

    static Vector3 FloatToVector3(float[] position, int joint)
    {
        //0.002173679f, 0.9727238f, 0.02858379f
        return new Vector3(position[joint*3], position[joint*3 + 2], position[joint*3 + 1] - 0.45f); 
    }

    //Matrix4x4: Column1, column2, column3, column4
    //Instead of row1, row2, row3, row4 
    //Therefore, it's 0, 3, 6, 1, 4, 7, 2, 5, 8.
    //Correct!
    static Quaternion RotationMatrixToQuaternion(float[] rotation, int joint)
    {
        
        return Quaternion.LookRotation(
            new Vector3(-rotation[2 + 9 * joint], rotation[5 + 9 * joint], rotation[8 + 9 * joint]),
            new Vector3(-rotation[1 + 9 * joint], rotation[4 + 9 * joint], rotation[7 + 9 * joint])
        );
        
        /*
        return QuaternionFromMatrix(new Matrix4x4(new Vector4(rotation[0 + joint * 9], rotation[6 + joint * 9], rotation[3 + joint * 9], 0.0f), //column 1
                                new Vector4(rotation[1 + joint * 9], rotation[8 + joint * 9], rotation[5 + joint * 9], 0.0f), //column 2
                                new Vector4(rotation[2 + joint * 9], rotation[7 + joint * 9], rotation[4 + joint * 9], 0.0f), //column 3
                                new Vector4(0.0f, 0.0f, 0.0f, 1.0f))); //column 4
        */
    }



    //Correct result, keep in mind that the Matrix4x4 is based on COLUMNS not rows!
    static Quaternion QuaternionFromMatrix(Matrix4x4 m)
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
    

}



[System.Serializable]
public class References
{
    public Transform root;          // 0

    public Transform pelvis;        // 1
    public Transform spine;         // 2

    [Tooltip("Optional")]
    public Transform chest;         // 3 Optional

    [Tooltip("Optional")]
    public Transform neck;          // 4 Optional
    public Transform head;          // 5

    [Tooltip("Optional")]
    public Transform leftShoulder;  // 6 Optional
    [Tooltip("VRIK also supports armless characters.If you do not wish to use arms, leave all arm references empty.")]
    public Transform leftUpperArm;  // 7
    [Tooltip("VRIK also supports armless characters.If you do not wish to use arms, leave all arm references empty.")]
    public Transform leftForearm;   // 8
    [Tooltip("VRIK also supports armless characters.If you do not wish to use arms, leave all arm references empty.")]
    public Transform leftHand;      // 9

    [Tooltip("Optional")]
    public Transform rightShoulder; // 10 Optional
    [Tooltip("VRIK also supports armless characters.If you do not wish to use arms, leave all arm references empty.")]
    public Transform rightUpperArm; // 11
    [Tooltip("VRIK also supports armless characters.If you do not wish to use arms, leave all arm references empty.")]
    public Transform rightForearm;  // 12
    [Tooltip("VRIK also supports armless characters.If you do not wish to use arms, leave all arm references empty.")]
    public Transform rightHand;     // 13

    [Tooltip("VRIK also supports legless characters.If you do not wish to use legs, leave all leg references empty.")]
    public Transform leftThigh;     // 14 Optional

    [Tooltip("VRIK also supports legless characters.If you do not wish to use legs, leave all leg references empty.")]
    public Transform leftCalf;      // 15 Optional

    [Tooltip("VRIK also supports legless characters.If you do not wish to use legs, leave all leg references empty.")]
    public Transform leftFoot;      // 16 Optional

    [Tooltip("Optional")]
    public Transform leftToes;      // 17 Optional

    [Tooltip("VRIK also supports legless characters.If you do not wish to use legs, leave all leg references empty.")]
    public Transform rightThigh;    // 18 Optional

    [Tooltip("VRIK also supports legless characters.If you do not wish to use legs, leave all leg references empty.")]
    public Transform rightCalf;     // 19 Optional

    [Tooltip("VRIK also supports legless characters.If you do not wish to use legs, leave all leg references empty.")]
    public Transform rightFoot;     // 20 Optional

    [Tooltip("Optional")]
    public Transform rightToes;     // 21 Optional

    public Quaternion pelvisRot;
    public Quaternion spineRot;
    public Quaternion chestRot;
    public Quaternion neckRot;
    public Quaternion headRot;
    public Quaternion leftShoulderRot;
    public Quaternion leftUpperArmRot;
    public Quaternion leftForearmRot;
    public Quaternion leftHandRot;
    public Quaternion rightShoulderRot;
    public Quaternion rightUpperArmRot;
    public Quaternion rightForearmRot;
    public Quaternion rightHandRot;
    public Quaternion leftThighRot;
    public Quaternion leftCalfRot;
    public Quaternion leftFootRot;
    public Quaternion leftToesRot;
    public Quaternion rightThighRot;
    public Quaternion rightCalfRot;
    public Quaternion rightFootRot;
    public Quaternion rightToesRot;



    public References() { }

    public References(BipedReferences b)
    {
        root = b.root;
        pelvis = b.pelvis;
        spine = b.spine[0];
        chest = b.spine.Length > 1 ? b.spine[1] : null;
        head = b.head;

        leftShoulder = b.leftUpperArm.parent;
        leftUpperArm = b.leftUpperArm;
        leftForearm = b.leftForearm;
        leftHand = b.leftHand;

        rightShoulder = b.rightUpperArm.parent;
        rightUpperArm = b.rightUpperArm;
        rightForearm = b.rightForearm;
        rightHand = b.rightHand;

        leftThigh = b.leftThigh;
        leftCalf = b.leftCalf;
        leftFoot = b.leftFoot;
        leftToes = b.leftFoot.GetChild(0);

        rightThigh = b.rightThigh;
        rightCalf = b.rightCalf;
        rightFoot = b.rightFoot;
        rightToes = b.rightFoot.GetChild(0);
    }

    /// <summary>
    /// Returns an array of all the Transforms in the definition.
    /// </summary>
    public Transform[] GetTransforms()
    {
        return new Transform[22] {
                    root, pelvis, spine, chest, neck, head, leftShoulder, leftUpperArm, leftForearm, leftHand, rightShoulder, rightUpperArm, rightForearm, rightHand, leftThigh, leftCalf, leftFoot, leftToes, rightThigh, rightCalf, rightFoot, rightToes
                };
    }

    /// <summary>
    /// Returns true if all required Transforms have been assigned (shoulder, toe and neck bones are optional).
    /// </summary>
    public bool isFilled
    {
        get
        {
            if (
                root == null ||
                pelvis == null ||
                spine == null ||
                head == null
            ) return false;

            bool noArmBones =
                leftUpperArm == null &&
                leftForearm == null &&
                leftHand == null &&
                rightUpperArm == null &&
                rightForearm == null &&
                rightHand == null;

            bool atLeastOneArmBoneMissing =
                leftUpperArm == null ||
                leftForearm == null ||
                leftHand == null ||
                rightUpperArm == null ||
                rightForearm == null ||
                rightHand == null;

            // If all leg bones are null, it is valid
            bool noLegBones =
                leftThigh == null &&
                leftCalf == null &&
                leftFoot == null &&
                rightThigh == null &&
                rightCalf == null &&
                rightFoot == null;

            bool atLeastOneLegBoneMissing =
                leftThigh == null ||
                leftCalf == null ||
                leftFoot == null ||
                rightThigh == null ||
                rightCalf == null ||
                rightFoot == null;

            if (atLeastOneLegBoneMissing && !noLegBones) return false;
            if (atLeastOneArmBoneMissing && !noArmBones) return false;

            // Shoulder, toe and neck bones are optional
            return true;
        }
    }

    /// <summary>
    /// Returns true if none of the Transforms have been assigned.
    /// </summary>
    public bool isEmpty
    {
        get
        {
            if (
                root != null ||
                pelvis != null ||
                spine != null ||
                chest != null ||
                neck != null ||
                head != null ||
                leftShoulder != null ||
                leftUpperArm != null ||
                leftForearm != null ||
                leftHand != null ||
                rightShoulder != null ||
                rightUpperArm != null ||
                rightForearm != null ||
                rightHand != null ||
                leftThigh != null ||
                leftCalf != null ||
                leftFoot != null ||
                leftToes != null ||
                rightThigh != null ||
                rightCalf != null ||
                rightFoot != null ||
                rightToes != null
            ) return false;

            return true;
        }
    }

    /// <summary>
    /// Auto-detects VRIK references. Works with a Humanoid Animator on the root gameobject only.
    /// </summary>
    public static bool AutoDetectReferences(Transform root, out References references)
    {
        references = new References();

        var animator = root.GetComponentInChildren<Animator>();
        if (animator == null || !animator.isHuman)
        {
            UnityEngine.Debug.LogWarning("VRIK needs a Humanoid Animator to auto-detect biped references. Please assign references manually.");
            return false;
        }

        references.root = root;
        references.pelvis = animator.GetBoneTransform(HumanBodyBones.Hips);
        references.spine = animator.GetBoneTransform(HumanBodyBones.Spine);
        references.chest = animator.GetBoneTransform(HumanBodyBones.Chest);
        references.neck = animator.GetBoneTransform(HumanBodyBones.Neck);
        references.head = animator.GetBoneTransform(HumanBodyBones.Head);
        references.leftShoulder = animator.GetBoneTransform(HumanBodyBones.LeftShoulder);
        references.leftUpperArm = animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
        references.leftForearm = animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
        references.leftHand = animator.GetBoneTransform(HumanBodyBones.LeftHand);
        references.rightShoulder = animator.GetBoneTransform(HumanBodyBones.RightShoulder);
        references.rightUpperArm = animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
        references.rightForearm = animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
        references.rightHand = animator.GetBoneTransform(HumanBodyBones.RightHand);
        references.leftThigh = animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
        references.leftCalf = animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
        references.leftFoot = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
        references.leftToes = animator.GetBoneTransform(HumanBodyBones.LeftToes);
        references.rightThigh = animator.GetBoneTransform(HumanBodyBones.RightUpperLeg);
        references.rightCalf = animator.GetBoneTransform(HumanBodyBones.RightLowerLeg);
        references.rightFoot = animator.GetBoneTransform(HumanBodyBones.RightFoot);
        references.rightToes = animator.GetBoneTransform(HumanBodyBones.RightToes);

        return true;
    }
}

[Serializable]
public struct PoseData
{
    public float[] pose;
    public float[] head_global_trans_list;

    public PoseData(float[] pose, float[] head_global_trans_list)
    {
        this.pose = pose;
        this.head_global_trans_list = head_global_trans_list;
    }
}

[Serializable]
public class PoseResult
{
    public float[] E_global_orientation;
    public float[] E_joint_rotation;
    public float[] Rodrigues_joints;
    public float[] E_global_position;
    public float[] E_joint_position;
    public float[] Global_matrot;

    public PoseResult(float[] E_global_orientation, float[] E_joint_rotation, float[] Rodrigues_joints, float[] E_global_position, float[] E_joint_position,
        float[] Global_matrot)
    {
        this.E_global_orientation = E_global_orientation;
        this.E_joint_rotation = E_joint_rotation;
        this.Rodrigues_joints = Rodrigues_joints;
        this.E_global_position = E_global_position;
        this.E_joint_position = E_joint_position;
        this.Global_matrot = Global_matrot;
    }
}
