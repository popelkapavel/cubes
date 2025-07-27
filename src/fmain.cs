using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Security;
using System.IO;
using Math3d;
using Mesh3d;

namespace cubes {
  enum ViewModes {Edit,Mesh}
  enum ColorModes {Color,NormalRGB,NormalRadial,Random,Three,GradientY,Sphere,CylinderZ,RadialZ}
  enum Tools   {None,Select,Pen,Fill3D,Fill2D,MoveSel,ViewLook,Box,Rect,Cylinder,Elipse,Sphere,Line,Cone}
  enum Colors  {Black,LightGreen=0xff0000,Green=0x800000,DarkGreen=0x400000,DarkGray=0x404040,Gray=0x808080,LightGray=0xc0c0c0,White=0xffffff}
  enum Conversion {Normal=0,Cubes=1,Rotoid=2,File=4}
  public partial class fmain:Form {
    fBoard Board; 
    internal bit3d b3d,brush,undo;
    List<undoitem> undos=new List<undoitem>();
    int undoc;
    int UndoMax;
    IntPtr AlmostBlack,AlmostWhite,YellowWhite,BlueWhite,Black,White,Brown,DarkBlue,Green;
    int bx,by,bz;
    int x=0,y=0,z=0,pen=1,size=8;
    int scx=0,scy=0;
    ViewModes mode;
    int zx=0,zy=0; // perspective center
    int lbrx,lbry,rbrx,rbry;
    double mpx,mpy;
    int mpz;
    //int CX,CY; // cursor
    Tools tool,mop;
    bool xori=false;
    bool invalid,setcenter;
    string importfile="out",exportfile="out",exportVector="out",exportBitmap=null;
    string importperm,importscale,importresize,importresize2;
    string exportperm,exportscale,exportresize,exportresizeflags,exportresize2;
    bool InvertWheelZ;
    byte meshsource;
    int exportfidx=2;
    int SX0=-1,SY0=-1,SX1=-1,SY1=-1,SZ0=-1,SZ1=-1; // selection box
    int CX=0,CY=0,CZ=0;
    int mmx,mmy,mmx2,mmy2;
    float fx,fy;
    bit3d Selection; // pasted selection
    bool Innercopy;
    internal camera cam=new camera();
    double[] light_vec=new double[] {0,-1,0};
    float lwidth=0;
    mesh msh;
    bool Dirty=false;
    OpenFileDialog ofd=new OpenFileDialog();
    SaveFileDialog sfd=new SaveFileDialog(); 
    ColorDialog cdialog=new ColorDialog();
    int mesh_color=0x808080;
    ColorModes color_mode;
    double[] color_rota=d33.M1,color_pos=d3.V();
    Conversion conv;
    internal bool closed;
    internal bool lazycolor=false,lazydraw=false,lazyshadow=false,revshadow=false;
    internal bool storeframe=false;
    internal bitmap lastframe=null;
    IntPtr RedBrush;
    bool wirefront=false;
    PrintDialog printd=null;
    PageSetupDialog paged=null;
    double[] palette=faces.PalGray;    
    static bool IsMesh(string filename) {
      return Regex.IsMatch(filename,@"\.((obj|stl|off)(.gz)?|pgm|3dt)$",RegexOptions.IgnoreCase);
    }
    static bool Yes(string s) {
      s=(""+s).Trim().ToLower();
      return s=="1"||s=="yes"||s=="true";
    }
    static string PrefixFlags(string text,string set,out string flags) {
      flags="";text+="";set+="";
      int i;
      for(i=0;i<text.Length&&(set==""?char.IsLetter(text[i]):0<=set.IndexOf(text[i]));i++);
      if(i>0) {flags=text.Substring(0,i);text=text.Substring(i);}
      return text;
    }
    void HelpCmd() {
      MessageBox.Show(@"Options
-ip yxZr	  permute mesh coords and/or reverese edges on mesh import
-is 1[,2,3] import scale
-ir 1[,2,3] import resize (for lower bound and upper bound)
-ep yxZr	  permute mesh coords and/or reverese edges on mesh export
-es 1[,2,3] export scale
-er 1[,2,3] export resize  
-erf xlyhzs export resize flags (x,y,z:axis,l:lo,h:high,c:center,m:master,n:slave,e:extend)
","Command line help",MessageBoxButtons.OK,MessageBoxIcon.Information);
    }
    void Ini(string inifile) {
     Innercopy=Yes(WINAPI.GetINI(inifile,"EDIT","INNER_COPY","0"));
     InvertWheelZ=Yes(WINAPI.GetINI(inifile,"EDIT","INVERT_WHEELZ","0"));
     mEditInvertWheelZ.Checked=Innercopy;
     if(!int.TryParse(WINAPI.GetINI(inifile,"EDIT","UNDO_MAX",""),out UndoMax)||UndoMax<1) UndoMax=5;
     eSize.Text=WINAPI.GetINI(inifile,"EDIT","CUBE_SIZE","16");
     float.TryParse(WINAPI.GetINI(inifile,"VIEW","LINE_WIDTH","0"),System.Globalization.NumberStyles.Float,System.Globalization.CultureInfo.InvariantCulture,out lwidth);
     if(lwidth<0) lwidth=0;     
     else if(lwidth>1) mViewBoldLine.Checked=true;
     double.TryParse(WINAPI.GetINI(inifile,"CAMERA","EYES","1"),System.Globalization.NumberStyles.Float,System.Globalization.CultureInfo.InvariantCulture,out cam.eyes);
     double.TryParse(WINAPI.GetINI(inifile,"CAMERA","EYEA","0"),System.Globalization.NumberStyles.Float,System.Globalization.CultureInfo.InvariantCulture,out cam.eyea);
     cam.RecordConfig(WINAPI.GetINI(inifile,"RECORD","PATH",""),WINAPI.GetINI(inifile,"RECORD","EXT","png"));
    }
    public fmain(string[] arg) {
     Board=new fBoard(this);
     string filename=null;
     int a=0;
     while(a<arg.Length) {
       string opt=arg[a];
       if(opt=="") continue;
       if(opt[0]!='-') break;
       a++;
       if(opt=="-") break;
       switch(opt) {
        case "-ip":importperm=arg[a++];break;
        case "-is":importscale=arg[a++];break;
        case "-ir":if(importresize!=null) importresize2=arg[a++];else importresize=arg[a++];break;
        case "-ep":exportperm=arg[a++];break;
        case "-er":if(exportresize!=null) exportresize2=arg[a++];else exportresize=arg[a++];break;
        case "-erf":exportresizeflags=arg[a++];break;
        case "-es":exportscale=arg[a++];break;
        case "-?":HelpCmd();break;
       }
     }
     if(a<arg.Length) {
       if(File.Exists(arg[a])) filename=arg[a];
       else MessageBox.Show("File "+arg[a]+" does not exists.","Open file");
     }
     unchecked {
      InitializeComponent();
      SetTool(Tools.Pen);
      if(filename!=null) ofd.FileName=filename;
      ofd.Filter="*.cub|*.cub|*.cub.gz|*.cub.gz|*.*|*.*";
      sfd.AddExtension=true;

      AlmostBlack=GDI.CreateSolidBrush(0x606060);
      AlmostWhite=GDI.CreateSolidBrush(0xe0e0e0);
      YellowWhite=GDI.CreateSolidBrush(0xffe0e0);
      BlueWhite=GDI.CreateSolidBrush(0xe0e0ff);
      Green=GDI.CreateSolidBrush(0x008000);
      DarkBlue=GDI.CreateSolidBrush(0xff0000);
      White=GDI.GetStockObject(GDI.StockObjects.WHITE_BRUSH);
      Black=GDI.GetStockObject(GDI.StockObjects.BLACK_BRUSH);
      Brown=GDI.CreateSolidBrush(0x80);
      //AlmostBlack=new SolidBrush(Color.FromArgb((int)0xff606060));
      //AlmostWhite=new SolidBrush(Color.FromArgb((int)0xffe0e0e0));
      //YellowWhite=new SolidBrush(Color.FromArgb((int)0xffffe0e0));
      //BlueWhite=new SolidBrush(Color.FromArgb((int)0xffe0e0ff));
      MouseWheel+=new MouseEventHandler(fmain_MouseWheel);

      string inifile=Directory.GetCurrentDirectory()+"\\cubes.ini";
      Ini(inifile);

      b3d=new bit3d();
      bool opened=false;
      if(ofd.FileName!=null&&ofd.FileName.Length>0)
       try { 
        if(IsMesh(ofd.FileName)) {          
          ImportMesh(ofd.FileName,2);
          Text="cubes - "+ofd.FileName;
          ofd.FileName=null;
        } else {
          LoadFile(ofd.FileName,false);
          opened=true;
        }
       } catch {}
      if(!opened) {
        ofd.FileName=null;
        b3d.alloc(32,32,32); 
      }       
      brush=new bit3d();
      ChangeZ(0);
      UpdateDZ();
      ChangePenSize(1);
      ApplyCubeSize();

     cam.pos[0]=b3d.dx/2;
     cam.pos[1]=b3d.dy/2;
     cam.pos[2]=-b3d.dz/2;
     cam.angle=50;
     cam.zmin=0.1;
     cam.zmax=1000;

     }
    }
    
    void ChangeFileName(string filename) {
      ofd.FileName=filename;
      string fn=Path.GetFileName(filename);
      Dirty=false;
      Text="cubes"+(string.IsNullOrEmpty(fn)?"":" - "+fn);//+(Dirty?"*":"");
    }


    void fmain_MouseWheel(object sender,MouseEventArgs e) {
      MWheel(e.Delta);
    }
    int MouseAxis() { 
      int w3=Width/3,h3=Height/3,x=mmx2-w3,y=mmy2-h3;
      if(x>0&&x<w3&&y>0&&y<h3) return 3;      
      if(Math.Abs(x=mmx2-Width/2)>Math.Abs(y=mmy2-Height/2)) return x<0?-1:1;
      return y<0?-2:2;
    }
    void Scroll2(bool vert,int scroll) {
      ScrollProperties sp=vert?(ScrollProperties)pMain.VerticalScroll:pMain.HorizontalScroll;
      int v2=sp.Value,v=v2+scroll*size;
      if(v<0) v=0;else if(v>sp.Maximum) v=sp.Maximum;
      if(v2==v) return;
      sp.Value=v;
      pMain.PerformLayout();
      v=sp.Value;
      if(v2==v) return;
      if(vert) scy=v;else scx=v;
      //if(mode==ViewModes.Edit) Draw();
    }
    internal void MWheel(int delta) { 
      if(delta==0) return;      
      if(mode==ViewModes.Mesh) {
        int i=delta/120;
        if(CtrlDown) {
          int axis=MouseAxis(),neg=axis<0?-1:1;
          axis*=neg;
          axis--;
          double[] rd3=d3.V();
          d3.mul_d(rd3,0,cam.body,3*axis,neg*(ShiftDown?10:1)*i*100*cam.speed);
          d3.add(cam.pos,cam.pos,rd3);
        } else {
          i=-i;
          while(i>0) {
            if(cam.angle<85)
              cam.angle*=105.0/100.0;
            i--;
          }
          while(i<0) {
            cam.angle*=100.0/105.0;
            i++;
          }
        }  
        lazydraw=true;
      } else {
        if(ShiftDown) {
          Scroll2(false,-delta*3/120);
        } else if(AltDown) {
          Scroll2(true,-delta*3/120);
        } else if(CtrlDown) {
          ChangeCubeSize(size+delta/120);
          int ex=mmx2+scx,ey=mmy2+scy;
          int x=ex/size,y=ey/size;
          UpdateXY(x,x-this.x,y,y-this.y,LButton||RButton);  
        } else
          ChangeZ(z-(InvertWheelZ?-1:1)*delta/120);
      }
    }
    void Invalid() {
      invalid=true;
    }
    bool SelectionEmpty() {
      return SX0<0;
    }
    void SelectionClear() {
      SX0=SX1=SY0=SY1=SZ0=SZ1=-1;
    }
    void NormaliseSelection() {
      int r;
      if(SX0>SX1) {r=SX0;SX0=SX1;SX1=r;}
      if(SY0>SY1) {r=SY0;SY0=SY1;SY1=r;}
      if(SZ0>SZ1) {r=SZ0;SZ0=SZ1;SZ1=r;}
    }
    bool SelectedAll() {
      return (SX0|SY0|SZ0)==0&&SX1==b3d.dx-1&&SY1==b3d.dy-1&&SZ1==b3d.dz-1;
    }        
    public void New(int x,int y,int z,bool paste) {
      if(paste) {
        string text="";
       try {
        text=Clipboard.GetText(TextDataFormat.UnicodeText);
        b3d=bit3d.Import(text,out CX,out CY,out CZ);
       } catch(Exception ex) { 
        MessageBox.Show(ex.Message,"Paste new");
        return;
       }
      } else {
        b3d.alloc(x,y,z);
        this.x=this.y=0;
      }
      ChangeFileName("");
      UpdateDZ();
      SelectionClear();
      ClearUndo();
      ChangeZ(b3d.dz/2);            
      SetMode(ViewModes.Edit,Conversion.Normal);      
      UpdateScroll();
      Invalid();      
    }
    void ChangePenSize(int size) {
      if(size<1) size=1;
      ePen.Text=size.ToString();
      pen=size*2-1;
      brush.alloc(pen,pen,pen);      
      bx=by=bz=pen/2;      
      if(size==1) {
        brush.set(0,0,0,true);
        return;
      } 
      double x=1-1.0/size;
      brush.eval(-x,-x,-x,x,x,x,f3d.sphere,f1d.GT0);      
    }
    void UpdateScroll() {
      pMain.AutoScrollMinSize=new Size(b3d.dx*size,b3d.dy*size);
      scx=pMain.HorizontalScroll.Value;
      scy=pMain.VerticalScroll.Value;
    }
    void UpdateDZ() {
      int m=b3d.dz-1;
      if(z>m) z=m;
      if(tZ.Value>m) tZ.Value=m;
      tZ.Maximum=m;
    }
    void ChangeCubeSize(int s) {
      if(s<1) s=1;else if(s>64) s=64;
      if(size==s) return;
      this.size=s;
      eSize.Text=s.ToString();
      UpdateScroll();      
      Invalid();
    }
    
    Bitmap CaptureBitmap(int x,int y,int width,int height) {
      Bitmap bmp=new Bitmap(width,height,PixelFormat.Format32bppRgb);
      Graphics gr=Graphics.FromImage(bmp);
      gr.CopyFromScreen(x,y,0,0,new Size(width,height));
      //OpenGL(gr);
      gr.Dispose();
      return bmp;
    }
    [DllImport("user32"),SuppressUnmanagedCodeSecurity,PreserveSig]
    static extern short GetKeyState(Keys key);
    [DllImport("user32"), SuppressUnmanagedCodeSecurity, PreserveSig]
    static extern short GetAsyncKeyState(Keys key);
    
    public static bool CtrlDown {get{ return 0!=(0x8000&GetKeyState(Keys.ControlKey));}}
    public static bool CtrlRKey {get{ return 0!=(0x8000&GetKeyState(Keys.RControlKey));}}    
    public static bool ShiftDown {get{ return 0!=(0x8000&GetKeyState(Keys.ShiftKey));}}
    public static bool ShiftRKey {get{ return 0!=(0x8000&GetKeyState(Keys.RShiftKey));}}    
    public static bool AltDown {get{ return 0!=(0x8000&GetKeyState(Keys.Menu));}}
    public static bool LButton {get{ return 0!=(0x8000&GetKeyState(Keys.LButton));}}
    public static bool MButton {get{ return 0!=(0x8000&GetKeyState(Keys.MButton));}}
    public static bool RButton {get{ return 0!=(0x8000&GetKeyState(Keys.RButton));}}
    public static bool CapsLock {get{ return 0!=(0x0001&GetKeyState(Keys.CapsLock));}}
    public static bool NumLock {get{ return 0!=(0x0001&GetKeyState(Keys.NumLock));}}
    public static bool ScrollLock {get{ return 0!=(0x0001&GetKeyState(Keys.Scroll));}}
    
    internal void SetMode(ViewModes mode,Conversion conv) {
      this.mode=mode;
      if(mode==ViewModes.Mesh) this.conv=conv;
      else if(string.IsNullOrEmpty(ofd.FileName)) ChangeFileName("");
      mViewMesh.Checked=mode==ViewModes.Mesh;      
      if(mode==ViewModes.Mesh) {
        setcenter=true;
      } else
        cam.Fly(0);
      MainMenuStrip.Enabled=MainMenuStrip.Visible=mode==ViewModes.Edit||FormBorderStyle!=FormBorderStyle.None;
      pMain.AutoScroll=mode==ViewModes.Edit;
      Invalid();
      pControl.Visible=mode==ViewModes.Edit;
    } 
    void Fullscreen(bool fullscreen) {            
      MainMenuStrip.Enabled=MainMenuStrip.Visible=!fullscreen||mode==ViewModes.Edit;
      FormBorderStyle=fullscreen?FormBorderStyle.None:FormBorderStyle.Sizable;
      WindowState=fullscreen?FormWindowState.Maximized:FormWindowState.Normal;
      FormBorderStyle=fullscreen?FormBorderStyle.None:FormBorderStyle.Sizable;
      pMain.Focus();
    }    
    void SetTool(Tools t) {
      tool=t;
      bPen.Enabled=t!=Tools.Pen;
      bBox.Enabled=t!=Tools.Box&&t!=Tools.Rect;
      bCyl.Enabled=t!=Tools.Cylinder&&t!=Tools.Elipse;
      bSph.Enabled=t!=Tools.Sphere;
      bCone.Enabled=t!=Tools.Cone;
      bLine.Enabled=t!=Tools.Line;
      bFill3D.Enabled=t!=Tools.Fill3D;
      bFill2D.Enabled=t!=Tools.Fill2D;
    }    
    internal void Draw() {
      Graphics gr=pMain.CreateGraphics();
      Draw(gr); 
      gr.Dispose();    
    }  
    void Draw(Graphics gr) {
      switch(mode) {
       case ViewModes.Edit:
        IntPtr hdc=gr.GetHdc();
        RECT rec;        
        if(b3d.dx*size-scx<pMain.Width) {
          rec.Left=b3d.dx*size-scx;rec.Top=0;rec.Right=pMain.Width;rec.Bottom=pMain.Height;       
          GDI.FillRect(hdc,ref rec,GDI.GetStockObject(GDI.StockObjects.LTGRAY_BRUSH));
        }  
          //gr.FillRectangle(Brushes.LightGray,b3d.dx*size,Panel1Top,Panel1Width-b3d.dx*size,Panel1Height);
        if(b3d.dy*size-scy<pMain.Height) {
          rec.Left=0;rec.Top=b3d.dy*size-scy;rec.Right=b3d.dx*size;rec.Bottom=pMain.Height;
          GDI.FillRect(hdc,ref rec,GDI.GetStockObject(GDI.StockObjects.LTGRAY_BRUSH));
          //gr.FillRectangle(Brushes.LightGray,0,b3d.dy*size+Panel1Top,b3d.dx*size,Panel1Height-b3d.dy*size);
        } 
        int z1=z<b3d.dz-1?z+1:-1,z0=z-1;
        for(int j=0;j<b3d.dy;j++) {
          for(int i=0;i<b3d.dx;i++) {           
            bool b=b3d.get(i,j,z),b0=z0<0?false:b3d.get(i,j,z0),b1=z1<0?false:b3d.get(i,j,z1);                        
            DrawBit(hdc,i,j,b,b0,b1);
            //gr.DrawRectangle(b?Pens.Black:Pens.White,x,y,size,size);          
          }
        }
        if(xori) DrawXor();
        gr.ReleaseHdc(hdc);            
        break;
/*       case ViewModes.Orto:
        Preview(gr,1);
        break;*/
       case ViewModes.Mesh:
        OpenGL(gr,mMeshWires.Checked,mMeshWhite.Checked);
        break;   
      }  
    }    
#if ORTO    
    void preview_cube(bitmap bm,int x,int y,int z,int scale,int depth,bool bx0,bool bx1,bool by0,bool by1,bool bz0,bool bz1) {
      int sx,sy,w=bm.w,h=bm.h;
      sx=z*depth+x*scale;
      sy=h-1-z*depth-y*scale;
      if(sx<0||sx+scale+depth>=w) return;
      if(sy-scale-depth<0||sy>=h) return;
      int b=sy*w+sx;
      int c1=(int)Colors.White,c2=(int)Colors.LightGray,c3=(int)Colors.Gray;
      for(int j=0;j<scale;j++) {
        for(int i=0;i<scale;i++)
          bm.pix[b-j*w+i]=c1;
        for(int i=1;i<depth;i++) {
          bm.pix[b-(scale+i)*w+i+j]=c2;
          bm.pix[b-(i+j)*w+scale+i]=c3;
        }  
      }    
      int c=(int)Colors.Black;
      for(int j=0;j<scale;j++) {
        bm.pix[b+j]=by1?c1:c;
        bm.pix[b-scale*w+1+j]=bz0?c2:c;
        if(!bx0) bm.pix[b-(j+1)*w]=c;
        if(!bx1) bm.pix[b-j*w+scale]=c;
        if(!bz1) bm.pix[b-(scale+depth)*w+depth+j]=c;
        bm.pix[b-(j+depth)*w+scale+depth]=bz1?c3:c;
      }
      for(int i=1;i<depth;i++) {
        if(bx0==by0) bm.pix[b-(scale+i)*w+i]=c;
        if(bx1==by0) bm.pix[b-(scale+i)*w+scale+i]=c;
        if(bx1==by1) bm.pix[b-i*w+scale+i]=c;
      }   
    }         
    /* void DrawPixels(Graphics gr,int[] pixel,int w,int h) {
       Bitmap bm=new Bitmap(w,h,PixelFormat.Format32bppRgb);
       BitmapData bd=bm.LockBits(new Rectangle(0,0,w,h),ImageLockMode.WriteOnly,bm.PixelFormat);
       Marshal.Copy(pixel,0,bd.Scan0,pixel.Length);
       bm.UnlockBits(bd);
       //   Graphics gr=this.CreateGraphics();
       gr.DrawImageUnscaled(bm,0,0);
       //gr.Dispose();
       bm.Dispose();    
    } */         
    void Preview(Graphics gr,int paint) {
     //try {
      bitmap bm=new bitmap(Panel1Width,Panel1Height);
      bm.gradient(0xffffff,0xc0c0c0);
      int x,y,z;      
      int scale=bm.w*8/(8*b3d.dx+3*b3d.dz);
      int i=bm.h*8/(8*b3d.dy+3*b3d.dz);
      if(i<scale) scale=i;
      int depth=3*scale/8;
      //int mx=scale+depth+1,my=mx;
      bool bx0,bx1,by0,by1,b0z,b1z;
      for(z=b3d.dz;z-->0;)
        for(y=b3d.dy;y-->0;)
          for(x=0;x<b3d.dx;x++)
            if(b3d.get(x,y,z)) {
              bx0=!(x==0||!b3d.get(x-1,y,z));
              bx1=!(x>=b3d.dx-1||!b3d.get(x+1,y,z));
              by0=!(y==0||!b3d.get(x,y-1,z));
              by1=!(y>=b3d.dy-1||!b3d.get(x,y+1,z));
              b0z=!(z==0||!b3d.get(x,y,z-1));
              b1z=!(z>=b3d.dz-1||!b3d.get(x,y,z+1));
              if(!bx0||!bx1
                  ||!by0||!by1
                  ||!b0z||!b1z)
                preview_cube(bm,x,b3d.dy-y-1,z,scale,depth,bx0,bx1,by0,by1,b0z,b1z);
              }
       bm.Draw(gr,0,Panel1Top);       
      //} catch {}       
    }
    void pers_cube1(bitmap bm,int x,int y,int z) {
      double a=0.5;
      double r1=a*(z)/b3d.dz,r=1-r1; 
      double r3=a*(z+1)/b3d.dz,r2=1-r3;       
      int ix=x*bm.w/b3d.dx,iy=y*bm.h/b3d.dy;
      int ix2=(x+1)*bm.w/b3d.dx,iy2=(y+1)*bm.h/b3d.dy;
      int cx=(int)(r*ix+r1*zx),cy=(int)(r*iy+r1*zy);
      int cx2=(int)(r*ix2+r1*zx),cy2=(int)(r*iy2+r1*zy);
      int dx=(int)(r2*ix+r3*zx),dy=(int)(r2*iy+r3*zy);
      int dx2=(int)(r2*ix2+r3*zx),dy2=(int)(r2*iy2+r3*zy);      
      Point[] pts=new Point[4];
      int[] order=new int[] {0,2,1,3};      
      int i;      
      if(cy>zy) {
        i=order[0];order[0]=order[2];order[2]=i;
      }
      if(cx>zx) {
        i=order[1];order[1]=order[3];order[3]=i;
      }
      for(i=0;i<4;i++) {
       Brush brush=Brushes.White;
       switch(order[i]) {
        case 0:
         pts[0].X=dx;pts[0].Y=dy;
         pts[1].X=dx2;pts[1].Y=dy;
         pts[2].X=cx2;pts[2].Y=cy;
         pts[3].X=cx;pts[3].Y=cy;
         break;
        case 1: 
         pts[0].X=dx;pts[0].Y=dy2;
         pts[1].X=dx2;pts[1].Y=dy2;
         pts[2].X=cx2;pts[2].Y=cy2;
         pts[3].X=cx;pts[3].Y=cy2;
         brush=Brushes.LightCoral;
         break;
        case 2: 
         pts[0].X=dx;pts[0].Y=dy;
         pts[1].X=dx;pts[1].Y=dy2;
         pts[2].X=cx;pts[2].Y=cy2;
         pts[3].X=cx;pts[3].Y=cy;
         brush=Brushes.SandyBrown;
         break;
        case 3:         
         pts[0].X=dx2;pts[0].Y=dy;
         pts[1].X=dx2;pts[1].Y=dy2;
         pts[2].X=cx2;pts[2].Y=cy2;
         pts[3].X=cx2;pts[3].Y=cy;
         brush=Brushes.Wheat;
         break;
       }          
       //gr.FillPolygon(brush,pts);
       //gr.DrawPolygon(Pens.Black,pts);
      } 
    }
    void pers_cube2(bitmap bm,int x,int y,int z) {
      int pw=bm.w,ph=bm.h;
      double a=0.5;
      double r1=a*(z)/b3d.dz,r=1-r1; 
      double r3=a*(z+1)/b3d.dz,r2=1-r3;       
      int ix=x*pw/b3d.dx,iy=y*ph/b3d.dy;
      int ix2=(x+1)*pw/b3d.dx,iy2=(y+1)*ph/b3d.dy;
      int cx=(int)(r*ix+r1*zx),cy=(int)(r*iy+r1*zy);
      int cx2=(int)(r*ix2+r1*zx),cy2=(int)(r*iy2+r1*zy);
      bm.Rectangle(cx,cy,cx2-cx,cy2-cy,(int)Colors.LightGray,(int)Colors.Black);
      //gr.FillRectangle(Brushes.LightGray,cx,cy,cx2-cx,cy2-cy);
      //gr.DrawRectangle(Pens.Black,cx,cy,cx2-cx,cy2-cy);
//  BitBlt(view.dc,sx,sy,scale+depth+1,scale+depth+1,mask,0,0,SRCAND);//SRCAND);
//  BitBlt(view.dc,sx,sy,scale+depth+1,scale+depth+1,color,0,0,SRCPAINT); //srcpaint
    }

    /*void pers(Graphics gr,int x,int y,int z) {
      if(x==0||!b3d.get(x-1,y,z)||x>=b3d.dx-1||!b3d.get(x+1,y,z)
          ||y==0||!b3d.get(x,y-1,z)||y>=b3d.dy-1||!b3d.get(x,y+1,z)
          ||z==0||!b3d.get(x,y,z-1)||z>=b3d.dz-1||!b3d.get(x,y,z+1))
        pers_cube1(gr,x,b3d.dy-y-1,z);
    }*/
    void Pers(Graphics gr) {
     try {
      bitmap bm=new bitmap(Panel1Width,Panel1Height);
      bm.gradient(0xffffff,0xc0c0c0);

      int i,b,l,x,y,z;
      int sx=zx*b3d.dx/bm.w,sy=zy*b3d.dy/bm.h;
      int n=sx>b3d.dx-sx?sx:b3d.dx-sx;
      if(n<sy) n=sy;
      if(n<b3d.dy-sy) n=b3d.dy-sy;
      int[] xa=new int[b3d.dx*b3d.dy],ya=new int[b3d.dx*b3d.dy];
      bool[] ba=new bool[b3d.dx*b3d.dy];
      int r=n;        
      int h=0;
      bool bsx=sx>=0&&sx<b3d.dx,bsy=sy>=0&&sy<b3d.dy;
      while(r>0) {
        int x0=sx-r,x1=sx+r,y0=sy-r,y1=sy+r;
        bool bx0=x0>=0&&x0<b3d.dx,bx1=x1>=0&&x1<b3d.dx;
        bool by0=y0>=0&&y0<b3d.dy,by1=y1>=0&&y1<b3d.dy;
        if(bx0&&by0) { xa[h]=x0;ya[h++]=y0;}
        if(bx1&&by0) { xa[h]=x1;ya[h++]=y0;}
        if(bx0&&by1) { xa[h]=x0;ya[h++]=y1;}
        if(bx1&&by1) { xa[h]=x1;ya[h++]=y1;}
                
        if(by0||by1) {
          b=x0+1;l=sx-b;b3d.Clip(0,ref b,ref l);
          for(i=0;i<l;i++) { 
            if(by0) {xa[h]=b+i;ya[h++]=y0;}
            if(by1) {xa[h]=b+i;ya[h++]=y1;}
          }
          b=sx+1;l=x1-b;b3d.Clip(0,ref b,ref l); 
          for(i=l;i-->0;) { 
            if(by0) {xa[h]=b+i;ya[h++]=y0;}
            if(by1) {xa[h]=b+i;ya[h++]=y1;}
          }
        }   
        if(bx0||bx1) {
          b=y0+1;l=sy-b;b3d.Clip(1,ref b,ref l);
          for(i=0;i<l;i++) { 
            if(bx0) {xa[h]=x0;ya[h++]=b+i;}
            if(bx1) {xa[h]=x1;ya[h++]=b+i;}
          }
          b=sy+1;l=y1-b;b3d.Clip(1,ref b,ref l); 
          for(i=l;i-->0;) { 
            if(bx0) {xa[h]=x0;ya[h++]=b+i;}
            if(bx1) {xa[h]=x1;ya[h++]=b+i;}
          }
        }   
        
        if(bx0&&bsy) { xa[h]=x0;ya[h++]=sy;}
        if(bx1&&bsy) { xa[h]=x1;ya[h++]=sy;}
        if(bsx&&by0) { xa[h]=sx;ya[h++]=y0;}
        if(bsx&&by1) { xa[h]=sx;ya[h++]=y1;}
        r--;
      }      
      if(bsx&&bsy) {xa[h]=sx;ya[h++]=sy;}  
      
      for(z=b3d.dz;z-->0;) {
        for(i=0;i<h;i++) {
          x=xa[i];y=ya[i];
          ba[i]=false;
          if(b3d.get(x,y,z)) {
            if(x==0||!b3d.get(x-1,y,z)||x>=b3d.dx-1||!b3d.get(x+1,y,z)
                ||y==0||!b3d.get(x,y-1,z)||y>=b3d.dy-1||!b3d.get(x,y+1,z)
                ||z==0||!b3d.get(x,y,z-1)||z>=b3d.dz-1||!b3d.get(x,y,z+1)) {
              ba[i]=true;
              pers_cube1(bm,x,y,z);
            }  
          }
        }
        for(i=0;i<h;i++)
          if(ba[i])
            pers_cube2(bm,xa[i],ya[i],z);
      }
      bm.Draw(gr,0,Panel1Top);
     } catch {
     } 
    }
#endif
    void OpenGL0(bool white) {
      opengl.glPolygonMode(GLEnum.GL_BACK,GLEnum.GL_FILL);
      opengl.glPolygonMode(GLEnum.GL_FRONT,wirefront?GLEnum.GL_LINE:GLEnum.GL_FILL);

      float bg=white?1f:0f;
      opengl.glClearColor(bg,bg,bg,0); // bgcolor
      opengl.glClear(GLbitfield.GL_COLOR_BUFFER_BIT|GLbitfield.GL_DEPTH_BUFFER_BIT);      
    }
    void OpenGL1(bool wires,bool white,double dx,double ax,int w,int h) {
      if(msh==null) return;
        opengl.ViewPush(cam.pos,cam.rot,cam.zmin,cam.zmax,cam.angle,w,h,dx,ax);      
      
        msh.glDraw(null,true);
      
        opengl.ViewPop();
      
        if(wires) {
      
          double[] colors=new double[3];
          colors[0]=colors[1]=colors[2]=white?1f:0f; //linecolor
          //colors[3]=colors[4]=colors[5]=0;
      
          opengl.ViewPush(cam.pos,cam.rot,cam.zmin*1.0025,cam.zmax,cam.angle,w,h,dx,ax);
          opengl.glDepthFunc(GLEnum.GL_LEQUAL);          
          opengl.glPolygonMode(GLEnum.GL_BACK,GLEnum.GL_LINE);
          opengl.glPolygonMode(GLEnum.GL_FRONT,wirefront?GLEnum.GL_POINT:GLEnum.GL_LINE);
          //opengl.glPointSize(0);
          msh.glDraw(colors,true);
          opengl.glDepthFunc(GLEnum.GL_LESS);

          opengl.ViewPop();
        }      
    }
    void OpenGL(Graphics gr,bool wires,bool white) {
            
      //Graphics gr=CreateGraphics();
      IntPtr hdc=gr.GetHdc();
      opengl.AllocHDC(hdc,lwidth<=0?1:lwidth);
      OpenGL0(wires||white);
      //opengl.glPolygonMode(GLEnum.GL_FRONT,GLEnum.GL_FILL);//GL_LINE,GL_POINTS
            



      int w=pMain.ClientRectangle.Width,h=pMain.ClientRectangle.Height;//-(MainMenuStrip.Visible?MainMenuStrip.Height:0);      
      if(cam.anaglyph||cam.crosseye) {
        bitmap bl,br;
        Rectangle r=MainMenuStrip.Visible?pMain.ClientRectangle:Screen.PrimaryScreen.Bounds;     
        Point p0=new Point(0,0);
        if(MainMenuStrip.Visible) p0=pMain.PointToScreen(p0);
        //opengl.SwapBuffers(hdc);
        bl=new bitmap(cam.crosseye?r.Width/2:r.Width,r.Height);
        //double eyes=-cam.eyes;//*(cam.anaglyph?-1:1);
        OpenGL1(wires,white,+cam.eyes,cam.eyea,w,h);
        GCHandle gh=GCHandle.Alloc(bl.pix,GCHandleType.Pinned);
        opengl.glReadBuffer(GLEnum.GL_BACK);
        opengl.glReadPixels(cam.crosseye?bl.w/2:0,0,bl.w,bl.h,GLEnum.GL_BGRA,GLEnum.GL_UNSIGNED_BYTE,gh.AddrOfPinnedObject());
        gh.Free();
        //bmp=CaptureBitmap(p0.X,p0.Y,r.Width,r.Height);
        //bl=bitmap.FromBitmap(bmp);
        //bmp.Dispose();
        OpenGL0(wires);
        br=new bitmap(cam.crosseye?r.Width/2:r.Width,r.Height);
        OpenGL1(wires,white,-cam.eyes,cam.eyea,w,h); 
        gh=GCHandle.Alloc(br.pix,GCHandleType.Pinned);
        opengl.glReadPixels(cam.crosseye?br.w/2:0,0,br.w,br.h,GLEnum.GL_BGRA,GLEnum.GL_UNSIGNED_BYTE,gh.AddrOfPinnedObject());
        gh.Free();
        //opengl.SwapBuffers(hdc);        
        //bmp=CaptureBitmap(p0.X,p0.Y,r.Width,r.Height);
        //br=bitmap.FromBitmap(bmp);
        //bmp.Dispose();        
        
        //if(cam.crosseye) bl.CrossEyes(br);
        if(cam.anaglyph) bl.Anaglyph(br);
        BITMAPINFO bi=new BITMAPINFO();
        bi.Size=Marshal.SizeOf(bi);
        bi.Width=bl.w;bi.Height=bl.h;
        bi.Planes=1;bi.BitCount=32;                
        gh=GCHandle.Alloc(bl.pix,GCHandleType.Pinned);
        GDI.SetDIBitsToDevice(hdc,0,0,bl.w,bl.h,0,0,0,bl.h,gh.AddrOfPinnedObject(),ref bi,0);
        gh.Free();
        if(cam.crosseye) {
          gh=GCHandle.Alloc(br.pix,GCHandleType.Pinned);
          GDI.SetDIBitsToDevice(hdc,bl.w,0,br.w,br.h,0,0,0,br.h,gh.AddrOfPinnedObject(),ref bi,0);
          gh.Free();
        }
        if(storeframe) {
          lastframe=bl;
          storeframe=false;
        }  
        //bl.UpsideDown();
      } else {
        OpenGL1(wires,white,0,0,w,h);
        if(storeframe) {
          lastframe=new bitmap(w,h);
          GCHandle gh=GCHandle.Alloc(lastframe.pix,GCHandleType.Pinned);
          opengl.glReadBuffer(GLEnum.GL_BACK);
          opengl.glReadPixels(0,0,lastframe.w,lastframe.h,GLEnum.GL_BGRA,GLEnum.GL_UNSIGNED_BYTE,gh.AddrOfPinnedObject());
          gh.Free();
          storeframe=false;           
        }
        /*opengl.ViewPush(cam.pos,cam.rot,cam.zmin,cam.zmax,cam.angle,w,h,0);      
      
        msh.glDraw(null);
      
        opengl.ViewPop();
      
        if(wires) {
      
          double[] colors=new double[3];
          colors[0]=colors[1]=colors[2]=0f; //linecolor
          //colors[3]=colors[4]=colors[5]=0;
      
          opengl.ViewPush(cam.pos,cam.rot,cam.zmin*1.0025,cam.zmax,cam.angle,w,h);
          opengl.glDepthFunc(GLEnum.GL_LEQUAL);
          opengl.glPolygonMode(GLEnum.GL_BACK,GLEnum.GL_LINE);
          opengl.glPolygonMode(GLEnum.GL_FRONT,GLEnum.GL_LINE);
          msh.glDraw(colors);
          opengl.glDepthFunc(GLEnum.GL_LESS);

          opengl.ViewPop();
        } */ 
        opengl.SwapBuffers(hdc);        
      }  
      
      if(cam.cross) 
        DrawCross(hdc,w,h);
      if(cam.record) {
        RECT r;
        r.Left=4;r.Top=4;
        r.Right=20;r.Bottom=20;
        if(RedBrush==IntPtr.Zero)
          RedBrush=GDI.CreateSolidBrush(0xff);
        GDI.FillRect(hdc,ref r,RedBrush);  
      }
      opengl.FreeHDC();
      gr.ReleaseHdc();                        
    }
    
    void DrawCross(IntPtr hdc,int w,int h) {
      double[] cz=d3.V(cam.head[2],cam.head[3+2],cam.head[6+2]);
      double lx=Math.Tan(Math.PI*cam.angle/180),dx,dy;
      double yperx=(double)h/w;
      GDI.ROPStyles rop=GDI.SetROP2(hdc,GDI.ROPStyles.R2_XORPEN);
      IntPtr pen2=GDI.CreatePen(GDI.PenStyles.PS_SOLID,0,0x008000);
      IntPtr pen=GDI.CreatePen(GDI.PenStyles.PS_SOLID,0,0x800080);
      IntPtr pen3=GDI.SelectObject(hdc,pen);
      int sx,sy;
      POINT lpt;
      if(cz[2]>0&&Math.Abs(cz[0])<cz[2]*lx&&Math.Abs(cz[1])<cz[2]*yperx*lx) {
        sx=(int)(w/2*(1+cz[0]/cz[2]/lx));
        sy=(int)(h/2*(1+cz[1]/cz[2]/(yperx*lx)));

        //      SelectObject(proc.dc,GetStockObject(WHITE_PEN));
        dx=cam.head[0];dy=-cam.head[3+0];
        double d=Math.Sqrt(dx*dx+dy*dy);
        dx/=d;dy/=d;
        //SelectObject(proc.dc,GetStockObject(BLACK_PEN));
        GDI.MoveToEx(hdc,(int)(sx-100*dx),(int)(sy-100*dy),out lpt);
        GDI.LineTo(hdc,(int)(sx+100*dx),(int)(sy+100*dy));

        dx=cam.head[1];dy=-cam.head[3+1];
        d=Math.Sqrt(dx*dx+dy*dy);
        dx/=d;dy/=d;
        GDI.MoveToEx(hdc,(int)(sx-50*dx),(int)(sy-50*dy),out lpt);
        GDI.LineTo(hdc,(int)(sx+100*dx),(int)(sy+100*dy));        
      }
      GDI.SelectObject(hdc,pen2);
      sx=w/2;sy=h/2;
      GDI.MoveToEx(hdc,sx-100,sy,out lpt);
      GDI.LineTo(hdc,sx+100,sy);
      GDI.MoveToEx(hdc,sx,sy-99,out lpt);
      GDI.LineTo(hdc,sx,sy+51);
      GDI.SelectObject(hdc,pen3);
      GDI.DeleteObject(pen);
      GDI.DeleteObject(pen2);
      GDI.SetROP2(hdc,rop);
    }
    void DrawXor() {
      int x,px=(int)mpx,py=(int)mpy,rx=mmx,ry=mmy;       
      if(px>rx) { x=px;px=rx;rx=x;}
      if(py>ry) { x=py;py=ry;ry=x;}
      DrawXor((int)(px*size-scx),(int)(py*size-scy),(int)((rx+1)*size-scx),(int)((ry+1)*size-scy));
    }
    void DrawXor(int px,int py,int rx,int ry) {
      int x;
      if(px>rx) { x=px;px=rx;rx=x;}
      if(py>ry) { x=py;py=ry;ry=x;}
      Graphics gr=pMain.CreateGraphics();
      IntPtr hdc=gr.GetHdc();
      GDI.ROPStyles rop2=GDI.SetROP2(hdc,GDI.ROPStyles.R2_XORPEN);      
      IntPtr p2=GDI.SelectObject(hdc,GDI.GetStockObject(GDI.StockObjects.WHITE_PEN));
      IntPtr b2=GDI.SelectObject(hdc,GDI.GetStockObject(GDI.StockObjects.NULL_BRUSH));
      GDI.Ellipse(hdc,px,py,rx,ry);
      GDI.Rectangle(hdc,px,py,rx,ry);      
      GDI.Rectangle(hdc,px-1,py-1,rx+1,ry+1);
      GDI.Rectangle(hdc,px+1,py+1,rx-1,ry-1);
      POINT lpt;
      GDI.MoveToEx(hdc,px,py,out lpt);
      GDI.LineTo(hdc,rx,ry);
      GDI.MoveToEx(hdc,px,ry,out lpt);
      GDI.LineTo(hdc,rx,py);
      GDI.MoveToEx(hdc,(px+rx)/2,py,out lpt);
      GDI.LineTo(hdc,(px+rx)/2,ry);
      GDI.MoveToEx(hdc,px,(py+ry)/2,out lpt);
      GDI.LineTo(hdc,rx,(py+ry)/2);
      GDI.SetROP2(hdc,rop2);            
      GDI.SelectObject(hdc,p2);
      GDI.SelectObject(hdc,b2);
      gr.ReleaseHdc(hdc);
      gr.Dispose();
    }
    
    bool InSel(int x,int y,int z) {
      int i,a;
      if(SX0<SX1) {i=SX0;a=SX1;}
      else {i=SX1;a=SX0;}
      if(x<i||x>a) return false;
      if(SY0<SY1) {i=SY0;a=SY1;}
      else {i=SY1;a=SY0;}
      if(y<i||y>a) return false;
      if(SZ0<SZ1) {i=SZ0;a=SZ1;}
      else {i=SZ1;a=SZ0;}
      if(z<i||z>a) return false;
      return true;
    }
    void DrawBit(IntPtr hdc,int x,int y,bool b,bool b0,bool b1) {
      int sx=x*size-scx,sy=y*size-scy;
      if(sx>=pMain.Width||sy>=pMain.Height) return;
      bool insel=InSel(x,y,z);
      bool xy=x==this.x&&y==this.y;  
      IntPtr br;
      if(b)
        br=b0?b1?AlmostBlack:Brown:b1?DarkBlue:Black;
      else 
        br=b0?b1?AlmostWhite:YellowWhite:b1?BlueWhite:insel?Green:White;
      RECT rec=new RECT();
      if(size>2) {
        rec.Left=sx;rec.Top=sy;rec.Right=rec.Left+size;rec.Bottom=rec.Top+size;
        GDI.FillRect(hdc,ref rec,br);
        rec.Left=sx+size-1;rec.Top=sy;rec.Right=rec.Left+1;rec.Bottom=sy+size;
        GDI.FillRect(hdc,ref rec,b?White:Black);
        rec.Left=sx;rec.Top=sy+size-1;rec.Right=sx+size;rec.Bottom=rec.Top+1;
        GDI.FillRect(hdc,ref rec,b?White:Black);
      } else {
        rec.Left=sx;rec.Top=sy;rec.Right=sx+size;rec.Bottom=sy+size;
        GDI.FillRect(hdc,ref rec,br);
      }
      if(xy) {
        rec.Top=sy+size/2;rec.Bottom=rec.Top+1;                
        GDI.FillRect(hdc,ref rec,b?White:Black);
        rec.Top=sy;rec.Bottom=sy+size;
        rec.Left=sx+size/2;rec.Right=rec.Left+1;
        GDI.FillRect(hdc,ref rec,b?White:Black);
      }  
    }
    void Draw(int x,int y,int width,int height) {
      if(mode!=ViewModes.Edit) return;
      b3d.Clip(0,ref x,ref width);
      b3d.Clip(1,ref y,ref height);
      if(width<1||height<1) return;
      Graphics gr=pMain.CreateGraphics();
      IntPtr hdc=gr.GetHdc();
      int z1=z<b3d.dz-1?z+1:-1,z0=z-1;      
      for(int j=0;j<height;j++)
        for(int i=0;i<width;i++)
          DrawBit(hdc,x+i,y+j,b3d.get(x+i,y+j,z),z0<0?false:b3d.get(x+i,y+j,z0),z1<0?false:b3d.get(x+i,y+j,z1));
      gr.ReleaseHdc();    
      gr.Dispose();
    }

    public void Cmd(object sender,EventArgs e) {
      Control c=sender as Control;
      string tag="";
      if(c!=null) {
        tag=""+c.Tag;
        if(tag=="") tag=""+c.Name;
      } else {
        ToolStripItem si=sender as ToolStripItem;
        tag=""+si.Tag;
        if(tag=="") tag=""+si.Name;
      }      
      switch(tag) {
       case "boundary":
        PushUndo();
        b3d.boundary(Board.SumMode());
        break;
       case "expand":
        PushUndo();
        if(SelectionEmpty()) b3d.expand(false,Board.SumMode());
        else b3d.expand(false,Board.SumMode(),SX0,SY0,SZ0,SX1-SX0+1,SY1-SY0+1,SZ1-SZ0+1);
        break;
       case "impand":       
        PushUndo();
        if(SelectionEmpty()) b3d.expand(true,Board.SumMode());
        else b3d.expand(true,Board.SumMode(),SX0,SY0,SZ0,SX1-SX0+1,SY1-SY0+1,SZ1-SZ0+1);
        break;
       case "not":
        if(SelectionEmpty()||SelectedAll()) b3d.inv();
        else {
          PushUndo();
          b3d.operation(bit_op.Not,SX0,SY0,SZ0,SX1-SX0+1,SY1-SY0+1,SZ1-SZ0+1);
        }
        break;
       case "sum":{
        int rule, count=0;
        if(!(fBoard.RuleParse(Board.eRule.Text, out rule) && rule!=0 && int.TryParse(Board.eCount.Text, out count))) return;
        if (count < 1) count = 1;
        b3d.sumfilter(Board.SumMode(), rule, false, count);        
       } break;
       case "filtersum":
        Board.Show(1);
        break;
      }
      Invalid();
    }

    void ChangeZ(int nz) {
      if(nz<0) nz=0;else if(nz>=b3d.dz) nz=b3d.dz-1;      
      tZ.Value=nz;
      if(z==nz) return;
      eZ.Text=(z=nz).ToString();
      if(mop==Tools.Select) {
        SZ1=nz;
        UpdateSele();
      }
      Invalid();       
    }
    void Rotate(int axis,bool dir,ref int x,ref int y,ref int z) {
      int x2,y2;
      switch(axis) {
       case 0:
        if(dir) { y2=y;y=z;z=b3d.dy-y2-1;}
        else  { y2=y;y=b3d.dz-z-1;z=y2;}
        break;       
       case 1:
        if(dir) { x2=x;x=b3d.dz-z-1;z=x2;}
        else  { x2=x;x=z;z=b3d.dx-x2-1;}
        break;
       case 2:
        if(dir) { x2=x;x=b3d.dy-y-1;y=x2;}
        else  { x2=x;x=y;y=b3d.dx-x2-1;}
        break;
      }
    }
    void Cuts(int axis) {
      PushUndo();
      b3d.cuts(axis,null,0);
      Invalid();
    }
    void And() {
      PushUndo();
      b3d.and(z);
      Invalid();
    }

    void Lathe(bool shift) {
      if(SelectionEmpty()&&(mmy!=y||mmx!=x)) {
        Lathe1(x,y,mmx,mmy);
      } else if(!SelectionEmpty()) {
        Lathe3(bit3d.abs(mmx-x)>bit3d.abs(mmy-y)); 
      }
    }
    void Lathe1() {
      if(SelectionEmpty()) return;
      if(bit3d.sqr(x-SX0,y-SY0)<bit3d.sqr(x-SX1,y-SY1))
        Lathe1(SX0,SY0,SX1,SY1);
      else
        Lathe1(SX1,SY1,SX0,SY0);
    }
    void Lathe1(int x0,int y0,int x1,int y1) {
      int z0=SZ0,z1=SZ1;
      if(SelectionEmpty()) z0=z1=z;
      int xd=bit3d.abs(x0-x1),yd=bit3d.abs(y0-y1),d=bit3d.max(xd,yd),sx=0,sy=0;
      if(xd>=yd) sx=bit3d.sgn(x1,x0);else sy=bit3d.sgn(y1,y0);
      PushUndo();
      for(int i=1;i<=d;i++) {
        int ax=x0+i*sx,ay=y0+i*sy;
        if(ax<0||ax>=b3d.dx||ay<0||ay>=b3d.dy) continue;
        for(int z2=z0;z2<=z1;z2++) {
          if(!b3d.get(ax,ay,z2)) continue;
          for(int j=0;j<=2*i;j++) {
            int j2=j/2,j1=j&1;
            double a=j*Math.PI/i/8,co=Math.Cos(a),si=Math.Sin(a),r=i+0.5;
            int dx=(int)(r*co),dy=(int)(r*si);
            b3d.chset(x0,y0,z2,dx,dy,0,true);
            b3d.chset(x0,y0,z2,dy,dx,0,true);
          }
        }
      }
      Invalid();
    }
    void Lathe3(bool yaxis) {
      if(SelectionEmpty()) return;
      PushUndo();
      if(yaxis) 
        for(int ax=SX0;ax<=SX1;ax++) {
          int rx=bit3d.abs(ax-x);
          if(rx==0) continue;
          for(int ay=SY0;ay<=SY1;ay++) {
            if(!b3d.get(ax,ay,z)) continue;
            for(int j=0;j<=2*rx;j++) {
              int j2=j/2,j1=j&1;
              double a=j*Math.PI/rx/8,co=Math.Cos(a),si=Math.Sin(a),r=rx+0.5;
              int dx=(int)(r*co),dy=(int)(r*si);
              b3d.chset(x,ay,z,dx,0,dy,true);
              b3d.chset(x,ay,z,dy,0,dx,true);
            }
          }
        }
      else
        for(int ay=SY0;ay<=SY1;ay++) {
          int ry=bit3d.abs(ay-y);
          if(ry==0) continue;
          for(int ax=SX0;ax<=SX1;ax++) {
            if(!b3d.get(ax,ay,z)) continue;
            for(int j=0;j<=2*ry;j++) {
              int j2=j/2,j1=j&1;
              double a=j*Math.PI/ry/8,co=Math.Cos(a),si=Math.Sin(a),r=ry+0.5;
              int dx=(int)(r*co),dy=(int)(r*si);
              b3d.chset(ax,y,z,0,dx,dy,true);
              b3d.chset(ax,y,z,0,dy,dx,true);
            }
          }
        }
      Invalid();    
    }
    void RotateSel(bool dir) {
      int wx=SX1-SX0,wy=SY1-SY0;
      if(wx>wy) wx=wy;
      if(wx<1) return;
      PushUndo();
      int x0=SX0,y0=SY0,x1=x0+wx,y1=y0+wx;
      while(x0<x1) {
        for(int z=SZ0;z<=SZ1;z++) {
          for(int i=0;i<wx;i++) {
            bool b0=b3d.get(x0+i,y0,z),b1=b3d.get(x1,y0+i,z),b2=b3d.get(x1-i,y1,z),b3=b3d.get(x0,y1-i,z);
            if(dir) { bool x=b1;b1=b3;b3=x;x=b0;b0=b2;b2=x;}
            b3d.set(x0+i,y0,z,b1);b3d.set(x1,y0+i,z,b2);b3d.set(x1-i,y1,z,b3);b3d.set(x0,y1-i,z,b0);          
          }
        }
        x0++;x1--;y0++;y1--;wx-=2;
      }
      Invalid();
    }
    void Rotate(int axis,bool dir) {
      int dx=b3d.dx,dy=b3d.dy;
      Rotate(axis,dir,ref x,ref y,ref z);
      if(SX0>=0) {
        Rotate(axis,dir,ref SX0,ref SY0,ref SZ0);
        Rotate(axis,dir,ref SX1,ref SY1,ref SZ1);
        NormaliseSelection();
      }
      if(axis==0||axis==1)
        eZ.Text=z.ToString();
      b3d.rotate(axis,dir);
      UpdateDZ();
      tZ.Value=z;
      UpdateScroll();
      Invalid();
    }
    private void bZDown_Click(object sender,EventArgs e) {
      ChangeZ(z+1);
    }

    private void bZUp_Click(object sender,EventArgs e) {
      ChangeZ(z-1);
    }

    private void eZ_KeyDown(object sender,KeyEventArgs e) {
      if(e.KeyCode!=Keys.Enter) return;
      int nz;
      if(int.TryParse(eZ.Text,out nz))
        ChangeZ(nz); 
    }

    private void eSize_KeyDown(object sender,KeyEventArgs e) {
     if(e.KeyCode!=Keys.Enter) return;
     ApplyCubeSize();
    }
    void ApplyCubeSize() {
     int ns;
     if(int.TryParse(eSize.Text,out ns)&&ns>=1&&ns<100)
       ChangeCubeSize(ns);
    }

    private void ePen_KeyUp(object sender,KeyEventArgs e) {
     if(e.KeyCode!=Keys.Enter) return;
     UpdatePen(0);
    }
    void UpdatePen(int delta) {    
     int i;
     if(!int.TryParse(ePen.Text,out i)) return;
     i+=delta;
     if(i>=1&&i<100) {
       ChangePenSize(i);
     }
    }
    private void lPen_MouseDown(object sender, MouseEventArgs e) {
      UpdatePen(e.Button==MouseButtons.Right?-1:+1);
    }
    void PenOp(bit3d b3d,bit_op op,int x,int y,int z,object param) {      
      b3d.operation(op,x-brush.dx/2,y-brush.dy/2,z-brush.dz/2,brush,0,0,0,brush.dx,brush.dy,brush.dz);      
    }
    void PenOp(MouseButtons bt,int x,int y) {
      bit_op op=0!=(bt&MouseButtons.Left)?bit_op.Or:bit_op.Sub;
      PenOp(b3d,op,x,y,z,null);
      Draw(x-brush.dx/2,y-brush.dy/2,brush.dx,brush.dy);
    }
    private void newToolStripMenuItem_Click(object sender,EventArgs e) {
      Board.ShowDialog(this);
    }
    void Rotate(int axis,double angle,bool headonly) {
      cam.Rotate(axis,angle,headonly&&cam.speed2!=0);
      lazydraw=true;
    }
    void MirrorMesh(bool x,bool y,bool z,bool reverse) {
      msh.Mirror(x,y,z,reverse);
      lazydraw=true;
    }
    void RotateMesh(int axis,bool back,bool reverse) {
      msh.Rotate(axis,back,reverse);
      lazydraw=true;
    }
    void ViewMove(int axis,double step) {
      cam.Move(axis,step);
      lazydraw=true;
    }
    void MoveAround(int x,int y,bool z) {
      cam.MoveAround(x,y,z);
      lazydraw=true;
    } 
    void FaceFilter(bool shift,bool ctrl,bool right) {      
     if(msh==null) return;
     if(ctrl&&shift) {
       if(right) msh.fcs.HideNext(true);
       else msh.fcs.Hide(2,8,true);
     } else if(shift) Command(right?"FilterFront":"FilterBack");
     else msh.fcs.Hide(false);
     lazydraw=true;
    }
    public void SelectAll() {
      if(SelectedAll())
        SelectionClear();          
      else {  
        SX0=SY0=SZ0=0;
        SX1=b3d.dx-1;SY1=b3d.dy-1;SZ1=b3d.dz-1;          
      } 
      Draw(); 
    }
    public static int atoiex(string s,int def) {
      if(s==null||s=="") return def;
      int sum=0,prod=0;
      char op='.';
      bool neg=false,l2=false,dig=false;
      int i=0;
      for(int h=0;h<s.Length;h++) 
        if(char.IsDigit(s[h])) {i=i*10+s[h]-'0';dig=true;}
        else if(s[h]=='+'||s[h]=='-') { 
          if(dig) {
            if(neg) i=-i;
            if(l2) i=op=='*'?prod*i:op=='/'?prod/i:prod%i;
            sum+=i;
            l2=false;dig=false;i=0;
          } 
          neg^=s[h]=='-';
        } else if(s[h]=='*'||s[h]=='/'||s[h]=='%') {
          if(dig) {
            if(neg) i=-i;
            if(l2) prod=op=='*'?prod*i:op=='/'?prod/i:prod%i;
            else {l2=true;prod=i;;}
            op=s[h];
            dig=false;i=0;
          }
        }
      if(dig) {
        if(neg) i=-i;
        if(l2) i=op=='*'?prod*i:op=='/'?prod/i:prod%i;
        sum+=i;
      }
      return sum;
    }
    
    bool ProcessCmdKeyPers(bool ctrl,bool shift,Keys key) {
      double step=0.5;
      switch(key) {
       case Keys.D2:
        if(ctrl) Export2D(shift);
        return ctrl;
       case Keys.D3:
        if(ctrl) SwitchStereo(CtrlRKey);
        return ctrl;
       case Keys.B:
        if(ctrl) mFileExportBmp_Click(null,null);
        return true; 
       case Keys.V:
        if(ctrl) mFileExportVector_Click(null,null);
        return true;
       case Keys.F10:
       case Keys.Space:Board.Show(-1);return true;
       case Keys.J:if(ctrl) cam.eyea=0;else cam.eyea+=shift?-0.01:0.01;lazydraw=true;return true;
       case Keys.K:cam.eyes*=ctrl?-1:shift?0.9:1.1;lazydraw=true;return true; 
       case Keys.N:SetMode(ViewModes.Edit,Conversion.Normal);return true;
       case Keys.R:msh.Relax(0.25,1,1);if(color_mode!=ColorModes.Random&&color_mode!=ColorModes.Three) lazycolor=true;else lazydraw=true;return true;
       case Keys.P:if(ctrl) mFilePrint_Click(null,null);else msh.Precise(!shift);lazydraw=true;return true;
       case Keys.T:msh.fcs.Triangulation();lazydraw=true;return true;
       case Keys.M:
        if(conv==Conversion.File) ImportMesh(importfile,2);
        else SetMode(ViewModes.Mesh, (Conversion.Normal|(conv&Conversion.Cubes))^(shift?Conversion.Cubes:0));
        return true;
       case Keys.C:if(ctrl) if(shift) CopyEMF();else CopyBitmap();else if(shift) mViewNormalRGB_Click(null,null);else mMeshColor_Click(null,null);return true;
       case Keys.U:Rotate(1,180,shift);return true;
       case Keys.A:Rotate(1,5,shift);return true;
       case Keys.D:Rotate(1,-5,shift);return true;
       case Keys.W:Rotate(0,-5,shift);return true;
       case Keys.S:if(ctrl) { ExportMesh(shift);} else Rotate(0,5,shift);return true;
       case Keys.Q:Rotate(2,5,shift);return true;
       case Keys.E:Rotate(2,-5,shift);return true;
       case Keys.G:FaceFilter(shift,ctrl,ShiftRKey);return true;
       case Keys.F:if(shift||ctrl) RotateMesh(ctrl?shift?0:2:1,CtrlRKey||ShiftRKey,false);return true;
       case Keys.X:MirrorMesh(!ctrl&&!shift,ctrl,shift,false);return true;
       case Keys.L:SetLight(shift,ctrl);return true;
       case Keys.Left:if(shift) MoveAround(-20,0,false);else ViewMove(0,-step);return true;
       case Keys.Right:if(shift) MoveAround(20,0,false);else ViewMove(0,+step);return true;
       case Keys.Up:if(shift) MoveAround(0,-20,false);else if(ctrl) ViewMove(1,-step);else ViewMove(2,+step);return true;
       case Keys.Down:if(shift) MoveAround(0,20,false);else if(ctrl) ViewMove(1,+step);else ViewMove(2,-step);return true;
       case Keys.Z:if(ctrl) Undo(shift);return true;
       case Keys.Y:if(ctrl) Undo(!shift);return true;
       case Keys.Add:
        if(cam.speed2==0) {
          cam.Fly(-1);
        } else
          cam.speed*=cam.speed2>0?8.0/9:9.0/8;          
        return true;
       case Keys.Subtract:
        if(cam.speed2==0) {
          cam.Fly(1);
        } else
          cam.speed*=cam.speed2<0?8.0/9:9.0/8;
        return true;
       case Keys.Enter:
        if(ctrl) { cam.HeadReset();lazydraw=true;
        } else if(shift) cam.BodyReset();
        else if(cam.speed2==0) return false;        
        cam.Fly(0);
        return true;
       default:return false;
      }
    }
    protected override bool ProcessCmdKey(ref Message msg,Keys keyData) {
      bool ctrl=0!=(keyData&Keys.Control);
      bool alt=0!=(keyData&Keys.Alt);
      bool shift=0!=(keyData&Keys.Shift);
      Keys key=keyData&~(Keys.Control|Keys.Alt|Keys.Shift);
      if((alt&&key!=Keys.D1&&key!=Keys.Oem3)||keyData==(Keys.Menu|Keys.Alt)) return base.ProcessCmdKey(ref msg, keyData);
      switch(key) {
       case Keys.CapsLock:
        bool b=base.ProcessCmdKey(ref msg,keyData);
        SetHeadOnly(CapsLock);
        if(!cam.headonly) {cam.HeadReset();lazydraw=true;}      
        return b;
       case Keys.Escape:
        if(!MainMenuStrip.Visible) {
          Fullscreen(false);
          return true;
        } if(mode==ViewModes.Mesh) {
          SetMode(ViewModes.Edit,Conversion.Normal);
          return true;
        } else
          return false;
       case Keys.F10:
        Board.Show(-1);
        return true;   
       case Keys.F11:
        Fullscreen(FormBorderStyle!=FormBorderStyle.None);
        return true;
       case Keys.O:SetMode(ViewModes.Mesh,(Conversion.Rotoid|(conv&Conversion.Cubes))^(shift?Conversion.Cubes:0));return true;
      }  
      if(mode==ViewModes.Mesh)
        return ProcessCmdKeyPers(ctrl,shift,key);
      switch(keyData&~Keys.Shift) {
       case Keys.Alt|Keys.Oem3:Board.Show(0);return true;
       case Keys.Alt|Keys.D1:Board.Show(1);return true;
       case Keys.Control|Keys.Space:
       case Keys.Space:if(ctrl||shift) ChangeZ(z+(ctrl?-1:1));else Board.Show(-1);return true;
       case Keys.E:Mirror(shift?1:0,true);return true;
       case Keys.Control|Keys.R:Lathe(!shift);return true;
       case Keys.R:if(SelectionEmpty()) Rotate(2,!shift);else RotateSel(!shift);return true;
       case Keys.T:Rotate(1,!shift);return true;
       case Keys.W:Rotate(0,!shift);return true;
       case Keys.Control|Keys.D0:AutoSize();return true;
       case Keys.P:MirrorX((int)(2*fx+0.5),ShiftDown);return true;
       case Keys.Control|Keys.N:Board.Show(0);return true;
       case Keys.M:SetMode(ViewModes.Mesh,ctrl?Conversion.Cubes:Conversion.Normal);return true;       
       case Keys.Control|Keys.Home:
        Scroll2(false,shift?-b3d.dx:-10);
        return true; 
       case Keys.Control|Keys.End:
        Scroll2(false,shift?b3d.dx:+10);
        return true; 
       case Keys.Control|Keys.PageUp:
       case Keys.PageUp:
        if(shift) Scroll2(true,ctrl?-b3d.dy:-10);
        else ChangeZ(ctrl?0:z-1);
        return true;
       case Keys.Control|Keys.PageDown:
       case Keys.PageDown:
        if(shift) Scroll2(true,ctrl?b3d.dy:+10);
        else ChangeZ(ctrl?b3d.dz:z+1);
        return true;
       case Keys.Control|Keys.A:SelectAll();break;
       case Keys.Control|Keys.F:
       case Keys.F:
        PushUndo();b3d.floodfill2d(mmx,mmy,z,!b3d.get(mmx,mmy,z),ctrl?shift?3:2:shift?1:0,chFillDown.Checked);Invalid();break;
       case Keys.X:
        Extrude(shift,-1,-1,false);
        break;
       case Keys.V:Extrude(shift,(int)(2*fx+0.5),(int)(2*fy+0.5),false);break;
       case Keys.B:Extrude(shift,(int)(2*fx+0.5),(int)(2*fy+0.5),true);break;
       case Keys.Control|Keys.T:
        Twist(shift,(int)(2*fx+0.5),(int)(2*fy+0.5),(shift?-1:1)*atoiex(eTwistAngle.Text,360));
        break;
       case Keys.Control|Keys.Z:
        Undo(shift);
        break; 
       case Keys.Control|Keys.Y:
        Undo(!shift);
        break; 
       case Keys.Control|Keys.C: //Copy
        mEdit_Click("copy",null);
        break; 
       case Keys.Control|Keys.X: //Cut
        mEdit_Click("cut",null);
        break;
       case Keys.Control|Keys.V://Paste
        Paste(mmx,mmy,shift?bit_op.Copy:bit_op.Or);
        break; 
       case Keys.Control|Keys.S:
        SaveFile(shift);
        break;
       case Keys.Control|Keys.Delete:
       case Keys.Delete:
        mEdit_Click("delete",null); 
        break;
      }
      return base.ProcessCmdKey(ref msg,keyData);
    }

    private void bRot_Click(object sender,EventArgs e) {
      string rot=""+(sender as Control).Tag;
      int axis=rot[0]=='Y'?1:rot[0]=='Z'?2:0;
      bool dir=rot[1]=='P';
      if(axis==2&&ShiftDown&&!SelectionEmpty()) RotateSel(dir);
      else Rotate(axis,dir);
    }

    protected override void OnPaintBackground(PaintEventArgs e) {
      //e.Graphics.FillRectangle(Brushes.LightGray,Panel1Width,0,Width-Panel1Width,Height);
      // exclude panel1 area
      //base.OnPaintBackground(e);
    }

    protected override void OnClosed(EventArgs e) {
      base.OnClosed(e);
      closed=true;
    }



    private void mViewMesh_Click(object sender,EventArgs e) {
      bool pers=!mViewMesh.Checked;
      SetMode(pers?ViewModes.Mesh:ViewModes.Edit,CtrlDown?Conversion.Cubes:Conversion.Normal);
    }

    private void mFileExit_Click(object sender,EventArgs e) {
      Close();
    }

    void Finished(string text) {
      MessageBox.Show(this,text,"Finished");
    }
    private void mFileExportOff_Click(object sender,EventArgs e) {
      ExportMesh(!ShiftDown);
    }
    bool ExportMesh(bool saveas) {    
      if(msh==null) return false;
      bool colors=ShiftDown;
      if(string.IsNullOrEmpty(exportfile)||saveas) {
        sfd.FileName=exportfile==null?"out.obj":exportfile;
        sfd.Title="Export mesh";
        sfd.Filter = "3DS files|*.3ds|wavefront (*.obj)|*.obj|*.off|*.off|povray (*.pov)|*.pov|STereoLithography (*.stl)|*.stl|vrml (*.wrl)|*.wrl|*.*|*.*";
        sfd.FilterIndex=exportfidx;
        sfd.DefaultExt="obj";
        if(DialogResult.OK!=sfd.ShowDialog(this)) return false;
        exportfile=sfd.FileName;
        exportfidx=sfd.FilterIndex;
      }
      string ext=Path.GetExtension(exportfile).ToUpperInvariant();
      string err=null;
      double[] add=null,mul=null;
      msh.Permute(exportperm,false);
     try {
      if(exportresize!=null||exportscale!=null) {
        double[] min=d3.V(),max=d3.V();
        msh.pts.MinMax(min,max);
        double[] r3=d3.V(),a3=d3.V();
        if(exportscale!=null&&mesh.ParseDouble3Exp(r3,exportscale)) mul=d3.V(r3[0],r3[1],r3[2]);
        if(exportresize2!=null&&mesh.ParseDouble3Exp(r3,exportresize)&&mesh.ParseDouble3Exp(a3,exportresize2)) msh.pts.Resize(exportresizeflags,mul,r3,a3,out mul,out add);
        else if(exportresize!=null&&mesh.ParseDouble3Exp(r3,exportresize)) msh.pts.Resize(exportresizeflags,mul,d3.V0,r3,out mul,out add);
        
      }
      switch(ext) {
       case ".WRL":err=msh.ExportVrml(exportfile,mul,add,mesh_color,light_vec);break;
       case ".OFF":err=msh.ExportOff(exportfile,mul,add);break;
       case ".POV":err=msh.ExportPov(exportfile,mul,add,true,colors);break;
       case ".3DS":err=msh.Export3ds(exportfile,mul,add);break;
       case ".STL":err=msh.ExportStl(exportfile,mul,add);break;
       default:err=msh.ExportObj(exportfile,mul,add);break;         
      }         
     } finally {
      msh.Permute(exportperm,true);
     }
      Finished(err!=null?err:"export "+exportfile);
      return true;
    }

    private void bPen_Click(object sender,EventArgs e) {
      bool mod=CtrlDown||ShiftDown;
      SetTool(sender==bLine?Tools.Line:sender==bBox?mod?Tools.Rect:Tools.Box:sender==bCyl?mod?Tools.Elipse:Tools.Cylinder:sender==bSph?Tools.Sphere:sender==bCone?Tools.Cone:Tools.Pen);
    }

    private void bFill_Click(object sender,EventArgs e) {
      SetTool(Tools.Fill3D);
    }

    private void bFill2D_Click(object sender,EventArgs e) {
      SetTool(Tools.Fill2D);
    }

    private void mFilterExtend_Click(object sender,EventArgs e) {
      PushUndo();
      b3d.extend(false,1);
      UpdateDZ();
      Invalid();
    }

    void Mirror(int axis,bool sele) {
      if(sele&&!SelectionEmpty()) {
        b3d.mirror(axis,SX0,SY0,SZ0,SX1,SY1,SZ1);
        if(axis==0) x=SX0+SX1-x;
        if(axis==1) y=SY0+SY1-y;
        if(axis==2) ChangeZ(SZ0+SZ1-z);
      } else {
        b3d.mirror(axis);
        if(axis==0) x=b3d.dx-1-x;
        if(axis==1) y=b3d.dy-1-y;
        if(axis==2) ChangeZ(b3d.dz-1-z);     
      }
      Invalid();
    }
    private void bMirrZ_Click(object sender,EventArgs e) {
      int axis=CtrlDown?1:2;
      Mirror(axis,ShiftDown);
    }

    private void mOpDoubleSize_Click(object sender,EventArgs e) {
      PushUndo();
      b3d.resize(2*b3d.dx,2*b3d.dy,2*b3d.dz);
      UpdateDZ();
      Invalid();
    }
    void ClearUndo() {
      undos.Clear();
      undoc=0;
      undo=null;      
    }
    void UpdateUndo() {
      mEditUndo.Enabled=undoc>0;
      mEditRedo.Enabled=undoc<undos.Count;
    }
    void GetUndo(undoitem u) {
      u.SX0=SX0;u.SX1=SX1;u.SY0=SY0;u.SY1=SY1;u.SZ0=SZ0;u.SZ1=SZ1;
      u.CX=CX;u.CY=CY;u.CZ=CZ;
      u.x=x;u.y=y;u.z=z;
      u.scx=scx;u.scy=scy;u.size=size;
    }
    void SwapUndo(undoitem u) {
      int r;
      r=SX0;SX0=u.SX0;u.SX0=r;r=SY0;SY0=u.SY0;u.SY0=r;r=SZ0;SZ0=u.SZ0;u.SZ0=r;
      r=SX1;SX1=u.SX1;u.SX1=r;r=SY1;SY1=u.SY1;u.SY1=r;r=SZ1;SZ1=u.SZ1;u.SZ1=r;
      r=CX;CX=u.CX;u.CX=r;r=CY;CY=u.CY;u.CY=r;r=CZ;CZ=u.CZ;u.CZ=r;
      r=x;x=u.x;u.x=r;r=y;y=u.y;u.y=r;
      r=z;ChangeZ(u.z);u.z=r;      
      r=size;ChangeCubeSize(u.size);u.size=r;
      r=scx;scx=u.scx;u.scx=r;r=scy;scy=u.scy;u.scy=r;
      if(scx<0) scx=0;if(scy<0) scy=0;
      pMain.HorizontalScroll.Value=scx;
      pMain.VerticalScroll.Value=scy;
    }
    void PushUndo() {
      undoitem ui=undoc<undos.Count?undos[undoc]:new undoitem();
      GetUndo(ui);
      undo=ui.undo=b3d.Clone();
      undos.RemoveRange(undoc,undos.Count-undoc);
      if(undoc>UndoMax) undos.RemoveAt(0);
      undos.Add(ui);  
      undoc=undos.Count;
      SetDirty();
      UpdateUndo();
    }
    public void Undo(bool redo) {
      if(redo) {
        if(undoc>=undos.Count) return;
        undoitem ui=undos[undoc];
        b3d.swap(ui.undo);
        SwapUndo(ui);
        undo=ui.undo;
        undoc++;
      } else {
        if(undoc<1) return;
        undoc--;
        undoitem ui=undos[undoc];
        b3d.swap(undo);
        SwapUndo(ui);
        undo=undoc<1?null:undos[undoc-1].undo;
      }
      Invalid();
      UpdateUndo();
    }

    private void bAutoSize_Click(object sender,EventArgs e) { AutoSize();}
    private void AutoSize() { 
      int sx=pMain.Width/b3d.dx,sy=pMain.Height/b3d.dy;
      ChangeCubeSize(sx<sy?sx:sy); 
    }

    private void mFileSave_Click(object sender,EventArgs e) {
      SaveFile(false);      
    }

    protected override void OnClosing(CancelEventArgs e) {
      if(!CheckDirty("Close window")) e.Cancel=true;
    }
    bool CheckDirty(string caption) {
       if (!Dirty) return true;
       DialogResult dr=MessageBox.Show(this,"Save changes?",caption,MessageBoxButtons.YesNoCancel,MessageBoxIcon.Exclamation,MessageBoxDefaultButton.Button3);
       if(dr!=DialogResult.Yes) return dr==DialogResult.No;
       return SaveFile(false);
    }
      
    void LoadFile(string filename,bool update) {
      b3d.open(filename);
      ChangeFileName(filename);
      ClearUndo();
      if(update) { Invalid();}
    }
    bool SaveFile(bool saveas) {
      if(string.IsNullOrEmpty(ofd.FileName)||saveas) {
        sfd.Title="Save as";
        sfd.FileName=ofd.FileName;
        sfd.Filter="*.cub|*.cub|*.cub.gz|*.cub.gz|*.*|*.*";
        sfd.DefaultExt="cub";
        string dir=Directory.GetCurrentDirectory();
        if(DialogResult.OK!=sfd.ShowDialog(this)) return false;
        string fname=sfd.FileName;
        if(Path.GetExtension(fname)=="") fname+=".b3d";
        ChangeFileName(sfd.FileName);
        Directory.SetCurrentDirectory(dir);
      }
      b3d.save(ofd.FileName);
      UnsetDirty();
      return true;
    }
    void SetDirty() {
      if(Dirty) return;
      Dirty=true;
      if(!Text.EndsWith("*")) Text+="*";
    }
    void UnsetDirty() {
      if(!Dirty) return;
      Dirty=false;
      if(Text.EndsWith("*")) Text=Text.Substring(0,Text.Length-1);        
    }


    private void mFileSaveAs_Click(object sender,EventArgs e) {
      SaveFile(true);
    }

    int ColorIntensity(int color,int x) {
      int r=(color&255)*x/256,g=((color>>8)&255)*x/256,b=((color>>16)&255)*x/256;
      if(r>255) r=255;
      if(g>255) g=255;
      if(b>255) b=255;
      return r|(g<<8)|(b<<16);
    }
    int Color2Int(Color c) {
      return c.R|(c.G<<8)|(c.B<<16);
    }
    Color Int2Color(int c) {
      return Color.FromArgb(c&255,(c>>8)&255,(c>>16)&255);
    }
    public void Colorize() {
      if(msh==null) return;
      //msh.ColorizePhong(0x004000,0x195919,0x72a572,3.0,0xffffff,light_vec);
      if(color_mode==ColorModes.NormalRGB) {
        msh.ColorizeNormalRGB(color_rota);
      } else if(color_mode==ColorModes.NormalRadial) {
        msh.ColorizeNormalRadial(color_rota,palette);
      } else if(color_mode==ColorModes.GradientY) {
        msh.ColorizeGradient(d3.V(-color_rota[3],-color_rota[4],-color_rota[5]),palette);
      } else if(color_mode==ColorModes.Sphere) {
        msh.ColorizeSphere(color_pos,palette);
      } else if(color_mode==ColorModes.CylinderZ) {
        msh.ColorizeCylinder(color_pos,d3.V(color_rota[6],color_rota[7],color_rota[8]),palette);
      } else if(color_mode==ColorModes.RadialZ) {
        msh.ColorizeRadial(color_pos,color_rota,palette);
      } else if(color_mode==ColorModes.Random) {
        msh.fcs.ColorizeRandom(2,new double[] {0,0.5,0.5,0.5 ,1,1,1,1});
      } else if(color_mode==ColorModes.Three) {
        msh.fcs.Colorize(new int[] {0x888888,0xcccccc,0xffffff});
      } else {
        int c1=ColorIntensity(mesh_color,0x80);//,c2=ColorIntensity(mesh_color,0x80);
        msh.ColorizePhong(c1,c1,0xffffff,3.0,0xffffff,light_vec);
      }  
    }
 
    void SetLight(bool reverse,bool shadow) {
      double rev=reverse?-1:1;
      if(color_mode!=ColorModes.Color&&!shadow) {
        color_rota=cam.rot.Clone() as double[];
        color_pos=cam.pos.Clone() as double[];
        if(reverse) {
          d3.mul_d(color_rota,6,color_rota,6,-1);
          d3.mul_d(color_rota,0,color_rota,0,-1);
        }  
      } else {
        light_vec[0]=rev*-cam.rot[6];
        light_vec[1]=rev*-cam.rot[7];
        light_vec[2]=rev*-cam.rot[8];
      }  
      lazycolor=true;
      revshadow=reverse;
      lazyshadow=shadow;
    }
    private void timer_Tick(object sender,EventArgs e) {
      if(invalid) {
        invalid=false;
        if(mode==ViewModes.Mesh) {          
          if(conv==Conversion.File) {            
          } else {
            meshsource=0;
            bool roto=0!=(conv&Conversion.Rotoid);
            bool cubes=0!=(conv&Conversion.Cubes);
            if(roto||b3d.dz>1) 
              msh=b3d.Mesh(cubes,roto);
            else {
              bit3d b2=new bit3d();
              b2.alloc(b3d.dx,b3d.dy,3);
              b2.copy(0,0,1,b3d,0,0,0,b3d.dx,b3d.dy,1);
              msh=b2.Mesh(cubes,false);
            }  
            if(!cubes) msh.pts.Scale(new double[3] {0.5,0.5,0.5});
            if(0!=(conv&Conversion.Rotoid)) {
              int n;
              int.TryParse(eRotoidCount.Text,out n);
              if(n<1) n=1;else if(n>64) n=64;
              mesh m2=n>0?new mesh(msh):null;
              int i=0;
              for(int j=0;j<n;j++) {
                double ba=2*Math.PI*j/n;
                if(j>0) msh.Append(m2);
                while(i<msh.pts.Count) {
                  double a=2*Math.PI*msh.pts.p[3*i+2]/b3d.dz;
                  double r=msh.pts.p[3*i];
                  msh.pts.p[3*i]=r*Math.Cos(ba+a/n);
                  msh.pts.p[3*i+2]=r*Math.Sin(ba+a/n);
                  i++;
                }
              }  
              msh.Join(0.005);
            }
          }  
          //msh.fcs.ColorizeRandom(2,new double[] {0,.2,0.2,0.2,1,1,1,1});
          lazycolor=true;
          double[] rd3=new double[3];
          msh.pts.MinMax(cam.center,rd3);
          d3.linear_interpol(cam.center,0.5,cam.center,rd3);
        }
        lazydraw=true;
      }
    }

    private void fmain_Paint(object sender,PaintEventArgs e) {
      lazydraw=true; 
    }
    
    void UpdateXY(int x,int dx,int y,int dy,bool size) {      
      int ax=dx,ay=dy;
      if(size) {
        ax=(dx<0?-1:1)*(1+Math.Abs(dx));
        ay=(dy<0?-1:1)*(1+Math.Abs(dy));
      }
      eX.Text=""+ax;eY.Text=""+ay;
    }
    void UpdateSele() {
      eSele0.Text=""+SX0+','+SY0+','+SZ0;
      int wx=bit3d.abs(SX1-SX0)+1,wy=bit3d.abs(SY1-SY0)+1,wz=bit3d.abs(SZ1-SZ0)+1;
      eSele1.Text=""+wx+','+wy+','+wz;
    }
    private void pMain_MouseDown(object sender,MouseEventArgs e) {
      if(0!=(e.Button&MouseButtons.Middle)) {
        SetMode(mode==ViewModes.Edit?ViewModes.Mesh:ViewModes.Edit,conv);
      }
      int ex=e.X+scx,ey=e.Y+scy;
      if(0!=(e.Button&(MouseButtons.Left|MouseButtons.Right))) {
        if(mode==ViewModes.Mesh) {          
          zx=ex;zy=ey;
          MeshMousePress(e);
          //Invalid();
        } else if(mode==ViewModes.Edit) {           
          int x2=x,y2=y;
          mpx=ex/size;mpy=ey/size;mpz=z;
          int px=ex/size;          
          int py=ey/size;
          if(px>=0&&px<b3d.dx&&py>=0&&py<b3d.dy) {
            x=px;y=py;
          }
          if(CtrlDown) { 
            Invalid();
            return;
          }
          Draw(x2,y2,1,1);          
          Tools t=tool;
          if(ShiftDown) t=Tools.Select;
          switch(t) {
           case Tools.Box:
           case Tools.Rect:
           case Tools.Sphere:
           case Tools.Cylinder:
           case Tools.Cone:
           case Tools.Elipse:
           case Tools.Line:
            mop=t;
            xori=true;DrawXor();
            break;
           case Tools.Pen:
            PushUndo();
            PenOp(e.Button,px,py);
            mop=t;
            break;
           case Tools.Fill3D:
            PushUndo();
            b3d.floodfill(px,py,z,0!=(e.Button&MouseButtons.Left),0,chFillDown.Checked);
            Invalid();
            break;
           case Tools.Fill2D:
            PushUndo();
            b3d.floodfill2d(px,py,z,0!=(e.Button&MouseButtons.Left),0,chFillDown.Checked);
            Invalid();
            break;  
           case Tools.Select:
            SX0=x;SY0=y;SZ0=z;SX1=x;SY1=y;SZ1=z;
            UpdateSele();
            mop=t;
            Invalid();
            break; 
          }          
        }  
      }  
    }

    void MeshMousePress(MouseEventArgs e) {
      mop=Tools.ViewLook;
      WINAPI.SetCapture(pMain.Handle);
      lbrx=rbrx=e.X;lbry=rbry=e.Y;
    }
    void MeshMouseMove(MouseEventArgs e) {
      double[] mx=d33.M(0),my=d33.M(0);
      int x=e.X,y=e.Y;
      bool ctrl=CtrlDown,shift=ShiftDown;
      if(0!=(e.Button&MouseButtons.Left)) {
        if(shift) {
          double[] rd3=d3.V();
          d33.rotation_aa(mx,cam.body,ctrl?6:3,(ctrl?1:-1)*(x-lbrx)*camera.mouse_angle);
          d33.rotation_aa(my,cam.body,0,+(y-lbry)*camera.mouse_angle);
          d3.sub(rd3,cam.pos,cam.center);
          d3.div_d33(rd3,rd3,mx);
          d3.div_d33(rd3,rd3,my);
          d3.add(cam.pos,cam.center,rd3);
          
          d33.rotation_aa(mx,ctrl?d3.VZ:d3.VY,0,(ctrl?-1:1)*(x-lbrx)*camera.mouse_angle);
          d33.rotation_aa(my,d3.VX,0,-(y-lbry)*camera.mouse_angle);
          d33.mul(cam.body,mx,cam.body);
          d33.mul(cam.body,my,cam.body);
          d33.normal(cam.body,cam.body);
          cam.HeadBody2Rota();
        } else {
          double[] rota=cam.headonly?cam.head:cam.body;
          d33.rotation_aa(mx,ctrl?d3.VZ:d3.VY,0,(ctrl?-1:-1)*(x-rbrx)*camera.mouse_angle);
          d33.rotation_aa(my,d3.VX,0,(y-rbry)*camera.mouse_angle);
          d33.mul(rota,mx,rota);
          d33.mul(rota,my,rota);
          d33.normal(rota,rota);
          cam.HeadBody2Rota();
        } 
        lazydraw=true;
      } else if(0!=(e.Button&MouseButtons.Right)) {
        bool octrl=ctrl&&!shift;
        double[] delta=d3.V();
        d3.mul_d(delta,0,cam.body,shift?3:0,(ctrl&&shift?-1:1)*(x-rbrx)*10*cam.speed);
        d3.add(cam.pos,cam.pos,delta);
        d3.mul_d(delta,0,cam.body,(octrl?3:6),(octrl?1:-1)*(y-rbry)*10*cam.speed);
        d3.add(cam.pos,cam.pos,delta);
        lazydraw=true;
      }
      
      lbrx=rbrx=e.X;lbry=rbry=e.Y;
    }
    void MeshMouseRelease(MouseEventArgs e) {
      WINAPI.ReleaseCapture();
      mop=Tools.None;
    }
    private void pMain_MouseMove(object sender,MouseEventArgs e) {
      mmx2=e.X;mmy2=e.Y;
      if(mode==ViewModes.Mesh) {        
        if(mop==Tools.ViewLook) MeshMouseMove(e);
        return;
      }
      fx=1f*(scx+e.X)/size;fy=1f*(scy+e.Y)/size;
      int ex=e.X+scx,ey=e.Y+scy;
      int x=ex/size,y=ey/size;
      if(xori) DrawXor();      
      UpdateXY(x,x-this.x,y,y-this.y,LButton||RButton);  
      if(mode==ViewModes.Edit) {        
        switch(mop) {
         case Tools.Pen:
          PenOp(e.Button,x,y);
          break;
         case Tools.Select:
          SX1=x;SY1=y;SZ1=z;
          UpdateSele();
          Invalid();
          break;
        }    
      }
      mmx=x;mmy=y;
      if(xori) DrawXor();      
    }
    private void pMain_MouseUp(object sender,MouseEventArgs e) {      
      int px,py,pz,pz2=mpz,ix,iy,iz,sx,sy,dx,dy,cax=2;
      double r2,rx2,ry2;
      bit_op bop=bop=CtrlDown?bit_op.Not:0!=(e.Button&MouseButtons.Right)?bit_op.Zero:bit_op.One;
      int rx=(scx+e.X)/size,ry=(scy+e.Y)/size,rz=z;
      if(pz2==rz&&(mop==Tools.Box||mop==Tools.Sphere||mop==Tools.Cylinder||mop==Tools.Cone)&&!rbD1n.Checked) {
        int dz;
        if(rbD1v.Checked) {
          dz=Math.Abs((int)mpx-rx);
          cax=1;
        } else if(rbD1h.Checked) {
          dz=Math.Abs((int)mpy-ry);
          cax=0;
        } else {
          int dax=Math.Abs((int)mpx-rx),day=Math.Abs((int)mpy-ry);
          if(dax<=day) {
            dz=dax;
            cax=1;
          } else {
            dz=day;
            cax=0;
          }          
        }
        pz2=z-dz/2;rz=z+(dz+1)/2;
      }
      switch(mop) {
       case Tools.ViewLook:
        MeshMouseRelease(e); 
        break;
       case Tools.Select:
        NormaliseSelection();
        UpdateSele();
        break; 
       case Tools.Rect:
       case Tools.Box:
        DrawXor();xori=false; 
        PushUndo();
        px=(int)mpx;py=(int)mpy;pz=pz2;
        b3d.Clip(ref px,ref rx,ref py,ref ry,ref pz,ref rz);
        if(mop==Tools.Rect) b3d.box(bop,px,py,pz,rx-px+1,ry-py+1,rz-pz+1,true);
        else b3d.operation(bop,px,py,pz,null,0,0,0,rx-px+1,ry-py+1,rz-pz+1);
        Invalid();
        break;
       case Tools.Sphere:
        DrawXor();xori=false;
        PushUndo();
        px=(int)mpx;py=(int)mpy;pz=pz2;        
        if(px==rx&&cax==2) {
          px+=(ry-py)/2;rx=px+py-ry;
        } else if(py==ry&&cax==2) {
          py+=(rx-px)/2;ry=py+px-rx;
        }
        if(pz==rz) {
          pz+=(rx-px)/2;rz=pz+px-rx;
        }
        b3d.sphere(bop,px,py,pz,rx,ry,rz,mop!=Tools.Sphere);
        Invalid();
        break;
       case Tools.Elipse:
       case Tools.Cylinder:
       case Tools.Cone:
        DrawXor();xori=false;
        PushUndo();
        px=(int)mpx;py=(int)mpy;pz=pz2;        
        if(px==rx&&cax==2) {
          px+=(ry-py)/2;rx=px+py-ry;
        } else if(py==ry&&cax==2) {
          py+=(rx-px)/2;ry=py+px-rx;
        }        
        if(mop==Tools.Cone) b3d.cone(bop,cax,px,py,pz,rx,ry,rz,false);
        else b3d.cylinder(bop,cax,px,py,pz,rx,ry,rz,mop==Tools.Elipse);
        Invalid();
        break;
       case Tools.Line:
        DrawXor();xori=false;
        PushUndo();
        bool b=brush.dx>1||brush.dy>1||brush.dz>1;
        if(b&&bop==bit_op.One) bop=bit_op.Or;
        else if(b&&bop==bit_op.Zero) bop=bit_op.Sub;
        b3d.Line(bop,(int)mpx,(int)mpy,mpz,rx,ry,rz,b?(Delegate3D)PenOp:null,null);
        Invalid();
        break;
      }
      mop=Tools.None;
    }

    public Bitmap CaptureBitmap(bool refresh) {
      Rectangle r=MainMenuStrip.Visible?pMain.ClientRectangle:Screen.PrimaryScreen.Bounds;     
      Point p0=new Point(0,0);
      if(MainMenuStrip.Visible) p0=pMain.PointToScreen(p0);
      //GDI.GetWindowRect(pMain.Handle,out r);
      if(refresh) Refresh();
      return CaptureBitmap(p0.X,p0.Y,r.Width,r.Height);
    }
    void CopyBitmap() {
      storeframe=true;
      Draw();
      lastframe.UpsideDown();
      Bitmap bmp=lastframe.ToBitmap();
      //Bitmap bmp=CaptureBitmap(true);
      Clipboard.SetImage(bmp);
      bmp.Dispose();
      lastframe=null;
    }
    private void mFileExportBmp_Click(object sender,EventArgs e) {      
      Export2D(true);
    }
    void Export2D(bool file) {
      if(file||exportBitmap==null) {
        sfd.Title="Export bitmap";
        sfd.FileName=exportBitmap==null?"out":exportBitmap;
        sfd.Filter="*.png|*.png|*.bmp|*.bmp|*.*|*.*";
        sfd.DefaultExt="png";      
        if(DialogResult.OK!=sfd.ShowDialog(this)) {
          return;
        }
        exportBitmap=sfd.FileName;
      }
      storeframe=true;
      Draw();
      lastframe.UpsideDown();
      //lastframe.BGR();      
      Bitmap bmp=lastframe.ToBitmap();//CaptureBitmap(true);
      lastframe=null;      
      bmp.Save(exportBitmap,exportBitmap.EndsWith(".bmp",StringComparison.InvariantCultureIgnoreCase)?ImageFormat.Bmp:ImageFormat.Png);
      bmp.Dispose();
    }

    
    /*private void mFileExport3D_Click(object sender,EventArgs e) {
      //Export3D(true);
    }
    void Export3D(bool file) {
      if(file) {
        sfd.Title="Export anaglyph";
        sfd.FileName=exportBitmap==null?"out":exportBitmap;
        sfd.Filter="*.png|*.png|*.bmp|*.bmp|*.*|*.*";
        sfd.DefaultExt="png";      
        if(DialogResult.OK!=sfd.ShowDialog(this)) {
          return;
        }
        exportBitmap=sfd.FileName; 
      }
      Bitmap bmp;
      bitmap bl,br;
      ViewMove(0,-1);
      Draw();
      Rectangle r=MainMenuStrip.Visible?pMain.ClientRectangle:Screen.PrimaryScreen.Bounds;     
      Point p0=new Point(0,0);
      if(MainMenuStrip.Visible) p0=pMain.PointToScreen(p0);
      bmp=CaptureBitmap(p0.X,p0.Y,r.Width,r.Height);
      //bmp=Capture(r.Left,r.Top,r.Right-r.Left,r.Bottom-r.Top);
      bl=bitmap.FromBitmap(bmp);
      bmp.Dispose();
      ViewMove(0,2);
      Draw();
      //bmp=Capture(p0.X,p0.Y,p1.X-p0.X,p1.Y-p0.Y);
      bmp=CaptureBitmap(p0.X,p0.Y,r.Width,r.Height);
      br=bitmap.FromBitmap(bmp);
      bmp.Dispose();
      ViewMove(0,-1);
      Draw();
      lazydraw=false;
      bl.Anaglyph(br);
      bmp=bl.ToBitmap();
      if(file) 
        bmp.Save(exportBitmap,exportBitmap.EndsWith(".bmp",StringComparison.InvariantCultureIgnoreCase)?ImageFormat.Bmp:ImageFormat.Png);
      Application.DoEvents();
      Graphics gr=pMain.CreateGraphics();
      bl.Draw(gr,0,0);
      gr.Dispose();
    }*/
    
    string ExportEMF(double[] pts,int w,int h,string filename) {
      Graphics gr=pMain.CreateGraphics();
      string error=msh.ExportEMF(pts,mMeshWires.Checked?mMeshWhite.Checked?0xffffff:0:-1,!mMeshWires.Checked&&!mMeshWhite.Checked,(int)lwidth,wirefront,w,h,2*cam.angle,cam.zmin,filename,gr.GetHdc());
      gr.ReleaseHdc();
      gr.Dispose();
      return error;
    }
    void CopyEMF() {
      double[] pts=cam.Points(msh);
      int w=pMain.ClientRectangle.Width,h=pMain.ClientRectangle.Height;//-(MainMenuStrip.Visible?MainMenuStrip.Height:0);            
      ExportEMF(pts,w,h,null);
    }
    private void mFileExportVector_Click(object sender,EventArgs e) {
      if(msh==null) return;
      sfd.Title="Export vector";
      sfd.FileName=exportVector==null?"out":exportVector;
      sfd.Filter="*.emf|*.emf|*.pdf|*.pdf|*.svg|*.svg|*.*|*.*";
      sfd.DefaultExt="emf";
      if(DialogResult.OK!=sfd.ShowDialog(this)) return;
      exportVector=sfd.FileName;      
      double[] pts=cam.Points(msh);
      string error;
      int w=pMain.ClientRectangle.Width,h=pMain.ClientRectangle.Height;//-(MainMenuStrip.Visible?MainMenuStrip.Height:0);      
      switch(Path.GetExtension(sfd.FileName).ToUpperInvariant()) {
       case ".EMF": 
        error=ExportEMF(pts,w,h,sfd.FileName);
        break;
       case ".PDF":
        error=msh.ExportPDF(pts,mMeshWires.Checked?mMeshWhite.Checked?0xffffff:0:-1,!mMeshWires.Checked&&!mMeshWhite.Checked,lwidth,wirefront,w,h,2*cam.angle,cam.zmin,sfd.FileName); 
        break;        
       default:
        error=msh.ExportSVG(pts,mMeshWires.Checked?mMeshWhite.Checked?0xffffff:0:-1,!mMeshWires.Checked&&!mMeshWhite.Checked,lwidth,wirefront,w,h,2*cam.angle,cam.zmin,sfd.FileName);
        break;
      }  
      if(error!=null) MessageBox.Show(error);
    }

    private void openToolStripMenuItem_Click(object sender,EventArgs e) {
      OpenFile();
    }
    void OpenFile() {
      if (!CheckDirty("Open file")) return;
      string dir=Directory.GetCurrentDirectory();
      ofd.Title="Open";
      ofd.Filter="*.cub|*.cub|*.cub.gz|*.cub.gz|*.*|*.*";
      ofd.DefaultExt="cub";
      if(DialogResult.OK==ofd.ShowDialog(this)) {
        if(undo==null) undo=new bit3d();
        b3d.swap(undo);
       try { 
        b3d.open(ofd.FileName);
        UpdateDZ();
        SelectionClear();
        ChangeZ(b3d.dz/2);        
        ChangeFileName(ofd.FileName);
        if(mode==ViewModes.Mesh) invalid=true;
        else Draw();
       } catch(Exception ex) {
        MessageBox.Show(this,ex.Message,"Exception",MessageBoxButtons.OK,MessageBoxIcon.Error);
       } 
      }
      Directory.SetCurrentDirectory(dir);
    }

    private void mFilterSum_Click(object sender,EventArgs e) {
      Board.Show(1);
    }

    void CopyCut(bool cut) {
      CX=x-SX0;CY=y-SY0;CZ=z-SZ0;
      int wx=SX1-SX0+1,wy=SY1-SY0+1,wz=SZ1-SZ0+1;
      bit3d tc;
      if(!Innercopy&&wx==b3d.dx&&wy==b3d.dy&&wz==b3d.dz) {
        tc=b3d;
      } else {
        tc=Selection=new bit3d();
        Selection.alloc(wx,wy,wz);
        Selection.copy(0,0,0,b3d,SX0,SY0,SZ0,Selection.dx,Selection.dy,Selection.dz);
      }
      if(!Innercopy) {
        string text=tc.Export(CX,CY,CZ);
       try { 
        Clipboard.SetText(text);
       } catch(Exception ex) {
        MessageBox.Show(ex.Message,cut?"Cut":"Copy");
        return;
       }
      }
      if(cut) {
        PushUndo();
        b3d.clear(false,SX0,SY0,SZ0,Selection.dx,Selection.dy,Selection.dz);
        Invalid();
      }
    }

    void Paste(int x,int y,bit_op op) {
      if(!Innercopy) {
       try {
        string text=Clipboard.GetText(TextDataFormat.UnicodeText);
        if(""+text=="") {
          Bitmap bmp=Clipboard.GetImage() as Bitmap;
          if(bmp==null) return;
          Selection=bit3d.Import(bmp);
          CX=CY=CZ=0;
        } else
          Selection=bit3d.Import(text,out CX,out CY,out CZ);
       } catch(Exception ex) {
        MessageBox.Show(ex.Message,"Paste");
        return;
       }
      }
      if(Selection!=null&&x>=0&&x<b3d.dx&&y>=0&&y<b3d.dy) {
        PushUndo();
        b3d.operation(op,x-CX,y-CY,z-CZ,Selection,0,0,0,Selection.dx,Selection.dy,Selection.dz);
        Invalid();
      }      
    }

    private void mOpHalfSize_Click(object sender,EventArgs e) {
      PushUndo();
      int nx=b3d.dx/2,ny=b3d.dy/2,nz=b3d.dz/2;
      b3d.resize(nx>0?nx:1,ny>0?ny:1,nz>0?nz:1);
      UpdateDZ();
      Invalid();
    }

    private void mOpMirrorX_Click(object sender, EventArgs e) { MirrorX(2*(int)mpx+(CtrlDown?0:1),ShiftDown);}
    private void MirrorX(int x2x,bool sele) {
      PushUndo();
      if(sele&&!SelectionEmpty()) {
        b3d.mirrorcopy(0,x2x,SX0,SY0,SZ0,SX1-SX0+1,SY1-SY0+1,SZ1-SZ0+1);
      } else b3d.mirrorcopy(0,x2x);
      Invalid();
    }

    private void mOpExtrude_Click(object sender, EventArgs e) {
      bool cone=sender==mOpExtrudeCone,sphere=sender==mOpExtrudeSphere;
      if(cone||sphere) {
        bool bx=x>=SX0&&SX0<=SX1,by=y>=SY0&&SY0<=SY1;
        Extrude(ShiftDown,bx?2*x+1:by?-1:SX0+SX1,by?2*y+1:bx?-1:SY0+SY1,sphere);
      } else Extrude(ShiftDown,-1,-1,false);
    }
    void Extrude(bool or,int c2x,int c2y,bool sphere) {
      if(SelectionEmpty()) return;
      PushUndo();
      b3d.extrude(or,z,SX0,SY0,SZ0,SX1-SX0+1,SY1-SY0+1,SZ1-SZ0+1,c2x<2*SX0||c2x>2*SX1+1?int.MinValue:c2x,c2y<2*SY0||c2y>2*SY1+1?int.MinValue:c2y,sphere);
      Invalid();
    }
    void Twist(bool or,int c2x,int c2y,int angle) {
      if(SelectionEmpty()) return;
      PushUndo();
      b3d.twist(z,SX0,SY0,SZ0,SX1-SX0+1,SY1-SY0+1,SZ1-SZ0+1,c2x,c2y,angle);
      Invalid();
    }
    
     
    private void mMeshTriangulation_Click(object sender,EventArgs e) {
      if(msh==null) return;
      msh.fcs.Triangulation();
      lazydraw=true;
    }

    private void mMeshPrecise_Click(object sender,EventArgs e) {
      if(msh==null) return;
      msh.Precise(!CtrlDown);
      lazydraw=true;
    }

    private void mMeshLight_Click(object sender,EventArgs e) {
      SetLight(ShiftDown,CtrlDown);
    }


    private void mMeshScale_Click(object sender, EventArgs e) {       
      if(msh==null) return;
      bool fat=ShiftDown,mirror=CtrlDown;
      double[] rd3,rd33=new double[9];
      double sq2=Math.Sqrt(2);
      if(fat) rd3=new double[] {sq2,1,sq2};
      else if(mirror) rd3=new double[] {-1,1,1};
      else rd3=new double[] {1,sq2,1};
      d33.transpose(rd33,cam.rot);
      d33.div_d3(rd33,rd33,rd3);
      d33.mul(rd33,rd33,cam.rot);
      d3.mul_d33(cam.center,cam.center,rd33);
      msh.pts.Rotate(rd33);
      if(mirror) msh.fcs.Reverse();
      d3.mul_d33(cam.pos,cam.pos,rd33);
      lazydraw=true;
    }

    private void mMeshTwist_Click(object sender, EventArgs e) {
      Twist(atoiex(eTwistAngle.Text,360),0);
    }
    private void mMeshKeg_Click(object sender, EventArgs e) {
       Twist(0,ShiftDown?-.75f:.75f);
    }
    void Twist(int angle,float keg) {
      if(msh==null) return;
      if(angle==0&&keg==0) return;
      double a=angle*Math.PI/180;
      if(ShiftDown) a=-a;
      double[] pos=d3.V(cam.pos),rot=d33.M(0),min=d3.V(),max=d3.V();
      d3.mul_d(pos,pos,-1);
      d33.copy(rot,cam.rot);
      d33.transpose(rot,cam.rot);
      msh.pts.Translate(pos);
      msh.pts.Rotate(rot);
      msh.pts.MinMax(min,max);
      double ai,co=1,si=0,k=1,k2=Math.Sqrt(1-keg*keg);
      if(min[2]<max[2])
        for(int i=0;i<msh.pts.Count;i++) {
          int i3=3*i;
          if(angle!=0) {
            ai=(msh.pts.p[i3+2]-min[2])*a/(max[2]-min[2]);
            co=Math.Cos(ai);
            si=Math.Sin(ai);
          }
          if(keg!=0) {
            double z=((msh.pts.p[i3+2]-min[2])*2/(max[2]-min[2])-1)*keg;
            z=z<=-1||z>=1?0:Math.Sqrt(1-z*z);
            z=z/k2;
            k=keg>0?z:2-z;
          }
          double x=msh.pts.p[i3],y=msh.pts.p[i3+1];
          msh.pts.p[i3]=k*(co*x-si*y);
          msh.pts.p[i3+1]=k*(si*x+co*y);
        }
      d33.transpose(rot,rot);
      d3.mul_d(pos,pos,-1);
      msh.pts.Rotate(rot);
      msh.pts.Translate(pos);
      lazydraw=true;
    }


    private void mViewAlign_Click(object sender, EventArgs e) {
      cam.AlignAxis(CtrlDown?2:1);
      lazydraw=true;
    }

    void InsertPlane(int pos,bool eq,ref int x) {
      if(eq?x>=pos:x>pos) x++;
    }
    void InsertPlane(int axis) {
      PushUndo();
      switch(axis) {
       case 0:
        b3d.insertplane(axis,x);
        InsertPlane(x,false,ref SX0);
        InsertPlane(x,true,ref SX1);
        UpdateScroll();
        break;
       case 1:
        b3d.insertplane(axis,y);
        InsertPlane(y,false,ref SY0);
        InsertPlane(y,true,ref SY1);
        UpdateScroll();
        break;
       case 2:
        b3d.insertplane(axis,z);
        InsertPlane(z,false,ref SZ0);
        InsertPlane(z,true,ref SZ1);
        UpdateDZ();
        break; 
      }  
      Draw();
    }

    private void mFilePage_Click(object sender, EventArgs e) { PrintPage();}
    void PrintPage() {
      if(paged==null) {
        paged=new PageSetupDialog();
        paged.AllowPaper=paged.AllowMargins=paged.AllowOrientation=true;
        paged.EnableMetric=true;
        paged.PageSettings=new System.Drawing.Printing.PageSettings() {Landscape=Width>Height};
        paged.PageSettings.Margins=new System.Drawing.Printing.Margins(0,0,0,0);
      }
      paged.ShowDialog();          
    }
    private void mFilePrint_Click(object sender,EventArgs e) {
      if(msh==null) return;
      if(paged==null) PrintPage();
      if(printd==null) {
        printd=new PrintDialog();
        printd.PrinterSettings.DefaultPageSettings.Landscape=Width>Height;
        printd.UseEXDialog=true;
      }      
      printd.PrinterSettings.DefaultPageSettings.Landscape=paged.PageSettings.Landscape;      
      if(DialogResult.OK==printd.ShowDialog(this)) {
        paged.PageSettings.Landscape=printd.PrinterSettings.DefaultPageSettings.Landscape;
        System.Drawing.Printing.PrintDocument doc=new System.Drawing.Printing.PrintDocument();
        doc.PrinterSettings=printd.PrinterSettings;
        doc.DocumentName=Text;
        doc.PrintPage+=new System.Drawing.Printing.PrintPageEventHandler(doc_PrintPage);
        doc.Print();
        doc.Dispose();        
      }
    }

    void doc_PrintPage(object sender,System.Drawing.Printing.PrintPageEventArgs e) {
      Graphics gr=e.Graphics;
      IntPtr pdc=gr.GetHdc();
      int w=GDI.GetDeviceCaps(pdc,8),h=GDI.GetDeviceCaps(pdc,10),sx=0,sy=0;//e.MarginBounds.Width,h=e.MarginBounds.Height;
      if(e.MarginBounds!=e.PageBounds) {
        int mw=e.MarginBounds.Width*w/e.PageBounds.Width,mh=e.MarginBounds.Height*h/e.PageBounds.Height;
        sx=e.MarginBounds.Left*w/e.PageBounds.Width;sy=e.MarginBounds.Top*h/e.PageBounds.Height;
        GDI.IntersectClipRect(pdc,sx,sy,sx+mw,sy+mh);
      }
      msh.Draw2D(pdc,cam.Points(msh),mMeshWires.Checked?0:-1,(int)lwidth,wirefront,sx,sy,w,h,2*cam.angle,cam.zmin);
      gr.ReleaseHdc();
      e.HasMorePages=false;
    }

    void DeletePlane(int pos,bool eq,ref int x) {
      if(eq?x>=pos:x>pos) x--;
    }
    void DeletePlane(int axis) {
      switch(axis) {
       case 0:
        if(b3d.dx<2) return;
        PushUndo();
        b3d.deleteplane(axis,x);
        if(!SelectionEmpty()) {
          if(SX0==SX1) SelectionClear();
          else {
            DeletePlane(x,false,ref SX0);
            DeletePlane(x,false,ref SX1);        
          }  
        }  
        if(x>=b3d.dx) x--;
        UpdateScroll();
        break;
       case 1:
        if(b3d.dy<2) return;
        PushUndo();
        b3d.deleteplane(axis,y);
        if(!SelectionEmpty()) {
          if(SY0==SY1) SelectionClear();
          else {
            DeletePlane(y,false,ref SY0);
            DeletePlane(y,false,ref SY1);
          }
        }
        if(y>=b3d.dy) y--;
        UpdateScroll();
        break;
       case 2:
        if(b3d.dz<2) return;
        PushUndo();          
        b3d.deleteplane(axis,z);
        if(!SelectionEmpty()) {
          if(SZ0==SZ1) SelectionClear();
          else {
            DeletePlane(z,false,ref SZ0);
            DeletePlane(z,false,ref SZ1);
          }
        }        
        if(z>=b3d.dz) 
          ChangeZ(z-1);
        UpdateDZ();
        break;
      }  
      Draw();
    }

    void ImportMesh(string filename,byte source) {
      try {
        importfile=filename;        
        msh=new mesh();
        if(mesh.ends(filename,".off")||mesh.ends(filename,".off.gz"))
          msh.ImportOff(filename);
        else if(mesh.ends(filename,".stl")||mesh.ends(filename,".stl.gz")) {
          msh.ImportStl(filename);
        } else if(filename.EndsWith(".3dt",StringComparison.InvariantCultureIgnoreCase)) {
          msh.Import3dt(filename);
        } else if(filename.EndsWith(".pgm",StringComparison.InvariantCultureIgnoreCase)) {
          msh.ImportPgm(filename);
          msh.pts.Scale(new double[] {1,0.125,1});
        } else
          msh.ImportObj(filename);
        msh.Permute(importperm,false);
        meshsource=source;
        double[] r3=new double[3],a3=new double[3];
        if(importresize2!=null&&mesh.ParseDouble3Exp(r3,importresize)&&mesh.ParseDouble3Exp(a3,importresize2)) msh.pts.Resize(r3,a3);
        else if(importresize!=null&&mesh.ParseDouble3Exp(r3,importresize)) msh.pts.Resize(r3);
        else if(importscale!=null&&mesh.ParseDouble3Exp(r3,importscale)) msh.pts.Scale(r3);
        SetMode(ViewModes.Mesh,Conversion.File);    
      } catch(Exception ex) {
        MessageBox.Show(this,ex.Message,"Exception",MessageBoxButtons.OK,MessageBoxIcon.Error);
      }
    }
    private void mFileViewMesh_Click(object sender,EventArgs e) {
      string dir=Directory.GetCurrentDirectory(),file=ofd.FileName;
     try { 
      ofd.Title="Import mesh";
      ofd.FileName=importfile;
      ofd.Filter = "wavefront (*.obj)|*.obj;*.obj.gz|*.off|*.off|STereoLithography (*.stl)|*.stl;*.stl.gz|graymap (*.pgm)|*.pgm|3d text (*.3dt)|*.3dt|*.*|*.*";      
      ofd.DefaultExt="obj";     
      if(DialogResult.OK==ofd.ShowDialog(this)) {
        ImportMesh(ofd.FileName,1);
       } 
     } catch(Exception ex) {
        MessageBox.Show(this,ex.Message,"Exception",MessageBoxButtons.OK,MessageBoxIcon.Error);
     }
      ofd.FileName=file;
      Directory.SetCurrentDirectory(dir);
    }

    private void mViewFullscreen_Click(object sender,EventArgs e) {
      if(MainMenuStrip.Visible) Fullscreen(true);
    }

    private void Button_Click(object sender,EventArgs e) {
      Button b=sender as Button;
      string tag=b==null?""+sender:""+b.Tag;
      switch(tag) {
       case "insx":InsertPlane(0);break;
       case "insy":InsertPlane(1);break;
       case "insz":InsertPlane(2);break;
       case "delx":DeletePlane(0);break;
       case "dely":DeletePlane(1);break;
       case "delz":DeletePlane(2);break;
       case "ext":Extrude(ShiftDown,-1,-1,false);break;
       case "extc":Extrude(ShiftDown,2*x,2*y,false);break;
       case "exts":Extrude(ShiftDown,2*x,2*y,true);break;
       case "twist":Twist(ShiftDown,2*x,2*y,atoiex(eTwistAngle.Text,360));break;
       case "lathex":Lathe3(false);break;
       case "lathey":Lathe3(true);break;
       case "lathez":Lathe1();break;
       case "cuts":Cuts(CtrlDown?2:ShiftDown?1:0);break;
       case "and":And();break;
      }
    }

    private void mViewPalette_Click(object sender, EventArgs e) {
      string tag=""+(sender as ToolStripItem).Tag;
      switch(tag) {
       case "reverse":palette=faces.PaletteReverse(palette,true);lazycolor=true;break;
       case "gray":palette=faces.Palette();break;
       case "redblue":palette=faces.PalRedBlue;break;
       case "rainbow":palette=faces.PalRainbow;break;
       case "layers":palette=faces.PalLayers;break;
      }

    }


    private void mMeshColor_Click(object sender,EventArgs e) {
      if(ShiftDown) {
        color_mode=ShiftRKey?ColorModes.Three:ColorModes.Random;
        lazycolor=true;
      } else {
        cdialog.Color=Int2Color(mesh_color);
        cdialog.FullOpen=true;      
        if(DialogResult.OK==cdialog.ShowDialog(this)) {
          color_mode=ColorModes.Color;
          mesh_color=Color2Int(cdialog.Color);
          palette=faces.Palette(mesh_color);
          lazycolor=true;
        }
      }
      cam.NoFly();
    }

    private void mViewSetColor_Click(object sender, EventArgs e) {
      string tag=""+(sender as ToolStripItem).Tag;
      switch(tag) {
       case "color":mMeshColor_Click(sender,e);return;
       case "gradienty":color_mode=ColorModes.GradientY;break;
       case "sphere":color_mode=ColorModes.Sphere;break;
       case "cylinderz":color_mode=ColorModes.CylinderZ;break;
       case "radialz":color_mode=ColorModes.RadialZ;break;
       case "normalradial":color_mode=ColorModes.NormalRadial;break;
       case "normalrgb":color_mode=ColorModes.NormalRGB;break;
       default:return;
      }      
      color_rota=cam.rot.Clone() as double[];
      color_pos=cam.pos.Clone() as double[];
      lazycolor=true;
    }

    private void mViewBoldLine_Click(object sender, EventArgs e) {
      bool ctrl=CtrlDown,shift=ShiftDown;
      mViewBoldLine.Checked^=true;
      if(ctrl||shift) mViewBoldLine.Checked=true;
      if(mViewBoldLine.Checked) lwidth=ctrl?shift?5:3:shift?4:2;
      else lwidth=1;
      lazydraw=true;
    }

    private void mEdit_Click(object sender, EventArgs e) { 
      string s=""+sender;
      ToolStripItem mi=sender as ToolStripItem;
      if(mi!=null) s=mi.Name.Replace("mEdit","").ToLowerInvariant();
      else s=(""+sender).ToLowerInvariant();

      if(s=="undo") {
        Undo(false);
      } if(s=="redo") {
        Undo(true);
      } if(s=="copy") {
        CopyCut(false);
      } if(s=="cut") {
        CopyCut(true);
      } if(s=="paste") {
        bool shift=ShiftDown,ctrl=CtrlDown;
        Paste(SX0,SY0,ctrl?shift?bit_op.And:bit_op.Sub:shift?bit_op.Copy:bit_op.Or);
      } if(s=="delete") {
        PushUndo();
        if(CtrlDown) b3d.not(SX0,SY0,SZ0,SX1+1-SX0,SY1+1-SY0,SZ1+1-SZ0);
        else b3d.clear(ShiftDown,SX0,SY0,SZ0,SX1+1-SX0,SY1+1-SY0,SZ1+1-SZ0);      
        Invalid();        
      } if(s=="innercopy")
        Innercopy=mEditInnerCopy.Checked^=true;
      else if(s=="invertwheelz")
        InvertWheelZ=mEditInvertWheelZ.Checked^=true;
    }



    private void mMeshRotoid_Click(object sender,EventArgs e) {
      SetMode(ViewModes.Mesh,Conversion.Rotoid|(conv&Conversion.Cubes));
    }

    private void mMeshRelax_Click(object sender,EventArgs e) {
      msh.Relax(0.25,1,1);if(color_mode!=ColorModes.Random) lazycolor=true;
    }

    private void mHelpAbout_Click(object sender,EventArgs e) {
      string version=GetType().Assembly.GetName().Version.ToString();
      MessageBox.Show(this,"Cubes voxel editor v."+version+"\n  by Pavel Popelka\n\nhttp://popelkapavel.sweb.cz/cubes/cubes.htm","Cubes");
    }

    private void mHelpHelp_Click(object sender,EventArgs e) {
      string file=GetType().Assembly.Location;
      file=file.Substring(0,file.Length-3)+"rtf";
      if(File.Exists(file)) {
        fhelp hlp=new fhelp("cubes.rtf");
        hlp.ShowDialog(this);
      }  
    }

    private void pMain_Scroll(object sender,ScrollEventArgs e) {
      switch(e.ScrollOrientation) {
       case ScrollOrientation.HorizontalScroll:scx=e.NewValue;break;
       case ScrollOrientation.VerticalScroll:scy=e.NewValue;break;       
      }
      //Text=""+scx+":"+scy;
      if(mode==ViewModes.Edit) Draw();
    }

    private void pMain_Resize(object sender,EventArgs e) {
      scx=pMain.HorizontalScroll.Value;
      scy=pMain.VerticalScroll.Value;
      lazydraw=true;
    }

    private void tZ_Scroll(object sender,EventArgs e) {
      if(z!=tZ.Value&&tZ.Value<b3d.dz)
        ChangeZ(tZ.Value);
    }
    

    private void bMirrX_Click(object sender,EventArgs e) {
      Mirror(0,ShiftDown);
    }

    private void bMirrY_Click(object sender,EventArgs e) {
      Mirror(1,ShiftDown);
    }

    private void mViewNormalRGB_Click(object sender,EventArgs e) {
      color_mode=ColorModes.NormalRGB;
      color_rota=cam.rot.Clone() as double[];
      lazycolor=true;
    }

    private void mMeshCutY_Click(object sender,EventArgs e) {
      if(msh==null) return;
      double[] normal=new double[3];
      d3.copy(normal,0,cam.rot,3);
      double a=-d3.scalar(cam.rot,3,cam.pos,0);      
      //d3.mul_d(normal,normal,-1);a*=-1;
      msh.CutPlane(normal,a);
      lazydraw=true;
    }
    
    private void mViewReset_Click(object sender,EventArgs e) {    
      cam.Reset();
      if(msh!=null&&msh.pts.Count>0) {
        msh.pts.Center(cam.center);
        cam.pos[0]=cam.center[0];
        cam.pos[1]=cam.center[1];
      }  
      lazydraw=true;
    }

    private void mViewPovrayCamera_Click(object sender,EventArgs e) {
      string s=cam.Povray(ClientSize.Width,ClientSize.Height);
     try { 
      Clipboard.SetText(s);
     } catch {} 
    }

    private void mViewCross_Click(object sender,EventArgs e) {
      mViewCross.Checked=(cam.cross^=true);
      Draw();      
    }

    void SetHeadOnly(bool value) {
      mViewHeadOnly.Checked=cam.headonly=value; 
    }
    private void mViewHeadOnly_Click(object sender,EventArgs e) {
      SetHeadOnly(!cam.headonly);
    }

    private void mViewHeadReset_Click(object sender,EventArgs e) {
      cam.HeadReset();
      lazydraw=true;
    }

    private void mViewBodyReset_Click(object sender,EventArgs e) {
      cam.BodyReset();
      lazydraw=true;
    }

    void Resolution(int w,int h) {
      SetClientSizeCore(w,h+(menu.Visible?menu.Height:0));
    }
    private void mViewRes320_Click(object sender,EventArgs e) {
      Resolution(320,240);
    }

    private void mViewRes640_Click(object sender,EventArgs e) {
      Resolution(640,480);
    }

    private void x600ToolStripMenuItem_Click(object sender,EventArgs e) {
      Resolution(800,600);
    }

    private void mViewRes1280_Click(object sender,EventArgs e) {
      Resolution(1280,1024);
    }

    private void mViewRes1024_Click(object sender,EventArgs e) {
      Resolution(1024,768);
    }

    private void mViewRecord_Click(object sender,EventArgs e) {
      mViewRecord.Checked=(cam.record^=true);
      lazydraw=true;
    }

    private void mViewRes480_Click(object sender,EventArgs e) {
      Resolution(480,360);
    }

    void SwitchStereo(bool crosseye) {
      if(crosseye) {cam.crosseye=!cam.crosseye;cam.anaglyph=false;}
      else {cam.anaglyph=!cam.anaglyph;cam.crosseye=false;}
      mViewAnaglyph.Checked=cam.anaglyph;
      mViewCrossEye.Checked=cam.crosseye;
      lazydraw=true;
    }
    private void mViewAnaglyph_Click(object sender,EventArgs e) { SwitchStereo(false);}
    
    private void mViewCrossEye_Click(object sender, EventArgs e) { SwitchStereo(true); }


    private void mViewRes1600_Click(object sender,EventArgs e) {
      Resolution(1600,1200);
    }

    private void mViewRes640w_Click(object sender,EventArgs e) {
      Resolution(640,360);
    }

    private void mViewRes1280w_Click(object sender,EventArgs e) {
      Resolution(1280,720);
    }

    private void mViewRes1920w_Click(object sender,EventArgs e) {
      Resolution(1920,1080);
    }

    private void bRotoid_Click(object sender,EventArgs e) {
      bool shift=ShiftDown;
      SetMode(ViewModes.Mesh,(Conversion.Rotoid|(conv&Conversion.Cubes))^(shift?Conversion.Cubes:0));
    }

    private void bMesh_Click(object sender,EventArgs e) {
      bool shift=ShiftDown;
      SetMode(ViewModes.Mesh,(Conversion.Normal|(conv&Conversion.Cubes))^(shift?Conversion.Cubes:0)); 
    }

    private void mMeshReverse_Click(object sender,EventArgs e) {
      if(msh!=null) {
        msh.fcs.Reverse();
        lazycolor=true;
      }  
    }
    void Command(string cmd) {
       
       switch(cmd) {
        case "ViewWireFron":mViewWireFron.Checked=(wirefront^=true);lazydraw=true;break;
        case "ViewWires":mMeshWires.Checked=!mMeshWires.Checked;lazydraw=true;break;
        case "ViewWhite":mMeshWhite.Checked=!mMeshWhite.Checked;lazydraw=true;break;
        case "FilterReset":
         if(msh!=null) msh.fcs.Hide(false);
         lazydraw=true;
         break;
        case "FilterInvert":
         if(msh!=null) msh.fcs.HideInvert();
         lazydraw=true;
         break;
        case "FilterFast":
         if(msh!=null) msh.fcs.Hide(1,2,true);
         lazydraw=true;
         break;
        case "FilterFront":
        case "FilterBack":
         if(msh!=null) {
           double[] x=d3.V(cam.rot[6],cam.rot[7],cam.rot[8]);
           msh.FilterBack(x,cmd=="FilterBack");
         }
         lazydraw=true;
         break;
        case "FilterLevels":
         if(msh!=null) {
           double[] x=d3.V(cam.rot[3],cam.rot[4],cam.rot[5]);
           msh.FilterLevels(x,11,false);
         }
         lazydraw=true;
         break;
        case "FilterUpper":
         if(msh!=null) {
           double[] x=CtrlDown?d3.V(cam.rot[0],cam.rot[1],cam.rot[2]):d3.V(cam.rot[3],cam.rot[4],cam.rot[5]);
           msh.FilterPlane(x,cam.pos[0]*x[0]+cam.pos[1]*x[1]+cam.pos[2]*x[2],ShiftDown);
         }
         lazydraw=true;
         break;
       }
    }
    private void Command_Click(object sender,EventArgs e) {
       ToolStripItem tsi=sender as ToolStripItem;
       if(tsi==null) return;
       string cmd=""+tsi.Tag;
       bool shift=ShiftDown,ctrl=CtrlDown;
       if(shift||ctrl) {
         switch(cmd) {
          case "FilterBack":cmd="FilterFront";break;
         }
       }
       Command(cmd);
    }
   
    public void Shadow(bool reverse) {
      if(msh==null) return;
      msh.Shadows(light_vec,reverse,0.5);
      lazyshadow=true;
    }
    private void mViewShadows_Click(object sender,EventArgs e) {
      Shadow(revshadow);
    }

    private void pMain_MouseDoubleClick(object sender,MouseEventArgs e) {
      if(mode==ViewModes.Edit&&tool==Tools.Pen) {
        bool black=0!=(e.Button&MouseButtons.Left);
        int x=(scx+e.X)/size,y=(scy+e.Y)/size;
        black=!b3d.get(x,y,z);
        PushUndo();        
        b3d.floodfill2d(x,y,z,black,0,false); 
        Invalid();
      }
    }

    private void miViewCenter_Click(object sender, EventArgs e) {
      d3.copy(cam.center,cam.pos);
    }


  }

