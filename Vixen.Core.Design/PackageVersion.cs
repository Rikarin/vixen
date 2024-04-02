using System.Text.RegularExpressions;

namespace Vixen.Core.Design;

/// <summary>
///     A hybrid implementation of SemVer that supports semantic versioning as described at http://semver.org while not
///     strictly enforcing it to
///     allow older 4-digit versioning schemes to continue working.
/// </summary>
public sealed partial class PackageVersion : IComparable, IComparable<PackageVersion>, IEquatable<PackageVersion> {
    const RegexOptions Flags = RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture;

    static readonly Regex SemanticVersionRegex = new(
        @"^(?<Version>\d+(\s*\.\s*\d+){0,3})(?<Release>-[0-9a-z]*[\.0-9a-z-]*)?(?<BuildMetadata>\+[0-9a-z]*[\.0-9a-z-]*)?$",
        Flags
    );

    static readonly Regex StrictSemanticVersionRegex = new(
        @"^(?<Version>\d+(\.\d+){2})(?<Release>-[0-9a-z]*[\.0-9a-z-]*)?(?<BuildMetadata>\+[0-9a-z]*[\.0-9a-z-]*)?$",
        Flags
    );

    readonly string originalString;

    /// <summary>
    ///     Defines version 0.
    /// </summary>
    public static readonly PackageVersion Zero = Parse("0");

    /// <summary>
    ///     Gets the normalized version portion.
    /// </summary>
    public Version Version { get; }

    /// <summary>
    ///     Gets the optional special version.
    /// </summary>
    public string? SpecialVersion { get; }

