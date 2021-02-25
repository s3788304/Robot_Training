using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Axis : MonoBehaviour
{
    public Vector3 rotationAxis;
    public Vector3 rotationAxisY;
    private Vector3 beginLocation;

    public float MinAngle;
    public float MaxAngle;

    public void Awake()
    {
        rotationAxis = new Vector3(0f, 0f, 1f);
        rotationAxisY = new Vector3(0f, 1f, 0f);
        beginLocation = transform.position;
    }

    public float MinAngleRadians => Mathf.Deg2Rad * MinAngle;
    public float MaxAngleRadians => Mathf.Deg2Rad * MaxAngle;
        
}
