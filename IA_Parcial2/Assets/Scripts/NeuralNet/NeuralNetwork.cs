using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NeuralNetwork
{
	public List<NeuronLayer> layers = new List<NeuronLayer>();
	public int totalWeightsCount = 0;
	public int inputsCount = 0;

	public int InputsCount
	{
		get { return inputsCount; }
    }

    public NeuralNetwork()
    {
    }

    public bool AddNeuronLayer(int neuronsCount, float bias, float p)
	{
		if (layers.Count == 0)
		{
			Debug.LogError("Call AddFirstNeuronLayer(int inputsCount, float bias, float p) for the first layer.");
			return false;
		}

		return AddNeuronLayer(layers[layers.Count - 1].OutputsCount, neuronsCount, bias, p);
	}

	public bool AddFirstNeuronLayer(int inputsCount, float bias, float p)
	{
		if (layers.Count != 0)
		{
			Debug.LogError("Call AddNeuronLayer(int neuronCount, float bias, float p) for the rest of the layers.");
			return false;
		}

		this.inputsCount = inputsCount;

		return AddNeuronLayer(inputsCount, inputsCount, bias, p);
	}

	private bool AddNeuronLayer(int inputsCount, int neuronsCount, float bias, float p)
	{
		if (layers.Count > 0 && layers[layers.Count - 1].OutputsCount != inputsCount)
		{
			Debug.LogError("Inputs Count must match outputs from previous layer.");
			return false;
		}

		NeuronLayer layer = new NeuronLayer(inputsCount, neuronsCount, bias, p);

		totalWeightsCount += (inputsCount + 1) * neuronsCount;

		layers.Add(layer);

		return true;
	}

	public int GetTotalWeightsCount()
	{
		return totalWeightsCount;
	}

	public void SetWeights(float[] newWeights)
	{
		int fromId = 0;

		for (int i = 0; i < layers.Count; i++)
		{
			fromId = layers[i].SetWeights(newWeights, fromId);
		}
	}

	public float[] GetWeights()
	{
		float[] weights = new float[totalWeightsCount];
		int id = 0;

		for (int i = 0; i < layers.Count; i++)
		{
			float[] ws = layers[i].GetWeights();

			for (int j = 0; j < ws.Length; j++)
			{
				weights[id] = ws[j];
				id++;
			}
		}

		return weights;
	}

    // (5) Para que todo ocurra hay mas de una neurona = "redes neuronales"
    // Una red neuronal es un conjunto de neuronas que recibe un input, lo procesa, lo manda a otra neurona,
    // lo procesa, y asi cuantas veces se quieran hasta devolverlo como un ouput
    public float[] Synapsis(float[] inputs) // Metodo synapsis de brain
    {
        // (6) Recibo los inputs y segun la cantidad de layers, me guardo el output del layer en cuestion,
        // lo convierto en input para la siguiente, y se lo paso al siguiente layer
        // Haciendo asi que los inputs se sumen y se promedien

        float[] outputs = null;

		for (int i = 0; i < layers.Count; i++)
		{
			outputs = layers[i].Synapsis(inputs);
			inputs = outputs;
		}

		return outputs;
	}

	// (7) Hay una cantidad de neurones como inputs haya
	// Luego estan los layers (cantidad de neuronas entre medio del input y output), puede haber la cantidad que se quiera con la cantidad de neurones que se quieran
	// Y por ultimo hay una cantidad de neuronas como outputs haya
}