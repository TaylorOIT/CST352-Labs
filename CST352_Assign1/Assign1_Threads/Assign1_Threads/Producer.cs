using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.CodeDom;
using System.Data;
using System.Net.Http.Headers;

namespace Assign1_Threads
{
    class Producer
    {
        private const int RETRIES = 10;
        private SafeRing ring;
        private int nItemsToProduce;
        private Thread thread;
        private Random rand;
        private ManualResetEvent complete;
        private int timeout;

        public WaitHandle Complete { get { return complete; } }

        public Producer(Random rand, SafeRing ring, int nItemsToProduce, int timeout = -1)
        {
            this.ring = ring;
            this.nItemsToProduce = nItemsToProduce;
            this.rand = rand;
            complete = new ManualResetEvent(false);
            this.timeout = timeout;
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
                
                int retries  = RETRIES;
                while (retries > 0)
                {
                    try
                    {
                        // Inserting the number into the queue
                        ring.Insert(item, timeout);
                        break;
                    } catch (Exception ex)
                    {
                        Console.WriteLine("Warning! Unable to insert produced item, retrying!");
                        retries--;
                        if (retries == 0)
                        {
                            Console.WriteLine("Error! Unable to insert produced item, giving up!");
                            Thread.Sleep(1000);
                        }
                    }
                }

                // Sleeping the thread for that number of msec
                Thread.Sleep(item);
            }

            // notify complete
            complete.Set();
        }
    }
}
