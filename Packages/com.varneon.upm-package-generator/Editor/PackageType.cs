namespace Varneon.UPMPackageGenerator.Editor
{
    public enum PackageType
    {
        /// <summary>
        /// Package's files get embedded in the project's "Packages" directory
        /// </summary>
        Embedded,

        /// <summary>
        /// Package gets generated in a custom directory and added as a local package via UPM
        /// </summary>
        Local
    }
}
