using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assign1_Threads
{
    internal class Program
    {
        static void Main(string[] args)
        {
            const int RING_CAPACITY = 5;
            const int NUM_PRODUCERS = 2;
            const int NUM_ITEMS_TO_PRODUCE = 10;
            const int NUM_CONSUMERS = 2;
            const int TIMEOUT = 10; // msec

            Random rand = new Random();

            SafeRing ring = new SafeRing(RING_CAPACITY);

            // create and start producers
            List<Producer> producers = new List<Producer>();
            for (int i = 0; i < NUM_PRODUCERS; i++)
            {
                Producer p = new Producer(rand, ring, NUM_ITEMS_TO_PRODUCE, TIMEOUT);
                producers.Add(p);
                p.Start();
            }

            // create and start the consumers
            List<Consumer> consumers = new List<Consumer>();
            for (int i = 0; i < NUM_CONSUMERS; i++)
            {
                Consumer c = new Consumer(rand, ring, TIMEOUT);
                consumers.Add(c);
                c.Start();
            }

            // wait until all producers are complete
            for (int i = 0; i < NUM_PRODUCERS; i++)
            {
                producers[i].Complete.WaitOne();
            }

            // stop the consumers
            for (int i = 0; i < NUM_CONSUMERS; i++)
            {
                consumers[i].Stop();
            }
        }
    }
}
