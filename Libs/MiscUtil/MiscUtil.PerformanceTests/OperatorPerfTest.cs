#if DOTNET35
using System;
using System.Diagnostics;
using NUnit.Framework;

namespace MiscUtil.PerformanceTests
{
    /// <summary>
    /// Compare Operator performance with compiled code
    /// </summary>
    [TestFixture]
    public class OperatorPerfTest
    {
        [Test]
        public void TestInt32Addition()
        {
            const int COUNT = 10000000, SEED = 123456, MAXVALUE = 1000;

            Func<int, int, int> add = Operator<int>.Add; // load early to avoid cost in loop
            Operator.Add<int>(1, 2);
            add(1, 2);
            Stopwatch watch = new Stopwatch();

            // test native
            Random rand = new Random(SEED);
            int sumNative = 0;
            watch.Reset();
            watch.Start();
            for (int i = 0; i < COUNT; i++)
            {
                sumNative += rand.Next(MAXVALUE);
            }
            watch.Stop();
            long msNative = watch.ElapsedMilliseconds;

            // test Operator (type inference)
            rand = new Random(SEED);
            int sumOperator = 0;
            watch.Reset();
            watch.Start();
            for (int i = 0; i < COUNT; i++)
            {
                sumOperator = Operator.Add(sumOperator, rand.Next(MAXVALUE));
            }
            watch.Stop();
            long msOperator = watch.ElapsedMilliseconds;
            Assert.AreEqual(sumNative, sumOperator);

            // test Operator<T> (simple)
            rand = new Random(SEED);
            int sumOperatorTSimple = 0;
            watch.Reset();
            watch.Start();
            for (int i = 0; i < COUNT; i++)
            {
                sumOperatorTSimple = Operator<int>.Add(sumOperatorTSimple, rand.Next(MAXVALUE));
            }
            watch.Stop();
            long msOperatorTSimple = watch.ElapsedMilliseconds;
            Assert.AreEqual(sumNative, sumOperatorTSimple);

            // test Operator<T> (optimised)
            rand = new Random(SEED);
            int sumOperatorTOptimised = 0;
            watch.Reset();
            watch.Start();
            for (int i = 0; i < COUNT; i++)
            {
                sumOperatorTOptimised = add(sumOperatorTOptimised, rand.Next(MAXVALUE));
            }
            watch.Stop();
            long msOperatorTOptimised = watch.ElapsedMilliseconds;
            Assert.AreEqual(sumNative, sumOperatorTOptimised);

            // output results
            Console.WriteLine("Int32 addition (x{0})\tNative: {1}\tOperator: {2}: Operator<T>: {3}\t(cached): {4}",
                COUNT, msNative, msOperator, msOperatorTSimple, msOperatorTOptimised);
        }


        [Test]
        public void TestDoubleAddition()
        {
            const int COUNT = 50000000, SEED = 123456;

            Func<double, double, double> add = Operator<double>.Add; // load early to avoid cost in loop
            Operator.Add<double>(1, 2);
            add(1, 2);
            Stopwatch watch = new Stopwatch();

            // test native
            Random rand = new Random(SEED);
            double sumNative = 0;
            watch.Reset();
            watch.Start();
            for (int i = 0; i < COUNT; i++)
            {
                sumNative += rand.NextDouble();
            }
            watch.Stop();
            long msNative = watch.ElapsedMilliseconds;

            // test Operator (type inference)
            rand = new Random(SEED);
            double sumOperator = 0;
            watch.Reset();
            watch.Start();
            for (int i = 0; i < COUNT; i++)
            {
                sumOperator = Operator.Add(sumOperator, rand.NextDouble());
            }
            watch.Stop();
            long msOperator = watch.ElapsedMilliseconds;
            Assert.AreEqual(sumNative, sumOperator);

            // test Operator<T> (simple)
            rand = new Random(SEED);
            double sumOperatorTSimple = 0;
            watch.Reset();
            watch.Start();
            for (int i = 0; i < COUNT; i++)
            {
                sumOperatorTSimple = Operator<double>.Add(sumOperatorTSimple, rand.NextDouble());
            }
            watch.Stop();
            long msOperatorTSimple = watch.ElapsedMilliseconds;
            Assert.AreEqual(sumNative, sumOperatorTSimple);

            // test Operator<T> (optimised)
            rand = new Random(SEED);
            double sumOperatorTOptimised = 0;
            watch.Reset();
            watch.Start();
            for (int i = 0; i < COUNT; i++)
            {
                sumOperatorTOptimised = add(sumOperatorTOptimised, rand.NextDouble());
            }
            watch.Stop();
            long msOperatorTOptimised = watch.ElapsedMilliseconds;
            Assert.AreEqual(sumNative, sumOperatorTOptimised);

            // output results
            Console.WriteLine("Double addition (x{0})\tNative: {1}\tOperator: {2}: Operator<T>: {3}\t(cached): {4}",
                COUNT, msNative, msOperator, msOperatorTSimple, msOperatorTOptimised);
        }

    }
}
#endif