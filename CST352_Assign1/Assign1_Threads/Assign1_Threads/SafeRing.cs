using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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

        public void Insert(int i)
        {
            // wait until it's safe and until there is capacity to insert
            wait(mutex,hasCapacity);

            // iunsert the item i
            buffer[tail] = i;

            // increment the tail
            tail = (tail + 1) % capacity;

            // increment the size
            size++;

            // notify any objects waiting to remove that there is an item in the queue
            hasItems.set();

            // cause any objects wanting to insert to sleep if there is no longer capacity
            if (size == capacity)
                hasCapacity.reset();

            // release the mutex
            mutex.unlock();

        }

        public int Remove()
        {
            return 0;
        }

        public int Count()
        {
            return 0;
        }
    }
}
