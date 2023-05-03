using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GoalService : MonoBehaviour
{

    public int goals;
    public int spawnRange;
    public int goalsOnScene { get; set; }
    public float goalsLifeTime;
    private float goalsLifeTimeRemaining;

    void Start()
    {
        CleanupGoals();
        goalsLifeTimeRemaining = goalsLifeTime;
        goalsOnScene = goals;
        for (int i = 0; i < goals; i++)
        {
            GameObject goal = Instantiate(
                GameObject.FindGameObjectWithTag("goal"), 
                new Vector3(Random.Range(-spawnRange, spawnRange), 5f, Random.Range(-spawnRange, spawnRange)), 
                Quaternion.Euler(0f, 0f, 0f)
            );
            goal.name = "Garbaga#" + i;
            goal.transform.SetParent(this.transform);
        }
    }

    void Update()
    {
        if (goalsOnScene == 0 || goalsLifeTimeRemaining < 0)
        {
            Start();
        }
        goalsLifeTimeRemaining -= Time.deltaTime;
    }

    private void CleanupGoals()
    {
        foreach (Transform child in this.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
    }

    public void RemoveGoal(GameObject goal)
    {
        goalsOnScene--;
        Destroy(goal);
    }

}
