# xabuild
Prototype of xabuild.exe, don't look here!


## What it does

1. References `MSBuild.exe` and invokes its `Main()` method
1. Copies `MSBuild.exe.config` to `xabuild.exe.config`
1. Fixes up the XML to set custom MSBuild properties and search paths
1. Adds various symbolic links to make things work

## Example xabuild.exe.config

Existing values will remain unchanged besides the following:
```xml
<msbuildToolsets default="15.0">
  <toolset toolsVersion="15.0">
    <property name="CSharpDesignTimeTargetsPath" value="C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\MSBuild\Microsoft\VisualStudio\Managed\Microsoft.CSharp.DesignTime.targets" />
    <property name="TargetFrameworkRootPath" value="%USERPROFILE%\Desktop\Git\xamarin-android\bin\Debug\lib\xamarin.android\xbuild-frameworks\" />
    <property name="MonoAndroidToolsDirectory" value="%USERPROFILE%\Desktop\Git\xamarin-android\bin\Debug\lib\xamarin.android\xbuild\Xamarin\Android" />
    <property name="AndroidNdkDirectory" value="%USERPROFILE%\android-toolchain\ndk" />
    <property name="AndroidSdkDirectory" value="%USERPROFILE%\android-toolchain\sdk" />
    <property name="MSBuildToolsPath" value="C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin" />
    <property name="MSBuildToolsPath32" value="C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin" />
    <property name="MSBuildToolsPath64" value="C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin" />
    <property name="VsInstallRoot" value="C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise" />
    <property name="RoslynTargetsPath" value="C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin\Roslyn" />
    <projectImportSearchPaths>
    <searchPaths os="windows">
        <property name="MSBuildExtensionsPath" value="$(MSBuildProgramFiles32)\MSBuild;%USERPROFILE%\Desktop\Git\xamarin-android\bin\Debug\lib\xamarin.android\xbuild" />
        <property name="MSBuildExtensionsPath32" value="$(MSBuildProgramFiles32)\MSBuild;%USERPROFILE%\Desktop\Git\xamarin-android\bin\Debug\lib\xamarin.android\xbuild" />
        <property name="MSBuildExtensionsPath64" value="$(MSBuildProgramFiles32)\MSBuild;%USERPROFILE%\Desktop\Git\xamarin-android\bin\Debug\lib\xamarin.android\xbuild" />
    </searchPaths>
    </projectImportSearchPaths>
  </toolset>
</msbuildToolsets>
```
