using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xamarin.Android.Build.Tests
{
	[TestFixture]
	public class XABuildTests
	{
		string currentDir;
		string samplesDir;
		string xabuild;

		[SetUp]
		public void SetUp()
		{
#if DEBUG
			string configuration = "Debug";
#else
			string configuration = "Release";
#endif

			currentDir = Path.GetDirectoryName (GetType ().Assembly.Location);
			samplesDir = Path.GetFullPath (Path.Combine (currentDir, "..", "..", "..", "samples"));
			xabuild = Path.GetFullPath (Path.Combine (currentDir, "..", "..", "..", "Xamarin.Android.Build", "bin", configuration, "xabuild.exe"));
		}

		/// <summary>
		/// Runs xabuild.exe and asserts on failure
		/// </summary>
		void XABuild(string project)
		{
			Clean (project);

			var p = Process.Start (new ProcessStartInfo (xabuild, $"{project} /bl") {
				CreateNoWindow = true,
				UseShellExecute = false,
				WorkingDirectory = Path.GetDirectoryName (xabuild),
			});
			p.WaitForExit ();

			if (p.ExitCode != 0)
				Assert.Fail ("xabuild.exe failed!");
		}

		void Clean(string project)
		{
			string dir = Path.GetDirectoryName (project);
			string bin = Path.Combine (dir, "bin");
			string obj = Path.Combine (dir, "obj");

			if (Directory.Exists (bin))
				Directory.Delete (bin, true);
			if (Directory.Exists (obj))
				Directory.Delete (obj, true);
		}

		[Test]
		public void HelloWorld()
		{
			XABuild (Path.Combine (samplesDir, "HelloWorld", "HelloWorld.csproj"));
		}

		[Test]
		public void HelloForms ()
		{
			XABuild (Path.Combine (samplesDir, "HelloForms", "HelloForms.Android", "HelloForms.Android.csproj"));
		}
	}
}
