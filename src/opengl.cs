using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Reflection;

[assembly: AssemblyVersion("0.20.0.0")]

namespace Mesh3d {
  public enum GLEnum {Zero
    ,GL_TRIANGLES=4,GL_QUADS=7,GL_POLYGON=9
    ,GL_FLAT=0x1D00,GL_SMOOTH=0x1D01    
    ,GL_LINE_SMOOTH=0x0B20
    ,GL_DEPTH_TEST=0x0B71
    ,GL_POINT=0x1B00,GL_LINE=0x1B01,GL_FILL=0x1B02
    ,GL_FRONT=0x404,GL_BACK=0x405    
    ,GL_MODELVIEW=0x1700,GL_PROJECTION=0x1701
    ,GL_VERTEX_ARRAY=0x8074
    ,GL_NORMAL_ARRAY=0x8075
    ,GL_COLOR_ARRAY=0x8076
    ,GL_DOUBLE=0x140A
    ,GL_LESS=0x0201,GL_LEQUAL=0x0203    
    ,GL_RGB=0x1907,GL_RGBA=0x1908,GL_BGR=0x80E0,GL_BGRA=0x80E1
    ,GL_BYTE=0x1400,GL_UNSIGNED_BYTE=0x1401,GL_INT=0x1404
  }
  [Flags]
  public enum GLbitfield {Zero
    ,GL_DEPTH_BUFFER_BIT=0x00000100
    ,GL_COLOR_BUFFER_BIT=0x00004000
  }
  public static class opengl {
    static opengl() {
      wglCreateContext(IntPtr.Zero);
    }
    public const int PFD_DRAW_TO_WINDOW=4;
    public const int PFD_SUPPORT_OPENGL=32;
    public const int PFD_DOUBLEBUFFER=1;
    public const int PFD_TYPE_RGBA=0;
    public const int PFD_MAIN_PLANE=0;
       
    
    [StructLayout(LayoutKind.Sequential)]
    public struct PIXELFORMATDESCRIPTOR {
	    public ushort nSize;
	    public ushort nVersion;
  	  public uint dwFlags;
 	    public byte iPixelType;
	    public byte cColorBits;
	    public byte cRedBits;
	    public byte cRedShift;
	    public byte cGreenBits;
	    public byte cGreenShift;
	    public byte cBlueBits;
	    public byte cBlueShift;
	    public byte cAlphaBits;
	    public byte cAlphaShift;
	    public byte cAccumBits;
	    public byte cAccumRedBits;
	    public byte cAccumGreenBits;
	    public byte cAccumBlueBits;
	    public byte cAccumAlphaBits;
	    public byte cDepthBits;
	    public byte cStencilBits;
	    public byte cAuxBuffers;
	    public byte iLayerType;
	    public byte bReserved;
	    public uint dwLayerMask;
	    public uint dwVisibleMask;
	    public uint dwDamageMask;
    }
    [DllImport("gdi32"), SuppressUnmanagedCodeSecurity]
    public static extern int SetTextColor(IntPtr hdc,int color);
    [DllImport("gdi32"), SuppressUnmanagedCodeSecurity]
    public static extern int SetBkColor(IntPtr hdc,int color);    
    [DllImport("gdi32.dll",CharSet=CharSet.Auto,SetLastError=true,ExactSpelling=true),SuppressUnmanagedCodeSecurity]
    public static extern int ChoosePixelFormat(IntPtr hdc,[In] ref PIXELFORMATDESCRIPTOR pdf);
    [DllImport("gdi32.dll",CharSet=CharSet.Auto,SetLastError=true,ExactSpelling=true),SuppressUnmanagedCodeSecurity]
    public static extern bool SetPixelFormat(IntPtr hdc,int pf,[In] ref PIXELFORMATDESCRIPTOR pdf);
    [DllImport("opengl32.dll",CharSet=CharSet.Auto,SetLastError=true,ExactSpelling=true),SuppressUnmanagedCodeSecurity]
    public static extern IntPtr wglCreateContext(IntPtr hDC);
    [DllImport("opengl32.dll",CharSet=CharSet.Auto,SetLastError=true,ExactSpelling=true),SuppressUnmanagedCodeSecurity]
    public static extern bool wglMakeCurrent(IntPtr hDC,IntPtr glrc);
    [DllImport("opengl32.dll",CharSet=CharSet.Auto,SetLastError=true,ExactSpelling=true),SuppressUnmanagedCodeSecurity]
    public static extern IntPtr wglGetCurrentContext();
    [DllImport("opengl32.dll",CharSet=CharSet.Auto,SetLastError=true,ExactSpelling=true),SuppressUnmanagedCodeSecurity]
    public static extern IntPtr wglGetCurrentDC();
    [DllImport("opengl32.dll",CharSet=CharSet.Auto,SetLastError=true,ExactSpelling=true),SuppressUnmanagedCodeSecurity]
    public static extern bool wglDeleteContext(IntPtr glrc);
    [DllImport("gdi32.dll",CharSet=CharSet.Auto,SetLastError=true,ExactSpelling=true),SuppressUnmanagedCodeSecurity]
    public static extern bool SwapBuffers(IntPtr hdc);
    [DllImport("opengl32.dll",CharSet=CharSet.Auto,SetLastError=true,ExactSpelling=true),SuppressUnmanagedCodeSecurity]
    public static extern void glClear(GLbitfield mode);
    [DllImport("opengl32.dll",CharSet=CharSet.Auto,SetLastError=true,ExactSpelling=true),SuppressUnmanagedCodeSecurity]
    public static extern void glClearColor(float r,float g,float b,float a);
    [DllImport("opengl32.dll",CharSet=CharSet.Auto,SetLastError=true,ExactSpelling=true),SuppressUnmanagedCodeSecurity]
    public static extern void glEnable(GLEnum cap);
    [DllImport("opengl32.dll",CharSet=CharSet.Auto,SetLastError=true,ExactSpelling=true),SuppressUnmanagedCodeSecurity]
    public static extern void glDisable(GLEnum cap);
    [DllImport("opengl32.dll",CharSet=CharSet.Auto,SetLastError=true,ExactSpelling=true),SuppressUnmanagedCodeSecurity]
    public static extern void glLineWidth(float width);
    [DllImport("opengl32.dll",CharSet=CharSet.Auto,SetLastError=true,ExactSpelling=true),SuppressUnmanagedCodeSecurity]
    public static extern void glShadeModel(GLEnum mode);

