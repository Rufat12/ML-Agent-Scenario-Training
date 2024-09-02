using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;


public class AgentController_V1 : Agent
{
    [Space(5)]
    [Header("Settings")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float reward = 2f;
    [SerializeField] private float punishment = -2f;
    [Header("Dependancy")]
    [SerializeField] private Transform target;

    public override void OnEpisodeBegin()
    {
        transform.localPosition = new Vector3(0f, 0.25f, 0f);

        int chance = UnityEngine.Random.Range(0,2);
        if (chance == 0)
        {
            target.localPosition = new Vector3(4f, 0.3f, 0);
        }
        else if (chance == 1)
        {
            target.localPosition = new Vector3(-4f, 0.3f, 0);
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(target.localPosition);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float move = actions.ContinuousActions[0];
        transform.localPosition += new Vector3(move, 0f) * Time.deltaTime * moveSpeed;
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
        Debug.Log(Vector3.Distance(target.transform.localPosition, transform.localPosition));
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxisRaw("Horizontal");
        float move = continuousActions[0];
        transform.localPosition += new Vector3(move, 0f) * Time.deltaTime * moveSpeed;
    }
}