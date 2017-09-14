using Microsoft.Build.CommandLine;
using System;
using System.ComponentModel;
using System.IO;
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
		[MTAThread]
		static int Main ()
		{
			var paths = new XABuildPaths ();
			if (!Directory.Exists (paths.XamarinAndroidBuildOutput)) {
				Console.WriteLine ($"Unable to find Xamarin.Android build output at {paths.XamarinAndroidBuildOutput}");
				return 1;
			}

			//Create a custom xabuild.exe.config
			CreateConfig (paths);

			//Create link to .NETPortable directory
			if (!CreateSymbolicLink (Path.Combine (paths.FrameworksDirectory, ".NETPortable"), paths.PortableProfiles)) {
				return 1;
			}

			//Create link to Microsoft MSBuild targets directories
			foreach (var dir in new [] { "Microsoft", "15.0" }) {
				var target = Path.Combine (paths.MSBuildPath, dir);
				if (Directory.Exists (target)) {
					if (!CreateSymbolicLink (Path.Combine (paths.CustomMSBuildExtensionsPath, dir), target)) {
						return 1;
					}
				}
			}

			return MSBuildApp.Main ();
		}

		static void CreateConfig(XABuildPaths paths)
		{
			var xml = new XmlDocument ();
			xml.Load (Path.Combine (paths.MSBuildBin, "MSBuild.exe.config"));

			var toolsets = xml.SelectSingleNode ("configuration/msbuildToolsets/toolset");
			toolsets.SelectSingleNode ("property[@name='VsInstallRoot']/@value").Value      = paths.VsInstallRoot;
			toolsets.SelectSingleNode ("property[@name='MSBuildToolsPath']/@value").Value   = paths.MSBuildBin;
			toolsets.SelectSingleNode ("property[@name='MSBuildToolsPath32']/@value").Value = paths.MSBuildBin;
			toolsets.SelectSingleNode ("property[@name='MSBuildToolsPath64']/@value").Value = paths.MSBuildBin;
			toolsets.SelectSingleNode ("property[@name='RoslynTargetsPath']/@value").Value  = Path.Combine (paths.MSBuildBin, "Roslyn");

			SetProperty (toolsets, "AndroidSdkDirectory", paths.AndroidSdkDirectory);
			SetProperty (toolsets, "AndroidNdkDirectory", paths.AndroidNdkDirectory);
			SetProperty (toolsets, "MonoAndroidToolsDirectory", paths.MonoAndroidToolsDirectory);
			SetProperty (toolsets, "TargetFrameworkRootPath", paths.FrameworksDirectory + Path.DirectorySeparatorChar); //NOTE: Must include trailing \
			SetProperty (toolsets, "CSharpDesignTimeTargetsPath", paths.CSharpDesignTimeTargetsPath);

			var msbuildExtensionsPath = toolsets.SelectSingleNode ("projectImportSearchPaths/searchPaths/property[@name='MSBuildExtensionsPath']/@value");
			msbuildExtensionsPath.Value += ";" + paths.CustomMSBuildExtensionsPath;

			msbuildExtensionsPath = toolsets.SelectSingleNode ("projectImportSearchPaths/searchPaths/property[@name='MSBuildExtensionsPath32']/@value");
			msbuildExtensionsPath.Value += ";" + paths.CustomMSBuildExtensionsPath;

			msbuildExtensionsPath = toolsets.SelectSingleNode ("projectImportSearchPaths/searchPaths/property[@name='MSBuildExtensionsPath64']/@value");
			msbuildExtensionsPath.Value += ";" + paths.CustomMSBuildExtensionsPath;

			xml.Save (Path.Combine (paths.XABuildDirectory, "xabuild.exe.config"));
		}

		static void SetProperty(XmlNode toolsets, string name, string value)
		{
			var property = toolsets.OwnerDocument.CreateElement ("property");
			property.SetAttribute ("name", name	);
			property.SetAttribute ("value", value);
			toolsets.PrependChild (property);
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
