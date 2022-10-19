using System;
using System.Collections;
using System.Text;

namespace Math3d {
  public abstract class d3 {
    public static readonly double[] V0=new double[] {0,0,0};
    public static readonly double[] V1=new double[] {1,1,1};
    public static readonly double[] VX=new double[] {1,0,0};
    public static readonly double[] VY=new double[] {0,1,0};
    public static readonly double[] VZ=new double[] {0,0,1};
    
    public static double[] V() {
      return new double[3];
    }
    public static double[] V(double value) {
      return new double[] {value,value,value};
    }
    public static double[] V(double x,double y,double z) {
      return new double[] {x,y,z};
    }
    public static double[] V(double[] x) {
      return x==null?null:new double[] {x[0],x[1],x[2]};
    }
    
    public static double Degree(double rad) {
      return rad*180/System.Math.PI;
    }
    
    public static double Radian(double deg) {
      return deg*System.Math.PI/180;
    }
     
    public static double[] copy(double[] r,double[] x) {
      r[0]=x[0];r[1]=x[1];r[2]=x[2];
      return r;
    }
    public static double[] copy(double [] r,int ri,double[] s,int si) {
      r[ri++]=s[si++];r[ri++]=s[si++];r[ri++]=s[si++];
      return r;
    }
    public static int bc3(int m) { return (0!=(m&1)?1:0)+(0!=(m&2)?1:0)+(0!=(m&4)?1:0);}
    public static int abs(double[] r) {
      int m=0;
      if(r[0]<0) {r[0]=-r[0];m|=1;m^=8;}
      if(r[1]<0) {r[1]=-r[1];m|=2;m^=8;}
      if(r[2]<0) {r[2]=-r[2];m|=4;m^=8;}
      return m;
    }
    public static void swap(ref double x,ref double y) { 
      double d;
      d=x;x=y;y=d;
    }
    public static double[] swap(double[] x,double[] y) {
      return swap(x,0,y,0);
    }
    public static double[] swap(double[] x,int xi,double[] y,int yi) {
      double r;
      r=x[xi];x[xi++]=y[yi];y[yi++]=r;
      r=x[xi];x[xi++]=y[yi];y[yi++]=r;
      r=x[xi];x[xi]=y[yi];y[yi]=r;
      return x;
    }
    
    public static double[] add(double[] r,double[] y) { return add(r,0,r,0,y,0);}  
    public static double[] add(double[] r,double[] x,double[] y) {
      return add(r,0,x,0,y,0);
    }
      
    public static double[] add(double[] r,int ri,double[] x,int xi,double[] y,int yi) {
      r[ri++]=x[xi++]+y[yi++];r[ri++]=x[xi++]+y[yi++];r[ri]=x[xi]+y[yi];
      return r;
    }
    
    public static double[] add_d(double[] r,double[] x,double a) {
      r[0]=x[0]+a;r[1]=x[1]+a;r[2]=x[2]+a;
      return r;
    }
    public static double[] add_d(double[] r,int ri,double[] x,int xi,double a,double[] y,int yi) {
      r[ri++]=x[xi++]+a*y[yi++];r[ri++]=x[xi++]+a*y[yi++];r[ri]=x[xi]+a*y[yi];
      return r;
    }
    
    public static double[] sub(double[] r,double[] x,double[] y) {
      r[0]=x[0]-y[0];r[1]=x[1]-y[1];r[2]=x[2]-y[2];
      return r;
    }
    public static double[] sub(double[] r,int ri,double[] x,int xi,double[] y,int yi) {
      r[ri++]=x[xi++]-y[yi++];r[ri++]=x[xi++]-y[yi++];r[ri]=x[xi]-y[yi];
      return r;
    }
    
    public static double[] mul(double[] r,double[] x,double[] y) {
      return mul(r,0,x,0,y,0);
    }
    public static double[] mul(double[] r,int ri,double[] x,int xi,double[] y,int yi) {
      r[ri++]=x[xi++]*y[yi++];r[ri++]=x[xi++]*y[yi++];r[ri]=x[xi]*y[yi];
      return r;
    }
    
