using System;
using System.Runtime.InteropServices;
using System.Collections;
using System.IO;
using Math3d;

namespace Mesh3d {
  [Flags]
  public enum POINTS {None
    ,CYCLE=1   // points forms cycle
    ,CYCLE2=2  // points form second axis cycle
    ,REVERSE=4 // points have reverse order
    ,CAP=8      // first point is cap
    ,CAP2=16    // last point is cap
    ,IMPLICIT_CAP=32 // add point with height of first point as bottom cap
    ,IMPLICIT_CAP2=64 // add point with height of last point as top cap
  }
  public class points {
    public double[] p;
    int N,M;  // actual size and maximal size in 3d vectors
    public int Count {get {return N;}set{if(value>=0&&value<N) N=value;}}
    public points(int capacity) {
      p=new double[3*(M=capacity)];      
    }
    public points(points pts) {
      N=pts.N;M=pts.M;
      p=pts.p.Clone() as double[];
    }
    public void Alloc(int nm) {
      if(nm<=M) return;      
      Array.Resize(ref p,3*(M=nm));
    }
    public bool append_space(int s) {
      while(N+s>M) Alloc(2*M);
      N+=s;
      return true;
    }
    public void Append(points pts) {
      int n2=N;
      append_space(pts.N);
      Array.Copy(pts.p,0,p,3*n2,3*pts.N);
    }
    public GCHandle Handle() {
      return GCHandle.Alloc(p,GCHandleType.Pinned);
    }
    public void Clear() { N=0;}
    public void Add(double[] d3) {
      if(d3!=null&&d3.Length>=3) Add(d3[0],d3[1],d3[2]);
    }  
    public void Add(double x,double y,double z) {
      if(N==M) Alloc(2*M);
      int h=3*N;
      p[h++]=x;p[h++]=y;p[h++]=z;
      N++;
    }
    public void MinMax(double[] min,double[] max) {
      nd3.MinMax(N,p,min,max);
    }
    public void Permute(int px,int py,bool nx,bool ny,bool nz,bool back) {
      nd3.Permute(N,p,px,py,nx,ny,nz,back);
    }
    public void Mirror(bool x,bool y,bool z) {
      if(!x&&!y&&!z) return;
      double[] min=d3.V(0),max=d3.V(0);
      nd3.MinMax(N,p,min,max);
      d3.add(min,max);
      for(int i=0,ie=3*N;i<ie;i+=3) {
        if(x) p[i]=min[0]-p[i];
        if(y) p[i+1]=min[1]-p[i+1];
        if(z) p[i+2]=min[2]-p[i+2];
      }
    }
    public void Rotate(int axis,bool back) {
      double[] min=d3.V(0),max=d3.V(0);
      nd3.MinMax(N,p,min,max);
      d3.add(min,max);
      d3.mul_d(min,min,0.5);
      int ix=0,iy=1,iz=2;
      double nx=1,ny=1,nz=1;
      if(axis==1) { ix=2;iz=0;if(back) nx=-1;else nz=-1;}
      else if(axis==2) { ix=1;iy=0;if(back) nx=-1;else ny=-1;}
      else { iy=2;iz=1;if(back) ny=-1;else nz=-1;}
      bool bx=nx!=1||ix!=0,by=ny!=1||iy!=1,bz=nz!=1||iz!=2;
      for(int i=0,ie=3*N;i<ie;i+=3) {
        double px,py,pz;
        px=p[i+ix];
        if(bx) px=(nx)*(px-min[ix])+min[0];
        py=p[i+iy];
        if(by) py=(ny)*(py-min[iy])+min[1];
        pz=p[i+iz];
        if(bz) pz=(nz)*(pz-min[iz])+min[2];
        p[i]=px;p[i+1]=py;p[i+2]=pz;
      }
    }
    public void Center(double[] center) {
      double[] min=d3.V(),max=d3.V();
      nd3.MinMax(N,p,min,max);
      d3.linear_interpol(center,0.5,min,max);
    }
    public void Resize(double[] size) { Resize(d3.V0,size);}
    public void Resize(double[] lo,double[] hi) {
      double[] min=d3.V(),max=d3.V();
      nd3.MinMax(N,p,min,max);
      double[] mm=new double[3],hl=new double[3];
      d3.sub(mm,max,min);d3.sub(hl,hi,lo);
      if(mm[0]==0||mm[1]==0||mm[2]==0) return;
      nd3.mul_d3(N,p,min,hl,mm,lo);
    }
    static void Byte3(ref int af,int idxmask,int mask,int value) {
      if(0!=(idxmask&1)) af=(af&~mask)|value;
      if(0!=(idxmask&2)) af=(af&~(mask<<8))|(value<<8);
      if(0!=(idxmask&4)) af=(af&~(mask<<16))|(value<<16);
    }
    static int Byte3(int af,int mask,int value,bool count) {
      int x=af&255,y=(af>>8)&255,z=(af>>16)&255;
      if(mask!=0) {x&=mask;y&=mask;z&=mask;}
      bool d=x==value,e=y==value,f=z==value;
      return count?(d?1:0)+(e?1:0)+(f?1:0):(d?1:0)|(e?2:0)|(f?4:0);
    }
    public void Resize(string flags,double[] premul,double[] lo,double[] hi,out double[] mul,out double[] add) {      
      int fx=0,idx=7;bool bmax=false;
      flags+="";
      foreach(char ch in flags)
        switch(ch) {
         case 'x':idx=1;break;
         case 'y':idx=2;break;
         case 'z':idx=4;break;
         case 'c':Byte3(ref fx,idx,3,0);break;
         case 'l':Byte3(ref fx,idx,3,1);break;
         case 'r':case 'u':
         case 'h':Byte3(ref fx,idx,3,2);break;
         case 's':Byte3(ref fx,idx,15,3+8);break;
         case 'e':bmax=true;break;         
         case 'm':Byte3(ref fx,7,12,4);Byte3(ref fx,idx,15,3+8);break;
         case 'n':Byte3(ref fx,7,15,3+8);Byte3(ref fx,idx,12,4);break;
        }
      double[] min=d3.V(),max=d3.V();      
      nd3.MinMax(N,p,premul,min,max);
      double[] mm=new double[3],hl=new double[3];
      int neg=d3.gt(lo,hi);
      d3.swap(lo,hi,neg);
      if(premul!=null) neg^=d3.abs(d3.V(premul));
      d3.sub(mm,max,min);d3.sub(hl,hi,lo);      
      mul=add=null;
      if(mm[0]==0||mm[1]==0||mm[2]==0) return;
      mul=d3.V();add=d3.V();
      d3.div(mul,hl,mm);
			if(Byte3(fx,3,3,true)!=3) {
			  double mz=double.NaN;
        int gmax=Byte3(fx,4,0,false),smax=Byte3(fx,8,0,false);
        if(gmax!=0&&smax!=0) {
          if(0!=(gmax&1)) mz=mul[0];
          if(0!=(gmax&2)&&(double.IsNaN(mz)||((mul[1]<mz)^bmax))) mz=mul[1];
          if(0!=(gmax&4)&&(double.IsNaN(mz)||((mul[2]<mz)^bmax))) mz=mul[2];
          if(0!=(smax&1)) mul[0]=mz;
          if(0!=(smax&2)) mul[1]=mz;
          if(0!=(smax&4)) mul[2]=mz;
        }
      }      
      for(int i=0;i<3;i++) {
        double mul2=mul[i],diff=(hl[i]-mm[i]*mul2);
        bool negi=0!=(neg&(1<<i));
        if(premul!=null) mul[i]*=premul[i];
        if(negi) {mul[i]*=-1;mul2*=-1;}
        switch((fx>>(8*i))&3) {
         case 0:add[i]+=diff/2;break;
         case 1:add[i]+=0;break;
         case 2:add[i]+=diff;break;
         default:add[i]=0;break;
        }
        add[i]=lo[i]+add[i]-(negi?max[i]:min[i])*mul2;
			}        
    }
    public void Translate(double[] v) {
      nd3.add_d3(N,p,v);
    }
    public void Rotate(double[] m) {
      nd3.mul_d33(N,p,m);
    }
    public void Scale(double[] v) {
      nd3.mul_d3(N,p,v);
    }    
    public void Write(StreamWriter sw,string format,double[] mul,double[] add) {
      for(int i=0;i<N;i++)
        sw.Write(d3.ToString(p,3*i,format,mul,add));
    }
    int D3Circle(double[] d,int v,double[] center,double[] scale,int radials,int axes) {
      int i;
      if(axes==0) axes=0x012;
      int a2=axes&3,a1=(axes>>4)&3,a0=(axes>>8)&3;
      double fi,c0=center[a0],c1=center[a1],c2=center[a2],r0=scale[a0],r2=scale[a2];

      for(i=0;i<radials;i++) {
        fi=2*System.Math.PI*i/(radials);
        d[v+a0]=c0+r0*System.Math.Cos(fi);
        d[v+a1]=c1;
        d[v+a2]=c2+r2*System.Math.Sin(fi);
        v+=3;
      }
      return v;
    }
    
// primitives
    public bool Sphere(double[] center,double[] rota,double[] radius,int layers,int halfcircles,short axes) {
      int v;
      int i,j;

      if(layers<2) layers=2;
      if(halfcircles<3) halfcircles=3;

      v=3*N;
      if(!append_space(2+(layers-2)*halfcircles))
        return false;

      if(axes==0) axes=0x012;
      int a2=axes&3,a1=(axes>>4)&3,a0=(axes>>8)&3;

      p[v+0]=center[0]-rota[3*a1+0]*radius[a1];
      p[v+1]=center[1]-rota[3*a1+1]*radius[a1];
      p[v+2]=center[2]-rota[3*a1+2]*radius[a1];
      v+=3;

      for(i=1;i<layers-1;i++) {
        double phi=System.Math.PI*i/(layers-1);
        double y=-radius[a1]*System.Math.Cos(phi);
        double r0=radius[a0]*System.Math.Sin(phi),r2=radius[a2]*System.Math.Sin(phi);
        for(j=0;j<halfcircles;j++) {
          double fi=2*System.Math.PI*j/halfcircles;
          double x=r0*System.Math.Cos(fi),z=r2*System.Math.Sin(fi);
       
          p[v+0]=center[0]+rota[3*a0+0]*x+rota[3*a1+0]*y+rota[3*a2+0]*z;
          p[v+1]=center[1]+rota[3*a0+1]*x+rota[3*a1+1]*y+rota[3*a2+1]*z;
          p[v+2]=center[2]+rota[3*a0+2]*x+rota[3*a1+2]*y+rota[3*a2+2]*z;
          v+=3;
         //printf("%f %f %f\n",center[0],r0,cos(fi));
        }
      }

      p[v+0]=center[0]+rota[3*a1+0]*radius[a1];
      p[v+1]=center[1]+rota[3*a1+1]*radius[a1];
      p[v+2]=center[2]+rota[3*a1+2]*radius[a1];  
      return true;
    }
// cylinder in y-axis
    public bool Cylinder(double[] center,double[] scale,int layers,int circles,int radials,short axes) {
      int v;
      double v1,r1;
      double[] center2=new double[3],scale2=new double[3];
      int i;

      if(circles<1||layers<2||radials<3)
        return false;

      v=3*N;
      if(!append_space(2+(layers+2*circles-2)*radials))
        return false;

      if(axes==0) axes=0x012;
      int a2=axes&3,a1=(axes>>4)&3,a0=(axes>>8)&3;

      d3.copy(center2,center);
      d3.copy(scale2,scale);
      v1=center[a1]-scale[a1];
      p[v+a0]=center[a0];p[v+a1]=v1;p[v+a2]=center[a2];
      v+=3;

      for(i=1;i<circles;i++) {
        r1=(double)i/circles;
        center2[a1]=v1;
        scale2[a0]=r1*scale[a0];scale2[a2]=r1*scale[a2];
        v=D3Circle(p,v,center2,scale2,radials,axes);
      }

      for(i=0;i<=layers-1;i++) {
        v1=center[a1]+scale[a1]*((double)i/(layers-1)*2-1);
        center2[a1]=v1;
        v=D3Circle(p,v,center2,scale,radials,axes);
      }


      v1=center[a1]+scale[a1];

      for(i=circles-1;i>0;i--) {
        r1=(double)i/circles;
        center2[a1]=v1;
        scale2[a0]=r1*scale[a0];scale2[a2]=r1*scale[a2];
        v=D3Circle(p,v,center2,scale2,radials,axes);
      }

      p[v+a0]=center[a0];p[v+a1]=v1;p[v+a2]=center[a2];  
      return true;  
    }
// cone in y-axis
    public bool Cone(double[] center,double[] scale,int layers,int circles,int radials,int axes) {
      int v;
      double v1,r1;
      double[] center2=new double[3],scale2=new double[3];
      int i;

      if(layers<2) layers=2;
      if(circles<1) circles=1;
      if(radials<3) radials=3;

      v=3*N;
      if(!append_space(2+(layers+circles-2)*radials))
        return false;

      if(axes==0) axes=0x012;
      int a2=axes&3,a1=(axes>>4)&3,a0=(axes>>8)&3;
      int n1=0!=(axes&0x2000)?-1:1;

      d3.copy(center2,center);
      v1=center[a1]-n1*scale[a1];
      p[v+a0]=center[a0];p[v+a1]=v1;p[v+a2]=center[a2];
      v+=3;

      center2[a1]=v1;
      for(i=1;i<circles;i++) {
        r1=(double)i/circles;
        d3.mul_d(scale2,scale,r1);
        v=D3Circle(p,v,center2,scale2,radials,axes);
      }

      for(i=0;i<layers-1;i++) {
        v1=center[a1]+n1*scale[a1]*((double)i/(layers-1)*2-1);
        r1=(double)(layers-i-1)/(layers-1);
        center2[a1]=v1;
        d3.mul_d(scale2,scale,r1);
        v=D3Circle(p,v,center2,scale2,radials,axes);
      }

      v1=center[a1]+n1*scale[a1];
      p[v+a0]=center[a0];p[v+a1]=v1;p[v+a2]=center[a2];  
      return true;  
    }
    public bool Box(double[] center,double[] scale,int xres,int yres,int zres) {
      int n,i,j,k;
      double v2,v1;
      int v;

      if(xres<2) xres=2;
      if(yres<2) yres=2;
      if(zres<2) zres=2;
      v=3*N;
      n=2*(xres*yres+xres*zres+yres*zres)-4*(xres+yres+zres)+8;
      if(!append_space(n))
        return false;
      v1=center[1]-scale[1];
      for(j=0;j<zres;j++) { // bottom
        v2=center[2]+(2.0*j/(zres-1)-1)*scale[2];
        for(i=0;i<xres;i++) {
          p[v++]=center[0]+(2.0*i/(xres-1)-1)*scale[0];p[v++]=v1;p[v++]=v2;
        }
      }

      for(k=1;k<yres-1;k++) {
        v1=center[1]+(2.0*k/(yres-1)-1)*scale[1];
        v2=center[2]-scale[2];
        for(i=0;i<xres;i++) {
          p[v++]=center[0]+(2.0*i/(xres-1)-1)*scale[0];p[v++]=v1;p[v++]=v2;
        }
        for(j=1;j<zres-1;j++) {
          v2=center[2]+(2.0*j/(zres-1)-1)*scale[2];
          p[v++]=center[0]-scale[0];p[v++]=v1;p[v++]=v2;
          p[v++]=center[0]+scale[0];p[v++]=v1;p[v++]=v2;
        }
        v2=center[2]+scale[2];
        for(i=0;i<xres;i++) {
          p[v++]=center[0]+(2.0*i/(xres-1)-1)*scale[0];p[v++]=v1;p[v++]=v2;
        }
      }
  
      v1=center[1]+scale[1];
      for(j=0;j<zres;j++) { // top
        v2=center[2]+(2.0*j/(zres-1)-1)*scale[2];
        for(i=0;i<xres;i++) {
          p[v++]=center[0]+(2.0*i/(xres-1)-1)*scale[0];p[v++]=v1;p[v++]=v2;
        }
      }      
      return true;
    }
// create rotoid from array of 3D points [x,y,z] around axis y
    public bool Rotoid(double[] center,double[] scale,int points,double[] point,int radials,POINTS mode) {
      int j,size;
      bool has_cap,has_cap2;
      double h;
      int pe;
      double[] cap=new double[3],cap2=new double[3];
      int v;
      int pi=0;

      if((has_cap=(0!=(mode&(POINTS.CAP|POINTS.IMPLICIT_CAP))))) {
        cap[1]=scale[1]*point[1];
        if(0!=(mode&POINTS.CAP)) {
          cap[0]=scale[0]*point[pi];
          cap[2]=scale[2]*point[pi+2];
          pi+=3;
          points--;
        } else
          cap[0]=cap[2]=0;          
      }

      if((has_cap2=(0!=(mode&(POINTS.CAP2|POINTS.IMPLICIT_CAP2))))) {
        cap2[1]=scale[1]*point[points*3-2];
        if(0!=(mode&POINTS.CAP2)) {
          cap2[0]=scale[0]*point[pi+points*3-3];
          cap2[2]=scale[2]*point[pi+points*3-1];
          points--;
        } else 
          cap2[0]=cap2[2]=0;
      }

      size=points*radials+(has_cap?1:0)+(has_cap2?1:0);
   
      v=3*N;
      if(!append_space(size))
        return false;
 
      if(has_cap) {
        d3.add(p,v,center,0,cap,0);
        v+=3;
      }  

      pe=pi+3*points;
      while(pi<pe) {
        double r0=point[pi],r2=point[pi+2];
        h=center[1]+scale[1]*point[pi+1];
        for(j=0;j<radials;j++) {
          double fi=2*System.Math.PI*j/radials,c=System.Math.Cos(fi),s=System.Math.Sin(fi);
          p[v++]=center[0]+scale[0]*(c*r0-s*r2);p[v++]=h;p[v++]=center[2]+scale[2]*(s*r0+c*r2);
        }
        pi+=3;
      }

      if(has_cap2) 
        d3.add(p,v,center,0,cap2,0);
      return true;
    }
    public bool Array2D(double[] min,double[] xaxis,double[] yaxis,int xres,int yres) {
      int r=3*N;
      if(!append_space(xres*yres)) return false;
      for(int y=0;y<yres;y++)
        for(int x=0;x<xres;x++) {
          p[r++]=min[0]+xaxis[0]*x/(xres-1)+yaxis[0]*y/(yres-1);
          p[r++]=min[1]+xaxis[1]*x/(xres-1)+yaxis[1]*y/(yres-1);
          p[r++]=min[2]+xaxis[2]*x/(xres-1)+yaxis[2]*y/(yres-1);
        }  
      return true;
    }
    public bool ArrayRadial(double[] center,double[] xaxis,double[] yaxis,int circles,int radials) {
      int r=3*N;
      if(!append_space(1+circles*radials)) return false;
      d3.copy(p,r,center,0);r+=3;
      for(int y=1;y<=circles;y++) {
        double r2=(double)y/circles;
        for(int x=0;x<radials;x++) {
          double f=2*System.Math.PI*x/radials;
          double c=r2*System.Math.Cos(f),s=r2*System.Math.Sin(f);
          p[r++]=center[0]+xaxis[0]*c+yaxis[0]*s;
          p[r++]=center[1]+xaxis[1]*c+yaxis[1]*s;
          p[r++]=center[2]+xaxis[2]*c+yaxis[2]*s;
        }
      }  
      return true;
    }
    public bool Toroid(double[] center,double[] scale,int points,double[] point,double radius,int radials,POINTS mode,short axes) {
      int pt=3*N;
      if(!append_space(points*radials)) return false;
      if(axes==0) axes=0x012;
      int a2=axes&3,a1=(axes>>4)&3,a0=(axes>>8)&3;
      for(int j=0;j<radials;j++) {
        double fi=2*System.Math.PI*j/radials;
        double cf=System.Math.Cos(fi),sf=System.Math.Sin(fi);
        for(int i=0;i<points;i++) {  
          p[pt+a0]=scale[a0]*(cf*(radius+point[3*i])-sf*point[3*i+2])+center[a0];
          p[pt+a1]=scale[a1]*point[3*i+1]+center[a1];
          p[pt+a2]=scale[a2]*(sf*(radius+point[3*i])+cf*point[3*i+2])+center[a2];
          pt+=3;
        }
      }
      return true;
    }
    public void Cube2Sphere(double[] center,double weight) {
      double[] v=new double[3];
      for(int pt=0;pt<N;pt++) {
        double a,mx;
        d3.sub(v,0,p,pt,center,0);
        double ax=System.Math.Abs(v[0]),ay=System.Math.Abs(v[1]),az=System.Math.Abs(v[2]);
        mx=ax>=ay?ax:ay;
        if(mx<az) mx=az;
        if(mx>0.0) {
          a=System.Math.Sqrt(3)*mx/System.Math.Sqrt(ax*ax+ay*ay+az*az);
          d3.mul_d(v,v,1+(a-1)*weight);
        }
        d3.add(p,pt,v,0,center,0);
      }      
    }
    public void Sphere2Cube(double[] center,double weight) {
      double[] v=new double[3];
      for(int pt=0;pt<N;pt++) {
        double a,mx;
        d3.sub(v,0,p,pt,center,0);
        double ax=System.Math.Abs(v[0]),ay=System.Math.Abs(v[1]),az=System.Math.Abs(v[2]);
        mx=ax>=ay?ax:ay;
        if(mx<az) mx=az;
        if(mx>0.0) {
          a=System.Math.Sqrt(ax*ax+ay*ay+az*az)/System.Math.Sqrt(3)/mx;
          d3.mul_d(v,v,1+(a-1)*weight);
        }
        d3.add(p,pt,v,0,center,0);
      }
    }
    public void FaceCenter(int[] faces,int fi,double[] center) {
      double[] rd3=new double[3];
      for(int i=1;i<faces[fi];i++)
        d3.add(rd3,0,rd3,0,p,faces[fi+i]);
      d3.mul_d(center,rd3,1.0/faces[fi]);
    }
    public bool Tetrahedron(double[] center,double[] scale) {
      double r;
      double v=-1.0/3; //1/sqrt(3)
      int i;
      int h=3*N;
      append_space(4);

//  d3_mul_d(scale2,scale,1/sqrt(3));
      r=System.Math.Sqrt(1-v*v);
      for(i=0;i<3;i++) {
        p[h]=r*System.Math.Cos(i*System.Math.PI*2/3);p[h+1]=v;p[h+2]=r*System.Math.Sin(i*System.Math.PI*2/3);
        d3.add(p,h,center,0,d3.mul(p,h,p,h,scale,0),h);
        h+=3;
      }
      p[h++]=center[0];p[h++]=center[1]+scale[1];p[h++]=center[2];   
      return true;
    }
    static readonly double[] HexahedronPts=new double[] {
      -1,-1,-1, 1,-1,-1, 1,-1,1, -1,-1,1, -1,1,-1, 1,1,-1, 1,1,1, -1,1,1
    };
    public bool Hexahedron(double[] center,double[] scale) {
      double[] scale2=new double[3];
      int h=3*N;
      append_space(8);

      d3.mul_d(scale2,scale,1/System.Math.Sqrt(3));
      for(int i=0;i<8;i++) {
        d3.add(p,h,center,0,d3.mul(p,h,HexahedronPts,3*i,scale2,0),h);
        h+=3;
      }  
      return true;
    }
    static readonly double[] OctahedronPts=new double[] {
      0,-1,0, -1,0,-1, 1,0,-1, 1,0,1, -1,0,1, 0,1,0
    };
    public bool Octahedron(double[] center,double[] scale) {
      double[] scale2=new double[3];
      int h=3*N;
      append_space(6);

      scale2[0]=scale[0]/System.Math.Sqrt(2);scale2[1]=scale[1];scale2[2]=scale[2]/System.Math.Sqrt(2);
      for(int i=0;i<6;i++) {
        d3.add(p,h,center,0,d3.mul(p,h,OctahedronPts,3*i,scale2,0),h);
        h+=3;
      }  
      return true;
    }
// points for dedecahedron
    static readonly double[] DodecahedronV=new double[] {
      -0.79465447229176612293
      ,-0.18759247408507989953
      ,0.18759247408507989953
      ,0.79465447229176612293
    };
    static readonly double[] DodecahedronO=new double[] {0,0,0.5,0.5};
    public bool Dodecahedron(double[] center,double[] scale) {
      int h=3*N;
      append_space(20);
  
      for(int j=0;j<4;j++) {
        double r=System.Math.Sqrt(1-DodecahedronV[j]*DodecahedronV[j]);
        for(int i=0;i<5;i++) {
          p[h++]=center[0]+scale[0]*(r*System.Math.Cos((i+DodecahedronO[j])*2*System.Math.PI/5));
          p[h++]=center[1]+scale[1]*DodecahedronV[j];
          p[h++]=center[2]+scale[2]*(r*System.Math.Sin((i+DodecahedronO[j])*2*System.Math.PI/5));
        }
      }  
      return true;
    }
    public bool Icosahedron(double[] center,double[] scale) {
      int h=3*N;
      append_space(12);
      
      p[h++]=center[0];p[h++]=center[1]-scale[1];p[h++]=center[2];
      for(int j=0;j<2;j++) {
        double v=j==0?-0.44721359549995793928:0.44721359549995793928;
        double o=j==0?0:0.5;
        double r=System.Math.Sqrt(1-v*v);
        for(int i=0;i<5;i++) {
          p[h++]=center[0]+scale[0]*(r*System.Math.Cos((i+o)*2*System.Math.PI/5));
          p[h++]=center[1]+scale[1]*v;
          p[h++]=center[2]+scale[2]*(r*System.Math.Sin((i+o)*2*System.Math.PI/5));
        }
      }
      p[h++]=center[0];p[h++]=center[1]+scale[1];p[h++]=center[2];  
      return true;
    }
    public bool HexCylinder(double[] center,double[] scale) {
      int pt=3*N;
      append_space(12);
      double h=center[1]-scale[1];
      for(int j=0;j<2;j++) {
        for(int i=0;i<6;i++) {
          double fi=(2*i+1)*System.Math.PI/6;
          p[pt++]=center[0]+scale[0]*System.Math.Cos(fi);
          p[pt++]=h;
          p[pt++]=center[2]+scale[2]*System.Math.Sin(fi);
        }
        h+=2*scale[1];
      }
      return true;
    }
    
