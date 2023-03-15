using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotController : MonoBehaviour
{
    public float maxSpeed = 60f;
    public float rotationMaxSpeed = 10f;
    public float spawnRange = 200;
    private Rigidbody rb;
    public GoalService goalService;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        this.transform.position = new Vector3(Random.Range(-spawnRange, spawnRange), 3f, Random.Range(-spawnRange, spawnRange));
    }

    void Update()
    {
        ControlSpeed();
    }

    void FixedUpdate()
    {
        if (rb)
        {
            HandleMovement();
        }
    }

    protected virtual void HandleMovement()
    {
       rb.AddForce(transform.forward * Input.GetAxis("Vertical") * maxSpeed);

        Quaternion rotation = Quaternion.Euler(0f, Input.GetAxis("Horizontal") * rotationMaxSpeed * Time.deltaTime, 0f);
        rb.MoveRotation(rb.rotation * rotation);
    }

    protected virtual void ControlSpeed()
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
        GameObject goal = other.gameObject;
        if (goal.tag == "goal")
        {
            goalService.DecreaseGoalsNumber();
            Destroy(goal);
        }
    }

}
