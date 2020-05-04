using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Xharness.BCLTestImporter.Templates;
using Xharness.BCLTestImporter.Templates.Managed;
using Xharness.BCLTestImporter.Xamarin;

namespace Xharness.BCLTestImporter {
	/// <summary>
	/// Class that knows how to generate .csproj files based on a BCLTestProjectDefinition.
	/// </summary>
	public class BCLTestProjectGenerator {

		// less typing
		class ProjectsDefinitions : Dictionary<string, (string ExtraArgs, double TimeoutMultiplier, (string Name, string [] Assemblies) [] Projects)> { } 

		// we have two different types of list, those that are for the iOS like projects (ios, tvos and watch os) and those 
		// for mac
		static readonly ProjectsDefinitions commoniOSTestProjects = new ProjectsDefinitions { 
			// NUNIT TESTS

			["BCL tests group 1"] = (
				ExtraArgs: null,
				TimeoutMultiplier: 1,
				Projects: new [] {
					(Name: "SystemCoreTests", Assemblies: new [] { "monotouch_System.Core_test.dll" }),
					(Name: "SystemNumericsTests", Assemblies: new [] { "monotouch_System.Numerics_test.dll" }),
					(Name: "SystemRuntimeSerializationTests", Assemblies: new [] { "monotouch_System.Runtime.Serialization_test.dll" }),
					(Name: "SystemTransactionsTests", Assemblies: new [] { "monotouch_System.Transactions_test.dll" }),
					(Name: "SystemXmlTests", Assemblies: new [] { "monotouch_System.Xml_test.dll" }),
					(Name: "SystemXmlLinqTests", Assemblies: new [] { "monotouch_System.Xml.Linq_test.dll" }),
					(Name: "MonoSecurityTests", Assemblies: new [] { "monotouch_Mono.Security_test.dll" }),
					(Name: "SystemComponentModelDataAnnotationsTests", Assemblies: new [] { "monotouch_System.ComponentModel.DataAnnotations_test.dll" }),
					(Name: "SystemJsonTests", Assemblies: new [] { "monotouch_System.Json_test.dll" }),
					(Name: "SystemServiceModelWebTests", Assemblies: new [] { "monotouch_System.ServiceModel.Web_test.dll" }),
					(Name: "SystemIOCompressionTests", Assemblies: new [] { "monotouch_System.IO.Compression_test.dll" }),
					(Name: "SystemIOCompressionFileSystemTests", Assemblies: new [] { "monotouch_System.IO.Compression.FileSystem_test.dll" }),
					(Name: "MonoCSharpTests", Assemblies: new [] { "monotouch_Mono.CSharp_test.dll" }),
					(Name: "SystemSecurityTests", Assemblies: new [] { "monotouch_System.Security_test.dll" }),
					(Name: "MonoDataSqliteTests", Assemblies: new [] { "monotouch_Mono.Data.Sqlite_test.dll" }),
					(Name: "MonoRuntimeTests", Assemblies: new [] { "monotouch_Mono.Runtime.Tests_test.dll" }),
			}),

			["BCL tests group 2"] = (
				ExtraArgs: null,
				TimeoutMultiplier: 2,
				Projects: new [] {
					( Name: "SystemTests", Assemblies: new[] {"monotouch_System_test.dll" }),
					( Name: "SystemDataTests", Assemblies: new [] { "monotouch_System.Data_test.dll" }),
					( Name: "SystemDataDataSetExtensionsTests", Assemblies: new [] { "monotouch_System.Data.DataSetExtensions_test.dll" }),
					( Name: "SystemNetHttpTests", Assemblies: new [] { "monotouch_System.Net.Http_test.dll" }),
					( Name: "MonoDataTdsTests", Assemblies: new [] { "monotouch_Mono.Data.Tds_test.dll" }),
					( Name: "SystemServiceModelTests", Assemblies: new [] { "monotouch_System.ServiceModel_test.dll" }),
					( Name: "CorlibTests", Assemblies: new [] { "monotouch_corlib_test.dll" }),
					( Name: "SystemWebServicesTests", Assemblies: new [] { "monotouch_System.Web.Services_test.dll" }),
			}),

			// XUNIT TESTS 

			["BCL tests group 3"] = (
				ExtraArgs: null,
				TimeoutMultiplier: 1,
				Projects: new [] {
					( Name: "SystemDataXunit", Assemblies: new [] { "monotouch_System.Data_xunit-test.dll" }),
					( Name: "SystemJsonXunit", Assemblies: new [] { "monotouch_System.Json_xunit-test.dll" }),
					( Name: "SystemSecurityXunit", Assemblies: new [] { "monotouch_System.Security_xunit-test.dll" }),
					( Name: "SystemLinqXunit", Assemblies: new [] { "monotouch_System.Xml.Linq_xunit-test.dll" }),
					( Name: "SystemComponentModelCompositionXunit", Assemblies: new [] { "monotouch_System.ComponentModel.Composition_xunit-test.dll" }),
					( Name: "SystemRuntimeSerializationXunit", Assemblies: new [] { "monotouch_System.Runtime.Serialization_xunit-test.dll" }),
					( Name: "SystemXmlXunit", Assemblies: new [] { "monotouch_System.Xml_xunit-test.dll" }),
					( Name: "SystemRuntimeCompilerServicesUnsafeXunit", Assemblies: new [] { "monotouch_System.Runtime.CompilerServices.Unsafe_xunit-test.dll" }),
			}),

			["BCL tests group 4"] = (
				ExtraArgs: null,
				TimeoutMultiplier: 1,
				Projects: new [] {
					( Name: "SystemNumericsXunit", Assemblies: new [] { "monotouch_System.Numerics_xunit-test.dll" }),
					( Name: "MicrosoftCSharpXunit", Assemblies: new [] { "monotouch_Microsoft.CSharp_xunit-test.dll" }),
			}),

			// BCL tests group 5
			["BCL tests group 5"] = (
				ExtraArgs: null,
				TimeoutMultiplier: 1,
				Projects: new [] {
					( Name: "SystemNetHttpUnitTestsXunit", Assemblies: new [] { "monotouch_System.Net.Http.UnitTests_xunit-test.dll" }),
					( Name: "SystemNetHttpFunctionalTestsXunit", Assemblies: new [] { "monotouch_System.Net.Http.FunctionalTests_xunit-test.dll" }),
			}),

			// Special assemblies that are in a single application due to their size being to large for the iOS 32b.
			["mscorlib Part 1"] = (
				ExtraArgs: null,
				TimeoutMultiplier: 1,
				Projects: new [] {
					( Name: "mscorlib Part 1", Assemblies: new [] { "monotouch_corlib_xunit-test.part1.dll" }), // special testcase for the corlib which is later used in xHarness for diff config options
			}),
			["mscorlib Part 2"] = (
				ExtraArgs: null,
				TimeoutMultiplier: 1,
				Projects: new [] {
					( Name: "mscorlib Part 2", Assemblies: new [] { "monotouch_corlib_xunit-test.part2.dll" }),
			}),
			["mscorlib Part 3"] = (
				ExtraArgs: null,
				TimeoutMultiplier: 1,
				Projects: new [] {
					( Name: "mscorlib Part 3", Assemblies: new [] { "monotouch_corlib_xunit-test.part3.dll" }),
			}),
			["SystemCoreXunit Part 1"] = (
				ExtraArgs: null,
				TimeoutMultiplier: 1,
				Projects: new [] {
					( Name: "SystemCoreXunit Part 1", Assemblies: new [] { "monotouch_System.Core_xunit-test.part1.dll" }),
			}),
			["SystemCoreXunit Part 2"] = (
				ExtraArgs: null,
				TimeoutMultiplier: 1,
				Projects: new [] {
					( Name: "SystemCoreXunit Part 2", Assemblies: new [] { "monotouch_System.Core_xunit-test.part2.dll" }),
			}),
			["SystemXunit"] = (
				ExtraArgs: $"--xml={Path.Combine (Harness.RootDirectory, "bcl-test", "SystemXunitLinker.xml")} --optimize=-custom-attributes-removal", // special case due to the need of the extra args,
				TimeoutMultiplier: 1,
				Projects: new [] {
					( Name: "SystemXunit", Assemblies: new [] { "monotouch_System_xunit-test.dll" }), 
			}),
		};

