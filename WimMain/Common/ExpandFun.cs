/*
 * Author：步尘
 * CreatDate ：2018-10-01 12:06:34
 * CLR Version ：4.0.30319.42000
 */
using System;
using System.Collections.Generic;
using System.Data;

namespace WimMain.Common
{
    /// <summary>
    /// 拓展方法
    /// </summary>
    public static class ExpandFun
    {
        /// <summary>
        /// 返回数组是否有值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ts"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public static bool HasItems<T>(this T[] ts, Func<T[], bool> func = null) => (ts != null && ts.Length > 0) && (func == null || func.Invoke(ts));

        /// <summary>
        /// 返回集合是否有值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ts"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public static bool HasItems<T>(this ICollection<T> ts, Func<ICollection<T>, bool> func = null) => (ts != null && ts.Count > 0) && (func == null || func.Invoke(ts));

        /// <summary>
        /// 返回DataTable是否有值
        /// </summary>
        /// <param name="ts"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public static bool HasItems(this DataTable ts, Func<DataTable, bool> func = null) => (ts != null && ts.Rows != null && ts.Rows.Count > 0) && (func == null || func.Invoke(ts));

        
    }
}
