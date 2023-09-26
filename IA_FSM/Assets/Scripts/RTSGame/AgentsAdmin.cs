using System.Collections.Concurrent;
using System.Threading.Tasks;
using Toolbox;
using RTSGame.Entities.Agents;

public class AgentsAdmin : MonoBehaviourSingleton<AgentsAdmin>
{
    private ConcurrentBag<Agent> agents = new ConcurrentBag<Agent>();
    private ParallelOptions options = null;

    private void Start()
    {
        options = new ParallelOptions { MaxDegreeOfParallelism = 5 };        
    }

    private void Update()
    {
        Parallel.ForEach(agents, options, currentItem =>
        {
            //currentItem.UpdateAgent();
        });
    }

    public void AddAgent(Agent agent)
    {
        agents.Add(agent);
    }
}