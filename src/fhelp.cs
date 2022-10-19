using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace cubes {
  public partial class fhelp:Form {
    public fhelp(string file) {
      InitializeComponent();      
      rtb.LoadFile(file);
    }
  }
}
