using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class CleanerAgent : Agent
{
    public float maxSpeed = 60f;
    public float rotationMaxSpeed = 10f;
    public float spawnRange = 200f;
    public float maxDetectionDistance = 150f;
    public float minDetectionDistance = 90;
    public const int numberOfSensors = 3;
    private Rigidbody rb;
    public GoalService goalService;

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
        this.transform.position = new Vector3(
            Random.Range(-spawnRange, spawnRange), 
            3f, 
            Random.Range(-spawnRange, spawnRange)
        );
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
            AddReward(10f);
            goalService.RemoveGoal(gameObject);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        GameObject gameObject = collision.gameObject;
        if (gameObject.tag == "wall")
        {
            SetReward(-10f);
            EndEpisode();
        }
    }

    public override void OnEpisodeBegin()
    {
        this.transform.position = new Vector3(Random.Range(-spawnRange, spawnRange), 3f, Random.Range(-spawnRange, spawnRange));
    }


    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxis("Vertical");
        continuousActions[1] = Input.GetAxis("Horizontal");
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float vertical = actions.ContinuousActions[0];
        float horizontal = actions.ContinuousActions[1];
        rb.AddForce(transform.forward * vertical * maxSpeed);
        Quaternion rotation = Quaternion.Euler(0f, horizontal * rotationMaxSpeed * Time.deltaTime, 0f);
        rb.MoveRotation(rb.rotation * rotation);
        ControlSpeed();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        //sensor.AddObservation(transform.localPosition);
        var sensorsDetectionInfo = CollectDetectionInfo();
        sensor.AddObservation(sensorsDetectionInfo.ConvertAll<float>(info => info.Key));
        sensor.AddObservation(sensorsDetectionInfo.ConvertAll<float>(info => info.Value));
    }

    public List<KeyValuePair<float, float>> CollectDetectionInfo()
    {
        var info = new List<KeyValuePair<float, float>>();
        info.Add(DetectWithRay(transform.position, Quaternion.AngleAxis(-18, transform.up) * transform.forward, maxDetectionDistance));
        info.Add(DetectWithRay(transform.position, Quaternion.AngleAxis(-12, transform.up) * transform.forward, maxDetectionDistance));
        info.Add(DetectWithRay(transform.position, transform.forward * 1f, maxDetectionDistance));
        info.Add(DetectWithRay(transform.position, Quaternion.AngleAxis(12, transform.up) * transform.forward, maxDetectionDistance));
        info.Add(DetectWithRay(transform.position, Quaternion.AngleAxis(18, transform.up) * transform.forward, maxDetectionDistance));
        
        info.Add(DetectWithRay(transform.position, Quaternion.AngleAxis(-12, transform.up) * transform.forward * -1f, minDetectionDistance));
        info.Add(DetectWithRay(transform.position, transform.forward * -1f, minDetectionDistance));
        info.Add(DetectWithRay(transform.position, Quaternion.AngleAxis(12, transform.up) * transform.forward * -1f, minDetectionDistance));
        //Debug.Log(info[0].Key + ", " + info[1].Key + ", " + info[2].Key + ", " + info[3].Key + ", " + info[4].Key + ", " + info[5].Key + ", " + info[6].Key + ", " + info[7].Key);
        //Debug.Log(info[0].Value + ", " + info[1].Value + ", " + info[2].Value + ", " + info[3].Value + ", " + info[4].Value + ", " + info[5].Value + ", " + info[6].Value + ", " + info[7].Value);
        return info;
    }

    private KeyValuePair<float, float> DetectWithRay(Vector3 position, Vector3 direction, float detectionDistance)
    {
        var sensorInfo = new KeyValuePair<float, float>(0f, -1f); // nothing
        var ray = new Ray(position, direction);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, detectionDistance))
        {
            hit.transform.gameObject.GetComponent<HitLogic>().needChange = true;
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
