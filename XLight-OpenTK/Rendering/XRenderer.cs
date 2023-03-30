using System;
using Assimp;
using OpenTK.Graphics.OpenGL;
using XLight_OpenTK.Rendering.Shaders;
using PrimitiveType = OpenTK.Graphics.OpenGL.PrimitiveType;

namespace XLight_OpenTK.Rendering
{
    public static class XRenderer
    {
        public static void OnRenderFrame(Mesh mesh, Texture baseTexture, Texture lightmap, ShaderProgram shaderProgram)
        {
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);

            shaderProgram.Start();
            var shaderDataInfo = shaderProgram.DataInfo;

            //Bind main texture
            {
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, baseTexture.Id);
                shaderProgram.SetUniform(shaderDataInfo.GetUniformField(ShaderUniformDataType.BaseTextureSampler), 0);
            }

            //Bind lightmap texture
            {
                GL.ActiveTexture(TextureUnit.Texture1);
                GL.BindTexture(TextureTarget.Texture2D, lightmap.Id);
                shaderProgram.SetUniform(shaderDataInfo.GetUniformField(ShaderUniformDataType.LightmapSampler), 1);
            }

            GL.BindVertexArray(mesh.GetVertexData().GetId());
            GL.EnableVertexAttribArray(0);
            GL.EnableVertexAttribArray(1);
            GL.EnableVertexAttribArray(2);

            shaderProgram.SetUniform(shaderDataInfo.GetUniformField(ShaderUniformDataType.ProjectionMatrix),
                XCamera.ProjectionMatrix);
            shaderProgram.SetUniform(shaderDataInfo.GetUniformField(ShaderUniformDataType.CameraViewMatrix),
                XCamera.ModelViewMatrix);

            GL.DrawElements(PrimitiveType.Triangles, mesh.GetNumberOfPoints(), DrawElementsType.UnsignedInt,
                IntPtr.Zero);

            GL.DisableVertexAttribArray(0);
            GL.DisableVertexAttribArray(1);
            GL.DisableVertexAttribArray(2);
            GL.BindVertexArray(0);
        }
    }
}