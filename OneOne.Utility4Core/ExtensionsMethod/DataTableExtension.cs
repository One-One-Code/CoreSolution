using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Text;

namespace OneOne.Utility4Core.Helper
{
    public static class DataTableExtension
    {
        /// <summary>
        /// 将DataTable转为对象集合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="table"></param>
        /// <returns></returns>
        public static List<T> DataTableToList<T>(this DataTable table) where T : new()
        {
            var result = new List<T>();
            var properties = typeof(T).GetProperties();
            var columnDic = new Dictionary<string, int>();
            for (int i = 0; i < table.Columns.Count; i++)
            {
                columnDic.Add(table.Columns[i].ColumnName.ToUpper(), i);
            }
            for (int i = 0; i < table.Rows.Count; i++)
            {
                var model = new T();
                foreach (var p in properties)
                {
                    if (p.PropertyType.IsGenericType)
                    {
                        continue;

                    }
                    var keyName = p.Name.ToUpper();
                    if (columnDic.ContainsKey(keyName))
                    {
                        var columnIndex = columnDic[keyName];
                        if (table.Rows[i][columnIndex] != null)
                        {
                            p.SetValue(model, Convert.ChangeType(table.Rows[i][columnIndex], p.PropertyType), null);
                        }
                    }
                }
                result.Add(model);
            }

            return result;
        }

        /// <summary>
        /// 将集合转成DataTable
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objList"></param>
        /// <param name="lower"></param>
        /// <returns></returns>
        public static DataTable ListToDataTable<T>(this IEnumerable<T> objList, bool lower = false)
        {
            DataTable dtResult = new DataTable();
            List<PropertyInfo> propertiyInfos = new List<PropertyInfo>();
            //生成各列
            Array.ForEach<PropertyInfo>(typeof(T).GetProperties(), p =>
            {
                if (!p.PropertyType.IsGenericType)
                {
                    propertiyInfos.Add(p);
                    var name = lower ? p.Name.ToLower() : p.Name;
                    dtResult.Columns.Add(name, p.PropertyType);
                }

            });
            //生成各行
            foreach (var item in objList)
            {
                if (item == null)
                {
                    continue;
                }
                DataRow dataRow = dtResult.NewRow();
                propertiyInfos.ForEach(p => dataRow[lower ? p.Name.ToLower() : p.Name] = p.GetValue(item, null));
                dtResult.Rows.Add(dataRow);
            }
            dtResult.AcceptChanges();
            return dtResult;
        }
    }
}