		private static readonly ProjectsDefinitions macTestProjects = new ProjectsDefinitions { 

			// NUNIT Projects
			["Mac OS X BCL tests group 1"] = (
				ExtraArgs: null,
				TimeoutMultiplier: 1,
				Projects: new [] {
					( Name: "MonoDataSqliteTests", Assemblies: new [] { "xammac_net_4_5_Mono.Data.Sqlite_test.dll" }),
					( Name: "MonoDataTdsTests", Assemblies: new [] { "xammac_net_4_5_Mono.Data.Tds_test.dll" }),
					( Name: "MonoMessagingTests", Assemblies: new [] { "xammac_net_4_5_Mono.Messaging_test.dll" }),
					( Name: "MonoPoxisTests", Assemblies: new [] { "xammac_net_4_5_Mono.Posix_test.dll" }),
					( Name: "MonoSecurityTests", Assemblies: new [] { "xammac_net_4_5_Mono.Security_test.dll" }),
					( Name: "SystemConfigurationTests", Assemblies: new [] { "xammac_net_4_5_System.Configuration_test.dll" }),
					( Name: "SystemDataLinqTests", Assemblies: new [] { "xammac_net_4_5_System.Data.Linq_test.dll" }),
					( Name: "SystemDataTests", Assemblies: new [] { "xammac_net_4_5_System.Data_test.dll" }),
					( Name: "SystemIOCompressionFileSystemTests", Assemblies: new [] { "xammac_net_4_5_System.IO.Compression.FileSystem_test.dll" }),
					( Name: "SystemIOCompressionTests", Assemblies: new [] { "xammac_net_4_5_System.IO.Compression_test.dll" }),
					( Name: "SystemIdentityModelTests", Assemblies: new [] { "xammac_net_4_5_System.IdentityModel_test.dll" }),
					( Name: "SystemJsonTests", Assemblies: new [] { "xammac_net_4_5_System.Json_test.dll" }),
					( Name: "SystemMessagingTests", Assemblies: new [] { "xammac_net_4_5_System.Messaging_test.dll" }),
					( Name: "SystemNetHttpWebRequestTests", Assemblies: new [] { "xammac_net_4_5_System.Net.Http.WebRequest_test.dll" }),
					( Name: "SystemNumericsTests", Assemblies: new [] { "xammac_net_4_5_System.Numerics_test.dll" }),
					( Name: "SystemRuntimeSerializationFormattersSoapTests", Assemblies: new [] { "xammac_net_4_5_System.Runtime.Serialization.Formatters.Soap_test.dll" }),
					( Name: "SystemRuntimeSerializationTests", Assemblies: new [] { "xammac_net_4_5_System.Runtime.Serialization_test.dll" }),
					( Name: "SystemServiceModelWebTest", Assemblies: new [] { "xammac_net_4_5_System.ServiceModel.Web_test.dll" }),
					( Name: "SystemServiceModelTests", Assemblies: new [] { "xammac_net_4_5_System.ServiceModel_test.dll" }),
					( Name: "SystemTransactionsTests", Assemblies: new [] { "xammac_net_4_5_System.Transactions_test.dll" }),
					( Name: "SystemWebServicesTests", Assemblies: new [] { "xammac_net_4_5_System.Web.Services_test.dll" }),
					( Name: "SystemXmlLinqTests", Assemblies: new [] { "xammac_net_4_5_System.Xml.Linq_test.dll" }),
					( Name: "SystemDataDataSetExtensionsTests", Assemblies: new [] { "xammac_net_4_5_System.Data.DataSetExtensions_test.dll" }),
			}),

			["Mac OS X BCL tests group 2"] = (
				ExtraArgs: null,
				TimeoutMultiplier: 1,
				Projects: new [] {
					( Name: "SystemNetHttpTests", Assemblies: new [] { "xammac_net_4_5_System.Net.Http_test.dll" }), // do not mix with SystemNetHttpWebRequestTests since there is a type collision
					( Name: "SystemComponentModelDataAnnotationsTests", Assemblies: new [] { "xammac_net_4_5_System.ComponentModel.DataAnnotations_test.dll" }),
					( Name: "SystemCoreTests", Assemblies: new [] { "xammac_net_4_5_System.Core_test.dll" }),
					( Name: "SystemSecurityTests", Assemblies: new [] { "xammac_net_4_5_System.Security_test.dll" }),
					( Name: "SystemXmlTests", Assemblies: new [] { "xammac_net_4_5_System.Xml_test.dll" }),
					( Name: "SystemTests", Assemblies: new [] { "xammac_net_4_5_System_test.dll" }),
					( Name: "MonoCSharpTests", Assemblies: new [] { "xammac_net_4_5_Mono.CSharp_test.dll" }), // if add to the first group, it blocks 'til a timeout, mono issue
			}),

			["Mac OS X BCL tests group 3"] = (
				ExtraArgs: null,
				TimeoutMultiplier: 1,
				Projects: new [] {
					( Name: "CorlibTests", Assemblies: new [] { "xammac_net_4_5_corlib_test.dll" }),
			}),

			// xUnit Projects
			["Mac OS X BCL tests group 4"] = (
				ExtraArgs: null,
				TimeoutMultiplier: 1,
				Projects: new [] {
					( Name: "MicrosoftCSharpXunit", Assemblies: new [] { "xammac_net_4_5_Microsoft.CSharp_xunit-test.dll" }),
					( Name: "SystemComponentModelCompositionXunit", Assemblies: new [] { "xammac_net_4_5_System.ComponentModel.Composition_xunit-test.dll" }),
					( Name: "SystemCoreXunit", Assemblies: new [] { "xammac_net_4_5_System.Core_xunit-test.dll" }),
					( Name: "SystemDataXunit", Assemblies: new [] { "xammac_net_4_5_System.Data_xunit-test.dll" }),
					( Name: "SystemJsonXunit", Assemblies: new [] { "xammac_net_4_5_System.Json_xunit-test.dll" }),
					( Name: "SystemNetHttpFunctionalTestsXunit", Assemblies: new [] { "xammac_net_4_5_System.Net.Http.FunctionalTests_xunit-test.dll" }),
					( Name: "SystemNetHttpUnitTestsXunit", Assemblies: new [] { "xammac_net_4_5_System.Net.Http.UnitTests_xunit-test.dll" }),
					( Name: "SystemNumericsXunit", Assemblies: new [] { "xammac_net_4_5_System.Numerics_xunit-test.dll" }),
					( Name: "SystemRuntimeCompilerServicesUnsafeXunit", Assemblies: new [] { "xammac_net_4_5_System.Runtime.CompilerServices.Unsafe_xunit-test.dll" }),
					( Name: "SystemSecurityXunit", Assemblies: new [] { "xammac_net_4_5_System.Security_xunit-test.dll" }),
					( Name: "SystemXmlLinqXunit", Assemblies: new [] { "xammac_net_4_5_System.Xml.Linq_xunit-test.dll" }),
					( Name: "SystemXmlXunit", Assemblies: new [] { "xammac_net_4_5_System.Xml_xunit-test.dll" }),
					( Name: "SystemXunit", Assemblies: new [] { "xammac_net_4_5_System_xunit-test.dll" }),
			}),
			// special testcase for the corlib which is later used in xHarness for diff config options
			["mscorlib"] = (
				ExtraArgs: null,
				TimeoutMultiplier: 1,
				Projects: new [] {
					( Name: "mscorlib", Assemblies: new [] { "xammac_net_4_5_corlib_xunit-test.dll" }),
			}),
		};

