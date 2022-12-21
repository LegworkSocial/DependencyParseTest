using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DependencyParseTest
{
    public class NpmDependencyParser : DependencyParserBase
    {

        /// <summary>
        /// Returns a <see cref="List{T}"/> of <see cref="PackageInfo"/> based on the provided file path
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public List<PackageInfo> GetPackageInfos(string file)
        {
            var fileContents = File.ReadAllText(file);
            var packageInfo = JsonConvert.DeserializeObject<JObject>(fileContents);
            var dependencies = (JObject)packageInfo.SelectToken("dependencies");
            var packageInfos = dependencies
                .Children<JProperty>()
                .Select(p => CreateDependencyInfo(p))
                .ToList();
            return packageInfos.OrderBy(d => d.PackageName).ToList();
        }

        /// <summary>
        /// Writes a csv file to the specified path
        /// </summary>
        /// <param name="filePath"></param>
        /// <exception cref="NotImplementedException"></exception>
        public override void WriteCSVFile(string filePath)
        {
            throw new NotImplementedException();
        }

        private PackageInfo CreateDependencyInfo(JProperty prop)
        {
            var name = prop.Name.Replace("@", "");
            var val = prop.Value.ToString().Replace("^", "").Replace("=", "").Replace("~", "").Trim();
            if (val != null && val.StartsWith("git")) val = "0.0.0";
            if (val != null && val.Contains('-'))
            {
                var index = val.IndexOf("-");
                val = val[..index];
            }
            var packageInfo = new PackageInfo(name, "0.0.0", PackageInfo.PackageSource.NPM);
            packageInfo = new PackageInfo(name, HygieneVersion(val), PackageInfo.PackageSource.NPM);
            return packageInfo;
        }



    }
}
