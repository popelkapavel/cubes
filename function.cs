using System;

namespace cubes {
  public abstract class f1d {
    public static bool GT0(double x) {
      return x>0;
    }
    public static bool GE0(double x) {
      return x>=0;
    }
  }
  public abstract class f3d {
    public static double sphere(double x,double y,double z) {
     return 1-(x*x+y*y+z*z);
    }

    public static double sinus(double x,double y,double z) {
      return Math.Sin(x)+Math.Sin(y)+Math.Sin(z);
    }
    
    public static double checker(double x,double y,double z) {
      int ix=(int)Math.Floor(x),iy=(int)Math.Floor(y),iz=(int)Math.Floor(z);
      return (ix^iy^iz)&1;
    }

    public static double sin1(double x,double y,double z) {
      x+=.1*Math.Sin((y+z)*5);y+=.1*Math.Sin((x+z)*10);z+=.1*Math.Sin((x+y)*10);
      x+=.1*Math.Sin((y+z)*10);y+=.1*Math.Sin((x+z)*5);z+=.1*Math.Sin((x+y)*10);
      x+=.1*Math.Sin((y+z)*10);y+=.1*Math.Sin((x+z)*10);z+=.1*Math.Sin((x+y)*5);
      return Math.Sin(x+Math.Sin(y+Math.Sin(z)));
    }

    public static double sin2(double x,double y,double z) {
      return Math.Sin(x+Math.Sin(y+Math.Sin(z)))+Math.Sin(y+Math.Sin(z+Math.Sin(x)))+Math.Sin(z+Math.Sin(x+Math.Sin(y)));
    }

    public static double sin3(double x,double y,double z) {
      return Math.Sin(Math.Sin(x)+2*Math.Sin(y)+3*Math.Sin(z))+Math.Sin(2*Math.Sin(x)+3*Math.Sin(y)+Math.Sin(z))+Math.Sin(3*Math.Sin(x)+Math.Sin(y)+2*Math.Sin(z));
    }

    public static double sin4(double x,double y,double z) {
      x+=Math.Sin(y+z);
      y+=Math.Sin(x+z);
      z+=Math.Sin(x+y);
      return Math.Sin(x)+Math.Sin(y)+Math.Sin(z);
    }

    public static double fractal(double x,double y,double z) {
      double zx,zy,zz,cx,cy,cz,r0,r1,r2,nx,ny,nz;
      int i;
      cx=zx=x;cy=zy=y;cz=zz=z;
      i=64;
      do {
        r0=zx*zx;
        r1=zy*zy;
        r2=zz*zz;
        if(r0+r1+r2>4) return 0;
        nx=r0-2*zy*zz;
        ny=r2+2*zx*zy;
        nz=r1+2*zx*zz;
        zx=nx+cx;zy=ny+cy;zz=nz+cz;
      } while(0<(i--));
      return 1;
    }  
  }
}
