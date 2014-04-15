using System.Collections.Generic;
using System.IO;
using MiscUtil.IO;
using MiscUtil.Linq;
using MiscUtil.Linq.Extensions;
using NUnit.Framework;

namespace MiscUtil.UnitTests.Linq
{
    [TestFixture]
    public class ExampleUsageTest
    {
        private const string FileContents =
    @"# rough cast of flintones 
#family flintstones 
Fred~Flintstone~32 
Wilma~Flintstone~31 
#family rubble 
Barney~Rubble~30 
Betty~Rubble~29 
#the kids 
BamBam~Flintstone~2 
Pebbles~Rubble~1 
# pets 
Dino~Flintstone~4 
#eof";
     private static readonly string ExpectedOutput = NormalizeLineEndings(
     @"Max Line Length: 26
Line Count: 13
Comments
rough cast of flintones
family flintstones
family rubble
the kids
pets
eof
People
{ Forename = Fred, Surname = Flintstone, Age = 32 }
{ Forename = Wilma, Surname = Flintstone, Age = 31 }
{ Forename = Barney, Surname = Rubble, Age = 30 }
{ Forename = Betty, Surname = Rubble, Age = 29 }
{ Forename = BamBam, Surname = Flintstone, Age = 2 }
{ Forename = Pebbles, Surname = Rubble, Age = 1 }
{ Forename = Dino, Surname = Flintstone, Age = 4 }
Stats
{ Surname = Flintstone, Count = 4, MaxAge = 32 }
{ Surname = Rubble, Count = 3, MaxAge = 30 }
");
        [Test]
        public void ComplexFileParsing()
        {
            // the push-LINQ start-point; a datap-producer 
            var source = new DataProducer<string>();

            // listen to data and log and comments 
            // (note that we don't have to use a list here, 
            // we could do something more interesting as 
            // we see the lines) 
            var comments = (from line in source
                            where line.StartsWith("#")
                            select line.TrimStart('#').Trim()).ToList();

            // listen to data and create entities from 
            // lines that aren't comments 
            var people = from line in source
                         where !line.StartsWith("#")
                         let fields = line.Split('~')
                         select new
                         {
                             Forename = fields[0],
                             Surname = fields[1],
                             Age = int.Parse(fields[2])
                         };

            // just for the fun of it, find the longest line-length etc 
            var maxLen = source.Max(line => line.Length);
            var count = source.Count();

            // and while we're having fun, perform some aggregates 
            // on the people *as we're reading them!* 
            // (not afterwards, like how LINQ-to-objects works) 
            var stats = (from person in people
                         group person by person.Surname into grp
                         let agg = new
                         {
                             Surname = grp.Key,
                             Count = grp.Count(),
                             MaxAge = grp.Max(p => p.Age)
                         }
                         orderby agg.Surname
                         select agg).ToList();

            // and we'll want to catch the people 
            var peopleList = people.ToList();

            // now that we've set everything up 
            // read the file *once* 
            //source.ProduceAndEnd(new LineReader(path)); 
            source.ProduceAndEnd(new LineReader(() => new StringReader(FileContents)));

            // sort the groups 
            //stats.Sort(grp => grp.Surname);

            // show what we got
            TextWriter output = new StringWriter();
            output.WriteLine("Max Line Length: {0}", maxLen);
            output.WriteLine("Line Count: {0}", count);
            Write(output, "Comments", comments);
            Write(output, "People", peopleList);
            Write(output, "Stats", stats);

            Assert.AreEqual(ExpectedOutput, NormalizeLineEndings(output.ToString()));
        }

        static string NormalizeLineEndings(string str)
        {
            return str.Replace("\r\n", "\n");
        }

        static void Write<T>(TextWriter output, string caption, IEnumerable<T> items)
        {
            output.WriteLine(caption);
            foreach (var item in items)
            {
                output.WriteLine(item);
            }
        }
    }
}