// Assignment 4
// Pete Myers
// OIT, Spring 2018
// Handout

using System;
using System.Collections.Generic;
using System.Threading;
using System.Diagnostics;

namespace SimpleShell
{
    public class Terminal
    {
        // NOTE: performs line discipline over driver
        private TerminalDriver driver;
        private LineQueue completedLineQueue;
        private Handler handler;

        public Terminal(TerminalDriver driver)
        {
            if (driver == null)
                throw new Exception("Null terminal driver disallowed!");

            completedLineQueue = new LineQueue();
            handler = new Handler(driver, completedLineQueue);

            this.driver = driver;
            this.driver.InstallInterruptHandler(handler);
        }

        public void Connect()
        {
            driver.Connect();
        }

        public void Disconnect()
        {
            driver.Disconnect();
            
            // reset the completed line queue.
            completedLineQueue = new LineQueue();
        }

        public bool Echo { get { return handler.Echo; } set { handler.Echo = value; } }

        public string ReadLine()
        {
            // NOTE: blocks until a line of text is available
            return completedLineQueue.Remove();
        }

        public void Write(string line)
        {
            // send each character to the terminal through the driver
            foreach(char c in line)
            {
                driver.SendChar(c);
            }
        }

        public void WriteLine(string line)
        {
            // add a new line at the end of the string
            Write(line);
            driver.SendNewLine();
        }

        private class LineQueue
        {
            private Queue<string> theQueue;
            private Mutex mutex;
            private ManualResetEvent hasItemsEvent;

            public LineQueue()
            {
                this.theQueue = new Queue<string>();
                this.mutex = new Mutex();
                this.hasItemsEvent = new ManualResetEvent(false);   // initially is empty
            }

            public void Insert(string s)
            {
                // wait until we have the mutex
                mutex.WaitOne();

                // insert into the buffer
                theQueue.Enqueue(s);

                // signal any threads waiting to remove an object
                hasItemsEvent.Set();

                mutex.ReleaseMutex();
            }

            public string Remove()
            {
                // wait until there is at least one object in the queue and we have the mutex
                WaitHandle.WaitAll(new WaitHandle[] { mutex, hasItemsEvent });

                // remove the item from the buffer
                string s = theQueue.Dequeue();

                // block any threads waiting to remove, if the queue is empty
                if (theQueue.Count == 0)
                {
                    hasItemsEvent.Reset();
                }
                
                mutex.ReleaseMutex();

                return s;
            }

            public int Count()
            {
                // wait until we have the mutex
                // return the number of items in the queue
                mutex.WaitOne();
                int c = theQueue.Count;
                mutex.ReleaseMutex();
                return c;
            }
        }

        class Handler : TerminalInterruptHandler
        {
            private TerminalDriver driver;
            private List<char> partialLineQueue;
            private LineQueue completedLineQueue;

            public Handler(TerminalDriver driver, LineQueue completedLineQueue)
            {
                this.driver = driver;
                this.completedLineQueue = completedLineQueue;
                this.partialLineQueue = new List<char>();
            }

            public bool Echo { get; set; }

            public void HandleInterrupt(TerminalInterrupt interrupt)
            {
                switch (interrupt)
                {
                    case TerminalInterrupt.CHAR:
                        // queue up the characters until we have a completed line
                        char c = driver.RecvChar();
                        if(Echo)
                        {
                            driver.SendChar(c);
                        }
                        partialLineQueue.Add(c);
                        break;

                    case TerminalInterrupt.ENTER:
                        // get all the characters from the partial line queue and create a completed line
                        completedLineQueue.Insert(new string (partialLineQueue.ToArray()));
                        partialLineQueue.Clear();
                        if (Echo)
                        {
                            driver.SendNewLine();
                        }
                        break;

                    case TerminalInterrupt.BACK:
                        // throw away the last character entered
                        // TODO
                        break;
                }
            }
        }
    }
}
