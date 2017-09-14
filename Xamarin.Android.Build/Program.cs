using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;
using Microsoft.Build.Framework;
using Microsoft.Build.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Xml;

namespace Xamarin.Android.Build
{
	enum SymbolLinkFlag {
		File = 0,
		Directory = 1,
		AllowUnprivilegedCreate = 2,
	}

	class Program
	{
		const string CustomMSBuildExtensionsPath = "CustomMSBuildExtensionsPath";

		static int Main (string[] args)
		{
			string currentDir    = Path.GetDirectoryName (Assembly.GetEntryAssembly ().Location);
			string xaBuildOutput = Path.GetFullPath (Path.Combine (currentDir, "..", "..", "..", "..", "xamarin-android", "bin", "Debug"));

			if (!Directory.Exists (xaBuildOutput)) {
				Console.WriteLine ($"Unable to find Xamarin.Android build output at {xaBuildOutput}");
				return 1;
			}

			string prefix              = Path.Combine (xaBuildOutput, "lib", "xamarin.android");
			string frameworksDirectory = Path.Combine (prefix, "xbuild-frameworks");
			string userProfile         = Environment.GetFolderPath (Environment.SpecialFolder.UserProfile);
			string programFiles        = Environment.GetFolderPath (Environment.SpecialFolder.ProgramFilesX86);
			string vsInstallRoot       = Path.Combine (programFiles, "Microsoft Visual Studio", "2017", "Enterprise");
			string msbuildBin          = Path.Combine (vsInstallRoot, "MSBuild", "15.0", "Bin");

			//Create a custom xabuild.exe.config
			CreateConfig (currentDir, msbuildBin);

			//Create link to .NETPortable directory if needed
			string portableProfiles    = Path.Combine (programFiles, "Reference Assemblies", "Microsoft", "Framework", ".NETPortable");
			string customProfiles      = Path.Combine (frameworksDirectory, ".NETPortable");
			if (!Directory.Exists (customProfiles)) {
				if (!CreateSymbolicLink(customProfiles, portableProfiles, SymbolLinkFlag.Directory | SymbolLinkFlag.AllowUnprivilegedCreate)) {
					var error = new Win32Exception ().Message;
					Console.Error.WriteLine ($"Unable to create symbolic link from `{portableProfiles}` to `{customProfiles}`: {error}");
					return 1;
				}
			}

			var globalProperties = new Dictionary<string, string> ();

			//NOTE: used in xabuild.exe.config
			globalProperties [CustomMSBuildExtensionsPath] = Path.Combine (prefix, "xbuild");

			//Pulled from xabuild.sh
			globalProperties ["AndroidSdkDirectory"]         = Path.Combine (userProfile, "android-toolchain", "sdk");
			globalProperties ["AndroidNdkDirectory"]         = Path.Combine (userProfile, "android-toolchain", "ndk");
			globalProperties ["MonoAndroidToolsDirectory"]   = Path.Combine (prefix, "xbuild", "Xamarin", "Android");
			globalProperties ["MonoDroidInstallDirectory"]   = prefix;
			globalProperties ["TargetFrameworkRootPath"]     = frameworksDirectory;
			globalProperties ["VsInstallRoot"]               = vsInstallRoot;
			globalProperties ["RoslynTargetsPath"]           = Path.Combine (msbuildBin, "Roslyn");

			//For some reason this is defaulting to \, which places stuff in C:\Debug
			globalProperties ["BaseIntermediateOutputPath"] = "obj\\";

			//WTF??? Seems to fix PCLs when you remove FrameworkPathOverride
			//globalProperties["NoStdLib"] = "True";

			//This was originally used on Windows, but seems to break w/ PCLs (tested Xamarin.Forms app)
			globalProperties ["FrameworkPathOverride"] = Path.Combine (prefix, "xbuild-frameworks", "MonoAndroid", "v1.0");

			var toolsetLocations = ToolsetDefinitionLocations.Default;
			var verbosity        = LoggerVerbosity.Diagnostic;

			var binaryLogger = new BinaryLogger {
				Parameters = "msbuild.binlog",
				Verbosity = verbosity
			};
			var consoleLogger = new ConsoleLogger {
				Verbosity = verbosity,
			};
			using (var projectCollection = new ProjectCollection (globalProperties, new ILogger[] { binaryLogger, consoleLogger }, toolsetLocations)) {

				var request = new BuildRequestData (args[0], globalProperties, projectCollection.DefaultToolsVersion, new[] { "Build" }, null);
				var parameters = new BuildParameters (projectCollection) {
					Loggers = projectCollection.Loggers,
					ToolsetDefinitionLocations = toolsetLocations,
					DefaultToolsVersion = projectCollection.DefaultToolsVersion,
					DetailedSummary = true,
				};
				var result = BuildManager.DefaultBuildManager.Build (parameters, request);
				return result.OverallResult == BuildResultCode.Success ? 0 : 1;
			}
		}

		static void CreateConfig(string currentDir, string msbuildBin)
		{
			XmlDocument xml = new XmlDocument ();
			xml.Load (Path.Combine (msbuildBin, "MSBuild.exe.config"));

			var searchPaths = xml.SelectSingleNode ("configuration/msbuildToolsets/toolset/projectImportSearchPaths/searchPaths/property[@name='MSBuildExtensionsPath']/@value");
			searchPaths.Value += $";$({CustomMSBuildExtensionsPath})";

			xml.Save (Path.Combine (currentDir, "xabuild.exe.config"));
		}


		}

		[DllImport ("kernel32.dll")]
		[return: MarshalAs (UnmanagedType.I1)]
		static extern bool CreateSymbolicLink (string lpSymlinkFileName, string lpTargetFileName, SymbolLinkFlag dwFlags);
	}
}
