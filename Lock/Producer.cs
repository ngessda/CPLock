using System;
using System.Threading;

namespace Lock
{
    public class Producer
    {
        private Thread t = null;
        private ConcurrentData cData;
        private object producerLock = new object();
        private bool isStopped = false;
        private int totalToProduce;
        private Random random;

        public bool IsStopped
        {
            get
            {
                lock (producerLock)
                {
                    return isStopped;
                }
            }
            private set
            {
                lock (producerLock)
                {
                    isStopped = value;
                }
            }
        }
        public Producer(ConcurrentData data, int productCount)
        {
            cData = data;
            totalToProduce = productCount;
            random = new Random();
        }
        private void Produce()
        {
            int currentCount = 0;
            while (!IsStopped)
            {
                if(currentCount == totalToProduce)
                {
                    Stop();
                    continue;
                }
                lock (cData)
                {
                    var data = random.Next(256);
                    Console.WriteLine($"Produce: produced value {data}");
                    cData.Data.Push(data);
                    Monitor.Pulse(data);
                    currentCount++;
                }
                Thread.Sleep(200);
            }
            Console.WriteLine("Producer's job is over");
        }
        public void Start()
        {
            if(t == null)
            {
                lock (cData)
                {
                    cData.IsProducerAlive = true;
                }
                isStopped = false;
                t = new Thread(Produce);
                t.Start();
            }
        }
        public void Stop()
        {
            if(t != null && t.IsAlive)
            {
                lock (cData)
                {
                    cData.IsProducerAlive = false;
                    Monitor.PulseAll(cData);
                }
                isStopped = true;
            }
        }
    }
}
