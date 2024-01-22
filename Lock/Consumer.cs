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
        private static int threadsOfConsumerCount = 0;
        private Thread t = null;
        private ConcurrentData cData;
        private object consumerLock = new object();
        private bool isStopped = false;
        private int totalToProduce;
        private int workPlan;
        private int partCount;
        

        public Consumer(ConcurrentData data, int productCount, int partCountOfTotal)
        {
            cData = data;
            totalToProduce = productCount;
            partCount = partCountOfTotal;
            workPlan = totalToProduce / partCount;
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
            Thread.CurrentThread.Name = $"consumer {consumerId}";
            while (!IsStopped)
            {
                    lock (cData)
                {
                    if (cData.Data.Count == 0 && cData.IsProducerAlive)
                    {
                        Monitor.Wait(cData);
                    }
                    if (threadsOfConsumerCount > workPlan || !cData.IsProducerAlive)
                    {
                        Stop();
                        continue;
                    }

                    if (cData.Data.Count < workPlan && cData.Flag)
                    {
                        if (cData.IsProducerAlive)
                        {
                            Monitor.Wait(cData);
                        }
                        else
                        {
                            var count = cData.Data.Count;
                            var countForCycle = count / threadsOfConsumerCount;
                            Monitor.Enter(cData);
                            if (count % threadsOfConsumerCount != 0 && cData.Flag)
                            {
                                countForCycle++;
                                cData.Flag = false;
                            }
                            Monitor.Exit(cData);
                            for (int i = 0; i < countForCycle; i++)
                            {
                                var elem = cData.Data.Pop();
                                Console.WriteLine($"Consumer #{consumerId}: consumed value {elem}");
                            }
                            Stop();
                        }
                    }
                    else
                    {
                        var count = cData.Data.Count;
                        var countForCycle = workPlan / threadsOfConsumerCount;
                        Monitor.Enter(cData);
                        if (workPlan % threadsOfConsumerCount != 0 && cData.Flag)
                        {
                            countForCycle++;
                            cData.Flag = false;
                        }
                        Monitor.Exit(cData);

                        if (!cData.IsProducerAlive)
                        {
                            Stop();
                            continue;
                        }

                        for (int i = 0; i < countForCycle; i++)
                        {
                            var elem = cData.Data.Pop();
                            Console.WriteLine($"Consumer #{consumerId}: consumed value {elem}");
                        }
                        if (cData.Data.Count == 0)
                        {
                            Monitor.Pulse(cData);
                        }
                    }
                }
                    Thread.Sleep(150);
            }
            Console.WriteLine($"Consumer's #{consumerId} job is over");
        }
        public void Start()
        {
            if(t == null)
            {
                IsStopped = false;
                t = new Thread(Consume);
                threadsOfConsumerCount++;
                t.Start();
            }
        }
        public void Stop()
        {
            if (t != null && t.IsAlive)
            {
                isStopped = true;
                threadsOfConsumerCount--;
            }
        }
    }
}
