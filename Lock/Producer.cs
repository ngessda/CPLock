using System;
using System.Configuration;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Lock
{
    public class Producer
    {
        private Thread t = null;
        private ConcurrentData cData;
        private object producerLock = new object();
        private bool isStopped = false;
        private int _NTotalToProduce;
        private int _nCounterOfParts;
        private int _MPartsToProduce;
        private static Random random = new Random();

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
        public Producer(ConcurrentData data, int productCount, int parts)
        {
            cData = data;
            _NTotalToProduce = productCount;
            _nCounterOfParts = parts;
            _MPartsToProduce = _NTotalToProduce / _nCounterOfParts;
    }
        private void Produce()
        {
            int currentCount = 0;
            while (!IsStopped)
            {
                if (currentCount == _NTotalToProduce)
                {
                    Stop();
                    continue;
                }
                lock (cData)
                {
                    var data = random.Next(256);
                    Console.WriteLine($"Produce: produced value {data}");
                    cData.Data.Push(data);
                    currentCount++;
                    if (currentCount % _MPartsToProduce == 0 || currentCount == _NTotalToProduce)
                    {
                        cData.Flag = true;
                        Monitor.Pulse(cData);
                        Monitor.Wait(cData);
                    }
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
