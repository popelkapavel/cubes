using System;
using System.IO;
using Mesh3d;

namespace Math3d {
  public enum bit_op:byte {
    Zero=0
    ,One=1
    ,Not=2
    ,And=3
    ,Or=4
    ,Xor=5
    ,Sub=6
    ,Copy=7
    ,Invert=8
    ,None=9
  }
  public delegate double Delegate3DToD(double x,double y,double z);
  public delegate bool DelegateDToB(double x);
  public delegate void Delegate3D(bit3d b3d,bit_op op,int x,int y,int z,object param);
  public delegate void DelegateLinePoint(StreamWriter sw,double x,double y,double z);
  public delegate void DelegateLineQuad(StreamWriter sw,int a,int b,int c,int d);
  public class bit3d {
     public int dx,dy,dz;
     uint[] bit;
     int dpl; //dwords per line
          
     public bit3d() {}
     public static int bpl(int dx) {
       return (((dx+31)&~31)>>5);
     }
     public static int size(int dx,int dy,int dz) {
       return (((dx+31)&~31)>>5)*dy*dz;
     }
     void offset_mask(out int offset,out uint mask,int x,int y,int z) {
       mask=(uint)(1<<(x&31));
       offset=dpl*(y+dy*z)+(x>>5);
     }
     public void alloc(int dx) {alloc(dx,dx,dx);}
     public void alloc(int dx,int dy,int dz) {
       bit=new uint[size(dx,dy,dz)];
       this.dx=dx;this.dy=dy;this.dz=dz;
       dpl=((dx+31)>>5);
     }
     public bit3d Clone() {
       bit3d n=new bit3d();
       n.alloc(dx,dy,dz);
       n.operation(bit_op.Copy,0,0,0,this,0,0,0,dx,dy,dz);
       return n;
     }
     public void swap(bit3d dst) { 
       int r;
       r=dx;dx=dst.dx;dst.dx=r;
       r=dy;dy=dst.dy;dst.dy=r;
       r=dz;dz=dst.dz;dst.dz=r;
       r=dpl;dpl=dst.dpl;dst.dpl=r;
       uint[] r2;
       r2=bit;bit=dst.bit;dst.bit=r2;
     }
     public void chset(int x,int y,int z,bool bit) {
       if(x>=0&&y>=0&&z>=0&&x<dx&&y<dy&&z<dz)
          set(x,y,z,bit);
     }
     public void chset(int sx,int sy,int sz,int dx,int dy,int dz,bool bit) {
       if(dx<0) dx=-dx;if(dy<0) dy=-dy;if(dz<0) dz=-dz;
       int x=sx-dx,y=sy-dy,z=sz-dz,x2=sx+dx,y2=sy+dy,z2=sz+dz;
       bool xm=x>=0&&x<this.dx,ym=y>=0&&y<this.dy,zm=z>=0&&z<this.dz;
       bool xp=dx>0&&x2>=0&&x2<this.dx,yp=dy>0&&y2>=0&&y2<this.dy,zp=dz>0&&z2>=0&&z2<this.dz;
       if(xm) {
         if(ym) {
           if(zm) set(x,y,z,bit);
           if(zp) set(x,y,z2,bit);
         }
         if(yp) {
           if(zm) set(x,y2,z,bit);
           if(zp) set(x,y2,z2,bit);
         }
       }
       if(xp) {
         if(ym) {
           if(zm) set(x2,y,z,bit);
           if(zp) set(x2,y,z2,bit);
         }
         if(yp) {
           if(zm) set(x2,y2,z,bit);
           if(zp) set(x2,y2,z2,bit);
         }
       }
     }
     public void set(int x,int y,int z,bool bit) {
       int offset;
       uint mask;
       offset_mask(out offset,out mask,x,y,z);
       this.bit[offset]=(this.bit[offset]&~mask)|(bit?mask:0);
     }
     public void sety(int x,int y0,int y1,int z,bool bit) {
       int offset;
       uint mask;
       if(y0>y1) { int y=y0;y0=y1;y1=y;}
       if(y0>=dz||y1<0) return;
       if(y0<0) y0=0;
       if(y1>=dy) y1=dy-1;
       offset_mask(out offset,out mask,x,y0,z);
       int size=bpl(dx);
       while(y0++<=y1) {
         this.bit[offset]=(this.bit[offset]&~mask)|(bit?mask:0);
         offset+=size;
       }
     }
     public void setz(int x,int y,int z0,int z1,bool bit) {
       int offset;
       uint mask;
       if(z0>z1) { int z=z0;z0=z1;z1=z;}
       if(z0>=dz||z1<0) return;
       if(z0<0) z0=0;
       if(z1>=dz) z1=dz-1;
       offset_mask(out offset,out mask,x,y,z0);
       int size=bpl(dx)*dy;
       while(z0++<=z1) {
         this.bit[offset]=(this.bit[offset]&~mask)|(bit?mask:0);
         offset+=size;
       }
     }
     public bool get(int x,int y,int z) {
       int offset;
       uint mask;
       offset_mask(out offset,out mask,x,y,z);
       return 0!=(bit[offset]&mask);
     }
     public int get3(int op,int mode,int x,int y,int z) {
       bool sum=op==2,and=op==1,or=!and&&!sum,b2,b1,b3,b4;
       int offset,offsetp,offsetn,bpy=bpl(dx),bpz=bpy*dy,s=0,fast=and?0:1;
       uint mask,maskp,maskn;
       offset_mask(out offset,out mask,x,y,z);       
       b1=0!=(bit[offset-bpy]&mask);
       b2=0!=(bit[offset+bpy]&mask);
       if(and&&(!b1||!b2)||or&&(b1||b2)) return fast;
       if(sum) s+=(b1?1:0)+(b2?1:0);
       b1=0!=(bit[offset-bpz]&mask);
       b2=0!=(bit[offset+bpz]&mask);
       if(and&&(!b1||!b2)||or&&(b1||b2)) return fast;
       if(sum) s+=(b1?1:0)+(b2?1:0);
       offset_mask(out offsetn,out maskn,x-1,y,z);
       offset_mask(out offsetp,out maskp,x+1,y,z);
       b1=0!=(bit[offsetn]&maskn);
       b2=0!=(bit[offsetp]&maskp);
       if(and&&(!b1||!b2)||or&&(b1||b2)) return fast;
       if(sum) s+=(b1?1:0)+(b2?1:0);
       if(mode<1) return sum?s:1-fast;

       b1=0!=(bit[offset-bpy-bpz]&mask);
       b2=0!=(bit[offset-bpy+bpz]&mask);
       b3=0!=(bit[offset+bpy-bpz]&mask);
       b4=0!=(bit[offset+bpy+bpz]&mask);
       if(and&&(!b1||!b2||!b3||!b4)||or&&(b1||b2||b3||b4)) return fast;
       if(sum) s+=(b1?1:0)+(b2?1:0)+(b3?1:0)+(b4?1:0);
       b1=0!=(bit[offsetn-bpy]&maskn);
       b2=0!=(bit[offsetn+bpy]&maskn);
       b3=0!=(bit[offsetn-bpz]&maskn);
       b4=0!=(bit[offsetn+bpz]&maskn);
       if(and&&(!b1||!b2||!b3||!b4)||or&&(b1||b2||b3||b4)) return fast;
       if(sum) s+=(b1?1:0)+(b2?1:0)+(b3?1:0)+(b4?1:0);
       b1=0!=(bit[offsetp-bpy]&maskp);
       b2=0!=(bit[offsetp+bpy]&maskp);
       b3=0!=(bit[offsetp-bpz]&maskp);
       b4=0!=(bit[offsetp+bpz]&maskp);
       if(and&&(!b1||!b2||!b3||!b4)||or&&(b1||b2||b3||b4)) return fast;
       if(sum) s+=(b1?1:0)+(b2?1:0)+(b3?1:0)+(b4?1:0);
       if(mode<2) return sum?s:1-fast;

       b1=0!=(bit[offsetp-bpy-bpz]&maskp);
       b2=0!=(bit[offsetp-bpy+bpz]&maskp);
       b3=0!=(bit[offsetp+bpy-bpz]&maskp);
       b4=0!=(bit[offsetp+bpy+bpz]&maskp);
       if(and&&(!b1||!b2||!b3||!b4)||or&&(b1||b2||b3||b4)) return fast;
       if(sum) s+=(b1?1:0)+(b2?1:0)+(b3?1:0)+(b4?1:0);
       b1=0!=(bit[offsetn-bpy-bpz]&maskn);
       b2=0!=(bit[offsetn-bpy+bpz]&maskn);
       b3=0!=(bit[offsetn+bpy-bpz]&maskn);
       b4=0!=(bit[offsetn+bpy+bpz]&maskn);
       if(and&&(!b1||!b2||!b3||!b4)||or&&(b1||b2||b3||b4)) return fast;
       if(sum) s+=(b1?1:0)+(b2?1:0)+(b3?1:0)+(b4?1:0);
       return sum?s:1-fast;
     }
     public bool get2(int x,int y,int z) {
      if(x<0||x>=dx||y<0||y>=dy||z<0||z>=dz)
        return false;     
       int offset;
       uint mask;
       offset_mask(out offset,out mask,x,y,z);
       return 0!=(bit[offset]&mask);
     }
     public bool flip(int x,int y,int z) {
      if(x<0||x>=dx||y<0||y>=dy||z<0||z>=dz)
        return false;     
       int offset;
       uint mask;
       offset_mask(out offset,out mask,x,y,z);
       return ((bit[offset]^=mask)&mask)!=0;
     }
     public void clear(bool b) {
       uint x=b?0xffffffff:0x0;
       for(int i=0;i<bit.Length;i++)
         bit[i]=x;       
     }
     public void not() {
       for(int i=0;i<bit.Length;i++)
         bit[i]^=0xffffffff;
     }
     public void box(bit_op op,int sx,int sy,int sz,int wx,int wy,int wz,bool hollow) {
       if(!hollow) {
         for(int x=0;x<wx;x++)
           for(int y=0;y<wy;y++) {
               operation(op,sx+x,sy+y,sz,false);
               if(wz>1) operation(op,sx+x,sy+y,sz+wz-1,false);
             }
         if(wz<2) return;
         sz++;wz-=2;
       }
       for(int x=0;x<wx;x++)
         for(int z=0;z<wz;z++) {
             operation(op,sx+x,sy,sz+z,false);
             if(wy>1) operation(op,sx+x,sy+wy-1,sz+z,false);
           }
       if(wy<2) return;
       sy++;wy-=2;
       for(int y=0;y<wy;y++)
         for(int z=0;z<wz;z++) {
             operation(op,sx,sy+y,sz+z,false);
             if(wx>1) operation(op,sx+wx-1,sy+y,sz+z,false);
           }
     }
     public void cylinder(bit_op op,int axis,int x,int y,int z,int x2,int y2,int z2,bool hollow) {
        sort(ref x,ref x2);sort(ref y,ref y2);sort(ref z,ref z2);
        if(axis==0) {
          if(x2<0||x>=dz) return;
          if(y==y2||z==z2) {
            if(y>=0&&y<dy&&z>=0&&z<dz) box(op,x,y,z,x2-x+1,y2-y+1,z2-z+1,true);
            return;
          }
          if(x<0) x=0;if(x2>=dx) x2=dx-1;
          int ry=y2-y,rz=z2-z,ry2=ry*ry,rz2=rz*rz,r2;
          //y=2*y+1;z=2*z+1;y2=2*y2+1;z2=2*z2+1;
          int sy=y+y2+1,sz=z+z2+1;
          r2=(ry)*(ry+1)*(rz)*(rz+1);
          range(ref y,ref y2,0,dy-1);
          range(ref z,ref z2,0,dz-1);
          for(int iz=z;iz<=z2;iz++)
            for(int iy=y;iy<=y2;iy++) {
              int dy2=2*iy+1-sy,dz2=2*iz+1-sz;
              if(rz2*dy2*dy2+ry2*dz2*dz2<=r2) for(int ix=x;ix<=x2;ix++)
                operation(op,ix,iy,iz,true);
            }
        } else if(axis==1) {
          if(y2<0||y>=dy) return;
          if(x==x2||z==z2) {
            if(x>=0&&x<dx&&z>=0&&z<dz) box(op,x,y,z,x2-x+1,y2-y+1,z2-z+1,true);
            return;
          }
          if(y<0) y=0;if(y2>=dy) y2=dy-1;
          int rx=x2-x,rz=z2-z,rx2=rx*rx,rz2=rz*rz,r2;
          //x=2*x+1;z=2*z+1;x2=2*x2+1;z2=2*z2+1;
          int sx=x+x2+1,sz=z+z2+1;
          r2=(rx)*(rx+1)*(rz)*(rz+1);
          range(ref x,ref x2,0,dx-1);
          range(ref z,ref z2,0,dz-1);
          for(int iz=z;iz<=z2;iz++)
            for(int ix=x;ix<=x2;ix++) {
              int dx2=2*ix+1-sx,dz2=2*iz+1-sz;
              if(rz2*dx2*dx2+rx2*dz2*dz2<=r2) for(int iy=y;iy<=y2;iy++)
                operation(op,ix,iy,iz,true);
            }
        } else {
          if(z2<0||z>=dz) return;
          if(x==x2||y==y2) {
            if(x>=0&&x<dx&&y>=0&&x<dy) box(op,x,y,z,x2-x+1,y2-y+1,z2-z+1,true);
            return;
          }
          if(z<0) z=0;if(z2>=dz) z2=dz-1;
          int rx=x2-x,ry=y2-y,rx2=rx*rx,ry2=ry*ry,r2;
          //x=2*x+1;y=2*y+1;x2=2*x2+1;y2=2*y2+1;
          int sx=x+x2+1,sy=y+y2+1;
          r2=(rx)*(rx+1)*(ry)*(ry+1);
          range(ref x,ref x2,0,dx-1);
          range(ref y,ref y2,0,dy-1);
          for(int iy=y;iy<=y2;iy++)
            for(int ix=x;ix<=x2;ix++) {
              int dx2=2*ix+1-sx,dy2=2*iy+1-sy;
              if(ry2*dx2*dx2+rx2*dy2*dy2<=r2) for(int iz=z;iz<=z2;iz++)
                operation(op,ix,iy,iz,true);
            }
        }
     }
     public void cone(bit_op op,int axis,int x,int y,int z,int x2,int y2,int z2,bool hollow) {
       if(axis==0) {
         if(x==x2) {
           cylinder(op,axis,x,y,z,x2,y2,z2,hollow);
           return;
         } 
         int ax=x2<x?-1:1,dx=Math.Abs(x2-x),sy=y+y2,dy=Math.Abs(y2-y),sz=z+z2,dz=Math.Abs(z2-z);         
         for(int i=0;i<=dx;i++)
           cylinder(op,axis,x2-ax*i,(sy-i*dy/dx)/2,(sz-i*dz/dx)/2,x2-ax*i,(sy+i*dy/dx+1)/2,(sz+i*dz/dx+1)/2,hollow);         
       } else if(axis==1) {
         if(y==y2) {
           cylinder(op,axis,x,y,z,x2,y2,z2,hollow);
           return;
         } 
         int ay=y2<y?-1:1,dy=Math.Abs(y2-y),sz=z+z2,dz=Math.Abs(z2-z),sx=x+x2,dx=Math.Abs(x2-x);         
         for(int i=0;i<=dy;i++)
           cylinder(op,axis,(sx-i*dx/dy)/2,y2-ay*i,(sz-i*dz/dy)/2,(sx+i*dx/dy+1)/2,y2-ay*i,(sz+i*dz/dy+1)/2,hollow);         
       } else {
         if(z==z2) {
           cylinder(op,axis,x,y,z,x2,y2,z2,hollow);
           return;
         } 
         int az=z2<z?-1:1,dz=Math.Abs(z2-z),sy=y+y2,dy=Math.Abs(y2-y),sx=x+x2,dx=Math.Abs(x2-x);         
         for(int i=0;i<=dz;i++)
           cylinder(op,axis,(sx-i*dx/dz)/2,(sy-i*dy/dz)/2,z2-az*i,(sx+i*dx/dz+1)/2,(sy+i*dy/dz+1)/2,z2-az*i,hollow);         
       }
     }