		public bool GroupTests { get; set; }
		public string OutputDirectoryPath { get; private  set; }
		public string MonoRootPath { get; private set; }

		public Func<string, Guid> GuidGenerator {
			get => TemplatedProject.GuidGenerator;
			set => TemplatedProject.GuidGenerator = value;
		}

		public string iOSMonoSDKPath {
			get {
				var locator = AssemblyLocator as AssemblyLocator;
				return locator?.iOSMonoSDKPath;
			}
			set {
				var locator = AssemblyLocator as AssemblyLocator;
				if (locator != null)
					locator.iOSMonoSDKPath = value;
			}
		}

		public string MacMonoSDKPath {
			get {
				var locator = AssemblyLocator as AssemblyLocator;
				return locator?.MacMonoSDKPath;
			}
			set {
				var locator = TemplatedProject.AssemblyLocator as AssemblyLocator;
				if (locator != null)
					locator.MacMonoSDKPath = value;
			}
		}

		public IAssemblyLocator AssemblyLocator { get; set; }
		public IProjectFilter ProjectFilter { get; set; }
		public ITemplatedProject TemplatedProject { get; set; }
		public ITestAssemblyDefinitionFactory AssemblyDefinitionFactory { get; set; }

		public BCLTestProjectGenerator (string outputDirectory)
		{
			OutputDirectoryPath = outputDirectory ?? throw new ArgumentNullException (nameof (outputDirectory));
			AssemblyLocator = new AssemblyLocator ();
			ProjectFilter = new ProjectFilter { AssemblyLocator = AssemblyLocator, IgnoreFilesRootDir = outputDirectory };
			AssemblyDefinitionFactory = new AssemblyDefinitionFactory ();
			TemplatedProject = new XamariniOSTemplate {
				AssemblyLocator = AssemblyLocator,
				OutputDirectoryPath = outputDirectory,
				IgnoreFilesRootDirectory = outputDirectory,
				ProjectFilter = ProjectFilter,
				AssemblyDefinitionFactory = AssemblyDefinitionFactory, 
			};
		}
		
