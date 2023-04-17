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
            thread.Start(???);
        }

        private static void ThreadFunc(Object param)
        {
            Produce();
        }

        private void Produce()
        {
            // do the producing
        }
    }
}