  public class undoitem {
    public bit3d undo;
    public int SX0,SY0,SZ0,SX1,SY1,SZ1;
    public int CX,CY,CZ;
    public int x,y,z;
    public int scx,scy,size;
    public override string ToString() {
      return "["+x+","+y+","+z+"]";
    }
  }
  
  public class camera {
    public const double mouse_angle=0.25*Math.PI/180;
    public double[] pos;
    public double[] rot,head,body;
    public double[] center;
    public double angle,zmin,zmax;
    public bool headonly,cross;
    public double speed=0.001;
    public int speed2; // -1,0,1
    public double lt;
    public bool record;
    public System.Drawing.Imaging.ImageFormat record_ext=System.Drawing.Imaging.ImageFormat.Png;
    public string record_path="";
    public bool anaglyph,crosseye;
    public double eyes,eyea;
    public camera() {
      pos=d3.V();
      rot=d33.M(1);head=d33.M(1);body=d33.M(1);
      center=d3.V();
      eyes=0.5;
      Reset();
    }
    public void Reset() {
      d3.copy(pos,d3.V0);
      d33.copy(rot,d33.M1);
      d33.copy(head,d33.M1);
      d33.copy(body,d33.M1);
      d3.copy(center,d3.V0);
      angle=30;
      zmin=0.1;
      zmax=1000;
      speed2=0;
    }
    public static double GetRealTime() {
      return DateTime.UtcNow.Ticks/10000.0;
    }    