		public BCLTestProjectGenerator (string outputDirectory, string monoRootPath) : this (outputDirectory)
		{
			MonoRootPath = monoRootPath ?? throw new ArgumentNullException (nameof (monoRootPath));
		}

		/// <summary>
		/// Generates all the project files for the given projects and platform
		/// </summary>
		/// <param name="projects">The list of projects to be generated.</param>
		/// <param name="platform">The platform to which the projects have to be generated. Each platform
		/// has its own details.</param>
		/// <param name="generatedDir">The dir where the projects will be saved.</param>
		/// <returns></returns>
		public Task<GeneratedProjects> GenerateTestProjectsAsync (IEnumerable<(string Name, string [] Assemblies, string ExtraArgs, double TimeoutMultiplier)> projects, Platform platform)
			=> TemplatedProject.GenerateTestProjectsAsync (projects, platform);
		
		List<(string Name, string [] Assemblies, string ExtraArgs, double TimeoutMultiplier)> GetProjectDefinitions (ProjectsDefinitions definitions, Platform platform)
		{
			var testProjects = new List<(string Name, string [] Assemblies, string ExtraArgs, double TimeoutMultiplier)> ();
			if (platform == Platform.WatchOS || !GroupTests) { 
				// go over the keys which define the groups, and split them
				foreach (var groupName in definitions.Keys) {
					var (ExtraArgs, TimeoutMultiplier, Projects) = definitions [groupName];
					foreach (var (Name, Assemblies) in Projects) { 
						testProjects.Add ((Name, Assemblies, ExtraArgs, TimeoutMultiplier));
					}
				}
			} else {
				// go over the keys, which define the groups and join all the asseblies in a single array
				foreach (var groupName in definitions.Keys) {
					var groupedAssemblies = new List<string> ();
					var (ExtraArgs, TimeoutMultiplier, Projects) = definitions [groupName];
					foreach (var (_, Assemblies) in Projects) {
						foreach (var a in Assemblies) {
							if (ProjectFilter == null || !ProjectFilter.ExcludeDll (platform, a))
								groupedAssemblies.AddRange (Assemblies);
						}
					}
					testProjects.Add ((Name: groupName, Assemblies: groupedAssemblies.ToArray (), ExtraArgs, TimeoutMultiplier));
				}
			}
			return testProjects;
		}

