using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NearestCompCheck : MonoBehaviour
{
    [SerializeField]
    private GameObject endEffector;

    

    //Bool for End Effector to be in front of Component at the beginning for training
    public bool inFrontOfComponent = false;
    public GameObject nearestComponent;

    void Start()
    {
        inFrontOfComponent = Random.value > 0.5f;

        if (inFrontOfComponent)
        {
            nearestComponent.transform.position = transform.position + new Vector3(Random.Range(0.5f, 1f), Random.Range(0.01f, 0.05f), Random.Range(0.5f, 1f));
        }
        else
        {
            nearestComponent.transform.position = endEffector.transform.TransformPoint(Vector3.zero) + new Vector3(Random.Range(0.1f, 0.2f), Random.Range(0.1f, 0.2f), Random.Range(0.1f, 0.2f));
        }

        float beginDistance = Vector3.Distance(endEffector.transform.TransformPoint(Vector3.zero), nearestComponent.transform.position);
        Debug.Log("Distance = " + beginDistance);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
