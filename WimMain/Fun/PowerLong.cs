using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WimMain.Common;

namespace WimMain.Fun
{
    /// <summary>
    /// 宝龙
    /// </summary>
    public class PowerLong : BaseFun
    {
        public PowerLong(DBHelper db) : base(db)
        {
            base.CompanyID = "271490";
            base.FormApplicationID = "7871590001";
            base.FormID = "8204100001";
        }

        /// <summary>
        /// 创建报告
        /// </summary>
        /// <param name="tigerPath">目标路径</param>
        /// <returns></returns>
        public Result CreatReport(string tigerPath)
        {
            return RunFun(logpath =>
            {
                tigerPath = ToolFile.GetAbsolutelyPath(tigerPath);

                string modthPath = ToolFile.GetUpIndex(base.ModelPath,1) + "Custom\\宝龙-满意度版块PDF的副本.rdl";

                string sqlStr = @"
                     Select distinct r.FormReportItemID,c.Text
                    From AskForm_FormReportItemParam r 
                    INNER JOIN AskForm_Choice c ON c.ChoiceID = r.[Value]
                    Where 
	                    r.CompanyID=@CompanyID And r.FormApplicationID =@FormApplicationID
                      And r.IsDeleted=0";
                
                DataTable dt = DbContent.GetTable(GetSqlParam() + sqlStr);

                foreach (DataRow dr in dt.Rows)
                {
                    ReportParameterCollection reportParameters = new ReportParameterCollection
                    {
                        new ReportParameter("CompanyID", @CompanyID),
                        new ReportParameter("FormApplicationID", FormApplicationID),
                        new ReportParameter("FormID", FormID),
                        new ReportParameter("MinValue", "0"),
                        new ReportParameter("ReportTitle", "板块报告"),
                        new ReportParameter("FormReportItemID", dr["FormReportItemID"] + "")
                    };

                    ToolReport.GenerateLocalReport(modthPath, tigerPath, dr["Text"] + ".pdf", reportParameters, true);
                }
                return Res;
            });


        }


    }
}