     public void sphere(bit_op op,int x,int y,int z,int x2,int y2,int z2,bool hollow) {
        sort(ref x,ref x2);sort(ref y,ref y2);sort(ref z,ref z2);
        if(z2<0||z>=dz) return;
        if(x==x2||y==y2) box(op,x,y,z,x2-x+1,y2-y+1,z2-z+1,true);
        if(z<0) z=0;if(z2>=dz) z2=dz-1;
        int rx=x2-x,ry=y2-y,rz=z2-z,rx2=rx*rx,ry2=ry*ry,rz2=rz*rz,r2;
        //x=2*x+1;y=2*y+1;x2=2*x2+1;y2=2*y2+1;
        int sx=x+x2+1,sy=y+y2+1,sz=z+z2+1;
        r2=(rx)*(rx+1)*(ry)*(ry+1)*(rz)*(rz+1);
        range(ref x,ref x2,0,dx-1);
        range(ref y,ref y2,0,dy-1);
        range(ref z,ref z2,0,dz-1);
        for(int iy=y;iy<=y2;iy++)
          for(int ix=x;ix<=x2;ix++) {
            int dx2=2*ix+1-sx,dy2=2*iy+1-sy;
              for(int iz=z;iz<=z2;iz++) {
                int dz2=2*iz+1-sz;
                if(ry2*rz2*dx2*dx2+rx2*rz2*dy2*dy2+rx2*ry2*dz2*dz2<=r2) 
                  operation(op,ix,iy,iz,true);
              }
                
          }
     }
     public long bits() {
       //int[] b16=new int[] {0,1,1,2,1,2,2,3,1,2,2,3,2,3,3,4};
       long p=0; 
       for(int i=0;i<bit.Length;i++) {
         uint u=bit[i],m=1,up=0;
         while(m!=0) {
           if(0!=(u&m)) up++;
           m<<=1; 
         }
         p+=up;
       }
       return p;
     }
     public void boundary(int mode) {
       bit3d tmp=new bit3d();
       int x,y,z;
       
       tmp.alloc(dx+2,dy+2,dz+2);
       tmp.copy(1,1,1,this,0,0,0,dx,dy,dz);
       clear(false);
       for(z=1;z<=dz;z++)
         for(y=1;y<=dy;y++)
           for(x=1;x<=dx;x++) {
             if(!tmp.get(x,y,z)) continue;
             bool o=0!=tmp.get3(1,0,x,y,z);//tmp.get(x,y,z-1)&&tmp.get(x,y,z+1)&&tmp.get(x,y-1,z)&&tmp.get(x,y+1,z)&&tmp.get(x-1,y,z)&&tmp.get(x+1,y,z);
             if(o&&mode>0) {
               o=tmp.get(x,y-1,z-1)&&tmp.get(x,y-1,z+1)&&tmp.get(x,y+1,z-1)&&tmp.get(x,y+1,z+1);
               if(o) o=tmp.get(x-1,y,z-1)&&tmp.get(x-1,y,z+1)&&tmp.get(x+1,y,z-1)&&tmp.get(x+1,y,z+1);
               if(o) o=tmp.get(x-1,y-1,z)&&tmp.get(x-1,y+1,z)&&tmp.get(x+1,y-1,z)&&tmp.get(x+1,y+1,z);
               if(o&&mode>1) {
                 o=tmp.get(x-1,y-1,z-1)&&tmp.get(x-1,y+1,z-1)&&tmp.get(x+1,y-1,z-1)&&tmp.get(x+1,y+1,z-1);
                 o=tmp.get(x-1,y-1,z+1)&&tmp.get(x-1,y+1,z+1)&&tmp.get(x+1,y-1,z+1)&&tmp.get(x+1,y+1,z+1);
               }
             }
             if(!o) 
               set(x-1,y-1,z-1,true);
           }
       
     }
     public void expand(bool impand,int mode) { expand(impand,mode,0,0,0,dx,dy,dz);}
     public void expand(bool impand,int mode,int sx,int sy,int sz,int wx,int wy,int wz) {
       bit3d tmp=new bit3d();
       int x,y,z;
       tmp.alloc(wx+2,wy+2,wz+2);
       tmp.clear(impand);
       tmp.operation(impand?bit_op.Invert:bit_op.Copy,1,1,1,this,sx,sy,sz,wx,wy,wz);
       bool value=!impand;
       for(z=1;z<=wz;z++)
         for(y=1;y<=wy;y++)
           for(x=1;x<=wx;x++) {
             if(tmp.get(x,y,z)) continue;
             bool o=0!=tmp.get3(0,mode,x,y,z);//  tmp.get(x,y,z-1)||tmp.get(x,y,z+1)||tmp.get(x,y-1,z)||tmp.get(x,y+1,z)||tmp.get(x-1,y,z)||tmp.get(x+1,y,z);
             /*if(!o&&mode>0) {
               o=tmp.get(x,y-1,z-1)||tmp.get(x,y-1,z+1)||tmp.get(x,y+1,z-1)||tmp.get(x,y+1,z+1);
               if(!o) o=tmp.get(x-1,y,z-1)||tmp.get(x-1,y,z+1)||tmp.get(x+1,y,z-1)||tmp.get(x+1,y,z+1);
               if(!o) o=tmp.get(x-1,y-1,z)||tmp.get(x-1,y+1,z)||tmp.get(x+1,y-1,z)||tmp.get(x+1,y+1,z);
               if(!o&&mode>1) {
                 o=tmp.get(x-1,y-1,z-1)||tmp.get(x-1,y+1,z-1)||tmp.get(x+1,y-1,z-1)||tmp.get(x+1,y+1,z-1);
                 o=tmp.get(x-1,y-1,z+1)||tmp.get(x-1,y+1,z+1)||tmp.get(x+1,y-1,z+1)||tmp.get(x+1,y+1,z+1);
               }
             }*/
             if(o) set(sx+x-1,sy+y-1,sz+z-1,value);
           }        
     }
     /*public void impand(int mode) { impand(mode,1,1,1,dx,dy,dz);}
     public void impand(int mode,int sx,int sy,int sz,int wx,int wy,int wz) {
       bit3d tmp=new bit3d();
       int x,y,z;
       tmp.alloc(wx+2,wy+2,wz+2);
       tmp.clear(false);
       tmp.operation(bit_op.Copy,1,1,1,this,sx,sy,sz,wx,wy,wz);
       for(z=1;z<wz;z++)
         for(y=1;y<wy;y++)
           for(x=1;x<wx;x++) {
             if(!tmp.get(x,y,z)) continue;
             bool o=tmp.get(x,y,z-1)&&tmp.get(x,y,z+1)&&tmp.get(x,y-1,z)&&tmp.get(x,y+1,z)&&tmp.get(x-1,y,z)&&tmp.get(x+1,y,z);
             if(o&&mode>0) {
               o=tmp.get(x,y-1,z-1)&&tmp.get(x,y-1,z+1)&&tmp.get(x,y+1,z-1)&&tmp.get(x,y+1,z+1);
               if(o) o=tmp.get(x-1,y,z-1)&&tmp.get(x-1,y,z+1)&&tmp.get(x+1,y,z-1)&&tmp.get(x+1,y,z+1);
               if(o) o=tmp.get(x-1,y-1,z)&&tmp.get(x-1,y+1,z)&&tmp.get(x+1,y-1,z)&&tmp.get(x+1,y+1,z);
               if(o&&mode>1) {
                 o=tmp.get(x-1,y-1,z-1)&&tmp.get(x-1,y+1,z-1)&&tmp.get(x+1,y-1,z-1)&&tmp.get(x+1,y+1,z-1);
                 o=tmp.get(x-1,y-1,z+1)&&tmp.get(x-1,y+1,z+1)&&tmp.get(x+1,y-1,z+1)&&tmp.get(x+1,y+1,z+1);
               }
             }
             if(!o) set(sx+x-1,sy+y-1,sz+z-1,false);
           }
     }*/
     public void extrude(bool or,int zsrc,int sx,int sy,int sz,int wx,int wy,int wz) {
       if(zsrc<0||zsrc>=dz||wz==1) return;
       for(int x=0;x<wx;x++)  
         for(int y=0;y<wy;y++) {
           bool b=get(sx+x,sy+y,zsrc);
           for(int z=0;z<wz;z++)
             if(b||!or) set(sx+x,sy+y,sz+z,b);
         }   
     }
     public void extrude(bool or,int zsrc,int sx,int sy,int sz,int wx,int wy,int wz,int c2x,int c2y,bool sphere) {       
       if(zsrc<0||zsrc>=dz||wz==1) return;
       bool bx=c2x!=int.MinValue,by=c2y!=int.MinValue;       
       if(sphere&&!bx&&!by) {
         bx=wx>1;if(bx) c2x=2*sx+wx;
         by=wy>1;if(by) c2y=2*sy+wy;
       }
       bool ex=even(c2x),ey=even(c2y);
       int cx=c2x/2,cy=c2y/2;
       if(!bx&&!by) extrude(or,zsrc,sx,sy,sz,wx,wy,wz);
       if(!or) {
         for(int z=0;z<wz;z++)
           if(sz+z!=zsrc)
             clear(false,sx,sy,sz+z,wx,wy,1);
       }
       bool rev=zsrc>=sz+wz-1;
       int n=sphere?1+(int)Math.Sqrt(wx*wx+wy*wy+wz*wz):0;
       for(int x=0;x<wx;x++)   
         for(int y=0;y<wy;y++) {
           bool b=get(sx+x,sy+y,zsrc);
           if(!b) continue;
           if(sphere) {
             double dx=2*(sx+x)+1-c2x,dy=2*(sy+y)+1-c2y,d=bx&&by?Math.Sqrt(dx*dx+dy*dy):Math.Abs(bx?dx:dy);
             if(d<=0) setz(sx+x,sy+y,sz,sz+wz-1,true);
             else for(int i=0;i<=n;i++) {               
               int nx=x,ny=y,nz;
               double a=i*Math.PI/2/n,co=Math.Cos(a);
               nz=(int)(0.5+(wz-1)*Math.Sin(a));
               if(rev) nz=wz-1-nz;
               nz+=sz;
               if(nz==zsrc) continue;
               nx=bx?(int)((c2x+dx*co)/2):sx+x;
               ny=by?(int)((c2y+dy*co)/2):sy+y;
               set(nx,ny,nz,b);
             }
           } else for(int z=0;z<wz;z++) {             
             int nx=sx+x,ny=sy+y,z2=wz-1-z,za=z,zb=z2;
             if(rev) { int zx=za;za=zb;zb=zx;}
             if(bx) nx=((c2x+(ex&&x<cx?-2:0))*za+(2*(sx+x)+1)*zb)/(wz-1)/2;             
             if(by) ny=((c2y+(ey&&y<cy?-2:0))*za+(2*(sy+y)+1)*zb)/(wz-1)/2;             
             set(nx,ny,sz+z,b);
           }           
         }   
     }
     public void twist(int zsrc,int sx,int sy,int sz,int wx,int wy,int wz,int c2x,int c2y,int angle) {
       if(zsrc<0||zsrc>=dz||wz==1) return;
       bit3d pz=new bit3d();
       pz.alloc(wx,wy,1);
       float fx=c2x*2,fy=c2y*2;
       for(int z=0;z<wz;z++) {
         if(z==zsrc) continue;
         double a=angle*(sz+z-zsrc)/(wz-1),a2=a*Math.PI/180,si=Math.Sin(a2),co=Math.Cos(a2);
         pz.copy(0,0,0,this,sx,sy,z,wx,wy,1);
         clear(false,sx,sy,sz+z,wx,wy,1);
         for(int x=0;x<wx;x++)   
           for(int y=0;y<wy;y++) {
             bool b=pz.get(x,y,0);
             if(!b) continue;
             double cx=4*(sx+x)+1-fx,cy=4*(sy+y)+1-fy;
             int nx,ny;
             nx=(int)(fx+cx*co+cy*si)/4;ny=(int)(fy-cx*si+cy*co)/4;
             if(nx>=sx&&nx<sx+wx&&ny>=sy&&ny<sy+wy) set(nx,ny,sz+z,true);
             cx+=2;
             nx=(int)(fx+cx*co+cy*si)/4;ny=(int)(fy-cx*si+cy*co)/4;
             if(nx>=sx&&nx<sx+wx&&ny>=sy&&ny<sy+wy)  set(nx,ny,sz+z,true);
             cy+=2;
             nx=(int)(fx+cx*co+cy*si)/4;ny=(int)(fy-cx*si+cy*co)/4;
             if(nx>=sx&&nx<sx+wx&&ny>=sy&&ny<sy+wy) set(nx,ny,sz+z,true);
             cx-=2;
             nx=(int)(fx+cx*co+cy*si)/4;ny=(int)(fy-cx*si+cy*co)/4;
             if(nx>=sx&&nx<sx+wx&&ny>=sy&&ny<sy+wy) set(nx,ny,sz+z,true);
           }
       }
     }
     