    public void Fly(int direction) {
      lt=GetRealTime();
      speed2=direction;
    }
    
    public void Fly() {
      double rt=GetRealTime();
      double[] v=d3.V(0);
      d3.add(pos,pos,d3.mul_d(v,0,body,6,speed2*speed*(rt-lt)));
      lt=rt;    
    }  
    
    public void NoFly() {
      if(speed2!=0) lt=GetRealTime();
    }
    
    public void RecordConfig(string path,string ext) {
      record_path=path;
      record_ext=ext.ToLowerInvariant()=="bmp"?System.Drawing.Imaging.ImageFormat.Bmp:System.Drawing.Imaging.ImageFormat.Png;      
    }

    public void BodyReset() {
      d33.copy(body,rot);
      d33.copy(head,d33.M1);
    }
    public void HeadReset() {
      d33.copy(head,d33.M1);
      HeadBody2Rota();
    }  

    public void HeadBody2Rota() {
      d33.mul(rot,head,body);
    }
    public void Rotate(int axis,double a,bool headonly) {
      double[] rm=d33.M(0);
      d33.rotation_aa(rm,d33.M1,axis==1?3:axis==2?6:0,d3.Radian(a));
      if(headonly) {
        d33.mul(head,rm,head);
        d33.normal(head,head);
      } else {
        d33.mul(body,rm,body);
        d33.normal(body,body);
      }
      HeadBody2Rota();
    }
    public void Move(int axis,double step) {
      d3.add_d(pos,0,pos,0,step,body,3*axis);
    }
    
