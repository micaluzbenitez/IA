using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Genome
{
    public float[] genome = null;
    public float fitness = 0;

    public Genome(float[] genes)
    {
        this.genome = genes;
        fitness = 0;
    }

    public Genome(int genesCount)
    {
        genome = new float[genesCount];

        for (int i = 0; i < genesCount; i++)
            genome[i] = Random.Range(-1.0f, 1.0f);

        fitness = 0;
    }

    public Genome()
    {
        genome = null;
        fitness = 0;
    }
}

public class GeneticAlgorithm 
{
	List<Genome> population = new List<Genome>();
	List<Genome> newPopulation = new List<Genome>();

	float totalFitness = 0f;

	int eliteCount = 0;
	float mutationChance = 0.0f;
	float mutationRate = 0.0f;

	public GeneticAlgorithm(int eliteCount, float mutationChance, float mutationRate)
	{
		this.eliteCount = eliteCount;
		this.mutationChance = mutationChance;
		this.mutationRate = mutationRate;
	}

	public Genome[] GetRandomGenomes(int count, int genesCount)
	{
		Genome[] genomes = new Genome[count];

		for (int i = 0; i < count; i++)
		{
			genomes[i] = new Genome(genesCount);
		}

		return genomes;
	}

    // (11)
    /*
	Entre todos estos cálculos se realiza el Epoch(), mediante un Crossover() con una mama y un papa, con RouletteSelection() se elige un punto donde cortar el genoma de uno y arrancar el del otro 
	y generar una cría; esto es mediante un pivote, contas de un punto a otro para calcular el genoma de mama y papa, y cada genoma tiene una probabilidad de mutar ShouldMutate(), esto es porque 
	si solamente promediamos genomas llega un punto donde no se puede avanzar más, porque si nadie llega a la solución o está cerca en el primer random que hicimos, nunca se va a resolver el 
	problema, pero si cada tanto uno de los genes del genoma de uno de nuestros agentes muta, puede que esa mutación sea la que pueda resolver el problema, ya que esa mutación cambiaria un valor 
	flotante de su genoma, que va a hacer que cambie el peso de una neurona en específico, que va a hacer que el output en una mutación pueda dar de una u otra manera.
	*/
    public Genome[] Epoch(Genome[] oldGenomes)
	{
		totalFitness = 0f;

		population.Clear();
		newPopulation.Clear();

		population.AddRange(oldGenomes);
		population.Sort(HandleComparison);

		foreach (Genome g in population)
		{
			totalFitness += g.fitness;
		}

		SelectElite();

		while (newPopulation.Count < population.Count)
		{
			Crossover();
		}

		return newPopulation.ToArray();
	}

	private void SelectElite()
	{
		for (int i = 0; i < eliteCount && newPopulation.Count < population.Count; i++)
		{
			newPopulation.Add(population[i]);
		}
	}

	private void Crossover()
	{
		Genome mom = RouletteSelection();
		Genome dad = RouletteSelection();

        Genome child1;
        Genome child2;

        Crossover(mom, dad, out child1, out child2);

        newPopulation.Add(child1);
		newPopulation.Add(child2);
	}

	private void Crossover(Genome mom, Genome dad, out Genome child1, out Genome child2)
	{
		child1 = new Genome();
		child2 = new Genome();

		child1.genome = new float[mom.genome.Length];
		child2.genome = new float[mom.genome.Length];

		int pivot = Random.Range(0, mom.genome.Length);

		for (int i = 0; i < pivot; i++)
		{
			child1.genome[i] = mom.genome[i];

			if (ShouldMutate())
				child1.genome[i] += Random.Range(-mutationRate, mutationRate);

			child2.genome[i] = dad.genome[i];

			if (ShouldMutate())
				child2.genome[i] += Random.Range(-mutationRate, mutationRate);
		}

		for (int i = pivot; i < mom.genome.Length; i++)
		{
			child2.genome[i] = mom.genome[i];

			if (ShouldMutate())
				child2.genome[i] += Random.Range(-mutationRate, mutationRate);

			child1.genome[i] = dad.genome[i];

			if (ShouldMutate())
				child1.genome[i] += Random.Range(-mutationRate, mutationRate);
		}
	}

	private bool ShouldMutate()
	{
		return Random.Range(0.0f, 1.0f) < mutationChance;
	}

	private int HandleComparison(Genome x, Genome y)
	{
		return x.fitness > y.fitness ? 1 : x.fitness < y.fitness ? -1 : 0;
    }

    public Genome RouletteSelection()
    {
        float rnd = Random.Range(0, Mathf.Max(totalFitness, 0));

        float fitness = 0;

        for (int i = 0; i < population.Count; i++)
        {
            fitness += Mathf.Max(population[i].fitness, 0);
            if (fitness >= rnd) 
				return population[i];
        }

        return null;
    }
}