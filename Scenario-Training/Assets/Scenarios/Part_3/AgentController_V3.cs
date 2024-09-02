using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class AgentController_V3 : Agent
{
    [Space(5)]
    [Header("Settings")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float reward = 2f;
    [SerializeField] private float punishment = -2f;
    [Header("Dependancy")]
    [SerializeField] private Transform target;

    [SerializeField] private Rigidbody rb;

    public override void OnEpisodeBegin()
    {
        transform.localPosition
            = new Vector3(Random.Range(-4f, 4f), 0.25f, Random.Range(-4f, 4f));
        target.localPosition
            = new Vector3(Random.Range(-4f, 4f), 0.25f, Random.Range(-4f, 4f));
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

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Target")
        {
            Debug.Log("+");
            AddReward(reward);
            EndEpisode();
        }
        else if (other.gameObject.tag == "Wall")
        {
            Debug.Log("-");
            AddReward(punishment);
            EndEpisode();
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxisRaw("Horizontal");
        continuousActions[1] = Input.GetAxisRaw("Vertical");
    }
}
