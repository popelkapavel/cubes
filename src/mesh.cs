using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;
using Math3d;
using System.Text.RegularExpressions;

namespace Mesh3d {
  public class mesh {
    public points pts;
    public faces fcs;
    public mesh() {
      pts=new points(8);
      fcs=new faces(16);
    }    
    public mesh(mesh m) {
      pts=new points(m.pts);
      fcs=new faces(m.fcs);
    }
    public void Append(mesh m) {
      int p2=pts.Count;
      pts.Append(m.pts);
      fcs.Append(m.fcs,p2);      
    }
    public void Join(double dist) {
      fcs.Join(pts.Join(dist));      
    }
    public void FaceCenter(int f,double[] center) {
      center[0]=center[1]=center[2]=0;
      for(int i=1;i<=fcs.f[f];i++) 
        d3.add(center,0,center,0,pts.p,3*fcs.f[f+i]);
      d3.mul_d(center,center,1.0/fcs.f[f]);  
    }
    public void FaceNormal(int f,double[] normal) {
      FaceNormal(pts.p,f,normal);
      /*double[] d1=new double[3],d2=new double[3];
      d3.sub(d1,0,pts.p,3*fcs.f[f+2],pts.p,3*fcs.f[f+1]);
      d3.sub(d2,0,pts.p,3*fcs.f[f+3],pts.p,3*fcs.f[f+1]);
      d3.normal(normal,d3.cross(d1,d1,d2));*/
    }
    public void FaceNormal(double[] pts,int f,double[] normal) {
      double[] d1=new double[3],d2=new double[3];
      d3.sub(d1,0,pts,3*fcs.f[f+2],pts,3*fcs.f[f+1]);
      d3.sub(d2,0,pts,3*fcs.f[f+3],pts,3*fcs.f[f+1]);
      d3.normal(normal,d3.cross(d1,d1,d2));
    }
    
    public double[] PointsNormals(double[] mul) {
      double[] normals=new double[3*pts.Count],vp=new double[3],vn=new double[3],fn=new double[3];
      int[] w=new int[pts.Count];
      for(int h=0;h<fcs.N;h+=fcs.A) {
        int n=fcs.f[h++];
        int i1=fcs.f[h],ip=fcs.f[h+n-1];
        int in_=i1;
        while(n-->0) {
          int i=fcs.f[h++];
          in_=n>0?fcs.f[h]:i1;
          d3.sub(vp,0,pts.p,ip*3,pts.p,i*3);
          d3.sub(vn,0,pts.p,in_*3,pts.p,i*3);
          if(mul!=null) {
            d3.mul(vp,vp,mul);
            d3.mul(vn,vn,mul);
          }
          d3.normal(fn,d3.cross(fn,vn,vp));
          w[i]++;
          d3.normal(normals,3*i,d3.linear_interpol(normals,3*i,1.0/w[i],normals,3*i,fn,0),0);
          ip=i;
        }        
      }
      return normals;
    }

    internal class PointComparer:IComparer<int> {
      double[] pts;
      int axis;
      internal PointComparer(int axis,double[] pts) {
        this.axis=axis;this.pts=pts;
      }
      public int Compare(int a,int b) {
        double da=pts[3*a+axis],db=pts[3*b+axis];
        return da<db?-1:da>db?1:0;
      }
    }
    
    public bool[] PointsInShadow(double[] light,bool reverse) {
      double[] rota=d33.M(1),l=d3.V(),a=d3.V(),b=d3.V(),c=d3.V();
      d3.normal(l,d3.mul_d(l,light,reverse?1:-1));
      d33.rotation_vv(rota,d3.VZ,l);
            
      bool[] sha=new bool[pts.Count];
      int[] idx=new int[sha.Length];
      double[] p=new double[pts.Count*3];
      for(int i=0;i<sha.Length;i++) {
        d3.mul_d33(p,3*i,pts.p,3*i,rota);
        idx[i]=i; 
      }
      Array.Sort(idx,0,idx.Length,new PointComparer(0,p));      
      for(int h=0;h<fcs.N;h+=fcs.A) {
        int n=fcs.f[h++];
        if(n<3||faces.Hidden(fcs.f[h+n])) {h+=n;continue;}
        int p1=3*fcs.f[h++],p2,p3=3*fcs.f[h++];
        n-=2;
        while(n-->0) {
          p2=p3;
          p3=3*fcs.f[h++];
          double x1=p[p2]-p[p1],y1=p[p2+1]-p[p1+1];
          double x2=p[p3]-p[p2],y2=p[p3+1]-p[p2+1];
          double x3=p[p1]-p[p3],y3=p[p1+1]-p[p3+1];
          double t,lr=(p[p3]-p[p1])*y1-(p[p3+1]-p[p1+1])*x1;
          if(lr==0) continue;          
          double r,ix=p[p1],ax=ix,iy=p[p1+1],ay=iy,iz=p[p1+2];
          r=p[p2];if(r<ix) ix=r;else if(r>ax) ax=r;
          r=p[p2+1];if(r<iy) iy=r;else if(r>ay) ay=r;
          r=p[p2+2];if(r<iz) iz=r;
          r=p[p3];if(r<ix) ix=r;else if(r>ax) ax=r;
          r=p[p3+1];if(r<iy) iy=r;else if(r>ay) ay=r;
          r=p[p3+2];if(r<iz) iz=r;
          bool blr=lr<0;
          int ib=0,ie=sha.Length-1,im;
          while(ib<ie) {
            im=(ib+ie)/2;
            double mx=p[3*idx[im]];
            if(mx<ix) ib=im+1;
            else ie=im-1;
          }
          for(int i=ib;i<sha.Length;i++) {      
            int pi=3*idx[i];            
            if(p[pi]>ax) break;
            if(sha[idx[i]]) continue;
            if(p1==pi||p2==pi||p3==pi) continue;
            //if(p[pi]<ix||p[pi]>ax) continue;
            //if(p[pi+1]<iy||p[pi+1]>ay) continue;
            if(iz>p[pi+2]) continue;
            //if(p[p1+2]>p[pi+2]&&p[p2+2]>p[pi+2]&&p[p3+2]>p[pi+2]) continue;
            t=(p[pi]-p[p1])*y1-(p[pi+1]-p[p1+1])*x1;
            if(blr?t>0:t<0) continue;
            t=(p[pi]-p[p2])*y2-(p[pi+1]-p[p2+1])*x2;
            if(blr?t>0:t<0) continue;
            t=(p[pi]-p[p3])*y3-(p[pi+1]-p[p3+1])*x3;
            if(blr?t>0:t<0) continue;
            sha[idx[i]]=true;
          }
        }
      }
      return sha;
    }
/*        
    public bool[] PointsInShadow2(double[] light) {
      double[] rota=d33.M(1),l=d3.V(),a=d3.V(),b=d3.V(),c=d3.V();
      d3.normal(l,d3.mul_d(l,light,-1));
      d33.rotation_vv(rota,d3.VZ,l);
            
      bool[] sha=new bool[pts.Count];
      double[] p=new double[pts.Count*3];
      for(int i=0;i<sha.Length;i++)
        d3.mul_d33(p,3*i,pts.p,3*i,rota);
      for(int i=0;i<sha.Length;i++) {      
        bool sh=false;
        int pi=3*i;
        for(int h=0;h<fcs.N;h+=fcs.A) {
          int n=fcs.f[h++];
          if(n<3) {h+=n;continue;}
          int p1=3*fcs.f[h++],p2,p3=3*fcs.f[h++];
          n-=2;
          while(n-->0) {
            p2=p3;
            p3=3*fcs.f[h++];
            if(p1==pi||p2==pi||p3==pi) continue;
            if(p[p1+2]>p[pi+2]&&p[p2+2]>p[pi+2]&&p[p3+2]>p[pi+2]) continue;
            double cx=p[p2]-p[p1],cy=p[p2+1]-p[p1+1];
            double t,lr=(p[p3]-p[p1])*cy-(p[p3+1]-p[p1+1])*cx;
            if(lr==0) continue;
            t=(p[pi]-p[p1])*cy-(p[pi+1]-p[p1+1])*cx;
            if(lr<0?t>0:t<0) continue;
            t=(p[pi]-p[p2])*(p[p3+1]-p[p2+1])-(p[pi+1]-p[p2+1])*(p[p3]-p[p2]);
            if(lr<0?t>0:t<0) continue;
            t=(p[pi]-p[p3])*(p[p1+1]-p[p3+1])-(p[pi+1]-p[p3+1])*(p[p1]-p[p3]);
            if(lr<0?t>0:t<0) continue;
            sh=true;
            goto shadow;
          }
        }
       shadow: 
        sha[i]=sh;
      }
      return sha;
    }*/
    
    public void Shadows(double[] light,bool reverse,double weight) {
      bool[] sha=PointsInShadow(light,reverse);
      for(int h=0;h<fcs.N;h+=fcs.A) {
        int s=0,n=fcs.f[h++];
        if(faces.Hidden(fcs.f[h+n])) {
          h+=n;
          continue;
        }
        for(int i=0;i<n;i++)
          if(sha[fcs.f[h++]]) s++;
        if(s>0) {
          double d=1-weight*(double)s/n;
          fcs.f[h]=faces.RGBd(fcs.f[h],d);
        }  
          
      }
    }
    
    public string ExportOff(string filename,double[] mul,double[] add) {
     StreamWriter sw=null;
     try { 
      sw=new StreamWriter(filename);
      int n,h;
      sw.WriteLine("{0} {1}",pts.Count,fcs.Faces());
      pts.Write(sw,"{0} {1} {2}\r\n",mul,add);
      sw.WriteLine();
      for(h=0;h<fcs.N;) {
        n=fcs.f[h++];
        sw.Write(""+n);
        while(n-->0) sw.Write(" "+fcs.f[h++]);
        h+=fcs.A;
        sw.WriteLine();
      }
      return null;
     } catch(Exception ex) {
      return ex.Message; 
     } finally {
      if(sw!=null) sw.Close();     
     }
    }
    
