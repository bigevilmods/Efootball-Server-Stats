using System;

namespace Efootball_Server_Stats
{
    partial class Form1
    {
        /// <summary>
        /// Variável de designer necessária.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Limpar os recursos que estão sendo usados.
        /// </summary>
        /// <param name="disposing">true se for necessário descartar os recursos gerenciados; caso contrário, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código gerado pelo Windows Form Designer

        /// <summary>
        /// Método necessário para suporte ao Designer - não modifique 
        /// o conteúdo deste método com o editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            this.listBoxServers = new System.Windows.Forms.ListBox();
            this.btnOptimize = new System.Windows.Forms.Button();
            this.txtResult = new System.Windows.Forms.TextBox();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.SuspendLayout();
            // 
            // listBoxServers
            // 
            this.listBoxServers.FormattingEnabled = true;
            this.listBoxServers.Location = new System.Drawing.Point(12, 17);
            this.listBoxServers.Name = "listBoxServers";
            this.listBoxServers.Size = new System.Drawing.Size(549, 30);
            this.listBoxServers.TabIndex = 0;
            // 
            // btnOptimize
            // 
            this.btnOptimize.Location = new System.Drawing.Point(12, 63);
            this.btnOptimize.Name = "btnOptimize";
            this.btnOptimize.Size = new System.Drawing.Size(549, 29);
            this.btnOptimize.TabIndex = 1;
            this.btnOptimize.Text = "Analyze";
            this.btnOptimize.UseVisualStyleBackColor = true;
            this.btnOptimize.Click += new System.EventHandler(this.btnOptimize_Click);
            // 
            // txtResult
            // 
            this.txtResult.Location = new System.Drawing.Point(12, 98);
            this.txtResult.Multiline = true;
            this.txtResult.Name = "txtResult";
            this.txtResult.ReadOnly = true;
            this.txtResult.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtResult.Size = new System.Drawing.Size(549, 103);
            this.txtResult.TabIndex = 2;
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(12, 207);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(549, 23);
            this.progressBar.TabIndex = 4;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(578, 246);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.txtResult);
            this.Controls.Add(this.btnOptimize);
            this.Controls.Add(this.listBoxServers);
            this.Name = "Form1";
            this.Text = "Efootball Server Sats";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            listBoxServers.Hide();
        }

        #endregion

        private System.Windows.Forms.ListBox listBoxServers;
        private System.Windows.Forms.Button btnOptimize;
        private System.Windows.Forms.TextBox txtResult;
        private System.Windows.Forms.ProgressBar progressBar;
    }
}

