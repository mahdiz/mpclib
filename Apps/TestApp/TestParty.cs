using MpcLib.DistributedSystem;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpcLib.Apps
{
    class TestParty<T> : Party where T : class
    {
        public Protocol<T> UnderTest;
        
        public override void Start()
        {
            UnderTest.Start();
        }

        public override void Receive(int fromId, Msg msg)
        {
            UnderTest.MessageHandler(fromId, msg);
        }
    }
}
