using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using WimMain.Common;

namespace WimMain.Fun
{
    /// <summary>
    /// 报告
    /// </summary>
    public class ReportTest : BaseFun
    {
        public ReportTest(DBHelper db) : base(db) { }

        #region 辅助方法

        /// <summary>
        /// 返回被评估人列表
        /// </summary>
        /// <returns></returns>
        public string GetObservedList()
        {
            return @"
                        Select e.EntryID,ec.Value as 'FullName' 
                        From AskForm_Entry e
                        INNER JOIN AskForm_EntryText ec ON e.EntryID =ec.EntryID
                        INNER JOIN AskForm_Field f ON ec.FieldID=f.FieldID And f.Name='FullName'
                        INNER JOIN AskForm_Form form ON ec.FormID=form.FormID

                        Where form.CompanyID='{0}' And ec.IsDeleted=0 and
                        form.FormApplicationID='{1}' And form.Name='Observed'; ";
        }

        /// <summary>
        /// 返回信息
        /// </summary>
        /// <param name="formId">表单ID</param>
        /// <returns></returns>
        public DataRow GetFormInfo(long formId)
        {
            return DbContent.GetTable(string.Format("SELECT * FROM AskForm_Form WHERE FormID = '{0}'; ", formId)).Rows[0];
        }



