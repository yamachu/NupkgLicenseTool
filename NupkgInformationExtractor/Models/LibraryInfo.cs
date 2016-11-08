using System;
namespace NupkgInformationExtractor.Models
{
	public class LibraryInfo
	{
		public string Title { get; set;} // Non-null

		#region Requires
		public string Id { get; set; }
		public string Authors { get; set; }
		public string LicenseUrl { get; set; }
		#endregion

		#region Optional
		public string Description { get; set; }
		public string Copyright { get; set; }
		public string ProjectUrl { get; set; }
		#endregion
	}
}
