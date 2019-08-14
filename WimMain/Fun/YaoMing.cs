using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WimMain.Common;

namespace WimMain.Fun
{
    /// <summary>
    /// 药明
    /// </summary>
    class YaoMing : BaseFun
    {
        public const string BUListSQL = "select distinct c.text from askform_EntryChoice ec" +
          " inner join askform_field f on ec.fieldid=f.fieldid" +
          " inner join askform_choice c on ec.value= c.choiceID" +
          " where ec.companyID = @CompanyID and ec.formid= @FormID" +
          " and f.name= 'BU'";

        public const string DepartmentListSQL = "Declare @TotalScore decimal (18,8)" +
         " declare  @EmployeeInfo table(entryID bigint, status int, BU nvarchar(100), Organization nvarchar(100),Education nvarchar(100) )" +
         " insert into @EmployeeInfo" +
         " exec[AskForm_EntryContent_GetPivotDataTableByFieldNames] @CompanyID,@FormApplicationID,@FormID,'BU,Organization,Education'" +
         " select distinct ei.Organization, ei.BU from @EmployeeInfo ei" +
         " where ei.BU= {BU}";
            //+
         //" UNION all select 'All', {BU}";

        public YaoMing(DBHelper db) : base(db)
        {
            CompanyID = "3";
            FormApplicationID = "200004";
            FormID = "380004";
        }

        /// <summary>
        /// 生成部门报告
        /// </summary>
        /// <param name="dsfd">dzhg</param>
        /// <returns></returns>
        public Result GeneratingReports()
        {
            //获取报告地址
            string modelFileName = "满意度-药明.rdl";
            string modelPath = Path.Combine(ModelPath, modelFileName);

            //开始生成报告
            return RunFun((logPath) =>
            {
                WriteLog(logPath, "开始生成报告");
                //绑定 CompanyID，FormApplicationID，FormID
                ReportParameterCollection col = ToolReport.BindPara(modelPath, Convert.ToInt32(FormID), false);

                //获取BUList
                string prefix = GetSqlParam();
                DataTable buList = DbContent.GetTable(prefix + BUListSQL);

                //BU循环
                foreach (DataRow buRow in buList.Rows)
                {
                    string bu = buRow.ItemArray[0].ToString();
                    WriteLog(logPath, $"当前BU：{bu}");
                    long time = Watch(() =>
                    {
                        string departmentListSQL = DepartmentListSQL.Replace("{BU}", $"'{bu}'");
                        DataTable departmentList = DbContent.GetTable(prefix + departmentListSQL);
                        //Department循环
                        foreach (DataRow departmentRow in departmentList.Rows)
                        {
                            string department = departmentRow.ItemArray[0].ToString();
                            WriteLog(logPath, $"当前Department：{department}");
                            //绑定 BU,Department参数
                            col.Add(new ReportParameter("BU", bu));
                            col.Add(new ReportParameter("Department", department));

                            // 绑定默认参数
                            col = ToolReport.BindPara(modelPath, col);

                            //生成部门报告
                            ToolReport.GenerateLocalReport(modelPath, Path.Combine(DataStarPath,bu)+"\\", $"Biologics-满意度-{department}.pdf", col, true);
                            WriteLog(logPath, $"Biologics-满意度-{department}.pdf 报告生成完毕");
                        }
                    });
                }
                WriteLog(logPath, "报告生成完毕");

                return Res;
            });
        }

    }
}
