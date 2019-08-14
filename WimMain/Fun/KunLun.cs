using NPOI.XWPF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using WimMain.Common;

namespace WimMain.Fun
{
    /// <summary>
    /// 昆仑银行
    /// </summary>
    public class KunLun : BaseFun
    {
        public KunLun(DBHelper db) : base(db) { }

        public string allPath = @"C:\Users\mayn\Desktop\笔试题4套\";

        public string allTiger = @"C:\Users\mayn\Desktop\导出\";

        #region MyRegion

        /// <summary>
        /// 题目正则
        /// </summary>
        public string SubjectReg = @"^[\d].{1,}(\(|（)[  ]*[A-Z]{1,} *(\)|）).*";

        /// <summary>
        /// 换行
        /// </summary>
        public string Br = "<br />";

        /// <summary>
        /// 解析Word文档
        /// </summary>
        /// <returns></returns>
        //public Result FormatWord()
        //{
        //    return RunFun(logPath =>
        //    {
        //        string path = allPath + @"信息科技校招试题\测试管理岗-400\测试管理试题-400.docx";
        //        string tigerPath = allTiger + @"信息科技校招试题\测试管理岗-400\测试管理试题-400.txt";

        //        if (!File.Exists(path))
        //        {
        //            Res.Msg = "文件不存在";
        //            return Res;
        //        }

        //        XWPFDocument document = ToolFile.ReadWord(path);
        //        StringBuilder builder = new StringBuilder();

        //        foreach (XWPFParagraph item in document.Paragraphs)
        //        {
        //            builder.AppendLine(item.ParagraphText.Trim().Replace(" ", ""));
        //        }

        //        ToolFile.CreatFile(tigerPath, builder.ToString(), false);
        //        // 
        //        return Res;
        //    });

        //}

        #endregion

        // 匹配题号
        private static string fun1Str1 = @"^[  ]*([\d]+[，, 、．《.：:]+).*";

        // 匹配答案
        private static string fun1Str4 = @"((\(|（|_)+[  　 ]*[A-Z√×对错、 　 ,，]{1,} *(\)|）|_)+)";

        // 匹配题目
        private static string fun1Str2 = fun1Str1 + fun1Str4 + ".*";

        // 匹配选项
        private static string fun1Str5 = @"[  （(]*[A-H]{1}[ ．、.)）:：]*";

        // 匹配选项内容
        private static string fun1Str3 = @"^" + fun1Str5 + ".*";

        /// <summary>
        /// 排除词汇
        /// </summary>
        private List<string> ExcludeList = new List<string>();

