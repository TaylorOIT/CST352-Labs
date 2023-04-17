using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.CodeDom;
using System.Data;

namespace Assign1_Threads
{
    class Producer
    {
        private SafeRing ring;
        private int nItemsToProduce;
        private Thread thread;
        private Random rand;
        private ManualResetEvent complete;

        public WaitHandle Complete { get { return complete; } }

        public Producer(Random rand, SafeRing ring, int nItemsToProduce)
        {
            this.ring = ring;
            this.nItemsToProduce = nItemsToProduce;
            this.rand = rand;
            complete = new ManualResetEvent(false);
        }

        public void Start()
        {
            // start a new thread
            thread = new Thread(ThreadFunc);
            thread.Start(this);
        }

        private static void ThreadFunc(Object param)
        {
            Producer p = param as Producer;
            p.Produce();
        }

        private void Produce()
        {
           // do the producing 

            for (int i =0; i < nItemsToProduce; i++)
            {
                // produce 1 item
                
                // Randomly generate the integer between 1 and 1000
                int item = rand.Next(1, 1000);
               
                // Inserting the number into the queue
                ring.Insert(item);

                // Sleeping the thread for that number of msec
                Thread.Sleep(item);
            }

            // notify complete
            complete.Set();
        }
    }
}
