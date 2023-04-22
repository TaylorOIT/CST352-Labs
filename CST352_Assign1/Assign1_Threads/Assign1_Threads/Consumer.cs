using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Assign1_Threads
{
    class Consumer
    {
        private SafeRing ring;
        private Random rand;
        private Thread thread;
        private bool stop;

        public Consumer(Random r, SafeRing ring)
        {
            rand = r;
            this.ring = ring;
        }

        public void Start()
        {
            stop = false;
            thread = new Thread(ThreadFunc);
            thread.Start(this);
        }

        private static void ThreadFunc(Object param)
        {
            Consumer c = param as Consumer;
            c.Consume();
        }

        private void Consume()
        {
            // consume until we're told to stop
            while (!stop)
            {
                try
                {
                    // consume one item
                    
                    // Remove an integer from the queue
                    int item = ring.Remove();
                    
                    // Randomly generate a second integer between 1 and 1000
                    int item2 = rand.Next(1, 1000);

                    // Sleep the thread for the number of msec
                    Thread.Sleep(item2);
                }
                catch (Exception e)
                {
                    // we've been told to stop
                    Console.WriteLine("Stopping...");
                }
            }
        }

        public void Stop()
        {
            stop = true;
            thread.Abort();
        }
    }
}
