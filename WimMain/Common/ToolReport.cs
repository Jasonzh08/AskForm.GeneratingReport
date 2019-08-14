using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Xml;
using System.Data;
using Microsoft.Reporting.WebForms;
using System.Text.RegularExpressions;

namespace WimMain.Common
{
    /// <summary>
    /// 报告工具类
    /// </summary>
    public class ToolReport
    {

        /// <summary>
        /// 绑定报告参数
        /// </summary>
        /// <param name="modePath">报告地址</param>
        /// <param name="formID">表单ID</param>
        /// <param name="isDeep">是否深入绑定参数</param>
        public static ReportParameterCollection BindPara(string modePath, long formID, bool isDeep = true)
        {
            DBHelper dB = DBHelper.GetDbContent();

            DataRow dr = dB.GetTable(string.Format("SELECT * FROM AskForm_Form WHERE FormID = '{0}'; ", formID)).Rows[0];

            ReportParameterCollection col = new ReportParameterCollection
                    {
                        new ReportParameter("CompanyID", dr["CompanyID"]+""),
                        new ReportParameter("FormApplicationID", dr["FormApplicationID"]+""),
                        new ReportParameter("FormID", dr["FormID"]+"")
            };
            if (!isDeep) return col;
            else return BindPara(modePath, col);
        }

        /// <summary>
        /// 绑定报告参数
        /// </summary>
        /// <param name="modePath">报告地址</param>
        /// <param name="col">报告参数</param>
        public static ReportParameterCollection BindPara(string modePath, ReportParameterCollection col)
        {
            if (!string.IsNullOrWhiteSpace(modePath) && File.Exists(modePath) && col != null)
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(modePath);
                XmlNamespaceManager xnm = new XmlNamespaceManager(doc.NameTable);
                xnm.AddNamespace("ns", doc.DocumentElement.NamespaceURI);

                XmlNodeList parameters = doc.SelectNodes("//ns:ReportParameters/ns:ReportParameter", xnm);
                foreach (XmlNode param in parameters)
                {
                    string name = param.Attributes["Name"].Value;
                    switch (name)
                    {
                        case "CompanyID":
                        case "FormApplicationID":
                        case "FormID":
                        case "ObservedID":
                        case "EntryID":
                        case "BU":
                        case "Department": break;
                        default:
                            col.Add(new ReportParameter(name, param.ChildNodes[1].FirstChild.InnerText));
                            break;
                    }
                }
            }
            return col;
        }

