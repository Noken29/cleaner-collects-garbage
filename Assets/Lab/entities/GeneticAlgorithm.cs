using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class GeneticAlgorithm : MonoBehaviour
{
    public int populationSize;
    public float mutationRate = 0.08f;
    public int inputSize = 20;
    public int outputSize = 2;
    public int[] hiddenSizes = {9, 5};
    public float populationLifeTime = 50;
    private float populationLifeTimeRemaining = 0;

    private List<CustomAgent> population;

    private void Start()
    {
        population = new List<CustomAgent>();
        foreach (Transform agent in transform)
            population.Add(agent.gameObject.GetComponent<CustomAgent>());
        populationSize = population.Count;
        for (int i = 0; i < populationSize; i++)
            population[i].neuralNetwork = new NeuralNetwork(inputSize, outputSize, hiddenSizes);
    }

    private void FixedUpdate()
    {
        populationLifeTimeRemaining += Time.deltaTime;
        if (populationLifeTimeRemaining > populationLifeTime)
        {
            Debug.Log("Average reward: " + (population.Select(e => EvalFitness(e)).Sum() / populationSize));
            TrainPopulation();
            populationLifeTimeRemaining = 0;
        }
    }

    private void TrainPopulation()
    {
        List<NeuralNetwork> newPopulation = MakeNewGeneration();
        for (int i = 0; i < populationSize; i++)
            population[i].neuralNetwork = newPopulation[i];
    }

    private float EvalFitness(CustomAgent agent)
    {
        return agent.GetCurrentReward();
    }

    private List<NeuralNetwork> MakeNewGeneration()
    {
        population.Sort((a, b) => EvalFitness(b).CompareTo(EvalFitness(a)));
        List<NeuralNetwork> parents = population.ConvertAll<NeuralNetwork>(agent => agent.neuralNetwork).GetRange(0, populationSize / 2);
        List<NeuralNetwork> newGeneration = new List<NeuralNetwork>();
        newGeneration.Add(parents[0]);
        while (newGeneration.Count < populationSize)
        {
            NeuralNetwork parent1 = parents[Random.Range(0, parents.Count)];
            NeuralNetwork parent2 = parents[Random.Range(0, parents.Count)];
            NeuralNetwork child = MakeCrossover(parent1, parent2);
            MakeMutation(child);
            newGeneration.Add(child);
        }
        return newGeneration;
    }

    private NeuralNetwork MakeCrossover(NeuralNetwork parent1, NeuralNetwork parent2)
    {
        float[][][] parent1Weights = parent1.GetWeights();
        float[][][] parent2Weights = parent2.GetWeights();
        float[][][] childWeights = new float[parent1Weights.Length][][];
        for (int i = 0; i < parent1Weights.Length; i++)
        {
            childWeights[i] = new float[parent1Weights[i].Length][];
            for (int j = 0; j < parent1Weights[i].Length; j++)
            {
                childWeights[i][j] = new float[parent1Weights[i][j].Length];
                for (int k = 0; k < parent1Weights[i][j].Length; k++)
                {
                    childWeights[i][j][k] = Random.Range(0f, 1f) < 0.5f ? parent1Weights[i][j][k] : parent2Weights[i][j][k];
                }
            }
        }
        NeuralNetwork child = new NeuralNetwork(inputSize, outputSize, hiddenSizes);
        child.SetWeights(childWeights);
        return child;
    }

    private void MakeMutation(NeuralNetwork network)
    {
        float[][][] weights = network.GetWeights();
        for (int i = 0; i < weights.Length; i++)
            for (int j = 0; j < weights[i].Length; j++)
                for (int k = 0; k < weights[i][j].Length; k++)
                    if (Random.Range(0f, 1f) < mutationRate)
                        weights[i][j][k] += Random.Range(-0.1f, 0.1f);
        network.SetWeights(weights);
    }
}

