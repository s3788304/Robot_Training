using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PenaltyColliders : MonoBehaviour
{
    //Referencing the parent Robot Controller
    public RobotControllerAgent parentAgent;

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.CompareTag("RobotInternal") || other.transform.CompareTag("Ground"))
        {
            if (parentAgent != null)
                parentAgent.GroundHitPenalty();

        }
    }
}
