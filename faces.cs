using System;
using System.Collections;
using Math3d;

namespace Mesh3d {
  public class faces {
    public int[] f;
    public int M,N,A;    
    public faces(int capacity) {
      f=new int[M=capacity];   
      A=1;   
    }
    public faces(faces fcs) {
      M=fcs.M;N=fcs.N;A=fcs.A;
      f=fcs.f.Clone() as int[];
    }
    public void Clear() {N=0;}
    public bool Alloc(int nm) {
      if(nm<=M) return true;
      Array.Resize(ref f,(M=nm));
      return true;
    }
    public bool append_space(int s) { return append_space(s,false);}               
    public bool append_space(int s,bool reserve) {            
      while(N+s>M) Alloc(2*M);
      if(!reserve) N+=s;
      return true;
    }
    public void append(int[] src,int index,int s) {
      int g=N;
      append_space(s);
      while(s-->0) f[g++]=src[index++];
    }
    public void Append(faces fcs,int p0) {
      int h=N;
      append_space(fcs.N);
      Array.Copy(fcs.f,0,f,h,fcs.N);
      if(p0==0) return;
      while(h<N) {
        int n=f[h++];
        while(n-->0) {
          int fh=f[h];
          f[h++]=fh+p0;
        }  
        h+=A;
      }
    }
    public void Join(ArrayList al) { 
      if(al==null) return;
      int min=3; // drop singular faces
      Hashtable ht=new Hashtable();
      for(int i=0;i<al.Count;i++) {
        ArrayList ali=al[i] as ArrayList;
        int j=(int)ali[0];
        foreach(object o in ali) ht[o]=j;
      }
      // joint points 
      int g=0,h=0;
      while(h<N) {
        int h2=h,g2=g;
        int n=f[h],d=0;
        f[g++]=f[h++];
        object o=ht[f[h]];
        int j2=o==null?-1:(int)o,j0=j2;
        f[g++]=j0>=0?j0:f[h];
        h++;
        for(int i=1;i<n;i++) {
          o=ht[f[h]];
          int j=o==null?-1:(int)o;
          if(j>=0&&(j==j2||i==n-1&&j==j0)) {
            d++;
          } else
            f[g++]=j>=0?j:f[h];
          h++;
        }
        bool add=g>g2+3;
        if(add) {
          for(int a=0;a<A;a++)
            f[g++]=f[h++];
          f[g2]=n-d;  
        } else {    
          h+=A;
          g=g2;
        }  
      }
      N=g;
    }
    public int Faces() {
      int n=0;
      for(int h=0;h<N;h+=f[h]+1+A)
        n++;
      return n;
    }  
    public int Triangles() {
      int n=0;
      for(int h=0;h<N;h+=f[h]+1+A)
        n+=f[h]-2;
      return n;
    }  
    public int Edges() {
      int n=0;
      for(int h=0;h<N;h+=f[h]+1+A)
        n+=f[h];
      return n;  
    }

// append 1 triangle (reversed)
    public bool Triangle(int a,int b,int c,POINTS mode) {
      if(!append_space(4+A))
        return false;
      int h=N-4-A;
      f[h++]=3;

      if(0!=(mode&POINTS.REVERSE)) {
        int r=a;a=c;c=r;
      }       
      f[h++]=a;f[h++]=b;f[h++]=c;
      return true;
    }

    public bool Quad(int a,int b,int c,int d,POINTS mode) {
      if(!append_space(5+A))
        return false;
      int h=N-5-A;  
      f[h++]=4;
  
      if(0!=(mode&POINTS.REVERSE)) {  
        f[h++]=d;f[h++]=c;f[h++]=b;f[h]=a;
      } else {
        f[h++]=a;f[h++]=b;f[h++]=c;f[h]=d;
      }  
      return true;
    }    
    public bool Face(int n,int[] p,int pi,POINTS mode) {
      return Face(0,n,p,pi,mode);
      /*if(!append_space(1+n+A))
        return false;
      int h=N-1-n-A;
      f[h++]=n;
      if(0!=(mode&POINTS.REVERSE)) {
        pi+=n-1;
        while(n-->0) f[h++]=p[pi--];
      } else
        while(n-->0) f[h++]=p[pi++];
      return true;*/
    }