    public static double[] mul_d(double[] r,double[] x,double a) {
      r[0]=x[0]*a;r[1]=x[1]*a;r[2]=x[2]*a;
      return r;
    }
    public static double[] mul_d(double[] r,int ri,double[] x,int xi,double a) {
      r[ri++]=x[xi++]*a;r[ri++]=x[xi++]*a;r[ri]=x[xi]*a;
      return r;
    }
    
    public static double[] div(double[] r,double[] x,double[] y) {
      r[0]=x[0]/y[0];r[1]=x[1]/y[1];r[2]=x[2]/y[2];
      return r;
    }
    
    public static double[] div_d(double[] r,double[] x,double a) {
      r[0]=x[0]/a;r[1]=x[1]/a;r[2]=x[2]/a;
      return r;
    }
    
    public static double[] axb(double[] r,double[] x,double[] a,double[] b) {
      r[0]=a[0]*x[0]+b[0];
      r[1]=a[1]*x[1]+b[1];
      r[2]=a[2]*x[2]+b[2];
      return r;
    }
    
    public static double[] linear_interpol(double[] r,double w,double[] x,double[] y) {
      double w1=1-w;
      r[0]=w1*x[0]+w*y[0];r[1]=w1*x[1]+w*y[1];r[2]=w1*x[2]+w*y[2];
      return r;
    }
    public static double[] linear_interpol(double[] r,int ri,double w,double[] x,int xi,double[] y,int yi) {
      double w1=1-w;
      r[ri]=w1*x[xi+0]+w*y[yi+0];r[ri+1]=w1*x[xi+1]+w*y[yi+1];r[ri+2]=w1*x[xi+2]+w*y[yi+2];
      return r;
    }
    
    public static double scalar(double[] x,double[] y) {
      return x[0]*y[0]+x[1]*y[1]+x[2]*y[2];
    }
    public static double scalar(double[] x,int xi,double[] y,int yi) {
      return x[xi]*y[yi]+x[xi+1]*y[yi+1]+x[xi+2]*y[yi+2];
    }
    public static double[] cross(double[] r,double[] x,double[] y) {
      return cross(r,0,x,0,y,0);
    }
    public static double[] cross(double[] r,int ri,double[] x,int xi,double[] y,int yi) {
      double r0,r1,r2;
      r0=x[xi+1]*y[yi+2]-x[xi+2]*y[yi+1];
      r1=x[xi+2]*y[yi+0]-x[xi+0]*y[yi+2];
      r2=x[xi+0]*y[yi+1]-x[xi+1]*y[yi+0];
      r[ri]=r0;r[ri+1]=r1;r[ri+2]=r2;
      return r;
    }
    public static double[] mul_d33(double[] r,double[] v,double[] m) {
      return mul_d33(r,0,v,0,m);
    }
    public static double[] mul_d33(double[] r,int ri,double[] v,int vi,double[] m) {
      double v0=v[vi],v1=v[vi+1],v2=v[vi+2];
      r[ri++]=v0*m[0]+v1*m[3]+v2*m[6];
      r[ri++]=v0*m[1]+v1*m[4]+v2*m[7];
      r[ri++]=v0*m[2]+v1*m[5]+v2*m[8];
      return r;
    }
    
    public static double[] div_d33(double[] r,double[] v,double[] m) {
      return div_d33(r,0,v,0,m);
    }
    public static double[] div_d33(double[] r,int ri,double[] v,int vi,double[] m) {
      double v0=v[vi],v1=v[vi+1],v2=v[vi+2];
      r[ri++]=v0*m[0]+v1*m[1]+v2*m[2];
      r[ri++]=v0*m[3]+v1*m[4]+v2*m[5];
      r[ri++]=v0*m[6]+v1*m[7]+v2*m[8];
      return r;
    }
    public static double radius(double[] v) { 
      return radius(v,0);
    }
    public static double radius(double[] v,int vi) {
      return System.Math.Sqrt(v[vi]*v[vi]+v[vi+1]*v[vi+1]+v[vi+2]*v[vi+2]);
    }
    
