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

            SafeRing ring = new SafeRing(RING_CAPACITY);

            ring.Insert(0);
            ring.Insert(1);
            ring.Insert(2);

            Console.WriteLine("Size = " + ring.Count().ToString());
            Console.WriteLine(".Remove() = " + ring.Remove().ToString());
            Console.WriteLine(".Remove() = " + ring.Remove().ToString());
            Console.WriteLine(".Remove() = " + ring.Remove().ToString());
        }
    }
}
