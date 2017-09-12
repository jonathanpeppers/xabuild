using Microsoft.Build.CommandLine;
using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace Xamarin.Android.Build
{
	class Program
	{
		static int Main (string[] args)
		{
			var commandLine      = new StringBuilder (Environment.CommandLine);
			string currentDir    = Path.GetDirectoryName (Assembly.GetEntryAssembly ().Location);
			string xaBuildOutput = Path.GetFullPath (Path.Combine (currentDir, "..", "..", "..", "..", "xamarin-android", "bin", "Debug"));

			if (!Directory.Exists (xaBuildOutput)) {
				Console.WriteLine ($"Unable to find Xamarin.Android build output at {xaBuildOutput}");
				return 1;
			}

			string prefix              = Path.Combine (xaBuildOutput, "lib", "xamarin.android");
			string frameworksDirectory = Path.Combine (prefix, "xbuild-frameworks");

			//Copy .NETPortable directory if needed
			string portableProfiles = Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.ProgramFilesX86),
				"Reference Assemblies", "Microsoft", "Framework", ".NETPortable");
			string copiedProfiles = Path.Combine (frameworksDirectory, ".NETPortable");
			if (!Directory.Exists (copiedProfiles)) {
				Copy (new DirectoryInfo (portableProfiles), new DirectoryInfo (copiedProfiles));
			}

			//NOTE: used in MSBuild.exe.config
			AddParameterIfOmitted (commandLine, "XamarinAndroidPath", Path.Combine (prefix, "xbuild"));

			//Pulled from xabuild.sh
			AddParameterIfOmitted (commandLine, "AndroidSdkDirectory", Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.UserProfile), "android-toolchain", "sdk"));
			AddParameterIfOmitted (commandLine, "AndroidNdkDirectory", Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.UserProfile), "android-toolchain", "ndk"));
			AddParameterIfOmitted (commandLine, "MonoAndroidToolsDirectory", Path.Combine (prefix, "xbuild", "Xamarin", "Android"));
			AddParameterIfOmitted (commandLine, "MonoDroidInstallDirectory", prefix);
			AddParameterIfOmitted (commandLine, "TargetFrameworkRootPath", frameworksDirectory);

			//WTF??? Seems to fix PCLs when you remove FrameworkPathOverride
			AddParameterIfOmitted (commandLine, "NoStdLib", "True");

			//This was originally used on Windows, but seems to break w/ PCLs (tested Xamarin.Forms app)
			//AddParameterIfOmitted (commandLine, "FrameworkPathOverride", Path.Combine (prefix, "xbuild-frameworks", "MonoAndroid", "v1.0"));

			return MSBuildApp.Execute (commandLine.ToString ()) == MSBuildApp.ExitType.Success ? 0 : 1;
		}

		static void AddParameterIfOmitted (StringBuilder commandLine, string name, string value)
		{
			string prefix = "/p:" + name;
			if (!commandLine.ToString ().Contains (prefix)) {
				commandLine.Append ($" {prefix}=\"{value}\"");
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
