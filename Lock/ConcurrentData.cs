using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lock
{
    public class ConcurrentData
    {
        public Stack<int> Data { get; } = new Stack<int>();
        public bool IsProducerAlive { get; set; } = false;
        public bool Flag { get; set; } = false;
    }
}
