/*
 * Author：步尘
 * CreatDate ：2018-10-03 09:44:38
 * CLR Version ：4.0.30319.42000
 */
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Text;

namespace WimMain.Common
{
    /// <summary>
    /// 文件操作
    /// </summary>
    public class ToolFile
    {
        /// <summary>
        /// 返回绝对路径
        /// </summary>
        /// <param name="path">路径</param>
        /// <param name="isAddSymbol">是否添加分割符</param>
        /// <returns></returns>
        public static string GetAbsolutelyPath(string path, bool isAddSymbol = true)
        {
            // 过滤特殊字符
            path = path.Replace("<", "").Replace(">", "").Replace("`", "");

            // 获取后缀名
            string suffix = GetSuffix(path);

            //  若后缀名为空，即为文件夹路径
            if (string.IsNullOrWhiteSpace(suffix) && isAddSymbol)
            {
                // 若文件夹路径最后一项不是\，添加
                if (path.LastIndexOf("\\") != path.Length - 1)
                    path += "\\";
            }

            if (!isAddSymbol)
            {
                // 若文件夹路径最后一项是\，删除
                if (path.LastIndexOf("\\") == path.Length - 1)
                    path = path.Substring(0, path.Length - 1);
            }

            if (ToolRegular.Contains(path, ToolRegular._RIsAbsolutelyPath))
                return path;

            return Path.GetFullPath(path);
        }

        /// <summary>
        /// 返回上层目录
        /// </summary>
        /// <param name="path">路径</param>
        /// <param name="index">上层层级</param>
        /// <returns></returns>
        public static string GetUpIndex(string path, int index)
        {
            path = GetAbsolutelyPath(path);


            if (!(GetSuffix(path).Length > 0))
                index++;

            for (int i = 0; i < index; i++)
            {
                int tmp = path.LastIndexOf("\\");

                if (tmp == -1)
                    break;
                path = path.Substring(0, tmp);
            }

            return GetAbsolutelyPath(path);
        }

        /// <summary>
        /// 返回后缀名
        /// </summary>
        /// <param name="path">路径</param>
        /// <returns></returns>
        public static string GetSuffix(string path)
        {
            int index = path.LastIndexOf(".");

            if (index == -1)
                return string.Empty;

            string suffix = path.Substring(index + 1);

            index = suffix.LastIndexOf("\\");
            if (index == -1)
                return suffix;

            return string.Empty;
        }

        /// <summary>
        /// 返回文件名称
        /// </summary>
        /// <param name="path">路径</param>
        /// <param name="hasSuffix">是否包含后缀名</param>
        /// <returns></returns>
        public static string GetFileName(string path, bool hasSuffix = true)
        {
            // 去掉路径
            int index = path.LastIndexOf("\\");
            if (index > -1)
                path = path.Substring(index + 1);

            // 去掉后缀
            if (!hasSuffix)
            {
                index = path.LastIndexOf(".");
                if (index > -1)
                    path = path.Substring(0, index);
            }

            return path;
        }

        /// <summary>
        /// 创建文件
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <param name="line">行数据</param>
        /// <param name="append">是否追加</param>
        /// <returns></returns>
        public static FileInfo CreatFile(string path, string line, bool append)
        {
            path = GetAbsolutelyPath(path);

            string suffix = GetSuffix(path);

            // 检查是否是文件路径
            if (string.IsNullOrWhiteSpace(suffix))
                return null;

            // 检查文件夹是否存在
            string name = path.Substring(path.LastIndexOf('\\') + 1);
            path = path.Replace(name, "");

            // 若文件夹不存在，创建文件夹
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            path += name;

            // 创建文件
            using (StreamWriter write = new StreamWriter(path, append, Encoding.UTF8))
                write.WriteLine(line);

            return new FileInfo(path);
        }

        #region Excel

        /// <summary>
        /// 读取Excel文件
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static DataSet ReadExcel(string filePath, bool isAll, bool deleteFile)
        {
            DataSet myDS = new DataSet();

            string myConn = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=\"" + filePath + "\";Extended Properties=\"Excel 12.0;HDR=Yes;IMEX=1\"";

            OleDbConnection myConnection = new OleDbConnection(myConn);

            try
            {
                myConnection.Open();
                DataTable schemaTable = myConnection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);

                List<string> strTableNames = new List<string>();
                for (int k = 0; k < schemaTable.Rows.Count; k++)
                {
                    string s = schemaTable.Rows[k]["TABLE_NAME"].ToString();
                    if (!s.Contains("FilterDatabase") && s.IndexOf("$") > 0)
                    {
                        strTableNames.Add(s);
                    }
                }
                foreach (string name in strTableNames)
                {
                    string mySQLstr = "SELECT * FROM [" + name + "]";
                    OleDbDataAdapter myDataAdapter = new OleDbDataAdapter(mySQLstr, myConnection);
                    myDataAdapter.Fill(myDS, name);
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                if (myConnection.State == ConnectionState.Open)
                    myConnection.Close();
                if (deleteFile)
                {
                    File.Delete(filePath);
                }
            }

            return myDS;
        }

