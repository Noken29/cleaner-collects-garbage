using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GoalService : MonoBehaviour
{

    public int goals;
    public int spawnRange;
    public int goalsOnScene { get; set; }
   
    void Start()
    {
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
        if (goalsOnScene == 0)
        {
            Start();
        }
    }

    public void RemoveGoal(GameObject goal)
    {
        goalsOnScene--;
        Destroy(goal);
    }

}
