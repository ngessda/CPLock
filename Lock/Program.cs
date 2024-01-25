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
            int divide = 4;
            ConcurrentData data = new ConcurrentData();
            Producer producer = new Producer(data, counter, divide);
            Consumer consumer = new Consumer(data, counter, divide);
            Consumer consumer_1 = new Consumer(data, counter, divide);
            producer.Start();
            consumer.Start();
            consumer_1.Start();
            Console.ReadKey();
        }
    }
}
