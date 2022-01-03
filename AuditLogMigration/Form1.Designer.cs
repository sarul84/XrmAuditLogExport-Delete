
namespace AuditLogMigration
{
    partial class Form1
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
            this.components = new System.ComponentModel.Container();
            this.label2 = new System.Windows.Forms.Label();
            this.txtUserName = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.OrgLabel = new System.Windows.Forms.Label();
            this.cbxorganization = new System.Windows.Forms.ComboBox();
            this.btnLogin = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnEntityLoad = new System.Windows.Forms.Button();
            this.btnConvert = new System.Windows.Forms.Button();
            this.errorProvider1 = new System.Windows.Forms.ErrorProvider(this.components);
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label7 = new System.Windows.Forms.Label();
            this.txtSearch = new System.Windows.Forms.TextBox();
            this.dgEntityList = new System.Windows.Forms.DataGridView();
            this.SelectedEntity = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.TableName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.SchemaName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.TypeCode = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.button3 = new System.Windows.Forms.Button();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.cbxAuditActionType = new System.Windows.Forms.ComboBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.button2 = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.lblAction = new System.Windows.Forms.Label();
            this.dtpToDate = new System.Windows.Forms.DateTimePicker();
            this.label4 = new System.Windows.Forms.Label();
            this.dtpFromDate = new System.Windows.Forms.DateTimePicker();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).BeginInit();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgEntityList)).BeginInit();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(60, 16);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(32, 13);
            this.label2.TabIndex = 8;
            this.label2.Text = "Email";
            // 
            // txtUserName
            // 
            this.txtUserName.Location = new System.Drawing.Point(5, 33);
            this.txtUserName.Margin = new System.Windows.Forms.Padding(2);
            this.txtUserName.Name = "txtUserName";
            this.txtUserName.Size = new System.Drawing.Size(142, 20);
            this.txtUserName.TabIndex = 1;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(50, 61);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Password";
            // 
            // txtPassword
            // 
            this.txtPassword.Location = new System.Drawing.Point(5, 78);
            this.txtPassword.Margin = new System.Windows.Forms.Padding(2);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.PasswordChar = '*';
            this.txtPassword.Size = new System.Drawing.Size(142, 20);
            this.txtPassword.TabIndex = 2;
            // 
            // OrgLabel
            // 
            this.OrgLabel.AutoSize = true;
            this.OrgLabel.Location = new System.Drawing.Point(43, 108);
            this.OrgLabel.Name = "OrgLabel";
            this.OrgLabel.Size = new System.Drawing.Size(66, 13);
            this.OrgLabel.TabIndex = 6;
            this.OrgLabel.Text = "Organization";
            // 
            // cbxorganization
            // 
            this.cbxorganization.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F);
            this.cbxorganization.FormattingEnabled = true;
            this.cbxorganization.Location = new System.Drawing.Point(5, 125);
            this.cbxorganization.Margin = new System.Windows.Forms.Padding(4);
            this.cbxorganization.Name = "cbxorganization";
            this.cbxorganization.Size = new System.Drawing.Size(142, 24);
            this.cbxorganization.TabIndex = 10;
            // 
            // btnLogin
            // 
            this.btnLogin.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.btnLogin.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnLogin.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F);
            this.btnLogin.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnLogin.Location = new System.Drawing.Point(14, 166);
            this.btnLogin.Name = "btnLogin";
            this.btnLogin.Size = new System.Drawing.Size(121, 27);
            this.btnLogin.TabIndex = 3;
            this.btnLogin.Text = "Login";
            this.btnLogin.UseVisualStyleBackColor = false;
            this.btnLogin.Click += new System.EventHandler(this.btnLogin_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btnEntityLoad);
            this.groupBox1.Controls.Add(this.cbxorganization);
            this.groupBox1.Controls.Add(this.btnLogin);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.txtUserName);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.txtPassword);
            this.groupBox1.Controls.Add(this.btnConvert);
            this.groupBox1.Controls.Add(this.OrgLabel);
            this.groupBox1.Location = new System.Drawing.Point(4, 9);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(153, 250);
            this.groupBox1.TabIndex = 9;
            this.groupBox1.TabStop = false;
            // 
            // btnEntityLoad
            // 
            this.btnEntityLoad.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.btnEntityLoad.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnEntityLoad.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F);
            this.btnEntityLoad.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnEntityLoad.Location = new System.Drawing.Point(14, 212);
            this.btnEntityLoad.Name = "btnEntityLoad";
            this.btnEntityLoad.Size = new System.Drawing.Size(121, 27);
            this.btnEntityLoad.TabIndex = 12;
            this.btnEntityLoad.Text = "Load Entities";
            this.btnEntityLoad.UseVisualStyleBackColor = false;
            this.btnEntityLoad.Click += new System.EventHandler(this.btnEntityLoad_Click);
            // 
            // btnConvert
            // 
            this.btnConvert.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.btnConvert.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnConvert.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F);
            this.btnConvert.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnConvert.Location = new System.Drawing.Point(976, 319);
            this.btnConvert.Name = "btnConvert";
            this.btnConvert.Size = new System.Drawing.Size(118, 27);
            this.btnConvert.TabIndex = 5;
            this.btnConvert.Text = "Validator JSON";
            this.btnConvert.UseVisualStyleBackColor = false;
            // 
            // errorProvider1
            // 
            this.errorProvider1.BlinkStyle = System.Windows.Forms.ErrorBlinkStyle.NeverBlink;
            this.errorProvider1.ContainerControl = this;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label7);
            this.groupBox2.Controls.Add(this.txtSearch);
            this.groupBox2.Controls.Add(this.dgEntityList);
            this.groupBox2.Location = new System.Drawing.Point(169, 9);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(367, 392);
            this.groupBox2.TabIndex = 12;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Entities";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(9, 20);
            this.label7.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(117, 13);
            this.label7.TabIndex = 13;
            this.label7.Text = "Search By Table Name";
            // 
            // txtSearch
            // 
            this.txtSearch.Location = new System.Drawing.Point(136, 17);
            this.txtSearch.Margin = new System.Windows.Forms.Padding(2);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(226, 20);
            this.txtSearch.TabIndex = 13;
            this.txtSearch.TextChanged += new System.EventHandler(this.txtSearch_TextChanged);
            // 
            // dgEntityList
            // 
            this.dgEntityList.AllowUserToAddRows = false;
            this.dgEntityList.AllowUserToDeleteRows = false;
            this.dgEntityList.AllowUserToResizeRows = false;
            this.dgEntityList.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgEntityList.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.SelectedEntity,
            this.TableName,
            this.SchemaName,
            this.TypeCode});
            this.dgEntityList.Location = new System.Drawing.Point(5, 48);
            this.dgEntityList.Name = "dgEntityList";
            this.dgEntityList.RowHeadersVisible = false;
            this.dgEntityList.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgEntityList.Size = new System.Drawing.Size(357, 337);
            this.dgEntityList.TabIndex = 0;
            // 
            // SelectedEntity
            // 
            this.SelectedEntity.DataPropertyName = "SelectedEntity";
            this.SelectedEntity.HeaderText = "Select";
            this.SelectedEntity.Name = "SelectedEntity";
            this.SelectedEntity.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.SelectedEntity.ToolTipText = "Select record to export audit data";
            this.SelectedEntity.Width = 40;
            // 
            // TableName
            // 
            this.TableName.DataPropertyName = "TableName";
            this.TableName.HeaderText = "Table Name";
            this.TableName.Name = "TableName";
            this.TableName.ReadOnly = true;
            this.TableName.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.TableName.ToolTipText = "Entity / Table Name";
            this.TableName.Width = 200;
            // 
            // SchemaName
            // 
            this.SchemaName.HeaderText = "Schema Name";
            this.SchemaName.Name = "SchemaName";
            this.SchemaName.ReadOnly = true;
            this.SchemaName.Width = 120;
            // 
            // TypeCode
            // 
            this.TypeCode.HeaderText = "Object Type Code";
            this.TypeCode.Name = "TypeCode";
            this.TypeCode.ReadOnly = true;
            this.TypeCode.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.TypeCode.Visible = false;
            // 
            // button3
            // 
            this.button3.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.button3.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.button3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F);
            this.button3.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.button3.Location = new System.Drawing.Point(59, 270);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(121, 27);
            this.button3.TabIndex = 13;
            this.button3.Text = "Start";
            this.button3.UseVisualStyleBackColor = false;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.cbxAuditActionType);
            this.groupBox3.Controls.Add(this.textBox1);
            this.groupBox3.Controls.Add(this.label5);
            this.groupBox3.Controls.Add(this.comboBox1);
            this.groupBox3.Controls.Add(this.button2);
            this.groupBox3.Controls.Add(this.label6);
            this.groupBox3.Controls.Add(this.lblAction);
            this.groupBox3.Controls.Add(this.button3);
            this.groupBox3.Controls.Add(this.dtpToDate);
            this.groupBox3.Controls.Add(this.label4);
            this.groupBox3.Controls.Add(this.dtpFromDate);
            this.groupBox3.Controls.Add(this.label1);
            this.groupBox3.Location = new System.Drawing.Point(552, 9);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(231, 385);
            this.groupBox3.TabIndex = 13;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Audit Export && Purging";
            // 
            // cbxAuditActionType
            // 
            this.cbxAuditActionType.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F);
            this.cbxAuditActionType.FormattingEnabled = true;
            this.cbxAuditActionType.Location = new System.Drawing.Point(15, 41);
            this.cbxAuditActionType.Margin = new System.Windows.Forms.Padding(4);
            this.cbxAuditActionType.Name = "cbxAuditActionType";
            this.cbxAuditActionType.Size = new System.Drawing.Size(200, 24);
            this.cbxAuditActionType.TabIndex = 21;
            this.cbxAuditActionType.SelectedIndexChanged += new System.EventHandler(this.cbxAuditActionType_SelectedIndexChanged);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(10, 233);
            this.textBox1.Margin = new System.Windows.Forms.Padding(2);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(178, 20);
            this.textBox1.TabIndex = 20;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(88, 217);
            this.label5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(48, 13);
            this.label5.TabIndex = 19;
            this.label5.Text = "File Path";
            // 
            // comboBox1
            // 
            this.comboBox1.Enabled = false;
            this.comboBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F);
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Items.AddRange(new object[] {
            "Excel",
            "Database"});
            this.comboBox1.Location = new System.Drawing.Point(10, 183);
            this.comboBox1.Margin = new System.Windows.Forms.Padding(4);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(209, 24);
            this.comboBox1.TabIndex = 17;
            this.comboBox1.Text = "Database";
            // 
            // button2
            // 
            this.button2.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.button2.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.button2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F);
            this.button2.Location = new System.Drawing.Point(193, 233);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(30, 22);
            this.button2.TabIndex = 18;
            this.button2.Text = "...";
            this.button2.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.button2.UseVisualStyleBackColor = false;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(56, 165);
            this.label6.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(121, 13);
            this.label6.TabIndex = 16;
            this.label6.Text = "Audit Reference Source";
            // 
            // lblAction
            // 
            this.lblAction.AutoSize = true;
            this.lblAction.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAction.Location = new System.Drawing.Point(83, 24);
            this.lblAction.Name = "lblAction";
            this.lblAction.Size = new System.Drawing.Size(64, 13);
            this.lblAction.TabIndex = 0;
            this.lblAction.Text = "Audit Action";
            // 
            // dtpToDate
            // 
            this.dtpToDate.CustomFormat = "MM/dd/yyyy";
            this.dtpToDate.Location = new System.Drawing.Point(15, 136);
            this.dtpToDate.Name = "dtpToDate";
            this.dtpToDate.Size = new System.Drawing.Size(200, 20);
            this.dtpToDate.TabIndex = 12;
            this.dtpToDate.Value = new System.DateTime(2021, 11, 25, 23, 59, 0, 0);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(89, 120);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(52, 13);
            this.label4.TabIndex = 11;
            this.label4.Text = "End Date";
            // 
            // dtpFromDate
            // 
            this.dtpFromDate.CustomFormat = "MM/dd/yyyy hh:mm:ss";
            this.dtpFromDate.Location = new System.Drawing.Point(15, 91);
            this.dtpFromDate.Name = "dtpFromDate";
            this.dtpFromDate.Size = new System.Drawing.Size(200, 20);
            this.dtpFromDate.TabIndex = 10;
            this.dtpFromDate.Value = new System.DateTime(2021, 11, 25, 0, 0, 0, 0);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(88, 75);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(55, 13);
            this.label1.TabIndex = 9;
            this.label1.Text = "Start Date";
            // 
            // groupBox5
            // 
            this.groupBox5.Location = new System.Drawing.Point(4, 265);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(153, 129);
            this.groupBox5.TabIndex = 14;
            this.groupBox5.TabStop = false;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(795, 413);
            this.Controls.Add(this.groupBox5);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.Text = "Xrm Audit Utility";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgEntityList)).EndInit();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtUserName;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.Label OrgLabel;
        private System.Windows.Forms.ComboBox cbxorganization;
        private System.Windows.Forms.Button btnLogin;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btnConvert;
        private System.Windows.Forms.ErrorProvider errorProvider1;
        private System.Windows.Forms.Button btnEntityLoad;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.DataGridView dgEntityList;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.DateTimePicker dtpToDate;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.DateTimePicker dtpFromDate;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtSearch;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.DataGridViewCheckBoxColumn SelectedEntity;
        private System.Windows.Forms.DataGridViewTextBoxColumn TableName;
        private System.Windows.Forms.DataGridViewTextBoxColumn SchemaName;
        private System.Windows.Forms.DataGridViewTextBoxColumn TypeCode;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.Label lblAction;
        private System.Windows.Forms.ComboBox cbxAuditActionType;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Label label6;
    }
}