        /// <summary>
        /// 解析题目
        /// </summary>
        /// <param name="file">文件路径</param>
        /// <param name="isCheck">是否检查段落</param>
        /// <param name="analysis">开启横向解析</param>
        /// <returns></returns>
        public Result Fun1(string file, bool isCheck, bool analysis)
        {
            return RunFun(logPath =>
            {
                string path = allPath + @"" + file + ".docx";
                string tigerPath = allTiger + @"" + file + ".txt";

                WriteLog(logPath, file + "," + isCheck);

                string exclude = allPath + "ExcludeList.json";

                if (File.Exists(exclude))
                    ExcludeList = ToolString.ConvertObject<List<string>>(File.ReadAllText(exclude));

                XWPFDocument document = ToolFile.ReadWord(path);
                StringBuilder builder = new StringBuilder();

                string subject = "";

                List<string> answerList = new List<string>();
                List<string> itemList = new List<string>();

                string[] typeArr = new string[] { "单项选择", "多项选择", "判断" };
                int index = -1, nextIndex = -1;// 题目类型
                int itemIndex = 0;// 答案索引
                int subjectIndex = 1;// 题目索引

                if (analysis)
                    fun1Str5 = @"[  （(]*[A-H]{1}[ ．、.)）:：]+";
                foreach (XWPFParagraph para in document.Paragraphs)
                {
                    string paragraphs = para.ParagraphText.Trim();//.Replace(" ", "");

                    for (int i = 0; i < typeArr.Length; i++)
                    {
                        if (ToolRegular.Contains(paragraphs, ".*" + typeArr[i] + @"题[(（][\d]*.*[)）]"))
                        {
                            nextIndex = i;
                            subjectIndex = 1;
                            if (index == -1)
                                index = i;
                        }

                    }

                    List<string> list = paragraphs.Split('\n').ToList();

                    foreach (var line in list)
                    {
                        // 判断是否是题目开头
                        Regex reg = new Regex(fun1Str1);
                        if (IsSubject(para, isCheck) || (reg.IsMatch(line) && !isCheck))
                        {
                            // 输出上一题
                            if (!string.IsNullOrWhiteSpace(subject))
                            {
                                subject = ReplaceSubject(subject, "、", isCheck);


                                if (!string.IsNullOrWhiteSpace(subject))
                                {
                                    answerList = AnswerList(subject);
                                    subject = answerList[0];
                                    answerList.Remove(subject);

                                    if (index == -1) { index = 0; nextIndex = 0; }

                                    subject += "[" + typeArr[index] + "题]\n";

                                    AnswerList(answerList, itemList, typeArr[index]);

                                    foreach (var item in itemList)
                                        subject += item + "\n";

                                    builder.AppendLine(subject);

                                    index = nextIndex;

                                    WriteLog(allTiger + @"" + file + ".log", "[" + typeArr[index] + "题]\t" + ToolString.AddStrByLength(subjectIndex + "", " ", 3) + "\t" + ToolString.AddStrByLength(string.Join(",", answerList), " ", 10) + "\t[" + string.Join(",", itemList.Select(c => c[0])) + "]", true);
                                    subjectIndex++;
                                }
                            }

                            // 开始下一题
                            itemList = new List<string>();
                            answerList = new List<string>();
                            subject = line;
                            itemIndex = 0;
                        }
                        else
                        {
                            if (string.IsNullOrWhiteSpace(subject))
                                continue;
                            // 判断是否是选项开头
                            else if (IsItem(para, isCheck) || new Regex(fun1Str3).IsMatch(line))
                            {
                                itemList.AddRange(ItemList(line, para, isCheck, itemIndex++, analysis));
                                continue;
                            }
                            // 判断是否是换行题目
                            else if (itemList.Count <= 0 && !string.IsNullOrWhiteSpace(line) && (!isCheck || para.GetNumFmt() != "lowerRoman"))
                            {
                                subject += Br + line;
                            }
                        }
                    }
                }

                // 输出最后一题
                if (!string.IsNullOrWhiteSpace(subject))
                {
                    subject = ReplaceSubject(subject, "、", isCheck);


                    if (!string.IsNullOrWhiteSpace(subject))
                    {
                        answerList = AnswerList(subject);
                        subject = answerList[0];
                        answerList.Remove(subject);

                        subject += "[" + typeArr[index] + "题]\n";

                        AnswerList(answerList, itemList, typeArr[index]);

                        foreach (var item in itemList)
                            subject += item + "\n";

                        builder.AppendLine(subject);

                        index = nextIndex;
                        WriteLog(allTiger + @"" + file + ".log", "[" + typeArr[index] + "题]\t" + ToolString.AddStrByLength(subjectIndex + "", " ", 3) + "\t" + ToolString.AddStrByLength(string.Join(",", answerList), " ", 10) + "\t[" + string.Join(",", itemList.Select(c => c[0])) + "]", true);
                    }
                }

                ToolFile.CreatFile(tigerPath, builder.ToString(), false);

                return Res;
            });
        }

