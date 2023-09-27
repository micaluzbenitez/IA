using System.Collections.Concurrent;
using System.Threading.Tasks;
using Toolbox;
using RTSGame.Entities.Agents;

namespace RTSGame
{
    // Cuando tengo procesos muy pesados puedo partir los subprocesos en procesos paralelos
    public class AgentsAdmin : MonoBehaviourSingleton<AgentsAdmin>
    {
        private ConcurrentBag<Agent> agents = new ConcurrentBag<Agent>(); // ConcurrentBag tiene todo lo que tiene una lista, pero para multithreading
        private ParallelOptions options = null;

        private const int maxDegreeOfParallelism = 1;

        private void Start()
        {
            options = new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism };
        }

        private void Update()
        {            
            Parallel.ForEach(agents, options, currentItem =>
            {
                currentItem.UpdateAgent();
            });
        }

        public void AddAgent(Agent agent)
        {
            agents.Add(agent);
        }
    }
}