    void MulAdd(double[] pts,int off,double[] mul,double[] add,out float x,out float y,out float z) {
      double a=pts[off],b=pts[off+1],c=pts[off+2];
      if(mul!=null) {a*=mul[0];b*=mul[1];c*=mul[2];}
      if(add!=null) {a+=add[0];b+=add[1];c+=add[2];}
      x=(float)a;y=(float)b;z=(float)c;
    }
    public string ExportStl(string filename,double[] mul,double[] add) {
     BinaryWriter bw=null;
     try { 
      bw=new BinaryWriter(new FileStream(filename,FileMode.OpenOrCreate,FileAccess.Write,FileShare.None));
      bw.Write(System.Text.Encoding.ASCII.GetBytes("cubes.exe STL-BINARY-FORMAT (http://popelkapavel.sweb.cz/cubes/cubes.htm)------\0"));
      bw.Write((int)fcs.Triangles());
      byte[] zero=new byte[12];
      for(int i=0;i<fcs.N;i+=fcs.A) {
        int n=fcs.f[i++];
        for(int j=2;j<n;j++) {
          bw.Write(zero,0,12);
          int off;
          float x,y,z;
          off=3*fcs.f[i];
          MulAdd(pts.p,off,mul,add,out x,out y,out z);          
          bw.Write(x);bw.Write(y);bw.Write(z);
          off=3*fcs.f[i+j-1];
          MulAdd(pts.p,off,mul,add,out x,out y,out z);          
          bw.Write(x);bw.Write(y);bw.Write(z);
          off=3*fcs.f[i+j];
          MulAdd(pts.p,off,mul,add,out x,out y,out z);          
          bw.Write(x);bw.Write(y);bw.Write(z);
          bw.Write((short)0);
        } 
        i+=n;       
      }
      bw.Flush();
      bw.BaseStream.SetLength(bw.BaseStream.Position);
      return null;
     } catch(Exception ex) {
      return ex.Message; 
     } finally {
      if(bw!=null) bw.Close();     
     }
    }

    public int Export3ds(BinaryWriter bw,double[] mul,double[] add,int face,int limit) {
      bool[] pm=new bool[pts.Count];
      int p=0,t=0,i=face;
      while(i<fcs.N) {
        int f=fcs.f[i];
        if(p+f>=limit||t+f-2>=limit) break;
        i++;
        if(f>2) {
          for(int j=0;j<f;j++)
            if(!pm[fcs.f[i+j]]) {
              p++;
              pm[fcs.f[i+j]]=true;
            }
          t+=f-2;
        }
        i+=f+fcs.A;
      }
      int face2=i;
      bw.Write((short)0x4100);
      int sk1=(int)bw.BaseStream.Position;
      bw.Write(6);
      bw.Write((short)0x4110);bw.Write(6+2+12*p);
      bw.Write((short)p);
      p=0;
      Dictionary<int,int> pi=new Dictionary<int,int>();
      for(i=0;i<pm.Length;i++)
        if(pm[i]) {
          pm[i]=false;
          pi[i]=p++;
          float x,y,z;
          MulAdd(pts.p,3*i,mul,add,out x,out y,out z);
          bw.Write(x);bw.Write(y);bw.Write(z);
        }
      bw.Write((short)0x4120);bw.Write(6+2+8*t);
      bw.Write((short)t);        
      for(i=face;i<face2;) {
        int f=fcs.f[i++];
        if(f>2) for(int j=2;j<f;j++) {
          bw.Write((short)pi[fcs.f[i]]);
          bw.Write((short)pi[fcs.f[i+j-1]]);
          bw.Write((short)pi[fcs.f[i+j]]);
          bw.Write((short)7);
        }
        i+=f+fcs.A;
      }
      int pos=(int)bw.BaseStream.Position;
      bw.Seek(sk1,SeekOrigin.Begin);bw.Write(pos-sk1+2);
      bw.Seek(pos,SeekOrigin.Begin);
      return face2;
    }
    public string Export3ds(string filename,double[] mul,double[] add) {
      FileStream fs=null;
     try {
      fs=new FileStream(filename,FileMode.OpenOrCreate,FileAccess.Write,FileShare.Read);
      BinaryWriter bw=new BinaryWriter(fs);      
      fs.Seek(22,SeekOrigin.Begin);      
      bw.Write((short)0x4000);
      int sk0=(int)fs.Position;
      bw.Write(6);
      bw.Write((byte)'o');bw.Write((byte)0);
      
      int len,t=fcs.Triangles();
      int limit=65536;
      if(pts.Count<limit&&t<limit) {
        bw.Write((short)0x4100);
        int sk1=(int)fs.Position;
        bw.Write(6);
        bw.Write((short)0x4110);bw.Write(6+2+12*pts.Count);
        bw.Write((short)pts.Count);
        for(int i=0,ei=3*pts.Count;i<ei;i+=3) {
          float x,y,z;
          MulAdd(pts.p,i,mul,add,out x,out y,out z);
          bw.Write(x);bw.Write(y);bw.Write(z);          
        }
        bw.Write((short)0x4120);bw.Write(6+2+8*t);
        bw.Write((short)t);
        for(int i=0;i<fcs.N;) {
          int f=fcs.f[i++];
          for(int j=2;j<f;j++) {
            bw.Write((short)fcs.f[i]);
            bw.Write((short)fcs.f[i+j-1]);
            bw.Write((short)fcs.f[i+j]);
            bw.Write((short)7);
          }
          i+=f+fcs.A;
        }
        len=(int)fs.Position;
        bw.Seek(sk1,SeekOrigin.Begin);bw.Write(len-sk1+2);
        bw.Seek(len,SeekOrigin.Begin);
      } else {
        int face=0;
        while(face<fcs.N)
          face=Export3ds(bw,mul,add,face,limit);
      }

      len=(int)fs.Position;
      fs.SetLength(fs.Position);            
      fs.Seek(sk0,SeekOrigin.Begin);bw.Write(len-sk0+2);
      fs.Seek(0,SeekOrigin.Begin);
      bw.Write((short)0x4d4d);bw.Write(len);
      bw.Write((short)0x2);bw.Write(10);bw.Write(3);
      bw.Write((short)0x3d3d);bw.Write(len-16);
      bw.BaseStream.SetLength(bw.BaseStream.Position);
      return null;
     } catch(Exception ex) {
      return ex.Message;
     } finally {
      if(fs!=null) fs.Close();
     }
    }
    public string ExportObj(string filename,double[] mul,double[] add) {
     StreamWriter sw=null;
     try { 
      sw=new StreamWriter(filename);
      int n,h;
      sw.WriteLine("# wavefront {0} points  {1} faces",pts.Count,fcs.Faces());
      pts.Write(sw,"v {0} {1} {2}\r\n",mul,add);
      sw.WriteLine();
      for(h=0;h<fcs.N;) {
        n=fcs.f[h++];
        sw.Write("f");
        while(n-->0) sw.Write(" "+(fcs.f[h++]+1));
        h+=fcs.A;
        sw.WriteLine();
      }
      return null;
     } catch(Exception ex) {
      return ex.Message; 
     } finally {
      if(sw!=null) sw.Close();     
     }
    }
    public string ExportVrml(string filename,double[] mul,double[] add,int color,double[] light) {
      StreamWriter sw=null;
     try {
      sw=new StreamWriter(filename);
      sw.WriteLine("#VRML V1.0 ascii\nSeparator {");
      sw.WriteLine("Material {{ diffuseColor {0} {1} {2}}}",(color&255)/255.0,((color>>8)&255)/255.0,((color>>16)&255)/255.0);
      sw.WriteLine("DirectionalLight {{ direction {0} {1} {2} }}",light[0],light[1],light[2]);
      sw.WriteLine("Coordinate3 {");
      sw.WriteLine("point [");
      pts.Write(sw,"{0} {1} {2}\r\n",mul,add);
      sw.WriteLine("  0 0 0]\n }\n IndexedFaceSet {\n  coordIndex [");
      int h,n;
      for(h=0;h<fcs.N;) {
        n=fcs.f[h++];
        while(n-->0) sw.Write(" "+fcs.f[h++]+",");
        h+=fcs.A;
        sw.WriteLine("-1,");
      }
      sw.WriteLine(" 0,0,0,-1]\n  }\n}\n");
      return null;
     } catch(Exception ex) {
      return ex.Message;
     } finally {
       if(sw!=null) sw.Close();
     }
    }
    public string ExportPov(string filename,double[] mul,double[] add,bool normals,bool colors) {
     StreamWriter sw=null;
     try { 
      sw=new StreamWriter(filename);
      int n,h;
      sw.WriteLine("vertex_vectors {{ {0}\r\n",pts.Count);
      pts.Write(sw," ,<{0},{1},{2}>\r\n",mul,add);
      sw.WriteLine("}");
      
      if(normals) {
        double[] vn=PointsNormals(mul);
        sw.WriteLine("normal_vectors {{ {0}\r\n",pts.Count);
        for(h=0;h<vn.Length;h+=3)
          sw.WriteLine(" ,<{0},{1},{2}>",vn[h],vn[h+1],vn[h+2]);
        sw.WriteLine("}");
      }
      if(colors) {
        sw.WriteLine("texture_list {{ {0}",fcs.Faces());
        for(h=0;h<fcs.N;h+=fcs.A) {
          h+=1+fcs.f[h];
          int rgb=fcs.f[h];
          sw.WriteLine("texture{{pigment{{rgb<{0},{1},{2}>/255}}}}",rgb&255,(rgb>>8)&255,(rgb>>16)&255);
        }
        sw.WriteLine("}");
      }
      
      sw.WriteLine("face_indices {{ {0}",fcs.Triangles());
      int i=0;
      for(h=0;h<fcs.N;i++) {
        n=fcs.f[h++];
        //sw.Write("f");
        for(int j=1;j<n-1;j++) {
          sw.Write(",<{0},{1},{2}>",fcs.f[h],fcs.f[h+j],fcs.f[h+j+1]);
          if(colors)
            sw.Write(",{0}",i);
        }
        h+=n+fcs.A;
        sw.WriteLine();
      }      
      sw.WriteLine("}");
      return null;
     } catch(Exception ex) {
      return ex.Message; 
     } finally {
      if(sw!=null) sw.Close();     
     }
    }
    
