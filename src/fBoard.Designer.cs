namespace cubes {
  partial class fBoard {
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
      this.components = new System.ComponentModel.Container();
      this.eX = new System.Windows.Forms.TextBox();
      this.eY = new System.Windows.Forms.TextBox();
      this.eZ = new System.Windows.Forms.TextBox();
      this.bNew = new System.Windows.Forms.Button();
      this.bCube = new System.Windows.Forms.Button();
      this.bPaste = new System.Windows.Forms.Button();
      this.tabControl1 = new System.Windows.Forms.TabControl();
      this.tp1 = new System.Windows.Forms.TabPage();
      this.vNewBytes = new System.Windows.Forms.TextBox();
      this.Bytes = new System.Windows.Forms.Label();
      this.vNewCubes = new System.Windows.Forms.TextBox();
      this.lNewCubes = new System.Windows.Forms.Label();
      this.lNew = new System.Windows.Forms.Label();
      this.tp2 = new System.Windows.Forms.TabPage();
      this.bNot = new System.Windows.Forms.Button();
      this.bBoundary = new System.Windows.Forms.Button();
      this.bImpand = new System.Windows.Forms.Button();
      this.bExpand = new System.Windows.Forms.Button();
      this.lSum = new System.Windows.Forms.Label();
      this.lX = new System.Windows.Forms.Label();
      this.gMode = new System.Windows.Forms.GroupBox();
      this.rb27 = new System.Windows.Forms.RadioButton();
      this.rb19 = new System.Windows.Forms.RadioButton();
      this.rb7 = new System.Windows.Forms.RadioButton();
      this.bApply = new System.Windows.Forms.Button();
      this.eCount = new System.Windows.Forms.TextBox();
      this.eRule = new System.Windows.Forms.TextBox();
      this.toolTip = new System.Windows.Forms.ToolTip(this.components);
      this.tabControl1.SuspendLayout();
      this.tp1.SuspendLayout();
      this.tp2.SuspendLayout();
      this.gMode.SuspendLayout();
      this.SuspendLayout();
      // 
      // eX
      // 
      this.eX.Location = new System.Drawing.Point(46, 6);
      this.eX.Name = "eX";
      this.eX.Size = new System.Drawing.Size(59, 20);
      this.eX.TabIndex = 0;
      this.eX.Text = "32";
      this.eX.TextChanged += new System.EventHandler(this.eXYZ_TextChanged);
      // 
      // eY
      // 
      this.eY.Location = new System.Drawing.Point(119, 6);
      this.eY.Name = "eY";
      this.eY.Size = new System.Drawing.Size(59, 20);
      this.eY.TabIndex = 1;
      this.eY.Text = "32";
      this.eY.TextChanged += new System.EventHandler(this.eXYZ_TextChanged);
      // 
      // eZ
      // 
      this.eZ.Location = new System.Drawing.Point(192, 6);
      this.eZ.Name = "eZ";
      this.eZ.Size = new System.Drawing.Size(67, 20);
      this.eZ.TabIndex = 2;
      this.eZ.Text = "32";
      this.eZ.TextChanged += new System.EventHandler(this.eXYZ_TextChanged);
      // 
      // bNew
      // 
      this.bNew.Location = new System.Drawing.Point(192, 32);
      this.bNew.Name = "bNew";
      this.bNew.Size = new System.Drawing.Size(67, 21);
      this.bNew.TabIndex = 3;
      this.bNew.Text = "&New";
      this.toolTip.SetToolTip(this.bNew, "New cubes");
      this.bNew.UseVisualStyleBackColor = true;
      this.bNew.Click += new System.EventHandler(this.bNew_Click);
      // 
      // bCube
      // 
      this.bCube.Location = new System.Drawing.Point(46, 32);
      this.bCube.Name = "bCube";
      this.bCube.Size = new System.Drawing.Size(59, 21);
      this.bCube.TabIndex = 4;
      this.bCube.Text = "&Cube";
      this.toolTip.SetToolTip(this.bCube, "New cube of cubes from first size field");
      this.bCube.UseVisualStyleBackColor = true;
      this.bCube.Click += new System.EventHandler(this.bCube_Click);
      // 
      // bPaste
      // 
      this.bPaste.Location = new System.Drawing.Point(119, 32);
      this.bPaste.Name = "bPaste";
      this.bPaste.Size = new System.Drawing.Size(59, 21);
      this.bPaste.TabIndex = 5;
      this.bPaste.Text = "&Paste";
      this.toolTip.SetToolTip(this.bPaste, "Create from clipboard");
      this.bPaste.UseVisualStyleBackColor = true;
      this.bPaste.Click += new System.EventHandler(this.bPaste_Click);
      // 
      // tabControl1
      // 
      this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.tabControl1.Controls.Add(this.tp1);
      this.tabControl1.Controls.Add(this.tp2);
      this.tabControl1.Location = new System.Drawing.Point(0, 0);
      this.tabControl1.Name = "tabControl1";
      this.tabControl1.SelectedIndex = 0;
      this.tabControl1.Size = new System.Drawing.Size(397, 179);
      this.tabControl1.TabIndex = 6;
      // 
      // tp1
      // 
      this.tp1.BackColor = System.Drawing.SystemColors.Control;
      this.tp1.Controls.Add(this.vNewBytes);
      this.tp1.Controls.Add(this.Bytes);
      this.tp1.Controls.Add(this.vNewCubes);
      this.tp1.Controls.Add(this.lNewCubes);
      this.tp1.Controls.Add(this.lNew);
      this.tp1.Controls.Add(this.eY);
      this.tp1.Controls.Add(this.bPaste);
      this.tp1.Controls.Add(this.eX);
      this.tp1.Controls.Add(this.bCube);
      this.tp1.Controls.Add(this.eZ);
      this.tp1.Controls.Add(this.bNew);
      this.tp1.Location = new System.Drawing.Point(4, 22);
      this.tp1.Name = "tp1";
      this.tp1.Padding = new System.Windows.Forms.Padding(3);
      this.tp1.Size = new System.Drawing.Size(389, 153);
      this.tp1.TabIndex = 0;
      this.tp1.Text = "File";
      // 
      // vNewBytes
      // 
      this.vNewBytes.Location = new System.Drawing.Point(306, 29);
      this.vNewBytes.Name = "vNewBytes";
      this.vNewBytes.ReadOnly = true;
      this.vNewBytes.Size = new System.Drawing.Size(76, 20);
      this.vNewBytes.TabIndex = 12;
      this.vNewBytes.Text = "32";
      this.toolTip.SetToolTip(this.vNewBytes, "Reguired memory bytes");
      // 
      // Bytes
      // 
      this.Bytes.AutoSize = true;
      this.Bytes.Location = new System.Drawing.Point(265, 32);
      this.Bytes.Name = "Bytes";
      this.Bytes.Size = new System.Drawing.Size(36, 13);
      this.Bytes.TabIndex = 11;
      this.Bytes.Text = "Bytes:";
      // 
      // vNewCubes
      // 
      this.vNewCubes.Location = new System.Drawing.Point(306, 5);
      this.vNewCubes.Name = "vNewCubes";
      this.vNewCubes.ReadOnly = true;
      this.vNewCubes.Size = new System.Drawing.Size(76, 20);
      this.vNewCubes.TabIndex = 10;
      this.vNewCubes.Text = "32";
      this.toolTip.SetToolTip(this.vNewCubes, "Cubes count");
      // 
      // lNewCubes
      // 
      this.lNewCubes.AutoSize = true;
      this.lNewCubes.Location = new System.Drawing.Point(265, 9);
      this.lNewCubes.Name = "lNewCubes";
      this.lNewCubes.Size = new System.Drawing.Size(40, 13);
      this.lNewCubes.TabIndex = 9;
      this.lNewCubes.Text = "Cubes:";
      // 
      // lNew
      // 
      this.lNew.AutoSize = true;
      this.lNew.Location = new System.Drawing.Point(6, 9);
      this.lNew.Name = "lNew";
      this.lNew.Size = new System.Drawing.Size(29, 13);
      this.lNew.TabIndex = 8;
      this.lNew.Text = "New";
      // 
      // tp2
      // 
      this.tp2.BackColor = System.Drawing.SystemColors.Control;
      this.tp2.Controls.Add(this.bNot);
      this.tp2.Controls.Add(this.bBoundary);
      this.tp2.Controls.Add(this.bImpand);
      this.tp2.Controls.Add(this.bExpand);
      this.tp2.Controls.Add(this.lSum);
      this.tp2.Controls.Add(this.lX);
      this.tp2.Controls.Add(this.gMode);
      this.tp2.Controls.Add(this.bApply);
      this.tp2.Controls.Add(this.eCount);
      this.tp2.Controls.Add(this.eRule);
      this.tp2.Location = new System.Drawing.Point(4, 22);
      this.tp2.Name = "tp2";
      this.tp2.Padding = new System.Windows.Forms.Padding(3);
      this.tp2.Size = new System.Drawing.Size(389, 153);
      this.tp2.TabIndex = 1;
      this.tp2.Text = "Filter";
      // 
      // bNot
      // 
      this.bNot.Image = global::cubes.Properties.Resources.not;
      this.bNot.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
      this.bNot.Location = new System.Drawing.Point(8, 16);
      this.bNot.Name = "bNot";
      this.bNot.Size = new System.Drawing.Size(70, 24);
      this.bNot.TabIndex = 19;
      this.bNot.Tag = "not";
      this.bNot.Text = "&Not";
      this.toolTip.SetToolTip(this.bNot, "Invert selection");
      this.bNot.UseVisualStyleBackColor = true;
      this.bNot.Click += new System.EventHandler(this.Cmd);
      // 
      // bBoundary
      // 
      this.bBoundary.Image = global::cubes.Properties.Resources.boundary;
      this.bBoundary.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
      this.bBoundary.Location = new System.Drawing.Point(140, 16);
      this.bBoundary.Name = "bBoundary";
      this.bBoundary.Size = new System.Drawing.Size(82, 24);
      this.bBoundary.TabIndex = 18;
      this.bBoundary.Tag = "boundary";
      this.bBoundary.Text = "&Boundary";
      this.bBoundary.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      this.toolTip.SetToolTip(this.bBoundary, "Leave just boundary cubes");
      this.bBoundary.UseVisualStyleBackColor = true;
      this.bBoundary.Click += new System.EventHandler(this.Cmd);
      // 
      // bImpand
      // 
      this.bImpand.Image = global::cubes.Properties.Resources.impand;
      this.bImpand.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
      this.bImpand.Location = new System.Drawing.Point(229, 16);
      this.bImpand.Name = "bImpand";
      this.bImpand.Size = new System.Drawing.Size(67, 24);
      this.bImpand.TabIndex = 17;
      this.bImpand.Tag = "impand";
      this.bImpand.Text = "&Impand";
      this.bImpand.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      this.toolTip.SetToolTip(this.bImpand, "Remove boundary cubes");
      this.bImpand.UseVisualStyleBackColor = true;
      this.bImpand.Click += new System.EventHandler(this.Cmd);
      // 
      // bExpand
      // 
      this.bExpand.Image = global::cubes.Properties.Resources.expand;
      this.bExpand.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
      this.bExpand.Location = new System.Drawing.Point(303, 16);
      this.bExpand.Name = "bExpand";
      this.bExpand.Size = new System.Drawing.Size(69, 24);
      this.bExpand.TabIndex = 16;
      this.bExpand.Tag = "expand";
      this.bExpand.Text = "&Expand";
      this.bExpand.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      this.toolTip.SetToolTip(this.bExpand, "Add boundary cubes");
      this.bExpand.UseVisualStyleBackColor = true;
      this.bExpand.Click += new System.EventHandler(this.Cmd);
      // 
      // lSum
      // 
      this.lSum.AutoSize = true;
      this.lSum.Location = new System.Drawing.Point(58, 132);
      this.lSum.Name = "lSum";
      this.lSum.Size = new System.Drawing.Size(28, 13);
      this.lSum.TabIndex = 15;
      this.lSum.Text = "Sum";
      // 
      // lX
      // 
      this.lX.AutoSize = true;
      this.lX.Location = new System.Drawing.Point(248, 132);
      this.lX.Name = "lX";
      this.lX.Size = new System.Drawing.Size(14, 13);
      this.lX.TabIndex = 14;
      this.lX.Text = "X";
      // 
      // gMode
      // 
      this.gMode.Controls.Add(this.rb27);
      this.gMode.Controls.Add(this.rb19);
      this.gMode.Controls.Add(this.rb7);
      this.gMode.Location = new System.Drawing.Point(96, 61);
      this.gMode.Name = "gMode";
      this.gMode.Size = new System.Drawing.Size(200, 42);
      this.gMode.TabIndex = 13;
      this.gMode.TabStop = false;
      this.gMode.Text = "Mode";
      // 
      // rb27
      // 
      this.rb27.Image = global::cubes.Properties.Resources.x27;
      this.rb27.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
      this.rb27.Location = new System.Drawing.Point(133, 13);
      this.rb27.Name = "rb27";
      this.rb27.Size = new System.Drawing.Size(54, 24);
      this.rb27.TabIndex = 2;
      this.rb27.Text = "27";
      this.rb27.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      this.toolTip.SetToolTip(this.rb27, "Full 3x3x3 box");
      this.rb27.UseVisualStyleBackColor = true;
      // 
      // rb19
      // 
      this.rb19.Image = global::cubes.Properties.Resources.x19;
      this.rb19.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
      this.rb19.Location = new System.Drawing.Point(67, 13);
      this.rb19.Name = "rb19";
      this.rb19.Size = new System.Drawing.Size(54, 24);
      this.rb19.TabIndex = 1;
      this.rb19.Text = "19";
      this.rb19.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      this.toolTip.SetToolTip(this.rb19, "Box without corners");
      this.rb19.UseVisualStyleBackColor = true;
      // 
      // rb7
      // 
      this.rb7.Checked = true;
      this.rb7.Image = global::cubes.Properties.Resources.x7;
      this.rb7.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
      this.rb7.Location = new System.Drawing.Point(6, 13);
      this.rb7.Name = "rb7";
      this.rb7.Size = new System.Drawing.Size(48, 24);
      this.rb7.TabIndex = 0;
      this.rb7.TabStop = true;
      this.rb7.Text = "7";
      this.rb7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      this.toolTip.SetToolTip(this.rb7, "Just side cubes");
      this.rb7.UseVisualStyleBackColor = true;
      // 
      // bApply
      // 
      this.bApply.Location = new System.Drawing.Point(316, 129);
      this.bApply.Name = "bApply";
      this.bApply.Size = new System.Drawing.Size(67, 21);
      this.bApply.TabIndex = 12;
      this.bApply.Tag = "sum";
      this.bApply.Text = "&Sum";
      this.toolTip.SetToolTip(this.bApply, "Apply sum filter");
      this.bApply.UseVisualStyleBackColor = true;
      this.bApply.Click += new System.EventHandler(this.bSumApply_Click);
      // 
      // eCount
      // 
      this.eCount.Location = new System.Drawing.Point(268, 129);
      this.eCount.Name = "eCount";
      this.eCount.Size = new System.Drawing.Size(37, 20);
      this.eCount.TabIndex = 9;
      this.eCount.Text = "1";
      this.eCount.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
      this.toolTip.SetToolTip(this.eCount, "Repeat count for sum filter");
      // 
      // eRule
      // 
      this.eRule.Location = new System.Drawing.Point(92, 129);
      this.eRule.Name = "eRule";
      this.eRule.Size = new System.Drawing.Size(150, 20);
      this.eRule.TabIndex = 8;
      this.eRule.Text = ".1-e";
      this.toolTip.SetToolTip(this.eRule, "Sum rule");
      // 
      // fBoard
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(397, 179);
      this.Controls.Add(this.tabControl1);
      this.Name = "fBoard";
      this.ShowInTaskbar = false;
      this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
      this.Text = "Board";
      this.tabControl1.ResumeLayout(false);
      this.tp1.ResumeLayout(false);
      this.tp1.PerformLayout();
      this.tp2.ResumeLayout(false);
      this.tp2.PerformLayout();
      this.gMode.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.TextBox eX;
    private System.Windows.Forms.TextBox eY;
    private System.Windows.Forms.TextBox eZ;
    private System.Windows.Forms.Button bNew;
    private System.Windows.Forms.Button bCube;
    private System.Windows.Forms.Button bPaste;
    private System.Windows.Forms.TabControl tabControl1;
    private System.Windows.Forms.TabPage tp1;
    private System.Windows.Forms.TabPage tp2;
    private System.Windows.Forms.Label lNew;
    private System.Windows.Forms.TextBox vNewCubes;
    private System.Windows.Forms.Label lNewCubes;
    private System.Windows.Forms.TextBox vNewBytes;
    private System.Windows.Forms.Label Bytes;
    private System.Windows.Forms.Label lX;
    private System.Windows.Forms.GroupBox gMode;
    private System.Windows.Forms.RadioButton rb27;
    private System.Windows.Forms.RadioButton rb19;
    private System.Windows.Forms.RadioButton rb7;
    private System.Windows.Forms.Button bApply;
    private System.Windows.Forms.Button bExpand;
    private System.Windows.Forms.Label lSum;
    private System.Windows.Forms.Button bNot;
    private System.Windows.Forms.Button bBoundary;
    private System.Windows.Forms.Button bImpand;
    public System.Windows.Forms.TextBox eCount;
    public System.Windows.Forms.TextBox eRule;
    private System.Windows.Forms.ToolTip toolTip;
  }
}