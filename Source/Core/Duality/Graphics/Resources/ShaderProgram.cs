using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace Duality.Graphics.Resources
{
    public class ShaderProgram : IDisposable
    {
        public int Handle { get; internal set; }
        public Dictionary<HashedString, int> Uniforms = new Dictionary<HashedString, int>();
        private Dictionary<HashedString, HashedString> _bindNamesToVarNames = new Dictionary<HashedString, HashedString>();
        private readonly Backend _backend;

        public bool HasTeselation = false;

        private object _mutex = new object();
        private readonly List<object> _boundHandles = new List<object>();

        public ShaderProgram(Backend backend)
        {
            Handle = -1;
            _backend = backend;
        }

        public void Dispose()
        {
            if (Handle >= 0)
            {
                _backend.RenderSystem.DestroyShader(Handle);
                Handle = -1;
            }
        }

        internal void Reset()
        {
            Uniforms = new Dictionary<HashedString, int>();
            _bindNamesToVarNames = new Dictionary<HashedString, HashedString>();
        }

        public int GetUniform(HashedString name)
        {
            int uniformLocation;
            if (!Uniforms.TryGetValue(name, out uniformLocation))
            {
                return -1;
            }

            return uniformLocation;
        }

        public void BindUniformLocations<T>(T handles) where T : class
        {
            lock (_mutex)
            {
                if (!_boundHandles.Contains(handles))
                    _boundHandles.Add(handles);

                var type = typeof(T);
                foreach (var field in type.GetFields())
                {
                    if (field.FieldType != typeof(int))
                        continue;

                    var fieldName = field.Name;
                    var uniformName = fieldName.Replace("Handle", "");
                    uniformName = char.ToLower(uniformName[0]) + uniformName.Substring(1);

                    int uniformLocation = GetUniform(uniformName);

                    field.SetValue(handles, uniformLocation);
                }
            }
        }
    }
}