        /// <summary>
        /// Lock变量
        /// </summary>
        private static readonly object Lock_GenerateLocalReport = new object();
        /// <summary>
        /// 生成报表
        /// </summary>
        /// <param name="modelPath">模板路径</param>
        /// <param name="targetPath">目标路径</param>
        /// <param name="fileName">文件名称</param>
        /// <param name="reportParameters">报表参数</param>
        /// <param name="ignoreExists">[True:不覆盖,False:覆盖]</param>
        public static List<string> GenerateLocalReport(string modelPath, string targetPath, string fileName, ReportParameterCollection reportParameters, bool ignoreExists = true)
        {
            List<string> result = new List<string>();

            result.Add(GetRunTime("总共耗时", () =>
            {
                #region 参数处理

                Warning[] warnings;
                string[] streamIds;
                string mimeType = string.Empty;
                string encoding = string.Empty;
                string extention = string.Empty;
                string extension = string.Empty;

                // 如果不存在模板，或者文件存在且允许覆盖
                if (!File.Exists(modelPath) || (File.Exists(targetPath + fileName) && !ignoreExists))
                    return; // 已经存在则忽略，返回

                // 创建文件夹
                DirectoryInfo dir = new DirectoryInfo(targetPath);
                if (!dir.Exists)
                    dir.Create();
                // 创建新文件
                if (File.Exists(fileName))
                    File.Delete(fileName);

                DBHelper Database = DBHelper.GetDbContent();

                // 获取类型
                string reportType = fileName.Substring(fileName.LastIndexOf('.') + 1).ToUpper();
                fileName = ToolString.RemoveCharAfter(fileName, ".", true);
                switch (reportType)
                {
                    case "EXCELOPENXML":
                    case "XLSX":
                        fileName += ".xlsx"; reportType = "EXCELOPENXML"; break;
                    case "EXCEL":
                    case "XLS":
                        fileName += ".xls"; reportType = "EXCEL"; break;
                    case "WORDOPENXML":
                    case "DOCX":
                        fileName += ".docx"; reportType = "WORDOPENXML"; break;
                    case "WORD":
                    case "DOC":
                        fileName += ".doc"; reportType = "WORD"; break;
                    case "PDF":
                        fileName += ".pdf"; break;
                    case "IMAGE":
                    case "TIF":
                        fileName += ".tif"; reportType = "IMAGE"; break;
                }

                // 拼装完整路径
                fileName = dir.FullName + fileName;

                #endregion

                ReportViewer viewer = new ReportViewer();
                viewer.ProcessingMode = ProcessingMode.Local;

                // 初始化报告参数
                ReportParameterCollection col = new ReportParameterCollection();
                // 加上线程锁
                lock (Lock_GenerateLocalReport)
                {
                    // 加载报告模板，XML格式
                    XmlDocument doc = new XmlDocument();
                    doc.Load(modelPath);
                    XmlNamespaceManager xnm = new XmlNamespaceManager(doc.NameTable);
                    xnm.AddNamespace("ns", doc.DocumentElement.NamespaceURI);

                    #region 添加报告参数

                    XmlNodeList parameters = doc.SelectNodes("//ns:ReportParameters/ns:ReportParameter", xnm);
                    foreach (XmlNode param in parameters)
                    {
                        string name = param.Attributes["Name"].Value;

                        result.Add(GetRunTime("添加参数，" + name, () =>
                        {
                            XmlNode defaultValueNode = param.SelectSingleNode(".//ns:DefaultValue/ns:Values/ns:Value", xnm);
                            ReportParameter parameter = new ReportParameter();
                            parameter.Name = name;
                            if (defaultValueNode != null)
                            {
                                parameter.Values.Add(defaultValueNode.InnerText);
                            }
                            if (reportParameters[name] != null && reportParameters[name].Values.Count > 0)
                            {
                                if (parameter.Values.Count > 0)
                                {
                                    parameter.Values[0] = reportParameters[name].Values[0];
                                }
                                else
                                {
                                    parameter.Values.Add(reportParameters[name].Values[0]);
                                }
                            }
                            col.Add(parameter);
                        }));
                    }

                    result.Add("");

                    #endregion

                    #region 加载报告数据

                    // 清理报告数据
                    viewer.LocalReport.DataSources.Clear();

                    XmlNodeList datasets = doc.SelectNodes("//ns:DataSets/ns:DataSet", xnm);

                    bool isExists = false;

                    foreach (XmlNode ds in datasets)
                    {
                        string name = ds.Attributes["Name"].Value;

                        result.Add(GetRunTime("加载数据，" + name, () =>
                        {
                            string sql = ds.SelectSingleNode(".//ns:CommandText", xnm).InnerText;
                            foreach (XmlNode node in parameters)
                            {
                                isExists = false;

                                foreach (ReportParameter p in col)
                                {
                                    if (p.Name == node.Attributes["Name"].Value)
                                    {
                                        Regex reg = new Regex("@" + p.Name + "\b");

                                        if (node.SelectSingleNode("./ns:DataType", xnm).InnerText == "String")
                                        {
                                            sql = reg.Replace(sql, "'" + p.Values[0] + "'");
                                        }
                                        else
                                        {
                                            sql = reg.Replace(sql, p.Values[0]);
                                        }
                                        isExists = true;
                                        break;
                                    }
                                }
                                if (!isExists)
                                {
                                    Regex reg = new Regex("@" + node.Attributes["Name"].Value + "\b");
                                    sql = reg.Replace(sql, node.SelectSingleNode("./ns:DefaultValue/ns:Values/ns:Value", xnm).InnerText);
                                }
                            }
                            viewer.LocalReport.DataSources.Add(new ReportDataSource(name, Database.GetTable(sql)));
                        }));
                    }

                    result.Add("");

                    #endregion

                    #region 创建报告

                    result.Add(GetRunTime("创建报告", () =>
                    {
                        using (MemoryStream stream = new MemoryStream())
                        {
                            doc.Save(stream);

                            stream.Seek(0, SeekOrigin.Begin);
                            viewer.LocalReport.EnableExternalImages = true;
                            viewer.LocalReport.LoadReportDefinition(stream);

                            viewer.LocalReport.SetParameters(col);

                            string ss = string.Join(",", viewer.LocalReport.ListRenderingExtensions().ToList());

                            byte[] bytes = viewer.LocalReport.Render(reportType, null, out mimeType, out encoding, out extension, out streamIds, out warnings);

                            using (FileStream fs = new FileStream(fileName, FileMode.Create))
                            {
                                fs.Write(bytes, 0, bytes.Length);
                            }
                        }
                    }));

                    #endregion
                }

                GC.Collect();
            }));
            return result;
        }

        /// <summary>
        /// 返回程序运行时间
        /// </summary>
        /// <param name="remark"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        private static string GetRunTime(string remark, Action action)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            action.Invoke();
            sw.Stop();
            return remark + "：\t" + sw.ElapsedMilliseconds + " ms";
        }

    }
}
