using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using WimMain.Common;
using WimMain.Models;

namespace WimMain.Fun
{
    /// <summary>
    /// Geely
    /// </summary>
    public class Geely : BaseFun
    {
        public Geely(DBHelper db) : base(db)
        {
            base.CompanyID = "270530";
            base.FormApplicationID = "7299450001";

            // 校招
            base.FormID = "7609930001";
            // 社招
            //base.FormID = "7622390001";
        }

        /// <summary>
        /// 获取所有文件
        /// </summary>
        /// <param name="dirPath">文件夹路径</param>
        /// <param name="suffix">文件后缀</param>
        /// <returns></returns>
        public Result GetDirFileName(string dirPath, string suffix)
        {
            return RunFun((logPath) =>
            {
                DirectoryInfo info = new DirectoryInfo(ToolFile.GetAbsolutelyPath(dirPath));

                if (info == null)
                {
                    Res.Msg = "文件夹不存在";
                    return Res;
                }

                if (string.IsNullOrWhiteSpace(suffix))
                    suffix = "";

                suffix = suffix.Replace(".", "").Trim().ToLower();
                string tmp = string.Format(SegmentingLine, "不包含的文件") + "\n";

                foreach (var item in info.GetFiles().OrderByDescending(c => c.FullName))
                {
                    if (string.IsNullOrWhiteSpace(suffix) || ToolFile.GetSuffix(item.FullName).ToLower() == suffix)
                        WriteLog(logPath, item.Name, true, false);
                    else
                        tmp += item.FullName + "\t" + item.CreationTime + "\n";
                }

                tmp += string.Format(SegmentingLine, "") + "\n";

                WriteLog(logPath, tmp, true, false);

                return Res;
            });
        }

        /// <summary>
        /// 移动文件
        /// </summary>
        /// <param name="logFile">日志文件</param>
        /// <param name="filePath">原始文件夹路径</param>
        /// <param name="targetPath">目标文件夹</param>
        /// <returns></returns>
        public Result MoveFile(string logFile, string filePath, string targetPath)
        {
            return RunFun((logPath) =>
            {

                List<FileInfo> fileList = new DirectoryInfo(filePath).GetFiles().ToList();

                List<string> lines = File.ReadAllLines(ToolFile.GetAbsolutelyPath(logFile)).ToList();

                foreach (var item in lines)
                {
                    FileInfo file = fileList.Where(c => item.ToLower().Contains(c.Name.ToLower())).FirstOrDefault();
                    if (file != null)
                        continue; //file.CopyTo(ToolFile.GetAbsolutelyPath(targetPath) + file.Name, true);
                    else
                        WriteLog(logPath, item, true, false);
                }

                return Res;
            });
        }

        /// <summary>
        /// 提取日志文件
        /// </summary>
        /// <param name="path">日志文件路径</param>
        /// <param name="targetPath">日志提取路径</param>
        /// <param name="str">包含的字符串</param>
        /// <returns></returns>
        public Result FilterLog(string path, string targetPath, string str)
        {
            return RunFun((logPath) =>
            {
                DirectoryInfo dirinfo = new DirectoryInfo(ToolFile.GetAbsolutelyPath(path));

                foreach (FileInfo file in dirinfo.GetFiles())
                {
                    List<string> list = File.ReadAllLines(file.FullName).ToList();
                    int count = 0;
                    foreach (var line in list)
                    {
                        if (line.ToUpper().IndexOf(str, StringComparison.InvariantCultureIgnoreCase) > 0)
                        {
                            WriteLog(ToolFile.GetAbsolutelyPath(targetPath) + "Res_" + file.Name, line, true);
                        }
                        count++;
                    }
                }
                return Res;
            });
        }

