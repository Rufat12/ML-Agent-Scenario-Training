using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using System.Collections;
using System.Collections.Generic;

public class AgentController_V4 : Agent
{
    [Space(5)]
    [Header("Settings")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float reward = 2f;
    [SerializeField] private float punishment = -2f;
    [Header("Dependancy")]
    [SerializeField] private Transform target;
    [SerializeField] private Rigidbody rb;
    [Header("Spawning")]
    [SerializeField] private int rewardCount;
    [SerializeField] private GameObject prefab;
    [SerializeField] private GameObject prefabParent;
    [SerializeField] private List<GameObject> prefabs;
    [Header("VisualFeedBack")]
    [SerializeField] private Renderer render;
    [SerializeField] private Material winMaterial;
    [SerializeField] private Material looseMaterial;
    [SerializeField] private Material blankMaterial;

    Coroutine coroutine;


    public override void OnEpisodeBegin()
    {
        transform.localPosition
            = new Vector3(Random.Range(-4f, 4f), 0.25f, Random.Range(-4f, 4f));

        OnSpawnRewards();
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

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxisRaw("Horizontal");
        continuousActions[1] = Input.GetAxisRaw("Vertical");
    }

    private void OnSpawnRewards()
    {
        OnClearOldRewards();

        for (int i = 0; i < rewardCount; i++)
        {
            GameObject newReward = Instantiate(prefab);
            newReward.transform.parent = prefabParent.transform;
            Vector3 randPosition = new Vector3(Random.Range(-4f, 4f), 0.25f, Random.Range(-4f, 4f));
            newReward.transform.localPosition = randPosition;
            prefabs.Add(newReward);
        }
    }

    private void OnClearOldRewards()
    {
        foreach (GameObject x in prefabs)
        {
            Destroy(x.gameObject);
        }
        prefabs.Clear();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Target")
        {
            prefabs.Remove(other.gameObject);
            Destroy(other.gameObject);

            Debug.Log("+");
            AddReward(reward);

            if (prefabs.Count == 0)
            {
                OnClearCoRoutine();
                coroutine =  StartCoroutine(ColorChange(winMaterial));
                EndEpisode();
            }
            
        }
        else if (other.gameObject.tag == "Wall")
        {
            Debug.Log("-");
            AddReward(punishment);
            OnClearCoRoutine();
            coroutine = StartCoroutine(ColorChange(looseMaterial));
            EndEpisode();
        }
    }

    IEnumerator ColorChange(Material mat)
    {
        render.material = mat;
        yield return new WaitForSeconds(2f);
        render.material = blankMaterial;
    }
    
    private void OnClearCoRoutine()
    {
        if (coroutine != null)
        {
            coroutine = null;
            render.material = blankMaterial;
        }
    }
}