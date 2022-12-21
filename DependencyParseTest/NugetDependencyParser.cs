using System.Text;
using System.Xml.Linq;

namespace DependencyParseTest
{
    public class NugetDependencyParser : DependencyParserBase
    {
        /// <summary>
        /// Returns a <see cref="List{T}"/> of <see cref="PackageInfo"/> based on the provided file path
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public List<PackageInfo> GetPackageInfos(string file)
        {
            using var reader = new StringReader(file);
            var fileContents = File.ReadAllText(file);
            fileContents = HygieneProjectFile(fileContents);
            var projectDoc = XDocument.Parse(fileContents);
            List<XElement> dependencyInfoElements = new List<XElement>();
            dependencyInfoElements = projectDoc.Descendants("PackageReference").ToList();
            var packageInfos = dependencyInfoElements.Select(p => new PackageInfo(p.Attribute("Include").Value.ToString(), HygieneVersion(p.Attribute("Version").Value.ToString()), PackageInfo.PackageSource.Nuget)).ToList();
            return packageInfos;
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

        private string HygieneProjectFile(string file)
        {
            string _byteOrderMarkUtf8 = Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble());
            if (file.StartsWith(_byteOrderMarkUtf8))
            {
                file = file.Remove(0, _byteOrderMarkUtf8.Length);
                if (!file.StartsWith("<"))
                {
                    file = "<" + file;
                }
            }

            return file;
        }
    }
}
