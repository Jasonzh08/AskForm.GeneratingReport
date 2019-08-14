/*
 * Author：步尘
 * CreatDate ：2018-10-01 10:16:51
 * CLR Version ：4.0.30319.42000
 */
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace WimMain.Common
{
    /// <summary>
    /// 配置文件操作
    /// </summary>
    public class ToolConfig
    {
        /// <summary>
        /// 配置文件字典缓存
        /// </summary>
        private static Dictionary<string, string> ConfigDic = new Dictionary<string, string>();

        /// <summary>
        /// 添加配置字典
        /// </summary>
        /// <param name="key"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        private static string AddConfigDic(string key, Func<string> action)
        {
            string res = string.Empty;

            if (!ConfigDic.Keys.Contains(key))
            {
                res = action.Invoke();
                if (res != null)
                    ConfigDic.Add(key, res);
            }
            else res = ConfigDic[key];

            return res;
        }

        /// <summary>
        /// 读取配置文件appSettings
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">配置名称</param>
        /// <returns>配置值</returns>
        public static string GetAppSetting(string key)
        {
            return AddConfigDic(key,
                () => ConfigurationManager.AppSettings[key] ?? "");
        }

        /// <summary>
        /// 修改配置文件appSettings
        /// </summary>
        /// <param name="key"></param>
        /// <param name="val"></param>
        public static void SetAppSetting(string key, object val)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            // 修改值
            config.AppSettings.Settings[key].Value = val + "";
            config.AppSettings.SectionInformation.ForceSave = true;

            // 仅保存修改的值
            config.Save(ConfigurationSaveMode.Modified);
            // 刷新节点
            ConfigurationManager.RefreshSection("appSettings");

            ConfigDic.Clear();
        }

        /// <summary>
        /// 返回连接字符串
        /// </summary>
        /// <param name="index">连接字符串索引</param>
        /// <returns></returns>
        public static string GetConStr(int index = 0)
        {
            return AddConfigDic($"ConnectionStrings[{index}]",
                () => ConfigurationManager.ConnectionStrings[index].ConnectionString);
        }

        /// <summary>
        /// 返回连接字符串
        /// </summary>
        /// <param name="name">连接字符串名称</param>
        /// <returns></returns>
        public static string GetConStr(string name)
        {
            return AddConfigDic($"ConnectionStrings[{name}]",
                () => ConfigurationManager.ConnectionStrings[name].ConnectionString);
        }

        /// <summary>
        /// 返回连接字符串列表
        /// </summary>
        /// <returns></returns>
        public static List<string> GetConList()
        {
            List<string> list = new List<string>();
            foreach (ConnectionStringSettings item in ConfigurationManager.ConnectionStrings)
                list.Add(item.Name);

            list.Remove("LocalSqlServer");

            return list;
        }
    }
}