    public static double[] normal(double[] w,double[] v) {
      return normal(w,0,v,0);
    }
    public static double[] normal(double[] w,int wi,double[] v,int vi) {
      double r=radius(v,vi);
      if(r==0)
        w[wi]=w[wi+1]=w[wi+2]=0;
      else {
        w[wi]=v[vi]/r;w[wi+1]=v[vi+1]/r;w[wi+2]=v[vi+2]/r;
      }  
      return w;
    }
        
    public static double distance(double[] u,double[] v) {
      double d0=v[0]-u[0],d1=v[1]-u[1],d2=v[2]-u[2];
      return System.Math.Sqrt(d0*d0+d1*d1+d2*d2);
    }
    public static double distance2(double[] u,int ui,double[] v,int vi) {
      double d0=v[vi]-u[ui],d1=v[vi+1]-u[ui+1],d2=v[vi+2]-u[ui+2];
      return d0*d0+d1*d1+d2*d2;
    }
    public static double distance(double[] u,int idx,double[] v) {
      double d0=v[0]-u[idx+0],d1=v[1]-u[idx+1],d2=v[2]-u[idx+2];
      return System.Math.Sqrt(d0*d0+d1*d1+d2*d2);
    }
        
// distance of point from line with center and normal    
    public static double line_distance(double[] center,double[] normal,double[] point,int idx) {
      double dx=point[idx+0]-center[0],dy=point[idx+1]-center[1],dz=point[idx+2]-center[2];
      double s=dx*normal[0]+dy*normal[1]+dz*normal[2];
      dx-=s*normal[0];dy-=s*normal[1];dz-=s*normal[2];
      return System.Math.Sqrt(dx*dx+dy*dy+dz*dz);
    }    
// return one of perpendicular vectors    
    public static double[] perpendicular(double[] w,double[] v) {
      double v0=v[0],v1=v[1],v2=v[2];
      double f0=System.Math.Abs(v0),f1=System.Math.Abs(v1),f2=System.Math.Abs(v2);
      double[] a=new double[3];
      if(f2>f0&&f2>f1) {
        a[0]=0;a[1]=v2;a[2]=-v1; // use x
      } else if(f1>f0&&f1>f2) {
        a[0]=v1;a[1]=-v0;a[2]=0; // use z
      } else {
        a[0]=-v2;a[1]=0;a[2]=v0; // use y
      }  
      cross(a,v,a);
      return normal(w,a);
    }      
// rotate x around axis by angle 
    public static double[] rotate(double[] r,double[] x,double[] axis,double angle) {
      double[] vo=new double[3],vr=new double[3],vr2=new double[3];
      mul_d(vo,axis,scalar(axis,x));
      sub(vr,x,vo);
      cross(vr2,axis,vr);
  
      mul_d(vr,vr,System.Math.Cos(angle));
      mul_d(vr2,vr2,System.Math.Sin(angle));
      sub(r,vr,vr2);
      add(r,r,vo);
      return r;
    }
// random normalised vector     
    public static double[] random(double[] r,double random11,double random02pi) {
      double rad,fi;
      double iz=random11;//rand()%2049-1024;
      r[2]=iz;
      rad=System.Math.Sqrt(1-iz*iz);
      fi=random02pi;
      r[0]=rad*System.Math.Cos(fi);
      r[1]=rad*System.Math.Sin(fi);
      //printf("%f %f %f\n",r[0],r[1],r[2]);
      return r;
    }  
    public static double[] random_deflection(double[] r,double[] n,double fi,double random02pi) {
      double phi;
      double[] v=new double[3],m=new double[9];

      phi=random02pi; 
      v[0]=System.Math.Sin(fi)*System.Math.Cos(phi);
      v[1]=System.Math.Cos(fi);
      v[2]=System.Math.Sin(fi)*System.Math.Sin(phi);
      d33.rotation_vv(m,n,d3.VY);
      mul_d33(r,v,m); //v
      return r;
    }   
    public static double[] swap_xyz(double[] r,double[] x,int xi,int yi,int zi) {
      double y0=x[xi],y1=x[yi],y2=x[zi];
      r[0]=y0;r[1]=y1;r[2]=y2;
      return r;
    }  
// angle between 2 vectors
    public static double angle(double[] v,double[] u) {
      double ca=scalar(v,u)/System.Math.Sqrt(scalar(v,v)*scalar(u,u));
      if(ca>1.0) ca=1.0;else if(ca<-1.0) ca=-1.0;
      return System.Math.Acos(ca);
    }   
    public static double[] bezier(double[] y,double x,double[] point) {
      double b0,b1,b2,b3;
      b0=1-x;b1=3*b0;b3=x*x;b2=b1*b3;b1*=b0*x;b0*=b0*b0;b3*=x;
      y[0]=b0*point[0]+b1*point[3]+b2*point[6]+b3*point[9];
      y[1]=b0*point[1]+b1*point[4]+b2*point[7]+b3*point[10];
      y[2]=b0*point[2]+b1*point[5]+b2*point[8]+b3*point[11];
      return y;
    }     
// interpolation from normal x to normal y with weight w
    public static double[] normal_interpolation(double[] r,double w,double[] x,double[] y) {
      double[] rota=new double[9],axis=new double[3];
      double angle2;
      cross(axis,y,x);
//  printf("axis=<%.3f,%.3f,%.3f>\n",axis[0],axis[1],axis[2]);
      angle2=angle(x,y);
//  printf("angle=%.4f\n",angle);
      d33.rotation_aa(rota,axis,0,angle2*w);
      mul_d33(r,x,rota);
      return r;
    }