     public void inv() {
       for(int i=0;i<bit.Length;i++)
         bit[i]=~bit[i];
     }
     static public bool operation(bit_op op,bool a,bool b) {
       switch(op) {
        case bit_op.Zero:return false;
        case bit_op.One:return true;
        case bit_op.Not:return !a;
        case bit_op.And:return a&&b;
        case bit_op.Sub:return a&&!b;
        case bit_op.Or:return a||b;
        case bit_op.Xor:return a^b;
        case bit_op.Copy:return b;
        case bit_op.Invert:return !b;
        default:return false;
      }
     }
     static public uint operation(bit_op op,uint a,uint b) {
       switch(op) {
        case bit_op.Zero:return 0u;
        case bit_op.One:return ~0u;
        case bit_op.Not:return ~a;
        case bit_op.And:return a&b;
        case bit_op.Sub:return a&~b;
        case bit_op.Or:return a|b;
        case bit_op.Xor:return a^b;
        case bit_op.Copy:return b;
        case bit_op.Invert:return ~b;
        default:return 0;
      }
     }
     public bool operation(bit_op op,int x,int y,int z,bool b) {
       int offset;
       uint mask;
       offset_mask(out offset,out mask,x,y,z);
       bool a=(bit[offset]&mask)!=0;
       bool c=operation(op,a,b);
       if(c!=a) bit[offset]^=mask;
       return a;       
     }
     void clip_low(int min,ref int x,int smin,ref int sx,ref int width) {
       int r0=min-x,r1=smin-sx;
       if(r1>r0) r0=r1;
       if(r0<1) return;
       width-=r0;
       x+=r0;
       sx+=r0;
     }
     void clip_high(int max,int x,int smax,int sx,ref int width) {
       int r0=x+width-max,r1=sx+width-smax;
       if(r1>r0) r0=r1;
       if(r0>0) width-=r0;       
     }
     void clip_high(int max,int x,ref int width) {
       int r0=x+width-max;
       if(r0>0) width-=r0;       
     }
     public void Clip(int axis,ref int value,ref int length) {
       if(length<0) length=0;
       int d=axis<1?dx:axis>1?dz:dy;
       if(value+length<=0||value>=d) {
         length=0;
         return;
       }
       if(value<0) {
         length+=value;
         value=0;
       }  
       if(value+length>d)
         length=d-value;     
     }
     public void Clip(int axis,ref int v) {
       if(v<0) v=0;if(v<0) v=0;
       int d=axis<1?dx:axis>1?dz:dy;
       if(v>=d) v=d-1;
     }
     public void Clip2(int axis,ref int u,ref int v) {
       Clip(axis,ref u);
       Clip(axis,ref v);
       if(v<u) {
         int r=u;u=v;v=r;
       }       
     }
     public void Clip(ref int x0,ref int x1,ref int y0,ref int y1,ref int z0,ref int z1) {
       Clip2(0,ref x0,ref x1);
       Clip2(1,ref y0,ref y1);
       Clip2(2,ref z0,ref z1);
     }
     public void operation(bit_op op,int sx,int sy,int sz,int wx,int wy,int wz) {
       bool xor=op==bit_op.Not||op==bit_op.Xor;
       bool value=op==bit_op.One;
       for(int x=0;x<wx;x++)
         for(int y=0;y<wy;y++)
           for(int z=0;z<wz;z++)
             set(sx+x,sy+y,sz+z,xor?!get(sx+x,sy+y,sz+z):value);
     }
     public void operation(bit_op op,int x,int y,int z,bit3d src,int sx,int sy,int sz,int width,int height,int depth) {
       clip_low(0,ref x,0,ref sx,ref width);
       clip_low(0,ref y,0,ref sy,ref height);
       clip_low(0,ref z,0,ref sz,ref depth);
       if(op==bit_op.Zero||op==bit_op.One||op==bit_op.Not ) {
         clip_high(dx,x,ref width);
         clip_high(dy,y,ref height);
         clip_high(dz,z,ref depth);         
       } else {
         clip_high(dx,x,src.dx,sx,ref width);
         clip_high(dy,y,src.dy,sy,ref height);
         clip_high(dz,z,src.dz,sz,ref depth);
       }  
       if(op==bit_op.Copy&&x==0&&y==0&&sx==0&&sy==0&&width==dx&&height==dy&&src.dx==dx&&src.dy==dy) {
         Array.Copy(src.bit,sz*dpl*dy,bit,z*dpl*dy,depth*dpl*dy);  
         return;
       }       
       if(op==bit_op.One||op==bit_op.Zero) {
         for(int k=0;k<depth;k++)
           for(int j=0;j<height;j++)
             for(int i=0;i<width;i++)          
               set(x+i,y+j,z+k,op==bit_op.One);
       } else if(op==bit_op.Not) {
         for(int k=0;k<depth;k++)
           for(int j=0;j<height;j++)
             for(int i=0;i<width;i++)          
               flip(x+i,y+j,z+k);         
       } else       
         for(int k=0;k<depth;k++)
           for(int j=0;j<height;j++)
             for(int i=0;i<width;i++)
               operation(op,x+i,y+j,z+k,src.get(sx+i,sy+j,sz+k));
     }
     public void copy(int x,int y,int z,bit3d src,int sx,int sy,int sz,int dx,int dy,int dz) {
       operation(bit_op.Copy,x,y,z,src,sx,sy,sz,dx,dy,dz);
     }
     public void clear(bool value,int x,int y,int z,int dx,int dy,int dz) {
       operation(value?bit_op.One:bit_op.Zero,x,y,z,null,0,0,0,dx,dy,dz);
     }         
     public void not(int x,int y,int z,int width,int height,int depth) {
       if((x|y|z)==0&&width==dx&&height==dy&&depth==dz) not();
       else operation(bit_op.Not,x,y,z,null,0,0,0,width,height,depth);
     }
     static bool even(int x) {return 0==(x&1);}
     static bool odd(int x) {return 1==(x&1);}
     public static int sqr(int x) { return x*x;}
     public static int sqr(int x,int y) { return x*x+y*y;}
     public static int abs(int x) { return x<0?-x:x;}
     public static int abs(int x,int y) { return (x<0?-x:x)+(y<0?-y:y);}
     public static int sgn(int x) { return x==0?0:x<0?-1:1;}
     public static int sgn(int x,int y) { return x==y?0:x<y?-1:1;}
     public static int min(int x,int y) { return x<y?x:y;}
     public static int min(int x,int y,int z) { return x<y?x<z?x:z:y<z?y:z;}
     public static int max(int x,int y) { return x>y?x:y;}
     public static int max(int x,int y,int z) { return x>y?x>z?x:z:y>z?y:z;}
     public static void sort(ref int x,ref int y) { 
       if(x>y) { int a=x;x=y;y=a;}
     }
     public static void range(ref int x,ref int x2,int min,int max) {
       if(x<min) x=min;else if(x>max) x=max;
       if(x2<min) x2=min;else if(x2>max) x2=max;
     }
     public void Line(bit_op op,int x,int y,int z,int x2,int y2,int z2,Delegate3D onxyz,object param) {
       int n=max(abs(x2-x),abs(y2-y),abs(z2-z));
       if(n<2) {
         operation(op,x,y,z,true);
         if(onxyz!=null) onxyz(this,op,x,y,z,param);
         if(n==1) {
           operation(op,x2,y2,z2,true);
           if(onxyz!=null) onxyz(this,op,x2,y2,z2,param);
         }
         return;
       }
       x=2*x+1;y=2*y+1;z=2*z+1;
       x2=2*x2+1;y2=2*y2+1;z2=2*z2+1;
       int ax=abs(x-x2),ay=abs(y-y2),az=abs(z-z2);
       int sx=sgn(x2,x),sy=sgn(y2,y),sz=sgn(z2,z);       
       for(int i=0;i<=n;i++) {
         int cx=x+sx*ax*i/n,cy=y+sy*ay*i/n,cz=z+sz*az*i/n;
         if(cx<0||cy<0||cz<0||cx>=2*dx||cy>=2*dy||cz>=2*dz) continue;
         cx/=2;cy/=2;cz/=2;
         operation(op,cx,cy,cz,true);
         if(onxyz!=null) onxyz(this,op,cx,cy,cz,param);
       }
     }
     public void eval(double xmin,double ymin,double zmin,double xmax,double ymax,double zmax,Delegate3DToD f3d,DelegateDToB f1d) {
       double[] rx=new double[dx],ry=new double[dy];
       int x,y,z;
       for(x=0;x<dx;x++)
         rx[x]=(x*xmax+(dx-1-x)*xmin)/(dx-1);
       for(y=0;y<dy;y++)
         ry[y]=(y*ymax+(dy-1-y)*ymin)/(dy-1);
       for(z=0;z<dz;z++) {
         double rz=(z*zmax+(dz-1-z)*zmin)/(dz-1);
         for(y=0;y<dy;y++)
           for(x=0;x<dx;x++)
             set(x,y,z,f1d(f3d(rx[x],ry[y],rz)));
       }
     } 
     
