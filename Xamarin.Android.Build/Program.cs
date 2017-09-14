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

			return MSBuildApp.Main ();
		}

		static void CreateConfig(XABuildPaths paths)
		{
			var xml = new XmlDocument ();
			xml.Load (Path.Combine (paths.MSBuildBin, "MSBuild.exe.config"));

			var toolsets = xml.SelectSingleNode ("configuration/msbuildToolsets/toolset");
			SetProperty (toolsets, "VsInstallRoot", paths.VsInstallRoot);
			SetProperty (toolsets, "MSBuildToolsPath", paths.MSBuildBin);
			SetProperty (toolsets, "MSBuildToolsPath32", paths.MSBuildBin);
			SetProperty (toolsets, "MSBuildToolsPath64", paths.MSBuildBin);
			SetProperty (toolsets, "MSBuildExtensionsPath", paths.MSBuildExtensionsPath);
			SetProperty (toolsets, "MSBuildExtensionsPath32", paths.MSBuildExtensionsPath);
			SetProperty (toolsets, "RoslynTargetsPath", Path.Combine (paths.MSBuildBin, "Roslyn"));
			SetProperty (toolsets, "AndroidSdkDirectory", paths.AndroidSdkDirectory);
			SetProperty (toolsets, "AndroidNdkDirectory", paths.AndroidNdkDirectory);
			SetProperty (toolsets, "MonoAndroidToolsDirectory", paths.MonoAndroidToolsDirectory);
			SetProperty (toolsets, "TargetFrameworkRootPath", paths.FrameworksDirectory + Path.DirectorySeparatorChar); //NOTE: Must include trailing \

			foreach (XmlNode property in toolsets.SelectNodes("projectImportSearchPaths/searchPaths/property[starts-with(@name, 'MSBuildExtensionsPath')]/@value")) {
				property.Value += ";" + paths.MSBuildPath;
			}

			xml.Save (Path.Combine (paths.XABuildDirectory, "xabuild.exe.config"));
		}

		/// <summary>
		/// If the value exists, sets value attribute, else creates the element
		/// </summary>
		static void SetProperty(XmlNode toolsets, string name, string value)
		{
			var valueAttribute = toolsets.SelectSingleNode ($"property[@name='{name}']/@value");
			if (valueAttribute != null) {
				valueAttribute.Value = value;
			} else {
				var property = toolsets.OwnerDocument.CreateElement ("property");
				property.SetAttribute ("name", name);
				property.SetAttribute ("value", value);
				toolsets.PrependChild (property);
			}
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
