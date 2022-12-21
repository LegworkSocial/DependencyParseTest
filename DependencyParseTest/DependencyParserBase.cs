using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NuGet.Common;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
namespace DependencyParseTest
{
    public abstract class DependencyParserBase
    {
        public const string DEFAULT_VERSION_NUMBER = "0.0.0";

        /// <summary>
        /// returns a <see cref="PackageInfo"/> with the version set to the latest version available from Nuget
        /// </summary>
        /// <param name="currentPackageInfo"></param>
        /// <returns></returns>
        private async Task<PackageInfo> GetNugetMaxVersionValuesAsync(PackageInfo currentPackageInfo)
        {
            var nugetPackageInfo = new PackageInfo(currentPackageInfo.PackageName, currentPackageInfo.Version.ToString(), currentPackageInfo.Source);
            ILogger logger = NullLogger.Instance;
            CancellationToken cancellationToken = CancellationToken.None;
            SourceCacheContext cache = new();
            SourceRepository repository = Repository.Factory.GetCoreV3("https://api.nuget.org/v3/index.json");
            FindPackageByIdResource resource = await repository.GetResourceAsync<FindPackageByIdResource>();

            var packageVersions = await resource.GetAllVersionsAsync(currentPackageInfo.PackageName, cache, logger, cancellationToken);

            nugetPackageInfo.Version = packageVersions.OrderByDescending(v => HygieneVersion(v.Version.ToString())).FirstOrDefault().Version;

            return nugetPackageInfo;
        }

        /// <summary>
        /// Returns a <see cref="PackageInfo" /> with the version set to the latest version available from NPM
        /// </summary>
        /// <param name="currentPackageInfo"></param>
        /// <returns></returns>
        private async Task<PackageInfo> GetNPMMaxVersionValueAsync(PackageInfo currentPackageInfo)
        {
            using (var client = new HttpClient())
            {
                var npmPackageInfo = new PackageInfo(currentPackageInfo.PackageName, currentPackageInfo.Version.ToString(), currentPackageInfo.Source);

                string maxVersion = DEFAULT_VERSION_NUMBER;
                var packageName = currentPackageInfo.PackageName;
                var npmCacheStream = await DownloadFileToStreamAsync($"https://registry.npmjs.org/{packageName}");
                if (npmCacheStream == null)
                {
                    packageName = "@" + packageName;
                    npmCacheStream = await DownloadFileToStreamAsync($"https://registry.npmjs.org/{packageName}");
                }
                if (npmCacheStream != null)
                {
                    var npmPackageJson = GetJsonFromStream<JObject>(npmCacheStream);
                    maxVersion = npmPackageJson["dist-tags"]["latest"].Value<string>();
                }
                npmPackageInfo.Version = new Version(HygieneVersion(maxVersion));
                return npmPackageInfo;
            }

        }

        private T GetJsonFromStream<T>(Stream stream)
        {
            var serializer = new JsonSerializer();
            using var sr = new StreamReader(stream);
            using var jsonTextReader = new JsonTextReader(sr);
            return serializer.Deserialize<T>(jsonTextReader);
        }

        private async Task<Stream?> DownloadFileToStreamAsync(string url)
        {
            using (HttpClient client = new())
            {
                try
                {
                    var response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();
                    var responseBody = await response.Content.ReadAsStreamAsync();
                    return responseBody;
                }
                catch (HttpRequestException httpEx)
                {
                    var message = $"HttpError - Url: {url} Message :{httpEx.Message}";
                }
                catch (Exception ex)
                {
                    var message = $"Error - Url: {url} Message :{ex.Message}";
                }
            }
            return null;
        }

        /// <summary>
        /// Replaces extra \ and single " with blanks
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string HygieneString(string input)
        {
            input = input.Replace("\"", "").Replace("\\", "");
            return input;
        }

        /// <summary>
        /// Returns a string that can be parsed by the <see cref="Version"/> class
        /// </summary>
        /// <param name="version"></param>
        /// <returns></returns>
        public static string HygieneVersion(string version)
        {
            if (string.IsNullOrEmpty(version))
            {
                version = DEFAULT_VERSION_NUMBER;
            }
            version = HygieneString(version);

            if (version.Contains('-'))
            {
                var dashIndex = version.IndexOf("-");
                version = version[..dashIndex];
            }

            var vals = version.Split(".").ToList();
            while (vals.Count < 3)
            {
                vals.Add("0");
                version = String.Join(".", vals);
            }
            return version;

        }

        public abstract void WriteCSVFile(string filePath);
    }
}