     public void mirrorcopy(int axis,int pos) {
       mirrorcopy(axis,pos,0,0,0,dx-1,dy-1,dz-1);
     }
     public void mirrorcopy(int axis,int pos,int sx,int sy,int sz,int wx,int wy,int wz) {
       bool even=0==(pos&1);
       pos/=2;
       int x,y,z,p=pos-1;
       bool r;
       pos-=axis==1?sy:axis==2?sz:sx;
       if(!even) pos+=1;
       if(pos<0) {p+=pos;pos=0;}
       switch(axis) {
        case 0:
         for(x=pos;x<wx&&p>=0;x++,p--) 
           for(y=0;y<wy;y++) 
             for(z=0;z<wz;z++) 
               set(sx+x,sy+y,sz+z,get(p,sy+y,sz+z));
         break;
        case 1:
         for(y=pos;y<wy&&p>=0;y++,p--) 
           for(x=0;x<wx;x++) 
             for(z=0;z<wz;z++) 
               set(sx+x,sy+y,sz+z,get(sz+x,p,sz+z));
         break;
        case 2:
         for(z=pos;z<wz&&p>=0;z++,p--) 
           for(x=0;x<wx;x++) 
             for(y=0;y<wy;y++) 
               set(sx+x,sx+y,sx+z,get(sz+x,sy+y,p));
         break;
       }               
     }
     
     public void mirror(int axis) { mirror(axis,0,0,0,dx-1,dy-1,dz-1);} 
     public void mirror(int axis,int sx0,int sy0,int sz0,int sx1,int sy1,int sz1) {
       int x,y,z,r2;
       bool r;

       switch(axis) {
        case 0:
         for(z=sz0;z<=sz1;z++)
           for(y=sy0;y<=sy1;y++)
             for(x=sx0,r2=sx1;x<r2;x++,r2--) {
               r=get(x,y,z);
               set(x,y,z,get(r2,y,z));
               set(r2,y,z,r);
             }  
         break;
        case 1:
         for(z=sz0;z<=sz1;z++)
           for(y=sy0,r2=sy1;y<r2;y++,r2--)
             for(x=sx0;x<=sx1;x++) {
               r=get(x,y,z);
               set(x,y,z,get(x,r2,z));
               set(x,r2,z,r);
             }               
         break;
        case 2:
         for(z=sz0,r2=sz1;z<r2;z++,r2--)
           for(y=sy0;y<=sy1;y++)
             for(x=sx0;x<=sx1;x++) {
               r=get(x,y,z);
               set(x,y,z,get(x,y,r2));
               set(x,y,r2,r);
             }  
         break;
       }
     }