        /// <summary>
        /// 导出Excel
        /// </summary>
        /// <param name="dt">数据源</param>
        /// <param name="path"></param>
        public static void ExportExcel(DataTable dt, string path)
        {
            StringBuilder strbu = new StringBuilder();

            // 写入标题
            for (int i = 0; i < dt.Columns.Count; i++)
                strbu.Append(dt.Columns[i].ColumnName.ToString() + "\t");

            // 加入换行字符串
            strbu.Append(Environment.NewLine);

            // 写入内容
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                for (int j = 0; j < dt.Columns.Count; j++)
                    strbu.Append(dt.Rows[i][j].ToString() + "\t");
                strbu.Append(Environment.NewLine);
            }

            CreatFile(path, strbu.ToString(), false);
        }

        /// <summary>
        /// Excel导入成Datable
        /// </summary>
        /// <param name="file">导入路径(包含文件名与扩展名)</param>
        /// <returns></returns>
        public static DataTable ExcelToTable(string file)
        {
            DataTable dt = new DataTable();
            IWorkbook workbook;
            string fileExt = Path.GetExtension(file).ToLower();
            using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read))
            {
                //XSSFWorkbook 适用XLSX格式，HSSFWorkbook 适用XLS格式
                if (fileExt == ".xlsx") { workbook = new XSSFWorkbook(fs); } else if (fileExt == ".xls") { workbook = new HSSFWorkbook(fs); } else { workbook = null; }
                if (workbook == null) { return null; }
                ISheet sheet = workbook.GetSheetAt(0);

                //表头  
                IRow header = sheet.GetRow(sheet.FirstRowNum);
                List<int> columns = new List<int>();
                for (int i = 0; i < header.LastCellNum; i++)
                {
                    object obj = GetValueType(header.GetCell(i));
                    if (obj == null || obj.ToString() == string.Empty)
                    {
                        dt.Columns.Add(new DataColumn("Columns" + i.ToString()));
                    }
                    else
                        dt.Columns.Add(new DataColumn(obj.ToString()));
                    columns.Add(i);
                }
                //数据  
                for (int i = sheet.FirstRowNum + 1; i <= sheet.LastRowNum; i++)
                {
                    DataRow dr = dt.NewRow();
                    bool hasValue = false;
                    foreach (int j in columns)
                    {
                        dr[j] = GetValueType(sheet.GetRow(i).GetCell(j));
                        if (dr[j] != null && dr[j].ToString() != string.Empty)
                        {
                            hasValue = true;
                        }
                    }
                    if (hasValue)
                    {
                        dt.Rows.Add(dr);
                    }
                }
            }
            return dt;
        }

        /// <summary>
        /// Datable导出成Excel
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="file">导出路径(包括文件名与扩展名)</param>
        public static void TableToExcel(DataTable dt, string file)
        {
            IWorkbook workbook;
            string fileExt = Path.GetExtension(file).ToLower();
            if (fileExt == ".xlsx") { workbook = new XSSFWorkbook(); } else if (fileExt == ".xls") { workbook = new HSSFWorkbook(); } else { workbook = null; }
            if (workbook == null) { return; }
            ISheet sheet = string.IsNullOrEmpty(dt.TableName) ? workbook.CreateSheet("Sheet1") : workbook.CreateSheet(dt.TableName);

            //表头  
            IRow row = sheet.CreateRow(0);
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                ICell cell = row.CreateCell(i);
                cell.SetCellValue(dt.Columns[i].ColumnName);
            }

            //数据  
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                IRow row1 = sheet.CreateRow(i + 1);
                for (int j = 0; j < dt.Columns.Count; j++)
                {
                    ICell cell = row1.CreateCell(j);
                    cell.SetCellValue(dt.Rows[i][j].ToString());
                }
            }

            //转为字节数组  
            MemoryStream stream = new MemoryStream();
            workbook.Write(stream);
            var buf = stream.ToArray();

            //保存为Excel文件  
            using (FileStream fs = new FileStream(file, FileMode.Create, FileAccess.Write))
            {
                fs.Write(buf, 0, buf.Length);
                fs.Flush();
            }
        }

        /// <summary>
        /// 获取单元格类型
        /// </summary>
        /// <param name="cell"></param>
        /// <returns></returns>
        private static object GetValueType(ICell cell)
        {
            if (cell == null)
                return null;
            switch (cell.CellType)
            {
                case CellType.Blank: //BLANK:  
                    return null;
                case CellType.Boolean: //BOOLEAN:  
                    return cell.BooleanCellValue;
                case CellType.Numeric: //NUMERIC:  
                    return cell.NumericCellValue;
                case CellType.String: //STRING:  
                    return cell.StringCellValue;
                case CellType.Error: //ERROR:  
                    return cell.ErrorCellValue;
                case CellType.Formula: //FORMULA:  
                default:
                    return "=" + cell.CellFormula;
            }
        }

        #endregion

        #region Word

        /// <summary>
        /// 读取Word
        /// </summary>
        /// <param name="filePath">文件路径</param>
        public static NPOI.XWPF.UserModel.XWPFDocument ReadWord(string filePath)
        {
            NPOI.XWPF.UserModel.XWPFDocument document = null;

            // 读取
            using (FileStream file = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                document = new NPOI.XWPF.UserModel.XWPFDocument(file);
            }

            return document;
        }

        #endregion

    }
}
