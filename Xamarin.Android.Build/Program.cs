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
		static int Main (string[] args)
		{
			string currentDir    = Path.GetDirectoryName (Assembly.GetEntryAssembly ().Location);
			string xaBuildOutput = Path.GetFullPath (Path.Combine (currentDir, "..", "..", "..", "..", "xamarin-android", "bin", "Debug"));

			if (!Directory.Exists (xaBuildOutput)) {
				Console.WriteLine ($"Unable to find Xamarin.Android build output at {xaBuildOutput}");
				return 1;
			}

			string prefix               = Path.Combine (xaBuildOutput, "lib", "xamarin.android");
			string frameworksDirectory  = Path.Combine (prefix, "xbuild-frameworks");
			string userProfile          = Environment.GetFolderPath (Environment.SpecialFolder.UserProfile);
			string programFiles         = Environment.GetFolderPath (Environment.SpecialFolder.ProgramFilesX86);
			string vsInstallRoot        = Path.Combine (programFiles, "Microsoft Visual Studio", "2017", "Enterprise");
			string msbuildPath          = Path.Combine (vsInstallRoot, "MSBuild");
			string msbuildBin           = Path.Combine (msbuildPath, "15.0", "Bin");
			string customExtensionsPath = Path.Combine (prefix, "xbuild");

			//Create a custom xabuild.exe.config
			CreateConfig (currentDir, vsInstallRoot, msbuildBin, customExtensionsPath);

			//Create link to .NETPortable directory
			string portableProfiles = Path.Combine (programFiles, "Reference Assemblies", "Microsoft", "Framework", ".NETPortable");
			string customProfiles = Path.Combine (frameworksDirectory, ".NETPortable");
			if (!CreateSymbolicLink (customProfiles, portableProfiles)) {
				return 1;
			}

			//Create link to Microsoft MSBuild targets
			foreach (var dir in new [] { "Microsoft", "15.0" }) {
				var target = Path.Combine (msbuildPath, dir);
				if (Directory.Exists (target)) {
					if (!CreateSymbolicLink (Path.Combine (customExtensionsPath, dir), target)) {
						return 1;
					}
				}
			}

			var globalProperties = new Dictionary<string, string> ();

			//Pulled from xabuild.sh
			globalProperties ["AndroidSdkDirectory"]          = Path.Combine (userProfile, "android-toolchain", "sdk");
			globalProperties ["AndroidNdkDirectory"]          = Path.Combine (userProfile, "android-toolchain", "ndk");
			globalProperties ["MonoAndroidToolsDirectory"]    = Path.Combine (prefix, "xbuild", "Xamarin", "Android");
			globalProperties ["MonoDroidInstallDirectory"]    = prefix;
			globalProperties ["TargetFrameworkRootPath"]      = frameworksDirectory + Path.DirectorySeparatorChar; //NOTE: Must include trailing \
			globalProperties ["CSharpDesignTimeTargetsPath"]  = Path.Combine (msbuildPath, "Microsoft", "VisualStudio", "Managed", "Microsoft.CSharp.DesignTime.targets");

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

		static void CreateConfig(string currentDir, string vsInstallRoot, string msbuildBin, string customExtensionsPath)
		{
			XmlDocument xml = new XmlDocument ();
			xml.Load (Path.Combine (msbuildBin, "MSBuild.exe.config"));

			var toolsets = xml.SelectSingleNode ("configuration/msbuildToolsets/toolset");
			toolsets.SelectSingleNode ("property[@name='VsInstallRoot']/@value").Value      = vsInstallRoot;
			toolsets.SelectSingleNode ("property[@name='MSBuildToolsPath']/@value").Value   = msbuildBin;
			toolsets.SelectSingleNode ("property[@name='MSBuildToolsPath32']/@value").Value = msbuildBin;
			toolsets.SelectSingleNode ("property[@name='MSBuildToolsPath64']/@value").Value = msbuildBin;
			toolsets.SelectSingleNode ("property[@name='RoslynTargetsPath']/@value").Value  = Path.Combine (msbuildBin, "Roslyn");

			var msbuildExtensionsPath = toolsets.SelectSingleNode ("projectImportSearchPaths/searchPaths/property[@name='MSBuildExtensionsPath']/@value");
			msbuildExtensionsPath.Value += ";" + customExtensionsPath;

			msbuildExtensionsPath = toolsets.SelectSingleNode ("projectImportSearchPaths/searchPaths/property[@name='MSBuildExtensionsPath32']/@value");
			msbuildExtensionsPath.Value += ";" + customExtensionsPath;

			msbuildExtensionsPath = toolsets.SelectSingleNode ("projectImportSearchPaths/searchPaths/property[@name='MSBuildExtensionsPath64']/@value");
			msbuildExtensionsPath.Value += ";" + customExtensionsPath;

			xml.Save (Path.Combine (currentDir, "xabuild.exe.config"));
		}

		static bool CreateSymbolicLink(string source, string target)
		{
			if (!Directory.Exists (source)) {
				if (!CreateSymbolicLink (source, target, SymbolLinkFlag.Directory | SymbolLinkFlag.AllowUnprivilegedCreate)) {
					var error = new Win32Exception ().Message;
					Console.Error.WriteLine ($"Unable to create symbolic link from `{source}` to `{target}`: {error}");
					return false;
				}
			}

			return true;
		}

		[DllImport ("kernel32.dll")]
		[return: MarshalAs (UnmanagedType.I1)]
		static extern bool CreateSymbolicLink (string lpSymlinkFileName, string lpTargetFileName, SymbolLinkFlag dwFlags);
	}
}
