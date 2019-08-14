using CCWin.SkinControl;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Xml.Linq;
using WimMain.Common;
using WimMain.Fun;
using WimMain.Models;

namespace WimMain
{
    public partial class F_Main : Form
    {

        #region 窗体美化

        /// <summary>
        /// 窗体入口
        /// </summary>
        public F_Main()
        {
            InitializeComponent();
            MouseMove += Form_MouseDown;
            flp_Para.MouseMove += Form_MouseDown;
        }

        #region 窗体阴影

        private const int CS_DropSHADOW = 0x20000;
        private const int GCL_STYLE = (-26);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int SetClassLong(IntPtr hwnd, int nIndex, int dwNewLong);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int GetClassLong(IntPtr hwnd, int nIndex);

        #endregion

        #region 窗体移动

        //窗体移动API
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();
        [DllImport("user32.dll")]
        public static extern bool SendMessage(IntPtr hwnd, int wMsg, int wParam, int IParam);
        public const int WM_SYSCOMMAND = 0x0112;
        public const int SC_MOVE = 0xF010;
        public const int HTCAPTION = 0x0002;
        [DllImport("user32")]
        private static extern int SendMessage(IntPtr hwnd, int wMsg, int wParam, IntPtr lParam);
        private const int WM_SETREDRAW = 0xB;

        protected void Form_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();

            Control control = (sender as Control);

            while (!(control is Form)) control = control.Parent;

            SendMessage(control.Handle, WM_SYSCOMMAND, SC_MOVE + HTCAPTION, 0);
        }

        #endregion

        #endregion

        #region 窗体事件

        /// <summary>
        /// 获取当前程序集
        /// </summary>
        private Assembly _assembly = Assembly.GetExecutingAssembly();

        // 当前选择的类
        private Type selType;

        // 当前选择的方法
        private MethodInfo selMethod;

        /// <summary>
        /// 窗体加载时执行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void F_Main_Load(object sender, EventArgs e)
        {
            // 绑定复选框
            string IsTop = ToolConfig.GetAppSetting("IsTop");
            string IsLog = ToolConfig.GetAppSetting("IsLog");

            cb_IsTop.Checked = IsTop != "0";
            cb_Log.Checked = IsLog != "0";
            TopMost = cb_IsTop.Checked;

            string name = "T:";

            // 绑定类的信息
            BindCom(
                cmb_Class,
                _assembly.GetTypes(),
                c => c.BaseType == typeof(BaseFun),// 父类是BaseFun
                c => new ComBoxItem() { Display = GetNote(name + c.FullName, ""), Value = c.FullName });

            // 绑定数据库列表
            BindCom(
                cmb_Database,
                ToolConfig.GetConList(),
                c => true,
                c => new ComBoxItem() { Display = c, Value = c });

        }

        /// <summary>
        /// 选项更改时执行，重新绑定方法列表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmb_Class_SelectedIndexChanged(object sender, EventArgs e)
        {
            // 获取当前选择的类
            selType = _assembly.GetType(cmb_Class.SelectedValue.ToString());

            if (selType == null) return;

            // 绑定方法的信息
            BindCom(
                cmb_Fun,
                selType.GetMethods(),
                c => c.ReturnType == typeof(Result) && c.DeclaringType == selType,
                c => new ComBoxItem() { Display = GetNote(GetMethodNoteSel(c), ""), Value = c.Name });
        }

        /// <summary>
        /// 选项更改时执行，重新绑定参数列表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmb_Fun_SelectedIndexChanged(object sender, EventArgs e)
        {
            selMethod = selType.GetMethod(cmb_Fun.SelectedValue.ToString());
            BindPara();
        }

        /// <summary>
        /// 按钮单击时执行，执行选中的指定方法
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bt_Exec_Click(object sender, EventArgs e)
        {
            List<object> list = new List<object>();

            // 循环方法的所有参数
            foreach (var item in selMethod.GetParameters())
            {
                // 寻找和参数名称相同的控件
                Control con = flp_Para.Controls.Find(item.Name, false).FirstOrDefault();
                object obj = null;

                #region 参数校验

                if (con == null)
                {
                    MessageBox.Show($"缺少参数：{item.Name}");
                    return;
                }
                if (string.IsNullOrWhiteSpace(con.Text))
                {
                    MessageBox.Show($"{item.Name}：值为空");
                    return;
                }

                try
                {
                    obj = Convert.ChangeType(con.Text, item.ParameterType);
                }
                catch (Exception)
                {
                    MessageBox.Show($"{item.Name}：类型错误 ({item.ParameterType.Name})");
                    return;
                }

                #endregion

                list.Add(obj);

            }

            // 构造函数的参数
            object[] paramArr = new object[] {
                 DBHelper.GetDbContent(ToolConfig.GetConStr(cmb_Database.SelectedValue + ""))
            };

            Result res = (Result)selMethod.Invoke(_assembly.CreateInstance(selType.FullName, true, BindingFlags.Default, null, paramArr, null, null), list.ToArray());

            if (cb_Log.Checked)
            {
                res.Msg += $"\n{selType.Name}\t{selMethod.Name}\t运行时间：{res.RunTime} ms\n";
                MessageBox.Show(res.Msg);
            }
        }

