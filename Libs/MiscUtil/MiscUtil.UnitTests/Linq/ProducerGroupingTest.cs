#if DOTNET35
using System;
using MiscUtil.Linq;
using MiscUtil.Linq.Extensions;
using NUnit.Framework;

namespace MiscUtil.UnitTests.Linq
{
    [TestFixture]
    public class ProducerGroupingTest
    {
        [Test]
        public void EventHooks()
        {
            var producer = new DataProducer<int>();
            var grp = new ProducerGrouping<int, int>(5, producer);
            Action<int> act = x => { };
            Action end = () => { };
            IFuture<int> sum = grp.Sum();
            grp.DataProduced += act;
            grp.EndOfData += end;
            producer.ProduceAndEnd(1, 2, 3);
            grp.DataProduced -= act;
            grp.EndOfData -= end;
            Assert.AreEqual(6, sum.Value);

        }
    }
}
#endif