    public static int gt(double[] a,double[] b) {
      return (a[0]>b[0]?1:0)|(a[1]>b[1]?2:0)|(a[2]>b[2]?4:0);
    }
    public static void swap(double[] a,double[] b,int mask) {
      double x;
      if(0!=(mask&1)) {x=a[0];a[0]=b[0];b[0]=x;}
      if(0!=(mask&2)) {x=a[1];a[1]=b[1];b[1]=x;}
      if(0!=(mask&4)) {x=a[2];a[2]=b[2];b[2]=x;}      
    }    
    public static void mul_d(double[] a,double d,int mask) {
      if(0!=(mask&1)) a[0]*=d;
      if(0!=(mask&2)) a[1]*=d;
      if(0!=(mask&4)) a[2]*=d;
    }
   
    public static string ToString(double[] d3,string format,double[] mul,double[] add) {
      return ToString(d3,0,format,mul,add);
    }
    public static string ToString(double[] d3,int i,string format,double[] mul,double[] add) {
      double d0=d3[i],d1=d3[i+1],d2=d3[i+2];
      if(mul!=null) {       
        if(add!=null) 
          {d0=d0*mul[0]+add[0];d1=d1*mul[1]+add[1];d2=d2*mul[2]+add[2];}
        else 
          {d0*=mul[0];d1*=mul[1];d2*=mul[2];}
      } else if(add!=null) 
        {d0+=add[0];d1+=add[1];d2+=add[2];}
      return string.Format(System.Globalization.CultureInfo.InvariantCulture,format,d0,d1,d2);
    }
        
    public static string ToString(double[] d3) {
      return "["+d3[0].ToString(System.Globalization.CultureInfo.InvariantCulture)
        +","+d3[1].ToString(System.Globalization.CultureInfo.InvariantCulture)
        +","+d3[2].ToString(System.Globalization.CultureInfo.InvariantCulture)
        +"]";
    }
    
  }
  public abstract class d33 {
    public static readonly double[] M0=new double[] {0,0,0,0,0,0,0,0,0};
    public static readonly double[] M1=new double[] {1,0,0,0,1,0,0,0,1};
    public static double[] M(double x) {
      return new double[9] {x,0,0,0,x,0,0,0,x};
    }
    public static double[] M(double x,double y,double z,bool deg) {
      double[] r=new double[9];
      rotation_xyz(r,x,y,z,deg);
      return r;
    }      
    public static double[] M(double []xyz,bool deg) { return M(xyz[0],xyz[1],xyz[2],deg);}    
    public static double[] copy(double[] dst,double[] src) {
      Array.Copy(src,dst,9);
      return dst;
    }
    
    public static double[] swap(double[] a,double[] b) {
      d3.swap(a,b);
      d3.swap(a,3,b,3);
      d3.swap(a,6,b,6);
      return a;
    }
      
    public static double[] mul_d(double[] r,double[] x,double p) {
      int i;
      for(i=0;i<9;i++)
        r[i]=x[i]*p;
      return r;
    }

