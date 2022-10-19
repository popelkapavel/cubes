namespace cubes {
  partial class fFilterSum {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components=null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing) {
      if(disposing&&(components!=null)) {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent() {
      this.eRule = new System.Windows.Forms.TextBox();
      this.eCount = new System.Windows.Forms.TextBox();
      this.bOK = new System.Windows.Forms.Button();
      this.bCancel = new System.Windows.Forms.Button();
      this.bApply = new System.Windows.Forms.Button();
      this.gMode = new System.Windows.Forms.GroupBox();
      this.rb27 = new System.Windows.Forms.RadioButton();
      this.rb19 = new System.Windows.Forms.RadioButton();
      this.rb7 = new System.Windows.Forms.RadioButton();
      this.lX = new System.Windows.Forms.Label();
      this.gMode.SuspendLayout();
      this.SuspendLayout();
      // 
      // eRule
      // 
      this.eRule.Location = new System.Drawing.Point(12, 12);
      this.eRule.Name = "eRule";
      this.eRule.Size = new System.Drawing.Size(150, 20);
      this.eRule.TabIndex = 0;
      this.eRule.Text = ".1-e";
      // 
      // eCount
      // 
      this.eCount.Location = new System.Drawing.Point(188, 12);
      this.eCount.Name = "eCount";
      this.eCount.Size = new System.Drawing.Size(37, 20);
      this.eCount.TabIndex = 1;
      this.eCount.Text = "1";
      this.eCount.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
      // 
      // bOK
      // 
      this.bOK.Location = new System.Drawing.Point(158, 86);
      this.bOK.Name = "bOK";
      this.bOK.Size = new System.Drawing.Size(67, 21);
      this.bOK.TabIndex = 3;
      this.bOK.Text = "OK";
      this.bOK.UseVisualStyleBackColor = true;
      this.bOK.Click += new System.EventHandler(this.bOK_Click);
      // 
      // bCancel
      // 
      this.bCancel.Location = new System.Drawing.Point(93, 86);
      this.bCancel.Name = "bCancel";
      this.bCancel.Size = new System.Drawing.Size(59, 21);
      this.bCancel.TabIndex = 4;
      this.bCancel.Text = "Cancel";
      this.bCancel.UseVisualStyleBackColor = true;
      this.bCancel.Click += new System.EventHandler(this.bCancel_Click);
      // 
      // bApply
      // 
      this.bApply.Location = new System.Drawing.Point(12, 86);
      this.bApply.Name = "bApply";
      this.bApply.Size = new System.Drawing.Size(67, 21);
      this.bApply.TabIndex = 5;
      this.bApply.Text = "&Apply";
      this.bApply.UseVisualStyleBackColor = true;
      this.bApply.Click += new System.EventHandler(this.bApply_Click);
      // 
      // gMode
      // 
      this.gMode.Controls.Add(this.rb27);
      this.gMode.Controls.Add(this.rb19);
      this.gMode.Controls.Add(this.rb7);
      this.gMode.Location = new System.Drawing.Point(16, 38);
      this.gMode.Name = "gMode";
      this.gMode.Size = new System.Drawing.Size(200, 42);
      this.gMode.TabIndex = 6;
      this.gMode.TabStop = false;
      this.gMode.Text = "Mode";
      // 
      // rb27
      // 
      this.rb27.AutoSize = true;
      this.rb27.Location = new System.Drawing.Point(105, 19);
      this.rb27.Name = "rb27";
      this.rb27.Size = new System.Drawing.Size(37, 17);
      this.rb27.TabIndex = 2;
      this.rb27.Text = "27";
      this.rb27.UseVisualStyleBackColor = true;
      // 
      // rb19
      // 
      this.rb19.AutoSize = true;
      this.rb19.Location = new System.Drawing.Point(57, 19);
      this.rb19.Name = "rb19";
      this.rb19.Size = new System.Drawing.Size(37, 17);
      this.rb19.TabIndex = 1;
      this.rb19.Text = "19";
      this.rb19.UseVisualStyleBackColor = true;
      // 
      // rb7
      // 
      this.rb7.AutoSize = true;
      this.rb7.Checked = true;
      this.rb7.Location = new System.Drawing.Point(6, 19);
      this.rb7.Name = "rb7";
      this.rb7.Size = new System.Drawing.Size(31, 17);
      this.rb7.TabIndex = 0;
      this.rb7.TabStop = true;
      this.rb7.Text = "7";
      this.rb7.UseVisualStyleBackColor = true;
      // 
      // lX
      // 
      this.lX.AutoSize = true;
      this.lX.Location = new System.Drawing.Point(168, 15);
      this.lX.Name = "lX";
      this.lX.Size = new System.Drawing.Size(14, 13);
      this.lX.TabIndex = 7;
      this.lX.Text = "X";
      // 
      // fFilterSum
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(237, 113);
      this.Controls.Add(this.lX);
      this.Controls.Add(this.gMode);
      this.Controls.Add(this.bApply);
      this.Controls.Add(this.bCancel);
      this.Controls.Add(this.bOK);
      this.Controls.Add(this.eCount);
      this.Controls.Add(this.eRule);
      this.Name = "fFilterSum";
      this.Text = "Sum filter";
      this.gMode.ResumeLayout(false);
      this.gMode.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.TextBox eRule;
    private System.Windows.Forms.TextBox eCount;
    private System.Windows.Forms.Button bOK;
    private System.Windows.Forms.Button bCancel;
    private System.Windows.Forms.Button bApply;
    private System.Windows.Forms.GroupBox gMode;
    private System.Windows.Forms.RadioButton rb27;
    private System.Windows.Forms.RadioButton rb19;
    private System.Windows.Forms.RadioButton rb7;
    private System.Windows.Forms.Label lX;
  }
}