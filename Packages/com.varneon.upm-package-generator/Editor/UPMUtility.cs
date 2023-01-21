using System.IO;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;

namespace Varneon.UPMPackageGenerator.Editor
{
    public static class UPMUtility
    {
        /// <summary>
        /// Adds a local package
        /// </summary>
        /// <param name="manifestDirectory"></param>
        /// <exception cref="DirectoryNotFoundException">The provided directory doesn't exist</exception>
        /// <exception cref="FileNotFoundException">The provided directory doesn't contain 'package.json' file</exception>
        public static void AddLocalPackage(string manifestDirectory)
        {
            if (!Directory.Exists(manifestDirectory)) { throw new DirectoryNotFoundException("The provided directory doesn't exist"); }

            if(!File.Exists(Path.Combine(manifestDirectory, "package.json"))) { throw new FileNotFoundException("The provided directory doesn't contain 'package.json' file"); }

            AddRequest addRequest = Client.Add(string.Format("file:{0}", manifestDirectory));
        }
    }
}
