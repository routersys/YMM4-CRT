using System;
using System.IO;
using System.Reflection;

namespace CRT
{
    internal class ShaderResourceLoader
    {
        public static byte[] GetShaderResource(string name)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = $"CRT.Shaders.{name}";
            using var stream = assembly.GetManifestResourceStream(resourceName) ??
                throw new Exception($"Resource {resourceName} not found.");
            var bytes = new byte[stream.Length];
            int offset = 0;
            int read;
            while ((read = stream.Read(bytes, offset, bytes.Length - offset)) > 0)
            {
                offset += read;
            }
            if (offset != bytes.Length)
            {
                throw new EndOfStreamException($"Could not read the entire resource {resourceName}.");
            }
            return bytes;
        }
    }
}