/*
 * Author：步尘
 * CreatDate ：2018-10-01 09:23:37
 * CLR Version ：4.0.30319.42000
 */
using System;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using WimMain.Common;

namespace WimMain.Fun
{
    /// <summary>
    /// 所有可执行方法的父类
    /// </summary>
    public class BaseFun
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="db"></param>
        public BaseFun(DBHelper db)
        {
            DbContent = db;
        }

        /// <summary>
        /// 数据库连接对象
        /// </summary>
        public DBHelper DbContent { get; set; }

        /// <summary>
        /// 用户编号
        /// </summary>
        public string CompanyID { get; set; }

        /// <summary>
        /// 应用程序编号
        /// </summary>
        public string FormApplicationID { get; set; }

        /// <summary>
        /// 表单编号
        /// </summary>
        public string FormID { get; set; }

        /// <summary>
        /// 报告模板路径
        /// </summary>
        protected string ModelPath { get; set; } = ToolFile.GetAbsolutelyPath(ToolConfig.GetAppSetting("ModelPath"));

        /// <summary>
        /// 返回结果
        /// </summary>
        public Result Res { get; set; }

        /// <summary>
        /// 日志初始路径
        /// </summary>
        public static string LogStarPath { get { return ToolFile.GetAbsolutelyPath(ToolConfig.GetAppSetting("LogStarPath")); } }

        /// <summary>
        /// 数据文件初始路径
        /// </summary>
        public static string DataStarPath { get { return ToolFile.GetAbsolutelyPath(ToolConfig.GetAppSetting("DataStarPath")); } }

        /// <summary>
        /// 分割线
        /// </summary>
        protected string SegmentingLine { get { return "=========={0}=========="; } }

        /// <summary>
        /// 运行
        /// </summary>
        /// <param name="func"></param>
        /// <returns></returns>
        public Result RunFun(Func<string, Result> func)
        {
            Res = new Result();
            string logPath = LogStarPath + GetMethodName(2) + ".log";

            WriteLog(logPath, string.Format(SegmentingLine, "Star"));

            // 定时器
            Stopwatch watch = new Stopwatch();
            // 开始计时
            watch.Start();

            // 执行方法
            func.Invoke(logPath);

            // 停止计时
            watch.Stop();
            // 返回运行时间
            Res.RunTime = watch.ElapsedMilliseconds;

            WriteLog(logPath, string.Format(SegmentingLine, "End ") + "\t" + Res.RunTime + " ms\n");
            return Res;
        }

        /// <summary>
        /// 运行
        /// </summary>
        /// <param name="func"></param>
        /// <returns></returns>
        public Result RunFun(Func<string, string, Result> func)
        {
            Res = new Result();
            string logPath = LogStarPath + GetMethodName(2) + ".log";
            string dataPath = DataStarPath + GetMethodName(2) + "\\";

            WriteLog(logPath, string.Format(SegmentingLine, "Star"));

            // 执行方法
            Res.RunTime = this.Watch(() => func.Invoke(logPath, dataPath));

            WriteLog(logPath, string.Format(SegmentingLine, "End ") + "\t" + Res.RunTime + " ms\n");
            return Res;
        }

        /// <summary>
        /// 获得当前方法名称
        /// </summary>
        /// <param name="index">堆栈帧</param>
        /// <returns></returns>
        protected string GetMethodName(int index = 3)
        {
            StackTrace ss = new StackTrace(true);
            MethodBase mb = ss.GetFrame(index).GetMethod();
            return mb.DeclaringType.Name + "\\" + mb.Name;
        }

        /// <summary>
        /// 写入日志
        /// </summary>
        /// <param name="logPath">日志路径</param>
        /// <param name="msg">日志消息</param>
        /// <param name="append">是否追加</param>
        /// <param name="hasDate">是否包含时间</param>
        protected void WriteLog(string logPath, string msg, bool append = true, bool hasDate = true)
        {
            logPath = ToolFile.GetAbsolutelyPath(logPath);
            if (File.Exists(logPath) && !append)
                File.Delete(logPath);

            ToolFile.CreatFile(logPath, (hasDate ? (DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff")) + "\t\t" : "") + msg, true);
        }

        /// <summary>
        /// 返回参数
        /// </summary>
        /// <param name="param">额外参数</param>
        /// <returns></returns>
        protected string GetSqlParam(string param = "")
        {
            string sql = "";
            sql += "Declare @" + nameof(CompanyID) + " bigint = " + CompanyID + "\n";
            sql += "Declare @" + nameof(FormApplicationID) + " bigint = " + FormApplicationID + "\n";
            sql += "Declare @" + nameof(FormID) + " bigint = " + FormID + "\n";

            sql += param + "\n\n";
            return sql;
        }

        /// <summary>
        /// 根据FormID，返回参数
        /// </summary>
        /// <param name="formID"></param>
        /// <returns></returns>
        protected BaseFun GetParam(long formID)
        {
            string sqlStr = string.Format("SELECT CompanyID,FormApplicationID,FormID FROM AskForm_Form WHERE FormID = {0} AND IsDeleted = 0", formID);

            DataTable dt = DbContent.GetTable(sqlStr);
            if (dt.Rows.Count >= 1)
            {
                BaseFun bf = new BaseFun(null);

                bf.CompanyID = dt.Rows[0]["CompanyID"] + "";
                bf.FormApplicationID = dt.Rows[0]["FormApplicationID"] + "";
                bf.FormID = dt.Rows[0]["FormID"] + "";
                return bf;
            }
            return null;
        }

        /// <summary>
        /// 计算运算时间
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        protected long Watch(Action action)
        {
            // 定时器
            Stopwatch watch = new Stopwatch();
            // 开始计时
            watch.Start();
            action.Invoke();
            // 停止计时
            watch.Stop();
            // 返回运行时间
            return watch.ElapsedMilliseconds;
        }

    }

    /// <summary>
    /// 返回结果
    /// </summary>
    public class Result
    {
        /// <summary>
        /// 消息
        /// </summary>
        public string Msg { get; set; }

        /// <summary>
        /// 运行时间，ms
        /// </summary>
        public long RunTime { get; set; }

        /// <summary>
        /// 返回对象
        /// </summary>
        public object Object { get; set; }

    }
}