    [DllImport("opengl32.dll",CharSet=CharSet.Auto,SetLastError=true,ExactSpelling=true),SuppressUnmanagedCodeSecurity]
    public static extern void glPolygonMode(GLEnum face,GLEnum mode);    
    [DllImport("opengl32.dll",CharSet=CharSet.Auto,SetLastError=true,ExactSpelling=true),SuppressUnmanagedCodeSecurity]
    public static extern void glVertexPointer(int size,GLEnum type,int stride,IntPtr pointer);    
    [DllImport("opengl32.dll",CharSet=CharSet.Auto,SetLastError=true,ExactSpelling=true),SuppressUnmanagedCodeSecurity]
    public static extern void glEnableClientState(GLEnum state);
    [DllImport("opengl32.dll",CharSet=CharSet.Auto,SetLastError=true,ExactSpelling=true),SuppressUnmanagedCodeSecurity]
    public static extern void glDisableClientState(GLEnum state);
    [DllImport("opengl32.dll",CharSet=CharSet.Auto,SetLastError=true,ExactSpelling=true),SuppressUnmanagedCodeSecurity]
    public static extern void glBegin(GLEnum mode);
    [DllImport("opengl32.dll",CharSet=CharSet.Auto,SetLastError=true,ExactSpelling=true),SuppressUnmanagedCodeSecurity]
    public static extern void glEnd();
    [DllImport("opengl32.dll",CharSet=CharSet.Auto,SetLastError=true,ExactSpelling=true),SuppressUnmanagedCodeSecurity]
    public static extern void glArrayElement(int index);
    [DllImport("opengl32.dll",CharSet=CharSet.Auto,SetLastError=true,ExactSpelling=true),SuppressUnmanagedCodeSecurity]
    public static extern void glColor3d(double r,double g,double b);
    [DllImport("opengl32.dll",CharSet=CharSet.Auto,SetLastError=true,ExactSpelling=true),SuppressUnmanagedCodeSecurity]
    public static extern void glColor3s(short r,short g,short b);    
    [DllImport("opengl32.dll",CharSet=CharSet.Auto,SetLastError=true,ExactSpelling=true),SuppressUnmanagedCodeSecurity]
    public static extern void glVertex3d(double x,double y,double z);
    
