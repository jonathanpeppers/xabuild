using Microsoft.Build.CommandLine;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Xamarin.Android.Build
{
	class Program
	{
		static int Main (string[] args)
		{
			var commandLine = new StringBuilder (Environment.CommandLine);

			string currentDir = Path.GetDirectoryName (Assembly.GetEntryAssembly ().Location);
			string xaBuildOutput = Path.GetFullPath (Path.Combine (currentDir, "..", "..", "..", "..", "xamarin-android", "bin", "Debug"));
			if (!Directory.Exists (xaBuildOutput)) {
				Console.WriteLine ($"Unable to find Xamarin.Android build output at {xaBuildOutput}");
				return 1;
			}

			string xa_prefix = Path.Combine (xaBuildOutput, "lib", "xamarin.android");

			//NOTE: used in MSBuild.exe.config
			AddParameterIfOmitted (commandLine, "XamarinAndroidPath", Path.Combine (xa_prefix, "xbuild"));

			//Pulled from xabuild.sh
			AddParameterIfOmitted (commandLine, "MonoAndroidToolsDirectory", Path.Combine (xa_prefix, "xbuild", "Xamarin", "Android"));
			AddParameterIfOmitted (commandLine, "MonoDroidInstallDirectory", xa_prefix);
			AddParameterIfOmitted (commandLine, "TargetFrameworkRootPath", Path.Combine (xa_prefix, "xbuild-frameworks"));
			AddParameterIfOmitted (commandLine, "FrameworkPathOverride", Path.Combine (xa_prefix, "xbuild-frameworks", "MonoAndroid", "v1.0"));

			return MSBuildApp.Execute (commandLine.ToString ()) == MSBuildApp.ExitType.Success ? 0 : 1;
		}

		static void AddParameterIfOmitted (StringBuilder commandLine, string name, string value)
		{
			string prefix = "/p:" + name;
			if (!commandLine.ToString ().Contains (prefix)) {
				commandLine.Append ($" {prefix}=\"{value}\"");
			}
		}
	}
}