        /// <summary>
        /// 返回所有请求成功的数据
        /// </summary>
        /// <param name="date">几天之内</param>
        /// <returns></returns>
        private Result GetAllPost(int date)
        {
            return RunFun((logpath, dataPath) =>
            {
                string allSend = "AllSend.log";
                string notSend = "NotSend.json";

                List<GeelyField> notSendList;
                List<string> isSendList;

                // 读取数据库
                if (!File.Exists(dataPath + notSend))
                {
                    string sqlDeclares = GetSqlParam($"Declare @Date int = {date}");

                    notSendList = GeelyField.GetAllPost(sqlDeclares, DbContent);
                    // 初始化，将数据库中已经发送过请求的数据
                    notSendList = GeelyField.GetSendList(notSendList, c => true);
                    // 放入待重新请求文件
                    ToolFile.CreatFile(dataPath + notSend, ToolString.ConvertJson(notSendList), false);
                }
                // 读取文件
                else
                {
                    notSendList = ToolString.ConvertObject<List<GeelyField>>(File.ReadAllText(dataPath + notSend));
                }

                // 过滤已发送的数据
                if (File.Exists(dataPath + allSend))
                {
                    // 所有获取所有已发送列表
                    isSendList = File.ReadAllLines(dataPath + allSend).ToList();
                    foreach (var item in isSendList)
                    {
                        GeelyField geely = notSendList.FirstOrDefault(c => c.cand == item);
                        if (geely != null)
                            notSendList.Remove(geely);
                    }
                }

                // 已处理过的数据日志，用于更新
                Res.Msg = dataPath + allSend;
                // 待处理的数据集
                Res.Object = notSendList;

                //foreach (var item in notSendList)
                //{
                //    string url = "http://local.askform.cn/Custom/CampusRecruitment.aspx";
                //    url += "?name=" + item.firstName + "&cand=" + item.cand + "&instr=" + item.instr + "&pid=" + item.pid + "&valid=" + item.valid + "&keyword=" + item.EntryID;

                //    string log = item.cand;

                //    try
                //    {
                //        log += "\t发起请求";
                //        string res = ToolHttp.RequestUrl(url);

                //        log += "\t请求成功";
                //        ToolFile.CreatFile(dataPath + allSend, item.cand, true);
                //    }
                //    catch (Exception ex)
                //    {
                //        log += "\t请求失败：" + ex.Message;
                //    }

                //    WriteLog(logpath, log);
                //}

                return Res;
            });
        }

        /// <summary>
        /// 返回复原力数据
        /// </summary>
        /// <returns></returns>
        public Result GetResilienceData()
        {
            return RunFun(logPath =>
            {
                List<GeelyField> list = (List<GeelyField>)GetAllPost(99999).Object;

                DataTable dicTable = new DataTable();
                dicTable.Columns.Add("cand");

                #region 获取用户数据

                foreach (var item in list)
                {
                    // 获取所有数据
                    Dictionary<string, string> tmp = ToolString.GetDiction(item.achievement);

                    // 添加表头
                    dicTable.Columns.AddRange(tmp.Keys.Where(c => !dicTable.Columns.Contains(c)).Select(c => new DataColumn(c)).ToArray());
                    // 添加一行数据
                    DataRow dr = dicTable.NewRow();
                    dr["cand"] = item.cand;// 添加唯一标识

                    // 添加数据
                    foreach (var key in tmp.Keys)
                        dr[key] = tmp[key];

                    dicTable.Rows.Add(dr);
                }

                #endregion


                DataTable dt = list.ToTable();

                #region 合并数据
                List<DataColumn> columnList = new List<DataColumn>
                {
                    dt.Columns["cand"]
                };

                dt.Union(dicTable, columnList);

                #endregion

                string resilience = "resilience";

                // 计算复原力
                foreach (DataRow dr in dt.Rows)
                {
                    dr[resilience] = GeelyField.Format((decimal.Parse(dr["自信"] + "") + decimal.Parse(dr["人际敏感"] + "") + decimal.Parse(dr["坚持不懈"] + "")) * 1.3M / 3);

                    if (!(dr["result"] + "").Contains(resilience))
                    {
                        string result = dr["result"] + "";

                        result = result.Substring(0, result.Length - 2);

                        result += $",\"{resilience}\":{dr[resilience]}" + "}}";
                        dr["result"] = result;
                    }
                    else
                    {
                        WriteLog(logPath, dr["cand"] + "");
                    }

                }

                dt.Columns.Remove("EntryID");
                dt.Columns.Remove("valid");
                dt.Columns.Remove("pid");
                dt.Columns.Remove("instr");
                dt.Columns.Remove("Speed");
                dt.Columns.Remove("isDown");
                dt.Columns.Remove("postT");
                dt.Columns.Remove("IsSend");
                dt.Columns.Remove("firstName");
                dt.Columns.Remove("email");
                dt.Columns.Remove("formula");
                dt.Columns.Remove("qualified");
                dt.Columns.Remove("createDate");
                dt.Columns.Remove("JobId");
                dt.Columns.Remove("Line178");
                dt.Columns.Remove("Line177");
                dt.Columns.Remove("Line175");
                dt.Columns.Remove("Line155");
                dt.Columns.Remove("Line110");
                dt.Columns.Remove("Line66");
                dt.Columns.Remove("Line11");

                ToolFile.TableToExcel(dt, @"C:\Users\mayn\Desktop\导出\Geely.xlsx");

                return Res;
            });
        }


    }
}