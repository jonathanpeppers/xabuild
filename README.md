# xabuild
Prototype of xabuild.exe, don't look here!


## What it does

1. References `MSBuild.exe` and invokes its `Main()` method
1. Copies `MSBuild.exe.config` to `xabuild.exe.config`
1. Fixes up `xabuild.exe.config` to set custom MSBuild properties and search paths
1. Add a symbolic link to the `.NETPortable` framework assemblies directory

## Example xabuild.exe.config

The following values are modified, based on the `xamarin-android` repo being checked out adjacent to `xabuild`'s repo:
```xml
<msbuildToolsets default="15.0">
  <toolset toolsVersion="15.0">
    <property name="TargetFrameworkRootPath" value="$(UserProfile)\Desktop\Git\xamarin-android\bin\Debug\lib\xamarin.android\xbuild-frameworks\" />
    <property name="MonoAndroidToolsDirectory" value="$(UserProfile)\Desktop\Git\xamarin-android\bin\Debug\lib\xamarin.android\xbuild\Xamarin\Android" />
    <property name="AndroidNdkDirectory" value="$(UserProfile)\android-toolchain\ndk" />
    <property name="AndroidSdkDirectory" value="$(UserProfile)\android-toolchain\sdk" />
    <property name="MSBuildToolsPath" value="C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin" />
    <property name="MSBuildToolsPath32" value="C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin" />
    <property name="MSBuildToolsPath64" value="C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin" />
    <property name="VsInstallRoot" value="C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise" />
    <property name="MSBuildExtensionsPath" value="$(UserProfile)\Desktop\Git\xamarin-android\bin\Debug\lib\xamarin.android\xbuild" />
    <property name="MSBuildExtensionsPath32" value="$(UserProfile)\Desktop\Git\xamarin-android\bin\Debug\lib\xamarin.android\xbuild" />
    <property name="RoslynTargetsPath" value="C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin\Roslyn" />
    <projectImportSearchPaths>
      <searchPaths os="windows">
        <property name="MSBuildExtensionsPath" value="C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\MSBuild;$(MSBuildProgramFiles32)\MSBuild" />
        <property name="MSBuildExtensionsPath32" value="C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\MSBuild;$(MSBuildProgramFiles32)\MSBuild" />
        <property name="MSBuildExtensionsPath64" value="C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\MSBuild;$(MSBuildProgramFiles32)\MSBuild" />
      </searchPaths>
    </projectImportSearchPaths>
  </toolset>
</msbuildToolsets>
```

## macOS

A couple notes:
- You have to build `xabuild.sln` with MSBuild
- `MSBuild.exe` is actually `MSBuild.dll` on macOS
- Therefore, to get the config to load, we must customize `MSBuild.dll.config`

An example `MSBuild.dll.config`:
```xml
<msbuildToolsets default="15.0">
  <toolset toolsVersion="15.0">
    <property name="TargetFrameworkRootPath" value="~/Desktop/Git/xabuild/xamarin-android/bin/Debug/lib/xamarin.android/xbuild-frameworks/" />
    <property name="MonoAndroidToolsDirectory" value="~/Desktop/Git/xabuild/xamarin-android/bin/Debug/lib/xamarin.android/xbuild/Xamarin/Android" />
    <property name="AndroidNdkDirectory" value="~/android-toolchain/ndk" />
    <property name="AndroidSdkDirectory" value="~/android-toolchain/sdk" />
    <property name="MSBuildExtensionsPath32" value="~/Desktop/Git/xabuild/xamarin-android/bin/Debug/lib/xamarin.android/xbuild" />
    <property name="MSBuildExtensionsPath" value="~/Desktop/Git/xabuild/xamarin-android/bin/Debug/lib/xamarin.android/xbuild" />
    <property name="MSBuildToolsPath" value="/Library/Frameworks/Mono.framework/Versions/Current/lib/mono/msbuild/15.0/bin" />
    <property name="MSBuildToolsPath32" value="/Library/Frameworks/Mono.framework/Versions/Current/lib/mono/msbuild/15.0/bin" />
    <property name="MSBuildToolsPath64" value="/Library/Frameworks/Mono.framework/Versions/Current/lib/mono/msbuild/15.0/bin" />
    <property name="RoslynTargetsPath" value="/Library/Frameworks/Mono.framework/Versions/Current/lib/mono/msbuild/15.0/bin/Roslyn" />
    <projectImportSearchPaths>
      <searchPaths os="osx">
        <property name="MSBuildExtensionsPath" value="/Library/Frameworks/Mono.framework/Versions/Current/lib/mono/msbuild;/Library/Frameworks/Mono.framework/Versions/Current/lib/mono/xbuild" />
        <property name="MSBuildExtensionsPath32" value="/Library/Frameworks/Mono.framework/Versions/Current/lib/mono/msbuild;/Library/Frameworks/Mono.framework/Versions/Current/lib/mono/xbuild" />
        <property name="MSBuildExtensionsPath64" value="/Library/Frameworks/Mono.framework/Versions/Current/lib/mono/msbuild;/Library/Frameworks/Mono.framework/Versions/Current/lib/mono/xbuild" />
      </searchPaths>
    </projectImportSearchPaths>
  </toolset>
</msbuildToolsets>
```
