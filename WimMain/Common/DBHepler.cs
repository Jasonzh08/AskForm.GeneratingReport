using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace WimMain.Common
{
    /// <summary>
    /// 数据库操作
    /// </summary>
    public class DBHelper
    {
        /// <summary>
        /// 数据库连接对象
        /// </summary>
        private static DBHelper DbContent { get; set; }
        
        /// <summary>
        /// 数据库连接
        /// </summary>
        /// <param name="conStr">链接字符串</param>
        private DBHelper(string conStr)
        {
            ConStr = conStr;
            Conn = new SqlConnection(ConStr);
        }

        /// <summary>
        /// 数据库连接
        /// </summary>
        private DBHelper()
        {
            Conn = new SqlConnection(ConStr);
        }

        /// <summary>
        /// 链接字符串，读配置文件
        /// </summary>
        private string ConStr
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_conStr))
                    _conStr = ConfigurationManager.ConnectionStrings[0].ConnectionString;
                return _conStr;
            }
            set { _conStr = value; }
        }
        private string _conStr;

        /// <summary>
        /// 连接对象
        /// </summary>
        private SqlConnection Conn = new SqlConnection();

        /// <summary>
        /// 返回连接对象
        /// </summary>
        /// <returns></returns>
        public static DBHelper GetDbContent()
        {
            if (DbContent == null)
                return DbContent = new DBHelper();
            return DbContent;
        }

        /// <summary>
        /// 返回连接对象
        /// </summary>
        /// <param name="conStr">链接字符串</param>
        /// <returns></returns>
        public static DBHelper GetDbContent(string conStr)
        {
            return DbContent = new DBHelper(conStr);
        }


        /// <summary>
        /// 执行SQL
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="sqlStr">SQL语句</param>
        /// <param name="func">cmd操作</param>
        /// <param name="type">执行的SQL类型</param>
        /// <returns></returns>
        private TResult ExecSql<TResult>(string sqlStr, Func<SqlCommand, TResult> func, CommandType type)
        {
            Conn.Close();
            Conn.Open();

            // 此处不做异常处理，向上层抛出错误
            SqlCommand cmd = new SqlCommand(sqlStr, Conn);
            cmd.CommandType = type;
            cmd.CommandTimeout = int.MaxValue;

            TResult resule = func.Invoke(cmd);

            Conn.Close();
            return resule;
        }

        /// <summary>
        /// 执行增删改
        /// </summary>
        /// <param name="sqlStr">SQL语句</param>
        /// <param name="type">执行的SQL类型</param>
        /// <returns></returns>
        public int ExecuteNonQuery(string sqlStr, CommandType type = CommandType.Text)
        {
            return ExecSql(sqlStr, (cmd) => cmd.ExecuteNonQuery(), type);
        }

        /// <summary>
        /// 执行查询
        /// </summary>
        /// <param name="sqlStr">SQL语句</param>
        /// <param name="type">执行的SQL类型</param>
        /// <returns></returns>
        public DataSet GetDataSet(string sqlStr, CommandType type = CommandType.Text)
        {
            return ExecSql(sqlStr, (cmd) =>
            {
                SqlDataAdapter dap = new SqlDataAdapter(cmd);

                DataSet ds = new DataSet();
                dap.Fill(ds);
                return ds;

            }, type);
        }

        /// <summary>
        /// 执行查询
        /// </summary>
        /// <param name="sqlStr">SQL语句</param>
        /// <param name="index">表索引</param>
        /// <param name="type">执行的SQL类型</param>
        /// <returns></returns>
        public DataTable GetTable(string sqlStr, int index = 0, CommandType type = CommandType.Text)
        {
            return GetDataSet(sqlStr, type).Tables[index];
        }

        /// <summary>
        /// 执行查询
        /// </summary>
        /// <param name="sqlStr">SQL语句</param>
        /// <param name="index">表索引</param>
        /// <param name="func">拓展</param>
        /// <param name="type">执行的SQL类型</param>
        /// <returns></returns>
        public List<T> GetTable<T>(string sqlStr, int index = 0, Func<DataColumn, string> func = null, CommandType type = CommandType.Text)
            where T : class, new()
        {
            return GetTable(sqlStr, index, type).ToList<T>(func);
        }

        /// <summary>
        /// SQL语句在事务中执行
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        protected string GetTranSql(string sql)
        {
            if (string.IsNullOrWhiteSpace(sql)) return "";

            StringBuilder bulider = new StringBuilder();

            bulider.AppendLine("BEGIN TRANSACTION \n");
            bulider.Append(sql);
            bulider.AppendLine("\nIF(@@Error<=0)");
            bulider.AppendLine("    Commit;");
            bulider.AppendLine("ELSE");
            bulider.AppendLine("    RollBack;");
            //bulider.AppendLine("GO");// 不可加GO，有特殊字符

            return bulider.ToString();
        }

    }
}