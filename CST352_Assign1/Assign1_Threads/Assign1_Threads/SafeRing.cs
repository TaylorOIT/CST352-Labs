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
        private int head;
        private int tail;
        private int size;
        private Mutex mutex;
        private ManualResetEvent hasCapacity;
        private ManualResetEvent hasItems;
        public SafeRing(int capacity)
        {
            this.capacity = capacity;
            buffer = new int[capacity];
            head = 0;
            tail = 0;
            size = 0;

            mutex = new Mutex();
            hasCapacity = new ManualResetEvent(true);
            hasItems = new ManualResetEvent(false);
        }

        public void Insert(int i, int timeout = -1)
        {
            // wait until it's safe and until there is capacity to insert
            if (!WaitHandle.WaitAll(new WaitHandle[] { mutex, hasCapacity }, timeout))
            {
                // time out expired
                throw new TimeoutException("SafeRing timed out in Insert!");
            }

            // iunsert the item i
            buffer[tail] = i;
            Console.WriteLine("Inserted " + i.ToString());

            // increment the tail
            tail = (tail + 1) % capacity;

            // increment the size
            size++;

            // notify any objects waiting to remove that there is an item in the queue
            hasItems.Set();

            // cause any objects wanting to insert to sleep if there is no longer capacity
            if (size == capacity)
                hasCapacity.Reset();

            // release the mutex
            mutex.ReleaseMutex();
        }

        public int Remove(int timeout = -1)
        {
            // wait until its safe and there is atleast one item to remove
           if (!WaitHandle.WaitAll(new WaitHandle[] {mutex, hasItems}))
            {
                // timeout expired
                throw new TimeoutException("SafeRing timed out in Remove!");
            }

            // get the item at the heaed of the queue
            int i = buffer[head];
            Console.WriteLine("Removed " + i.ToString());

            // increment the head
            head = (head + 1) % capacity;

            // decrement the size
            size--;

            // indicate that we have capacity
            hasCapacity.Set();

            // indicate that the queue is empty if we removed the last item
            if (size == 0)
                hasItems.Reset();
            
            // unlock the mutex
            mutex.ReleaseMutex();

            // return the item we removed from the queue
            return i;
        }

        public int Count()
        {
            mutex.WaitOne();
            int count = size;
            mutex.ReleaseMutex();

            return count;
        }
    }
}
