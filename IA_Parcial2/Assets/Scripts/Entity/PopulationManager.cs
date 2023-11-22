using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;
using GridMap;

// (15) La populaciones son conjuntos de agentes que tienen que poder cumplir una tarea en un entorno

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

    [Header("Grid")]
    [Range(100, 200)] public int Size = 100;
    public float Unit = 1f; // Cellsize

    [Header("Settings")]
    [Tooltip("Less than or equal to the width of the grid")] public int PopulationCount = 0; // Cantidad de agentes
    public float TurnsDelay = 0f;
    public int Turns = 0;
    public int IterationCount = 0;
    public int AgentMaxGeneration = 0;
    public float DeathChance = 0f;
    public float PlusMutationRate = 0f;

    [Header("Teams")]
    public Team[] teams = null;

    [Header("Prefabs")]
    public GameObject agent1Prefab = null;
    public GameObject agent2Prefab = null;
    public GameObject foodPrefab = null;
    public GameObject floor = null;

    [Header("UI")]
    public StartConfigurationScreen startConfigurationScreen = null;
    public SimulationScreen simulationScreen = null;

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

    private Grid<int> grid;

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

        if (PopulationCount > Size) PopulationCount = Size;

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

        CreateGrid();
    }

    private void CreateGrid()
    {
        int width = Size;
        int height = Size;

        grid = new Grid<int>(width, height, Unit, new Vector3(-width * Unit / 2, 0, -height * Unit / 2), (Grid<int> grid, int x, int y) => new int());

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                Vector3 position = grid.GetWorldPosition(x, z) + (Vector3.one * Unit / 2);
                GameObject GO = Instantiate(floor, position, Quaternion.identity);
                GO.transform.localScale = new Vector3(Unit, 0.01f, Unit);
                GO.transform.SetParent(transform);
            }
        }
    }

    private void Start()
    {
        startConfigurationScreen.Init(StartGame);
        simulationScreen.Init(PauseGame, StopSimulation);
    }

    public void StartSimulation(List<Agent> agents, bool dataLoaded)
    {
        // Creo y configuro el algoritmo genético
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

        teams[1].foods = 0;
        teams[1].deaths = 0;

        if (!dataLoaded)
        {
            teams[0].extincts = 0;
            teams[1].extincts = 0;
        }
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

        SetNearFoodInAgents();
        SetAgentsPositions();

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

    // Genero la población inicial con datos de carga
    private void GenerateInitialPopulationWithLoadData(List<Agent> agents)
    {
        // Lista de agentes del equipo correspondiente
        List<Agent> agentsA = agents.FindAll(c => c.Team == TEAM.A);
        List<Agent> agentsB = agents.FindAll(c => c.Team == TEAM.B);

        // Establezco los nuevos genomas según el peso de cada NeuralNetwork
        for (int i = 0; i < agentsA.Count; i++)
        {
            NeuralNetwork brain = CreateBrainA();
            Genome genome = populationsA[i];
            brain.SetWeights(genome.genome);
            agentsA[i].SetBrain(genome, brain);
        }

        for (int i = 0; i < agentsB.Count; i++)
        {
            NeuralNetwork brain = CreateBrainB();
            Genome genome = populationsB[i];
            brain.SetWeights(genome.genome);
            agentsB[i].SetBrain(genome, brain);
        }
    }

    // Genero la población inicial sin datos de carga
    private void GenerateInitialPopulation(List<Agent> agents)
    {
        populationsA.Clear();
        populationsB.Clear();

        for (int i = 0; i < agents.Count; i++)
        {
            NeuralNetwork brain = null;
            Genome genome = null;

            // Establezco los nuevos genomas según el peso de cada NeuralNetwork
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

    // Creo una nueva red neuronal
    NeuralNetwork CreateBrainA()
    {
        NeuralNetwork brain = new NeuralNetwork();

        // Agrego la primera neuron layer que tenga tantas neuronas como entradas
        brain.AddFirstNeuronLayer(teams[0].InputsCount, teams[0].Bias, teams[0].P);

        for (int i = 0; i < teams[0].HiddenLayers; i++)
        {
            // Agrego cada hidden layer con un recuento de neuronas personalizado
            brain.AddNeuronLayer(teams[0].NeuronsCountPerHL, teams[0].Bias, teams[0].P);
        }

        // Agrego la output layer con tantas neuronas como salidas
        brain.AddNeuronLayer(teams[0].OutputsCount, teams[0].Bias, teams[0].P);

        return brain;
    }

    // Creo una nueva red neuronal
    NeuralNetwork CreateBrainB()
    {
        NeuralNetwork brain = new NeuralNetwork();

        // Agrego la primera neuron layer que tenga tantas neuronas como entradas
        brain.AddFirstNeuronLayer(teams[1].InputsCount, teams[1].Bias, teams[1].P);

        for (int i = 0; i < teams[1].HiddenLayers; i++)
        {
            // Agrego cada hidden layer con un recuento de neuronas personalizado
            brain.AddNeuronLayer(teams[1].NeuronsCountPerHL, teams[1].Bias, teams[1].P);
        }

        // Agrego la output layer con tantas neuronas como salidas
        brain.AddNeuronLayer(teams[1].OutputsCount, teams[1].Bias, teams[1].P);

        return brain;
    }

    public void Epoch(List<Agent> agents, Action<Genome[], NeuralNetwork[], TEAM> onCreateNewAgents)
    {
        generation++;  // Incremento el contador de generacion
        turnsLeft = Turns;

        // Calculo las puntuaciones del fitness (best, average and worst) de la poblacion actual
        bestFitness = getBestFitness();
        avgFitness = getAvgFitness();
        worstFitness = getWorstFitness();

        // Limpio la poblacion actual
        populations.Clear();
        populationsA.Clear();
        populationsB.Clear();

        ExtinctAgents(agents); // Agentes extintos
        SurvivingAgents(agents); // Agentes sobrevivientes
        BreeadingAgents(agents, onCreateNewAgents); // Creo nuevos agentes a partir de los agentes de reproduccion seleccionados
        CreateNewTeamAgents(agents, onCreateNewAgents); // Creo nuevos agentes para el otro equipo

        // Agrego una nueva poblacion
        populations.AddRange(populationsA);
        populations.AddRange(populationsB);

        totalAgents = populations.Count;
        teams[0].agents = populationsA.Count;
        teams[1].agents = populationsB.Count;

        teams[0].foods = 0;
        teams[0].deaths = 0;

        teams[1].foods = 0;
        teams[1].deaths = 0;

        /*
        Este método es el responsable de la selección natural en el sistema de selección de reproducción, 
        donde los agentes con mejor fitness tienen una mayor probabilidad de sobrevivir y reproducirse. 
        Además, crea nuevos agentes a partir de los agentes seleccionados.
        */
    }

    void FixedUpdate()
    {
        if (!isRunning) return;
        if (agents.Count == 0) isRunning = false; // Si no hay agentes, se termina el juego

        // El bucle se ejecuta varias veces en función del valor de "IterationCount"
        // "Mathf.Clamp(IterationCount / 100.0f * 50f, 1f, 50f)" se conoce como un: cálculo por pasos (step calculation)
        // Es para garantizar que el juego no se ejecute demasiado rápido, evitanto un rendimiento inadecuado o imprecisión en las actualizaciones
        for (int i = 0; i < Mathf.Clamp(IterationCount / 100.0f * 50f, 1f, 50f); i++)
        {
            turnsTimer += Time.fixedDeltaTime;
            UpdateMoveChaimbots(turnsTimer / TurnsDelay); // Actualizo la posición y dirección de los agentes

            if (turnsTimer > TurnsDelay)
            {
                turnsTimer = 0f;

                SetNearFoodInAgents(); // Establezco la comida cercana a cada agente
                ProcessAgentsInSameIndex(); // Proceso a los agentes que están en la misma posición

                if (agents.Count == 0)
                {
                    isRunning = false; // Si no hay agentes, se termina el juego
                }
                else
                {
                    turnsLeft--; // Disminuye el número de turnos restantes
                    if (turnsLeft <= 0) ResetSimulation(); // Si no quedan turnos, reinicia la simulación
                }
            }
        }
    }

    #region Helpers
    private void SpawnAgents(bool dataLoaded)
    {
        // Obtengo la cantidad total de agentes
        int totalAgentsA = PopulationCount;
        int totalAgentsB = PopulationCount;

        // Si "dataLoaded" es verdadero, actualizo los valores
        if (dataLoaded)
        {
            totalAgentsA = GetPopulationACount();
            totalAgentsB = GetPopulationBCount();
        }

        // Creo e inicializo los agentes
        // Creo una nueva instancia del agente correspondiente y lo aniado a la lista
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
        // Se itera por cada genoma recibido
        for (int i = 0; i < newGenomes.Length; i++)
        {
            // Se crea un nuevo agente utilizando la informacion proporcionada por el genoma
            // y la red neuronal correspondiente
            GameObject agentGO = null;
            if (team == TEAM.A) agentGO = Instantiate(agent1Prefab);
            else agentGO = Instantiate(agent2Prefab);

            agentGO.transform.SetParent(transform);
            Agent agent = agentGO.GetComponent<Agent>();
            agent.Init(Unit, size, team);

            // Se configura la red neuronal del agente
            // Se obtiene la red neuronal correspodiente al indice actual del bucle
            NeuralNetwork brain = brains[i];
            brain.SetWeights(newGenomes[i].genome);
            agent.SetBrain(newGenomes[i], brain);

            // Se agrega el agente al conjunto de agentes existentes
            agents.Add(agent);
        }
    }

    private void SpawnFoods()
    {
        // Lista de comida
        List<Vector2Int> foodUsedIndexs = new List<Vector2Int>();

        for (int i = 0; i < foodSize; i++)
        {
            GameObject foodGO = Instantiate(foodPrefab);
            foodGO.transform.SetParent(transform);
            Food food = foodGO.GetComponent<Food>();

            Vector2Int foodIndex = GetRandomIndex(foodUsedIndexs.ToArray());

            // Se posiciona la comida en funcion del tamanio del terreno y la unidad
            Vector3 startPosition = new Vector3(-size / 2f, 0.5f, -size / 2f);
            food.transform.position = (startPosition + new Vector3(foodIndex.x, 0f, foodIndex.y)) * Unit;
            food.Init(foodIndex);

            foods.Add(food);
            foodUsedIndexs.Add(foodIndex);
        }
    }

    private void SetAgentsPositions()
    {
        Vector3 startPosition = new Vector3(-size / 2f, 0f, -size / 2f); // Centro del terreno
        int aIndexX = 0;
        int aIndexY = 0;
        int bIndexX = size;
        int bIndexY = size;

        for (int i = 0; i < agents.Count; i++)
        {
            Vector2Int index;

            // Los posiciona dependiendo el equipo
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

            // Se actualiza la posicion del agente
            agents[i].Index = index;
            agents[i].transform.position = (startPosition + new Vector3(index.x, 0f, index.y)) * Unit;
            agents[i].ResetData();
        }
    }

    private void SetNearFoodInAgents()
    {
        for (int i = 0; i < agents.Count; i++)
        {
            // Chequeo si el agente esta dentro del limite Y, y si no esta muerto
            if (!(agents[i].Index.y >= 0 && agents[i].Index.y <= size) || agents[i].Dead) continue;

            agents[i].SetNearFood(GetNearFood(agents[i].transform.position)); // Le mando la comida mas cercana
            agents[i].Think();
        }
    }

    private void ProcessAgentsInSameIndex()
    {
        // Guardo los agentes por indice
        Dictionary<Vector2Int, List<Agent>> indexAgents = new Dictionary<Vector2Int, List<Agent>>();
        for (int i = 0; i < agents.Count; i++)
        {
            // Si esta muerto, fuera del limite o si ya esta en la lista, continuo
            if (agents[i].Dead || agents[i].InOutLimit || indexAgents.ContainsKey(agents[i].Index)) continue;

            bool inFoodIndex = CheckIndexInFood(agents[i].Index); // Chequeo si hay comida en ese indice

            for (int j = 0; j < agents.Count; j++) // Busco agentes en el mismo indice
            {
                if (i == j || agents[j].Dead || agents[i].InOutLimit) continue;

                // Si esta en el mismo indice o hay comida
                if (agents[i].Index == agents[j].Index || inFoodIndex)
                {
                    // Actualizo el diccionario con el agente
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

        // Recorro el diccionario y chequeo si en ese indice hay comida
        foreach (KeyValuePair<Vector2Int, List<Agent>> entry in indexAgents)
        {
            if (CheckIndexInFood(entry.Key))
            {
                List<Agent> eatingAgents = entry.Value; // Obtengo la lista de agentes en el indice actual
                bool foodConsumed = false; // Si se consumio comida o no

                if (eatingAgents.Count > 1) // Si hay mas de un agente en la posicion actual
                {
                    eatingAgents.RemoveAll(c => !c.ToStay); // Elimino los agentes que no se quedan

                    if (eatingAgents.Count > 0) // Si quedan agentes
                    {
                        if (!CheckSameTeamsInList(eatingAgents)) // Si los agentes en la lista no pertenecen al mismo equipo
                        {
                            // Elijo un equipo aleatorio
                            TEAM executeTeam = (TEAM)Random.Range((int)TEAM.A, (int)TEAM.B + 1) + 1;

                            for (int i = 0; i < eatingAgents.Count; i++)
                            {
                                // Si el equipo del agente es igual al equipo aleatorio, muere el agente
                                if (eatingAgents[i].Team == executeTeam)eatingAgents[i].Death();
                            }

                            eatingAgents.RemoveAll(c => c.Team == executeTeam);
                        }

                        int agentEatingIndex = 0; // Indice del agente que consume comida

                        if (eatingAgents.Count > 1)
                        {
                            // Elijo un indice de agente aleatorio para consumir comida
                            agentEatingIndex = Random.Range(0, eatingAgents.Count);

                            for (int i = 0; i < eatingAgents.Count; i++)
                            {
                                // Si el indice actual es distinto al indice del agente aleatorio, no se queda quieto
                                if (i != agentEatingIndex) eatingAgents[i].ToStay = false;
                            }
                        }

                        // El agente consume comida
                        eatingAgents[agentEatingIndex].ConsumeFood();
                        foodConsumed = true;
                    }
                }
                else
                {
                    // Si solo hay una agente en el indice actual, consume comida
                    eatingAgents[0].ConsumeFood();
                    foodConsumed = true;
                }

                if (foodConsumed)
                {
                    // Elimino la comida consumida
                    Food food = foods.Find(f => f.Index == entry.Key);
                    if (food != null)
                    {
                        Destroy(food.gameObject);
                        foods.Remove(food);
                    }
                }
            }
            else // Si la posicion actual no esta asociada con comida
            {
                List<Agent> agentsInSameIndex = entry.Value; // Obtengo una lista de agentes en el indice actual
                List<Agent> agentsCowards = agentsInSameIndex.FindAll(c => !c.ToStay); // Obtengo lista de agentes no se deben quedarse

                // Si hay agentes que no deben quedarse y pertenecen al distinto grupo, y si hay agentes que no deben quedarse
                if (!CheckSameTeamsInList(agentsCowards) && agentsCowards.Count != agentsInSameIndex.Count)
                {
                    for (int i = 0; i < agentsCowards.Count; i++)
                    {
                        // Si la probabilidad es menor que "DeathChance", el agente muere
                        int prob = Random.Range(0, 101);
                        if (prob < DeathChance) agentsCowards[i].Death();
                    }
                }

                agentsInSameIndex.RemoveAll(c => !c.ToStay);

                // Si hay mas de un agente en el indice actual y no todos son del mismo equipo
                if (agentsInSameIndex.Count > 1 && !CheckSameTeamsInList(agentsInSameIndex))
                {
                    // Elijo un equipo aleatorio
                    TEAM executeTeam = (TEAM)Random.Range((int)TEAM.A, (int)TEAM.B + 1) + 1;

                    for (int i = 0; i < agentsInSameIndex.Count; i++)
                    {
                        // Si el equipo del agente es igual al equipo aleatorio, muere el agente
                        if (agentsInSameIndex[i].Team == executeTeam) agentsInSameIndex[i].Death();
                    }
                }
            }
        }
    }

    private void UpdateAgentsGeneration()
    {
        // Aumento la generacion de los agentes
        for (int i = 0; i < agents.Count; i++)
        {
            agents[i].GenerationCount++;
        }
    }

    private void UpdateMoveChaimbots(float lerp)
    {
        foreach (Agent agent in agents)
        {
            if (!(agent.Index.y >= 0 && agent.Index.y <= size)) continue; // Chequeo el limite Y

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
        // Destruyo toda la comida actual
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
            if (foods[i].Index == checkIndex) return true;
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
                if (auxAgents[i].Team != team) return false;
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
        // Lista con agentes que su comida sea = 0, este muerto o su generacion es mayor al limite maximo de generaciones
        List<Agent> extinctAgents = agents.FindAll(c => c.FoodsConsumed == 0 || c.Dead || c.GenerationCount > AgentMaxGeneration);

        for (int i = 0; i < extinctAgents.Count; i++)
        {
            Destroy(extinctAgents[i].gameObject);
            agents.Remove(extinctAgents[i]);
        }
    }

    private void SurvivingAgents(List<Agent> agents)
    {
        // Lista con agentes si consumio al menos 1 comida
        List<Agent> survivingAgents = agents.FindAll(c => c.FoodsConsumed >= 1);
        // Lista de agentes sobrevivientes del equipo correspondiente
        List<Agent> survivingAgentsA = survivingAgents.FindAll(c => c.Team == TEAM.A);
        List<Agent> survivingAgentsB = survivingAgents.FindAll(c => c.Team == TEAM.B);
        // Lista con genomas de los agentes sobrevivientes del equipo correspondiente
        List<Genome> survivingGenomesA = new List<Genome>();
        List<Genome> survivingGenomesB = new List<Genome>();

        for (int i = 0; i < survivingAgentsA.Count; i++)
        {
            survivingGenomesA.Add(survivingAgentsA[i].Genome);
        }

        for (int i = 0; i < survivingAgentsB.Count; i++)
        {
            survivingGenomesB.Add(survivingAgentsB[i].Genome);
        }

        // Se agregan los genomas de los agentes sobrevivientes a la poblacion del equipo correspondiente
        populationsA.AddRange(survivingGenomesA);
        populationsB.AddRange(survivingGenomesB);
    }

    private void BreeadingAgents(List<Agent> agents, Action<Genome[], NeuralNetwork[], TEAM> onCreateNewAgents)
    {
        // Lista con agentes si consumio al menos 2 de comida
        List<Agent> breedingAgents = agents.FindAll(c => c.FoodsConsumed >= 2);
        // Lista de agentes de reproduccion del equipo correspondiente
        List<Agent> breedingAgentsA = breedingAgents.FindAll(c => c.Team == TEAM.A);
        List<Agent> breedingAgentsB = breedingAgents.FindAll(c => c.Team == TEAM.B);
        // Lista con genomas de los agentes de reproduccion del equipo correspondiente
        List<Genome> breedingGenomesA = new List<Genome>();
        List<Genome> breedingGenomesB = new List<Genome>();

        for (int i = 0; i < breedingAgentsA.Count; i++)
        {
            breedingGenomesA.Add(breedingAgentsA[i].Genome);
        }

        for (int i = 0; i < breedingAgentsB.Count; i++)
        {
            breedingGenomesB.Add(breedingAgentsB[i].Genome);
        }

        // Evoluciona cada genoma resultando nuevos genomas de cada equipo correspondiente
        Genome[] newGenomesA = null;
        Genome[] newGenomesB = null;

        // La evolucion se realiza mediante el metodo "Epoch()"
        if (breedingGenomesA.Count >= 2)
        {
            if (breedingAgentsA.Count % 2 != 0)
            {
                breedingAgentsA.RemoveAt(breedingAgentsA.Count - 1);
            }
            newGenomesA = genAlgA.Epoch(breedingGenomesA.ToArray());
        }

        if (breedingGenomesB.Count >= 2)
        {
            if (breedingGenomesB.Count % 2 != 0)
            {
                breedingGenomesB.RemoveAt(breedingGenomesB.Count - 1);
            }
            newGenomesB = genAlgB.Epoch(breedingGenomesB.ToArray());
        }

        // Si se crearon nuevos genomas se crean nuevas brains y se llama a "onCreateNewAgents()"
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
        // Crea nuevos agentes en caso de que alguno de los dos equipos desaparezca

        // Lista de agentes del equipo correspondiente
        List<Agent> agentsA = agents.FindAll(c => c.Team == TEAM.A);
        List<Agent> agentsB = agents.FindAll(c => c.Team == TEAM.B);

        // Si no quedan mas agentes del equipo A
        if (agentsA.Count == 0 && agentsB.Count > 0)
        {
            List<Genome> genomesB = new List<Genome>();

            for (int i = 0; i < agentsB.Count; i++)
            {
                // Reinicio el fitness de cada genoma
                genomesB.Add(agentsB[i].Genome);
                genomesB[i].fitness = 0f;
            }

            // Creo un nuevo algoritmo genetico
            GeneticAlgorithm genAlgPlusA = new GeneticAlgorithm(teams[0].EliteCount, teams[0].MutationChance, teams[0].MutationRate * PlusMutationRate);

            // Evoluciona cada genoma y crea una nueva serie de genomas:
            // Realizo un proceso de evolucion en la poblacion de genomas, incluyendo la seleccion de
            // invividuos de aptitud alta, cruzando entre estos individuos para crear nuevos con
            // caracteristicas mixtas, logrando una mutacion aleatoria para introducir variacion en la poblacion
            Genome[] newGenomesA = genAlgPlusA.Epoch(genomesB.ToArray());
            NeuralNetwork[] brainsA = new NeuralNetwork[newGenomesA.Length];
            for (int i = 0; i < brainsA.Length; i++)
            {
                brainsA[i] = CreateBrainA();
            }
            onCreateNewAgents?.Invoke(newGenomesA, brainsA, TEAM.A);
            populationsA.AddRange(newGenomesA); // Se agregan a la poblacion correspondiente
            teams[0].extincts++;
        }

        // Si no quedan mas agentes del equipo B
        if (agentsB.Count == 0 && agentsA.Count > 0)
        {
            List<Genome> genomesA = new List<Genome>();

            for (int i = 0; i < agentsA.Count; i++)
            {
                genomesA.Add(agentsA[i].Genome);
                genomesA[i].fitness = 0f;
            }

            GeneticAlgorithm genAlgPlusB = new GeneticAlgorithm(teams[1].EliteCount, teams[1].MutationChance, teams[1].MutationRate * PlusMutationRate);

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

        UpdateAgentsGeneration();
        Epoch(agents, SpawnNewAgents);

        SetNearFoodInAgents();
        SetAgentsPositions();
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