     public void rotate(int axis,bool dir) {
       bit3d tmp;
       int x,y,z;

       switch(axis) {
        case 0:
         tmp=new bit3d();
         tmp.alloc(dx,dz,dy);         
         for(z=0;z<dz;z++)
           for(y=0;y<dy;y++)
             if(dir)
               for(x=0;x<dx;x++)
                 tmp.set(x,z,dy-y-1,get(x,y,z));
             else
               for(x=0;x<dx;x++)
                 tmp.set(x,dz-z-1,y,get(x,y,z));
         swap(tmp);
         break;
        case 1:
         tmp=new bit3d();
         tmp.alloc(dz,dy,dx);
         for(z=0;z<dz;z++)
           for(y=0;y<dy;y++)
             if(dir) 
               for(x=0;x<dx;x++)
                  tmp.set(dz-z-1,y,x,get(x,y,z));
             else     
               for(x=0;x<dx;x++)
                  tmp.set(z,y,dx-x-1,get(x,y,z));
         swap(tmp);
         break;
        case 2:
         tmp=new bit3d();
         tmp.alloc(dy,dx,dz);
         for(z=0;z<dz;z++)
           for(y=0;y<dy;y++)
             if(dir)
               for(x=0;x<dx;x++)               
                 tmp.set(dy-y-1,x,z,get(x,y,z));
             else      
               for(x=0;x<dx;x++)               
                 tmp.set(y,dx-x-1,z,get(x,y,z));
         swap(tmp);
         break;
       }                 
     }
     internal class flood_state { internal int s,xi,xa,yi,ya,zi,za;};
     static void flood_seed(bit3d bound,bit3d fill,bool black,flood_state ns,int x,int y,int z) {
       bound.set(x,y,z,black);
       fill.set(x,y,z,black);
       if(x<ns.xi) ns.xi=x;
       if(x>ns.xa) ns.xa=x;
       if(y<ns.yi) ns.yi=y;
       if(y>ns.ya) ns.ya=y;
       if(z<ns.zi) ns.zi=z;
       if(z>ns.za) ns.za=z;
       ns.s++;
     }
     public void floodfill(int sx,int sy,int sz,bool black,int mode,bool down) {
       bit3d tmp=new bit3d();
       int x,y,z;
       flood_state os,ns=new flood_state();// old step and new step
       tmp.alloc(dx+2,dy+2,dz+2);
       tmp.clear(black);
       tmp.operation(bit_op.Copy,1,1,1,this,0,0,0,dx,dy,dz);
       alloc(dx+2,dy+2,dz+2);
       clear(!black);
       set(sx+1,sy+1,sz+1,black);
       tmp.set(sx+1,sy+1,sz+1,black);
       ns.xi=ns.xa=sx+1;
       ns.yi=ns.ya=sy+1;
       ns.zi=ns.za=sz+1;
       ns.s=1;
       while(ns.s>0) {
         os=ns;
         ns=new flood_state();
         ns.s=0;
         ns.xi=dx;ns.xa=-1;
         ns.yi=dy;ns.ya=-1;
         ns.zi=dz;ns.za=-1;
         if(os.xi<1) os.xi=0;
         if(os.xa>dx-2) os.xa=dx-2;
         if(os.yi<1) os.yi=0;
         if(os.ya>dy-2) os.ya=dy-2;
         if(os.zi<1) os.zi=0;
         if(os.za>dz-2) os.za=dz-2;
         for(z=os.zi;z<=os.za;z++) 
           for(y=os.yi;y<=os.ya;y++) 
             for(x=os.xi;x<=os.xa;x++)
               if(black==get(x,y,z)) {
                 set(x,y,z,!black);
                 if(black!=tmp.get(x-1,y,z))
                   flood_seed(this,tmp,black,ns,x-1,y,z);
                 if(black!=tmp.get(x+1,y,z))
                   flood_seed(this,tmp,black,ns,x+1,y,z);
                 if(black!=tmp.get(x,y-1,z)&&(!down||y>sy+1))
                   flood_seed(this,tmp,black,ns,x,y-1,z);
                 if(black!=tmp.get(x,y+1,z))
                   flood_seed(this,tmp,black,ns,x,y+1,z);
                 if(black!=tmp.get(x,y,z-1))
                   flood_seed(this,tmp,black,ns,x,y,z-1);
                 if(black!=tmp.get(x,y,z+1))
                   flood_seed(this,tmp,black,ns,x,y,z+1);
               }
       }
       alloc(dx-2,dy-2,dz-2);
       operation(bit_op.Copy,0,0,0,tmp,1,1,1,dx,dy,dz);
     }
    static int Hi(int hilo) { return (hilo>>16)&65535;}
    static int Lo(int hilo) { return hilo&65535;}
    static int HiLo(int hi,int lo) { return (hi<<16)|lo;}

    public void floodfill2d(int sx,int sy,int sz,bool black,int mode,bool down) {
       bit3d tmp=new bit3d();
       tmp.alloc(dx,dy,1);
       tmp.clear(false);       
       int[] fifo=new int[dx*dy];
       int n=0,m=0,k=0;
       fifo[m++]=HiLo(sy,sx);
       tmp.set(sx,sy,0,true);
       while(n<m) {
         int x=Lo(fifo[n]),y=Hi(fifo[n++]);
         bool b=false;
         if(x>0&&!tmp.get(x-1,y,0)) { if(get(x-1,y,sz)!=black) {fifo[m++]=HiLo(y,x-1);tmp.set(x-1,y,0,true);} else b=true;}
         if(x<dx-1&&!tmp.get(x+1,y,0)) {if(get(x+1,y,sz)!=black) {fifo[m++]=HiLo(y,x+1);tmp.set(x+1,y,0,true);} else b=true;}
         if(y>(down?sy:0)&&!tmp.get(x,y-1,0)) {if(get(x,y-1,sz)!=black) {fifo[m++]=HiLo(y-1,x);tmp.set(x,y-1,0,true);} else b=true;}
         if(y<dy-1&&!tmp.get(x,y+1,0)) {if(get(x,y+1,sz)!=black) {fifo[m++]=HiLo(y+1,x);tmp.set(x,y+1,0,true);} else b=true;}
         if(b) {
           x=fifo[n-1];fifo[n-1]=fifo[k];fifo[k++]=x;
         }
       }
       if(m==1) {
         set(sx,sy,sz,black);
       } else if(mode==1) { 
         for(int i=0;i<k;i++) 
           set(Lo(fifo[i]),Hi(fifo[i]),sz,black);
       } else if(mode==3) {
         for(int i=k;i<m;i++) 
           set(Lo(fifo[i]),Hi(fifo[i]),sz,black);
       } else if(mode==2) {
         for(int i=0;i<k;i++) {
           int x=Lo(fifo[i]),y=Hi(fifo[i]);
           if(x>0) set(x-1,y,sz,!black);
           if(x<dx-1) set(x+1,y,sz,!black);
           if(y>0) set(x,y-1,sz,!black);
           if(y<dy-1) set(x,y+1,sz,!black);
         }         
       } else 
         for(int i=0;i<n;i++) set(Lo(fifo[i]),Hi(fifo[i]),sz,black);
    }               
    public void resize(int nx,int ny,int nz) {
      int x,y,z,z2;
      int[] x2=new int[nx],y2=new int [ny];
      bit3d tmp=new bit3d();
      tmp.alloc(nx,ny,nz);
      for(y=0;y<ny;y++)
        y2[y]=y*dy/ny;
      for(x=0;x<nx;x++)
        x2[x]=x*dx/nx;  
      for(z=0;z<nz;z++) {
        z2=z*dz/nz;
        for(y=0;y<ny;y++)
          for(x=0;x<nx;x++) 
            tmp.set(x,y,z,get(x2[x],y2[y],z2));
      }
      swap(tmp);
    }
    public void extend(bool bit,int size) {
      int x,y,z;
      bit3d tmp=new bit3d();
      tmp.alloc(dx+2*size,dy+2*size,dz+2*size);
      tmp.clear(bit);
      for(z=0;z<dz;z++) 
        for(y=0;y<dy;y++)
          for(x=0;x<dx;x++) 
            tmp.set(x+size,y+size,z+size,get(x,y,z));
      swap(tmp);
    }
    public void insertplane(int axis,int d) {
      if(d<0) return;
      if(axis==2) {
        if(d>=dz) return;
        dz++;
        uint[] bit2=new uint[size(dx,dy,dz)];
        int h=dpl*dy*(d+1),he=bit2.Length;
        while(h<he) {
          he--;
          bit2[he]=bit[he-dpl*dy];
        }  
        Array.Copy(bit,bit2,h);
        bit=bit2;
      } else if(axis==1) {
        if(d>dy) return;
        dy++;
        uint[] bit2=new uint[size(dx,dy,dz)];
        int h=dpl*d,g=h+dpl,b=dpl*dy;
        Array.Copy(bit,0,bit2,0,g);
        for(int z=1;z<dz;z++) {
          Array.Copy(bit,h,bit2,g,b);
          h+=b-dpl;
          g+=b;
        }  
        Array.Copy(bit,h,bit2,g,(dy-d-1)*dpl);        
        bit=bit2;
      } else {
        if(d>dx) return;
        dx++;
        if((dx&31)==1) {
          uint[] bit2=new uint[size(dx,dy,dz)];
          int h=0,g=0;
          while(g<bit2.Length) {
            switch(dpl) {
             case 1:bit2[g++]=bit[h++];break;
             case 2:bit2[g++]=bit[h++];bit2[g++]=bit[h++];break;
             default:
              for(int x=0;x<dpl;x++) bit2[g++]=bit[h++];
              break;
            }  
            bit2[g++]=0;
          }  
          dpl++;
          bit=bit2;
        }
        int nl=d>>5,nr=dpl-nl-1;
        int m=1<<(d&31),ml=m|(m-1),mr=~ml|m;
        for(int p=bit.Length-1;p>=0;p-=nl) {
          for(int n=nr;n>0;n--) {
            bit[p]=(uint)((bit[p]<<1)|((bit[p-1]&0x80000000)==0?0:1));
            p--;
          }
          bit[p]=(uint)(((bit[p]&mr)<<1)|(bit[p]&ml));
          p--;
       }
      }
    }
    public void deleteplane(int axis,int d) {
      if(d<0) return;
      if(axis==2) {  
        if(d>=dz) return;      
        for(int h=dpl*dy*(d+1);h<bit.Length;h++)
          bit[h-dpl*dy]=bit[h];
        dz--;
        Array.Resize(ref bit,size(dx,dy,dz));
      } else if(axis==1) {
        if(d>=dy) return;
        int b=dy-1,g=dpl*d,h=g+dpl,he=dpl*dy*dz;
        while(h<he) {
          for(int n=dpl;n>0;n--) 
            bit[g++]=bit[h++];
          b--;  
          if(b==0) {
            b=dy-1;
            h+=dpl;
          }
        }
        //Array.Copy(bit,g,bit,h,(dy-d-1)*dpl);
        dy--;        
        Array.Resize(ref bit,size(dx,dy,dz));
      } else {
        if(d>=dx) return;
        dx--;
        int nl=d>>5,nr=dpl-nl-1;
        int m=1<<(d&31),ml=(m-1),mr=~ml&~m;
        int pe=dpl*dy*dz;
        for(int p=0;p<bit.Length;) {
          p+=nl;
          bit[p]=(uint)(((bit[p]&mr)>>1)|(bit[p]&ml)|(nr<1?0:(bit[p+1]&1)==0?0:0x80000000));
          p++;
          for(int n=nr;n-->0;p++)
            bit[p]=(uint)((bit[p]>>1)|(n<1?0:(bit[p+1]&1)==0?0:0x80000000));
        }
        if((dx&31)==0) {
          int h=0,g=0;
          dpl--;
          for(int yz=dy*dz;yz>0;yz--) {
            switch(dpl) {
             case 1:bit[g++]=bit[h++];break;
             case 2:bit[g++]=bit[h++];bit[g++]=bit[h++];break;
             default:for(int x=dpl;x>0;x--) bit[g++]=bit[h++];break;
            }
            h++;
          }
          Array.Resize(ref bit,size(dx,dy,dz));
        }
      }
    }        
    public void random(double p,Random rnd) {
      int x,y,z,l=(int)(p*int.MaxValue);
      if(rnd==null) rnd=new Random();
      for(z=0;z<dz;z++)
        for(y=0;y<dy;y++)
          for(x=0;x<dx;x++)
            set(x,y,z,l>rnd.Next());      
    }

