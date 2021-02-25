using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using System.Linq;

public class RotationCheck : MonoBehaviour
{
    //Vector3 rotationAx;
    public GameObject[] armAxes;

    public GameObject endEffector;

    // Start is called before the first frame update
    void Start()
    {
        //rotationAx = FindObjectOfType<Axis>().rotationAxis;
       MoveToSafeRandomPosition();
    }

    // Update is called once per frame
    void Update()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        //Debug.Log(rotationAx.ToString());


        //armAxes[0].transform.localRotation = Quaternion.AngleAxis(horizontalInput * 180f, armAxes[0].GetComponent<Axis>().rotationAxisY);
        //armAxes[1].transform.localRotation = Quaternion.AngleAxis(horizontalInput * 90f, armAxes[1].GetComponent<Axis>().rotationAxis);
       //armAxes[2].transform.localRotation = Quaternion.AngleAxis(horizontalInput * 180f, armAxes[2].GetComponent<Axis>().rotationAxis);
       //armAxes[3].transform.localRotation = Quaternion.AngleAxis(horizontalInput * 90f, armAxes[3].GetComponent<Axis>().rotationAxis);
       // armAxes[4].transform.localRotation = Quaternion.AngleAxis(horizontalInput * 90f, armAxes[4].GetComponent<Axis>().rotationAxisY);


        //transform.localRotation = Quaternion.AngleAxis(horizontalInput * 90, rotationAx);
        //transform.Rotate(rotationAx * horizontalInput* 90f, Space.Self);
        
    }


    private void MoveToSafeRandomPosition()
    {
        //To prevent infine loop
        int maxTries = 100;
        bool safePositionFound = false;
        Vector3 angle = new Vector3(0f, 0f, 0f);

        while (maxTries > 0)
        {

            for (int i = 0; i < armAxes.Length; i++)
            {
                Axis ax = armAxes[i].GetComponent<Axis>();
                if (i == 0 || i == 4)
                    angle = ax.rotationAxisY * Random.Range(ax.MinAngle, ax.MaxAngle);
                else
                    angle = ax.rotationAxis * Random.Range(ax.MinAngle, ax.MaxAngle);

                ax.transform.localRotation = Quaternion.Euler(angle.x, angle.y, angle.z);
                //Debug.Log("Axis : " + i  + "   angle = " + Random.Range(ax.MinAngle, ax.MaxAngle));

                
            }
            
            
            Vector3 tipPosition = endEffector.transform.TransformPoint(Vector3.zero);
            Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
            float distanceFromGround = groundPlane.GetDistanceToPoint(tipPosition);



            if (distanceFromGround > 0.1f && distanceFromGround <= 1.5f && tipPosition.y > 0.05f)
            {
                safePositionFound = true;
                Debug.Log("tipY = " + tipPosition.y);
                break;
            }

            maxTries--;
        }
        
        //To check if a safe position is found or not
        Debug.Assert(safePositionFound, "Could not find a safe position");
    }
}
