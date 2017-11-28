using NUnit.Framework;
using System;
using System.Diagnostics;
using System.IO;

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

			currentDir = TestContext.CurrentContext.TestDirectory;
			samplesDir = Path.GetFullPath (Path.Combine (currentDir, "..", "..", "..", "samples"));
			xabuild = Path.GetFullPath (Path.Combine (currentDir, "..", "..", "..", "xabuild", "bin", configuration, "xabuild.exe"));

			Console.WriteLine($"xabuild.exe path {xabuild}");
		}

		/// <summary>
		/// Runs xabuild.exe and asserts on failure
		/// </summary>
		void XABuild(string project, string extraArgs = null)
		{
			Clean (project);

			var psi = new ProcessStartInfo () {
				CreateNoWindow = true,
				RedirectStandardOutput = true,
				UseShellExecute = false,
				WorkingDirectory = Path.GetDirectoryName (xabuild),
			};

			if (Environment.OSVersion.Platform == PlatformID.Win32NT) {
				psi.FileName = xabuild;
				psi.Arguments = $"{project} /bl {extraArgs}";
			} else {
				psi.FileName = "mono";
				psi.Arguments = $"{xabuild} {project} /bl {extraArgs}";
			}

			using (var p = Process.Start (psi)) {
				Console.Write (p.StandardOutput.ReadToEnd ());

				if (p.ExitCode != 0) {
					//In case of failure, output config file
					Console.WriteLine ("xabuild.exe.config:");
					Console.WriteLine (File.ReadAllText(xabuild + ".config"));

					Assert.Fail ("xabuild.exe failed!");
				}
			}
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

		[Test]
		public void HelloFSharp ()
		{
			XABuild (Path.Combine (samplesDir, "HelloFSharp", "HelloFSharp.fsproj"));
		}

		[Test]
		public void HelloNetStandard()
		{
			XABuild (Path.Combine (samplesDir, "HelloNetStandard", "HelloNetStandard.csproj"));
		}

		[Test]
		public void RestoreNetStandard ()
		{
			XABuild (Path.Combine (samplesDir, "HelloNetStandard", "HelloNetStandard.csproj"), "/t:Restore");
		}
	}
}
