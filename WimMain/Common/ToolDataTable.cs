using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace WimMain.Common
{
    /// <summary>
    /// 当属性加上此特性，可以和DataTable中的列字段相匹配
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class DataRowNameAttribute : Attribute
    {
        public List<string> DtNameArr { get; } = new List<string>();

        /// <summary>
        /// 对应的列名称
        /// </summary>
        /// <param name="dtName"></param>
        public DataRowNameAttribute(string dtName)
        {
            DtNameArr.Add(dtName);
            DtNameArr = DtNameArr.Distinct().ToList();
        }

        /// <summary>
        /// 对应的列名称
        /// </summary>
        /// <param name="dtName"></param>
        public DataRowNameAttribute(string[] dtName)
        {
            DtNameArr.AddRange(dtName);
            DtNameArr = DtNameArr.Distinct().ToList();
        }

    }

    /// <summary>
    /// DataTable工具
    /// </summary>
    public static class ToolDataTable
    {
        private static Dictionary<string, object> DicCache = new Dictionary<string, object>();

        /// <summary>
        /// 添加缓存
        /// </summary>
        /// <param name="key"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        private static T AddCache<T>(string key, Func<T> func)
        {
            T res = default(T);

            if (!DicCache.Keys.Contains(key))
            {
                res = func.Invoke();
                if (res != null)
                    DicCache.Add(key, res);
                return res;
            }
            else res = (T)DicCache[key];

            return res;
        }

        /// <summary>
        /// 创建Datatable结构
        /// </summary>
        /// <param name="dic"></param>
        /// <returns></returns>
        public static DataTable CreateDataTable(Dictionary<string, Type> dic, Func<Dictionary<string, Type>, Dictionary<string, Type>> func)
        {
            Dictionary<string, Type> tmp = new Dictionary<string, Type>(dic);

            return CreateDataTable(func.Invoke(tmp));
        }

        /// <summary>
        /// 创建Datatable结构
        /// </summary>
        /// <param name="dic">[列名，类型]</param>
        /// <returns></returns>
        public static DataTable CreateDataTable(Dictionary<string, Type> dic)
        {
            DataTable dt = new DataTable();

            foreach (var key in dic.Keys)
                dt.Columns.Add(key, dic[key]);
            return dt;
        }

        /// <summary>
        /// 填充数据，
        /// 数据源属性应与DataRow字段名相同，或属性添加DataRowName特性，不区分大小写
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dr">待填充数据的DataRow</param>
        /// <param name="obj">数据源</param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static DataRow FullDataRow<T>(DataRow dr, T obj, Func<DataRow, T, DataRow> func = null) where T : class, new()
        {
            Type type = typeof(T);

            #region 获取属性信息

            // 获取一个类型所有的属性，并存入缓存
            PropertyInfo[] proArr = AddCache("DaraRow:" + type.FullName, () => type.GetProperties());

            // 获取属性中对应的特性，存入缓存
            Dictionary<PropertyInfo, List<string>> dicAttribute = AddCache("DaraRowAttribute:" + type.FullName, () =>
            {
                Dictionary<PropertyInfo, List<string>> dicTem = new Dictionary<PropertyInfo, List<string>>();

                // 循环属性
                foreach (var pro in proArr)
                {
                    // 判断是否存在指定特性
                    if (pro.IsDefined(typeof(DataRowNameAttribute), true))
                    {
                        // 将属性和特性名称存入字典
                        dicTem.Add(pro, ((DataRowNameAttribute)(pro.GetCustomAttributes(typeof(DataRowNameAttribute), true).FirstOrDefault())).DtNameArr);
                    }
                }
                return dicTem;
            });

            #endregion

            foreach (DataColumn dc in dr.Table.Columns)
            {
                #region 寻找匹配属性

                // 寻找字段对应的属性
                PropertyInfo pro = proArr.Where(c =>
                    c.Name.ToLower() == dc.ColumnName.ToLower()).FirstOrDefault();

                // 在特性中寻找匹配的属性
                if (pro == null && dicAttribute != null)
                {
                    foreach (var key in dicAttribute.Keys)
                    {
                        // 若存在匹配的属性
                        if (dicAttribute[key].Where(c => c.ToLower() == dc.ColumnName.ToLower()).Count() > 0)
                        {
                            pro = key;
                            continue;
                        }
                    }
                    if (pro == null)
                        continue;
                };
                #endregion

                // 为指定字段赋值
                dr[dc.ColumnName] = pro.GetValue(obj, null);
            }

            if (func != null)
                dr = func.Invoke(dr, obj);
            return dr;
        }

        /// <summary>
        /// DataTable转List
        /// </summary>
        /// <typeparam name="T">待转换类型</typeparam>
        /// <param name="dt"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public static List<T> ToList<T>(this DataTable dt, Func<DataColumn, string> func = null) where T : class, new()
        {
            List<T> list = new List<T>();

            if (dt == null) return list;

            foreach (DataRow dr in dt.Rows)
            {
                T obj = Activator.CreateInstance<T>();

                foreach (DataColumn column in dr.Table.Columns)
                {
                    string proName = column.ColumnName;

                    if (func != null)
                        proName = func.Invoke(column);

                    PropertyInfo prop = obj.GetType().GetProperty(proName);
                    if (prop == null) continue;
                    object value = dr[column.ColumnName];
                    prop.SetValue(obj, Convert.ChangeType(value + "", prop.PropertyType), null);
                }

                list.Add(obj);
            }
            return list.Distinct().ToList();
        }

        /// <summary>
        /// List转DataTable
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public static DataTable ToTable<T>(this List<T> list, Func<T, string> func = null)
        {
            DataTable dt = new DataTable();
            // 获取类型属性
            PropertyInfo[] proArr = typeof(T).GetProperties();
            // 添加头
            dt.Columns.AddRange(proArr.Select(c => new DataColumn(c.Name, c.PropertyType)).ToArray());

            if (dt.Columns.Count > 0)
            {
                // 循环数据
                foreach (var item in list)
                {
                    DataRow dr = dt.NewRow();
                    foreach (var pro in proArr)
                    {
                        dr[pro.Name] = pro.GetValue(item);
                    }
                    dt.Rows.Add(dr);
                }
            }
            return dt;
        }

        /// <summary>
        /// DataTable合并
        /// </summary>
        /// <param name="dt">主数据表</param>
        /// <param name="unionDt">待合并的数据表</param>
        /// <param name="columnList">合并关联字段</param>
        /// <returns></returns>
        public static DataTable Union(this DataTable dt, DataTable unionDt, List<DataColumn> columnList)
        {
            if (dt == null || unionDt == null || columnList == null || columnList.Count == 0)
                return dt;

            // 移除副表标题相同的列
            foreach (DataColumn item in dt.Columns)
            {
                // 关联字段不移除
                if (columnList.Exists(c => c.ColumnName == item.ColumnName))
                    continue;
                // 移除
                if (unionDt.Columns.Contains(item.ColumnName))
                    unionDt.Columns.Remove(item.ColumnName);
            }
            // 将副表的字段合并到主表
            foreach (DataColumn item in unionDt.Columns)
            {
                if (columnList.Exists(c => c.ColumnName == item.ColumnName))
                    continue;
                dt.Columns.Add(item.ColumnName, item.DataType);
            }

            // 循环主表，根据主表数据，查找相关的副表数据
            foreach (DataRow dr in dt.Rows)
            {
                string where = "";
                // 查找关联数据
                foreach (var item in columnList)
                    where += item.ColumnName + "='" + dr[item.ColumnName] + "' and ";

                where = where.Substring(0, where.Length - 5);

                // 
                DataRow[] drSel = unionDt.Select(where);

                if (drSel.Length <= 0 || drSel.Length > 1)
                    continue;

                // 添加数据
                foreach (DataColumn item in unionDt.Columns)
                {
                    dr[item.ColumnName] = drSel[0][item.ColumnName];
                }
            }
            return dt;
        }

    }
}