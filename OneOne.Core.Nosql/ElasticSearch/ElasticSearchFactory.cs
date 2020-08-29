using System;
using System.Collections.Generic;
using System.Text;

namespace OneOne.Core.Nosql.ElasticSearch
{
    using System.Linq;

    using Elasticsearch.Net;

    using Nest;

    /// <summary>
    /// ES操作工厂类
    /// </summary>
    public class ElasticSearchFactory
    {
        private static ElasticClient client = null;

        /// <summary>
        /// uri需要作为配置项处理
        /// </summary>
        static ElasticSearchFactory()
        {
            var nodes = new Uri[]
                        {
                            new Uri("http://localhost:9200")
                        };

            var pool = new StaticConnectionPool(nodes);
            var settings = new ConnectionSettings(pool);
            
            client = new ElasticClient(settings);
        }

        /// <summary>
        /// 保存数据
        /// 更新或者修改
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        public static void SaveData<T>(T data) where T : EsDocumentBase, new()
        {
            if (data == null)
            {
                return;
            }

            if (CreateIndex<T>(data.IndexName))
            {
                var result = client.Index(data, idx => idx.Index(data.IndexName));
                if (!result.IsValid)
                {
                    throw result.OriginalException;
                }
            }
        }

        /// <summary>
        /// 删除数据
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="id">唯一id</param>
        public static void DeleteData<T>(int id) where T : EsDocumentBase, new()
        {
            var obj = new T();
            var response = client.IndexExists(obj.IndexName);
            if (response.Exists)
            {
                var deleteRequest = new DeleteRequest(obj.IndexName, id);
                var result = client.Delete(deleteRequest);
                if (!result.IsValid)
                {
                    throw result.OriginalException;
                }
            }
        }

        /// <summary>
        /// 搜索
        /// </summary>
        /// <typeparam name="T">返回的数据类型</typeparam>
        /// <param name="selector">搜索表达式</param>
        /// <returns></returns>
        public static List<T> Search<T>(Func<SearchDescriptor<T>, ISearchRequest> selector) where T : EsDocumentBase, new()
        {
            var obj = new T();
            var response = client.IndexExists(obj.IndexName);
            if (response.Exists)
            {
                var result = client.Search<T>(selector);
                if (result.IsValid)
                {
                    return result.Documents.ToList();
                }
                else
                {
                    throw result.OriginalException;
                }
            }
            throw new InvalidOperationException(string.Format("索引{0}不存在", obj.IndexName));
        }

        /// <summary>
        /// 创建索引，1个副本，5个分片
        /// 当索引存在的时候返回true
        /// 当索引不存在则创建，并根据T进行创建索引的mapping
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="indexName"></param>
        /// <returns></returns>
        private static bool CreateIndex<T>(string indexName) where T : class, new()
        {
            var response = client.IndexExists(indexName);
            if (!response.Exists)
            {
                IIndexState indexState = new IndexState()
                {
                    Settings = new IndexSettings()
                    {
                        NumberOfReplicas = 1, //副本数
                        NumberOfShards = 5 //分片数
                    }
                };
                var indexResponse = client.CreateIndex(indexName, p => p.InitializeUsing(indexState).Mappings(x => x.Map<T>(y => y.AutoMap())));
                if (indexResponse.IsValid)
                {
                    return true;
                }

                throw indexResponse.OriginalException;
            }

            return true;
        }
    }
}
