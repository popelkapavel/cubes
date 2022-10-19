using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace cubes {
  public partial class fBoard:Form {
    fmain f;
    public fBoard(fmain f) {
      InitializeComponent();
      this.f=f;
      UpdateNew();
    }

    protected override void OnClosing(CancelEventArgs e) {
      e.Cancel=true;
      Hide();
    }

    public void Show(int panel) {
      if(panel>=0)
        tabControl1.SelectedIndex=panel;
      if(!Visible) {
        Point pt=Cursor.Position;
        Left=pt.X-Width/2;
				Top=pt.Y-Height/2;
				if(Left<0) Left=0;
				if(Top<0) Top=0;
				Rectangle ps=Screen.PrimaryScreen.Bounds;
				if(Left+Width>ps.Width) Left=ps.Width-Width;
				if(Top+Height>ps.Height) Top=ps.Height-Height;
      }
      Show(f);
    }

    private void bNew_Click(object sender,EventArgs e) {
      int x,y,z;
      if(int.TryParse(eX.Text,out x)&&int.TryParse(eY.Text,out y)&&int.TryParse(eZ.Text,out z)) 
        New(x,y,z);
    }

    private void bCube_Click(object sender,EventArgs e) {
      int x;
      if(int.TryParse(eX.Text,out x)) 
        New(x,x,x);
    }
    void New(int x,int y,int z) {    
      f.New(x,y,z,false);
      Hide();  
    }

    private void bPaste_Click(object sender, EventArgs e) {
      f.New(0,0,0,true);
      Hide();
    }

    private void eXYZ_TextChanged(object sender, EventArgs e) { UpdateNew();}
    void UpdateNew() {      
      int x,y,z,c,b;
      int.TryParse(eX.Text,out x);int.TryParse(eY.Text,out y);int.TryParse(eZ.Text,out z);
      if(x<0) x=1;if(y<1) y=1;if(z<1) z=1;
      c=x*y*z;
      b=4*((x+31)>>5)*y*z;
      vNewCubes.Text=c.ToString("#,#");
      vNewBytes.Text=b.ToString("#,#");
    }

    static int Chindex(char ch) {
      if(ch>='0'&&ch<='9') return ch-'0';
      else if(ch>='a'&&ch<='r') return ch-'a'+10;
      else if(ch>='A'&&ch<='R') return ch-'A'+10;
      return -1;
    }
    public static bool RuleParse(string s,out int rule) {
      rule=0;
      int mask=1,mode=0;
      for(int i=0;i<s.Length;) {
        char ch=s[i++];
        if(ch=='.') mode=1;
        else if(mode==0) {
          if(ch=='0'||ch=='1') {          
            if(ch=='1') rule|=mask;
            mask<<=1;
          }
        } else {
          int i1=Chindex(ch),i2;
          if(i1<0) continue;
          if(i+1<s.Length&&s[i]=='-'&&0<=(i2=Chindex(s[i+1]))) {
            rule^=((1<<i1)-1)^((1<<(i2+1))-1);
            i+=2;
            continue;
          }
          rule^=1<<i1;
        }
      }
      return true;
    }
    public int SumMode() {
      return rb7.Checked?0:rb19.Checked?1:2;
    }

    private void bCancel_Click(object sender,EventArgs e) { Hide(); }

    private void bSumApply_Click(object sender, EventArgs e) {
      Cmd(sender,e);
    }

    private void Cmd(object sender, EventArgs e) {
      f.Cmd(sender,e);
    }
    public void SetTab(int tab) {
      if(tab==tabControl1.SelectedIndex) Hide();
      tabControl1.SelectTab(tab);
    }

    static Control Focusing(Control x) {
      IContainerControl c=x as IContainerControl;
      while(c!=null) {
        x=c.ActiveControl;
        c=x as IContainerControl;
      }
      return x;
    }


    protected override bool ProcessCmdKey(ref Message msg,Keys keyData) {
      TextBox ftb;
      switch(keyData) {
  		 //case Keys.F1:f.ProcessCommand("help");return true;
			 case Keys.Escape:
       case Keys.Space:
			 case Keys.F10:Close();return true;
			 //case Keys.F11:Main.Fullscreen();return true;
       case Keys.A|Keys.Control:
         ftb=Focusing(this) as TextBox;
         if(ftb!=null) ftb.SelectAll();
         else f.SelectAll();
         return true;
       case Keys.Oem3|Keys.Alt:
       case Keys.Oem3|Keys.Control:SetTab(0);return true;
       case Keys.D1|Keys.Alt:
       case Keys.D1|Keys.Control:SetTab(1);return true;
       case Keys.D2|Keys.Alt:
       case Keys.D2|Keys.Control:SetTab(2);return true;
       case Keys.D3|Keys.Alt:
       case Keys.D3|Keys.Control:SetTab(3);return true;
       case Keys.D4|Keys.Alt:
       case Keys.D4|Keys.Control:SetTab(4);return true;
       case Keys.D5|Keys.Alt:
       case Keys.D5|Keys.Control:SetTab(5);return true;
       case Keys.D6|Keys.Alt:
       case Keys.D6|Keys.Control:SetTab(6);return true;
       case Keys.Z|Keys.Control:
         ftb=Focusing(this) as TextBox;
         if(ftb!=null) return false;
         f.Undo(false);
         break;
       case Keys.Y|Keys.Control:
         ftb=Focusing(this) as TextBox;
         if(ftb!=null) return false;
        f.Undo(true);
        break;
			}
			return false;
		}

  }
}
