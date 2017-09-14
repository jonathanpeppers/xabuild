using System;
using System.IO;

namespace Xamarin.Android.Build
{
	/// <summary>
	/// Various paths needed by xabuild.exe
	/// </summary>
	class XABuildPaths
	{
		/// <summary>
		/// Directory to xabuild.exe
		/// </summary>
		public string XABuildDirectory { get; private set; }

		/// <summary>
		/// The build output directory of Xamarin.Android such as ~/Git/xamarin-android/bin/Debug
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
		/// Path to the .NETPortable directory
		/// </summary>
		public string PortableProfiles { get; private set; }

		/// <summary>
		/// Our default $(MSBuildExtensionPath) which should be the "xbuild" directory in the Xamarin.Android build output
		/// </summary>
		public string MSBuildExtensionsPath { get; private set; }

		/// <summary>
		/// The xbuild-frameworks directory inside the Xamarin.Android build output
		/// </summary>
		public string FrameworksDirectory { get; private set; }

		public string MonoAndroidToolsDirectory { get; private set; }

		public string AndroidSdkDirectory { get; private set; }

		public string AndroidNdkDirectory { get; private set; }

		public XABuildPaths ()
		{
			XABuildDirectory          = Path.GetDirectoryName (GetType ().Assembly.Location);
			XamarinAndroidBuildOutput = Path.GetFullPath (Path.Combine (XABuildDirectory, "..", "..", "..", "..", "xamarin-android", "bin", "Debug"));

			string programFiles       = Environment.GetFolderPath (Environment.SpecialFolder.ProgramFilesX86);
			string userProfile        = Environment.GetFolderPath (Environment.SpecialFolder.UserProfile);
			string prefix             = Path.Combine (XamarinAndroidBuildOutput, "lib", "xamarin.android");

			FrameworksDirectory       = Path.Combine (prefix, "xbuild-frameworks");
			VsInstallRoot             = Path.Combine (programFiles, "Microsoft Visual Studio", "2017", "Enterprise");
			PortableProfiles          = Path.Combine (programFiles, "Reference Assemblies", "Microsoft", "Framework", ".NETPortable");
			MSBuildPath               = Path.Combine (VsInstallRoot, "MSBuild");
			MSBuildBin                = Path.Combine (MSBuildPath, "15.0", "Bin");
			MSBuildExtensionsPath     = Path.Combine (prefix, "xbuild");
			MonoAndroidToolsDirectory = Path.Combine (prefix, "xbuild", "Xamarin", "Android");
			AndroidSdkDirectory       = Path.Combine (userProfile, "android-toolchain", "sdk");
			AndroidNdkDirectory       = Path.Combine (userProfile, "android-toolchain", "ndk");
		}
	}
}
