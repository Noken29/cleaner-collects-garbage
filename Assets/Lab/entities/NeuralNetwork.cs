using UnityEngine;
using System.Collections.Generic;
using System;

public class NeuralNetwork
{
    private int inputSize;
    private int outputSize;
    private int[] hiddenSizes;
    private float[][] neurons;
    private float[][][] weights;

    public NeuralNetwork(int inputSize, int outputSize, int[] hiddenSizes)
    {
        this.inputSize = inputSize;
        this.outputSize = outputSize;
        this.hiddenSizes = hiddenSizes;
        int numLayers = hiddenSizes.Length + 2;
        neurons = new float[numLayers][];
        weights = new float[numLayers - 1][][];
        neurons[0] = new float[inputSize + 1];
        for (int i = 0; i < hiddenSizes.Length; i++)
            neurons[i + 1] = new float[hiddenSizes[i] + 1]; 
        neurons[numLayers - 1] = new float[outputSize];
        for (int i = 0; i < numLayers - 1; i++)
        {
            int neuronsInCurrentLayer = neurons[i].Length;
            int neuronsInNextLayer = neurons[i + 1].Length;
            weights[i] = new float[neuronsInCurrentLayer][];
            for (int j = 0; j < neuronsInCurrentLayer; j++)
                weights[i][j] = new float[neuronsInNextLayer];
        }
        InitRandom();
    }

    private void InitRandom()
    {
        for (int i = 0; i < weights.Length; i++)
            for (int j = 0; j < weights[i].Length; j++)
                for (int k = 0; k < weights[i][j].Length; k++)
                    weights[i][j][k] = UnityEngine.Random.Range(-1f, 1f);
    }

    public float[] Eval(float[] inputs)
    {
        for (int i = 0; i < inputs.Length; i++) neurons[0][i] = inputs[i];
        int numLayers = neurons.Length;
        for (int i = 1; i < numLayers; i++)
        {
            int numNeuronsInPreviousLayer = neurons[i - 1].Length;
            int numNeuronsInCurrentLayer = neurons[i].Length - 1; 
            for (int j = 0; j < numNeuronsInCurrentLayer; j++)
            {
                float sum = 0f;
                for (int k = 0; k < numNeuronsInPreviousLayer; k++)
                    sum += neurons[i - 1][k] * weights[i - 1][k][j];
                neurons[i][j] = (float) Math.Tanh(sum);
            }
            neurons[i][numNeuronsInCurrentLayer] = 1f;
        }
        return neurons[numLayers - 1];
    }

    public float[][][] GetWeights()
    {
        return weights;
    }

    public void SetWeights(float[][][] newWeights)
    {
        weights = newWeights;
    }
}