    [DllImport("opengl32.dll",CharSet=CharSet.Auto,SetLastError=true,ExactSpelling=true),SuppressUnmanagedCodeSecurity]
    public static extern void glMatrixMode(GLEnum mode);
    [DllImport("opengl32.dll",CharSet=CharSet.Auto,SetLastError=true,ExactSpelling=true),SuppressUnmanagedCodeSecurity]
    public static extern void glPushMatrix();
    [DllImport("opengl32.dll",CharSet=CharSet.Auto,SetLastError=true,ExactSpelling=true),SuppressUnmanagedCodeSecurity]
    public static extern void glPopMatrix();
    [DllImport("opengl32.dll",CharSet=CharSet.Auto,SetLastError=true,ExactSpelling=true),SuppressUnmanagedCodeSecurity]
    public static extern void glLoadIdentity();
    [DllImport("opengl32.dll",CharSet=CharSet.Auto,SetLastError=true,ExactSpelling=true),SuppressUnmanagedCodeSecurity]
    public static extern void glMultMatrixd(double[] m4x4);
    [DllImport("opengl32.dll",CharSet=CharSet.Auto,SetLastError=true,ExactSpelling=true),SuppressUnmanagedCodeSecurity]
    public static extern void glLoadMatrixd(double[] m4x4);
    [DllImport("opengl32.dll",CharSet=CharSet.Auto,SetLastError=true,ExactSpelling=true),SuppressUnmanagedCodeSecurity]
    public static extern void glTranslated(double x,double y,double z);
    [DllImport("opengl32.dll",CharSet=CharSet.Auto,SetLastError=true,ExactSpelling=true),SuppressUnmanagedCodeSecurity]
    public static extern void glFrustum(double left,double right,double bottom,double top,double near,double far);
    [DllImport("opengl32.dll",CharSet=CharSet.Auto,SetLastError=true,ExactSpelling=true),SuppressUnmanagedCodeSecurity]
    public static extern void glDepthRange(double near,double far);
    [DllImport("opengl32.dll",CharSet=CharSet.Auto,SetLastError=true,ExactSpelling=true),SuppressUnmanagedCodeSecurity]
    public static extern void glDepthFunc(GLEnum func);
    [DllImport("opengl32.dll",CharSet=CharSet.Auto,SetLastError=true,ExactSpelling=true),SuppressUnmanagedCodeSecurity]
    public static extern void glReadBuffer(GLEnum func);
    [DllImport("opengl32.dll",CharSet=CharSet.Auto,SetLastError=true,ExactSpelling=true),SuppressUnmanagedCodeSecurity]
    public static extern void glReadPixels(int x,int y,int w,int h,GLEnum format,GLEnum type,IntPtr data);
    //[DllImport("opengl32.dll",CharSet=CharSet.Auto,SetLastError=true,ExactSpelling=true),SuppressUnmanagedCodeSecurity]
    //public static extern void glPointSize(float size);


//    [DllImport("glu32.dll",CharSet=CharSet.Auto,SetLastError=true,ExactSpelling=true),SuppressUnmanagedCodeSecurity]
//    public static extern void glPolygonOffsetEXT(float factor,float bias);