        #endregion

        #region 复选框

        private void cb_IsTop_CheckedChanged(object sender, EventArgs e)
        {
            // 设置为顶层窗体
            TopMost = cb_IsTop.Checked;
            ToolConfig.SetAppSetting("IsTop", cb_IsTop.Checked ? 1 : 0);
        }

        private void cb_Log_CheckedChanged(object sender, EventArgs e)
        {
            ToolConfig.SetAppSetting("IsLog", cb_Log.Checked ? 1 : 0);
        }

        #endregion

        #region 右键菜单

        private void 日志文件夹ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string path = BaseFun.LogStarPath + selType.Name;
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            System.Diagnostics.Process.Start("explorer.exe", path);
        }

        private void 彻底关闭ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Environment.Exit(0);
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 绑定下拉框选项
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cmb">待绑定的下拉框</param>
        /// <param name="dataList">数据集</param>
        /// <param name="funcWhere">过滤条件</param>
        /// <param name="func">返回下拉项</param>
        public void BindCom<T>(ComboBox cmb, ICollection<T> dataList, Func<T, bool> funcWhere, Func<T, ComBoxItem> func)
        {
            List<ComBoxItem> list = new List<ComBoxItem>();

            if (!dataList.HasItems()) return;

            // 循环数据集
            foreach (var item in dataList)
            {
                // 执行条件
                if (funcWhere.Invoke(item))
                    list.Add(func.Invoke(item));
            }

            if (list == null) return;

            // 绑定数据集
            ComBoxItem option = new ComBoxItem();
            cmb.ValueMember = nameof(option.Value);
            cmb.DisplayMember = nameof(option.Display);
            cmb.DataSource = list;
        }

        /// <summary>
        /// 绑定参数列表
        /// </summary>
        private void BindPara()
        {
            // 清空所有控件
            flp_Para.Controls.Clear();

            string name = GetMethodNoteSel(selMethod);

            int y = 5;

            // 循环方法所需的所有参数
            foreach (var item in selMethod.GetParameters())
            {
                int x = 0;

                // 加载参数
                SkinLabel paraName = new SkinLabel
                {
                    Location = new Point(x, y + 2),
                    TextAlign = ContentAlignment.MiddleRight,
                    Size = new Size(80, 20),
                    Text = item.Name + "："
                };
                paraName.MouseMove += Form_MouseDown;

                x += paraName.Size.Width + 5;

                // 加载文本框
                SkinTextBox text = new SkinTextBox
                {
                    Name = item.Name,
                    Size = new Size(150, 20),
                    Location = new Point(x, y),
                    WaterText = GetNote(name, item.Name)
                };

                x += text.Size.Width + 5;

                // 加载参数类型
                SkinLabel paraType = new SkinLabel
                {
                    Location = new Point(x, y + 2),
                    TextAlign = ContentAlignment.MiddleLeft,
                    Size = new Size(70, 20),
                    Text = item.ParameterType.Name
                };
                paraType.MouseMove += Form_MouseDown;

                y += 27;

                flp_Para.Controls.Add(paraName);
                flp_Para.Controls.Add(text);
                flp_Para.Controls.Add(paraType);
            }
        }

        /// <summary>
        /// 返回注释信息
        /// </summary>
        /// <param name="name">名称</param>
        /// <param name="para">参数</param>
        /// <returns></returns>
        private string GetNote(string name, string para)
        {
            // 读取XML
            XDocument document = XDocument.Load(_assembly.GetName().Name + ".xml");

            // 根据name寻找节点
            var item = document.Descendants("member").Where(c => c.Attribute("name").Value == name).FirstOrDefault();

            if (item == null) return ToolString.RemoveCharAfter(name, "(");

            // 若参数名称为空
            if (string.IsNullOrWhiteSpace(para))
                return (item.Element("summary")?.Value + "").Replace("\n", "").Trim(); ;

            // 返回参数注释
            return (item.Elements("param").Where(c => c.Attribute("name").Value == para).FirstOrDefault()?.Value + "").Replace("\n", "").Trim();
        }

        /// <summary>
        /// 返回方法注释选择
        /// </summary>
        /// <param name="method">方法的元数据</param>
        /// <returns></returns>
        private string GetMethodNoteSel(MethodInfo method)
        {
            string name = $"M:{selType.FullName}.{method.Name}";
            if (method.GetParameters().Length > 0)
                name += $"({string.Join(",", method.GetParameters().Select(c => c.ParameterType.FullName))})";
            return name;
        }

        #endregion


    }



}