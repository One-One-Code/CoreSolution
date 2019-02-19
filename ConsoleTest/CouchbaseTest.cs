using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleTest
{
    using System.IO;
    using System.Linq;

    using Couchbase;
    using Couchbase.Authentication;
    using Couchbase.Configuration.Client;

    using Newtonsoft.Json;

    using ProtoBuf;

    using OneOne.Core.Cache.Couchbase;

    public class CouchbaseTest
    {
        private static Cluster _instance;

        public static void Test()
        {
            var clientConfiguration = new ClientConfiguration() { Servers = new List<Uri>(), BucketConfigs = new Dictionary<string, BucketConfiguration>() };
            
          
            clientConfiguration.Servers.Add(new Uri("http://10.0.2.190:8091/pools"));
            //var authenticator = new PasswordAuthenticator("username", "password");
            var clusterCredentials = new ClusterCredentials { BucketCredentials = new Dictionary<string, string>() };
            clusterCredentials.BucketCredentials.Add("MOTK-ExamResult", "OneOne2014");
            var manager = new CouchbaseManager(clientConfiguration, clusterCredentials);
            var bucket = manager.GetBucket("MOTK-ExamResult");
            var query = bucket.CreateQuery("test", "tv").Limit(5000);
            var result1 = bucket.Query<dynamic>(query);
            
            bucket.Upsert(CacheObject.Key, new CacheObject() { ObjId = 33, ObjName = "ss", Remark = ".Net Core Write" });
            var result = bucket.Get<CacheObject>(CacheObject.Key);
            Console.WriteLine(JsonConvert.SerializeObject(result));

        }


    }

    [ProtoContract]
    public class CacheObject
    {
        public static string Key = "CacheObject";

        [ProtoMember(1)]
        public int ObjId { get; set; }

        [ProtoMember(2)]
        public string ObjName { get; set; }

        [ProtoMember(3)]
        public string Remark { get; set; }
    }
}
