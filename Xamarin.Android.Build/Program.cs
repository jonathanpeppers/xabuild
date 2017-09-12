using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;
using Microsoft.Build.Framework;
using Microsoft.Build.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Linq;

namespace Xamarin.Android.Build
{
	class Program
	{
		static int Main (string[] args)
		{
			string currentDir = Path.GetDirectoryName (Assembly.GetEntryAssembly ().Location);
			string xaBuildOutput = Path.GetFullPath (Path.Combine (currentDir, "..", "..", "..", "..", "xamarin-android", "bin", "Debug"));

			if (!Directory.Exists (xaBuildOutput)) {
				Console.WriteLine ($"Unable to find Xamarin.Android build output at {xaBuildOutput}");
				return 1;
			}

			string prefix = Path.Combine (xaBuildOutput, "lib", "xamarin.android");
			string frameworksDirectory = Path.Combine (prefix, "xbuild-frameworks");

			//Copy .NETPortable directory if needed
			string portableProfiles = Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.ProgramFilesX86),
				"Reference Assemblies", "Microsoft", "Framework", ".NETPortable");
			string copiedProfiles = Path.Combine (frameworksDirectory, ".NETPortable");
			if (!Directory.Exists (copiedProfiles)) {
				Copy (new DirectoryInfo (portableProfiles), new DirectoryInfo (copiedProfiles));
			}

			var globalProperties = new Dictionary<string, string> ();

			//NOTE: used in MSBuild.exe.config
			globalProperties["XamarinAndroidPath"] = Path.Combine (prefix, "xbuild");

			//Pulled from xabuild.sh
			globalProperties["AndroidSdkDirectory"] = Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.UserProfile), "android-toolchain", "sdk");
			globalProperties["AndroidNdkDirectory"] = Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.UserProfile), "android-toolchain", "ndk");
			globalProperties["MonoAndroidToolsDirectory"] = Path.Combine (prefix, "xbuild", "Xamarin", "Android");
			globalProperties["MonoDroidInstallDirectory"] = prefix;
			globalProperties["TargetFrameworkRootPath"] = frameworksDirectory;

			//WTF??? Seems to fix PCLs when you remove FrameworkPathOverride
			globalProperties["NoStdLib"] = "True";

			//This was originally used on Windows, but seems to break w/ PCLs (tested Xamarin.Forms app)
			//globalProperties["FrameworkPathOverride"] = Path.Combine (prefix, "xbuild-frameworks", "MonoAndroid", "v1.0");

			var logger = new BinaryLogger {
				Parameters = "msbuild.binlog",
				Verbosity = LoggerVerbosity.Diagnostic
			};
			var toolsetLocations = ToolsetDefinitionLocations.Default;
			using (var projectCollection = new ProjectCollection (globalProperties, new[] { logger }, toolsetLocations)) {
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

		static void Copy (DirectoryInfo source, DirectoryInfo target)
		{
			Directory.CreateDirectory (target.FullName);

			foreach (var file in source.GetFiles ()) {
				file.CopyTo (Path.Combine (target.FullName, file.Name), true);
			}
			
			foreach (var directory in source.GetDirectories ()) {
				var subDirectory = target.CreateSubdirectory (directory.Name);
				Copy (directory, subDirectory);
			}
		}
	}
}
