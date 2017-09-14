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
        <property name="MSBuildExtensionsPath" value="$(MSBuildProgramFiles32)\MSBuild;C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\MSBuild" />
        <property name="MSBuildExtensionsPath32" value="$(MSBuildProgramFiles32)\MSBuild;C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\MSBuild" />
        <property name="MSBuildExtensionsPath64" value="$(MSBuildProgramFiles32)\MSBuild;C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\MSBuild" />
      </searchPaths>
    </projectImportSearchPaths>
  </toolset>
</msbuildToolsets>
```
