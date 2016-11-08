using MonoDevelop.Components.Commands;
using MonoDevelop.Ide;
using System;
using System.Linq;

namespace NupkgLicenseTool.Handlers
{
	public class CheckLicenseHandler : CommandHandler
	{
		protected override void Update(CommandInfo info)
		{
			// not Startup Project, selected Project
			info.Visible = IdeApp.ProjectOperations.CurrentSelectedProject != null;
			info.Text = String.Format("Check license {0}",
			                          IdeApp.ProjectOperations.CurrentSelectedProject?.Name ?? "");
		}

		protected override void Run()
		{
			var monitor = IdeApp.Workbench.ProgressMonitors.GetToolOutputProgressMonitor(true, null);

			var project = IdeApp.ProjectOperations.CurrentSelectedProject;

			var assemblies = (project as MonoDevelop.Projects.DotNetProject)
				.References
				.Where(p => !p.HintPath.IsNullOrEmpty)
				.ToList();
			
			var from_project = (project as MonoDevelop.Projects.DotNetProject)
				.References
				.Where(p => p.ReferenceType == MonoDevelop.Projects.ReferenceType.Project)
				.Select(p => p.Reference)
				.ToList();

			Console.WriteLine("From assembly");

			foreach (var asm in assemblies) {
				Console.WriteLine(asm.Reference +": "+ asm.HintPath);
				var info = NupkgInformationExtractor.Extractor.getLibraryInfo(asm.Reference, asm.HintPath);
				if (info == null) {
					Console.WriteLine(asm.Reference + " is null");
					continue;
				}
				monitor.Console.Out.WriteLine(
					String.Format("- id: {0}" + Environment.NewLine +
				                  "  name: {1}" + Environment.NewLine +
				                  "  authors: {2}" + Environment.NewLine +
				                  "  license: {3}", info.Id, info.Title, info.Authors, info.LicenseUrl ?? " #LICENSE#"));
			}

			Console.WriteLine("From project");

			foreach (var prj in from_project) {
				Console.WriteLine(prj);
			}
		}
	}
}