    public void MoveAround(int x,int y,bool z) {
        double[] rd3=new double[3],mx=new double[9],my=new double[9];
        
        d33.rotation_aa(mx,body,z?6:3,-x*mouse_angle);
        d33.rotation_aa(my,body,0,-y*mouse_angle);
        d3.sub(rd3,pos,center);
        d3.div_d33(rd3,rd3,mx);
        d3.div_d33(rd3,rd3,my);
        d3.add(pos,center,rd3);

        d33.rotation_aa(mx,(z?d3.VZ:d3.VY),0,x*mouse_angle);
        d33.rotation_aa(my,d3.VX,0,y*mouse_angle);
        d33.mul(body,mx,body);
        d33.mul(body,my,body);
        d33.normal(body,body);
        HeadBody2Rota();
    }
    
    public double[] Points(mesh msh) {
      if(msh==null) return null;
      double[] pts=new double[msh.pts.Count*3],r=new double[3];
      for(int i=0;i<pts.Length;i+=3) {
        d3.sub(r,0,msh.pts.p,i,pos,0);
        d3.div_d33(pts,i,r,0,rot);
      }
      return pts;
    }
    public void AlignAxis(int axis) {
      if(axis<0||axis>2) return;
      int ya=0;
      double f=Math.Abs(rot[3*axis]),f2;
      if((f2=Math.Abs(rot[3*axis+1]))>f) {f=f2;ya=1;}
      if(Math.Abs(rot[3*axis+2])>f) ya=2;
      int yd=rot[3*axis+ya]>0?1:-1;
      double[] rd3=new double[3],rd33=new double[9];
      rd3[0]=rd3[1]=rd3[2]=0;
      rd3[ya]=yd;
      double[] v=new double[3];
      d3.copy(v,0,rot,3*axis);
      d33.rotation_vv(rd33,rd3,v);
      d33.mul(rot,rot,rd33);
    }

