using OpenTK;
using OpenTK.Graphics.OpenGL;
using XLight_OpenTK.Rendering.Bridges;

namespace XLight_OpenTK.Rendering
{
    public class Mesh
    {
        private readonly Vao vertexData;
        private readonly int mumberOfVertices;

        public Mesh(LightX.Face[] faces, Vector3[] vertices, Vector2[] textureCoordinates, Vector2[] lightmapTextureCoordinates, int[] indices)
        {
            Faces = faces;
            mumberOfVertices = vertices.Length;
            vertexData = new Vao();
            vertexData.Push(0, 3, vertices, BufferTarget.ArrayBuffer);
            vertexData.Push(1, 2, textureCoordinates, BufferTarget.ArrayBuffer);
            vertexData.Push(2, 2, lightmapTextureCoordinates, BufferTarget.ArrayBuffer);
            vertexData.Push(3, 1, indices, BufferTarget.ElementArrayBuffer);
            vertexData.Initialize();
        }
        
        public LightX.Face[] Faces { get; private set; }

        public Vao GetVertexData() 
        {
            return vertexData;
        }

        public int GetNumberOfPoints() 
        {
            return mumberOfVertices;
        }
    }
}