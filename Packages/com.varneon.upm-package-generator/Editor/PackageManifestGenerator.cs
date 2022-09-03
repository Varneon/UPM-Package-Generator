using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;

namespace Varneon.UPMPackageGenerator.Editor
{
    internal static class PackageManifestGenerator
    {
        internal static void GenerateManifest(string path, string name, string displayName, string authorName, string authorEmail, string authorURL)
        {
            // Generate the base properties of the manifest
            JObject manifest = new JObject(
                new JProperty("name", name),
                new JProperty("version", "0.0.1"),
                new JProperty("description", string.Empty),
                new JProperty("displayName", displayName)
                );

            // State for knowing if author info should be added
            bool addAuthorInfo = false;

            // Create a object for author info properties
            JObject authorInfo = new JObject();

            // Check what author info was provided and add them if they exist
            if (!string.IsNullOrEmpty(authorName)) { authorInfo.Add(new JProperty("name", authorName)); addAuthorInfo = true; }
            if (!string.IsNullOrEmpty(authorEmail)) { authorInfo.Add(new JProperty("email", authorEmail)); addAuthorInfo = true; }
            if (!string.IsNullOrEmpty(authorURL)) { authorInfo.Add(new JProperty("url", authorURL)); addAuthorInfo = true; }

            // If any of the author properties was provided, add author info object
            if (addAuthorInfo)
            {
                manifest.Add(new JProperty("author", authorInfo));
            }

            // Write the manifest file at the provided path
            using (StreamWriter writer = new StreamWriter(path))
            {
                writer.Write(JsonConvert.SerializeObject(manifest, Formatting.Indented));
            }
        }
    }
}
