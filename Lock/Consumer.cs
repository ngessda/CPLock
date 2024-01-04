using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Lock
{
    public class Consumer
    {
        private Thread t = null;
        private ConcurrentData cData;
        private object consumerLock = new object();
        private bool isStopped = false;
        private int workPlan = 3;

        public Consumer(ConcurrentData data)
        {
            cData = data;
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
            int consumerId = Thread.CurrentThread.ManagedThreadId;
            while (!IsStopped)
            {
                lock (cData)
                {
                    if(cData.Data.Count < workPlan)
                    {
                        if (cData.IsProducerAlive)
                        {
                            Monitor.Wait(cData);
                        }
                        else
                        {
                            var count = cData.Data.Count;
                            for (int i = 0; i < count; i++)
                            {
                                var elem = cData.Data.Pop();
                                Console.WriteLine($"Consumer #{consumerId}: consumed value {elem}");
                            }
                            Stop();
                        }
                    }
                    else
                    {
                        for (int i = 0; i < workPlan; i++)
                        {
                            var elem = cData.Data.Pop();
                            Console.WriteLine($"Consumer #{consumerId}: consumed value {elem}");
                        }
                    }
                }
                Thread.Sleep(1500);
            }
            Console.WriteLine($"Consumer's #{consumerId} job is over");
        }
        public void Start()
        {
            if(t == null)
            {
                IsStopped = false;
                t = new Thread(Start);
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
