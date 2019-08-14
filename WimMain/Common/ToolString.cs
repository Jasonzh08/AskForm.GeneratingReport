/*
 * Author：步尘
 * CreatDate ：2018-10-24 15:20:56
 * CLR Version ：4.0.30319.42000
 */
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WimMain.Common
{
    /// <summary>
    /// 字符串操作
    /// </summary>
    public class ToolString
    {
        /// <summary>
        /// 移除指定字符串后的字符串
        /// </summary>
        /// <param name="str">原字符串</param>
        /// <param name="ch">节点字符串</param>
        /// <param name="isContain">返回字符串中，是否包含节点字符串</param>
        /// <returns></returns>
        public static string RemoveCharAfter(string str, string ch, bool isContain = false)
        {
            if (string.IsNullOrWhiteSpace(str) || !str.Contains(ch))
                return str;

            int index = str.LastIndexOf(ch);

            if (isContain)
                return str.Substring(0, index + ch.Length - 1);
            return str.Substring(0, index);
        }

        /// <summary>
        /// 字符串补长
        /// </summary>
        /// <param name="str"></param>
        /// <param name="ch"></param>
        /// <param name="length"></param>
        /// <param name="isAppend">是否向后追加[true:在字符串后追加，false:在字符串前追加]</param>
        /// <returns></returns>
        public static string AddStrByLength(string str, string ch, int length, bool isAppend = true)
        {
            if (string.IsNullOrWhiteSpace(str))
                str = string.Empty;

            while (str.Length < length)
            {
                if (isAppend) str += ch;
                else str = ch + str;
            }
            return str;
        }

        /// <summary>
        /// 返回文本中的英文字母
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string GetLetter(string str)
        {
            string res = "";

            for (int i = 0; i < str.Length; i++)
            {
                if (('a' <= str[i] && str[i] <= 'z') || ('A' <= str[i] && str[i] <= 'Z'))
                    res += str[i];
            }

            return res;
        }

        /// <summary>
        /// 返回随机字符串
        /// </summary>
        /// <param name="maxLen">长度</param>
        /// <param name="format">格式{ 0:混合,1:混合大写,2:混合小写,3: 英文混合,4:英文大写,5:英文小写,6:数字}</param>
        /// <returns></returns>
        public static string GetRandomStr(int maxLen, int format = 0)
        {
            // 待返回的字符串
            string resStr = "";

            string regStr = ".*";

            switch (format)
            {
                case 1:// 混合大写
                    regStr = "[A-Z0-9]*";
                    break;
                case 2:// 混合小写
                    regStr = "[a-z0-9]*";
                    break;
                case 3:// 英文混合
                    regStr = "[a-zA-Z]*";
                    break;
                case 4:// 英文大写
                    regStr = "[A-Z]*";
                    break;
                case 5:// 英文小写
                    regStr = "[a-z]*";
                    break;
                case 6:// 数字
                    regStr = "[0-9]*";
                    break;
                default:
                    regStr = "[a-zA-Z0-9]*";
                    break;
            }

            // 当ResStr的长度正好为MaxLen时，终止循环
            while (resStr.Length != maxLen)
            {
                // 获得36位Guid字符串，包含4个字符，"-"
                string tempStr = Guid.NewGuid().ToString();

                // 根据正则获取数据
                tempStr = ToolRegular.GetPoint(tempStr, regStr).FirstOrDefault() + "";

                // 将TempStr拼接至ResStr
                resStr += tempStr;
                // 若ResStr的长度超过MaxLen，对其进行截取
                if (resStr.Length > maxLen)
                    resStr = resStr.Substring(0, maxLen);
                // 若ResStr的长度少于MaxLen，会继续拼接
            }
            // 将此字符串返回
            return resStr;
        }

        #region Json

        /// <summary>
        /// 对象转Json
        /// </summary>
        /// <param name="obj">对象</param>
        /// <param name="hasNull">是否派出null值</param>
        /// <returns></returns>
        public static string ConvertJson(object obj, bool hasNull = false)
        {
            JsonSerializerSettings set;
            if (hasNull)
                set = new JsonSerializerSettings();
            else
                set = new JsonSerializerSettings()
                {
                    NullValueHandling = NullValueHandling.Ignore
                };

            return ConvertJsonString(obj == null ? null : obj is string ? obj.ToString() : JsonConvert.SerializeObject(obj, set));
        }

        /// <summary>
        /// Json转对象
        /// </summary>
        /// <param name="json"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T ConvertObject<T>(string json)
        {
            return json == null ? default(T) : JsonConvert.DeserializeObject<T>(json);
        }

        /// <summary>
        /// 格式化Json
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string ConvertJsonString(string str)
        {
            if (string.IsNullOrWhiteSpace(str)) return null;
            // 格式化json字符串 
            JsonSerializer serializer = new JsonSerializer();
            TextReader tr = new StringReader(str);
            JsonTextReader jtr = new JsonTextReader(tr);

            object obj = serializer.Deserialize(jtr);
            if (obj != null)
            {
                StringWriter textWriter = new StringWriter();
                JsonTextWriter jsonWriter = new JsonTextWriter(textWriter)
                {
                    Formatting = Newtonsoft.Json.Formatting.Indented,
                    Indentation = 4,
                    IndentChar = ' '
                }
                ;
                serializer.Serialize(jsonWriter, obj);
                return textWriter.ToString();
            }
            else
            {
                return str;
            }
        }

        /// <summary>
        /// json转键值对
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static Dictionary<string, string> GetDiction(string json)
        {

            List<string> list = ToolRegular.GetPoint(json, "\".*?\":.*?(,|})").Select(c => c.Substring(0, c.Length - 1)).ToList();
            Dictionary<string, string> dic = new Dictionary<string, string>();

            foreach (var item in list)
            {
                string[] kvArr = item.Split(':').Select(c => c.Replace("\"", "")).ToArray();
                if (kvArr.Length != 2)
                {
                    throw new Exception("键值对有误");
                }
                dic.Add(kvArr[0], kvArr[1]);
            }

            return dic;
        }

        #endregion

    }
}
