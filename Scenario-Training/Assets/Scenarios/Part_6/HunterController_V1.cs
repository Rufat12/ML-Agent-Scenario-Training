using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using System.Collections;
using System.Collections.Generic;

public class HunterController_V1 : Agent
{
    [Space(5)]
    [Header("Settings")]
    [SerializeField] private float moveSpeed = 2f;
    [Header("Dependancy")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Material winMaterial;
    [SerializeField] private Material looseMaterial;
    [Header("OtherAgents")]
    [SerializeField] private PreyController_V1 prey;
    public override void OnEpisodeBegin()
    {
        Vector3 spawnLocation = new Vector3(Random.Range(-4f, 4f), 0.25f, Random.Range(-4f, 4f));
        bool distanceGood = prey.OnCheckForOverLap(prey.transform.localPosition, spawnLocation, 5f);

        int count = 0;
        while (!distanceGood)
        {
            spawnLocation = new Vector3(Random.Range(-4f, 4f), 0.25f, Random.Range(-4f, 4f));
            distanceGood = prey.OnCheckForOverLap(prey.transform.localPosition, spawnLocation, 5f);

            count++;
            if (count > 100)
                break;
        }

        transform.localPosition = spawnLocation;
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxisRaw("Horizontal");
        continuousActions[1] = Input.GetAxisRaw("Vertical");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Prey")
        {
            Debug.Log("Hunter Win");
            AddReward(10f);
            prey.AddReward(-10f);
            prey.OnInvokeColorChange(winMaterial);
            prey.EndEpisode();
            EndEpisode();
        }
        else if (other.gameObject.tag == "Wall")
        {
            Debug.Log("Hunter Hit Wall");
            AddReward(-10f);
            prey.OnInvokeColorChange(looseMaterial);
            prey.EndEpisode();
            EndEpisode();
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveRotate = actions.ContinuousActions[0];
        float moveForward = actions.ContinuousActions[1];

        rb.MovePosition(transform.position + transform.forward * moveForward * moveSpeed * Time.deltaTime);
        transform.Rotate(0f, moveRotate * moveSpeed, 0f, Space.Self);
    }
}