        /// <summary>
        /// 返回答案列表
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public List<string> AnswerList(string str)
        {
            if (string.IsNullOrWhiteSpace(str)) return null;
            List<string> list = new List<string>();

            List<string> tmp = ToolRegular.GetPoint(str, fun1Str4, 1);

            for (int i = 0; i < tmp.Count; i++)
            {
                string str1 = tmp[i];
                if (ExcludeList.Exists(c => str1.ToUpper().Contains(c.ToUpper())))
                {
                    tmp.RemoveAt(i--);
                    continue;
                }
                str = str.Replace(str1, "( )");

                str1 = str1.Replace("、", "");

                for (int j = 0; j < str1.Length; j++)
                {
                    if (('A' <= str1[j] && str1[j] <= 'Z') || str1[j] == '√' || str1[j] == '×' || str1[j] == '对' || str1[j] == '错')
                    {
                        if (str1[j] == '√' || str1[j] == '对') list.Add('A' + "");
                        else if (str1[j] == '×' || str1[j] == 'X' || str1[j] == 'x' || str1[j] == '错') list.Add('B' + "");
                        else list.Add(str1[j] + "");
                    }
                }
            }

            list.Insert(0, str);
            return list;
        }

        /// <summary>
        /// 返回选项列表
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public List<string> ItemList(string str, XWPFParagraph para, bool isCheck, int index, bool analysis)
        {
            List<string> list = new List<string>();

            string log = str;

            string t = "";

            if (isCheck)
            {
                t = (char)('A' + index) + "";
            }
            else
            {
                if (analysis)
                {
                    // 获取横排所有选项
                    List<string> tmp = ToolRegular.GetPoint(str, fun1Str5, 0).ToList();

                    for (int i = 0; i < tmp.Count; i++)
                    {
                        string str1 = tmp[i];
                        t = "";

                        str = str.Replace(str1, "灬").Trim();
                        // 获取选项字符
                        for (int j = 0; j < str1.Length; j++)
                        {
                            if ('A' <= str1[j] && str1[j] <= 'Z')
                                t += str1[j];
                        }

                        list.Add(t + "．");
                    }
                    tmp = str.Split('灬').Where(c => !string.IsNullOrWhiteSpace(c)).Select(c => c.Trim()).ToList();

                    try
                    {
                        for (int i = 0; i < tmp.Count; i++)
                            list[i] += tmp[i];
                        return list;
                    }
                    catch (Exception)
                    {
                        ToolFile.CreatFile(@"D:\111.log", log + "\t" + para.ParagraphText, true);
                    }
                }
                else
                {
                    string tmp = ToolRegular.GetPoint(str, fun1Str5, 0).FirstOrDefault();

                    str = str.Replace(tmp, "").Trim();
                    for (int i = 0; i < tmp.Length; i++)
                    {
                        if ('A' <= tmp[i] && tmp[i] <= 'Z')
                            t += tmp[i];
                    }
                }


            }

            list.Add(t + "．" + str);

            return list;
        }

        /// <summary>
        /// 答案与选项匹配
        /// </summary>
        /// <param name="answer"></param>
        /// <param name="item"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public List<string> AnswerList(List<string> answer, List<string> item, string type)
        {
            if (type == "判断"&& item.Count!=2)
            {
                item.Clear();
                item.Add("A．对");
                item.Add("B．错");
            }

            for (int i = 0; i < answer.Count; i++)
            {
                string str = answer[i] + "．";

                string ans = item.Where(c => c.Contains(str)).FirstOrDefault();

                int index = item.IndexOf(ans);

                if (index == -1)
                {
                    item.Add(str + "[答案]");
                }
                else
                {
                    item.Remove(ans);
                    item.Insert(index, ans + "[答案]");
                }

            }

            return item;
        }

        /// <summary>
        /// 替换题目序列
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public string ReplaceSubject(string str, string ch, bool isCheck)
        {
            if (string.IsNullOrWhiteSpace(str)) return "";

            if (isCheck)
                return str;

            string str1 = ToolRegular.GetPoint(str, fun1Str1, 1).FirstOrDefault();

            if (str1 != null)
            {
                str = str.Substring(str1.Length).Trim();
                str1 = str1.Trim();
            }

            //return str1.Substring(0, str1.Length - 1) + ch + str;
            return str;
        }


        public bool IsSubject(XWPFParagraph para, bool isCheck)
        {
            return para.GetNumFmt() == "decimal";
        }

        public bool IsItem(XWPFParagraph para, bool isCheck)
        {
            return para.GetNumFmt() == "upperLetter";
        }

    }
}
