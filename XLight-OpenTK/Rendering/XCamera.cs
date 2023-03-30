using OpenTK;

namespace XLight_OpenTK.Rendering
{
    public class XCamera
    {
        private static Vector3 worldLocation = Vector3.Zero;
        private static Vector3 target = Vector3.Zero;
        
        public static Vector3 WorldLocation 
        { 
            get
            {
                return worldLocation;
            }
            set
            {
                worldLocation = value;
                UpdateModelViewMatrix();
            }
        }
        
        public static Vector3 Target 
        { 
            get
            {
                return target;
            }
            set
            {
                target = value;
                UpdateModelViewMatrix();
            }
        }

        public static Matrix4 ModelViewMatrix { get; private set; } = Matrix4.Identity;
        
        public static Matrix4 ProjectionMatrix { get; private set; } = Matrix4.Identity;

        public static float Fov { get; set; } = MathHelper.PiOver4;

        public static void CreateProjectionMatrix(INativeWindow display)
        {
            ProjectionMatrix = Matrix4.CreatePerspectiveFieldOfView(Fov, (float) display.Width / display.Height, 0.01f, 100.0f);
        }
        
        private static void UpdateModelViewMatrix()
        {
            ModelViewMatrix = Matrix4.LookAt(worldLocation, target, Vector3.UnitY);
        }
    }
}