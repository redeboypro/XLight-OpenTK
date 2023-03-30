using System.Collections.Generic;

namespace XLight_OpenTK.Rendering.Shaders
{
    public enum ShaderUniformDataType { 
        ProjectionMatrix, 
        CameraViewMatrix,
        LightmapSampler,
        BaseTextureSampler
    }
    
    public class ShaderDataInfo
    {
        private readonly Dictionary<ShaderUniformDataType, string> uniformFields = new Dictionary<ShaderUniformDataType, string>();

        public string GetUniformField(ShaderUniformDataType type) 
        {
            return uniformFields[type];
        }

        public void PushUniformField(ShaderUniformDataType type, string fieldName) 
        {
            uniformFields.Add(type, fieldName);
        }
    }
}