		async Task<List<(string Name, string Path, bool XUnit, string ExtraArgs, List<Platform> Platforms, string Failure, double TimeoutMultiplier)>> GenerateAlliOSTestProjectsAsync ()
		{
			var projectPaths = new List<(string Name, string Path, bool XUnit, string ExtraArgs, List<Platform> Platforms, string Failure, double TimeoutMultiplier)> ();
			var projects = new Dictionary<string, (string Path, bool XUnit, string ExtraArgs, List<Platform> Platforms, string Failure, double TimeoutMultiplier)> ();
			foreach (var platform in new [] { Platform.iOS, Platform.TvOS, Platform.WatchOS }) {
				var generated = await GenerateTestProjectsAsync (GetProjectDefinitions (commoniOSTestProjects, platform), platform);
				foreach (var tp in generated) {
					if (!projects.ContainsKey (tp.Name)) {
						projects [tp.Name] = (tp.Path, tp.XUnit, tp.ExtraArgs, new List<Platform> { platform }, tp.Failure, tp.TimeoutMultiplier);
					} else {
						var project = projects [tp.Name];
						project.Platforms.Add (platform);
						project.TimeoutMultiplier += (tp.TimeoutMultiplier - 1);
					}
				}
			} // foreach platform

			// return the grouped projects
			foreach (var name in projects.Keys) {
				projectPaths.Add ((name, projects [name].Path, projects [name].XUnit, projects [name].ExtraArgs, projects [name].Platforms, projects [name].Failure, projects [name].TimeoutMultiplier));
			}
			return projectPaths;
		}

		public List<(string Name, string Path, bool XUnit, string ExtraArgs, List<Platform> Platforms, string Failure, double TimeoutMultiplier)> GenerateAlliOSTestProjects () => GenerateAlliOSTestProjectsAsync ().Result;

		public Task<GeneratedProjects> GenerateAllMacTestProjectsAsync (Platform platform) => GenerateTestProjectsAsync (GetProjectDefinitions (macTestProjects, platform), platform);

		public GeneratedProjects GenerateAllMacTestProjects (Platform platform) => GenerateAllMacTestProjectsAsync (platform).Result;
	}
}