    public void sumfilter(int mode,int rule,bool border,int count) { sumfilter(mode,rule,border,count,0,0,0,dx,dy,dz);}
    public void sumfilter(int mode,int rule,bool border,int count,int sx,int sy,int sz,int wx,int wy,int wz) {
      bit3d tmp=new bit3d();
      int x,y,z;
      int x2,y2,z2,s;
      tmp.alloc(wx+2,wy+2,wz+2);
      tmp.operation(bit_op.Copy,1,1,1,this,sx,sy,sz,wx,wy,wz);      
      if(border) tmp.box(bit_op.One,0,0,0,wx+2,wy+2,wz+2,false);
     while(count-->0)
      for(z=1;z<=wz;z++)
        for(y=1;y<=wy;y++)
          for(x=1;x<=wx;x++) {
            s=tmp.get(x,y,z)?1:0; 
            s+=tmp.get3(2,mode,x,y,z);
            /*
            if(mode!=2) {
              if(tmp.get(x,y,z)) s++;
              if(tmp.get(x+1,y,z)) s++;if(tmp.get(x-1,y,z)) s++;
              if(tmp.get(x,y+1,z)) s++;if(tmp.get(x,y-1,z)) s++;
              if(tmp.get(x,y,z+1)) s++;if(tmp.get(x,y,z-1)) s++;
              if(mode==1) {
                if(tmp.get(x,y,z)) s++;
                if(tmp.get(x+1,y+1,z)) s++;if(tmp.get(x+1,y-1,z)) s++;if(tmp.get(x-1,y+1,z)) s++;if(tmp.get(x-1,y-1,z)) s++;
                if(tmp.get(x+1,y,z+1)) s++;if(tmp.get(x+1,y,z-1)) s++;if(tmp.get(x-1,y,z+1)) s++;if(tmp.get(x-1,y,z-1)) s++;
                if(tmp.get(x,y+1,z+1)) s++;if(tmp.get(x,y+1,z-1)) s++;if(tmp.get(x,y-1,z+1)) s++;if(tmp.get(x,y-1,z-1)) s++;
              }
            } else 
              for(z2=-1;z2<2;z2++)
                for(y2=-1;y2<2;y2++)
                  for(x2=-1;x2<2;x2++)
                    if(tmp.get(x+x2,y+y2,z+z2))
                      s++;*/
            set(sx+x-1,sy+y-1,sz+z-1,((1<<s)&rule)!=0);
          } 
      return;
    }
    public void open(string filename) {
      BinaryReader br=null;
      bool gz=mesh.isgz(filename);
     try { 
       Stream fs=new FileStream(filename,FileMode.Open,FileAccess.Read);
       if(gz) fs=new System.IO.Compression.GZipStream(fs,System.IO.Compression.CompressionMode.Decompress,false);
       br=new BinaryReader(fs);
      int i0=br.ReadInt32(),i1=br.ReadInt32(),i2=br.ReadInt32(),i3=br.ReadInt32();
      if(i3==0) {
        dx=i0;dy=i1;dz=i2;
      } else {
        dx=i1;dy=i2;dz=i3;
      }
      alloc(dx,dy,dz);
      for(int i=0;i<bit.Length;i++)
        bit[i]=br.ReadUInt32();
      br.Close();
      br=null;
     } finally {
       if(br!=null) br.Close();
     } 
    }