    bool Face(int _base,int n,int[] p,int pi,POINTS mode) {
      if(!append_space(1+n+A))
        return false;
      int h=N-1-n-A;
      f[h++]=n;
      if(0!=(mode&POINTS.REVERSE)) {
        pi+=n-1;
        while(n-->0) f[h++]=_base+p[pi--];
      } else
        while(n-->0) f[h++]=_base+p[pi++];
      return true;
    }
// create triangles from point to pts, POINTS_CYCLE|POINTS_REVERSE
    bool Rays(int point,int npts,int pts,POINTS mode) {
      int fbase=N,h;
      int i,on;

      on=N;
      Alloc(N+(4+A)*(npts+(0!=(mode&POINTS.CYCLE)?1:0))); // only alloc (do not append)
      h=fbase;
      for(i=0;i<npts-1;i++)
        if(!Triangle(point,pts+i,pts+i+1,mode))
          goto err;
      if(0!=(mode&POINTS.CYCLE)) 
        if(!Triangle(point,pts+npts-1,pts,mode))
          goto err;
      return true;
     err:
      N=on;
      return false;
    }

// belt of quads
    bool Belt(int pts1,int npts,int pts2,POINTS mode) {
      int i,on;
      on=N;
      for(i=0;i<npts-1;i++)
        if(!Quad(pts1+i,pts1+i+1,pts2+i+1,pts2+i,mode))
          goto err;
      if(0!=(mode&POINTS.CYCLE))
        if(!Quad(pts1+npts-1,pts1,pts2,pts2+npts-1,mode))
          goto err;
      return true;
     err:
      N=on;
      return false;
    }

// create faces for 2D array mode POINTS_CYCLE|POINTS_CYCLE2|POINTS_REVERSE
    public bool Array2D(int pbase,int width,int height,POINTS mode) {
      int on,i;   
      on=N;
   
      for(i=0;i<height-1;i++) 
        if(!Belt(pbase+i*width,width,pbase+(i+1)*width,mode))
          goto err;
      if(0!=(mode&POINTS.CYCLE2))
        if(!Belt(pbase+i*width,width,pbase,mode)) 
          goto err;
      return true; 
     err:
      N=on;
      return false;
    }

// create faces for sphere
    bool Sphere(int pbase,int layers,int halfcircles,POINTS mode) {
      int on,size;
      POINTS reverse=mode&POINTS.REVERSE;

      if(layers<2) layers=2;
      if(halfcircles<3) halfcircles=3;
      size=(layers-2)*halfcircles;

      on=N;
      if(!Rays(pbase,halfcircles,pbase+1,POINTS.CYCLE|(POINTS.REVERSE^reverse))
          ||!Array2D(pbase+1,halfcircles,layers-2,POINTS.CYCLE|reverse)
          ||!Rays(pbase+1+size,halfcircles,pbase+1+size-halfcircles,POINTS.CYCLE|reverse)) {
        N=on;
        return false;
      }
      return true;
    }

