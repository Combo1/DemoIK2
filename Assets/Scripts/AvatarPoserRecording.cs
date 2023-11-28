using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Linq;

public class AvatarPoserRecording : MonoBehaviour
{

    public GameObject head;
    public GameObject leftHand;
    public GameObject rightHand;

    public string csvFileName;
    private StreamWriter writer;

    private Vector3 prevHead;
    private Vector3 prevLeftHand;
    private Vector3 prevRightHand;

    private Matrix4x4 prevRotationHead;
    private Matrix4x4 prevRotationLeftHand;
    private Matrix4x4 prevRotationRightHand;


    // Start is called before the first frame update
    void Start()
    {
        writer = new StreamWriter(csvFileName);

        prevHead = head.transform.position;
        prevLeftHand = leftHand.transform.position;
        prevRightHand = rightHand.transform.position;

        prevRotationHead = head.transform.localToWorldMatrix;
        prevRotationLeftHand = leftHand.transform.localToWorldMatrix;
        prevRotationRightHand = rightHand.transform.localToWorldMatrix;
    }

    // Update is called once per frame
    void Update()
    {
        var rotationMatrixHead = head.transform.localToWorldMatrix;
        var rotationMatrixLeftHand = leftHand.transform.localToWorldMatrix;
        var rotationMatrixRightHand = rightHand.transform.localToWorldMatrix;

        //R3x3: rotation matrix, changed data order
        double[][] mHead = new double[][] { new double[] { rotationMatrixHead[0, 2], rotationMatrixHead[0, 0], rotationMatrixHead[0, 1] }, new double[]{rotationMatrixHead[1, 2], rotationMatrixHead[1, 0], rotationMatrixHead[1, 1]}, new double[]{rotationMatrixHead[2, 2], rotationMatrixHead[2, 0], rotationMatrixHead[2, 1]}};
        double[][] mLeftHand = new double[][] { new double[] { rotationMatrixLeftHand[0, 2], rotationMatrixLeftHand[0, 0], rotationMatrixLeftHand[0, 1] }, new double[] { rotationMatrixLeftHand[1, 2], rotationMatrixLeftHand[1, 0], rotationMatrixLeftHand[1, 1] }, new double[] { rotationMatrixLeftHand[2, 2], rotationMatrixLeftHand[2, 0], rotationMatrixLeftHand[2, 1] } };
        double[][] mRightHand = new double[][] { new double[] { rotationMatrixRightHand[0, 2], rotationMatrixRightHand[0, 0], rotationMatrixRightHand[0, 1] }, new double[] { rotationMatrixRightHand[1, 2], rotationMatrixRightHand[1, 0], rotationMatrixRightHand[1, 1] }, new double[] { rotationMatrixRightHand[2, 2], rotationMatrixRightHand[2, 0], rotationMatrixRightHand[2, 1] } };
        
        //R-1 3x3: rotation matrix from one frame before, changed data order
        double[][] mPrevHead = new double[][] { new double[] { prevRotationHead[0, 2], prevRotationHead[0, 0], prevRotationHead[0, 1] }, new double[] { prevRotationHead[1, 2], prevRotationHead[1, 0], prevRotationHead[1, 1] }, new double[] { prevRotationHead[2, 2], prevRotationHead[2, 0], prevRotationHead[2, 1] } };
        double[][] mPrevLeftHand = new double[][] { new double[] { prevRotationLeftHand[0, 2], prevRotationLeftHand[0, 0], prevRotationLeftHand[0, 1] }, new double[] { prevRotationLeftHand[1, 2], prevRotationLeftHand[1, 0], prevRotationLeftHand[1, 1] }, new double[] { prevRotationLeftHand[2, 2], prevRotationLeftHand[2, 0], prevRotationLeftHand[2, 1] } };
        double[][] mPrevRightHand = new double[][] { new double[] { prevRotationRightHand[0, 2], prevRotationRightHand[0, 0], prevRotationRightHand[0, 1] }, new double[] { prevRotationRightHand[1, 2], prevRotationRightHand[1, 0], prevRotationRightHand[1, 1] }, new double[] { prevRotationRightHand[2, 2], prevRotationRightHand[2, 0], prevRotationRightHand[2, 1] } };

        //Invert the previous time step's rotation matrix
        mPrevHead = MatrixInverse(mPrevHead); 
        mPrevLeftHand = MatrixInverse(mPrevLeftHand);
        mPrevRightHand = MatrixInverse(mPrevRightHand);

        //Multiply the previous time step's rotation matrix with the current time step rotation matrix
        var rotvelHead = MatrixProduct(mPrevHead, mHead);
        var rotvelLeftHand = MatrixProduct(mPrevLeftHand, mLeftHand);
        var rotvelRightHand = MatrixProduct(mPrevRightHand, mRightHand);


        //Write orientation, rotational velocity, position, velocity
        writer.WriteLine(
            rotationMatrixHead[0, 0] + "," + rotationMatrixHead[0, 1] + "," + rotationMatrixHead[0, 2] + "," + rotationMatrixHead[1, 0] + "," + rotationMatrixHead[1, 1] + "," + rotationMatrixHead[1, 2] + "," + rotationMatrixLeftHand[0, 0] + "," + rotationMatrixLeftHand[0, 1] + "," + rotationMatrixLeftHand[0, 2] + "," + rotationMatrixLeftHand[1, 0] + "," + rotationMatrixLeftHand[1, 1] + "," + rotationMatrixLeftHand[1, 2] + "," + rotationMatrixRightHand[0, 0] + "," + rotationMatrixRightHand[0, 1] + "," + rotationMatrixRightHand[0, 2] + "," + rotationMatrixRightHand[1, 0] + "," + rotationMatrixRightHand[1, 1] + "," + rotationMatrixRightHand[1, 2] + "," + //rotation
            rotvelHead[0][0] + "," + rotvelHead[0][1] + "," + rotvelHead[0][2] + "," + rotvelHead[1][0] + "," + rotvelHead[1][1] + "," + rotvelHead[1][2] + "," + rotvelLeftHand[0][0] + "," + rotvelLeftHand[0][1] + "," + rotvelLeftHand[0][2] + "," + rotvelLeftHand[1][0] + "," + rotvelLeftHand[1][1] + "," + rotvelLeftHand[1][2] + "," + rotvelRightHand[0][0] + "," + rotvelRightHand[0][1] + "," + rotvelRightHand[0][2] + "," + rotvelRightHand[1][0] + "," + rotvelRightHand[1][1] + "," + rotvelRightHand[1][2] + "," + //rotational velocity
            head.transform.position.z + "," + head.transform.position.x + "," + head.transform.position.y + "," + leftHand.transform.position.z + "," + leftHand.transform.position.x + "," + leftHand.transform.position.y + "," + rightHand.transform.position.z + "," + rightHand.transform.position.x + "," + rightHand.transform.position.y + "," + //position
            (head.transform.position.z - prevHead.z) + "," + (head.transform.position.x - prevHead.x) + "," + (head.transform.position.y - prevHead.y) + "," + (leftHand.transform.position.z - prevLeftHand.z) + "," + (leftHand.transform.position.x - prevLeftHand.x) + "," + (leftHand.transform.position.y - prevLeftHand.y) + "," + (rightHand.transform.position.z - prevRightHand.z) + "," + (rightHand.transform.position.x - prevRightHand.x) + "," + (rightHand.transform.position.y - prevRightHand.y) //velocity
            );
        /*
        writer.WriteLine(
            rotationMatrixHead[0,0] + "," + rotationMatrixHead[0,1] + "," + rotationMatrixHead[0,2] + "," + rotationMatrixHead[1, 0] + "," + rotationMatrixHead[1, 1] + "," + rotationMatrixHead[1, 2] + "," + rotationMatrixLeftHand[0, 0] + "," + rotationMatrixLeftHand[0, 1] + "," + rotationMatrixLeftHand[0, 2] + "," +  rotationMatrixLeftHand[1, 0] + "," + rotationMatrixLeftHand[1, 1] + "," + rotationMatrixLeftHand[1, 2] + "," + rotationMatrixRightHand[0, 0] + "," + rotationMatrixRightHand[0, 1] + "," + rotationMatrixRightHand[0, 2] + "," + rotationMatrixRightHand[1, 0] + "," + rotationMatrixRightHand[1, 1] + "," + rotationMatrixRightHand[1, 2] + "," + //rotation
            rotvelHead[0][0] + "," + rotvelHead[0][1] + "," + rotvelHead[0][2] + "," + rotvelHead[1][0] + "," + rotvelHead[1][1] + "," + rotvelHead[1][2] + "," + rotvelLeftHand[0][0] + "," + rotvelLeftHand[0][1] + "," + rotvelLeftHand[0][2] + "," + rotvelLeftHand[1][0] + "," + rotvelLeftHand[1][1] + "," + rotvelLeftHand[1][2] + "," + rotvelRightHand[0][0] + "," + rotvelRightHand[0][1] + "," + rotvelRightHand[0][2] + "," + rotvelRightHand[1][0] + "," + rotvelRightHand[1][1] + "," + rotvelRightHand[1][2] + "," + //rotational velocity
            head.transform.position.x + "," + head.transform.position.y + "," + head.transform.position.z + "," + leftHand.transform.position.x + "," + leftHand.transform.position.y + "," + leftHand.transform.position.z + "," + rightHand.transform.position.x + "," + rightHand.transform.position.y + "," + rightHand.transform.position.z + "," + //position
            (head.transform.position.x - prevHead.x) + "," + (head.transform.position.y - prevHead.y) + "," + (head.transform.position.z - prevHead.z) + "," + (leftHand.transform.position.x - prevLeftHand.x) + "," + (leftHand.transform.position.y - prevLeftHand.y) + "," + (leftHand.transform.position.z - prevLeftHand.z) + "," + (rightHand.transform.position.x - prevRightHand.x) + "," + (rightHand.transform.position.y - prevRightHand.y) + "," + (rightHand.transform.position.z - prevRightHand.z) //velocity
            );
        */


        prevHead = head.transform.position;
        prevLeftHand = leftHand.transform.position;
        prevRightHand = rightHand.transform.position;

        prevRotationHead = rotationMatrixHead;
        prevRotationLeftHand = rotationMatrixLeftHand;
        prevRotationRightHand = rotationMatrixRightHand;
    }

    void OnApplicationQuit()
    {
        writer.Close();
    }

    //Taken from: https://stackoverflow.com/questions/46836908/how-to-invert-double-in-c-sharp
    static double[][] MatrixCreate(int rows, int cols)
    {
        double[][] result = new double[rows][];
        for (int i = 0; i < rows; ++i)
            result[i] = new double[cols];
        return result;
    }
    static double[][] MatrixIdentity(int n)
    {
        // return an n x n Identity matrix
        double[][] result = MatrixCreate(n, n);
        for (int i = 0; i < n; ++i)
            result[i][i] = 1.0;

        return result;
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

}
