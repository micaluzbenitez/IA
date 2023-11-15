using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

public class PopulationManager : MonoBehaviour
{
    [Serializable]
    public class Team
    {
        public string Tag = "";
        public int EliteCount = 0;
        public float MutationChance = 0f;
        public float MutationRate = 0f;
        public int InputsCount = 0;
        public int HiddenLayers = 0;
        public int OutputsCount = 0;
        public int NeuronsCountPerHL = 0;
        public float Bias = 0f;
        public float P = 0f;

        [HideInInspector] public int agents = 0;
        [HideInInspector] public int foods = 0;
        [HideInInspector] public int deaths = 0;
        [HideInInspector] public int extincts = 0;
    }

    [Header("Prefabs")]
    public GameObject agent1Prefab = null;
    public GameObject agent2Prefab = null;
    public GameObject foodPrefab = null;

    [Header("UI")]
    public StartConfigurationScreen startConfigurationScreen = null;
    public SimulationScreen simulationScreen = null;

    [Header("General Settings")]
    public float Unit = 0f;
    public float TurnsDelay = 0f;
    public int PopulationCount = 0; // Cantidad de agentes
    public int Turns = 0;
    public int IterationCount = 0;
    public int AgentMaxGeneration = 0;
    public float DeathChance = 0f;
    public float PlusMutationRate = 0f;

    [Header("Teams")]
    public Team[] teams = null;

    [Header("Data")]
    public TextAsset brainDataJson = null;

    GeneticAlgorithm genAlgA = null;
    GeneticAlgorithm genAlgB = null;

    List<Genome> populations = new List<Genome>();
    List<Genome> populationsA = new List<Genome>();
    List<Genome> populationsB = new List<Genome>();

    private List<Agent> agents = new List<Agent>();
    private List<Food> foods = new List<Food>();
    private int size = 0;
    private int foodSize = 0;
    private float turnsTimer = 0f;
    private bool isRunning = false;

    public int generation { get; private set; }
    public int turnsLeft { get; set; }
    public float bestFitness { get; private set; }
    public float avgFitness { get; private set; }
    public float worstFitness { get; private set; }

    [HideInInspector] public int agentNro = 0;
    [HideInInspector] public string agentTeam = "";
    [HideInInspector] public float agentFitness = 0f;
    [HideInInspector] public int agentFoodsConsumed = 0;
    [HideInInspector] public int agentGeneration = 0;
    [HideInInspector] public int row = 0;
    [HideInInspector] public int column = 0;
    [HideInInspector] public int totalAgents = 0;
    [HideInInspector] public int totalFoodsConsumed = 0;
    [HideInInspector] public int totalDeaths = 0;

    private string dataPath = "";
    private string fileName = "/Data/brain_data.json";

    private float getBestFitness()
    {
        float fitness = 0f;
        foreach (Genome g in populations)
        {
            if (fitness < g.fitness)
            {
                fitness = g.fitness;
            }
        }

        return fitness;
    }

    private float getAvgFitness()
    {
        float fitness = 0f;
        foreach (Genome g in populations)
        {
            fitness += g.fitness;
        }
        fitness = populations.Count == 0 ? 0f : fitness / populations.Count;

        return fitness;
    }

    private float getWorstFitness()
    {
        float fitness = populations.Count == 0 ? 0f : float.MaxValue;
        foreach (Genome g in populations)
        {
            if (fitness > g.fitness)
            {
                fitness = g.fitness;
            }
        }

        return fitness;
    }

    static PopulationManager instance = null;

    public static PopulationManager Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType<PopulationManager>();

