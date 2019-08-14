/*
 * Author：步尘
 * CreatDate ：2018-10-03 10:29:33
 * CLR Version ：4.0.30319.42000
 */
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace WimMain.Common
{
    /// <summary>
    /// 正则表达式
    /// </summary>
    public class ToolRegular
    {
        #region 正则表达式

        /// <summary>
        /// 是否是绝对路径
        /// </summary>
        public static string _RIsAbsolutelyPath { get {return @"[a-zA-Z]{1}:\.*"; } }
        
        #endregion

        /// <summary>
        /// 是否满足正则
        /// </summary>
        /// <param name="content">内容</param>
        /// <param name="regStr">正则</param>
        /// <returns></returns>
        public static bool Contains(string content, string regStr)
        {
            Regex reg = new Regex(regStr);
            return reg.IsMatch(content);
        }


        /// <summary>
        /// 返回所有匹配的字符串
        /// </summary>
        /// <param name="con">原字符串</param>
        /// <param name="regex">正则表达式</param>
        /// <param name="option">正则匹配项</param>
        /// <returns></returns>
        public static List<string> GetPoint(string con, string regx, object index = null, RegexOptions option = RegexOptions.IgnoreCase)
        {
            List<string> resList = new List<string>();

            if (!string.IsNullOrWhiteSpace(con) && !string.IsNullOrWhiteSpace(regx))
            {
                if (Regex.IsMatch(con, regx, option))
                {
                    MatchCollection matchCol = Regex.Matches(con, regx, option);

                    if (matchCol.Count > 0)
                    {
                        for (int i = 0; i < matchCol.Count; i++)
                        {
                            if (index is int)
                                resList.Add(matchCol[i].Groups[int.Parse(index + "")].Value);
                            else if (index is string)
                                resList.Add(matchCol[i].Groups[index + ""].Value);
                            else
                                resList.Add(matchCol[i].Value);
                        }

                    }
                }
            }
            return resList;
        }

    }
}
