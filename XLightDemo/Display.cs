using System;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using XLight_OpenTK.Rendering;
using XLight_OpenTK.Rendering.Shaders;
using static XLight_OpenTK.LightX;

namespace XLightDemo
{
    public class Shader : ShaderProgram
    {
        private const string vertexShader = @"
#version 140

in vec3 position;
in vec2 base_uv;
in vec2 lightmap_uv;

out vec2 pass_base_uv;
out vec2 pass_lightmap_uv;

uniform mat4 projectionMatrix;
uniform mat4 modelMatrix;

void main(void) {
	gl_Position = projectionMatrix * modelMatrix * vec4(position, 1.0);
	pass_base_uv = base_uv;
    pass_lightmap_uv = lightmap_uv;
}
";
        
        private const string fragmentShader = @"
#version 140

in vec2 pass_base_uv;
in vec2 pass_lightmap_uv;

out vec4 out_Color;

uniform sampler2D baseTexture;
uniform sampler2D lightmapTexture;

void main(void) {
	out_Color = texture(baseTexture, pass_base_uv);
    vec4 lightmap = texture(lightmapTexture, pass_lightmap_uv);
    out_Color.rgb *= lightmap.rgb;
}
";

        public Shader(ShaderDataInfo dataInfo) : base(dataInfo, vertexShader, fragmentShader)
        {
            //Nothing to implement
        }

        protected override void BindAttributes() 
        {
            BindAttribute(0, "position");
            BindAttribute(1, "base_uv");
            BindAttribute(2, "lightmap_uv");
        }
    }

    public class Display : GameWindow
    {
        private Mesh mesh;
        private Texture lightmap;
        private Texture texture;
        private Shader shader;

        private bool isPressed;
        private float yaw;
        private float pitch;
        private int zoom = 10;

        public Display()
        {
            Title = "Lightmapping";
            Width = 800;
            Height = 600;
            Run();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Texture2D);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            
            var shaderDataInfo = new ShaderDataInfo();
            shaderDataInfo.PushUniformField(ShaderUniformDataType.BaseTextureSampler, "baseTexture");
            shaderDataInfo.PushUniformField(ShaderUniformDataType.LightmapSampler, "lightmapTexture");
            shaderDataInfo.PushUniformField(ShaderUniformDataType.ProjectionMatrix, "projectionMatrix");
            shaderDataInfo.PushUniformField(ShaderUniformDataType.CameraViewMatrix, "modelMatrix");
            shader = new Shader(shaderDataInfo);
            
            XCamera.WorldLocation = new Vector3(-2.0f, 5.0f, 10.0f);
            //XCamera.Target = new Vector3(-2.0f, 0, 0);
            XCamera.CreateProjectionMatrix(this);
            
            texture = new Texture(new Bitmap("test.png"));
            
            var rawMesh = LoadMesh("test.obj");
            (mesh, lightmap) = Bake(rawMesh, new Vector3(0.5f, -1.0f, 0.5f), 128, 4);
            
            //ExportXml("test.txt", mesh);
            //lightmap.Save("lightmap.png");
            
            //mesh = ImportXml("test.txt");
            //lightmap = new Texture(new Bitmap("lightmap.png"));
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);
            zoom -= e.Delta;
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButton.Left)
            {
                isPressed = true;
            }
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);
            if (e.Button == MouseButton.Left)
            {
                isPressed = false;
            }
        }

        protected override void OnMouseMove(MouseMoveEventArgs e)
        {
            base.OnMouseMove(e);
            if (isPressed)
            {
                yaw += e.XDelta;
                if (yaw > 360 || yaw < -360)
                {
                    yaw = 0;
                }

                pitch += e.YDelta;
                pitch = MathHelper.Clamp(pitch, -89.0f, 89.0f);
            }
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            if (!isPressed)
            {
                yaw += (float) e.Time * 15;
            }

            var x = (float) Math.Sin(MathHelper.DegreesToRadians(yaw));
            
            var y = (float) Math.Tan(MathHelper.DegreesToRadians(pitch));
            
            var z = (float) -Math.Cos(MathHelper.DegreesToRadians(yaw));
            
            XCamera.WorldLocation = new Vector3(x, y, z).Normalized() * zoom;
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            GL.Viewport(0, 0, Width, Height);
            GL.ClearColor(Color.LightSlateGray);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            XRenderer.OnRenderFrame(mesh, texture, lightmap, shader);
            SwapBuffers();
        }
    }
}