    public void PovrayRotation(double[] pov,double[] rota) {
      double[] fi=new double[3];
      double cy;

      fi[1]=Math.Asin(-rota[2]);
      cy=Math.Cos(fi[1]);
      if(cy!=0) {
        fi[0]=Math.Atan2(rota[5]/cy,rota[8]/cy);
        fi[2]=Math.Atan2(rota[1]/cy,rota[0]/cy);
      } else 
        fi[0]=fi[2]=0;
      // nutno doresit
      d3.mul_d(pov,fi,180/Math.PI);
    }
    
    public string Povray(int width,int height) {
      double[] pov=new double[3];
      string s;
 
      PovrayRotation(pov,rot);
      s=String.Format(System.Globalization.CultureInfo.InvariantCulture,"camera {{\r\n"
          +"  rotate <{0},{1},{2}>\r\n  translate <{3},{4},{5}>\r\n"
          +"  angle {6} // size {7}x{8}\r\n"
          +"  // right <{9},{10},{11}>\r\n  // up <{12},{13},{14}>\r\n  // direction <{15},{16},{17}>\r\n"
          +"}}\r\n"
        ,pov[0],pov[1],pov[2]
        ,pos[0],pos[1],pos[2]
        ,2*angle,width,height
        ,rot[0],rot[1],rot[2]
        ,-rot[3],-rot[4],-rot[5]
        ,rot[6],rot[7],rot[8]
      ); 
      return s;
    }
  }
  
