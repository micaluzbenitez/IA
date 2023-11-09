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

    public void OnThink()
    {
        ProcessInputs();
        float[] outputs = brain.Synapsis(inputs);
        ProcessOutputs(outputs);
    }

    public void UpdateFitness(float fitness)
    {
        this.fitness *= fitness;
        if (this.fitness < 1f) this.fitness = 1f;
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

    protected abstract void ProcessInputs();

    protected abstract void ProcessOutputs(float[] outputs);

    private void SetGenomeFitness()
    {
        genome.fitness = fitness;
    }
}