    internal double PainterDepth(int h,double[] pts,out double depth2,out double depth3) {
      double depth=0;
      depth2=depth3=depth;
      int n=fcs.f[h++];
      while(n-->0) {
        int p=3*fcs.f[h++];
        double d=pts[p]*pts[p]+pts[p+1]*pts[p+1]+pts[p+2]*pts[p+2];
        if(d>=depth) depth=d;
        else if(d>=depth2) depth2=d;
        else if(d>=depth3) depth3=d;
      }      
      return depth;
    }
    internal class PainterComparer:IComparer<int> {
      mesh m;
      double[] pts;
      internal PainterComparer(mesh m,double[] pts) {
        this.m=m;this.pts=pts;
      }
      public int Compare(int a,int b) {
        double a2,a3,da=m.PainterDepth(a,pts,out a2,out a3),b2,b3,db=m.PainterDepth(b,pts,out b2,out b3);
        return da<db?1:da>db?-1:a2<b2?1:a2>b2?-1:a3<b3?1:a3>b3?-1:0;
      }
    }
    
    internal int PainterSort(double[] pts,double zmin,out int[] fa) {
      fa=new int[fcs.N];
      int fe,h,n;
      for(fe=0,h=0;h<fcs.N;h+=fcs.A) {
        bool ok=true;
        fa[fe]=h;
        n=fcs.f[h++];
        while(n-->0) {
          int p=3*(fcs.f[h++]);
          if(pts[p+2]<zmin) ok=false;
        } 
        if(ok) fe++;
      }
      Array.Sort(fa,0,fe,new PainterComparer(this,pts));
      return fe;
    }
    string Int2HtmlColor(int c) {
      char[] cha=new char[7];
      c=((c>>16)&255)|(c&0xff00)|((c&255)<<16);
      cha[0]='#';
      for(int i=6;i>0;i--) {
        int x=c&15;
        cha[i]=(char)(x<10?'0'+x:'A'+x-10);
        c>>=4;
      }        
      return new string(cha);
    }
    static double atof(string s,double def) {
      double d;
      int h=0;
      return ParseDouble(ref h,s,out d)?d:def;
    }
    static string ftoa(double x) {
      return x.ToString(System.Globalization.CultureInfo.InvariantCulture);
    }    
    static string ftoa(double x,int dec) {
      return x.ToString(dec==0?"0":dec==1?".0":dec==2?".00":dec==3?".000":"",System.Globalization.CultureInfo.InvariantCulture);
    }    
    
    public string ExportSVG(double[] pts,int color,bool black,float lwidth,bool wirefront,int width,int height,double angle,double zmin,string filename) {
      StreamWriter sw=null;
     try {
      sw=new StreamWriter(filename);
      sw.Write("<?xml version='1.0' encoding='UTF-8' standalone='no'?>\n"
        +"<svg xmlns:svg='http://www.w3.org/2000/svg' xmlns='http://www.w3.org/2000/svg' version='1.0' width='"+width+"' height='"+height+"' id='xx'>\n"
        +(black?" <rect width='"+width+"' height='"+height+"' style='fill:#000000;stroke:#000000;' />":null)
        +" <g  d='layer1' style='stroke-linejoin:round;stroke-width:"+ftoa(lwidth<=1?0.25:lwidth)+";fill:none"+(color==-1?null:";stroke:"+Int2HtmlColor(color))+"'>\n");
      
      int[] fa;
      int fh,h,n,fe=PainterSort(pts,zmin,out fa);
      double c=width/2.0/Math.Tan(angle/2*Math.PI/180);
      double[] normal=null,normal2=null;
      for(fh=0;fh<fe;fh++) {
        h=fa[fh];
        int fill=0xffffff;
        sw.Write("  <path d='M");
        n=fcs.f[h++];
        while(n-->0) {
          int p=3*(fcs.f[h++]);
          double x=width/2+c*pts[p]/pts[p+2],y=height/2.0+c*pts[p+1]/pts[p+2];
          sw.Write("{0} {1} {2}",ftoa(x,1),ftoa(y,1),(n>0?'L':'z'));
          //sw.Write(ftoa(x)+','+ftoa(y)+' '+(n>0?'L':'z'));
        }
        fill=fcs.f[h];
        bool solid=true;
        if(wirefront) {
          if(normal==null) {
            normal=d3.V();
            normal2=d3.V();
          }
          FaceNormal(pts,fa[fh],normal);
          d3.normal(normal2,0,pts,fcs.f[fa[fh]+1]*3);
          double s=d3.scalar(normal,normal2);
          solid=s>=0;
        }
        if(solid)
          sw.Write("' style='fill:"+Int2HtmlColor(fill)+(color==-1?";stroke:"+Int2HtmlColor(fill):null)+";' />\n");
        else
          sw.Write("' style='stroke:"+Int2HtmlColor(fill)+";' />\n");
      }      
      sw.Write(" </g>\n</svg>");  
      return null;
     } catch(Exception ex) {
      return ex.Message;
     } finally {
      if(sw!=null) sw.Close();
     }
    }

