using System.IO;
using System.Net;
using System.Text;

namespace WimMain.Common
{
    /// <summary>
    /// 网络请求工具
    /// </summary>
    public class ToolHttp
    {
        /// <summary>
        /// 发送网络请求
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="data">请求数据</param>
        /// <param name="method">请求模式</param>
        /// <param name="contentType">请求头</param>
        /// <returns>返回消息</returns>
        public static string RequestUrl(string url, string data = "Meaningless", string method = "Post", string contentType = "application/json;charset=UTF-8")
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = method;
            request.ContentType = contentType;

            byte[] payload = Encoding.UTF8.GetBytes(data);
            request.ContentLength = payload.Length;

            Stream writer = request.GetRequestStream();
            writer.Write(payload, 0, payload.Length);
            writer.Close();

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream s = response.GetResponseStream();

            string StrDate = "";
            string strValue = "";
            using (StreamReader Reader = new StreamReader(s, Encoding.UTF8))
            {
                while ((StrDate = Reader.ReadLine()) != null)
                {
                    strValue += StrDate + "\r\n";
                }
            }
            return strValue;
        }

    }
}