    public void save(string filename) {
      bool gz=mesh.isgz(filename);
      Stream fs=new FileStream(filename,FileMode.OpenOrCreate,FileAccess.Write),fs2=fs;
      if(gz) fs=new System.IO.Compression.GZipStream(fs,System.IO.Compression.CompressionMode.Compress,false);
      BinaryWriter bw=new BinaryWriter(fs);
      bw.Write(0x627563);//cub\0 
      bw.Write(dx);
      bw.Write(dy);
      bw.Write(dz);
      //bw.Write(0);
      for(int i=0;i<bit.Length;i++)
        bw.Write(bit[i]);
      bw.Flush();
      if(gz) fs.Flush();
      fs2.SetLength(fs2.Position);
      bw.Close();
    }
    public string Export(int cx,int cy,int cz) {
      System.Text.StringBuilder sb=new System.Text.StringBuilder();
      sb.Append(""+ dx +","+dy+","+dz);
      if((cx|cy|cz)!=0) sb.Append("\r\n#cursor "+cx+","+cy+","+cz);
      for(int i=0;i<bit.Length;i++) {
        if(0==(i&31)) sb.AppendLine("");
        else sb.Append(",");
        sb.Append(bit[i]==0xffffffff?"*":(bit[i]&0xff000000)==0xff000000?"~"+(~bit[i]).ToString("X"):bit[i].ToString("X"));
      }
      return ""+sb;
    }
    static int Hex(char ch) {
      return ch>='0'&&ch<='9'?ch-'0':ch>='A'&&ch<='F'?ch-'A'+10:ch>='a'&&ch<='f'?ch-'a'+10:-1;
    }
    public static bit3d Import(string text,out int cx,out int cy,out int cz) {
      cx=cy=cz=0;
      if(""+text=="") return null;
      int tx,ty,tz;
      int pos=text.IndexOf('\n'),pos2=pos<1?-1:text.IndexOf('\n',++pos);
      if(pos<1) return null;      
      System.Text.RegularExpressions.Match m,m2;
      m=System.Text.RegularExpressions.Regex.Match(text.Substring(0,pos-1),@"^\s*(\d+)\s*,\s*(\d+)\s*,\s*(\d+)");
      if(!m.Success||(tx=int.Parse(""+m.Groups[1]))<1||(ty=int.Parse(""+m.Groups[2]))<1||(tz=int.Parse(""+m.Groups[3]))<1) return null;
      if(pos2>0) {
        m2=System.Text.RegularExpressions.Regex.Match(text.Substring(pos,pos2-pos),@"^\s*#cursor\s*(-?\d+)\s*,\s*(-?\d+)\s*,\s*(-?\d+)");
        if(m2.Success) { cx=int.Parse(""+m2.Groups[1]);cy=int.Parse(""+m2.Groups[2]);cz=int.Parse(""+m2.Groups[3]);}
      }
      bit3d b3=new bit3d();
      b3.alloc(tx,ty,tz);
      bool comm=false;
      int h=0,x,d=0,bl=b3.bit.Length;
      while(pos<text.Length) {
        char ch=text[pos++];        
        if(comm) {
          if(ch=='\n') comm=false;
        } else if(0<=(x=Hex(ch))||ch=='~') {
          bool neg=x<0;
          if(neg) x=0;
          while(pos<text.Length&&0<=(d=Hex(text[pos]))) {
            x=(x<<4)|d;
            pos++;
          }
          if(neg) x=~x;
          b3.bit[h++]=(uint)x;
          if(h>=bl) break;
        } else if(ch=='*') {
          b3.bit[h++]=0xffffffff;
          if(h>=bl) break;
        } else if(ch=='#') {
          comm=true;
        } 
      }
      return b3;
    }
    public static bit3d Import(System.Drawing.Bitmap bm) {
      if(bm==null) return null;
      bit3d b3=new bit3d();
      b3.alloc(bm.Width,bm.Height,1);
      System.Drawing.Imaging.BitmapData bd=bm.LockBits(new System.Drawing.Rectangle(0,0,bm.Width,bm.Height),System.Drawing.Imaging.ImageLockMode.ReadOnly
        ,0!=(bm.PixelFormat&System.Drawing.Imaging.PixelFormat.Alpha)?System.Drawing.Imaging.PixelFormat.Format32bppArgb:System.Drawing.Imaging.PixelFormat.Format32bppRgb);
      byte[] line=new byte[4*bd.Width];
      for(int y=0;y<bm.Height;y++) {
        System.Runtime.InteropServices.Marshal.Copy(new IntPtr((long)bd.Scan0+y*bd.Stride),line,0,line.Length);
        for(int x=0,h=0;x<bm.Width;x++,h+=4)
          if(line[h]<144&&line[h+1]<144&&line[h+2]<144) b3.set(x,y,0,true);
      }
      bm.UnlockBits(bd);
      return b3;
    }
    void PTS_MIRROR(int[] pts,int x,int y) {
      int r=pts[y];
      pts[y]=pts[x];
      pts[x]=r;
    }
    void cube_mirror(int[] pts,int axis,ref byte mask) {
      switch(axis) {
       case 0: // x
        PTS_MIRROR(pts,0,2);PTS_MIRROR(pts,4,7);PTS_MIRROR(pts,5,6);PTS_MIRROR(pts,8,10);
        mask=(byte)(((mask&0x11)<<3)|((mask&0x22)<<1)|((mask&0x44)>>1)|((mask&0x88)>>3));
       break;
       case 1: // y
        PTS_MIRROR(pts,1,3);PTS_MIRROR(pts,4,5);PTS_MIRROR(pts,6,7);PTS_MIRROR(pts,9,11);
        mask=(byte)(((mask&0x11)<<1)|((mask&0x22)>>1)|((mask&0x44)<<1)|((mask&0x88)>>1));
       break;
       case 2: // z
        PTS_MIRROR(pts,0,8);PTS_MIRROR(pts,1,9);PTS_MIRROR(pts,2,10);PTS_MIRROR(pts,3,11);
        mask=(byte)(((mask&0x0f)<<4)|((mask&0xf0)>>4));
       break;
       case 3: // x+y
        PTS_MIRROR(pts,0,1);PTS_MIRROR(pts,2,3);PTS_MIRROR(pts,4,6);PTS_MIRROR(pts,8,9);PTS_MIRROR(pts,10,11);
        mask=(byte)((mask&0xaa)|((mask&0x11)<<2)|((mask&0x44)>>2));
       break;
       case 4: // x-y
        PTS_MIRROR(pts,0,3);PTS_MIRROR(pts,1,2);PTS_MIRROR(pts,5,7);PTS_MIRROR(pts,8,11);PTS_MIRROR(pts,9,10);
        mask=(byte)((mask&0x55)|((mask&0x22)<<2)|((mask&0x88)>>2));
       break;
      }            
    }
    void bitrotate(ref byte mask,byte[] rot,byte[] rot2) {
      byte m=mask,m2=0;
      if(0!=(m&rot[0])) m2|=rot[3];
      if(0!=(m&rot[1])) m2|=rot[0];
      if(0!=(m&rot[2])) m2|=rot[1];
      if(0!=(m&rot[3])) m2|=rot[2];
      if(0!=(m&rot2[0])) m2|=rot2[3];
      if(0!=(m&rot2[1])) m2|=rot2[0];
      if(0!=(m&rot2[2])) m2|=rot2[1];
      if(0!=(m&rot2[3])) m2|=rot2[2];
      mask=m2;
    } 
    void PTS_ROTATE(int[] pts,int a,int b,int c,int d) {
      int r=pts[a];
      pts[a]=pts[b];
      pts[b]=pts[c];
      pts[c]=pts[d];
      pts[d]=r;
    }  
    void PTS_SWAP(int[] pts,int a,int b) {
      int r=pts[a];
      pts[a]=pts[b];
      pts[b]=r;
    }
    static byte[] xrot=new byte[] {1,16,128,8},xrot2=new byte[] {2,32,64,4};
    static byte[] yrot=new byte[] {1,2,32,16},yrot2=new byte[] {8,4,64,128};
    static byte[] zrot=new byte[] {1,2,4,8},zrot2=new byte[] {16,32,64,128};

    void cube_rotate(int[] pts,int axis,ref byte mask) {
      switch(axis) {
       case 0: // x
        PTS_ROTATE(pts,0,8,10,2);PTS_ROTATE(pts,1,5,9,6);PTS_ROTATE(pts,3,4,11,7);
        bitrotate(ref mask,xrot,xrot2); 
        break;
       case 1: // y
        PTS_ROTATE(pts,0,5,8,4);PTS_ROTATE(pts,1,9,11,3);PTS_ROTATE(pts,2,6,10,7);
        bitrotate(ref mask,yrot,yrot2);
        break;
       case 2: // z
        PTS_ROTATE(pts,0,1,2,3);PTS_ROTATE(pts,4,5,6,7);PTS_ROTATE(pts,8,9,10,11);
        bitrotate(ref mask,zrot,zrot2);
       break; 
      }
    }
               
