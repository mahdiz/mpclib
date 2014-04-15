using System.Collections.Generic;
using System.Linq;
using MiscUtil.Linq;
using MiscUtil.Linq.Extensions;
using NUnit.Framework;

namespace MiscUtil.UnitTests.Linq.Extensions
{
    [TestFixture]
    public partial class DataProducerExtTest
    {
        void ProduceAndCheck<T>(DataProducer<T> source, IDataProducer<T> result, T[] inputData, T[] expectedOutput)
        {
            IEnumerable<T> enumerable = result.AsEnumerable();
            source.ProduceAndEnd(inputData);
            Assert.IsTrue(expectedOutput.SequenceEqual(enumerable));
        }

        class Wrap<T>
        {
            public T Value { get; private set; }
            public Wrap(T value) { Value = value; }
        }
    }
}