    /// <summary>
    ///     Initializes a new instance of the <see cref="PackageVersion" /> class.
    /// </summary>
    /// <param name="version">The version.</param>
    public PackageVersion(string version) : this(Parse(version)) {
        // The constructor normalizes the version string so that it we do not need to normalize it every time we need to operate on it.
        // The original string represents the original form in which the version is represented to be used when printing.
        originalString = version;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="PackageVersion" /> class.
    /// </summary>
    /// <param name="major">The major.</param>
    /// <param name="minor">The minor.</param>
    /// <param name="build">The build.</param>
    /// <param name="revision">The revision.</param>
    public PackageVersion(int major, int minor, int build, int revision)
        : this(new Version(major, minor, build, revision)) { }

    /// <summary>
    ///     Initializes a new instance of the <see cref="PackageVersion" /> class.
    /// </summary>
    /// <param name="major">The major.</param>
    /// <param name="minor">The minor.</param>
    /// <param name="build">The build.</param>
    /// <param name="specialVersion">The special version.</param>
    public PackageVersion(int major, int minor, int build, string specialVersion)
        : this(new(major, minor, build), specialVersion) { }

    /// <summary>
    ///     Initializes a new instance of the <see cref="PackageVersion" /> class.
    /// </summary>
    /// <param name="version">The version.</param>
    public PackageVersion(Version version) : this(version, string.Empty) { }

    /// <summary>
    ///     Initializes a new instance of the <see cref="PackageVersion" /> class.
    /// </summary>
    /// <param name="version">The version.</param>
    /// <param name="specialVersion">The special version.</param>
    public PackageVersion(Version version, string? specialVersion) : this(version, specialVersion, null) { }

    PackageVersion(Version version, string? specialVersion, string? originalString) {
        if (version == null) {
            throw new ArgumentNullException(nameof(version));
        }

        Version = NormalizeVersionValue(version);
        SpecialVersion = specialVersion ?? string.Empty;
        this.originalString = string.IsNullOrEmpty(originalString)
            ? version + (!string.IsNullOrEmpty(specialVersion) ? '-' + specialVersion : null)
            : originalString;
    }

    internal PackageVersion(PackageVersion semVer) {
        originalString = semVer.ToString();
        Version = semVer.Version;
        SpecialVersion = semVer.SpecialVersion;
    }

    public string[] GetOriginalVersionComponents() {
        if (!string.IsNullOrEmpty(originalString)) {
            // search the start of the SpecialVersion part, if any
            var dashIndex = originalString.IndexOf('-');
            var original = dashIndex != -1 ? originalString.Substring(0, dashIndex) : originalString;

            return SplitAndPadVersionString(original);
        }

        return SplitAndPadVersionString(Version.ToString());
    }

    /// <summary>
    ///     Parses a version string using loose semantic versioning rules that allows 2-4 version components followed by an
    ///     optional special version.
    /// </summary>
    public static PackageVersion Parse(string version) {
        if (string.IsNullOrEmpty(version)) {
            throw new ArgumentNullException(nameof(version), "cannot be null or empty");
        }

        if (!TryParse(version, out var semVer)) {
            throw new ArgumentException($"Invalid version format [{version}]", nameof(version));
        }

        return semVer;
    }

    /// <summary>
    ///     Parses a version string using loose semantic versioning rules that allows 2-4 version components followed by an
    ///     optional special version.
    /// </summary>
    public static bool TryParse(string version, out PackageVersion value) =>
        TryParseInternal(version, SemanticVersionRegex, out value);

    /// <summary>
    ///     Parses a version string using strict semantic versioning rules that allows exactly 3 components and an optional
    ///     special version.
    /// </summary>
    public static bool TryParseStrict(string version, out PackageVersion value) =>
        TryParseInternal(version, StrictSemanticVersionRegex, out value);

    /// <summary>
    ///     Attempts to parse the version token as a SemanticVersion.
    /// </summary>
    /// <returns>An instance of SemanticVersion if it parses correctly, null otherwise.</returns>
    public static PackageVersion ParseOptionalVersion(string version) {
        TryParse(version, out var semVer);
        return semVer;
    }

    public int CompareTo(object? obj) {
        if (ReferenceEquals(obj, null)) {
            return 1;
        }

        var other = obj as PackageVersion;
        if (other == null) {
            throw new ArgumentException("Object must be a SemanticVersion", nameof(obj));
        }

        return CompareTo(other);
    }

    public int CompareTo(PackageVersion? other) {
        if (ReferenceEquals(other, null)) {
            return 1;
        }

        var result = Version.CompareTo(other.Version);

        if (result != 0) {
            return result;
        }

        var empty = string.IsNullOrEmpty(SpecialVersion);
        var otherEmpty = string.IsNullOrEmpty(other.SpecialVersion);
        if (empty && otherEmpty) {
            return 0;
        }

        if (empty) {
            return 1;
        }

        if (otherEmpty) {
            return -1;
        }

        return StringComparer.OrdinalIgnoreCase.Compare(SpecialVersion, other.SpecialVersion);
    }

    public static bool operator ==(PackageVersion version1, PackageVersion version2) => Equals(version1, version2);

    public static bool operator !=(PackageVersion version1, PackageVersion version2) => !Equals(version1, version2);

    public static bool operator <(PackageVersion version1, PackageVersion version2) {
        if (version1 == null) {
            throw new ArgumentNullException(nameof(version1));
        }

        return version1.CompareTo(version2) < 0;
    }

    public static bool operator <=(PackageVersion version1, PackageVersion version2) =>
        version1 == version2 || version1 < version2;

    public static bool operator >(PackageVersion version1, PackageVersion version2) {
        if (version1 == null) {
            throw new ArgumentNullException(nameof(version1));
        }

        return version2 < version1;
    }

    public static bool operator >=(PackageVersion version1, PackageVersion version2) =>
        version1 == version2 || version1 > version2;

    /// <inheritdoc />
    public override string ToString() => originalString;

    /// <inheritdoc />
    public bool Equals(PackageVersion? other) {
        if (ReferenceEquals(other, null)) {
            return false;
        }

        if (ReferenceEquals(other, this)) {
            return true;
        }

        return Version.Equals(other.Version)
            && SpecialVersion.Equals(other.SpecialVersion, StringComparison.OrdinalIgnoreCase);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) {
        if (ReferenceEquals(obj, null)) {
            return false;
        }

        if (ReferenceEquals(obj, this)) {
            return true;
        }

        return Equals(obj as PackageVersion);
    }

    /// <inheritdoc />
    public override int GetHashCode() {
        unchecked {
            var hashCode = Version.GetHashCode();
            if (SpecialVersion != null) {
                hashCode = (hashCode * 4567) ^ SpecialVersion.GetHashCode();
            }

            return hashCode;
        }
    }

    static string[] SplitAndPadVersionString(string version) {
        var a = version.Split('.');
        if (a.Length == 4) {
            return a;
        }

        // if 'a' has less than 4 elements, we pad the '0' at the end
        // to make it 4.
        string[] b = { "0", "0", "0", "0" };
        Array.Copy(a, 0, b, 0, a.Length);
        return b;
    }

    static bool TryParseInternal(string version, Regex regex, out PackageVersion? semVer) {
        semVer = null;
        if (string.IsNullOrEmpty(version)) {
            return false;
        }

        var match = regex.Match(version.Trim());
        if (!match.Success || !Version.TryParse(match.Groups["Version"].Value, out var versionValue)) {
            // Support integer version numbers (i.e. 1 -> 1.0)
            if (int.TryParse(version, out var versionNumber)) {
                semVer = new(new Version(versionNumber, 0));
                return true;
            }

            return false;
        }

        semVer = new(
            NormalizeVersionValue(versionValue),
            match.Groups["Release"].Value.TrimStart('-'),
            version.Replace(" ", string.Empty)
        );
        return true;
    }

    static Version NormalizeVersionValue(Version version) =>
        new(
            version.Major,
            version.Minor,
            System.Math.Max(version.Build, 0),
            System.Math.Max(version.Revision, 0)
        );
}
