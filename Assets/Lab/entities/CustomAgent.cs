using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomAgent : MonoBehaviour
{
    public GoalService goalService;
    
    public float maxSpeed = 60f;
    public float rotationMaxSpeed = 10f;
    public float spawnRange = 800f;
    public float maxDetectionDistance = 150f;
    public float minDetectionDistance = 90;
    public float agentLifeTime = 500;
    private float agentLifeTimeRemaining = 0;

    private Rigidbody rb;
    private float currentReward;
    public NeuralNetwork neuralNetwork;

    public void FixedUpdate()
    {
        agentLifeTimeRemaining += Time.deltaTime;
        if (agentLifeTimeRemaining >= agentLifeTime)
            ReInit(true);
        Move(neuralNetwork.Eval(CollectObservations()));
    }

    public void Start()
    {
        rb = GetComponent<Rigidbody>();
        currentReward = 0f;
        this.transform.position = new Vector3(
            Random.Range(-spawnRange, spawnRange),
            3f,
            Random.Range(-spawnRange, spawnRange)
        );
    }

    public void ReInit(bool useInactivityPenalty)
    {
        if (useInactivityPenalty)
            currentReward -= 10f;
        this.transform.position = new Vector3(Random.Range(-spawnRange, spawnRange), 3f, Random.Range(-spawnRange, spawnRange));
        agentLifeTimeRemaining = 0;
    }

    private void ControlSpeed()
    {
        Vector3 velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        if (velocity.magnitude > maxSpeed)
        {
            Vector3 limitedVelocity = velocity.normalized * maxSpeed;
            rb.velocity = new Vector3(limitedVelocity.x, 0f, limitedVelocity.z);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        GameObject gameObject = other.gameObject;
        if (gameObject.tag == "goal")
        {
            currentReward += 5f;
            goalService.RemoveGoal(gameObject);
        }
        if (gameObject.tag == "wall")
        {
            currentReward += -5f;
            ReInit(false);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        GameObject gameObject = collision.gameObject;
        if (gameObject.tag == "wall")
        {
            currentReward -= 3f;
        }
    }

    public void Move(float[] actions)
    {
        float vertical = actions[0];
        float horizontal = actions[1];
        rb.AddForce(transform.forward * vertical * maxSpeed);
        Quaternion rotation = Quaternion.Euler(0f, horizontal * rotationMaxSpeed * Time.deltaTime, 0f);
        rb.MoveRotation(rb.rotation * rotation);
        ControlSpeed();
    }

    public float[] CollectObservations()
    {
        var sensorsDetectionInfo = CollectDetectionInfo();
        List<float> sensorsInfo = new List<float>();
        sensorsInfo.AddRange(sensorsDetectionInfo.ConvertAll<float>(info => info.Key));
        sensorsInfo.AddRange(sensorsDetectionInfo.ConvertAll<float>(info => info.Value));
        return sensorsInfo.ToArray();
    }

    public float GetCurrentReward()
    {
        return currentReward;
    }

    private List<KeyValuePair<float, float>> CollectDetectionInfo()
    {
        var info = new List<KeyValuePair<float, float>>();
        info.Add(DetectWithRay(transform.position, Quaternion.AngleAxis(-20, transform.up) * transform.forward, maxDetectionDistance));
        info.Add(DetectWithRay(transform.position, Quaternion.AngleAxis(-12, transform.up) * transform.forward, maxDetectionDistance));
        info.Add(DetectWithRay(transform.position, transform.forward * 1f, maxDetectionDistance));
        info.Add(DetectWithRay(transform.position, Quaternion.AngleAxis(12, transform.up) * transform.forward, maxDetectionDistance));
        info.Add(DetectWithRay(transform.position, Quaternion.AngleAxis(20, transform.up) * transform.forward, maxDetectionDistance));
        info.Add(DetectWithRay(transform.position, Quaternion.AngleAxis(-16, transform.up) * transform.forward * -1f, minDetectionDistance));
        info.Add(DetectWithRay(transform.position, Quaternion.AngleAxis(-12, transform.up) * transform.forward * -1f, minDetectionDistance));
        info.Add(DetectWithRay(transform.position, transform.forward * -1f, minDetectionDistance));
        info.Add(DetectWithRay(transform.position, Quaternion.AngleAxis(12, transform.up) * transform.forward * -1f, minDetectionDistance));
        info.Add(DetectWithRay(transform.position, Quaternion.AngleAxis(16, transform.up) * transform.forward * -1f, minDetectionDistance));
        return info;
    }

    private KeyValuePair<float, float> DetectWithRay(Vector3 position, Vector3 direction, float detectionDistance)
    {
        var sensorInfo = new KeyValuePair<float, float>(0f, -1f); // nothing
        var ray = new Ray(position, direction);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, detectionDistance))
        {
            HitLogic hitLogic;
            hit.transform.gameObject.TryGetComponent<HitLogic>(out hitLogic);
            if (hitLogic)
                hitLogic.needChange = true;
            sensorInfo = GetDetectionTypeAndDistance(hit);
            sensorInfo = new KeyValuePair<float, float>(sensorInfo.Key, sensorInfo.Value / detectionDistance);
        }
        return sensorInfo;
    }

    private KeyValuePair<float, float> GetDetectionTypeAndDistance(RaycastHit hit)
    {
        float type = 0f;
        switch (hit.transform.gameObject.tag)
        {
            case "wall":
                type = -1f;
                break;
            case "goal":
                type = 1f;
                break;
        }
        float distance = Vector3.Distance(transform.position, hit.transform.position);
        return new KeyValuePair<float, float>(type, distance);
    }
}