    // create faces for rotoid with/out caps
    public bool Rotoid(int pbase,int layers,int radials,POINTS mode) {
      int on;
      POINTS reverse=mode&POINTS.REVERSE;

      on=N;
      if(0!=(mode&(POINTS.CAP|POINTS.IMPLICIT_CAP))) {
        if(!Rays(pbase,radials,pbase+1,POINTS.CYCLE|(POINTS.REVERSE^reverse)))
          goto error;
        else {
          pbase++;
          if(0!=(mode&POINTS.CAP)) layers--;
        }
      }
      if(0!=(mode&POINTS.CAP2)) layers--;
      if(!Array2D(pbase,radials,layers,POINTS.CYCLE|reverse))
        goto error;
      pbase+=layers*radials;
      if(0!=(mode&(POINTS.CAP2|POINTS.CAP2))
           &&!Rays(pbase,radials,pbase-radials,POINTS.CYCLE|reverse)) 
        goto error; 
      return true;
     error:
      N=on;
      return false;
    }
// create faces for box
    public bool Box(int pbase,int xres,int yres,int zres,POINTS mode) {
      int pn=2*(yres*(xres-1+zres-1)+(xres-2)*(zres-2));
      int fn=2*((yres-1)*(xres-1+zres-1)+(xres-1)*(zres-1));
      int i,k,j,p,l,p2;
      int f0,f1,f2,f3,d1,d3;

      if(!Alloc(N+(5+A)*fn)) {
        return false;
      }
      Array2D(pbase,xres,zres,mode^POINTS.REVERSE);
      p=pbase;
      for(k=1;k<yres;k++) {
        l=k>1?2*(xres-1+zres-1):xres*zres;
        p2=p+l;

        f0=p;f1=f0+1;f2=p2;f3=f2+1;
        d1=1;d3=1;
        for(i=1;i<xres;i++) {
          Quad(f0,f1,f3,f2,mode);
          f0=f1;f1+=d1;f2=f3;f3+=d3;
        }

        f0=p2-xres;f1=f0+1;f2=p2+(k<yres-1?xres+2*(zres-2):xres*(zres-1));f3=f2+1;
        for(i=1;i<xres;i++) {
          Quad(f0,f1,f3,f2,mode^POINTS.REVERSE);
          f0=f1;f1+=d1;f2=f3;f3+=d3;
        } 

        f0=p;f1=f0+xres;f2=p2;f3=f2+xres;
        d1=k>1?2:xres;d3=k<yres-1?2:xres;
        for(j=1;j<zres;j++) { 
          Quad(f0,f1,f3,f2,mode^POINTS.REVERSE);
          f0=f1;f1+=d1;f2=f3;f3+=d3;
        } 

        f0=p+xres-1;f2=p2+xres-1;
        for(j=1;j<zres;j++) { 
          d1=k>1&&j<zres-1?2:xres;d3=k<yres-1&&j<zres-1?2:xres;
          f1=f0+d1;f3=f2+d3;
          Quad(f0,f1,f3,f2,mode);
          f0=f1;f2=f3;
        } 

        p=p2;
      }
      Array2D(pbase+pn-xres*zres,xres,zres,mode);
      return true;
    }    
    public bool Tetrahedron(int pbase,POINTS mode) {
      Triangle(pbase,pbase+1,pbase+2,mode);
      Triangle(pbase,pbase+2,pbase+3,mode);
      Triangle(pbase+1,pbase,pbase+3,mode);
      Triangle(pbase+2,pbase+1,pbase+3,mode);
      return true;
    }
    static readonly int[] HexahedronEdges=new int[]{
        0,1,2,3, 0,4,5,1, 1,5,6,2, 2,6,7,3, 3,7,4,0, 4,7,6,5
      };

    public bool Hexahedron(int pbase,POINTS mode) {
      for(int i=0;i<6;i++)
        Face(pbase,4,HexahedronEdges,4*i,mode);
      return true;
    }
    static readonly int[] OctahedronEdges=new int[] {
      0,1,2, 0,2,3, 0,3,4, 0,4,1, 1,4,5, 2,1,5, 3,2,5, 4,3,5
    };

    public bool Octahedron(int pbase,POINTS mode) {
      for(int i=0;i<8;i++) 
        Face(pbase,3,OctahedronEdges,3*i,mode);
      return true;
    }
  
    static readonly int[] DodecahedronEdges=new int[] {
      0,1,2,3,4
      ,1,0,5,10,6, 2,1,6,11,7, 3,2,7,12,8, 4,3,8,13,9, 0,4,9,14,5
      ,5,14,19,15,10, 6,10,15,16,11, 7,11,16,17,12, 8,12,17,18,13, 9,13,18,19,14
      ,15,19,18,17,16
    };

    public bool Dodecahedron(int pbase,POINTS mode) {
      for(int i=0;i<12;i++) 
        Face(pbase,5,DodecahedronEdges,5*i,mode);
      return true;
    }

    static readonly int[] IcosahedronEdges={
      0,1,2, 0,2,3, 0,3,4, 0,4,5, 0,5,1
      ,2,1,6, 3,2,7, 4,3,8, 5,4,9, 1,5,10
      ,6,7,2, 7,8,3, 8,9,4, 9,10,5, 10,6,1
      ,6,10,11, 7,6,11, 8,7,11, 9,8,11, 10,9,11
    };