    public static double[] div_d(double[] r,double[] x,double p) {
      int i;
      for(i=0;i<9;i++)
        r[i]=x[i]/p;
      return r;
    }
    public static double[] mul_d3(double[] r,double[] x,double[] v) {
      double v0=v[0],v1=v[1],v2=v[2];
      r[0]=x[0]*v0;r[1]=x[1]*v0;r[2]=x[2]*v0;
      r[3]=x[3]*v1;r[4]=x[4]*v1;r[5]=x[5]*v1;
      r[6]=x[6]*v2;r[7]=x[7]*v2;r[8]=x[8]*v2;
      return r;
    }

    public static double[] div_d3(double[] r,double[] x,double[] v) {
      double v0=v[0],v1=v[1],v2=v[2];
      r[0]=x[0]*v0;r[1]=x[1]*v1;r[2]=x[2]*v2;
      r[3]=x[3]*v0;r[4]=x[4]*v1;r[5]=x[5]*v2;
      r[6]=x[6]*v0;r[7]=x[7]*v1;r[8]=x[8]*v2;
      return r;
    }

    public static double[] mul(double[] r,double[] a,double[] b) {
      double[] d;      
      if(r==a||r==b) {
        d=new double[9]; 
      } else d=r;
      d3.mul_d33(d,a,b);
      d3.mul_d33(d,3,a,3,b);
      d3.mul_d33(d,6,a,6,b);
      if(d!=r)
        d33.copy(r,d);
      return d;
    }

    public static double[] div(double[] r,double[] a,double[] b) {
      double[] d;      
      if(r==a||r==b) {
        d=new double[9]; 
      } else d=r;
      d3.div_d33(d,a,b);
      d3.div_d33(d,3,a,3,b);
      d3.div_d33(d,6,a,6,b);
      if(d!=r)
        d33.copy(r,d);
      return d;
    }
    