        /// <summary>
        /// 返回信息
        /// </summary>
        /// <param name="entryId"></param>
        /// <returns></returns>
        public DataRow GetEntryInfo(string entryId)
        {
            return DbContent.GetTable(string.Format(@"
                        SELECT ct.* 
                        ,et.CreatedDate
                        INTO #dt
                        FROM AskForm_EntryContent ct
                            INNER JOIN AskForm_Entry et ON et.EntryID = ct.EntryID AND et.IsDeleted = 0
                        WHERE ct.EntryID = {0}

                        SELECT distinct FieldID ,FieldTitle ,FieldName ,Position
                        INTO #Fie
                        FROM #dt
                        ORDER BY Position

                        DECLARE @sql nvarchar(2000) = ' SELECT t.EntryID,Min(t.CreatedDate)  AS ''CreatedDate'' '
                        DECLARE @FieldID nvarchar(200)
                        DECLARE @Name nvarchar(200)

                        WHILE EXISTS(SELECT *
                        FROM #Fie)
                         BEGIN
                            SELECT @FieldID= FieldID ,@Name= FieldName
                            FROM #Fie;
                            SET @sql +=',(SELECT [Value] FROM #dt WHERE EntryID = t.EntryID AND FieldID = '+@FieldID+' ) AS '''+@Name+''' '
                            DELETE FROM #Fie WHERE FieldID=@FieldID AND FieldName = @Name;
                        END

                        SET @sql +='FROM #dt t GROUP BY t.EntryID,t.CreatedDate ORDER BY t.CreatedDate'

                        exec sp_executesql @sql

                        DROP TABLE #dt,#Fie", entryId)).Rows[0];
        }

        #endregion

        #region 报告生成

        /// <summary>
        /// 生成报告
        /// </summary>
        /// <param name="modelPath">报告模板</param>
        /// <param name="tigerPath">生成地址</param>
        /// <param name="formId">问卷编号</param>
        /// <param name="entryId">被评估人编号</param>
        /// <param name="suffix">报告格式</param>
        /// <param name="maxLength">最多生成报告数，0为不限制</param>
        /// <returns></returns>
        public Result CreateReport(string modelPath, string tigerPath, long formId, long entryId, string suffix, int maxLength)
        {
            return RunFun(logPath =>
            {
                tigerPath = ToolFile.GetAbsolutelyPath(tigerPath);

                modelPath = base.ModelPath + modelPath;

                DataRow dr = GetFormInfo(formId);

                if (entryId > 0)
                {
                    ReportParameterCollection col = new ReportParameterCollection
                    {
                        new ReportParameter("CompanyID", dr["CompanyID"]+""),
                        new ReportParameter("FormApplicationID", dr["FormApplicationID"]+""),
                        new ReportParameter("FormID", dr["FormID"]+""),
                        new ReportParameter("ObservedID", entryId+""),
                        new ReportParameter("EntryID", entryId+""),
                        new ReportParameter("MinValue", "1")
                    };

                    col = ToolReport.BindPara(modelPath, col);

                    ToolReport.GenerateLocalReport(modelPath, tigerPath, entryId + "." + suffix, col, false);
                }
                else
                {
                    DataTable dt = DbContent.GetTable(string.Format(GetObservedList(), dr["CompanyID"] + "", dr["FormApplicationID"] + ""));

                    int con = 0;

                    foreach (DataRow item in dt.Rows)
                    {
                        if (maxLength > 0 && con >= maxLength)
                            break;

                        ReportParameterCollection col = new ReportParameterCollection
                            {
                                new ReportParameter("CompanyID", dr["CompanyID"]+""),
                                new ReportParameter("FormApplicationID", dr["FormApplicationID"]+""),
                                new ReportParameter("FormID", dr["FormID"]+""),
                                new ReportParameter("ObservedID", item["EntryID"]+""),
                                new ReportParameter("EntryID", item["EntryID"]+""),
                                new ReportParameter("MinValue", "1")
                            };

                        col = ToolReport.BindPara(modelPath, col);
                        con++;
                        WriteLog(logPath, dt.Rows.IndexOf(item) + "\t" + item["FullName"] + "_" + item["EntryID"]);
                        ToolReport.GenerateLocalReport(modelPath, tigerPath, item["FullName"] + "_" + item["EntryID"] + "." + suffix, col, false);
                    }

                }
                return Res;
            });
        }

        /// <summary>
        /// 生成团队报告
        /// </summary>
        /// <param name="modelPath">报告模板</param>
        /// <param name="tigerPath">生成地址</param>
        /// <param name="formId">问卷编号</param>
        /// <param name="suffix">报告格式</param>
        /// <returns></returns>
        public Result CreateTeamReport(string modelPath, string tigerPath, long formId, string suffix)
        {
            return RunFun(logPath =>
            {
                tigerPath = ToolFile.GetAbsolutelyPath(tigerPath);

                modelPath = base.ModelPath + modelPath;

                string sqlFilter = $"SELECT * FROM AskForm_FormFilter  ff WHERE ff.FormID = {formId} AND ff.Name LIKE '%FieldFilter%' AND ff.IsDeleted = 0";

                DataTable dtFilter = DbContent.GetTable(sqlFilter);

                // 循环过滤条件
                foreach (DataRow filter in dtFilter.Rows)
                {
                    ReportParameterCollection col = new ReportParameterCollection
                    {
                                new ReportParameter("CompanyID", filter["CompanyID"]+""),
                                new ReportParameter("FormApplicationID", filter["FormApplicationID"]+""),
                                new ReportParameter("FormID", filter["FormID"]+""),
                                new ReportParameter("FormFilterID", filter["FormFilterID"]+""),
                                new ReportParameter("MinValue", "1")
                     };

                    col = ToolReport.BindPara(modelPath, col);

                    // 获取过滤字段
                    string sqlField = $"SELECT f.FieldID,f.Title,fff.Content FROM AskForm_FormFilterField fff INNER JOIN AskForm_Field f ON f.FieldID = fff.FieldID WHERE FormFilterID = {filter["FormFilterID"]} AND fff.IsDeleted = 0";
                    DataTable dtField = DbContent.GetTable(sqlField);

                    // 循环字段
                    foreach (DataRow field in dtField.Rows)
                    {
                        string sqlContent = $"SELECT distinct [Value] FROM AskForm_EntryText et WHERE et.FieldID = {field["FieldID"]} AND et.IsDeleted = 0";
                        DataTable dtContent = DbContent.GetTable(sqlContent);

                        // 循环内容
                        foreach (DataRow content in dtContent.Rows)
                        {
                            // 修改内容
                            string sqlUpFilterField = $"UPDATE AskForm_FormFilterField SET Content='{content["Value"]}' WHERE FormFilterID = {filter["FormFilterID"]} AND FieldID ={field["FieldID"]} ";

                            WriteLog(logPath, "修改【" + field["Title"] + "】为【" + content["Value"] + "】，" + DbContent.ExecuteNonQuery(sqlUpFilterField));
                            ToolReport.GenerateLocalReport(modelPath, tigerPath, field["Title"] + "_" + content["Value"] + "." + suffix, col, false);
                        }
                    }
                }
                return Res;
            });
        }

        #endregion

        #region 报告操作

        /// <summary>
        /// 检查报告是否全部生成
        /// </summary>
        /// <param name="tigerPath"></param>
        /// <param name="formId"></param>
        /// <returns></returns>
        public Result CheckReportIsAll(string tigerPath, long formId)
        {
            return RunFun(logPath =>
            {
                tigerPath = ToolFile.GetAbsolutelyPath(tigerPath);

                List<FileInfo> list = new DirectoryInfo(tigerPath).GetFiles().ToList();

                DataRow dr = GetFormInfo(formId);

                DataTable dt = DbContent.GetTable(string.Format(GetObservedList(), dr["CompanyID"] + "", dr["FormApplicationID"] + ""));

                WriteLog(logPath, "共" + dt.Rows.Count + "人");

                foreach (DataRow item in dt.Rows)
                {
                    string name = item["FullName"] + "_" + item["EntryID"];
                    if (!list.Exists(c => c.Name.Contains(name)))
                        WriteLog(logPath, name);
                }
                return Res;
            });

        }

        /// <summary>
        /// 报告分组
        /// </summary>
        /// <param name="tigerPath">文件夹</param>
        /// <param name="group">分组条件，多个用','分割</param>
        /// <param name="max">文件夹最大占用</param>
        /// <returns></returns>
        public Result ReportGroup(string tigerPath, string group, double max)
        {
            return RunFun(logpath =>
            {
                string res = "分组" + DateTime.Now.Millisecond + "\\";
                tigerPath = ToolFile.GetAbsolutelyPath(tigerPath);

                DirectoryInfo dir = new DirectoryInfo(tigerPath);

                // 移除所有文件夹
                while (dir.GetDirectories().Length > 0)
                    Directory.Delete(dir.GetDirectories()[0].FullName, true);

                List<string> groupList = group.Split(',').Where(c => !string.IsNullOrWhiteSpace(c)).ToList();

                Dictionary<string, long> dic = new Dictionary<string, long>();

                List<Task> taskList = new List<Task>();

                for (int i = 0; i < dir.GetFiles().Length; i++)
                {
                    WriteLog(logpath, i + "");

                    string path = tigerPath + res;
                    FileInfo field = dir.GetFiles()[i];

                    List<string> nameList = field.Name.Replace("." + ToolFile.GetSuffix(field.Name), "").Split('_').Where(c => !string.IsNullOrWhiteSpace(c)).ToList();

                    if (nameList.Count != 2)
                    {
                        WriteLog(logpath, field.Name);
                        continue;
                    }
                    DataRow dr = GetEntryInfo(nameList[1]);

                    foreach (var item in groupList)
                        if (dr.Table.Columns.Contains(item))
                            path += dr[item] + "\\";

                    if (!Directory.Exists(path))
                        Directory.CreateDirectory(path);
                    else
                    {
                        if (dic.Keys.Contains(path) && max > 0)
                        {
                            double memory = dic[path] * 1.0;
                            if (max * 1024 * 1024 < memory + field.Length)
                            {
                                path = ToolString.RemoveCharAfter(path, "\\", true) + "(" + dic.Keys.Where(c => c.Contains(path)).Count() + ")\\";
                                Directory.CreateDirectory(path);
                            }
                        }
                    }

                    if (dic.Keys.Contains(path))
                        dic[path] += field.Length;
                    else
                        dic.Add(path, field.Length);


                    taskList.Add(Task.Run(() =>
                    {
                        File.Copy(field.FullName, path + field.Name, true);
                    }));
                }

                Task.WaitAll(taskList.ToArray());

                dir = new DirectoryInfo(tigerPath + res);

                foreach (var key in dic.Keys)
                    WriteLog(logpath, (dic[key] * 1.0 / 1020 / 1024) + "M\t" + key);

                return Res;
            });
        }

        #endregion

        #region 报告数据

        /// <summary>
        /// 报告数据导入数据库
        /// </summary>
        /// <param name="dirPath"></param>
        /// <param name="formId"></param>
        /// <param name="reportId"></param>
        /// <returns></returns>
        public Result AddReportItem(string dirPath= "D:\\Shared\\弘阳报告", long formId= 8374570001, long reportId= 15581150001)
        {
            return RunFun(logpath =>
            {
                string path = "\\Survey\\ExportReports\\";// 根目录
                // 获取表单GUID并拼接路径
                string sqlStr = "SELECT * FROM AskForm_Form WHERE FormID = " + formId;
                DataTable dt = DbContent.GetTable(sqlStr);
                string companyId = "";
                string appId = "";

                if (dt.HasItems(c => c.Columns.Contains("FormGuid")))
                {
                    path += dt.Rows[0]["FormGuid"] + "\\" + reportId + "\\";
                    companyId = dt.Rows[0]["CompanyID"] + "";
                    appId = dt.Rows[0]["FormApplicationID"] + "";
                }
                else
                {
                    return Res;
                }
                // 获取文件夹下所有文件
                if (Directory.Exists(dirPath))
                {
                    string[] pdfArr = Directory.GetFiles(dirPath);

                    foreach (var pdf in pdfArr)
                    {
                        string fileName = ToolFile.GetFileName(pdf,false);
                        string name = ToolFile.GetFileName(pdf);

                        sqlStr = string.Format(@"INSERT AskForm_FormReportItem SELECT {0},{1},'{2}','','{3}',0,{4},{5},{6},0,GETDATE(),GETDATE()"
                        , reportId, ToolString.GetRandomStr(12, 6), name, path + fileName, companyId, appId, formId);
                        DbContent.ExecuteNonQuery(sqlStr);
                        WriteLog(logpath, sqlStr);
                    }
                }
                return Res;
            });
        }


        #endregion
    }
}