using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using Mono.Unix;

namespace Xamarin.Android.Build
{
	static class SymbolicLink
	{
		public static bool Create (string source, string target)
		{
			if (!Directory.Exists (source)) {
				if (Environment.OSVersion.Platform == PlatformID.Win32NT) {
					//NOTE: attempt with and without the AllowUnprivilegedCreate flag, seems to fix Windows Server 2016
					if (!CreateSymbolicLink (source, target, SymbolLinkFlag.Directory | SymbolLinkFlag.AllowUnprivilegedCreate) &&
						!CreateSymbolicLink (source, target, SymbolLinkFlag.Directory)) {
						var error = new Win32Exception ().Message;
						Console.Error.WriteLine ($"Unable to create symbolic link from `{source}` to `{target}`: {error}");
						return false;
					}
				} else {
					var fileInfo = new UnixFileInfo (target);
					var link = fileInfo.CreateSymbolicLink (source);
					if (!link.Exists) {
						Console.Error.WriteLine ($"Unable to create symbolic link from `{source}` to `{target}`");
						return false;
					}
				}
			}

			return true;
		}

		enum SymbolLinkFlag {
			File = 0,
			Directory = 1,
			AllowUnprivilegedCreate = 2,
		}

		[DllImport ("kernel32.dll")]
		[return: MarshalAs (UnmanagedType.I1)]
		static extern bool CreateSymbolicLink (string lpSymlinkFileName, string lpTargetFileName, SymbolLinkFlag dwFlags);
	}
}
