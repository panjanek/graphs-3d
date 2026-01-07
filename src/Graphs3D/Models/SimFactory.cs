using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Graphs3D.Models
{
    public static class SimFactory
    {
        private static readonly JsonSerializerOptions serializerOptions = new JsonSerializerOptions() { IncludeFields = true, WriteIndented = true };
    }
}
