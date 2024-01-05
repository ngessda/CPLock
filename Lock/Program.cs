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
            ConcurrentData data = new ConcurrentData();
            Producer producer = new Producer(data, 10, 2);
            Consumer consumer = new Consumer(data, 10, 2);
            producer.Start();
            consumer.Start();
            Console.ReadKey();
        }
    }
}
