using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using System.Collections;
using System.Collections.Generic;

public class AgentController_V6 : Agent
{
    [Space(5)]
    [Header("Settings")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float reward = 2f;
    [SerializeField] private float punishment = -2f;
    [SerializeField] private float timer = 5f;
    [Header("Dependancy")]
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
    [SerializeField] private Material timeOutMaterial;

    private Coroutine coroutineColor;
    private Coroutine coroutineTimer;
    private int tempIndex;


    public override void OnEpisodeBegin()
    {
        tempIndex = rewardCount;
        transform.localPosition
            = new Vector3(Random.Range(-4f, 4f), 0.25f, Random.Range(-4f, 4f));

        OnClearOldRewards();
        OnSpawnRewards();

        if (coroutineTimer != null)
            StopCoroutine(coroutineTimer);
        coroutineTimer = StartCoroutine(OnStartEpisodeTimer());
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
        for (int i = 0; i < rewardCount; i++)
        {
            int counter = 0;
            bool distaceIsGood;
            bool alreadyDecrimented = false;

            GameObject newReward = Instantiate(prefab);
            newReward.transform.parent = prefabParent.transform;
            Vector3 randPosition = new Vector3(Random.Range(-4f, 4f), 0.25f, Random.Range(-4f, 4f));

            if (prefabs.Count != 0)
            {
                for (int j = 0; j < prefabs.Count; j++)
                {
                    if (counter < 10)
                    {
                        distaceIsGood = OnCheckForSpwanOverLap(randPosition, prefabs[j].transform.localPosition, 5f);
                        if (!distaceIsGood)
                        {
                            randPosition = new Vector3(Random.Range(-4f, 4f), 0.25f, Random.Range(-4f, 4f));
                            j--;
                            alreadyDecrimented = true;
                        }

                        distaceIsGood = OnCheckForSpwanOverLap(randPosition, transform.localPosition, 5f);
                        if (!distaceIsGood)
                        {
                            randPosition = new Vector3(Random.Range(-4f, 4f), 0.25f, Random.Range(-4f, 4f));
                            if (!alreadyDecrimented)
                                j--;
                        }

                        counter++;
                    }
                    else
                        break;
                }
            }

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

            if (prefabs.Count != tempIndex)
            {
                Debug.Log("+");
                AddReward(reward);
                tempIndex = prefabs.Count;
            }

            if (prefabs.Count == 0)
            {
                OnClearCoRoutine();
                coroutineColor = StartCoroutine(ColorChange(winMaterial));
                EndEpisode();
            }

        }
        else if (other.gameObject.tag == "Wall")
        {
            Debug.Log("-");
            AddReward(punishment);
            OnClearCoRoutine();
            coroutineColor = StartCoroutine(ColorChange(looseMaterial));
            EndEpisode();
        }
    }

    private void OnClearCoRoutine()
    {
        if (coroutineColor != null)
        {
            coroutineColor = null;
            render.material = blankMaterial;
        }
    }

    private bool OnCheckForSpwanOverLap(Vector3 target, Vector3 prevObj, float minDistace)
    {
        float distanceBetweenObjects = Vector3.Distance(target, prevObj);
        if (distanceBetweenObjects <= minDistace)
            return true;
        return false;
    }

    IEnumerator ColorChange(Material mat)
    {
        render.material = mat;
        yield return new WaitForSeconds(2f);
        render.material = blankMaterial;
    }

    IEnumerator OnStartEpisodeTimer()
    {
        yield return new WaitForSeconds(timer);

        StartCoroutine(ColorChange(timeOutMaterial));
        AddReward(punishment);
        EndEpisode();
    }
}