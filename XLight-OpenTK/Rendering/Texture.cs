using System.Drawing;
using System.Drawing.Imaging;
using OpenTK.Graphics.OpenGL;
using PixelFormat = OpenTK.Graphics.OpenGL.PixelFormat;

namespace XLight_OpenTK.Rendering
{
    public class Texture
    {
        public readonly int Id;
        private readonly Bitmap bitmap;

        public Texture(Bitmap bitmap)
        {
            this.bitmap = bitmap;
            
            GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);
            GL.GenTextures(1, out Id);
            GL.BindTexture(TextureTarget.Texture2D, Id);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

            var safeData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, safeData.Width, safeData.Height, 0, 
                PixelFormat.Bgra, PixelType.UnsignedByte, safeData.Scan0);

            bitmap.UnlockBits(safeData);
        }

        public int GetBitmapWidth()
        {
            return bitmap.Width;
        }
        
        public int GetBitmapHeight()
        {
            return bitmap.Height;
        }
        
        public void Save(string fileName) 
        {
            bitmap.Save(fileName);
        }
        
        public void Delete() 
        {
            GL.DeleteTexture(Id);
        }
    }
}