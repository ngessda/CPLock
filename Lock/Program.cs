using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lock
{
    internal class Program
    {
        static void Main(string[] args)
        {
            int counter = 10;
            int div = 3;
            ConcurrentData data = new ConcurrentData();
            Producer producer = new Producer(data, counter, div);
            Consumer consumer = new Consumer(data, counter / div);
            producer.Start();
            consumer.Start();
            Console.ReadKey();
        }
    }
}
