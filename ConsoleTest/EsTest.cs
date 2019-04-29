using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleTest
{
    using Nest;

    using OneOne.Core.Nosql.ElasticSearch;

    public class EsTest
    {
        public static void Test()
        {
            for (int i = 0; i < 50; i++)
            {
                var people = new ObjEs();
                people.ObjId = i;
                people.Content = "A、B、C为数轴上三点，若点C到点A的距离是点C到点B的距离的2倍，则称点C是(A，B)的奇异点，例如图1中，点A表示的数为-1，点B表示的数为2，表示1的点C到点A的距离为2，到点B的距离为1，则点C是(A，B)的奇异点，但不是(B，A)的奇异点";
                people.Name = "黄平";
                people.Score = people.ObjId * 0.15;
                ElasticSearchFactory.SaveData(people);
                
            }
            var result = ElasticSearchFactory.Search<ObjEs>(p => p.Index(new ObjEs().IndexName).Query(
                x => x.TermRange(y => y.Field(a => a.Score).GreaterThan("1.5").LessThan("3.0"))));
        }
    }

    [ElasticsearchType(IdProperty = "ObjId")]
    public class ObjEs : EsDocumentBase
    {
        [Number(NumberType.Long, Index = true, Name = "ObjId")]
        public long ObjId { get; set; }

        [Keyword(Index = true, Name = "Name")]
        public string Name { get; set; }

        [Text(Index = true, Analyzer = "ik", Name = "Content")]
        public string Content { get; set; }

        [Number(NumberType.Double, Index = true, Name = "Score")]
        public double Score { get; set; }

        public override string IndexName => "test-1";
    }
}
