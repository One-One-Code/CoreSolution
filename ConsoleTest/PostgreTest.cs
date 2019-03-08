using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleTest
{
    using System.Data;

    using Npgsql;

    /// <summary>
    /// PostgreSQL demo
    /// 使用Npgsql类库
    /// </summary>
    public class PostgreTest
    {
        private static NpgsqlConnection connection = null;

        static PostgreTest()
        {
            connection = new NpgsqlConnection("Host=127.0.0.1;Username=admin;Password=123456;Database=Main");
        }

        /// <summary>
        /// 执行sql查询
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <returns></returns>
        public static T QuerySql<T>(string sql)
            where T : new()
        {
            connection.Open();
            var pros = typeof(T).GetProperties();
            var result = new T();
            using (var cmd = new NpgsqlCommand())
            {
                cmd.Connection = connection;
                cmd.CommandText = sql;
                using (var reader = cmd.ExecuteReader())
                {
                    reader.Read();
                    if (reader.HasRows)
                    {
                        foreach (var propertyInfo in pros)
                        {
                            try
                            {
                                var value = reader[propertyInfo.Name];
                                propertyInfo.SetValue(result, value);
                            }
                            catch (Exception e)
                            {

                            }
                        }
                    }

                }
            }

            return result;
        }

        /// <summary>
        /// 执行sql查询
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <returns></returns>
        public static List<T> QuerySqlMany<T>(string sql)
            where T : new()
        {
            connection.Open();
            var pros = typeof(T).GetProperties();
            var result = new List<T>();
            using (var cmd = new NpgsqlCommand())
            {
                cmd.Connection = connection;
                cmd.CommandText = sql;
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader.HasRows)
                        {
                            var obj = new T();
                            foreach (var propertyInfo in pros)
                            {
                                try
                                {
                                    var value = reader[propertyInfo.Name];
                                    propertyInfo.SetValue(obj, value);
                                   
                                }
                                catch (Exception e)
                                {

                                }
                            }
                            result.Add(obj);
                        }
                    }
                }
            }

            return result;
        }

        public static void ExecuteFunction()
        {
            connection.Open();
            var pros = typeof(User).GetProperties();
            using (var cmd = new NpgsqlCommand())
            {
                cmd.Connection = connection;
                cmd.CommandText = "sp_user_read";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("v_userid", 1);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var obj = new User();
                        foreach (var propertyInfo in pros)
                        {
                            try
                            {
                                var value = reader[propertyInfo.Name];
                                propertyInfo.SetValue(obj, value);

                            }
                            catch (Exception e)
                            {

                            }
                        }
                    }
                }
            }
        }
    }



    public class User
    {
        public int UserId { get; set; }

        public string Name { get; set; }

        public int Age { get; set; }

        public string Info { get; set; }

        public string Extension { get; set; }
    }
}
