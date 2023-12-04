using UnityEngine;

public enum TEAM
{
    A,
    B
}

public abstract class AgentBase : MonoBehaviour
{
    protected Genome genome = null;
    protected NeuralNetwork brain = null;

    protected TEAM team = TEAM.A;
    protected float[] inputs = null;
    protected float fitness = 1f;

    public Genome Genome { get => genome; }
    public NeuralNetwork Brain { get => brain; }
    public float Fitness { get => fitness; }

    public void SetBrain(Genome genome, NeuralNetwork brain)
    {
        this.genome = genome;
        this.brain = brain;
        inputs = new float[brain.InputsCount];
        OnReset();
    }

    // (3) El cerebro del objeto (neurona) es el encargado de procesar los inputs y generar el output
    // Estos van aprendiendo con el tiempo y mejorando ese resultado
    public void OnThink()
    {
        ProcessInputs();
        float[] outputs = brain.Synapsis(inputs); // <-- (5)
        ProcessOutputs(outputs);

        // (4) El output luego va a uno de los inputs (puede ir a uno o a uno de varias neuronas, pero siempre solo a un input)
    }

    // (14) Por otro lado esta el fitness, que es la manera del programador de puntuar a cada agente madiente con bien lo hizo o no
    // Mientras mas fitness, mas chances de reproducirse tiene el agente
    // Los que lo hicieron mejor de la generacion anteorir, continuan en la siguiente (elites)
    public void UpdateFitness(float fitness)
    {
        this.fitness += fitness;
        SetGenomeFitness();
    }

    public void SetFitness(float fitness)
    {
        this.fitness = fitness;
        SetGenomeFitness();
    }

    protected virtual void OnReset()
    {
        fitness = 1f;
    }

    // (1) Nuestro objeto recibe una serie de inputs (informacion que le queremos pasar) y da un output
    // Recibe de manera random como procesar los inputs a la hora de convertirlos en un output
    protected abstract void ProcessInputs();

    // (2) Output: entre cuantos valores se distribuye el procesamiento neuronal que hago, todas las synapsis
    protected abstract void ProcessOutputs(float[] outputs);

    private void SetGenomeFitness()
    {
        genome.fitness = fitness;
    }
}