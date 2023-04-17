﻿using System;
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

        public void Insert(int i)
        {
            // wait until it's safe and until there is capacity to insert
            WaitHandle.WaitAll(new WaitHandle[] { mutex, hasCapacity });

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

        public int Remove()
        {
            // wait until its safe and there is atleast one item to remove
            WaitHandle.WaitAll(new WaitHandle[] {mutex, hasItems});

            int i = buffer[head];
            head = (head + 1) %
            capacity;
            size--;
            hasCapacity.set();
            if (size == 0)
                hasItems.reset();
            mutex.unlock();
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
