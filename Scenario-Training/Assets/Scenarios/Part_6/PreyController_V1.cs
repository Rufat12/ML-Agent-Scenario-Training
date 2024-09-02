using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using System.Collections;
using System.Collections.Generic;


public class PreyController_V1: Agent
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

    [Header("OtherAgents")]
    [SerializeField] private HunterController_V1 hunter;

    Coroutine coroutineColor;
    Coroutine coroutineTimer;


    public override void OnEpisodeBegin()
    {
        transform.localPosition
            = new Vector3(Random.Range(-4f, 4f), 0.25f, Random.Range(-4f, 4f));

        OnClearOldRewards();
        OnSpawnRewards();

        if(coroutineTimer != null)
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

            Vector3 randPosition = new Vector3(Random.Range(-4f, 4f), 0.25f, Random.Range(-4f, 4f));

            if (prefabs.Count != 0)
            {
                for (int j = 0; j < prefabs.Count; j++)
                {
                    if (counter < 10)
                    {
                        distaceIsGood = OnCheckForOverLap(randPosition, prefabs[j].transform.localPosition, 5f);
                        if (!distaceIsGood)
                        {
                            randPosition = new Vector3(Random.Range(-4f, 4f), 0.25f, Random.Range(-4f, 4f));
                            j--;
                            alreadyDecrimented = true;
                        }

                        distaceIsGood = OnCheckForOverLap(randPosition, transform.localPosition, 5f);
                        if (!distaceIsGood)
                        {
                            randPosition = new Vector3(Random.Range(-4f, 4f), 0.25f, Random.Range(-4f, 4f));
                            if (!alreadyDecrimented)
                                j--;
                        }

                        counter++;

                        if (distaceIsGood)
                            break;
                    }
                    else
                        break;
                }
            }

            GameObject newReward = Instantiate(prefab);
            newReward.transform.parent = prefabParent.transform;
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
            Debug.Log("Prey Win");
            prefabs.Remove(other.gameObject);
            Destroy(other.gameObject);
            AddReward(reward);
            hunter.AddReward(punishment);

            if (prefabs.Count == 0)
            {
                OnClearCoRoutine();
                coroutineColor = StartCoroutine(ColorChange(winMaterial));
                hunter.EndEpisode();
                EndEpisode();
            }

        }
        else if (other.gameObject.tag == "Wall")
        {
            Debug.Log("Prey Hit Wall");
            AddReward(punishment);
            OnClearCoRoutine();
            coroutineColor = StartCoroutine(ColorChange(looseMaterial));
            hunter.EndEpisode();
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

    internal bool OnCheckForOverLap(Vector3 target, Vector3 prevObj, float minDistace)
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

    internal void OnInvokeColorChange(Material mat) => coroutineColor = StartCoroutine(ColorChange(mat));

    IEnumerator OnStartEpisodeTimer()
    {
        yield return new WaitForSeconds(timer);

        Debug.Log("Timer Run Out!");
        StartCoroutine(ColorChange(timeOutMaterial));
        hunter.AddReward(punishment);
        AddReward(punishment);
        hunter.EndEpisode();
        EndEpisode();
    }
}