  public class bitmap { 
    public int w,h;
    public int[] pix;
    public bitmap() {}
    public bitmap(int width,int height) {
      alloc(width,height);
    }
    public void alloc(int width,int height) {
      pix=new int[(w=width)*(h=height)];
    }    
    public static int interpol(int color0,int color1,int a,int ab) {
      int r0=(color0>>16)&255,r1=(color1>>16)&255;
      int g0=(color0>>8)&255,g1=(color1>>8)&255;
      int b0=(color0>>0)&255,b1=(color1>>0)&255;
      r0=(r0*(ab-a)+r1*a)/ab;
      g0=(g0*(ab-a)+g1*a)/ab;
      b0=(b0*(ab-a)+b1*a)/ab;
      return (r0<<16)|(g0<<8)|b0;
    }
    public void clear(int color) {
      for(int i=0;i<pix.Length;i++)
        pix[i]=color;
    }
    public void gradient(int color0,int color1) {
      int p=0;
      for(int j=0;j<h;j++) {
        int c=interpol(color0,color1,j,h-1);
        for(int i=0;i<w;i++)
          pix[p++]=c;
      }    
    }
    public void Rectangle(int x,int y,int width,int height,int fill,int stroke) {
      if(width<0||height<0) return;
      if(x<0||x+width>=w||y<0||y+height>=h) return;
      int b=w*y+x,i,j;
      for(j=1;j<height;j++) {
        pix[b+j*w]=stroke;
        for(i=1;i<width;i++) pix[b+j*w+i]=fill;
        pix[b+j*w+width]=stroke;
      }
      for(i=0;i<=width;i++) {
        pix[b+i]=stroke;
        pix[b+height*w+i]=stroke;
      }        
      /*if(cx<0||cx>=pw||cx2<0||cx2>=pw) return;
      if(cy<0||cy>=ph||cy2<0||cy2>=ph) return;
      int b=cy*pw+cx,dx=cx2-cx,dy=cy2-cy;
      int c=(int)Colors.LightGray,c2=(int)Colors.Black;      
      int i,j;
      for(j=1;j<dy;j++) {
        for(i=1;i<dx;i++)
          bm.pix[b+j*pw+i]=c;
        bm.pix[b+j*pw]=c2;
        bm.pix[b+j*pw+dx]=c2;  
      }
      for(i=0;i<=dx;i++) {
        bm.pix[b+i]=c2;
        pixel[b+(dy)*pw+i]=c2;  
      } */

    }        
    public void Draw(Graphics gr,int x,int y) {
       /*Bitmap bm=new Bitmap(w,h,PixelFormat.Format32bppRgb);
       BitmapData bd=bm.LockBits(new Rectangle(0,0,w,h),ImageLockMode.WriteOnly,bm.PixelFormat);
       Marshal.Copy(pix,0,bd.Scan0,pix.Length);
       bm.UnlockBits(bd);*/
       //   Graphics gr=this.CreateGraphics();
       Bitmap bm=ToBitmap();
       gr.DrawImageUnscaled(bm,x,y);
       //gr.Dispose();
       bm.Dispose();    
    }
    public static bitmap FromBitmap(Bitmap bmp) {
      int w=bmp.Width,h=bmp.Height;
      bitmap bm=new bitmap(w,h);
      BitmapData bd=bmp.LockBits(new Rectangle(0,0,w,h),ImageLockMode.ReadOnly,bmp.PixelFormat);
      Marshal.Copy(bd.Scan0,bm.pix,0,bm.pix.Length);
      bmp.UnlockBits(bd);
      return bm;
    }
    public Bitmap ToBitmap() {
       Bitmap bm=new Bitmap(w,h,PixelFormat.Format32bppRgb);
       BitmapData bd=bm.LockBits(new Rectangle(0,0,w,h),ImageLockMode.WriteOnly,bm.PixelFormat);
       Marshal.Copy(pix,0,bd.Scan0,pix.Length);
       bm.UnlockBits(bd);
       return bm;
    }
    public void Anaglyph(bitmap right) {
      for(int i=0;i<pix.Length;i++)
        pix[i]=(pix[i]&0xff0000)|(right.pix[i]&0xffff);
    }
    public void CrossEyes(bitmap right) { 
      int w2=w/2,a=0,b=0;
      for(int y=0;y<h;y++) {
        for(int x=0;x<w2;x++)
          pix[a++]=right.pix[b++];
        a+=w-w2;b+=right.w-w2;
      }
    }
    public void UpsideDown() {
      int r,h=0,g=pix.Length-w;
      for(int hey=w*(this.h/2);h<hey;) {
        for(int hex=h+w;h<hex;h++,g++) {
          r=pix[h];pix[h]=pix[g];pix[g]=r;
        }
        g-=2*w;
      }
    }
    public void BGR() {
      int h=0,he=h+pix.Length;
      while(h<he) {
        uint p=(uint)pix[h];
        pix[h++]=(int)((p&0xff00ff00)|((p&0xff0000)>>16)|((p&0xff)<<16));
      }
    }
  }

}