    public static void AllocHDC(IntPtr hdc,float width) {
      opengl.SetTextColor(hdc,0xffffff);
      opengl.SetBkColor(hdc,0);
      opengl.PIXELFORMATDESCRIPTOR pfd=new opengl.PIXELFORMATDESCRIPTOR();
      pfd.nSize=(ushort)Marshal.SizeOf(typeof(opengl.PIXELFORMATDESCRIPTOR));
      pfd.nVersion=1;
      pfd.dwFlags=(uint)(opengl.PFD_DRAW_TO_WINDOW|opengl.PFD_SUPPORT_OPENGL|opengl.PFD_DOUBLEBUFFER);
      pfd.iPixelType=opengl.PFD_TYPE_RGBA;
      pfd.cDepthBits=32;
      pfd.iLayerType=opengl.PFD_MAIN_PLANE;
      int pf=opengl.ChoosePixelFormat(hdc,ref pfd);
      bool set=opengl.SetPixelFormat(hdc,pf,ref pfd);      
      IntPtr glrc=opengl.wglCreateContext(hdc);
      opengl.wglMakeCurrent(hdc,glrc);
            
      opengl.glDisableClientState(GLEnum.GL_NORMAL_ARRAY);
      opengl.glDisableClientState(GLEnum.GL_COLOR_ARRAY);
      opengl.glPolygonMode(GLEnum.GL_FRONT,GLEnum.GL_FILL);//GL_LINE,GL_POINTS
      opengl.glPolygonMode(GLEnum.GL_BACK,GLEnum.GL_FILL);
      opengl.glEnable(GLEnum.GL_DEPTH_TEST);
      opengl.glDisable(GLEnum.GL_LINE_SMOOTH);
      opengl.glShadeModel(GLEnum.GL_FLAT);
      opengl.glLineWidth(width<=0?1:width);
      
      //glEdgeFlag(1);
    }
    public static void FreeHDC() {
      IntPtr glrc=opengl.wglGetCurrentContext();
      IntPtr hdc=opengl.wglGetCurrentDC();
      opengl.wglMakeCurrent(hdc,IntPtr.Zero);      
      opengl.wglDeleteContext(glrc);
    }
    public static double RotX(double[] rot,double cax,double sax,int j,int i) {
      double v=rot[3*j+i];
      if(j==1||cax==1) return v;
      if(j==2) sax=-sax;
      return cax*v+sax*rot[3*(2-j)+i];
    }
    public static void ViewPush(double[] pos,double[] rot,double zmin,double zmax,double angle,int width,int height,double dx,double ax) {
      /*opengl.glMatrixMode(GLEnum.GL_MODELVIEW);
      opengl.glPushMatrix();
      opengl.glLoadIdentity();*/
      if(dx<0) ax=-ax;
      opengl.glMatrixMode(GLEnum.GL_PROJECTION);
      opengl.glPushMatrix();
      double[] m4x4=new double[16];
      double cax=Math.Cos(ax),sax=Math.Sin(ax);      
      int i,j;
      for(j=0;j<3;j++)
        for(i=0;i<3;i++)
          m4x4[4*i+j]=(j!=0?-1:1)*RotX(rot,cax,sax,j,i);
      //for(i=0;i<3;i++)
      //  m4x4[4*3+i]=m4x4[4*i+3]=0;
      m4x4[4*3+3]=1;
      opengl.glLoadIdentity();
      //double zmin=0.01;
      double xmin=zmin*System.Math.Tan(angle*System.Math.PI/180);
      double ymin=xmin*height/width;
      opengl.glFrustum(-xmin,xmin,-ymin,ymin,zmin,zmax);
      opengl.glMultMatrixd(m4x4);
      //opengl.glTranslated(pos[0],pos[1],pos[2]);      
      //opengl.glDepthRange(zmin,zmax);
      //zbias=0;
      
      //opengl.glPolygonOffsetEXT(1f,(float)zbias);
      opengl.glTranslated(-pos[0]-dx*rot[0],-pos[1]-dx*rot[1],-pos[2]-dx*rot[2]);
      //opengl.glTranslated(pos[0],pos[1],pos[2]);
    }
    public static void ViewPop() {
      /*opengl.glMatrixMode(GLEnum.GL_MODELVIEW);
      opengl.glPopMatrix();*/
      opengl.glMatrixMode(GLEnum.GL_PROJECTION);
      opengl.glPopMatrix();
    }
    public static void glColor(int c) {
      opengl.glColor3s((short)((c<<7)&0x7f80),(short)((c>>1)&0x7f80),(short)((c>>9)&0x7f80));
    }
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct RECT {
    public int Left,Top,Right,Bottom;
  }
  [StructLayout(LayoutKind.Sequential)]
  public struct POINT {
    public int x,y;
  }  
  [StructLayout(LayoutKind.Sequential)]
  public struct BITMAPINFO {
    public int Size,Width,Height;
    public ushort Planes,BitCount;
    public int Compression,SizeImage,XPelsPerMeter,YPelsPerMeter,ClrUsed,ClrImportant;
  }  
  public class GDI {
    public enum StockObjects {
      WHITE_BRUSH,LTGRAY_BRUSH,GRAY_BRUSH,DKGRAY_BRUSH,BLACK_BRUSH,NULL_BRUSH,WHITE_PEN,BLACK_PEN,NULL_PEN
    }
    public enum PenStyles {
      PS_SOLID,PS_DASH,PS_DOT,PS_DASHDOT,PS_DASHDOTDOT,PS_NULL
    }
    public enum ROPStyles {
      R2_XORPEN=7,R2_NOTXORPEN=10,R2_COPYPEN=13
    }    
    public const int GWL_STYLE=-16,WS_CAPTION=0xc00000;
    [DllImport("user32.dll",ExactSpelling=true),SuppressUnmanagedCodeSecurity]
    public static extern bool GetWindowRect(IntPtr hWnd,out RECT lpRect);    
    [DllImport("user32"), SuppressUnmanagedCodeSecurity,PreserveSig]
    public static extern bool FillRect(IntPtr hdc,[In] ref RECT rec,IntPtr brush);
    [DllImport("gdi32"), SuppressUnmanagedCodeSecurity,PreserveSig]
    public static extern bool Rectangle(IntPtr hdc,int left,int top,int right,int bottom);    
    [DllImport("gdi32"), SuppressUnmanagedCodeSecurity,PreserveSig]
    public static extern bool Ellipse(IntPtr hdc,int left,int top,int right,int bottom);    
    [DllImport("gdi32"), SuppressUnmanagedCodeSecurity,PreserveSig]
    public static extern bool MoveToEx(IntPtr hdc,int x,int y,out POINT old);
    [DllImport("gdi32"), SuppressUnmanagedCodeSecurity,PreserveSig]
    public static extern bool LineTo(IntPtr hdc, int x, int y);    
    [DllImport("gdi32"), SuppressUnmanagedCodeSecurity,PreserveSig]
    public static extern IntPtr CreatePen(PenStyles style,int width,int color);    
    [DllImport("gdi32"), SuppressUnmanagedCodeSecurity,PreserveSig]
    public static extern ROPStyles SetROP2(IntPtr hdc,ROPStyles style);    
    [DllImport("gdi32"), SuppressUnmanagedCodeSecurity,PreserveSig]    
    public static extern IntPtr GetStockObject(StockObjects obj);    
    [DllImport("gdi32"), SuppressUnmanagedCodeSecurity,PreserveSig]
    public static extern IntPtr CreateSolidBrush(int color);
    [DllImport("gdi32"), SuppressUnmanagedCodeSecurity,PreserveSig]
    public static extern IntPtr CreateEnhMetaFile(IntPtr hdc,string filename,ref RECT rect,string desc);
    [DllImport("gdi32"), SuppressUnmanagedCodeSecurity,PreserveSig]
    public static extern IntPtr CloseEnhMetaFile(IntPtr fdc);
    [DllImport("gdi32"), SuppressUnmanagedCodeSecurity,PreserveSig]
    public static extern bool DeleteEnhMetaFile(IntPtr hemf);
    [DllImport("gdi32"), SuppressUnmanagedCodeSecurity,PreserveSig]
    public static extern bool Polygon(IntPtr hdc,POINT[] points,int count);
    [DllImport("gdi32"), SuppressUnmanagedCodeSecurity,PreserveSig]
    public static extern bool Polyline(IntPtr hdc,POINT[] points,int count);
    [DllImport("gdi32"), SuppressUnmanagedCodeSecurity,PreserveSig]
    public static extern IntPtr SelectObject(IntPtr hdc,IntPtr obj);
    [DllImport("gdi32"), SuppressUnmanagedCodeSecurity,PreserveSig]
    public static extern IntPtr DeleteObject(IntPtr hdc);
    [DllImport("gdi32"), SuppressUnmanagedCodeSecurity,PreserveSig]
    public static extern int GetDeviceCaps(IntPtr hdc,int cap);       	  
    [DllImport("gdi32"), SuppressUnmanagedCodeSecurity,PreserveSig]
    public static extern int SetDIBitsToDevice(IntPtr hdc,int x,int y,int width,int height,int sx,int sy,int scan0,int scans,IntPtr bits,ref BITMAPINFO bih,int coloruse);
    [DllImport("gdi32"), SuppressUnmanagedCodeSecurity,PreserveSig]
    public static extern int IntersectClipRect(IntPtr hdc,int left,int top,int right,int bottom);    
  }
  