    public bool Icosahedron(int pbase,POINTS mode) {
      for(int i=0;i<20;i++) 
        Face(pbase,3,IcosahedronEdges,3*i,mode);
      return true;      
    }
    static readonly int[] HexCylinderEdges={
      6,5,4,3,2,1,0
      ,4,0,1,7,6, 4,1,2,8,7, 4,2,3,9,8, 4,3,4,10,9, 4,4,5,11,10, 4,5,0,6,11
      ,6,6,7,8,9,10,11
    };
    public bool HexCylinder(int pbase,POINTS mode) {
      int h=0;
      for(int i=8;i-->0;h+=HexCylinderEdges[h]) 
        Face(pbase,HexCylinderEdges[h],HexCylinderEdges,h,mode);
      return true;
    }    
    public void Reverse() {
      for(int h=0;h<N;h+=A) {
        int i,h2=h+1,hr=h+f[h];
        h=hr+1;
        while(h2<hr) {
          i=f[h2];f[h2++]=f[hr];f[hr--]=i;
        }  
      }
    }
    
    public bool Triangulation() {
      int i,a,n,t,h,g;
      int[] t0;

      n=Triangles();
      if(N==n*(4+A)) return false;
      t0=new int[n*(4+A)];
      t=0;
      for(h=0;h<N;h+=A) {
        for(i=2;i<f[h];i++) {
          t0[t++]=3;
          t0[t++]=f[h+1];
          t0[t++]=f[h+i];
          t0[t++]=f[h+i+1];                    
          for(a=A,g=h+f[h]+1;a-->0;t0[t++]=f[g++]);
        }  
        h+=f[h]+1;
      }
      f=t0;
      M=N=f.Length;
      return true;
    }
    public void Colorize(int[] color) {
      if(A<1) return;
      int colors=color.Length;
      int i=0;
      for(int h=0;h<N;h+=A) {
        h+=f[h]+1;
        Color(ref f[h],color[i++%colors]);
      }
    }
    public static int RGBd(int color,double i) {
      if(i<0) return 0;
      double r=i*(color&255),g=i*((color>>8)&255),b=i*((color>>16)&255);
      if(r>255) r=255;if(g>255) g=255;if(b>255) b=255;
      return ((int)r)|(((int)g)<<8)|(((int)b)<<16);
    }
    
    public static int RGB2int(double[]rgb,int i) {
      return RGB2int(rgb[i],rgb[i+1],rgb[i+2]);
    }
    public static int RGB2int(double r,double g,double b) {
      byte r2=(byte)(((int)(r*65535))>>8);
      byte g2=(byte)(((int)(g*65535))>>8);
      byte b2=(byte)(((int)(b*65535))>>8);
      return r2|(g2<<8)|(b2<<16);
      
    }

    public static double[] PaletteReverse(double[] p,bool clone) {
      if(clone) p=p.Clone() as double[];
      int i,j,n=p.Length/4;
      for(i=0,j=(n-1)*4;i<j;i+=4,j-=4) {
        double x;
        x=p[i];p[i]=1-p[j];p[j]=1-x;
        x=p[i+1];p[i+1]=p[j+1];p[j+1]=x;
        x=p[i+2];p[i+2]=p[j+2];p[j+2]=x;
        x=p[i+3];p[i+3]=p[j+3];p[j+3]=x;
      }
      if(i==j) p[i]=1-p[i];
      return p;
    }
    public static readonly double[] PalGray=new double[] {0,0,0,0,1,1,1,1};
    public static readonly double[] PalRedBlue=new double[] {0,0,0,0 ,.1,0,0,1 ,.5,1,0,0 ,1,1,1,1};
    public static readonly double[] PalRainbow=new double[] {0,0,0,0 ,1/7.0,1,0,0 ,2/7.0,1,1,0  ,3/7.0,0,1,0 ,4/7.0,0,1,1 ,5/7.0,0,0,1 ,6/7.0,1,0,1 ,7/7.0,1,1,1};
    public static readonly double[] PalLayers=new double[] {  0,0,0,0 ,36/255.0,1,0,0 ,41/255.0,0,0,0 ,73/255.0,1,1,0 ,78/255.0,0,0,0 ,109/255.0,0,1,0
        ,114/255.0,0,0,0 ,146/255.0,0,1,1 ,151/255.0,0,0,0 ,182/255.0,0,0,1,187/255.0,0,0,0 ,219/255.0,1,0,1 ,224/255.0,0,0,0 ,1,1,1,1};

