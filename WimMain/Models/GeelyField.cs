using System;
using System.Collections.Generic;
using System.Linq;
using WimMain.Common;

namespace WimMain.Models
{
    /// <summary>
    /// Geely
    /// </summary>
    public class GeelyField
    {
        public string EntryID { get; set; }

        /// <summary>
        /// 用户标识
        /// </summary>
        public string cand { get; set; }
        /// <summary>
        /// 工具ID
        /// </summary>
        public string instr { get; set; }
        /// <summary>
        /// 项目ID
        /// </summary>
        public string pid { get; set; }
        /// <summary>
        /// 是否完成
        /// </summary>
        public string valid { get; set; }
        /// <summary>
        /// 完成度
        /// </summary>
        public string Speed { get; set; }
        /// <summary>
        /// WebService状态
        /// </summary>
        public string isDown { get; set; }
        /// <summary>
        /// 请求类型
        /// </summary>
        public string postT { get; set; }
        /// <summary>
        /// 请求状态
        /// </summary>
        public string IsSend { get; set; }
        /// <summary>
        /// 返回结果
        /// </summary>
        public string result { get; set; }
        /// <summary>
        /// 用户名称
        /// </summary>
        public string firstName { get; set; }
        /// <summary>
        /// 用户邮箱
        /// </summary>
        public string email { get; set; }

        /// <summary>
        /// 2019-1-24，复原力
        /// </summary>
        public string resilience { get; set; }

        /// <summary>
        /// 用户得分
        /// </summary>
        public string achievement { get; set; }

        /// <summary>
        /// 公式得分
        /// </summary>
        public string formula { get; set; }
        /// <summary>
        /// 胜任力得分
        /// </summary>
        public string qualified { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public string createDate { get; set; }
        /// <summary>
        /// 岗位ID，社招使用
        /// </summary>
        public string JobId { get; set; }

        public static List<GeelyField> GetAllPost(string declares, DBHelper DbContent)
        {
            string sql = declares + @"
                    -- 查询指定天数的数据
                    SELECT et.EntryID ,e.CreatedDate ,et.FieldID ,fi.Title ,fi.Name,fi.Position ,et.[Value]
                    INTO #dt
                    FROM AskForm_EntryText et
                        INNER JOIN AskForm_Field fi ON fi.FieldID = et.FieldID
                        INNER JOIN AskForm_Entry e ON e.EntryID = et.EntryID
                    WHERE et.CompanyID = @CompanyID
                        AND et.FormApplicationID = @FormApplicationId
                        AND et.FormID = @FormID
                        AND et.IsDeleted = 0
                        AND fi.IsDeleted = 0
                        AND e.IsDeleted = 0
                        AND DateDiff(dd,e.CreatedDate,GETDATE())<=@Date
                    ORDER BY et.EntryID

                    -- 获得所有字段
                    SELECT distinct FieldID ,Title,Name,Position
                    INTO #Fie
                    FROM #dt
                    ORDER BY Position

                    DECLARE @sql nvarchar(2000) = ' SELECT t.EntryID '
                    DECLARE @FieldID nvarchar(200)
                    DECLARE @Name nvarchar(200)

                    -- 行转列，拼接SQL
                    WHILE EXISTS(SELECT * FROM #Fie)
                     BEGIN
                        SELECT @FieldID= FieldID ,@Name= Name
                        FROM #Fie;
                        SET @sql +=',(SELECT [Value] FROM #dt WHERE EntryID = t.EntryID AND FieldID = '+@FieldID+' ) AS '''+@Name+''' '
                        DELETE FROM #Fie WHERE FieldID=@FieldID AND Name = @Name;
                    END

                    SET @sql +='FROM #dt t GROUP BY t.EntryID,t.CreatedDate ORDER BY t.CreatedDate'

                    -- 执行SQL
                    exec sp_executesql @sql

                    DROP TABLE #dt,#Fie";

            return DbContent.GetTable(sql).ToList<GeelyField>((c) => c.ColumnName.Replace("bc-", ""));

        }


        /// <summary>
        /// 返回未发送的数据
        /// </summary>
        /// <param name="list"></param>
        /// <param name="where"></param>
        /// <returns></returns>
        private List<GeelyField> GetNotSendList(List<GeelyField> list, Func<GeelyField, bool> where)
        {
            // 获得所有API调用成功的用户名称
            var candList = list.Where(c => c.IsSend == "API调用成功").Select(c => c.cand).Distinct().ToList();

            var tmpList = list.Where(c => true).ToList();

            // 移除掉所有调用成功的用户
            tmpList.RemoveAll(c => candList.Contains(c.cand));

            // 过滤定时控制请求的用户
            tmpList = tmpList.Where(c => c.postT == "用户请求").ToList();

            // 执行条件
            tmpList = tmpList.Where(where).ToList();

            return Distinct(tmpList);
        }

        /// <summary>
        /// 返回已发送数据
        /// </summary>
        /// <param name="list"></param>
        /// <param name="where"></param>
        /// <returns></returns>
        public static List<GeelyField> GetSendList(List<GeelyField> list, Func<GeelyField, bool> where = null)
        {
            return Distinct(list.Where(c => c.IsSend == "API调用成功" && (where == null || where.Invoke(c))).Distinct().ToList());
        }

        /// <summary>
        /// 去掉重复值
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static List<GeelyField> Distinct(List<GeelyField> list)
        {
            List<GeelyField> resList = new List<GeelyField>();

            // 获得所有人姓名，不重复
            List<string> candList = list.Select(c => c.cand).Distinct().ToList();

            // 循环姓名
            foreach (var cand in candList)
            {
                // 寻找完成度最高的一次记录
                string speed = list.Where(c => c.cand == cand).Select(c => int.Parse(c.Speed)).Max() + "";

                GeelyField geely = list.LastOrDefault(c => c.cand == cand && c.Speed == speed);
                if (!resList.Contains(geely))
                    resList.Add(geely);
            }

            return resList;
        }

        /// <summary>
        /// 数据五分制
        /// </summary>
        /// <param name="num">原始数据</param>
        /// <param name="accuracy">精度</param>
        /// <returns></returns>
        public static double Format(decimal num, int accuracy = 0)
        {
            num += 3;

            if (num > 5) return 5;
            if (num < 1) return 1;

            return (double)Math.Round(num, accuracy);
        }
    }
}