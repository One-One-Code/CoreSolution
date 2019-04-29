using System;
using System.Collections.Generic;
using System.Text;

namespace OneOne.Core.Nosql.ElasticSearch
{
    /// <summary>
    /// es数据的基类
    /// 所有document对象都必须集成该类
    /// </summary>
    public abstract class EsDocumentBase
    {
        public abstract string IndexName { get; }
    }
}
