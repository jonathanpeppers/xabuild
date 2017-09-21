using System;
using System.IO;

namespace Xamarin.Android.Build
{
	/// <summary>
	/// Various paths needed by xabuild.exe
	/// </summary>
	class XABuildPaths
	{
		public bool IsWindows { get; private set; }

		/// <summary>
		/// Directory to xabuild.exe
		/// </summary>
		public string XABuildDirectory { get; private set; }

		/// <summary>
		/// Path to xabuild.exe.config, on Unix it seems to use MSBuild.dll.config instead
		/// </summary>
		public string XABuildConfig { get; private set; }

		/// <summary>
		/// The build output directory of Xamarin.Android, which is a submodule in this repo. Assumes it is already built.
		/// </summary>
		public string XamarinAndroidBuildOutput { get; private set; }

		/// <summary>
		/// $(VsInstallRoot), normally C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise
		/// </summary>
		public string VsInstallRoot { get; private set; }

		/// <summary>
		/// Path to MSBuild directory
		/// </summary>
		public string MSBuildPath { get; private set; }

		/// <summary>
		/// Path to directory of MSBuild.exe
		/// </summary>
		public string MSBuildBin { get; private set; }

		/// <summary>
		/// Path to MSBuild's App.config file
		/// </summary>
		public string MSBuildConfig { get; private set; }

		/// <summary>
		/// Path to the .NETPortable directory
		/// </summary>
		public string PortableProfiles { get; private set; }

		/// <summary>
		/// Our default $(MSBuildExtensionPath) which should be the "xbuild" directory in the Xamarin.Android build output
		/// </summary>
		public string MSBuildExtensionsPath { get; private set; }

		/// <summary>
		/// Array of search paths for MSBuildExtensionsPath
		/// </summary>
		public string [] ProjectImportSearchPaths { get; private set; }

		/// <summary>
		/// The xbuild-frameworks directory inside the Xamarin.Android build output
		/// </summary>
		public string FrameworksDirectory { get; private set; }

		public string MonoAndroidToolsDirectory { get; private set; }

		public string AndroidSdkDirectory { get; private set; }

		public string AndroidNdkDirectory { get; private set; }

		public XABuildPaths ()
		{
			IsWindows                 = Environment.OSVersion.Platform == PlatformID.Win32NT;
			XABuildDirectory          = Path.GetDirectoryName (GetType ().Assembly.Location);
			XamarinAndroidBuildOutput = Path.GetFullPath (Path.Combine (XABuildDirectory, "..", "..", "..", "xamarin-android", "bin", "Debug"));

			string programFiles       = Environment.GetFolderPath (Environment.SpecialFolder.ProgramFilesX86);
			string userProfile        = Environment.GetFolderPath (Environment.SpecialFolder.UserProfile);
			string prefix             = Path.Combine (XamarinAndroidBuildOutput, "lib", "xamarin.android");

			if (IsWindows) {
				foreach (var edition in new [] { "Enterprise", "Professional", "Community", "BuildTools" }) {
					var vsInstall = Path.Combine (programFiles, "Microsoft Visual Studio", "2017", edition);
					if (Directory.Exists (vsInstall)) {
						VsInstallRoot = vsInstall;
						break;
					}
				}
				if (VsInstallRoot == null)
					VsInstallRoot = programFiles;

				MSBuildPath              = Path.Combine (VsInstallRoot, "MSBuild");
				MSBuildBin               = Path.Combine (MSBuildPath, "15.0", "Bin");
				MSBuildConfig            = Path.Combine (MSBuildBin, "MSBuild.exe.config");
				ProjectImportSearchPaths = new [] { MSBuildPath, "$(MSBuildProgramFiles32)\\MSBuild" };
				PortableProfiles         = Path.Combine (programFiles, "Reference Assemblies", "Microsoft", "Framework", ".NETPortable");
				XABuildConfig            = Path.Combine (XABuildDirectory, "xabuild.exe.config");
			} else {
				//NOTE: This mono path is not correct for Linux
				string mono              = "/Library/Frameworks/Mono.framework/Versions/Current/lib/mono";
				MSBuildPath              = Path.Combine (mono, "msbuild");
				MSBuildBin               = Path.Combine (MSBuildPath, "15.0", "bin");
				MSBuildConfig            = Path.Combine (MSBuildBin, "MSBuild.dll.config");
				ProjectImportSearchPaths = new [] { MSBuildPath, Path.Combine (mono, "xbuild") };
				PortableProfiles         = Path.Combine (mono, "xbuild-frameworks", ".NETPortable");
				XABuildConfig            = Path.Combine (XABuildDirectory, "MSBuild.dll.config");
			}

			FrameworksDirectory       = Path.Combine (prefix, "xbuild-frameworks");
			MSBuildExtensionsPath     = Path.Combine (prefix, "xbuild");
			MonoAndroidToolsDirectory = Path.Combine (prefix, "xbuild", "Xamarin", "Android");
			AndroidSdkDirectory       = Path.Combine (userProfile, "android-toolchain", "sdk");
			AndroidNdkDirectory       = Path.Combine (userProfile, "android-toolchain", "ndk");
		}
	}
}