    public static double[] transpose(double[] r,double[] m) {
      if(r==m) {
        double x;
        x=r[1];r[1]=r[3];r[3]=x;
        x=r[2];r[2]=r[6];r[6]=x;
        x=r[5];r[5]=r[7];r[7]=x;
      } else {
        r[0]=m[0];r[1]=m[3];r[2]=m[6];
        r[3]=m[1];r[4]=m[4];r[5]=m[7];
        r[6]=m[2];r[7]=m[5];r[8]=m[8];
      }
      return r;
    }
// calculate matrix as rotation of angle around axis
    public static double[] rotation_aa(double[] w,double[] axis,int ai,double angle) {
      double co=System.Math.Cos(angle),co1=1-co,si=System.Math.Sin(angle);
      double ax=axis[ai]*co1,ay=axis[ai+1]*co1,az=axis[ai+2]*co1;

      w[0]=axis[ai]*ax+co; w[1]=axis[ai+1]*ax-axis[ai+2]*si; w[2]=axis[ai+2]*ax+axis[ai+1]*si;
      w[3]=axis[ai]*ay+axis[ai+2]*si; w[4]=axis[ai+1]*ay+co; w[5]=axis[ai+2]*ay-axis[ai+0]*si;
      w[6]=axis[ai]*az-axis[ai+1]*si; w[7]=axis[ai+1]*az+axis[ai+0]*si; w[8]=axis[ai+2]*az+co;
      return w;      
    }
// calculate rotation matrix from one vector to second vector
    public static double[] rotation_vv(double[] w,double[] v1,double[] v2) {
      double[] u=new double[3];
      double r,fi,cfi;

      d3.cross(u,v1,v2);
      r=d3.radius(u);
//  printf("rotation_vv.r=%e\n",r);
      if (r<1E-20) {
        if(d3.scalar(v1,v2)>0)
          d33.copy(w,d33.M1);
        else {  // backwards
          d3.perpendicular(u,v1);
          d33.rotation_aa(w,u,0,System.Math.PI);
        }
      } else {
        u[0]/=r;u[1]/=r;u[2]/=r;
        cfi=d3.scalar(v1,v2)/d3.radius(v1)/d3.radius(v2);
        if(cfi<-1) cfi=-1;
        else if(cfi>1) cfi=1;
   //   printf("rotation_vv.cfi=%e\n",cfi);
        fi=System.Math.Acos(cfi);
   
 //  printf("rotation_vv.fi=%e\n",fi);
        d33.rotation_aa(w,u,0,fi);
      }
      return w;
    }
    public static double[] rotation_xyz(double[] w,double x,double y,double z,bool deg) {
      if(deg) { x=x*Math.PI/180;y=y*Math.PI/180;z=z*Math.PI/180;}
      double cx=Math.Cos(x),sx=Math.Sin(x),cy=Math.Cos(y),sy=Math.Sin(y),cz=Math.Cos(z),sz=Math.Sin(z);
      double sxsy=sx*sy,cxsy=cx*sy;
      w[0]=cy*cz;w[1]=cy*sz;w[2]=sy;
      w[3]=-sxsy*cz-cx*sz;w[4]=-sxsy*sz+cx*cz;w[5]=sx*cy;
      w[6]=-cxsy*cz+sx*sz;w[7]=-cxsy*sz-sx*cz;w[8]=cx*cy;
/*
1   0  0|  cy 0 sy |    cy   0   sy |   cz  sz 0  |  cycz                 cysz   sy
0  cx sx|   0 1  0 | -sxsy  cx sxcy |  -sz  cz 0  | -sxsycz-cxsz  -sxsysz+cxcz sxcy
0 -sx cx| -sy 0 cy | -cxsy -sx cxcy |    0   0 1  | -cxsycz+sxsz  -cxsysz-sxcz cxcy

 */      
      return w;
    }
// get rotation angles
    public static void rotation_xyz(double[] w,double []m,bool deg) {
      double y=Math.Asin(m[2]),z,x;
      if(Math.Abs(m[2])<0.999999) {
        z=Math.Atan2(m[1],m[0]);
        x=Math.Atan2(m[5],m[8]);
      } else {
        x=Math.Atan2(-m[7],m[4]);
        z=0;
      }
      if(deg) { x=x*180/Math.PI;y=y*180/Math.PI;z=z*180/Math.PI;}
      w[0]=x;w[1]=y;w[2]=z;
    }
    public static double[] rotate(double[] m,int axis,double angle,bool deg) {
      if(deg) angle*=Math.PI/180;
      double c=Math.Cos(angle),s=Math.Sin(angle),a,b;
      if(axis<1) {
        a=m[1];b=m[2];m[1]=c*a-s*b;m[2]=s*a+c*b;
        a=m[4];b=m[5];m[4]=c*a-s*b;m[5]=s*a+c*b;
        a=m[7];b=m[8];m[7]=c*a-s*b;m[8]=s*a+c*b;
      } else if(axis==1) {
        a=m[0];b=m[2];m[0]=c*a-s*b;m[2]=s*a+c*b;
        a=m[3];b=m[5];m[3]=c*a-s*b;m[5]=s*a+c*b;
        a=m[6];b=m[8];m[6]=c*a-s*b;m[8]=s*a+c*b;
      } else if(axis>1) {
        a=m[0];b=m[1];m[0]=c*a-s*b;m[1]=s*a+c*b;
        a=m[3];b=m[4];m[3]=c*a-s*b;m[4]=s*a+c*b;
        a=m[6];b=m[7];m[6]=c*a-s*b;m[7]=s*a+c*b;
      }
      return m;
    }
// orthonormalize matrix 
    public static double[] normal(double[] w,double[] m) {
      d3.normal(w,m);
      if(System.Math.Abs(w[2])>0.5)
        w[5]=-(w[0]*(w[3]=m[3])+w[1]*(w[4]=m[4]))/w[2];
      else if(System.Math.Abs(w[1])>0.5)
        w[4]=-(w[0]*(w[3]=m[3])+w[2]*(w[5]=m[5]))/w[1];
      else 
        w[3]=-(w[1]*(w[4]=m[4])+w[2]*(w[5]=m[5]))/w[0];
      d3.normal(w,3,w,3);
      d3.cross(w,6,w,0,w,3);
      d3.normal(w,6,w,6);
      return w;
    }
    public static double determinant(double[] m) {
      return m[0]*m[4]*m[8]+m[1]*m[5]*m[6]+m[2]*m[3]*m[7]-m[0]*m[5]*m[7]-m[1]*m[3]*m[8]-m[2]*m[4]*m[6];
    }
  }
  public abstract class nd3 {
    
