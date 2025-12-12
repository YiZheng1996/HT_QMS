using System.Drawing;
using System.Windows.Forms;

namespace QMSCientForm
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.btnTestInfo = new System.Windows.Forms.Button();
            this.btnDeviceInfo = new System.Windows.Forms.Button();
            this.SuspendLayout();

            // 
            // btnTestInfo
            // 
            this.btnTestInfo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(152)))), ((int)(((byte)(219)))));
            this.btnTestInfo.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnTestInfo.Font = new System.Drawing.Font("微软雅黑", 14F, System.Drawing.FontStyle.Bold);
            this.btnTestInfo.ForeColor = System.Drawing.Color.White;
            this.btnTestInfo.Location = new System.Drawing.Point(150, 150);
            this.btnTestInfo.Name = "btnTestInfo";
            this.btnTestInfo.Size = new System.Drawing.Size(200, 60);
            this.btnTestInfo.TabIndex = 0;
            this.btnTestInfo.Text = "试验信息发送";
            this.btnTestInfo.UseVisualStyleBackColor = false;
            this.btnTestInfo.Click += new System.EventHandler(this.btnTestInfo_Click);
            this.btnTestInfo.MouseEnter += new System.EventHandler(this.Button_MouseEnter);
            this.btnTestInfo.MouseLeave += new System.EventHandler(this.Button_MouseLeave);

            // 
            // btnDeviceInfo
            // 
            this.btnDeviceInfo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(204)))), ((int)(((byte)(113)))));
            this.btnDeviceInfo.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDeviceInfo.Font = new System.Drawing.Font("微软雅黑", 14F, System.Drawing.FontStyle.Bold);
            this.btnDeviceInfo.ForeColor = System.Drawing.Color.White;
            this.btnDeviceInfo.Location = new System.Drawing.Point(450, 150);
            this.btnDeviceInfo.Name = "btnDeviceInfo";
            this.btnDeviceInfo.Size = new System.Drawing.Size(200, 60);
            this.btnDeviceInfo.TabIndex = 1;
            this.btnDeviceInfo.Text = "设备信息发送";
            this.btnDeviceInfo.UseVisualStyleBackColor = false;
            this.btnDeviceInfo.Click += new System.EventHandler(this.btnDeviceInfo_Click);
            this.btnDeviceInfo.MouseEnter += new System.EventHandler(this.Button_MouseEnter);
            this.btnDeviceInfo.MouseLeave += new System.EventHandler(this.Button_MouseLeave);

            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(800, 400);
            this.Controls.Add(this.btnDeviceInfo);
            this.Controls.Add(this.btnTestInfo);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "QMS数据查询系统";
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Button btnTestInfo;
        private System.Windows.Forms.Button btnDeviceInfo;

        /// <summary>
        /// 鼠标进入按钮效果
        /// </summary>
        private void Button_MouseEnter(object sender, System.EventArgs e)
        {
            Button btn = sender as Button;
            if (btn != null)
            {
                btn.FlatAppearance.BorderSize = 2;
                btn.FlatAppearance.BorderColor = System.Drawing.Color.White;
            }
        }

        /// <summary>
        /// 鼠标离开按钮效果
        /// </summary>
        private void Button_MouseLeave(object sender, System.EventArgs e)
        {
            Button btn = sender as Button;
            if (btn != null)
            {
                btn.FlatAppearance.BorderSize = 0;
            }
        }
    }
}