  public class WINAPI {
    [DllImport("user32.dll"),SuppressUnmanagedCodeSecurity,PreserveSig]
	  public static extern bool WaitMessage();
	  
		[DllImport("kernel32"),SuppressUnmanagedCodeSecurity] 
		private static extern int GetPrivateProfileString(string section,string key,string def,System.Text.StringBuilder value, int size, string filename);		
		[DllImport("kernel32.dll"),SuppressUnmanagedCodeSecurity]
		private  static extern int  WritePrivateProfileString(string section,string key, string value, string filename);    

    [DllImport("user32.dll"),SuppressUnmanagedCodeSecurity,PreserveSig]
	  public static extern bool OpenClipboard(IntPtr hWndNewOwner);
	  [DllImport("user32.dll"),SuppressUnmanagedCodeSecurity,PreserveSig]
	  public static extern bool EmptyClipboard();
	  [DllImport("user32.dll"),SuppressUnmanagedCodeSecurity,PreserveSig]
	  public static extern IntPtr SetClipboardData(uint uFormat, IntPtr hMem);
	  [DllImport("user32.dll"),SuppressUnmanagedCodeSecurity,PreserveSig]
	  public static extern bool CloseClipboard();		

	  [DllImport("user32.dll"),SuppressUnmanagedCodeSecurity,PreserveSig]
	  public static extern IntPtr SetCapture(IntPtr hwnd);
	  [DllImport("user32.dll"),SuppressUnmanagedCodeSecurity,PreserveSig]
	  public static extern bool ReleaseCapture();
	  
	  public static string GetINI(string filename,string section,string key,string def) {
	    System.Text.StringBuilder sb=new System.Text.StringBuilder(256);
	    int ret=GetPrivateProfileString(section,key,def,sb,sb.Capacity,filename);
	    if(ret<1) return def;
	    return sb.ToString(0,ret);
	  }
  }  
  
}