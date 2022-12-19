namespace DependencyParseTest
{
    public class PackageInfo : IEquatable<PackageInfo>
    {
        public string PackageName { get; set; }
        public Version Version { get; set; }
        public PackageSource Source { get; set; }
        public enum PackageSource
        {
            NPM,
            Nuget
        }

        public PackageInfo(string packageName, string version, PackageSource source)
        {
            if (string.IsNullOrEmpty(version))
            {
                Version = new Version(0, 0, 0);
            }
            else
            {
                Version = new Version(version);
            }
            PackageName = packageName;

            Source = source;
        }

        public bool Equals(PackageInfo? other)
        {
            if (other == null) throw new ArgumentException(nameof(other));
            return other.PackageName == this.PackageName;
        }
        public override bool Equals(object? obj) => Equals(obj as PackageInfo);
        public override int GetHashCode() => (PackageName).GetHashCode();

    }
}
