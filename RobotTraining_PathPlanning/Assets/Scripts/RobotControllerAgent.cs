using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Random = UnityEngine.Random;
using System;
using System.Linq;

public class RobotControllerAgent : Agent
{
    //List of Robot Axis(0-4) Game Objects
    [SerializeField]
    private GameObject[] armAxes;

    //Game Object for the End Effector
    [SerializeField]
    private GameObject endEffector;

    //Training mode active or not
    public bool trainingMode;

    //Bool for End Effector to be in front of Component at the beginning for training
    public bool inFrontOfComponent = false;
    public GameObject nearestComponent;

    //List to store angles(actions) for the robot arms
    private float[] angles = new float[5];

    //Distance values for end effector and component
    private float beginDistance;
    private float prevBest;

    private float baseAngle;
    private const float stepPenalty = -0.0001f;
          
       
    void Start()
    {
        
    }


    //Initialise values for Agent
    public override void Initialize()
    {
        ResetAllAxis();
        MoveToSafeRandomPosition();

        //If not training mode, no max step, run forever
        if (!trainingMode)
            MaxStep = 0;
    }


    //Reset All Robot Axis to zero rotation
    private void ResetAllAxis()
    {
        armAxes.All(c =>
        {
            c.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
            return true;
        });
    }


    //On episode begin from Agent class
    public override void OnEpisodeBegin()
    {
        if (trainingMode)
            ResetAllAxis();

        MoveToSafeRandomPosition();
        UpdateNearestComponent();
    }

   
    //Update location for the component
    private void UpdateNearestComponent()
    {
        if (trainingMode)
            inFrontOfComponent = Random.value > 0.5f;

        if(inFrontOfComponent)
        {
            nearestComponent.transform.position = transform.position + new Vector3(Random.Range(0.3f, 0.7f), Random.Range(0.1f, 0.3f), Random.Range(0.3f, .7f));
        }
        else
        {
            nearestComponent.transform.position = endEffector.transform.TransformPoint(Vector3.zero) + new Vector3(Random.Range(0.1f, 0.2f), Random.Range(0.1f, 0.2f), Random.Range(0.1f, 0.2f));
        }

        beginDistance = Vector3.Distance(endEffector.transform.TransformPoint(Vector3.zero), nearestComponent.transform.position);
        prevBest = beginDistance;

        baseAngle = Mathf.Atan2(transform.position.x - nearestComponent.transform.position.x, transform.position.z - nearestComponent.transform.position.z) * Mathf.Deg2Rad;
        if (baseAngle < 0)
            baseAngle += 360f;
    }


    /// <summary>
    /// Collect observations from the environment
    /// </summary>
    /// <param name="sensor"></param>
    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(angles);      //5 Observtions
        sensor.AddObservation(transform.position.normalized);       //3 Observations
        sensor.AddObservation(endEffector.transform.TransformPoint(Vector3.zero).normalized);       //3 Observations
        sensor.AddObservation(nearestComponent.transform.position.normalized);      //3 Observations

        Vector3 toComponent = nearestComponent.transform.position - endEffector.transform.TransformPoint(Vector3.zero);
        sensor.AddObservation(toComponent.normalized);      //3 Observations

        float angleBet = Vector3.Angle(nearestComponent.transform.up, endEffector.transform.up);
        sensor.AddObservation(angleBet); //1 Observation

