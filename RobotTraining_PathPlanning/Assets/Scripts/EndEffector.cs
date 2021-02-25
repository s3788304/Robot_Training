using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndEffector : MonoBehaviour
{
    //Referencing the parent Robot Controller
    public RobotControllerAgent parentAgent;

    private void OnTriggerEnter(Collider other)
    {
        if(other.transform.CompareTag("Component"))
        {
            if (parentAgent != null)
                parentAgent.JackpotReward(other);
        }

        else if(other.transform.CompareTag("Ground"))
        {
            if (parentAgent != null)
                parentAgent.GroundHitPenalty();
        }
    }
}
