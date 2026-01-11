using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Graphs3D.Utils
{
    public static class ResourceUtil
    {
        public static string LoadStringFromResource(string name)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = $"Graphs3D.{name}";
            using Stream stream = assembly.GetManifestResourceStream(resourceName) ?? throw new InvalidOperationException($"Resource not found: {resourceName}");
            using StreamReader reader = new StreamReader(stream);
            var str = reader.ReadToEnd();
            return str;
        }
    }
}
