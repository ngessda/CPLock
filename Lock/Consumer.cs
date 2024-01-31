using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Schema;

namespace Lock
{
    public class Consumer
    {
        private Thread t = null;
        private ConcurrentData cData;
        private object consumerLock = new object();
        private bool isStopped = false;
        private int workPlan;
        private static int cThread = 0;
        private bool _flag = true;

        public Consumer(ConcurrentData data, int dividedCount)
        {
            cData = data;
            workPlan = dividedCount;
        }
        public bool IsStopped
        {
            get
            {
                lock (consumerLock)
                {
                    return isStopped;
                }
            }
            private set
            {
                lock(consumerLock)
                {
                    isStopped = value;
                }
            }
        }
        private void Consume()
        {
            int consumerID = Thread.CurrentThread.ManagedThreadId;
            Thread.CurrentThread.Name = $"Consumer #{consumerID}";

            while (!IsStopped)
            {
                lock (cData)
                {
                    if (!cData.Flag && cData.IsProducerAlive || cData.Data.Count == 0 && cData.IsProducerAlive)
                    {
                        Monitor.Wait(cData);
                        continue;
                    }
                    else if (cData.Data.Count == 0 && !cData.IsProducerAlive) 
                    {
                        Stop();
                        continue;
                    }
                    else
                    {
                        if (cData.Data.Count < workPlan)
                        {
                            var count = cData.Data.Count;
                            for (int i = 0; i < PlanWithThreads(); i++)
                            {
                                var elem = cData.Data.Pop();
                                Console.WriteLine($"Consumer #{consumerID}: consumed value {elem}");
                            }
                        }
                        else
                        {
                            int cycleValue = PlanWithThreads();
                            for (int i = 0; i < cycleValue; i++)
                            {
                                var elem = cData.Data.Pop();
                                Console.WriteLine($"Consumer #{consumerID}: consumed value {elem}");
                            }
                        }
                    }
                    if (cData.Data.Count == 0)
                    {
                        cData.Flag = false;
                        _flag = true;
                    }
                    Monitor.Pulse(cData);
                }
                Thread.Sleep(150);
                
            }
            Console.WriteLine($"Consumer's #{consumerID} job is over");
        }
        private int PlanWithThreads()
        {
            int planWithThreads = workPlan / cThread;
            if (workPlan % cThread != 0 && _flag)
            {
                planWithThreads++;
                _flag = false;
                return planWithThreads;
            }
            else
            {
                return planWithThreads;
            }
        }

        public void Start()
        {
            if(t == null)
            {
                IsStopped = false;
                t = new Thread(Consume);
                cThread++;
                t.Start();
            }
        }
        public void Stop()
        {
            if (t != null && t.IsAlive)
            {
                isStopped = true;
            }
        }
    }
}
