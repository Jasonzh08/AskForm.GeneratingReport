namespace WimMain
{
    partial class F_Main
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(F_Main));
            this.label1 = new System.Windows.Forms.Label();
            this.cmb_Class = new System.Windows.Forms.ComboBox();
            this.cmb_Fun = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.cb_IsTop = new System.Windows.Forms.CheckBox();
            this.bt_Exec = new System.Windows.Forms.Button();
            this.cb_Log = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.cms_Menu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.日志文件夹ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.彻底关闭ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.flp_Para = new System.Windows.Forms.Panel();
            this.tip = new System.Windows.Forms.ToolTip(this.components);
            this.label3 = new System.Windows.Forms.Label();
            this.cmb_Database = new System.Windows.Forms.ComboBox();
            this.cb_para = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.cms_Menu.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(5, 9);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(29, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "类名";
            // 
            // cmb_Class
            // 
            this.cmb_Class.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmb_Class.FormattingEnabled = true;
            this.cmb_Class.Location = new System.Drawing.Point(49, 6);
            this.cmb_Class.Margin = new System.Windows.Forms.Padding(2);
            this.cmb_Class.Name = "cmb_Class";
            this.cmb_Class.Size = new System.Drawing.Size(305, 20);
            this.cmb_Class.TabIndex = 1;
            this.cmb_Class.SelectedIndexChanged += new System.EventHandler(this.cmb_Class_SelectedIndexChanged);
            // 
            // cmb_Fun
            // 
            this.cmb_Fun.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmb_Fun.FormattingEnabled = true;
            this.cmb_Fun.Location = new System.Drawing.Point(49, 32);
            this.cmb_Fun.Margin = new System.Windows.Forms.Padding(2);
            this.cmb_Fun.Name = "cmb_Fun";
            this.cmb_Fun.Size = new System.Drawing.Size(305, 20);
            this.cmb_Fun.TabIndex = 3;
            this.cmb_Fun.SelectedIndexChanged += new System.EventHandler(this.cmb_Fun_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(5, 34);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(41, 12);
            this.label2.TabIndex = 2;
            this.label2.Text = "方法名";
            // 
            // cb_IsTop
            // 
            this.cb_IsTop.AutoSize = true;
            this.cb_IsTop.Checked = true;
            this.cb_IsTop.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cb_IsTop.Location = new System.Drawing.Point(7, 295);
            this.cb_IsTop.Margin = new System.Windows.Forms.Padding(2);
            this.cb_IsTop.Name = "cb_IsTop";
            this.cb_IsTop.Size = new System.Drawing.Size(72, 16);
            this.cb_IsTop.TabIndex = 5;
            this.cb_IsTop.Text = "是否置顶";
            this.cb_IsTop.UseVisualStyleBackColor = true;
            this.cb_IsTop.CheckedChanged += new System.EventHandler(this.cb_IsTop_CheckedChanged);
            // 
            // bt_Exec
            // 
            this.bt_Exec.Location = new System.Drawing.Point(7, 316);
            this.bt_Exec.Margin = new System.Windows.Forms.Padding(2);
            this.bt_Exec.Name = "bt_Exec";
            this.bt_Exec.Size = new System.Drawing.Size(350, 38);
            this.bt_Exec.TabIndex = 8;
            this.bt_Exec.Text = "执行方法";
            this.bt_Exec.UseVisualStyleBackColor = true;
            this.bt_Exec.Click += new System.EventHandler(this.bt_Exec_Click);
            // 
            // cb_Log
            // 
            this.cb_Log.AutoSize = true;
            this.cb_Log.Location = new System.Drawing.Point(83, 296);
            this.cb_Log.Margin = new System.Windows.Forms.Padding(2);
            this.cb_Log.Name = "cb_Log";
            this.cb_Log.Size = new System.Drawing.Size(72, 16);
            this.cb_Log.TabIndex = 7;
            this.cb_Log.Text = "弹框日志";
            this.cb_Log.UseVisualStyleBackColor = true;
            this.cb_Log.CheckedChanged += new System.EventHandler(this.cb_Log_CheckedChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.ContextMenuStrip = this.cms_Menu;
            this.groupBox1.Controls.Add(this.flp_Para);
            this.groupBox1.Location = new System.Drawing.Point(10, 121);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox1.Size = new System.Drawing.Size(347, 173);
            this.groupBox1.TabIndex = 9;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "参数列表";
            // 
            // cms_Menu
            // 
            this.cms_Menu.ImageScalingSize = new System.Drawing.Size(28, 28);
            this.cms_Menu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.日志文件夹ToolStripMenuItem,
            this.彻底关闭ToolStripMenuItem});
            this.cms_Menu.Name = "cms_Menu";
            this.cms_Menu.Size = new System.Drawing.Size(137, 48);
            // 
            // 日志文件夹ToolStripMenuItem
            // 
            this.日志文件夹ToolStripMenuItem.Name = "日志文件夹ToolStripMenuItem";
            this.日志文件夹ToolStripMenuItem.Size = new System.Drawing.Size(136, 22);
            this.日志文件夹ToolStripMenuItem.Text = "日志文件夹";
            this.日志文件夹ToolStripMenuItem.Click += new System.EventHandler(this.日志文件夹ToolStripMenuItem_Click);
            // 
            // 彻底关闭ToolStripMenuItem
            // 
            this.彻底关闭ToolStripMenuItem.Name = "彻底关闭ToolStripMenuItem";
            this.彻底关闭ToolStripMenuItem.Size = new System.Drawing.Size(136, 22);
            this.彻底关闭ToolStripMenuItem.Text = "彻底关闭";
            this.彻底关闭ToolStripMenuItem.Click += new System.EventHandler(this.彻底关闭ToolStripMenuItem_Click);
            // 
            // flp_Para
            // 
            this.flp_Para.AutoScroll = true;
            this.flp_Para.ContextMenuStrip = this.cms_Menu;
            this.flp_Para.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flp_Para.Location = new System.Drawing.Point(2, 16);
            this.flp_Para.Margin = new System.Windows.Forms.Padding(2);
            this.flp_Para.Name = "flp_Para";
            this.flp_Para.Size = new System.Drawing.Size(343, 155);
            this.flp_Para.TabIndex = 10;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(5, 90);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(41, 12);
            this.label3.TabIndex = 2;
            this.label3.Text = "数据库";
            // 
            // cmb_Database
            // 
            this.cmb_Database.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmb_Database.FormattingEnabled = true;
            this.cmb_Database.Location = new System.Drawing.Point(49, 84);
            this.cmb_Database.Margin = new System.Windows.Forms.Padding(2);
            this.cmb_Database.Name = "cmb_Database";
            this.cmb_Database.Size = new System.Drawing.Size(305, 20);
            this.cmb_Database.TabIndex = 3;
            this.cmb_Database.SelectedIndexChanged += new System.EventHandler(this.cmb_Fun_SelectedIndexChanged);
            // 
            // cb_para
            // 
            this.cb_para.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cb_para.FormattingEnabled = true;
            this.cb_para.Location = new System.Drawing.Point(49, 58);
            this.cb_para.Margin = new System.Windows.Forms.Padding(2);
            this.cb_para.Name = "cb_para";
            this.cb_para.Size = new System.Drawing.Size(305, 20);
            this.cb_para.TabIndex = 11;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(4, 60);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(41, 12);
            this.label4.TabIndex = 10;
            this.label4.Text = "参数列";
            // 
            // F_Main
            // 
            this.AcceptButton = this.bt_Exec;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(369, 361);
            this.ContextMenuStrip = this.cms_Menu;
            this.Controls.Add(this.cb_para);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.bt_Exec);
            this.Controls.Add(this.cb_Log);
            this.Controls.Add(this.cb_IsTop);
            this.Controls.Add(this.cmb_Database);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.cmb_Fun);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.cmb_Class);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MaximizeBox = false;
            this.Name = "F_Main";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "入口函数";
            this.Load += new System.EventHandler(this.F_Main_Load);
            this.groupBox1.ResumeLayout(false);
            this.cms_Menu.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cmb_Class;
        private System.Windows.Forms.ComboBox cmb_Fun;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox cb_IsTop;
        private System.Windows.Forms.Button bt_Exec;
        private System.Windows.Forms.CheckBox cb_Log;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Panel flp_Para;
        private System.Windows.Forms.ToolTip tip;
        private System.Windows.Forms.ContextMenuStrip cms_Menu;
        private System.Windows.Forms.ToolStripMenuItem 日志文件夹ToolStripMenuItem;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox cmb_Database;
        private System.Windows.Forms.ToolStripMenuItem 彻底关闭ToolStripMenuItem;
        private System.Windows.Forms.ComboBox cb_para;
        private System.Windows.Forms.Label label4;
    }
}