    public static double[] Palette() {
      return new double[] {0,0,0,0,1,1,1,1};
    }
    public static double[] Palette(int color) {
      int r=color&255,g=(color>>8)&255,b=(color>>16)&255,sum=r+g+b;
      if(r==g&&g==b) return Palette();
      return new double[] {0,0,0,0,0.5,r/255.0,g/255.0,b/255.0,1,1,1,1};      
    }
    public static int PaletteColor(int colors,double[] pal,double d) {
      int i=0,a=4*(colors-1);
      if(d<=pal[i]) return RGB2int(pal,i+1);
      if(d>=pal[a]) return RGB2int(pal,a+1);     
     loop:
      if(i+4==a) {
        double wi=(pal[a]-d)/(pal[a]-pal[i]),wa=1-wi;
        return RGB2int(wi*pal[i+1]+wa*pal[a+1],wi*pal[i+2]+wa*pal[a+2],wi*pal[i+3]+wa*pal[a+3]);
      }
      int m=(i+a)/2&~3;
      if(pal[m]<d) i=m;
      else if(pal[m]>d) a=m;
      else return RGB2int(pal,m+1);
      goto loop;
    }
    public void ColorizeRandom(int colors,double[] palette) {
      if(A<1) return;
      Random rnd=new Random();
      for(int h=0;h<N;h+=A) {
        h+=f[h]+1;
        Color(ref f[h],PaletteColor(colors,palette,rnd.NextDouble()));
      }      
    }
    public const int White=0xffffff;
    public static void Color(ref int value,int color) { value=(value&~White)|(color&White);}
    public const int HideMask=1<<24;
    public static bool Hidden(int i) { return 0!=(i&HideMask);}
    public void Hide(ref int value,bool hide) { if(hide) value|=HideMask;else value&=~HideMask;}
    public void Hide(bool hide) {
      if(A<1) return;      
      for(int h=0;h<N;) {
        int n=f[h++];
        h+=n;
        Hide(ref f[h],hide);
        h+=A;
      }
    }
    public void HideInvert() {
      if(A<1) return;      
      for(int h=0;h<N;) {
        int n=f[h++];
        h+=n;
        f[h]^=HideMask;
        h+=A;
      }
    }
    public void HideNext(bool show) {
      bool ff=false;
      for(int h=0;h<N;) {
        int n=f[h++];
        h+=n;
        if(Hidden(f[h])==show) {
          if(ff) {
            ff=false;
            f[h]^=HideMask;
          }
        } else ff=true;
        h+=A;
     }
    }
    public void Hide(int a,int b,bool hide) {
      if(A<1) return;
      else if(a<1) Hide(true);
      else if(a>=b) Hide(false);
      else {
        for(int h=0,x=0;h<N;) {
          int n=f[h++];
          h+=n;
          if(Hidden(f[h])!=hide) {
            if(x<a) f[h]^=HideMask;
            x++;if(x>=b) x=0;
          }
          h+=A;
        }
      }
    }
    public void Hide(bool[] hide,int mode) { // 0 get,1 set,2 or
      if(hide==null) {
        if(mode==1) Hide(false);
        return;
      }
      for(int h=0,i=0;h<N;i++) {
        int n=f[h++];
        h+=n;
        if(mode==2) {
          if(i<hide.Length&&hide[i]) f[h]|=HideMask;
        } else if(mode==1)
          Hide(ref f[h],i<hide.Length&&hide[i]);
        else
          if(i<hide.Length) hide[i]=Hidden(f[h]);
        h+=A;
      }
    }
    public void glDraw(double[] colors,bool hide) {
      bool o=false;
      int n2=0,ci=0;
      if(A<1) hide=false;
      for(int h=0;h<N;) {
        int n=f[h++];        
        if(hide&&Hidden(f[h+n])) {h+=n;goto skip;}
        if(o&&n2!=n) {
          opengl.glEnd();
          o=false;
        }
        if(!o) {
          switch(n) {
           case 3:opengl.glBegin(GLEnum.GL_TRIANGLES);n2=n;o=true;break;
           case 4:opengl.glBegin(GLEnum.GL_QUADS);n2=n;o=true;break;
           default:opengl.glBegin(GLEnum.GL_POLYGON);break;
          }                  
        }
        if(colors!=null) {
          opengl.glColor3d(colors[ci],colors[ci+1],colors[ci+2]);
          ci+=3;
          if(ci>=colors.Length) ci=0;
        } else {  
          opengl.glColor(f[h+n]);
        }  
        while(n-->0)
          opengl.glArrayElement(f[h++]);
        if(!o) opengl.glEnd();
       skip:
        h+=A;
      }
      if(o) opengl.glEnd();  
    }
  }
}
