using MpcLib.Common;
using MpcLib.DistributedSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpcLib.BasicProtocols
{
    public class BasicMessage<T> : Msg where T : ISizable, IEquatable<T>
    {
        public readonly T Value;

        public BasicMessage(T value)
            : base(MsgType.Basic)
        {
            Value = value;
        }
        
        public override string ToString()
        {
            return Value.ToString();
        }

        public override int Size
        {
            get
            {
                return Value.Size;
            }
        }
    }
}