    public static void MinMax(int n,double[] p,double[] min,double[] max) { MinMax(n,p,null,min,max);}
    public static void MinMax(int n,double[] p,double[] mul,double[] min,double[] max) {
      if(p==null||p.Length<3||n<1) {
        min[0]=min[1]=min[2]=0;
        max[0]=max[1]=max[2]=1;
        return;
      }      
      double min0,min1,min2,max0,max1,max2; 
      max0=min0=p[0];max1=min1=p[1];max2=min2=p[2];
      int ie=n*3;
      for(int i=3;i<ie;i++) {
        if(p[i]<min0) min0=p[i];
        else if(p[i]>max0) max0=p[i];
        i++;
        if(p[i]<min1) min1=p[i];
        else if(p[i]>max1) max1=p[i];
        i++;
        if(p[i]<min2) min2=p[i];
        else if(p[i]>max2) max2=p[i];        
      }
      min[0]=min0;min[1]=min1;min[2]=min2;
      max[0]=max0;max[1]=max1;max[2]=max2;
      if(mul!=null) {
        if(mul[0]<0) d3.swap(ref min[0],ref max[0]);
        if(mul[1]<0) d3.swap(ref min[1],ref max[1]);
        if(mul[2]<0) d3.swap(ref min[2],ref max[2]);
        min[0]*=mul[0];max[0]*=mul[0];
        min[1]*=mul[1];max[1]*=mul[1];
        min[2]*=mul[2];max[2]*=mul[2];
      }
    }
    public static void add_d3(int n,double[] p,double[] v) {
      if(p==null||p.Length<3||n<1) return;
      int ie=n*3;
      for(int i=0;i<ie;i+=3)
        d3.add(p,i,p,i,v,0);        
    }
    public static void mul_d3(int n,double[] p,double[] v) {
      if(p==null||p.Length<3||n<1) return;
      int ie=n*3;
      for(int i=0;i<ie;i+=3)
        d3.mul(p,i,p,i,v,0);        
    }
    public static void mul_d3(int n,double[] p,double[] a,double[] b,double[] c,double[] d) {
      if(p==null||p.Length<3||n<1) return;
      int i=0,ie=n*3;
      while(i<ie) {
        p[i]=(p[i]-a[0])*b[0]/c[0]+d[0];i++;
        p[i]=(p[i]-a[1])*b[1]/c[1]+d[1];i++;
        p[i]=(p[i]-a[2])*b[2]/c[2]+d[2];i++;        
      }
    }
    public static void mul_d33(int n,double[] p,double[] m) {
      if(p==null||p.Length<3||n<1) return;
      int ie=n*3;
      for(int i=0;i<ie;i+=3)
        d3.mul_d33(p,i,p,i,m);        
    }
    public static void Permute(int n,double[] p,int px,int py,bool nx,bool ny,bool nz,bool back) {
      int pz;
      if(py==px) py=(py+1)%3;
      pz=3-px-py;
      if(back) {
        int[] rev=new int[3];        
        rev[px]=0;rev[py]=1;rev[pz]=2;
        bool[] b=new bool[3];
        b[0]=nx;b[1]=ny;b[2]=nz;
        px=rev[0];py=rev[1];pz=rev[2];
        nx=b[px];ny=b[py];nz=b[pz];
      }      
      for(int i=0,ie=3*n;i<ie;) {
        double x=p[i+px],y=p[i+py],z=p[i+pz];
        if(nx) x=-x;
        if(ny) y=-y;
        if(nz) z=-z;
        p[i++]=x;p[i++]=y;p[i++]=z;
      }
    }
    public static double[] Scalar(double[] r,int n,double[] p,double[] v) {
      if(p==null||p.Length<3||n<1) return r;
      int i=0,ie=n*3,j=0;
      if(r==null) r=new double[n];
      while(i<ie) {
        r[j]=p[i]*v[0]+p[i+1]*v[1]+p[i+2]*v[2];
        j++;i+=3;
      }
      return r;
    }
  }

}
