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

            while (!IsStopped)
            {
                lock (cData)
                {
                    if (!cData.Flag && cData.IsProducerAlive)
                    {
                        Monitor.Wait(cData);
                        continue;
                    }
                    else
                    {
                        if (cData.Data.Count < workPlan)
                        {
                            var count = cData.Data.Count;
                            for (int i = 0; i < count; i++)
                            {
                                var elem = cData.Data.Pop();
                                Console.WriteLine($"Consumer #{consumerID}: consumed value {elem}");
                            }
                            Stop();
                        }
                        else
                        {
                            for (int i = 0; i < workPlan; i++)
                            {
                                var elem = cData.Data.Pop();
                                Console.WriteLine($"Consumer #{consumerID}: consumed value {elem}");
                            }
                        }
                    }
                    Monitor.Pulse(cData);
                    cData.Flag = false;
                }
                Thread.Sleep(150);
                
            }
            Console.WriteLine($"Consumer's #{consumerID} job is over");
        }

        public void Start()
        {
            if(t == null)
            {
                IsStopped = false;
                t = new Thread(Consume);
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
