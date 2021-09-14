using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APIServices.Common
{
    public class Transaction
    {
        public string FromPublicKey { get; internal set; }
        public string ToPublicKey { get; internal set; }
        public double Amount { get; internal set; }
        public string Signature { get; internal set; }

        public override string ToString()
        {
            return $"{this.Amount}:{this.FromPublicKey}:{this.ToPublicKey}";
        }
    }
}
