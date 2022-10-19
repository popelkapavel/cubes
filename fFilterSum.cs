using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace cubes {
  public partial class fFilterSum:Form {
    fmain f;
    public fFilterSum(fmain f) {
      InitializeComponent();
      this.f=f;
    }

    private void bOK_Click(object sender,EventArgs e) {
      Apply();
      f.lazydraw = true;
      Close();
    }

    static int Chindex(char ch) {
      if(ch>='0'&&ch<='9') return ch-'0';
      else if(ch>='a'&&ch<='r') return ch-'a'+10;
      else if(ch>='A'&&ch<='R') return ch-'A'+10;
      return -1;
    }
    static bool RuleParse(string s,out int rule) {
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
    private void Apply() {
      int rule, count;
      if (RuleParse(eRule.Text, out rule) && rule!=0 && int.TryParse(eCount.Text, out count)) {
        if (count < 1) count = 1;
        int mode=rb7.Checked?1:rb19.Checked?2:0;
        f.b3d.sumfilter(0, rule, false, count);        
      }
    }

    private void bCancel_Click(object sender,EventArgs e) { Close(); }

    private void bApply_Click(object sender, EventArgs e) {
      Apply();
      f.Draw();
    }
  }
}
