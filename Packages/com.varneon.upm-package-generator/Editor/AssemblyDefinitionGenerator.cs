using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using UnityEngine;

namespace Varneon.UPMPackageGenerator.Editor
{
    public static class AssemblyDefinitionGenerator
    {
        public static void CreateAssemblyDefinition(string path, string name, bool editorOnly, params string[] references)
        {
            JObject asmdef = new JObject(
                new JProperty("name", name),
                new JProperty("references", references),
                new JProperty("includePlatforms", editorOnly ? new string[] { "Editor" } : new string[0]),
                new JProperty("excludePlatforms", new string[0]),
                new JProperty("allowUnsafeCode", false),
                new JProperty("overrideReferences", false),
                new JProperty("precompiledReferences", new string[0]),
                new JProperty("autoReferenced", true),
                new JProperty("defineConstraints", new string[0]),
                new JProperty("versionDefines", new string[0]),
                new JProperty("noEngineReferences", false)
                );

            string directoryName = Path.GetDirectoryName(path);

            if (!Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }

            // Write the raw JSON of the assembly definition
            using (StreamWriter writer = new StreamWriter(path))
            {
                writer.Write(JsonConvert.SerializeObject(asmdef, Formatting.Indented));
            }
        }
    }
}
