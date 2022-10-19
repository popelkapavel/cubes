using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Reflection;
using System.IO;


[assembly: AssemblyVersion("0.23.0.0")]

namespace cubes {
  static class main {    
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]    
    static void Main(string[] arg) {      
      int vfid=1;
      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);
      fmain fm=new fmain(arg);
      fm.Show();
      Application.AddMessageFilter(new MessageFilter(fm));
      while(!fm.closed) {        
        Application.DoEvents();
        if(fm.closed) break;
        if(fm.cam.speed2!=0) {
          fm.cam.Fly();
          fm.lazydraw=true;
        } 
        if(fm.lazydraw||fm.lazycolor||fm.lazyshadow) {
          if(fm.lazycolor) {
            fm.Colorize();
            fm.lazycolor=false;
          }
          if(fm.lazyshadow) {
            fm.Shadow(fm.revshadow);
            fm.lazyshadow=false;
          }   
          if(fm.cam.record)
            fm.storeframe=true;
          fm.Draw();                    
          fm.lazydraw=false;
          if(fm.cam.record&&fm.lastframe!=null) {
            fm.lastframe.UpsideDown();
            System.Drawing.Bitmap bmp=fm.lastframe.ToBitmap();
            string recfile=string.Format("{1}{0:000000}.{2}",vfid++,fm.cam.record_path,fm.cam.record_ext.ToString().ToLowerInvariant());
            bmp.Save(recfile,fm.cam.record_ext);
          } 
        }  
        if(fm.cam.speed2==0)
          Mesh3d.WINAPI.WaitMessage();
      }
      //Application.Run(new fmain(filename));
    }
  }
  
  internal class MessageFilter:IMessageFilter {
    fmain fm;
    public MessageFilter(fmain fm) {
      this.fm=fm;
    }
    public bool PreFilterMessage(ref Message m) {
      if(m.Msg==522) { // wheel, becouse trackbar tZ steps by 3
        int wp=(int)(m.WParam.ToInt64());
        fm.MWheel(wp>>16);
        return true;
      } else
        return false;  
    }
  }
}
