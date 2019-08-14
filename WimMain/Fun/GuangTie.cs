using System.Collections.Generic;
using System.IO;
using System.Linq;
using WimMain.Common;
using WimMain.Models;

namespace WimMain.Fun
{
    /// <summary>
    /// 广铁
    /// </summary>
    public class GuangTie : BaseFun
    {
        public GuangTie(DBHelper db) : base(db) { }

        /// <summary>
        /// 返回Excel文本
        /// </summary>
        /// <param name="dirPath"></param>
        /// <param name="length">被评估人信息长度</param>
        /// <returns></returns>
        public Result GetExcelTxt(string dirPath, int length)
        {
            return RunFun(logPath =>
            {
                dirPath = ToolFile.GetAbsolutelyPath(dirPath);

                if (!Directory.Exists(dirPath))
                {
                    Res.Msg = "文件夹不存在";
                    return Res;
                }

                // 获取所有子文件夹
                List<DirectoryInfo> listDir = new DirectoryInfo(dirPath).GetDirectories().ToList();

                foreach (var dir in listDir)
                {
                    // 获取待整理的名单文件
                    List<FileInfo> fileList = dir.GetFiles().Where(c => c.Name.Contains("_") && c.Extension.ToLower().Contains("txt") && !c.Name.Contains("Res")).ToList();

                    // 循环名单文件
                    foreach (var file in fileList)
                    {
                        List<ExcelFormat> excelList = new List<ExcelFormat>();
                        //WriteLog(logPath, file.FullName);
                        // 循环行
                        foreach (var line in File.ReadAllLines(file.FullName))
                        {
                            List<ExcelFormat> tmpList = new List<ExcelFormat>();
                            // 获取被评估人信息
                            string[] edArr = line.Split('	').Where(c => !string.IsNullOrWhiteSpace(c)).Select(c => c.Replace(" ", "")).ToArray();

                            if (edArr.Length != length)
                            {
                                WriteLog(logPath, file.FullName + "\t" + "被评估人信息\t" + line);
                                continue;
                            }

                            string t = "名单";

                            if (edArr[4].Contains(t))
                            {
                                string name = ToolString.GetLetter(edArr[4]);

                                for (int i = 0; i < name.Length; i++)
                                {
                                    // 循环名单列表
                                    foreach (var nameLine in File.ReadAllLines(ToolFile.GetAbsolutelyPath(file.DirectoryName) + t + name[i] + ".txt"))
                                    {
                                        string[] nameArr = nameLine.Split('	').Where(c => !string.IsNullOrWhiteSpace(c)).Select(c => c.Replace(" ", "")).ToArray();

                                        if (nameArr.Length != 2)
                                        {
                                            WriteLog(logPath, file.FullName + "\t" + line + "\t" + "循环名单列表\t" + nameLine);
                                            continue;
                                        }

                                        if (edArr[1] == nameArr[1])
                                        {
                                            WriteLog(logPath, file.FullName + "\t" + edArr[1] + "\t" + "重名\t" + name[i] + ".txt");
                                            continue;
                                        }

                                        ExcelFormat excel = new ExcelFormat();
                                        // 基础信息
                                        excel.ObservedDepartment = edArr[0];
                                        excel.ObservedName = edArr[1];
                                        excel.Mapping = edArr[2];
                                        excel.Weight = edArr[3];
                                        excel.ObserverDepartment = nameArr[0];
                                        excel.ObserverName = nameArr[1];

                                        tmpList.Add(excel);

                                    }
                                }

                            }
                            else
                            {
                                // 添加分割属性
                                edArr[4] += "、";

                                // 获取多评估人信息，
                                string[] erArr = edArr[4].Split('、').Where(c => !string.IsNullOrWhiteSpace(c)).ToArray();

                                foreach (var er in erArr)
                                {
                                    ExcelFormat excel = new ExcelFormat();
                                    // 基础信息
                                    excel.ObservedDepartment = edArr[0];
                                    excel.ObservedName = edArr[1];
                                    excel.Mapping = edArr[2];
                                    excel.Weight = edArr[3];
                                    excel.ObserverName = er;
                                    tmpList.Add(excel);
                                }
                            }
                            excelList.AddRange(tmpList);
                        }
                        ExcelFormat.CreateText(ToolFile.GetAbsolutelyPath(file.DirectoryName) + "Res" + file.Name, excelList);
                    }
                }

                return Res;
            });
        }

    }
}
