using System;
using System.Reflection;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml.XPath;

using NupkgInformationExtractor.Models;

namespace NupkgInformationExtractor
{
	public class Extractor
	{
		public static LibraryInfo getLibraryInfo(string packageName, string dllPath)
		{
			var basePath = dllPath.Split(new string[] { "/lib/" }, StringSplitOptions.RemoveEmptyEntries)?[0];
			var nupkgPath = System.IO.Path.Combine(basePath, basePath.Split('/').Last() + ".nupkg");

			System.Diagnostics.Debug.WriteLine(basePath, System.Reflection.MethodBase.GetCurrentMethod().Name);
			System.Diagnostics.Debug.WriteLine(nupkgPath, System.Reflection.MethodBase.GetCurrentMethod().Name);

			return System.IO.File.Exists(nupkgPath) ?
				         extractLibraryInfoFromNupkg(packageName, nupkgPath) :
				         extractLibraryInfoFromDll(packageName, dllPath);
		}

		private static LibraryInfo extractLibraryInfoFromDll(string packageName, string dllPath)
		{
			var assembly = Assembly.LoadFrom(dllPath);

			var name = assembly.GetName();
			var description = assembly.GetCustomAttribute<AssemblyDescriptionAttribute>().Description;
			var copyright = assembly.GetCustomAttribute<AssemblyCopyrightAttribute>().Copyright;

			return new LibraryInfo()
			{
				Id = name.Name,
				Authors = copyright,
				Description = description,
				Title = packageName
			};
		}

		private static LibraryInfo extractLibraryInfoFromNupkg(string packageName, string nupkgPath)
		{
			System.Diagnostics.Debug.WriteLine(nupkgPath, System.Reflection.MethodBase.GetCurrentMethod().Name);

			using (var archive = ZipFile.OpenRead(nupkgPath))
			{
				var nuspecEntry
				= archive.Entries.First(e => string.Compare(Path.GetExtension(e.Name), ".nuspec", true) == 0);
				using (var stream = nuspecEntry.Open())
				{
					// http://qiita.com/kouh/items/007d8bd95d147a7a60b5
					var entries = System.Xml.Linq.XDocument.Load(stream);
					foreach (var e in entries.Descendants()) e.Name = e.Name.LocalName;

					// https://docs.nuget.org/ndocs/schema/nuspec
					#region Must Appear Element
					var id = entries.XPathSelectElement("package/metadata/id").Value;
					var authors = entries.XPathSelectElement("package/metadata/authors").Value;
					var description = entries.XPathSelectElement("package/metadata/description").Value;
					#endregion

					#region Optional Element
					var title = entries.XPathSelectElement("package/metadata/title")?.Value ?? packageName;
					var licenseUrl = entries.XPathSelectElement("package/metadata/licenseUrl")?.Value;
					var projectUrl = entries.XPathSelectElement("package/metadata/projectUrl")?.Value;
					var copyright = entries.XPathSelectElement("package/metadata/copyright")?.Value;
					#endregion

					return new LibraryInfo()
					{
						Id = id,
						Authors = authors,
						Description = description,
						Title = title,
						LicenseUrl = licenseUrl,
						ProjectUrl = projectUrl,
						Copyright = copyright
					};
				}
			}
		}
	}
}
