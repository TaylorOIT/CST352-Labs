using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assign1_Threads
{
    class SafeRing
    {
        private int capacity;
        private int[] buffer;
        public SafeRing(int capacity)
        {
            this.capacity = capacity;
            buffer = new int[capacity];
        }
    }
}
