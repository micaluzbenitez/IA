using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace FiniteStateMachine.Multithreading
{
    public class SoldiersAdmin : MonoBehaviour
    {
        public System.Collections.Concurrent.ConcurrentBag<Soldier> soldiers = new System.Collections.Concurrent.ConcurrentBag<Soldier>();

        public List<Soldier> soldiersForBag;

        private void Start()
        {
            foreach(Soldier soldier in soldiersForBag)
            {
                soldiers.Add(soldier);
            }

            ParallelOptions options = new ParallelOptions { MaxDegreeOfParallelism = 5 };

            Parallel.ForEach(soldiers, options, currentItem =>
            {
                //currentItem.Update();
            });
        }
    }
}