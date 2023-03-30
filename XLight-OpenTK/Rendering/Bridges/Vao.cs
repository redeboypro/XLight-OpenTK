using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;

namespace XLight_OpenTK.Rendering.Bridges
{
    public class Vao
    {
        private int id;
        private readonly List<int> positions = new List<int>();
        private readonly List<Action> bindings = new List<Action>();
        
        public int GetId()
        {
            return id;
        }

        public int GetVboId(int dataLocation) 
        {
            return positions[dataLocation];
        }

        public void Push<T>(int location, int dim, T[] data, BufferTarget target) where T : struct 
        {
            bindings.Add(() =>
            {
                var vbo = new Vbo<T>(location, dim, data, target);
                positions.Add(vbo.Id);
            });
        }

        public void Initialize() 
        {
            GL.GenVertexArrays(1, out id);
            GL.BindVertexArray(id);
            foreach (var binding in bindings) 
            {
                binding.Invoke();
            }
            GL.BindVertexArray(0);
        }

        public void Delete() 
        {
            GL.BindVertexArray(id);
            foreach (var vboId in positions) 
            {
                GL.DeleteBuffer(vboId);
            }
            GL.DeleteVertexArray(id);
            GL.BindVertexArray(0);
        }
    }
}