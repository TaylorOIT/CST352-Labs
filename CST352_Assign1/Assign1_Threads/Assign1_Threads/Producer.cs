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

        public Producer(SafeRing ring, int nItemsToProduce)
        {
            this.ring = ring;
            this.nItemsToProduce = nItemsToProduce;
        }
    }
}