    static int[] bc=new int[] {0,1,1,2,1,2,2,3,1,2,2,3,2,3,3,7};
    int bitcount(int n) {
      return bc[n&15]+bc[n>>4];
    }    
    static void tria(mesh msh,int[] pts,bool inv,int a,int b,int c) {
      if(inv) {
        int r;
        r=a;a=c;c=r;
      }
      msh.fcs.Triangle(pts[a]-1,pts[b]-1,pts[c]-1,POINTS.None);
    }
    static void quad(mesh msh,int[] pts,bool inv,int a,int b,int c,int d) {
      if(inv) {
        int r;
        r=a;a=d;d=r;r=b;b=c;c=r;
      }
      msh.fcs.Quad(pts[a]-1,pts[b]-1,pts[c]-1,pts[d]-1,POINTS.None);
    } 
    int cube_poly(int n,byte mask,int[] pts,mesh msh) {
      int lns=0;
      bool inv=false,neg;
      int xmax,ymax,zmax;

      if((neg=n>4)) {
        mask=(byte)~mask;
        inv^=true;
        n=8-n;
      }  
      xmax=max(bitcount(mask&0xcc),bitcount(mask&0x33));
      ymax=max(bitcount(mask&0x66),bitcount(mask&0x99));
      zmax=max(bitcount(mask&0xf0),bitcount(mask&0x0f));

      if(xmax>zmax||ymax>zmax) 
        cube_rotate(pts,xmax>ymax?0:1,ref mask);
      if(bitcount(mask&0xf0)>bitcount(mask&0x0f)) {
        inv^=true;
        cube_mirror(pts,2,ref mask);
      }     
      if(mask!=15)
        while((0==(mask&1))||(0!=(mask&8)))
          cube_rotate(pts,2,ref mask); 
      if(((mask&5)==5)) {
        if((0!=(mask&64))&&(0==(mask&16))) {
          inv^=true;
          cube_mirror(pts,3,ref mask);
        }  
        if((0==(mask&2))&&(0!=(mask&128))&&(0==(mask&32))) {
          inv^=true;
          cube_mirror(pts,4,ref mask);
        }  
      }
      if((mask&15)==3) {
        byte rb0=(byte)(bitcount(mask&0x90));
        byte rb1=(byte)(bitcount(mask&0x60));
        if(rb0>rb1||(mask&0xf0)==0xa0) {
          inv^=true;
          cube_mirror(pts,1,ref mask);
        }  
      }
  
      switch(mask) {
       case 1:
        tria(msh,pts,inv,0,3,4);
        lns++;
        break;
       case 3:
        quad(msh,pts,inv,4,5,1,3);
        /* tria(lnsfile,lprf,pts,inv,4,5,1);
          tria(lnsfile,lprf,pts,inv,4,1,3); */
        lns+=1;
        break;
       case 5:
        if(neg) {
          quad(msh,pts,inv,4,6,2,3);
          quad(msh,pts,inv,6,4,0,1);
          /* tria(lnsfile,lprf,pts,inv,4,6,2);
          tria(lnsfile,lprf,pts,inv,4,2,3);
          tria(lnsfile,lprf,pts,inv,6,4,0);
          tria(lnsfile,lprf,pts,inv,6,0,1); */
          lns+=2;
        } else {
          tria(msh,pts,inv,0,3,4);
          tria(msh,pts,inv,2,1,6);
          lns+=2;
        }
        break;
       case 7:
        tria(msh,pts,inv,4,5,6);
        quad(msh,pts,inv,4,6,2,3);
        /* tria(msh,pts,inv,4,6,2);
        tria(msh,pts,inv,4,2,3); */
        lns+=2;
        break;  
       case 15:
        quad(msh,pts,inv,4,5,6,7);
        /* tria(msh,pts,inv,4,5,6);
        tria(msh,pts,inv,4,6,7); */
        lns+=1;
        break;
       case 21:
        if(neg) {
          tria(msh,pts,inv,11,2,3);
          tria(msh,pts,inv,11,6,2);
          tria(msh,pts,inv,11,8,6);
          tria(msh,pts,inv,8,1,6);
          tria(msh,pts,inv,8,0,1);
          lns+=5;
        } else {
          tria(msh,pts,inv,0,3,11);
          tria(msh,pts,inv,0,11,8);
          tria(msh,pts,inv,2,1,6);
          lns+=3;
        }
        break; 
       case 23:
        tria(msh,pts,inv,8,5,6);
        tria(msh,pts,inv,11,8,6);
        tria(msh,pts,inv,11,6,2);
        tria(msh,pts,inv,11,2,3);
        lns+=4;
        break;
       case 37:
        if(neg) {
          tria(msh,pts,inv,4,8,9);
          tria(msh,pts,inv,4,9,6);
          tria(msh,pts,inv,4,6,2);
          tria(msh,pts,inv,4,2,3);
          tria(msh,pts,inv,1,5,0);
          lns+=5;
        } else {
          tria(msh,pts,inv,0,3,4);
          tria(msh,pts,inv,2,1,6);
          tria(msh,pts,inv,5,8,9);
          lns+=3;
        }
        break; 
       case 39:
        tria(msh,pts,inv,4,6,2);
        tria(msh,pts,inv,4,2,3);
        tria(msh,pts,inv,4,8,6);
        tria(msh,pts,inv,6,8,9);
        lns+=4;
        break;
       case 65:
        tria(msh,pts,inv,0,3,4);
        tria(msh,pts,inv,9,10,6);
        lns+=2;
        break;
       case 67:
        if(neg) {
          tria(msh,pts,inv,1,3,6);
          tria(msh,pts,inv,3,10,6);
          tria(msh,pts,inv,3,4,10);
          tria(msh,pts,inv,4,9,10);
          tria(msh,pts,inv,4,5,9);
          lns+=5; 
        } else {
          tria(msh,pts,inv,4,5,1);
          tria(msh,pts,inv,4,1,3);
          tria(msh,pts,inv,6,9,10);
          lns+=3;
        }
        break;
       case 85:
        tria(msh,pts,inv,0,3,11);
        tria(msh,pts,inv,0,11,8);
        tria(msh,pts,inv,2,1,9);
        tria(msh,pts,inv,2,9,10);
        lns+=4;
        break;
       case 135:
        tria(msh,pts,inv,4,5,6);
        tria(msh,pts,inv,4,6,2);
        tria(msh,pts,inv,4,2,3);
        tria(msh,pts,inv,10,11,7);
        lns+=4;
        break;
       case 165:
        tria(msh,pts,inv,0,3,4);
        tria(msh,pts,inv,2,1,6);
        tria(msh,pts,inv,10,11,7);
        tria(msh,pts,inv,8,9,5);
        lns+=4;
        break;
       case 195:
        tria(msh,pts,inv,4,5,1);
        tria(msh,pts,inv,4,1,3);
        tria(msh,pts,inv,7,6,11);
        tria(msh,pts,inv,11,6,9);
        lns+=4;
        break;
       default: 
        //printf("mask=%d\n",mask);
        break;
      }
      return lns;
    }       
    void mesh_poly2(mesh msh,bool cyclic) {
      int i,h,g,f,x,y,z,pts2,lns2;//*h,*g,*f,*xy
      int[] gxy,xy,xy1=new int[2*dx*dy],xy2=new int[2*dx*dy],zz=new int[dx*dy];
      h=0;xy=xy1;
      lns2=pts2=0;
      for(y=0;y<dy;y++) {// create flat array
        for(x=0;x<dx-1;x++) {
          if(get(x,y,0)^get(x+1,y,0)) {
            i=++pts2;
            msh.pts.Add(2*x+1,2*y,0);
          } else 
            i=0;
          xy[h++]=i;
        }
        h++;
        if(y<dy-1) 
          for(x=0;x<dx;x++) {
            if(get(x,y+1,0)^get(x,y,0)) {
              i=++pts2;
              msh.pts.Add(2*x,2*y+1,0);
            } else 
              i=0;
            xy[h++]=i;
          }
      }
      for(z=1;z<=dz;z++) { 
        int z1=z-1,z2=z;
        if(z==dz) {
          if(!cyclic) break;
          z2=0; 
        }  
        xy=xy==xy1?xy2:xy1; // switch plates
        h=0;
        for(y=0;y<dy;y++) { // create flat array
          for(x=0;x<dx-1;x++) {
            if(get(x,y,z2)^get(x+1,y,z2)) {
              i=++pts2;
              msh.pts.Add(2*x+1,2*y,2*z);
            } else 
              i=0;
            xy[h++]=i;
          }
          h++;
          if(y<dy-1) 
            for(x=0;x<dx;x++) {
              if(get(x,y+1,z2)^get(x,y,z2)) {
                i=++pts2;
                msh.pts.Add(2*x,2*y+1,2*z);
              } else 
                i=0;
              xy[h++]=i;
            }
       }
       h=0; // make inter array
       for(y=0;y<dy;y++)
         for(x=0;x<dx;x++) {
           if(get(x,y,z1)^get(x,y,z2)) {
             i=++pts2;
             msh.pts.Add(2*x,2*y,2*z-1);
           } else 
             i=0;
           zz[h++]=i;
         }
       h=0;f=0;g=0;
       gxy=xy==xy1?xy2:xy1;
       for(y=0;y<dy-1;y++) {
         //int ci,cn;
         int[] cp=new int[12];
         byte mn,mb;
         for(x=0;x<dx-1;x++,h++,g++,f++) { // go through cubes
           mn=mb=0;
           if(get(x,y,z1)) {mn++;mb|=1;}
           if(get(x+1,y,z1)) {mn++;mb|=2;}
           if(get(x+1,y+1,z1)) {mn++;mb|=4;}
           if(get(x,y+1,z1)) {mn++;mb|=8;}
           if(get(x,y,z2)) {mn++;mb|=16;}
           if(get(x+1,y,z2)) {mn++;mb|=32;}
           if(get(x+1,y+1,z2)) {mn++;mb|=64;}
           if(get(x,y+1,z2)) {mn++;mb|=128;}
           if(mn>0&&mn<8) {
             //cn=0;
             cp[0]=gxy[g];
             cp[1]=gxy[g+dx+1];
             cp[2]=gxy[g+2*dx];
             cp[3]=gxy[g+dx];
             cp[4]=zz[f];
             cp[5]=zz[f+1];
             cp[6]=zz[f+dx+1];
             cp[7]=zz[f+dx];
             cp[8]=xy[h];
             cp[9]=xy[h+dx+1];
             cp[10]=xy[h+2*dx];
             cp[11]=xy[h+dx];
             lns2+=cube_poly(mn,mb,cp,msh);
             // h[0],h[dx],h[dx+1],h[2*dx]
             // f[0],f[1],f[dx],f[dx+1]
             // g[0],g[dx],g[dx+1],g[2*dx]
           }
         }
         h+=dx+1;
         f++;
         g+=dx+1;
       }
     }
   }
   static void stream_copy(Stream d,Stream s) {
     byte[] buf=new byte[4096];
     int r;     
      while(0<(r=s.Read(buf,0,buf.Length)))
        d.Write(buf,0,r);
   }

   class plg_s {
     internal int pt,pl;//*p;
     internal int[] p;
     internal int s1,s2,z;
     internal mesh msh;
   }; 

   void test_point(plg_s p,int p1) {
     if(p.p[p1]==0) {
       int i=p1;
       p.msh.pts.Add(i%p.s1,i%p.s2/p.s1,p.z+(i>=p.s2?1:0));
       p.p[p1]=++p.pt;
     }
   }

   void poly(plg_s p,int p1,int p2,int p3,int p4) {
     test_point(p,p1);
     test_point(p,p2);
     test_point(p,p3);
     test_point(p,p4);
     p.msh.fcs.Quad(p.p[p1]-1,p.p[p2]-1,p.p[p3]-1,p.p[p4]-1,POINTS.None);
     //p.lprf(p.plf,p.p[p1]-1,p.p[p2]-1,p.p[p3]-1,p.p[p4]-1);
     p.pl++;
   }

   void mesh_poly(mesh msh,bool cyclic,bool sides) {
     plg_s p=new plg_s();
     //StreamWriter f;
     int x,y,r;//,*r;
     int s1=dx+1,s2=s1*(dy+1);
     int pt_idx_size=s2;
     bit3d b1=new bit3d(),b2=new bit3d();  // buffer for 2 cuts
     bool l0,l1;

     p.msh=msh;
     p.s1=s1;p.s2=s2;
     p.pt=p.pl=0;
     p.p=new int[2*pt_idx_size];
     //p2=s2;

     b1.alloc(2+dx,2+dy,1);
     b2.alloc(2+dx,2+dy,1);
     //memset(p2,0,pt_idx_size);
     if(cyclic)
       b2.operation(bit_op.Copy,1,1,0,this,0,0,dz-1,dx,dy,1);
     else
       b2.clear(false);
     for(p.z=0;p.z<=dz;p.z++) {
       Array.Copy(p.p,pt_idx_size,p.p,0,pt_idx_size);
       //memcpy(p.p,p2,pt_idx_size);	// nove indexy do starych
       b1.operation(bit_op.Copy,0,0,0,b2,0,0,0,b2.dx,b2.dy,b2.dz);
       //memcpy(b1.bit,b2.bit,b1.size());
       Array.Clear(p.p,pt_idx_size,pt_idx_size);
       //memset(p.p2,0,pt_idx_size);         // nove vynulovat
       b2.clear(false);
       if(p.z<dz)
         for(y=0;y<dy;y++)
           for(x=0;x<dx;x++) {
             b2.set(x+1,y+1,0,get(x,y,p.z));
           }
       else if(cyclic)
         b2.operation(bit_op.Copy,1,1,0,this,0,0,0,dx,dy,1);    
       for(y=0;y<=dy;y++)
         for(x=0;x<=dx;x++) {
           r=y*s1+x;
           if(sides||x>0&&x<dx) {
             l0=b2.get(x+1,y+1,0);
             l1=b2.get(x,y+1,0);                       //leva/prava
             if(l0&&!l1) poly(p,r,r+s1,r+s1+s2,r+s2);
             if(l1&&!l0) poly(p,r+s2,r+s1+s2,r+s1,r);
           }
           if(sides||y>0&&y<dy) {  
             l0=b2.get(x+1,y+1,0);
             l1=b2.get(x+1,y,0);			// horni/dolni
             if(l0&&!l1) poly(p,r+s2,r+1+s2,r+1,r);
             if(l1&&!l0) poly(p,r,r+1,r+1+s2,r+s2);  
           }
           if(sides||cyclic||p.z>0&&p.z<dz) {  
             l0=b2.get(x+1,y+1,0);
             l1=b1.get(x+1,y+1,0);                   // predni zadni OK
             if(l0&&!l1) poly(p,r,r+1,r+1+s1,r+s1); 
             if(l1&&!l0) poly(p,r+s1,r+1+s1,r+1,r);
           }  
         }
     }
   } 
   

   public mesh Mesh(bool cubes,bool cyclic) {
     mesh msh=new mesh();
     if(cubes) {
       mesh_poly(msh,cyclic,false);
       msh.fcs.Reverse();
     } else
       mesh_poly2(msh,cyclic);
     return msh;
   }

  }
}