            return instance;
        }
    }

    void Awake()
    {
        instance = this;

        generation = 0;
        turnsLeft = 0;

        bestFitness = 0f;
        avgFitness = 0f;
        worstFitness = 0f;

        agentNro = 0;
        agentTeam = string.Empty;
        agentFitness = 0f;
        agentFoodsConsumed = 0;
        agentGeneration = 0;
        row = 0;
        column = 0;

        totalAgents = 0;
        totalFoodsConsumed = 0;
        totalDeaths = 0;

        teams[0].agents = 0;
        teams[0].foods = 0;
        teams[0].deaths = 0;

        teams[1].agents = 0;
        teams[1].foods = 0;
        teams[1].deaths = 0;

        dataPath = Application.dataPath;
    }

    private void Start()
    {
        startConfigurationScreen.Init(StartGame);
        simulationScreen.Init(PauseGame, StopSimulation);
    }

    public void StartSimulation(List<Agent> agents, bool dataLoaded)
    {
        // Create and confiugre the Genetic Algorithm
        genAlgA = new GeneticAlgorithm(teams[0].EliteCount, teams[0].MutationChance, teams[0].MutationRate);
        genAlgB = new GeneticAlgorithm(teams[1].EliteCount, teams[1].MutationChance, teams[1].MutationRate);

        generation = dataLoaded ? generation : 0;
        turnsLeft = Turns;

        bestFitness = 0f;
        avgFitness = 0f;
        worstFitness = 0f;

        if (dataLoaded) GenerateInitialPopulationWithLoadData(agents);
        else GenerateInitialPopulation(agents);

        populations.AddRange(populationsA);
        populations.AddRange(populationsB);

        totalAgents = populations.Count;
        teams[0].agents = populationsA.Count;
        teams[1].agents = populationsB.Count;

        totalFoodsConsumed = 0;
        totalDeaths = 0;

        teams[0].foods = 0;
        teams[0].deaths = 0;
        teams[0].extincts = 0;

        teams[1].foods = 0;
        teams[1].deaths = 0;
        teams[1].extincts = 0;
    }
    private void StartGame(bool dataLoaded)
    {
        startConfigurationScreen.Active(false);
        simulationScreen.Active(true);

        size = PopulationCount;
        foodSize = size * 2;

        SpawnAgents(dataLoaded);
        SpawnFoods();

        StartSimulation(agents, dataLoaded);

        SetAgentsPositions();
        ProcessAgents();

        isRunning = true;
    }

    private void PauseGame()
    {
        isRunning = !isRunning;
    }

    private void StopSimulation()
    {
        isRunning = false;

        DestroyAgents();
        DestroyFoods();

        startConfigurationScreen.Active(true);
        simulationScreen.Active(false);
    }

    // Generate initial population with load data
    private void GenerateInitialPopulationWithLoadData(List<Agent> agents)
    {
        List<Agent> agentsA = agents.FindAll(c => c.Team == TEAM.A);
        List<Agent> agentsB = agents.FindAll(c => c.Team == TEAM.B);

        // Set the new genomes as each NeuralNetwork weights
        for (int i = 0; i < agentsA.Count; i++)
        {
            NeuralNetwork brain = CreateBrainA();
            Genome genome = populationsA[i];

            brain.SetWeights(genome.genome);

            agentsA[i].SetBrain(genome, brain);
        }

        // Set the new genomes as each NeuralNetwork weights
        for (int i = 0; i < agentsB.Count; i++)
        {
            NeuralNetwork brain = CreateBrainB();
            Genome genome = populationsB[i];

            brain.SetWeights(genome.genome);

            agentsB[i].SetBrain(genome, brain);
        }
    }

    // Generate the random initial population
    private void GenerateInitialPopulation(List<Agent> agents)
    {
        populationsA.Clear();
        populationsB.Clear();

        for (int i = 0; i < agents.Count; i++)
        {
            NeuralNetwork brain = null;
            Genome genome = null;

            // Set the new genomes as each NeuralNetwork weights
            if (agents[i].Team == TEAM.A)
            {
                brain = CreateBrainA();
                genome = new Genome(brain.GetTotalWeightsCount());

                brain.SetWeights(genome.genome);
                populationsA.Add(genome);
            }
            else if (agents[i].Team == TEAM.B)
            {
                brain = CreateBrainB();
                genome = new Genome(brain.GetTotalWeightsCount());

                brain.SetWeights(genome.genome);
                populationsB.Add(genome);
            }

            agents[i].SetBrain(genome, brain);
        }
    }

    // Creates a new NeuralNetwork
    NeuralNetwork CreateBrainA()
    {
        NeuralNetwork brain = new NeuralNetwork();

        // Add first neuron layer that has as many neurons as inputs
        brain.AddFirstNeuronLayer(teams[0].InputsCount, teams[0].Bias, teams[0].P);

        for (int i = 0; i < teams[0].HiddenLayers; i++)
        {
            // Add each hidden layer with custom neurons count
            brain.AddNeuronLayer(teams[0].NeuronsCountPerHL, teams[0].Bias, teams[0].P);
        }

        // Add the output layer with as many neurons as outputs
        brain.AddNeuronLayer(teams[0].OutputsCount, teams[0].Bias, teams[0].P);

        return brain;
    }

    // Creates a new NeuralNetwork
    NeuralNetwork CreateBrainB()
    {
        NeuralNetwork brain = new NeuralNetwork();

        // Add first neuron layer that has as many neurons as inputs
        brain.AddFirstNeuronLayer(teams[1].InputsCount, teams[1].Bias, teams[1].P);

        for (int i = 0; i < teams[1].HiddenLayers; i++)
        {
            // Add each hidden layer with custom neurons count
            brain.AddNeuronLayer(teams[1].NeuronsCountPerHL, teams[1].Bias, teams[1].P);
        }

        // Add the output layer with as many neurons as outputs
        brain.AddNeuronLayer(teams[1].OutputsCount, teams[1].Bias, teams[1].P);

        return brain;
    }

    // Evolve!!!
    public void Epoch(List<Agent> agents, Action<Genome[], NeuralNetwork[], TEAM> onCreateNewAgents)
    {
        generation++;  // Increment generation counter
        turnsLeft = Turns;

        // Calculate best, average and worst fitness
        bestFitness = getBestFitness();
        avgFitness = getAvgFitness();
        worstFitness = getWorstFitness();

        // Clear current population
        populations.Clear();
        populationsA.Clear();
        populationsB.Clear();

        ExtinctAgents(agents);
        SurvivingAgents(agents);
        BreeadingAgents(agents, onCreateNewAgents);
        CreateNewTeamAgents(agents, onCreateNewAgents);

        // Add new population
        populations.AddRange(populationsA);
        populations.AddRange(populationsB);

        totalAgents = populations.Count;
        teams[0].agents = populationsA.Count;
        teams[1].agents = populationsB.Count;

        teams[0].foods = 0;
        teams[0].deaths = 0;

        teams[1].foods = 0;
        teams[1].deaths = 0;
    }

    void FixedUpdate()
    {
        if (!isRunning) return;

        if (agents.Count == 0 || foods.Count == 0) isRunning = false; // End game

        for (int i = 0; i < Mathf.Clamp(IterationCount / 100.0f * 50f, 1f, 50f); i++)
        {
            turnsTimer += Time.fixedDeltaTime;
            UpdateMoveChaimbots(turnsTimer / TurnsDelay);

            if (turnsTimer > TurnsDelay)
            {
                turnsTimer = 0f;

                ProcessAgents();
                ProcessAgentsInSameIndex();

                turnsLeft--;

                if (turnsLeft <= 0) ResetSimulation();
            }
        }
    }

    #region Helpers
    private void SpawnAgents(bool dataLoaded)
    {
        int totalAgentsA = PopulationCount;
        int totalAgentsB = PopulationCount;

        if (dataLoaded)
        {
            totalAgentsA = GetPopulationACount();
            totalAgentsB = GetPopulationBCount();
        }

        for (int i = 0; i < totalAgentsA; i++)
        {
            GameObject agentGO = Instantiate(agent1Prefab);
            agentGO.transform.SetParent(transform);
            Agent agent = agentGO.GetComponent<Agent>();
            agent.Init(Unit, size, TEAM.A);

            agents.Add(agent);
        }

        for (int i = 0; i < totalAgentsB; i++)
        {
            GameObject agentGO = Instantiate(agent2Prefab);
            agentGO.transform.SetParent(transform);
            Agent agent = agentGO.GetComponent<Agent>();
            agent.Init(Unit, size, TEAM.B);

            agents.Add(agent);
        }
    }

    private void SpawnNewAgents(Genome[] newGenomes, NeuralNetwork[] brains, TEAM team)
    {
        for (int i = 0; i < newGenomes.Length; i++)
        {
            GameObject agentGO = null;
            if (team == TEAM.A) agentGO = Instantiate(agent1Prefab);
            else agentGO = Instantiate(agent2Prefab);

            agentGO.transform.SetParent(this.transform);
            Agent agent = agentGO.GetComponent<Agent>();
            agent.Init(Unit, size, team);

            NeuralNetwork brain = brains[i];
            brain.SetWeights(newGenomes[i].genome);
            agent.SetBrain(newGenomes[i], brain);

            agents.Add(agent);
        }
    }

    private void SpawnFoods()
    {
        List<Vector2Int> foodUsedIndexs = new List<Vector2Int>();

        for (int i = 0; i < foodSize; i++)
        {
            GameObject foodGO = Instantiate(foodPrefab);
            foodGO.transform.SetParent(transform);
            Food food = foodGO.GetComponent<Food>();

            Vector2Int foodIndex = GetRandomIndex(foodUsedIndexs.ToArray());

            Vector3 startPosition = new Vector3(-size / 2f, 0.5f, -size / 2f);
            food.transform.position = (startPosition + new Vector3(foodIndex.x, 0f, foodIndex.y)) * Unit;
            food.Init(foodIndex);

            foods.Add(food);
            foodUsedIndexs.Add(foodIndex);
        }
    }

    private void SetAgentsPositions()
    {
        Vector3 startPosition = new Vector3(-size / 2f, 0f, -size / 2f);
        int aIndexX = 0;
        int aIndexY = 0;
        int bIndexX = size;
        int bIndexY = size;

        for (int i = 0; i < agents.Count; i++)
        {
            Vector2Int index;

            if (agents[i].Team == TEAM.A)
            {
                index = new Vector2Int(aIndexX, aIndexY);
                aIndexX++;

                if (aIndexX > size)
                {
                    aIndexX = 0;
                    aIndexY++;
                }
            }
            else
            {
                index = new Vector2Int(bIndexX, bIndexY);
                bIndexX--;

                if (bIndexX < 0)
                {
                    bIndexX = size;
                    bIndexY--;
                }
            }

            agents[i].Index = index;
            agents[i].transform.position = (startPosition + new Vector3(index.x, 0f, index.y)) * Unit;
            agents[i].ResetData();
        }
    }

    private void ProcessAgents()
    {
        for (int i = 0; i < agents.Count; i++)
        {
            // Check limit Y or dead
            if (!(agents[i].Index.y >= 0 && agents[i].Index.y <= size) || agents[i].Dead) continue;

            agents[i].SetNearFood(GetNearFood(agents[i].transform.position));
            agents[i].Think();
        }
    }

    private void ProcessAgentsInSameIndex()
    {
        Dictionary<Vector2Int, List<Agent>> indexAgents = new Dictionary<Vector2Int, List<Agent>>();
        for (int i = 0; i < agents.Count; i++)
        {
            if (agents[i].Dead || indexAgents.ContainsKey(agents[i].Index)) continue;
            bool inFoodIndex = CheckIndexInFood(agents[i].Index);

            for (int j = 0; j < agents.Count; j++)
            {
                if (i == j || agents[j].Dead) continue;

                if (agents[i].Index == agents[j].Index || inFoodIndex)
                {
                    if (!indexAgents.ContainsKey(agents[i].Index))
                    {
                        indexAgents.Add(agents[i].Index, new List<Agent>() { agents[i] });
                    }

                    if (!inFoodIndex)
                    {
                        indexAgents[agents[i].Index].Add(agents[j]);
                    }
                }
            }
        }

        foreach (KeyValuePair<Vector2Int, List<Agent>> entry in indexAgents)
        {
            if (CheckIndexInFood(entry.Key))
            {
                List<Agent> eatingAgents = entry.Value;
                bool foodConsumed = false;

                if (eatingAgents.Count > 1)
                {
                    eatingAgents.RemoveAll(c => !c.ToStay);

                    if (eatingAgents.Count > 0)
                    {
                        if (!CheckSameTeamsInList(eatingAgents))
                        {
                            TEAM executeTeam = (TEAM)Random.Range((int)TEAM.A, (int)TEAM.B + 1) + 1;

                            for (int i = 0; i < eatingAgents.Count; i++)
                            {
                                if (eatingAgents[i].Team == executeTeam)
                                {
                                    eatingAgents[i].Death();
                                }
                            }

                            eatingAgents.RemoveAll(c => c.Team == executeTeam);
                        }

                        int agentEatingIndex = 0;
                        if (eatingAgents.Count > 1)
                        {
                            agentEatingIndex = Random.Range(0, eatingAgents.Count);

                            for (int i = 0; i < eatingAgents.Count; i++)
                            {
                                if (i != agentEatingIndex)
                                {
                                    eatingAgents[i].ToStay = false;
                                }
                            }
                        }

                        eatingAgents[agentEatingIndex].ConsumeFood();
                        foodConsumed = true;
                    }
                }
                else
                {
                    eatingAgents[0].ConsumeFood();
                    foodConsumed = true;
                }

                if (foodConsumed)
                {
                    Food food = foods.Find(f => f.Index == entry.Key);
                    if (food != null)
                    {
                        Destroy(food.gameObject);
                        foods.Remove(food);
                    }
                }
            }
            else
            {
                List<Agent> agentsInSameIndex = entry.Value;
                List<Agent> agentsCowards = agentsInSameIndex.FindAll(c => !c.ToStay);
                if (!CheckSameTeamsInList(agentsCowards) && agentsCowards.Count != agentsInSameIndex.Count)
                {
                    for (int i = 0; i < agentsCowards.Count; i++)
                    {
                        int prob = Random.Range(0, 101);
                        if (prob < DeathChance)
                        {
                            agentsCowards[i].Death();
                        }
                    }
                }

                agentsInSameIndex.RemoveAll(c => !c.ToStay);
                if (agentsInSameIndex.Count > 1 && !CheckSameTeamsInList(agentsInSameIndex))
                {
                    TEAM executeTeam = (TEAM)Random.Range((int)TEAM.A, (int)TEAM.B + 1) + 1;

                    for (int i = 0; i < agentsInSameIndex.Count; i++)
                    {
                        if (agentsInSameIndex[i].Team == executeTeam)
                        {
                            agentsInSameIndex[i].Death();
                        }
                    }
                }
            }
        }
    }

    private void ProcessAgentsNextGeneration()
    {
        for (int i = 0; i < agents.Count; i++)
        {
            agents[i].GenerationCount++;
        }
    }

    private void UpdateMoveChaimbots(float lerp)
    {
        foreach (Agent agent in agents)
        {
            if (!(agent.Index.y >= 0 && agent.Index.y <= size)) continue; // Check limit Y

            agent.Move(lerp);
        }
    }

    private void DestroyAgents()
    {
        for (int i = 0; i < agents.Count; i++)
        {
            Destroy(agents[i].gameObject);
        }

        agents.Clear();
    }

    private void DestroyFoods()
    {
        for (int i = 0; i < foods.Count; i++)
        {
            Destroy(foods[i].gameObject);
        }

        foods.Clear();
    }

    private bool CheckIndexInFood(Vector2Int checkIndex)
    {
        for (int i = 0; i < foods.Count; i++)
        {
            if (foods[i].Index == checkIndex)
            {
                return true;
            }
        }

        return false;
    }

    private bool CheckSameTeamsInList(List<Agent> auxAgents)
    {
        if (auxAgents.Count > 0)
        {
            TEAM team = auxAgents[0].Team;

            for (int i = 1; i < auxAgents.Count; i++)
            {
                if (auxAgents[i].Team != team)
                {
                    return false;
                }
            }
        }

        return true;
    }

    private Food GetNearFood(Vector3 position)
    {
        Food nearest = null;

        if (foods.Count > 0)
        {
            nearest = foods[0];
            float distance = (position - nearest.transform.position).sqrMagnitude;

            foreach (Food food in foods)
            {
                float newDist = (food.transform.position - position).sqrMagnitude;
                if (newDist < distance)
                {
                    nearest = food;
                    distance = newDist;
                }
            }
        }

        return nearest;
    }

    public int GetPopulationACount()
    {
        return populationsA.Count;
    }

    public int GetPopulationBCount()
    {
        return populationsB.Count;
    }

    public void AddFoodsConsumed(TEAM team)
    {
        totalFoodsConsumed++;

        if (team == TEAM.A) teams[0].foods++;
        else teams[1].foods++;
    }

    public void AddDeaths(TEAM team)
    {
        totalDeaths++;

        if (team == TEAM.A) teams[0].deaths++;
        else teams[1].deaths++;
    }

    private void ExtinctAgents(List<Agent> agents)
    {
        List<Agent> extinctAgents = agents.FindAll(c => c.FoodsConsumed == 0 || c.Dead || c.GenerationCount > AgentMaxGeneration);

        for (int i = 0; i < extinctAgents.Count; i++)
        {
            Destroy(extinctAgents[i].gameObject);
            agents.Remove(extinctAgents[i]);
        }
    }

    private void SurvivingAgents(List<Agent> agents)
    {
        List<Agent> survivingAgents = agents.FindAll(c => c.FoodsConsumed >= 1);
        List<Agent> survivingAgentsA = survivingAgents.FindAll(c => c.Team == TEAM.A);
        List<Agent> survivingAgentsB = survivingAgents.FindAll(c => c.Team == TEAM.B);

        List<Genome> survivingGenomesA = new List<Genome>();

        for (int i = 0; i < survivingAgentsA.Count; i++)
        {
            survivingGenomesA.Add(survivingAgentsA[i].Genome);
        }

        List<Genome> survivingGenomesB = new List<Genome>();

        for (int i = 0; i < survivingAgentsB.Count; i++)
        {
            survivingGenomesB.Add(survivingAgentsB[i].Genome);
        }

        populationsA.AddRange(survivingGenomesA);
        populationsB.AddRange(survivingGenomesB);
    }

    private void BreeadingAgents(List<Agent> agents, Action<Genome[], NeuralNetwork[], TEAM> onCreateNewAgents)
    {
        List<Agent> breedingAgents = agents.FindAll(c => c.FoodsConsumed >= 2);
        List<Agent> breedingAgentsA = breedingAgents.FindAll(c => c.Team == TEAM.A);
        List<Agent> breedingAgentsB = breedingAgents.FindAll(c => c.Team == TEAM.B);

        List<Genome> breedingGenomesA = new List<Genome>();

        for (int i = 0; i < breedingAgentsA.Count; i++)
        {
            breedingGenomesA.Add(breedingAgentsA[i].Genome);
        }

        List<Genome> breedingGenomesB = new List<Genome>();

        for (int i = 0; i < breedingAgentsB.Count; i++)
        {
            breedingGenomesB.Add(breedingAgentsB[i].Genome);
        }

        Genome[] newGenomesA = null;

        if (breedingGenomesA.Count >= 2)
        {
            if (breedingAgentsA.Count % 2 != 0)
            {
                breedingAgentsA.RemoveAt(breedingAgentsA.Count - 1);
            }

            // Evolve each genome and create a new array of genomes
            newGenomesA = genAlgA.Epoch(breedingGenomesA.ToArray());
        }

        Genome[] newGenomesB = null;

        if (breedingGenomesB.Count >= 2)
        {
            if (breedingGenomesB.Count % 2 != 0)
            {
                breedingGenomesB.RemoveAt(breedingGenomesB.Count - 1);
            }

            // Evolve each genome and create a new array of genomes
            newGenomesB = genAlgB.Epoch(breedingGenomesB.ToArray());
        }

        if (newGenomesA != null)
        {
            NeuralNetwork[] brainsA = new NeuralNetwork[newGenomesA.Length];
            for (int i = 0; i < brainsA.Length; i++)
            {
                brainsA[i] = CreateBrainA();
            }
            onCreateNewAgents?.Invoke(newGenomesA, brainsA, TEAM.A);
            populationsA.AddRange(newGenomesA);
        }

        if (newGenomesB != null)
        {
            NeuralNetwork[] brainsB = new NeuralNetwork[newGenomesB.Length];
            for (int i = 0; i < brainsB.Length; i++)
            {
                brainsB[i] = CreateBrainB();
            }
            onCreateNewAgents?.Invoke(newGenomesB, brainsB, TEAM.B);
            populationsB.AddRange(newGenomesB);
        }
    }

    private void CreateNewTeamAgents(List<Agent> agents, Action<Genome[], NeuralNetwork[], TEAM> onCreateNewAgents)
    {
        List<Agent> agentsA = agents.FindAll(c => c.Team == TEAM.A);
        List<Agent> agentsB = agents.FindAll(c => c.Team == TEAM.B);

        if (agentsA.Count == 0 && agentsB.Count > 0)
        {
            List<Genome> genomesB = new List<Genome>();

            for (int i = 0; i < agentsB.Count; i++)
            {
                genomesB.Add(agentsB[i].Genome);
                genomesB[i].fitness = 0f;
            }

            GeneticAlgorithm genAlgPlusA = new GeneticAlgorithm(teams[0].EliteCount, teams[0].MutationChance, teams[0].MutationRate * PlusMutationRate);

            // Evolve each genome and create a new array of genomes
            Genome[] newGenomesA = genAlgPlusA.Epoch(genomesB.ToArray());
            NeuralNetwork[] brainsA = new NeuralNetwork[newGenomesA.Length];
            for (int i = 0; i < brainsA.Length; i++)
            {
                brainsA[i] = CreateBrainA();
            }
            onCreateNewAgents?.Invoke(newGenomesA, brainsA, TEAM.A);
            populationsA.AddRange(newGenomesA);
            teams[0].extincts++;
        }

        if (agentsB.Count == 0 && agentsA.Count > 0)
        {
            List<Genome> genomesA = new List<Genome>();

            for (int i = 0; i < agentsA.Count; i++)
            {
                genomesA.Add(agentsA[i].Genome);
                genomesA[i].fitness = 0f;
            }

            GeneticAlgorithm genAlgPlusB = new GeneticAlgorithm(teams[1].EliteCount, teams[1].MutationChance, teams[1].MutationRate * PlusMutationRate);
            
            // Evolve each genome and create a new array of genomes
            Genome[] newGenomesB = genAlgPlusB.Epoch(genomesA.ToArray());
            NeuralNetwork[] brainsB = new NeuralNetwork[newGenomesB.Length];
            for (int i = 0; i < brainsB.Length; i++)
            {
                brainsB[i] = CreateBrainB();
            }
            onCreateNewAgents?.Invoke(newGenomesB, brainsB, TEAM.B);
            populationsB.AddRange(newGenomesB);
            teams[1].extincts++;
        }
    }

    private void ResetSimulation()
    {
        DestroyFoods();
        SpawnFoods();

        ProcessAgentsNextGeneration();

        Epoch(agents, SpawnNewAgents);

        SetAgentsPositions();
        ProcessAgents();
    }

    private Vector2Int GetRandomIndex(params Vector2Int[] usedIndexs)
    {
        Vector2Int index;
        bool repeat;

        do
        {
            repeat = false;

            int x = Random.Range(0, size);
            int y = Random.Range(1, size - 1);
            index = new Vector2Int(x, y);

            if (usedIndexs != null && usedIndexs.Length > 0)
            {
                repeat = usedIndexs.Contains(index);
            }

        } while (repeat);

        return index;
    }
    #endregion

    #region Data

    public void SaveData()
    {
        string path = null;

#if UNITY_EDITOR
        path = EditorUtility.SaveFilePanel("Save Brain Data", "", "brain_data.json", "json");
        if (string.IsNullOrEmpty(path)) return;
#endif  

        if (string.IsNullOrEmpty(path)) path = dataPath + fileName;

        BrainData data = new BrainData();
        data.genomesA = populationsA;
        data.genomesB = populationsB;
        data.GenerationCount = generation;
        data.PopulationCount = PopulationCount;
        data.Turns = Turns;

        data.A_EliteCount = teams[0].EliteCount;
        data.A_MutationChance = teams[0].MutationChance;
        data.A_MutationRate = teams[0].MutationRate;
        data.A_InputsCount = teams[0].InputsCount;
        data.A_HiddenLayers = teams[0].HiddenLayers;
        data.A_OutputsCount = teams[0].OutputsCount;
        data.A_NeuronsCountPerHL = teams[0].NeuronsCountPerHL;
        data.A_Bias = teams[0].Bias;
        data.A_P = teams[0].P;

        data.B_EliteCount = teams[1].EliteCount;
        data.B_MutationChance = teams[1].MutationChance;
        data.B_MutationRate = teams[1].MutationRate;
        data.B_InputsCount = teams[1].InputsCount;
        data.B_HiddenLayers = teams[1].HiddenLayers;
        data.B_OutputsCount = teams[1].OutputsCount;
        data.B_NeuronsCountPerHL = teams[1].NeuronsCountPerHL;
        data.B_Bias = teams[1].Bias;
        data.B_P = teams[1].P;

        string dataJson = JsonUtility.ToJson(data, true);
        File.WriteAllText(path, dataJson);
    }

    public void LoadData(Action<bool> onStartGame)
    {
        string path = null;

#if UNITY_EDITOR
        path = EditorUtility.OpenFilePanel("Select Brain Data", "", "json");
#endif

        BrainData data = null;
        string dataJson;

        if (string.IsNullOrEmpty(path)) dataJson = brainDataJson == null ? string.Empty : brainDataJson.text;
        else dataJson = File.ReadAllText(path);

        data = JsonUtility.FromJson<BrainData>(dataJson);

        if (data == null) return;

        populations.Clear();
        populationsA.Clear();
        populationsB.Clear();

        populationsA = data.genomesA;
        populationsB = data.genomesB;
        generation = data.GenerationCount;
        PopulationCount = data.PopulationCount;
        Turns = data.Turns;

        teams[0].EliteCount = data.A_EliteCount;
        teams[0].MutationChance = data.A_MutationChance;
        teams[0].MutationRate = data.A_MutationRate;
        teams[0].InputsCount = data.A_InputsCount;
        teams[0].HiddenLayers = data.A_HiddenLayers;
        teams[0].OutputsCount = data.A_OutputsCount;
        teams[0].NeuronsCountPerHL = data.A_NeuronsCountPerHL;
        teams[0].Bias = data.A_Bias;
        teams[0].P = data.A_P;

        teams[1].EliteCount = data.B_EliteCount;
        teams[1].MutationChance = data.B_MutationChance;
        teams[1].MutationRate = data.B_MutationRate;
        teams[1].InputsCount = data.B_InputsCount;
        teams[1].HiddenLayers = data.B_HiddenLayers;
        teams[1].OutputsCount = data.B_OutputsCount;
        teams[1].NeuronsCountPerHL = data.B_NeuronsCountPerHL;
        teams[1].Bias = data.B_Bias;
        teams[1].P = data.B_P;

        onStartGame?.Invoke(true);
    }
    #endregion
}