using System;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL;

namespace XLight_OpenTK.Rendering.Bridges
{
    public class Vbo<T> where T : struct
    {
        public readonly int Id;

        public Vbo(int location, int dim, T[] data, BufferTarget target) 
        {
            GL.GenBuffers(1, out Id);
            GL.BindBuffer(target, Id);
            GL.BufferData(target, (IntPtr) (data.Length * Marshal.SizeOf<T>()), data, BufferUsageHint.DynamicDraw);

            if (target is BufferTarget.ElementArrayBuffer) 
            {
                return;
            }
            
            GL.VertexAttribPointer(location, dim, VertexAttribPointerType.Float, false, 0, 0);
            GL.BindBuffer(target, 0);
        }

        public void Delete() 
        {
            GL.DeleteBuffer(Id);
        }
    }
}