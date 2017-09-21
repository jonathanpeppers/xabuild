using Microsoft.Build.CommandLine;
using System;
using System.IO;
using System.Xml;

namespace Xamarin.Android.Build
{
	class XABuild
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
			if (!SymbolicLink.Create (Path.Combine (paths.FrameworksDirectory, ".NETPortable"), paths.PortableProfiles)) {
				return 1;
			}

			return MSBuildApp.Main ();
		}

		static void CreateConfig (XABuildPaths paths)
		{
			var xml = new XmlDocument ();
			xml.Load (paths.MSBuildConfig);

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

			foreach (XmlNode property in toolsets.SelectNodes ("projectImportSearchPaths/searchPaths/property[starts-with(@name, 'MSBuildExtensionsPath')]/@value")) {
				property.Value = string.Join (";", paths.ProjectImportSearchPaths);
			}

			xml.Save (paths.XABuildConfig);
		}

		/// <summary>
		/// If the value exists, sets value attribute, else creates the element
		/// </summary>
		static void SetProperty (XmlNode toolsets, string name, string value)
		{
			if (string.IsNullOrEmpty (value))
				return;

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
	}
}
