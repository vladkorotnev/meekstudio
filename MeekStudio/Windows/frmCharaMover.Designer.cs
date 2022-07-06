namespace MeekStudio
{
    partial class frmCharaMover
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmCharaMover));
            this.gbxMove = new System.Windows.Forms.GroupBox();
            this.btnInsertMove = new System.Windows.Forms.Button();
            this.btnSetPos = new System.Windows.Forms.Button();
            this.btnFetchPos = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.cmbCharaIdxMove = new System.Windows.Forms.ComboBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.cpZ = new System.Windows.Forms.TextBox();
            this.cpY = new System.Windows.Forms.TextBox();
            this.cpX = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.gbxRot = new System.Windows.Forms.GroupBox();
            this.btnInsertRot = new System.Windows.Forms.Button();
            this.btnSendRot = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.tbRot = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.cmbCharaIdxRot = new System.Windows.Forms.ComboBox();
            this.gbxMove.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.gbxRot.SuspendLayout();
            this.SuspendLayout();
            // 
            // gbxMove
            // 
            this.gbxMove.Controls.Add(this.btnInsertMove);
            this.gbxMove.Controls.Add(this.btnSetPos);
            this.gbxMove.Controls.Add(this.btnFetchPos);
            this.gbxMove.Controls.Add(this.label1);
            this.gbxMove.Controls.Add(this.cmbCharaIdxMove);
            this.gbxMove.Controls.Add(this.groupBox3);
            resources.ApplyResources(this.gbxMove, "gbxMove");
            this.gbxMove.Name = "gbxMove";
            this.gbxMove.TabStop = false;
            // 
            // btnInsertMove
            // 
            resources.ApplyResources(this.btnInsertMove, "btnInsertMove");
            this.btnInsertMove.Name = "btnInsertMove";
            this.btnInsertMove.UseVisualStyleBackColor = true;
            this.btnInsertMove.Click += new System.EventHandler(this.btnInsertMove_Click);
            // 
            // btnSetPos
            // 
            resources.ApplyResources(this.btnSetPos, "btnSetPos");
            this.btnSetPos.Name = "btnSetPos";
            this.btnSetPos.UseVisualStyleBackColor = true;
            this.btnSetPos.Click += new System.EventHandler(this.btnSetPos_Click);
            // 
            // btnFetchPos
            // 
            resources.ApplyResources(this.btnFetchPos, "btnFetchPos");
            this.btnFetchPos.Name = "btnFetchPos";
            this.btnFetchPos.UseVisualStyleBackColor = true;
            this.btnFetchPos.Click += new System.EventHandler(this.btnFetchPos_Click);
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // cmbCharaIdxMove
            // 
            this.cmbCharaIdxMove.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbCharaIdxMove.FormattingEnabled = true;
            this.cmbCharaIdxMove.Items.AddRange(new object[] {
            resources.GetString("cmbCharaIdxMove.Items"),
            resources.GetString("cmbCharaIdxMove.Items1"),
            resources.GetString("cmbCharaIdxMove.Items2")});
            resources.ApplyResources(this.cmbCharaIdxMove, "cmbCharaIdxMove");
            this.cmbCharaIdxMove.Name = "cmbCharaIdxMove";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.cpZ);
            this.groupBox3.Controls.Add(this.cpY);
            this.groupBox3.Controls.Add(this.cpX);
            resources.ApplyResources(this.groupBox3, "groupBox3");
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.TabStop = false;
            // 
            // cpZ
            // 
            resources.ApplyResources(this.cpZ, "cpZ");
            this.cpZ.Name = "cpZ";
            // 
            // cpY
            // 
            resources.ApplyResources(this.cpY, "cpY");
            this.cpY.Name = "cpY";
            // 
            // cpX
            // 
            resources.ApplyResources(this.cpX, "cpX");
            this.cpX.Name = "cpX";
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // gbxRot
            // 
            this.gbxRot.Controls.Add(this.btnInsertRot);
            this.gbxRot.Controls.Add(this.btnSendRot);
            this.gbxRot.Controls.Add(this.label4);
            this.gbxRot.Controls.Add(this.tbRot);
            this.gbxRot.Controls.Add(this.label3);
            this.gbxRot.Controls.Add(this.cmbCharaIdxRot);
            resources.ApplyResources(this.gbxRot, "gbxRot");
            this.gbxRot.Name = "gbxRot";
            this.gbxRot.TabStop = false;
            // 
            // btnInsertRot
            // 
            resources.ApplyResources(this.btnInsertRot, "btnInsertRot");
            this.btnInsertRot.Name = "btnInsertRot";
            this.btnInsertRot.UseVisualStyleBackColor = true;
            this.btnInsertRot.Click += new System.EventHandler(this.btnInsertRot_Click);
            // 
            // btnSendRot
            // 
            resources.ApplyResources(this.btnSendRot, "btnSendRot");
            this.btnSendRot.Name = "btnSendRot";
            this.btnSendRot.UseVisualStyleBackColor = true;
            this.btnSendRot.Click += new System.EventHandler(this.btnSendRot_Click);
            // 
            // label4
            // 
            resources.ApplyResources(this.label4, "label4");
            this.label4.Name = "label4";
            // 
            // tbRot
            // 
            resources.ApplyResources(this.tbRot, "tbRot");
            this.tbRot.Name = "tbRot";
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.label3.Name = "label3";
            // 
            // cmbCharaIdxRot
            // 
            this.cmbCharaIdxRot.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbCharaIdxRot.FormattingEnabled = true;
            this.cmbCharaIdxRot.Items.AddRange(new object[] {
            resources.GetString("cmbCharaIdxRot.Items"),
            resources.GetString("cmbCharaIdxRot.Items1"),
            resources.GetString("cmbCharaIdxRot.Items2")});
            resources.ApplyResources(this.cmbCharaIdxRot, "cmbCharaIdxRot");
            this.cmbCharaIdxRot.Name = "cmbCharaIdxRot";
            // 
            // frmCharaMover
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.gbxRot);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.gbxMove);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.KeyPreview = true;
            this.Name = "frmCharaMover";
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Load += new System.EventHandler(this.frmCharaMover_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.frmCharaMover_KeyDown);
            this.gbxMove.ResumeLayout(false);
            this.gbxMove.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.gbxRot.ResumeLayout(false);
            this.gbxRot.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox gbxMove;
        private System.Windows.Forms.Button btnFetchPos;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cmbCharaIdxMove;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.TextBox cpZ;
        private System.Windows.Forms.TextBox cpY;
        private System.Windows.Forms.TextBox cpX;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnSetPos;
        private System.Windows.Forms.GroupBox gbxRot;
        private System.Windows.Forms.Button btnSendRot;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox tbRot;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox cmbCharaIdxRot;
        private System.Windows.Forms.Button btnInsertMove;
        private System.Windows.Forms.Button btnInsertRot;
    }
}