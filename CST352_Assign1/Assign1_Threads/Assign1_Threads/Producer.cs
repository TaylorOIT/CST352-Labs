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

        public Producer(SafeRing ring, int nItemsToProduce)
        {
            this.ring = ring;
            this.nItemsToProduce = nItemsToProduce;
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
               // Inserting the number into the queue
               // Sleeping the thread for that number of msec
            }

            // notify complete
        }
    }
}