    static string PDFColor(int color,bool stroke) {
      return string.Format("{0} {1:.000} {2:.000} {3}",
        ftoa((color&255)/255.0,3),ftoa(((color>>8)&255)/255.0,3),ftoa(((color>>16)&255)/255.0,3),stroke?"RG":"rg");    
    }
    public string ExportPDF(double[] pts,int color,bool black,float lwidth,bool wirefront,int width,int height,double angle,double zmin,string filename) {
      StreamWriter sw=null;
      bool compress=true;
     try {
      sw=new StreamWriter(filename);
      sw.Write(@"%PDF-1.0
1 0 obj
<<
/Type /Catalog
/Pages 2 0 R
>>
endobj


2 0 obj
<<
/Type /Pages
/Count 1
/Kids[3 0 R]
>>
endobj

3 0 obj
<<
/Type /Page
/Parent 2 0 R
/Resources << /ProcSet 5 0 R>>
/MediaBox[0 0 "+width+" "+height+@"]
/Contents 4 0 R
>>
endobj

4 0 obj
<< "+(compress?"/Filter [ /ASCII85Decode /LZWDecode ] ":null)+"/Length "+width+@" >>
stream
");
      PDFCompress lzw=new PDFCompress(sw,compress);
      lzw.Write(@"
1 j
"+ftoa(lwidth<=1?0.1:lwidth,1)+@" w
");
      bool wires=color!=-1;      
      if(black) lzw.WriteLine("0 0 0 rg\n-1 -1 "+(width+1)+" "+(height+1)+" re f");
      lzw.WriteLine(PDFColor(color,true));
      int[] fa;
      int fh,h,n,fe=PainterSort(pts,zmin,out fa);
      double c=width/2.0/Math.Tan(angle/2*Math.PI/180);
      double[] normal=null,normal2=null;
      bool wire=false;
      for(fh=0;fh<fe;fh++) {
        h=fa[fh];
        int fill=0xffffff;
        n=fcs.f[h++];
        fill=fcs.f[h+n];
        bool solid=true;
        if(wirefront) {
          if(normal==null) {
            normal=d3.V();
            normal2=d3.V();
          }
          FaceNormal(pts,fa[fh],normal);
          d3.normal(normal2,0,pts,fcs.f[fa[fh]+1]*3);
          double s=d3.scalar(normal,normal2);
          solid=s>=0;
        }
        if(solid) {         
          if(wire) lzw.WriteLine(PDFColor(color,true));
          lzw.WriteLine(PDFColor(fill,false)+(wires?null:" "+PDFColor(fill,true)));
          wire=false;
        } else {
          lzw.WriteLine(PDFColor(fill,true));
          wire=true;
        } 
        char ch='m';  
        while(n-->0) {
          int p=3*(fcs.f[h++]);
          double x=width/2+c*pts[p]/pts[p+2],y=height/2.0+c*pts[p+1]/pts[p+2];
          lzw.WriteLine(string.Format("{0} {1} {2}",ftoa(x,1),ftoa(height-y,1),ch));
          ch='l';
        }
        lzw.WriteLine(wire?"s":"b");// 'f' for just fill
      }
      lzw.Flush();
sw.Write(@"
endstream
endobj

5 0 obj
[
/PDF
]
endobj

xref
0 6
0000000000 65535 f
0000000016 00000 n
0000000086 00000 n
0000000136 00000 n
0000000200 00000 n
0000000328 00000 n
trailer
<<
/Size 8
/Root 1 0 R
>>
startxref
1110
%%EOF");            
      return null;
     } catch(Exception ex) {
      return ex.Message;
     } finally {
      if(sw!=null) sw.Close();
     }
    }

    public void Draw2D(IntPtr fdc,double[] pts,int color,int lwidth,bool wirefront,int sx,int sy,int width,int height,double angle,double zmin) {
      bool wpen=color!=-1&&lwidth>1;
      IntPtr solidpen=wpen?GDI.CreatePen(GDI.PenStyles.PS_SOLID,lwidth,0):GDI.GetStockObject(color==-1?GDI.StockObjects.NULL_PEN:color==0xffffff?GDI.StockObjects.WHITE_PEN:GDI.StockObjects.BLACK_PEN);
      GDI.SelectObject(fdc,solidpen);
      //xdpi=GDI.GetDeviceCaps(fdc,88);
      //ydpi=GDI.GetDeviceCaps(fdc,90);
      //Dictionary<int,IntPtr> brushes=new Dictionary<int,IntPtr>();
      IntPtr brush,brush2=IntPtr.Zero,pen=IntPtr.Zero,pen2=IntPtr.Zero;
      int[] fa;
      int fh,h,n,n2,fe=PainterSort(pts,zmin,out fa),g;
      double c=width/2.0/Math.Tan(angle/2*Math.PI/180);
      POINT[] pt2=new POINT[16];
      double[] normal=null,normal2=null;
      for(fh=0;fh<fe;fh++) {
        h=fa[fh];
        int fill=0xffffff;
        n2=n=fcs.f[h++];
        if(pt2.Length<2*n) Array.Resize(ref pt2,2*n);
        g=0;
        while(n-->0) {
          int p=3*(fcs.f[h++]);
          double x=width/2+c*pts[p]/pts[p+2],y=height/2.0+c*pts[p+1]/pts[p+2];
          pt2[g].x=sx+(int)x;
          pt2[g++].y=sy+(int)y;
        }
        fill=fcs.f[h];
        bool solid=true;
        if(wirefront) {
          if(normal==null) {
            normal=d3.V();
            normal2=d3.V();
          }
          FaceNormal(pts,fa[fh],normal);
          d3.normal(normal2,0,pts,fcs.f[fa[fh]+1]*3);
          double s=d3.scalar(normal,normal2);
          solid=s>=0;
        }
        if(solid) {          
          if(pen!=IntPtr.Zero) {
            pen2=GDI.SelectObject(fdc,solidpen);
            GDI.DeleteObject(pen2);
            pen=IntPtr.Zero;
          }
          brush=GDI.CreateSolidBrush(fill);
          brush2=GDI.SelectObject(fdc,brush);
          GDI.Polygon(fdc,pt2,n2);
          GDI.DeleteObject(brush2);
        } else {
          pen=GDI.CreatePen(GDI.PenStyles.PS_SOLID,lwidth,fill);
          pen2=GDI.SelectObject(fdc,pen);
          GDI.Polyline(fdc,pt2,n2);
          GDI.DeleteObject(pen2);          
        }  
      }
      GDI.SelectObject(fdc,GDI.GetStockObject(GDI.StockObjects.BLACK_PEN));
      if(wpen) GDI.DeleteObject(solidpen);
    }
    public string ExportEMF(double[] pts,int color,bool black,int lwidth,bool wirefront,int width,int height,double angle,double zmin,string filename,IntPtr hdc) {
      //int xdpi=GDI.GetDeviceCaps(hdc,88),ydpi=GDI.GetDeviceCaps(hdc,90);
      int xsiz=GDI.GetDeviceCaps(hdc,4)*100,xres=GDI.GetDeviceCaps(hdc,8);
      int ysiz=GDI.GetDeviceCaps(hdc,6)*100,yres=GDI.GetDeviceCaps(hdc,10);
      RECT r=new RECT() {Right=width*xsiz/xres,Bottom=height*ysiz/yres};
      IntPtr fdc=GDI.CreateEnhMetaFile(hdc,filename,ref r,null);
      if(black)
        GDI.FillRect(fdc,ref r,GDI.GetStockObject(GDI.StockObjects.BLACK_BRUSH));
     try {   
      Draw2D(fdc,pts,color,lwidth,wirefront,0,0,width,height,angle,zmin);
      return null;
     } catch(Exception ex) {
      return ex.Message;
     } finally {
      IntPtr edc=GDI.CloseEnhMetaFile(fdc);
      if(filename==null) {
        if(WINAPI.OpenClipboard(IntPtr.Zero)) {
          WINAPI.EmptyClipboard();
          WINAPI.SetClipboardData(14/*CF_ENHMETAFILE*/,edc);
          WINAPI.CloseClipboard();
        }  
      }
      GDI.DeleteEnhMetaFile(edc); 
     }
    }
    
/*int MeshExportVRML(Mesh *m,char *filename) {
  FILE *f;
  int n,*h,*he;

  if(!(f=fopen(filename,"w")))
    return 0;

  fputs(
      "#VRML V1.0 ascii\nSeparator {\n"
      "Material { diffuseColor 0.3 0.8 0.9}\n"
      "Coordinate3 {\n"
      "point [\n",f);
  PointsFPrintf(&m->pts,f,"  %f %f %f,\n");

  fputs("  0 0 0]\n }\n IndexedFaceSet {\n  coordIndex [\n",f);

  for(h=m->fcs.r,he=h+m->fcs.n;h<he;) {
    n=*h++;
    while(n--) fprintf(f," %d,",*h++);
    h+=m->fcs.a;
    fputs("-1,\n",f);
  }
  fputs(" 0,0,0,-1]\n  }\n}\n",f);
  fclose(f);
  return 1;
}*/
        
    static bool ParseInt(ref int h,string s,out int i) {
      i=0;
      if(s==null||h<0||h>=s.Length) return false;
      while(h<s.Length&&char.IsWhiteSpace(s[h])) h++;
      bool neg=false;
      if(h<s.Length&&s[h]=='-') {neg=true;h++;}
      while(h<s.Length) {
        char ch=s[h];
        if(ch<'0'||ch>'9') break;
        i=i*10+ch-'0';
        h++;
      }
      if(neg) i=-i;
      return true;
    }
    static double log10=Math.Log(10);
    static bool ParseDouble(ref int h,string s,out double d,out double dec,bool nocom) {
      d=dec=0;
      if(s==null||h<0||h>=s.Length) return false;
      while(h<s.Length&&char.IsWhiteSpace(s[h])) h++;
      if(h<0||h>=s.Length) return false;
      bool neg=s[h]=='-';
      if(neg) h++;
      int digi=0;
      while(h<s.Length) {
        char ch=s[h];
        if(ch>='0'&&ch<='9') {
          d=d*10+(ch-'0');
          dec*=10;
          digi++;
        } else if(ch=='.'||ch==','&&!nocom) 
          dec=1;
         else
          break;
        h++;
      }
      if(digi<1) return false;
      if(dec<1) dec=1;
      if(h<s.Length&&(s[h]=='e'||s[h]=='E')) {
        h++;
        int exp;
        if(!ParseInt(ref h,s,out exp))
          return false;
        if(exp!=0) {  
          double mul;
          switch(exp) {
           case -6:dec*=1000000;break;
           case -5:dec*=100000;break;
           case -4:dec*=10000;break;
           case -3:dec*=1000;break;
           case -2:dec*=100;break;
           case -1:dec*=10;break;
           case 1:dec/=10;break;
           case 2:dec/=100;break;
           case 3:dec/=1000;break;
           case 4:dec/=10000;break;
           case 5:dec/=100000;break;
           case 6:dec/=1000000;break;
           default:dec*=Math.Exp(-log10*exp);break;
          }   
        }  
      }
      if(neg) d=-d;
      return true;
    }    
    public static bool ParseDouble(ref int h,string s,out double d) { return ParseDouble(ref h,s,out d,false);}
    public static bool ParseDouble(ref int h,string s,out double d,bool nocom) {
      double dec;
      bool ok=ParseDouble(ref h,s,out d,out dec,nocom);
      if(ok) d/=dec;
      return ok;
    }
    public static bool ParseDoubleExp(ref int h2,string s,out double d) {
      d=0;
      if(s==null||s=="") return false;
      double sum=0,prod=0,i=0;
      char op='.';
      bool neg=false,l2=false,dig=false;
      int h=h2;
      while(h<s.Length) {
        if(!ParseDouble(ref h,s,out i,true)) return false ;
        dig=true;
        if(h>=s.Length) break;
        if(s[h]=='+'||s[h]=='-') { 
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
        } else break;
        h++;
      }
      if(dig) {
        if(neg) i=-i;
        if(l2) i=op=='*'?prod*i:op=='/'?prod/i:prod%i;
        sum+=i;
      }
      h2=h;d=sum;
      return true;
    }
    public static bool ParseDouble3Exp(double[] d3,string s) {
      int h=0;
      double d0=0;
      if(!ParseDoubleExp(ref h,s,out d0)) return false;
      if(h>=s.Length) { d3[0]=d3[1]=d3[2]=d0; return true;}
      double d1=0,d2=0;
      h++;
      if(!ParseDoubleExp(ref h,s,out d1)) return false;
      h++;
      if(!ParseDoubleExp(ref h,s,out d2)) return false;
      d3[0]=d0;d3[1]=d1;d3[2]=d2;
      return true;
    }

    public string ImportOff(string filename) {
     StreamReader sr=null; 
     try {
      Stream fs=new FileStream(filename,FileMode.Open,FileAccess.Read,FileShare.Read);
      if(isgz(filename)) fs=new System.IO.Compression.GZipStream(fs,System.IO.Compression.CompressionMode.Decompress,false);
      sr=new StreamReader(fs);
      int p=0,pi=0,f=0,fi=0,s=0;
      double[] d=new double[3];
      int[] pt=new int[8];
      string line;
      while(null!=(line=sr.ReadLine())) {        
        int h=0;
        while(h<line.Length&&char.IsWhiteSpace(line[h])) h++;
        if(h>=line.Length) continue;
        if(line[h]=='#') continue;
        switch(s) {
         case 0:
          ParseInt(ref h,line,out p);ParseInt(ref h,line,out f);
          pts.Alloc(p);
          fcs.Alloc(f*(4+fcs.A));
          s=1;
          break;
         case 1:
          ParseDouble(ref h,line,out d[0]);
          ParseDouble(ref h,line,out d[1]);
          ParseDouble(ref h,line,out d[2]);
          pts.Add(d);
          pi++;
          if(pi>=p) s=2;
          break;
         case 2:
          int n;
          ParseInt(ref h,line,out n);
          if(pt.Length<n) pt=new int[n];
          for(int i=0;i<n;i++)
            ParseInt(ref h,line,out pt[i]);
          fcs.Face(n,pt,0,POINTS.None);
          fi++;
          if(fi>=f) s=3;
          break;
        }
      }
      sr.Close();
      return null; 
     } catch(Exception ex) {
       if(sr!=null) sr.Close();
       return ex.Message;
     }
    }
    static char hexdigi(int x,bool up) { if(up) x>>=4;x&=15;return (char)(x<10?x+'0':x+'a'-10);}
    
    string hexdigi(byte[] data,int start,int length) {
      char[] cha=new char[2*length];
      for(int i=0;i<length;i++) {
        cha[2*i]=hexdigi(data[start+i],true);
        cha[2*i+1]=hexdigi(data[start+i],false);
      }
      return new string(cha);
    }
    public static bool ends(string text,string suff) {
      return text!=null&&text.EndsWith(suff,StringComparison.InvariantCultureIgnoreCase);
    }
    public static bool isgz(string text) { return ends(text,".gz");}

    public string ImportStl(string filename) {
      BinaryReader br=null;
      TextReader tr=null;
     try {
      Stream fs=new FileStream(filename,FileMode.Open,FileAccess.Read,FileShare.Read);
      if(isgz(filename)) fs=new System.IO.Compression.GZipStream(fs,System.IO.Compression.CompressionMode.Decompress,false);
      br=new BinaryReader(fs);
      byte[] hdr=br.ReadBytes(80); // skip header
      bool txt=hdr[79]!=0&&System.Text.Encoding.ASCII.GetString(hdr,0,6)=="solid ";
      Hashtable point=new Hashtable();
      int[] abc=new int[3];
      if(txt) {
        br.BaseStream.Seek(0,SeekOrigin.Begin);
        tr=new StreamReader(br.BaseStream,System.Text.Encoding.ASCII);
        string line;
        string[] sa=new string[5];
        int j=0;
        while(null!=(line=tr.ReadLine())) {
          int g=0,h2,h=0;
          while(h<line.Length) {
            while(h<line.Length&&char.IsWhiteSpace(line[h])) h++;
            h2=h;
            while(h<line.Length&&!char.IsWhiteSpace(line[h])) h++;
            if(h>h2&&g<sa.Length) sa[g++]=line.Substring(h2,h-h2);
          }
          if(g<1) continue;
          if(sa[0]=="vertex"&&g>3) {
            string hs=sa[1]+":"+sa[2]+":"+sa[3];
            object o=point[hs];
            if(o==null) {
              point[hs]=abc[j]=pts.Count;
              pts.Add(float.Parse(sa[1]),float.Parse(sa[2]),float.Parse(sa[3]));
            } else abc[j]=(int)o;
            j++;
          } else if(sa[0]=="endloop") {
            if(j==3) fcs.Triangle(abc[0],abc[1],abc[2],POINTS.None);
            j=0;
          } else if(sa[0]=="endsolid") break;
          else continue;
        }
        return null;
      } else {
        int n=br.ReadInt32();
        for(int i=0;i<n;i++) {
          byte[] tri=br.ReadBytes(50);
          for(int j=0;j<3;j++) {
            int off=12*(j+1);
            string hs=hexdigi(tri,off,12);
            object o=point[hs];
            if(o==null) {
              point[hs]=abc[j]=pts.Count;
              pts.Add(BitConverter.ToSingle(tri,off),BitConverter.ToSingle(tri,off+4),BitConverter.ToSingle(tri,off+8));
            } else abc[j]=(int)o;
          }        
          fcs.Triangle(abc[0],abc[1],abc[2],POINTS.None);
        }
       }
     } catch(Exception ex) {
       return ex.Message;
     } finally {
       if(br!=null) br.Close();
     }  
     return null;
    }
    public string ImportObj(string filename) {
     StreamReader sr=null; 
     try {
      Stream fs=new FileStream(filename,FileMode.Open,FileAccess.Read,FileShare.Read);
      if(isgz(filename)) fs=new System.IO.Compression.GZipStream(fs,System.IO.Compression.CompressionMode.Decompress,false);
      sr=new StreamReader(fs);
      int p=0,pi=0,f=0,fi=0,s=0;
      double[] d=new double[3];
      int[] pt=new int[8];
      string line;
      while(null!=(line=sr.ReadLine())) {        
        int h=0;
        while(h<line.Length&&char.IsWhiteSpace(line[h])) h++;
        if(h>=line.Length-2) continue;
        if(line[h]=='#') continue;
        if(line[h]=='v'&&line[h+1]==' ') {
          h+=2;
          ParseDouble(ref h,line,out d[0]);
          ParseDouble(ref h,line,out d[1]);
          ParseDouble(ref h,line,out d[2]);
          pi=pts.Count*3;
          pts.append_space(1);
          d3.copy(pts.p,pi,d,0);          
        } else if(line[h]=='f'&&line[h+1]==' ') {
          h+=2;
          int n=0,i;
          while(h<line.Length&&ParseInt(ref h,line,out i)) {
            if(n>=pt.Length) Array.Resize(ref pt,pt.Length*2);
            pt[n++]=i-1;
            while(h<line.Length&&!char.IsWhiteSpace(line,h)) h++;
          }
          if(n>2) {
            fcs.Face(n,pt,0,POINTS.None);
          }
        }
      }
      sr.Close();
      return null; 
     } catch(Exception ex) {
       if(sr!=null) sr.Close();
       return ex.Message;
     }
    }
    int PgmNumber(FileStream fs) {
      int n=0,ch;
      bool digi=false;
     again: 
      while((ch=fs.ReadByte())>0) {
        if(!digi&&(ch==' '||ch=='\r'||ch=='\n'))
          continue;
        if(ch=='#') {
          while((ch=fs.ReadByte())>0)
            if(ch=='\n')
              if(n==0) goto again;
            else
              return n;  
          return n;
        }
        if(ch<'0'||ch>'9') return n;
        digi=true;
        n=n*10+(ch-'0');
      }
      return n;
    }
    internal struct Import3dOp {
      internal char op;
      internal double[] param;

      internal Import3dOp(char mode,double[] value) {
        this.op=mode;this.param=value;
      }
      public override string ToString() {
        string s=""+op;
        foreach(double x in param) s+=" "+param;
        return s;
      }

    }
    internal class Import3dtContext {
      internal mesh msh;
      internal bool reverse;      
      internal List<Import3dOp> stack=new List<Import3dOp>();
      internal double[] a=d3.V(),b=d3.V(),c=d3.V();
      internal int i,j,k;

      internal Import3dtContext(mesh msh) { this.msh=msh;}

      static bool F(string s,char ch) {
        return s.IndexOf(ch)>=0;
      }
      internal void Tx(double[] x) {
        if(stack.Count>0) for(int i=stack.Count-1;i>=0;i--) {
          Import3dOp op=stack[i];
          if(op.op=='s') d3.mul(x,x,op.param);
          else if(op.op=='r') d3.mul_d33(x,x,op.param);
          else d3.add(x,op.param);
        }
      }
      internal void Tx(int pbase) {
        double[] x=d3.V();
        for(int i=pbase,j=3*i;i<msh.pts.Count;i++,j+=3) {
          x[0]=msh.pts.p[j+0];
          x[1]=msh.pts.p[j+1];
          x[2]=msh.pts.p[j+2];
          Tx(x);
          msh.pts.p[j+0]=x[0];
          msh.pts.p[j+1]=x[1];
          msh.pts.p[j+2]=x[2];
        }
      }
      internal bool Line(string line) {
        line=line.Trim();
        if(line==""||!char.IsLetter(line[0])) return false;
        string[] sa=Regex.Split(line,@"\s+");
        string cmd=sa[0],flags=sa.Length>1?sa[1]:"";
        bool b0,b1;
        char ch;
        double d0;
        int s;        
        switch(cmd) {
         case "exit":return true;
         case "reverse":reverse^=true;break;
         case "pos":
          ParseDouble3Exp(a,sa[1]);
          stack.Add(new Import3dOp('p',d3.V(a)));
          break;
         case "scale":
          ParseDouble3Exp(a,sa[1]);
          stack.Add(new Import3dOp('s',d3.V(a)));
          break;
         case "pop":
          if(stack.Count>0) 
            stack.RemoveAt(stack.Count-1);
          break;
         case "import":
          msh.Import3dt(sa[1],this);
          break;
         case "ball":
         case "box":
         case "cone":
         case "cyli":
         case "toro": {
          short axes=0x021;
          b0=false;
          foreach(char f in flags) 
            if(f=='x') axes=0x102;
            else if(f=='y') axes=0x021;
            else if(f=='z') axes=0x210;
            else if(f=='X') axes=0x2201;
            else if(f=='Y') axes=0x2120;
            else if(f=='Z') axes=0x2012;
            else if(f=='r') b0^=true;
          ParseDouble3Exp(a,sa[2]);
          ParseDouble3Exp(b,sa[3]);
          b1=cmd[2]!='x';
          if(F(flags,'b')) {
            d3.mul_d(b,d3.sub(b,b,a),0.5);
            d3.add(a,b);
            d3.abs(b);
          }
          s=4;
          if(cmd=="toro") {
            d0=atof(sa[s++],0.1);//ftoa(sa[s++]);
          } else d0=0;
          i=j=k=b1?12:1;
          if(sa.Length>s) i=j=k=int.Parse(sa[s++]);
          if(sa.Length>s) j=int.Parse(sa[s++]);
          if(sa.Length>s) k=int.Parse(sa[s++]);
          int pbase=msh.pts.Count;
          if(cmd=="ball")
            msh.Sphere(a,d33.M1,b,j,i,b0?POINTS.REVERSE:POINTS.None,axes);
          else if(cmd=="cone")
            msh.Cone(a,b,j,k,i,b0?POINTS.REVERSE:POINTS.None,axes);
          else if(cmd=="cyli")
            msh.Cylinder(a,b,j,k,i,b0?POINTS.REVERSE:POINTS.None,axes);
          else if(cmd=="toro")
            msh.Toroid(a,b,d0,j,1.0,i,b0?POINTS.REVERSE:POINTS.None,axes);
          else
            msh.Box(a,b,i+1,j+1,k+1,b0?POINTS.REVERSE:POINTS.None);
            
          Tx(pbase);
          } break;
        }
        return false;
      }
    }
    public string Import3dt(string filename) { return Import3dt(filename,new Import3dtContext(this));}
    internal string Import3dt(string filename,Import3dtContext ctx) {
     try {
      using(TextReader tr=new StreamReader(filename)) {
        string line;
        while(null!=(line=tr.ReadLine()))           
          if(ctx.Line(line)) break;
      }
      return null;
     } catch(Exception ex) {
       return ex.Message;
     }
    }
    public string ImportPgm(string filename) {
      FileStream fs=null;
     try {
      fs=new FileStream(filename,FileMode.Open,FileAccess.Read,FileShare.Read);
      int a=fs.ReadByte(),b=fs.ReadByte();
      if(a!='P'||(b!='2'&&b!='5'))
        return "not pgm";      
      bool ascii=b=='2';
      fs.ReadByte();
      int width=PgmNumber(fs),height=PgmNumber(fs),max=PgmNumber(fs);
      pts.Array2D(d3.V0,new double[] {width,0,0},new double[] {0,0,height},width,height);
      int he=width*height;      
      for(int h=0;h<he;h++) {
        double y;
        if(ascii) y=PgmNumber(fs);
        else y=fs.ReadByte();
        pts.p[3*h+1]=y;
      }  
      fcs.Array2D(0,width,height,POINTS.None);
      fs.Close();
      return null;
     } catch(Exception ex) {
      return ex.Message;
     } finally {
       if(fs!=null) fs.Close();
     }
    }
    // primitives
    public bool Sphere(double[] center,double[] rota,double[] scale,int layers,int halfcircles,POINTS mode,short axes) {
      int pbase=pts.Count;
      if(!pts.Sphere(center,rota,scale,layers,halfcircles,axes)
          ||!fcs.Rotoid(pbase,layers,halfcircles,POINTS.CAP|POINTS.CAP2|(mode&POINTS.REVERSE))) {
        pts.Count=pbase;
        return false;
      }  
      return true;
    }
    public bool Cylinder(double[] center,double[] scale,int layers,int circles,int radials,POINTS mode,short axes) {
      int pbase=pts.Count;
      if(!pts.Cylinder(center,scale,layers,circles,radials,axes)
          ||!fcs.Rotoid(pbase,layers+2*circles,radials,POINTS.CAP|POINTS.CAP2|(mode&POINTS.REVERSE))) {
        pts.Count=pbase;
        return false;
      }
      return true;
    }
    public bool Cone(double[] center,double[] scale,int layers,int circles,int radials,POINTS mode,short axes) {
      int pbase=pts.Count;
      if(!pts.Cone(center,scale,layers,circles,radials,axes)
          ||!fcs.Rotoid(pbase,layers+circles,radials,POINTS.CAP|POINTS.CAP2|(mode&POINTS.REVERSE))) {
        pts.Count=pbase;
        return false;
      }
      return true;
    }
    public bool Toroid(double[] center,double[] scale,double radius2,int points,double radius,int radials,POINTS mode,short axes) {
      int pbase=pts.Count;
      double[] r2=new double[3*points];
      for(int p=0;p<points;p++) {
        double a=p*2*Math.PI/points;
        r2[3*p+0]=Math.Cos(a)*radius2;
        r2[3*p+1]=-Math.Sin(a)*radius2;        
      }
      if(!pts.Toroid(center,scale,points,r2,radius,radials,mode,axes)
          ||!fcs.Array2D(pbase,points,radials,mode|POINTS.CYCLE|POINTS.CYCLE2)) {
        pts.Count=pbase;
        return false;
      }
      return true;
    }
    public bool Toroid(double[] center,double[] scale,int points,double[] point,double radius,int radials,POINTS mode,short axes) {
      int pbase=pts.Count;
      if(!pts.Toroid(center,scale,points,point,radius,radials,mode,axes)
          ||!fcs.Array2D(pbase,points,radials,mode|POINTS.CYCLE|POINTS.CYCLE2)) {
        pts.Count=pbase;
        return false;
      }
      return true;
    }
// box 
    public bool Box(double[] center,double[] scale,int xres,int yres,int zres,POINTS mode) {
      int pbase=pts.Count;
      if(!pts.Box(center,scale,xres,yres,zres)
          ||!fcs.Box(pbase,xres,yres,zres,mode&POINTS.REVERSE)) {
        pts.Count=pbase;
        return false;
      }
      return true;
    }
    public bool Tetrahedron(double[] center,double[] scale,POINTS mode) {
      int pbase=pts.Count;
      if(!pts.Tetrahedron(center,scale)
           ||!fcs.Tetrahedron(pbase,mode)) {
        pts.Count=pbase;
        return false;
      }
      return true;
    }   

    public bool Hexahedron(double[] center,double[] scale,POINTS mode) {
      int pbase=pts.Count;
      if(!pts.Hexahedron(center,scale)
          ||!fcs.Hexahedron(pbase,mode)) {
        pts.Count=pbase;
        return false;
      }
      return true;
    }
    public bool Octahedron(double[] center,double[] scale,POINTS mode) {
      int pbase=pts.Count;
      if(!pts.Octahedron(center,scale)
          ||!fcs.Octahedron(pbase,mode)) {
        pts.Count=pbase;
        return false;
      }
      return true;
    }
    public bool Dodecahedron(double[] center,double[] scale,POINTS mode) {
      int pbase=pts.Count;
      if(!pts.Dodecahedron(center,scale)
          ||!fcs.Dodecahedron(pbase,mode)) {
        pts.Count=pbase;
        return false;
      }
      return true;
    }
    public bool Icosahedron(double[] center,double[] scale,POINTS mode) {
      int pbase=pts.Count;
      if(!pts.Icosahedron(center,scale)
          ||!fcs.Icosahedron(pbase,mode)) {
        pts.Count=pbase;
        return false;
      }
      return true;
    }
    
    struct RelaxPt {
      internal double x,y,z;
      internal int n;
    }
    public void Relax(double weight,double power,int iteration) {
      RelaxPt[] rp=new RelaxPt[pts.Count];
      if(iteration<1) iteration=1;
      double[] rd3=d3.V();
      double rd;
      power--;
      while(iteration-->0) {
        for(int f=0;f<fcs.N;f++) {
          int i,j,n=fcs.f[f++];
          for(j=fcs.f[f+n-1];n>0;n--) {
            i=fcs.f[f++];
            d3.sub(rd3,0,pts.p,3*i,pts.p,3*j);
            rd=d3.radius(rd3);
            rd=rd<1e-20||power==0?1:Math.Pow(rd,power);
            rp[j].x+=rd*rd3[0];rp[i].x-=rd*rd3[0];
            rp[j].y+=rd*rd3[1];rp[i].y-=rd*rd3[1];
            rp[j].z+=rd*rd3[2];rp[i].z-=rd*rd3[2];
            rp[j].n++;rp[i].n++;
            j=i;
          }
        }
        for(int i=0;i<pts.Count;i++) {
          if(rp[i].n>0) {
            int n=rp[i].n;
            rd3[0]=rp[i].x/n;
            rd3[1]=rp[i].y/n;
            rd3[2]=rp[i].z/n;
            rd=d3.radius(rd3);
            rd=rd<1e-20||power==0?1:Math.Pow(rd,power);
            int i3=i*3;
            pts.p[i3++]+=weight*rd*rd3[0];
            pts.p[i3++]+=weight*rd*rd3[1];
            pts.p[i3++]+=weight*rd*rd3[2];
            rp[i].x=rp[i].y=rp[i].z=0;
            rp[i].n=0;
          }
        }
      }      
    }
    
    internal struct Edge {
      internal int from,to,middle;
    }
    
    internal class EdgeComparer:IComparer<Edge> {
      public int Compare(Edge a,Edge b) {
        int r=a.from-b.from;
        if(r==0) r=a.to-b.to;
        return r;
      }
    }
    
    public void Precise(bool faces) { // edge/faces division
      int e,e2,ee,en=faces?fcs.Edges():3*fcs.Triangles(),nf=0;
      // edge array
      Edge[] et=new Edge[en];
      e=0;
      for(int h=0;h<fcs.N;h+=fcs.A) {
        nf++;
        int n=fcs.f[h++];
        if(faces) {
          int a=fcs.f[h+n-1];
          for(int i=0;i<n;i++) {
            int c=a;
            a=fcs.f[h++];
            if(c<a) {et[e].from=c;et[e].to=a;} else {et[e].from=a;et[e].to=c;};
            e++;            
          }
        } else {
          int a=fcs.f[h++],c=fcs.f[h++];
          for(int i=2;i<n;i++) {
            int b=c;
            c=fcs.f[h++];
            if(a<b) {et[e].from=a;et[e].to=b;} else {et[e].from=b;et[e].to=a;};
            e++;
            if(b<c) {et[e].from=b;et[e].to=c;} else {et[e].from=c;et[e].to=b;};
            e++;
            if(c<a) {et[e].from=c;et[e].to=a;} else {et[e].from=a;et[e].to=c;};
            e++;
          }  
        }        
      }
      // unique edges
      Array.Sort(et,new EdgeComparer());
      for(e2=e=0,ee=et.Length;e<ee;e2++) {
        if(e2<e) et[e2]=et[e];
        for(e++;e<ee&&et[e].from==et[e2].from&&et[e].to==et[e2].to;e++);        
      }
      // point index cache
      int[] ps=new int[pts.Count];
      int j=0;
      for(j=0;j<ps.Length;ps[j++]=-1);
      j=-1;
      for(e=0;e<e2;e++)
        if(et[e].from!=j) {
          j=et[e].from;
          ps[j]=e;
        }
      // add middle points
      int pi=pts.Count;
      pts.append_space(e2);
      for(e=0;e<e2;e++) {
        et[e].middle=pi++;
        int pf=3*et[e].from,pt=3*et[e].to,pm=3*et[e].middle;
        pts.p[pm++]=(pts.p[pf++]+pts.p[pt++])/2;
        pts.p[pm++]=(pts.p[pf++]+pts.p[pt++])/2;
        pts.p[pm++]=(pts.p[pf++]+pts.p[pt++])/2;
      }
      // replace big faces with smallers      
      en=faces?en*(1+4+fcs.A):en/3*4*(1+3+fcs.A);
      int[] fcs2=fcs.f;
      fcs.f=new int[en];
      int g=0,f2n=fcs.N;
      fcs.N=en;
      if(faces) {
        int b=pts.Count,pc=3*b;
        pts.append_space(nf);
        for(int h=0;h<f2n;h+=fcs.A,pc+=3,b++) {
          int f,n=fcs2[h++],ha=h+n;
          d3.copy(pts.p,pc,d3.V0,0);
          int a=fcs2[ha-1],d=fcs2[ha-2];
          for(int i=0;i<n;i++) {
            int c=d;
            d=a;a=fcs2[h++];
            if(a<d) {e=ps[a];f=d;} else {e=ps[d];f=a;}
            while(et[e].to!=f) e++;
            int ad=et[e].middle;
            if(c<d) {e=ps[c];f=d;} else {e=ps[d];f=c;}
            while(et[e].to!=f) e++;
            int cd=et[e].middle;            
            
            fcs.f[g++]=4;fcs.f[g++]=ad;fcs.f[g++]=b;fcs.f[g++]=cd;fcs.f[g++]=d;
            for(j=0;j<fcs.A;j++) fcs.f[g++]=fcs2[ha+j];

            d3.add(pts.p,pc,pts.p,pc,pts.p,3*a);            
          }
          d3.mul_d(pts.p,pc,pts.p,pc,1.0/n);
        }
      } else {
        for(int h=0;h<f2n;h+=fcs.A) {
          int f,n=fcs2[h++],ha=h+n;
          int a=fcs2[h++];
          int c=fcs2[h++];
          for(int i=2;i<n;i++) {
            int b=c;
            c=fcs2[h++];
            if(a<b) {e=ps[a];f=b;} else {e=ps[b];f=a;}
            while(et[e].to!=f) e++;
            int ab=et[e].middle;
            if(b<c) {e=ps[b];f=c;} else {e=ps[c];f=b;}
            while(et[e].to!=f) e++;
            int bc=et[e].middle;
            if(c<a) {e=ps[c];f=a;} else {e=ps[a];f=c;}
            while(et[e].to!=f) e++;
            int ca=et[e].middle;
          
            fcs.f[g++]=3;fcs.f[g++]=a;fcs.f[g++]=ab;fcs.f[g++]=ca;
            for(j=0;j<fcs.A;j++) fcs.f[g++]=fcs2[ha+j];
            fcs.f[g++]=3;fcs.f[g++]=ab;fcs.f[g++]=bc;fcs.f[g++]=ca;
            for(j=0;j<fcs.A;j++) fcs.f[g++]=fcs2[ha+j];
            fcs.f[g++]=3;fcs.f[g++]=ab;fcs.f[g++]=b;fcs.f[g++]=bc;
            for(j=0;j<fcs.A;j++) fcs.f[g++]=fcs2[ha+j];
            fcs.f[g++]=3;fcs.f[g++]=bc;fcs.f[g++]=c;fcs.f[g++]=ca;
            for(j=0;j<fcs.A;j++) fcs.f[g++]=fcs2[ha+j];
          }
        }  
      }
    }
    
    
    public void glDraw(double[] colors,bool hide) {
      opengl.glDisableClientState(GLEnum.GL_NORMAL_ARRAY);
      opengl.glDisableClientState(GLEnum.GL_COLOR_ARRAY);
      opengl.glEnableClientState(GLEnum.GL_VERTEX_ARRAY);
      GCHandle gch=pts.Handle();
      opengl.glVertexPointer(3,GLEnum.GL_DOUBLE,0,gch.AddrOfPinnedObject());
      fcs.glDraw(colors,hide);
      gch.Free();   
    }         
    
    public static int Phong(int ambient,double i01,int diffuse,double j01,int specular,double shine,int light) {
      double sf1=Math.Pow(j01,shine),x;
      x=((ambient&255)*255+(i01*(diffuse&255)+sf1*(specular&255))*(light&255))/255.0;
      byte r=x<255?(byte)x:(byte)255;
      x=(((ambient>>8)&255)*255+(i01*((diffuse>>8)&255)+sf1*((specular>>8)&255))*((light>>8)&255))/255.0;
      byte g=x<255?(byte)x:(byte)255;
      x=(((ambient>>16)&255)*255+(i01*((diffuse>>16)&255)+sf1*((specular>>16)&255))*((light>>16)&255))/255.0;
      byte b=x<255?(byte)x:(byte)255;
      return r|(g<<8)|(b<<16);
    }
    
    public void ColorizePhong(int ambient,int diffuse,int specular,double shine,int light,double[] light_vec) {
      double[] d1=new double[3],d2=new double[3],x=new double[3];
      for(int h=0;h<fcs.N;h+=1+fcs.f[h]+fcs.A) {
        int c=fcs.f[h];
        if(faces.Hidden(fcs.f[h+c+1])) continue;
        d3.sub(d1,0,pts.p,3*fcs.f[h+2],pts.p,3*fcs.f[h+1]);
        d3.sub(d2,0,pts.p,3*fcs.f[h+3],pts.p,3*fcs.f[h+1]);
        d3.cross(x,d1,d2);
        double f=1-d3.angle(x,light_vec)/Math.PI;
        faces.Color(ref fcs.f[h+c+1],Phong(ambient,f,diffuse,f,specular,shine,light));
      }  
    }
    
    public void ColorizeNormalRGB(double[] rota) { 
      double[] normal=new double[3];
      for(int h=0;h<fcs.N;h+=1+fcs.f[h]+fcs.A) {
        int c=fcs.f[h];
        if(faces.Hidden(fcs.f[h+c+1])) continue;
        FaceNormal(h,normal);
        d3.div_d33(normal,normal,rota);
        d3.mul_d(normal,normal,-1);
        d3.mul_d(normal,d3.add_d(normal,normal,1),0.5);
        faces.Color(ref fcs.f[h+c+1],faces.RGB2int(normal,0));
      }    
    }
    
    public void ColorizeNormalRadial(double[] rota,double[] palette) {
      double[] normal=new double[3];
      int pl=palette.Length/4;
      for(int h=0;h<fcs.N;h+=1+fcs.f[h]+fcs.A) {
        int c=fcs.f[h];
        if(faces.Hidden(fcs.f[h+c+1])) continue;
        FaceNormal(h,normal);                
        d3.div_d33(normal,normal,rota);
        double d=Math.Atan2(normal[0],normal[2]);
        d=Math.Abs(d)/Math.PI;
        faces.Color(ref fcs.f[h+c+1],faces.PaletteColor(pl,palette,d));
      }
    }
    public void ColorizeGradient(double[] v,double[] palette) {
      double[] normal=new double[3];
      int pl=palette.Length/4;
      double[] fx=FaceCenters(v);
      double min=fx[0],max=min;
      foreach(double d in fx) 
        if(d<min) min=d;else if(d>max) max=d;
      ColorizeMinMax(fx,1,min,max,false,palette);
    }

    public void ColorizeSphere(double[] p,double[] palette) {      
      double[] fx=FaceCenters(null);
      int n=fx.Length/3;
      if(n<2) return;      
      int pl=palette.Length/4;
      double min=d3.distance(fx,0,p),max=min;
      fx[0]=min;
      for(int i=3;i<fx.Length;i+=3) {
        double d=d3.distance(fx,i,p);
        fx[i]=d;
        if(d<min) min=d;else if(d>max) max=d;
      }
      ColorizeMinMax(fx,3,min,max,true,palette);
    }

    public void ColorizeCylinder(double[] center,double[] normal,double[] palette) {      
      double[] fx=FaceCenters(null);
      int n=fx.Length/3;
      if(n<2) return;                  
      double min=d3.line_distance(center,normal,fx,0),max=min;
      fx[0]=min;
      for(int i=3;i<fx.Length;i+=3) {
        double d=d3.line_distance(center,normal,fx,i);
        fx[i]=d;
        if(d<min) min=d;else if(d>max) max=d;
      }
      ColorizeMinMax(fx,3,min,max,false,palette);
    }
    public void ColorizeMinMax(double[] fx,int stride,double min,double max,bool rev,double[] palette) {      
      if(min==max) return;
      int pl=palette.Length/4;
      int f=0;
      for(int h=0;h<fcs.N;h+=1+fcs.f[h]+fcs.A,f+=stride) {
        int c=fcs.f[h];
        if(faces.Hidden(fcs.f[h+c+1])) continue;
        double x=rev?(max-fx[f])/(max-min):(fx[f]-min)/(max-min);
        faces.Color(ref fcs.f[h+c+1],faces.PaletteColor(pl,palette,x));
      }
    }
    public void ColorizeRadial(double[] center,double[] rota,double[] palette) {      
      double[] fx=FaceCenters(null);
      int n=fx.Length/3;
      if(n<2) return;                  
      double[] rd3=d3.V();
      for(int i=0;i<fx.Length;i+=3) {
        d3.sub(rd3,0,fx,i,center,0);
        d3.div_d33(rd3,rd3,rota);
        double d=Math.Atan2(rd3[1],rd3[0]);
        fx[i]=Math.Abs(d)/Math.PI;
      }
      ColorizeMinMax(fx,3,0,1,false,palette);
    }

    public void FilterBack(int n,int m,bool sub) {
      fcs.Hide(n,m,sub);      
    }
    public void FilterBack(double[] x,bool inv) {
      bool[] hide=new bool[fcs.Faces()];
      double[] normal=d3.V();
      for(int h=0,i=0;h<fcs.N;h+=1+fcs.f[h]+fcs.A,i++) {
        FaceNormal(h,normal);
        double s=d3.scalar(normal,x);
        if(inv?s<0:s>0) hide[i]=true;
      }      
      fcs.Hide(hide,2);
    }

    public double[] FaceCenters(double[] v) {
      int n=fcs.Faces();
      bool scalar=v!=null;
      double[] res=new double[(scalar?1:3)*n],x3=d3.V();
      for(int h=0,i=0;h<fcs.N;h+=1+fcs.f[h]+fcs.A) {
        FaceCenter(h,x3);
        if(scalar) res[i++]=x3[0]*v[0]+x3[1]*v[1]+x3[2]*v[2];
        else {res[i++]=x3[0];res[i++]=x3[1];res[i++]=x3[2];}
      }
      return res;
    }

    public void FilterLevels(double[] v,int levels,bool inv) {
      double[] fl=FaceCenters(v);
      if(fl.Length>0) {
        double mi=fl[0],ma=fl[0];
        foreach(double d in fl) if(d<mi) mi=d;else if(d>ma) ma=d;
        if(ma>mi) {
          bool[] hide=new bool[fl.Length];
          for(int i=0;i<fl.Length;i++) {
            int l=(int)((fl[i]-mi)*levels/(ma-mi));
            hide[i]=inv^((l&1)==1&&l<levels);
          }                
          fcs.Hide(hide,2);
        }
      }
    }

    public void FilterPlane(double[] x,double level,bool inv) {
      double[] fl=FaceCenters(x);
      if(fl.Length>0) {
        bool[] hide=new bool[fl.Length];
        for(int i=0;i<fl.Length;i++) {
          hide[i]=inv^(fl[i]>=level);
        }
        fcs.Hide(hide,2);
      }
    }


    
    // cut plane support
    internal struct inter {
      internal int i,o; // point inside,point outside
      internal int idx; // index of intersection point
      
      internal inter(int i,int o,int idx) {
        this.i=i;this.o=o;this.idx=idx;
      }
    }; // intersections

    internal class intersections {
      internal mesh m;
      internal double a;
      internal double[] normal=new double[3]; 
      internal int idx; // current idx for point
      internal List<inter> inter=new List<inter>(); // table of intersections      
      
      public intersections(mesh m,double[] normal,double a) {
        this.m=m;idx=m.pts.Count;
        d3.copy(this.normal,normal);
        this.a=a;
      }
      
      public int Point(int i,int o) {
        for(int h=0;h<inter.Count;h++)
          if(inter[h].i==i&&inter[h].o==o)
            return inter[h].idx;
        inter.Add(new inter(i,o,idx++));    
        return idx-1;
      }
      
      public void Points() {
        double[] p=m.pts.p,r3=new double[3];
        for(int h=0;h<inter.Count;h++) {
          int p1=inter[h].i*3,p2=inter[h].o*3;
          double f1=p[p1]*normal[0]+p[p1+1]*normal[1]+p[p1+2]*normal[2]+a;
          double f2=p[p2]*normal[0]+p[p2+1]*normal[1]+p[p2+2]*normal[2]+a;
          double w=f1/(f1-f2);
          d3.linear_interpol(r3,0,w,p,p1,p,p2);
          m.pts.Add(r3);          
        }
      }
    };

    public void CutPlane(double[] normal,double a) {
      faces nfcs=new faces(32);
      intersections ins=new intersections(this,normal,a);
      double[] p=pts.p;
      int i,i2,n,n2,pi,g,g2;
      for(int h=0;h<fcs.N;h+=fcs.f[h-1]+fcs.A) {
        n=fcs.f[h++];
        n2=0;
        for(i=0;i<n;i++) {
          pi=3*fcs.f[h+i];
          if(normal[0]*p[pi]+normal[1]*p[pi+1]+normal[2]*p[pi+2]+a>=0) n2++;          
        }
        if(n2==n) goto copy;
        if(n2==0) goto skip;
        i2=fcs.f[h+n-1];
        pi=3*i2;
        bool pf2=normal[0]*p[pi]+normal[1]*p[pi+1]+normal[2]*p[pi+2]+a>=0;
        nfcs.append_space(nfcs.N+1+(n*3/2+1)+nfcs.A,true);
        // table_realloc??
        g2=g=nfcs.N;
        g++;
        for(i=0;i<n;i++) {
          int i1=fcs.f[h+i];
          pi=3*i1;
          bool pf1=normal[0]*p[pi]+normal[1]*p[pi+1]+normal[2]*p[pi+2]+a>=0;
          if(pf1) {
            if(!pf2) nfcs.f[g++]=ins.Point(i1,i2);
            nfcs.f[g++]=i1;
          } else if(pf2) {
            nfcs.f[g++]=ins.Point(i2,i1);
          }
          i2=i1;
          pf2=pf1;
        }
        nfcs.f[g2]=g-g2-1;
        for(i=0;i<fcs.A;i++)
          nfcs.f[g++]=fcs.f[h+i+n];
        nfcs.N=g;    
/*
   // cut some parts of polygon
   i2=h[n-1];
   p=(double*)(pb+pa*i2);
   pf2=normal[0]*p[0]+normal[1]*p[1]+normal[2]*p[2]+a>=0;
   table_realloc((table*)&fcs,fcs.n+1+(n*3/2+1)+fa);
   g2=g=(int*)fcs.r+fcs.n;
   g++;//count
   for(i=0;i<n;i++) {
     i1=h[i];
     p=(double*)(pb+pa*i1);
     pf1=normal[0]*p[0]+normal[1]*p[1]+normal[2]*p[2]+a>=0;
     if(pf1) {
       if(!pf2) *g++=IntersectionsPoint(ins,i1,i2);
       *g++=i1;
     } else if(pf2) {
       *g++=IntersectionsPoint(ins,i2,i1);
     }
     i2=i1;
     pf2=pf1;
   }
   *g2=g-g2-1;
   for(i=0;i<fa;i++)
     *g++=h[i+n];
   fcs.n+=1+*g2+fa;
*/    
        continue;
       copy:
        nfcs.append(fcs.f,h-1,1+n+fcs.A);
       skip:;
      }
      fcs.Clear();
      fcs.Append(nfcs,0);
      n2=pts.Count;
      ins.Points();
      p=pts.p;
      n=pts.Count;
      g=0;
      int[] ra=new int[n];
      for(i2=i=0;i<n2;i++) {
        pi=3*i;
        if(normal[0]*p[pi]+normal[1]*p[pi+1]+normal[2]*p[pi+2]+a>=0) {
          p[3*i2]=p[3*i];p[3*i2+1]=p[3*i+1];p[3*i2+2]=p[3*i+2];          
          ra[g]=i2++;
        } else
          ra[g]=-1;
        g++;
      }
      Array.Copy(p,3*i,p,3*i2,(n-n2)*3);
      while(i<n) {
        ra[g++]=i2++;
        i++;
      }
      pts.Count=i2;
      for(int h=0;h<fcs.N;h+=fcs.A) 
        for(i=fcs.f[h++];i>0;i--) {
          fcs.f[h]=ra[fcs.f[h]];
          h++;
        }  
    }
    public void Permute(string code,bool back) {
      if(""+code=="") return;
      int px=0,py=0;
      bool nx=false,ny=nx,nz=ny,rev=nx;
      int pos=0;
      foreach(char ch in code) {
        int i;
        if(ch=='r') {rev=true;continue;}
        else if(ch=='x'||ch=='X') i=0;
        else if(ch=='y'||ch=='Y') i=1;
        else if(ch=='z'||ch=='Z') i=2;
        else continue;
        if(pos==0) {px=i;nx=char.IsUpper(ch);pos++;}
        else if(pos==1) {py=i;ny=char.IsUpper(ch);pos++;}
        else nz=char.IsUpper(ch);
      }
      if(pos==0) {py=1;}
      else if(pos==1) {py=px==1?0:1;}
      else if(px==py) py=(px+1)%3;
      if(px==0&&py==1&&!nx&&!ny&&!nz) return;
      Permute(px,py,nx,ny,nz,rev,back);
    }
    public void Permute(int px,int py,bool nx,bool ny,bool nz,bool reverse,bool back) {
      pts.Permute(px,py,nx,ny,nz,back);
      if(reverse) fcs.Reverse();
    }
    public void Mirror(bool x,bool y,bool z,bool reverse) {
      pts.Mirror(x,y,z);
      if(reverse) fcs.Reverse();      
    }
    public void Rotate(int axis,bool back,bool reverse) {
      pts.Rotate(axis,back);
      if(reverse) fcs.Reverse();      
    }
  }
   internal class PDFCompress {
    byte[] bit=new byte[4];
    char[] cha=new char[5];
    int bits;
    TextWriter ts;
    bool compress;
    const int ClearMark=256,EOD=257;
    
    Dictionary<string,int> dict=new Dictionary<string,int>();
    int blen,idx;
    string seq="";
    
    public void Clear() {
      dict.Clear();      
      blen=9;
      seq="";idx=258;
    }
    public PDFCompress(TextWriter ts,bool compress) {
      this.ts=ts;this.compress=compress;
      Clear();
      if(compress) WriteBitsMSB(ClearMark,blen);
    }
    public void WriteLine(string s) {
      if(!compress)
        ts.WriteLine(s);
      else {        
        Write(s);
        Write('\r');Write('\n');
      }
    }
    public void Write(string s) {
      if(string.IsNullOrEmpty(s)) return;
      if(!compress)
        ts.Write(s);
      else foreach(char ch in s)
        Write(ch);
    }
    public void Write(char ch) {
      if(!compress) {
        ts.Write(ch);
        return;
      }  
      string seq2=seq+ch;
      if(seq==""||dict.ContainsKey(seq2)) {
        seq=seq2;
        return;
      }
      OutputSeq();
      dict[seq2]=idx;
      idx++;
      if(0!=(idx&(1<<blen))) {
        if(blen==12) {
          WriteBitsMSB(ClearMark,blen);
          Clear();
        } else blen++;
      }
      seq=""+ch;
    }
    void OutputSeq() {
      int k;
      if(seq.Length==1) {
        WriteBitsMSB(seq[0],blen);
      } else if(seq.Length>1) {
        k=dict[seq];
        WriteBitsMSB((uint)k,blen);
      }    
    }
    public void WriteBitsMSB(uint b,int n) {
      if(n<1) return;
      if(n>32) n=32;      
      while(n>0) {
        int l=8-(bits&7);
        if(l>n) l=n;
        int k=n-l;
        byte b2=(byte)(b>>k);
        b2&=(byte)((1<<l)-1);
        bit[bits>>3]|=(byte)((b2<<(8-(bits&7)-l)));
        bits+=l;
        if(bits==32)
          Output();
        n-=l;
      }
    }
    void Ascii85() {
      uint b=(uint)((((((bit[0]<<8)|bit[1])<<8)|bit[2])<<8)|bit[3]);
      cha[4]=(char)(33+b%85);b/=85;
      cha[3]=(char)(33+b%85);b/=85;
      cha[2]=(char)(33+b%85);b/=85;
      cha[1]=(char)(33+b%85);b/=85;
      cha[0]=(char)(33+b%85);
    }
    public void Output() {
      bits=0;
      if(bit[0]==0&&bit[1]==0&&bit[2]==0&&bit[3]==0) {
        ts.Write('z');
        return;
      }      
      Ascii85();
      bit[0]=bit[1]=bit[2]=bit[3]=0;
      ts.Write(cha);
    }
    
    void AsciiFlush() {
      int bytes=(bits+7)>>3;
      if(bytes>0) {
        Ascii85();
        ts.Write(cha,0,bytes+1);
      }
      ts.Write("~>");
    }
    
    public void Flush() {
      if(!compress) return;
      OutputSeq();
      WriteBitsMSB(EOD,blen);
      AsciiFlush();
    }
  }    
}