    internal class PointComparer:System.Collections.Generic.IComparer<int> {
      double[] P;
      int S0,S1,S2;
      
      internal PointComparer(double[] p,int s0,int s1) {
        P=p;
        S0=s0;S1=s1;S2=3-s0-s1;
      }
      public int Compare(int a,int b) {
        int pa=3*a,pb=3*b;
        if(P[pa+S0]==P[pb+S0]) {
          if(P[pa+S1]==P[pb+S1]) {
            if(P[pa+S2]==P[pb+S2]) {
              return 0;
            } else
              return P[pa+S2]<P[pb+S2]?-1:1;
          } else
            return P[pa+S1]<P[pb+S1]?-1:1;
        } else
          return P[pa+S0]<P[pb+S0]?-1:1;
      }
    }

    public ArrayList Join(double dist) {
      ArrayList[] master=new ArrayList[N];
      int n=0;
      double dist2=dist*dist;
      int[] order=new int[N];
      for(int i=0;i<N;i++) order[i]=i;
      Array.Sort(order,new PointComparer(p,0,1));
      int j1=0;
      for(int i2=0;i2<N-1;i2++) {
        int i=order[i2];
        double xi=p[3*i],xid=xi+dist;
        for(int j2=i2+1;j2<N;j2++) {
          int j=order[j2];
          double xj=p[3*j];
          if(xj>xid) break;
          double d=d3.distance2(p,3*i,p,3*j);
          if(d<dist2) {
            ArrayList ai=master[i],aj=master[j];
            if(ai!=null) {
              master[j]=ai;
              if(aj==null)
                ai.Add(j);
              else if(ai!=aj) {
                n--;
                foreach(object o in aj)
                  ai.Add(o);                  
              }
            } else if(aj!=null) {
              master[i]=aj;
              aj.Add(i);
            } else {
              n++;
              master[i]=master[j]=ai=new ArrayList();
              ai.Add(i);
              ai.Add(j);
            }           
          }
        }
      }  
      if(n<1) return null;  
      ArrayList r=new ArrayList(n);
      for(int i=0;i<master.Length;i++) {
        ArrayList a=master[i];
        if(a!=null&&i==(int)a[0])
          r.Add(a);
      }
      /*foreach(ArrayList a in master)
        if(a!=null&&!r.Contains(a)) r.Add(a);*/
      return r;
    }  
    
  }  
}