        sensor.AddObservation(Vector3.Distance(nearestComponent.transform.position, endEffector.transform.TransformPoint(Vector3.zero)));       //1 Observation
        sensor.AddObservation(StepCount / 5000);        //1 Observation
    }


    /// <summary>
    /// Actions to assign to agent 
    /// </summary>
    /// <param name="vectorAction"></param>
    public override void OnActionReceived(float[] vectorAction)
    {
        angles = vectorAction;

        if(trainingMode)
        {
            //Translate the floating point values to degrees for each robot axis
            armAxes[0].transform.localRotation = Quaternion.AngleAxis(angles[0] * 180f, armAxes[0].GetComponent<Axis>().rotationAxisY);
            armAxes[1].transform.localRotation = Quaternion.AngleAxis(angles[1] * 90f, armAxes[1].GetComponent<Axis>().rotationAxis);
            armAxes[2].transform.localRotation = Quaternion.AngleAxis(angles[2] * 90f, armAxes[2].GetComponent<Axis>().rotationAxis);
            armAxes[3].transform.localRotation = Quaternion.AngleAxis(angles[3] * 90f, armAxes[3].GetComponent<Axis>().rotationAxis);
            armAxes[4].transform.localRotation = Quaternion.AngleAxis(angles[4] * 180f, armAxes[4].GetComponent<Axis>().rotationAxisY);

            float distance = Vector3.Distance(endEffector.transform.TransformPoint(Vector3.zero), nearestComponent.transform.position);
            float diff = beginDistance - distance;

            if(distance > prevBest)
            {
                //Adding a penalty if the arm moves away from the target position
                AddReward(prevBest - distance);
            }
            else
            {
                AddReward(diff);
                prevBest = distance;
            }

            //Reward for angle between component and gripper
            float angleReward = Mathf.Clamp01(Vector3.Dot(nearestComponent.transform.up.normalized, endEffector.transform.up.normalized));
            AddReward(angleReward * 0.01f);

            //Penalty for evrty time step passed
            AddReward(stepPenalty);
        }
    }


    //Add penalty for hitting the ground and end episode
    public void GroundHitPenalty()
    {
        AddReward(-10f);
        EndEpisode();
    }

    
    //Checking collision with other game objects
    /*public void OnCollisionEnter(Collider other)
    {
        JackpotReward(other);
    }
    */

    //Jackpot reward for reaching the component
    public void JackpotReward(Collider other)
    {
        if(other.transform.CompareTag("Component"))
        {
            float successReward = 0.5f;
            float bonus = Mathf.Clamp01(Vector3.Dot(nearestComponent.transform.up.normalized, endEffector.transform.up.normalized));
            float reward = successReward + bonus;

            if (float.IsInfinity(reward) || float.IsNaN(reward))
                return;

            Debug.LogWarning("End Effector Reached Nearest Component. Reward = " + reward);
            AddReward(reward);

            //Update nearest component location again
            UpdateNearestComponent();

        }
    }


    //Move the robot axes to a safe random position 
    private void MoveToSafeRandomPosition()
    {
        //To prevent infine loop
        int maxTries = 100;
        bool safePositionFound = false;
        Vector3 angle = new Vector3(0f, 0f, 0f);

        while (maxTries > 0)
        {
            for(int i = 0; i< armAxes.Length; i++)
                {
                    Axis ax = armAxes[i].GetComponent<Axis>();
                    if(i == 0 || i == 4)
                        angle = ax.rotationAxisY * Random.Range(ax.MinAngle, ax.MaxAngle);
                    else
                        angle = ax.rotationAxis * Random.Range(ax.MinAngle, ax.MaxAngle);
                    ax.transform.localRotation = Quaternion.Euler(angle.x, angle.y, angle.z);  
                }
            

            Vector3 tipPosition = endEffector.transform.TransformPoint(Vector3.zero);
            Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
            float distanceFromGround = groundPlane.GetDistanceToPoint(tipPosition);

            if (distanceFromGround > 0.1f && distanceFromGround <= 1.5f && tipPosition.y > 0.05f)
            {
                safePositionFound = true;
                break;
            }

            maxTries--;
        }

        //Tp check if a safe position is found or not
        Debug.Assert(safePositionFound, "Could not find a safe position");
    }


    //Draw line between End Effector and Component
    private void Update()
    {
        if (nearestComponent != null)
            Debug.DrawLine(endEffector.transform.position, nearestComponent.transform.position, Color.green);
    }
       
}
