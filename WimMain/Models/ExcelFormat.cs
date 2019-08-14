using System;
using System.Collections.Generic;
using WimMain.Common;

namespace WimMain.Models
{
    /// <summary>
    /// 返回的Excel格式
    /// </summary>
    public class ExcelFormat
    {
        /// <summary>
        /// 被评估人部门
        /// </summary>
        public string ObservedDepartment { get; set; }

        /// <summary>
        /// 被评估人姓名
        /// </summary>
        public string ObservedName { get; set; }

        /// <summary>
        /// 被评估人邮箱
        /// </summary>
        public string ObservedEmail { get; set; }

        /// <summary>
        /// 被评估人手机号码
        /// </summary>
        public string ObservedPhone { get; set; }

        /// <summary>
        /// 关系
        /// </summary>
        public string Mapping { get; set; }

        /// <summary>
        /// 权值
        /// </summary>
        public string Weight { get; set; }

        /// <summary>
        /// 评估人部门
        /// </summary>
        public string ObserverDepartment { get; set; }

        /// <summary>
        /// 评估人姓名
        /// </summary>
        public string ObserverName { get; set; }

        /// <summary>
        /// 评估人邮箱
        /// </summary>
        public string ObserverEmail { get; set; }

        /// <summary>
        /// 评估人手机号码
        /// </summary>
        public string ObserverPhone { get; set; }


        public static void CreateText(string path, List<ExcelFormat> list)
        {
            List<string> conList = new List<string>();

            foreach (var item in list)
            {
                // 补充邮箱
                if (string.IsNullOrWhiteSpace(item.ObservedEmail))
                    item.ObservedEmail = item.ObservedName + "@askform.cn";
                if (string.IsNullOrWhiteSpace(item.ObserverEmail))
                    item.ObserverEmail = item.ObserverName + "@askform.cn";

                // 获取属性
                foreach (var pro in item.GetType().GetProperties())
                {
                    // 判断属性是否有值
                    if (!string.IsNullOrWhiteSpace(pro.GetValue(item) + ""))
                        // 判断属性是否已存
                        if (!conList.Contains(pro.Name))
                        {
                            conList.Add(pro.Name);
                        }
                }
            }

            ToolFile.CreatFile(path, string.Join("\t", conList), false);


            foreach (var item in list)
            {
                string line = "";
                Type type = item.GetType();

                for (int i = 0; i < conList.Count; i++)
                {
                    if (i != 0)
                        line += "\t";
                    var val = type.GetProperty(conList[i]).GetValue(item) + "";
                    if (val == null)
                        val = " ";

                    line += val;
                }

                ToolFile.CreatFile(path, line, true);
            }
        }

    }
}
