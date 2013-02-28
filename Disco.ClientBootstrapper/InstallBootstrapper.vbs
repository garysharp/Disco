Option Explicit

Dim objFSO, objReg, objShell, objFile
Dim SourceFolder, DestinationFolder, GroupPolicyScriptLocation
Const HKLM = &H80000002

DestinationFolder = "C:\Disco"

Set objFSO = CreateObject("Scripting.FileSystemObject")
Set objReg = GetObject("winmgmts:root\default:StdRegProv")
Set objShell = CreateObject("WScript.Shell")

If objFSO.FolderExists(DestinationFolder) Then
	Call objFSO.DeleteFolder(DestinationFolder)
End If
Call objFSO.CreateFolder(DestinationFolder)
SourceFolder = Mid(WScript.ScriptFullName, 1, InStrRev(WScript.ScriptFullName, "\"))
Call objFSO.CopyFile(SourceFolder & "*.*", DestinationFolder, True)

GroupPolicyScriptLocation = objShell.ExpandEnvironmentStrings("%WinDir%\System32\GroupPolicy\Machine\Scripts\scripts.ini")
If objFSO.FileExists(GroupPolicyScriptLocation) Then
	Call objFSO.DeleteFile(GroupPolicyScriptLocation)
End If
Set objFile = objFSO.CreateTextFile(GroupPolicyScriptLocation, True, True)
Call objFile.WriteLine()
Call objFile.WriteLine("[Startup]")
Call objFile.WriteLine("0CmdLine=C:\Disco\Disco.ClientBootstrapper.exe")
Call objFile.WriteLine("0Parameters=/Uninstall")
Call objFile.Close()
Set objFile = Nothing

Set objFSO = Nothing

Call objReg.SetDWORDValue(HKLM, "SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon", "HideStartupScripts", 0)
Call objReg.SetDWORDValue (HKLM, "SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon", "RunStartupScriptSync", 1)

Call objReg.CreateKey(HKLM, "SOFTWARE\Microsoft\Windows\CurrentVersion\Group Policy\Scripts\Shutdown")
Call objReg.CreateKey(HKLM, "SOFTWARE\Microsoft\Windows\CurrentVersion\Group Policy\Scripts\Startup\0\0")
Call objReg.SetStringValue(HKLM, "SOFTWARE\Microsoft\Windows\CurrentVersion\Group Policy\Scripts\Startup\0", "GPO-ID", "LocalGPO")
Call objReg.SetStringValue(HKLM, "SOFTWARE\Microsoft\Windows\CurrentVersion\Group Policy\Scripts\Startup\0", "SOM-ID", "Local")
Call objReg.SetStringValue(HKLM, "SOFTWARE\Microsoft\Windows\CurrentVersion\Group Policy\Scripts\Startup\0", "FileSysPath", "C:\WINDOWS\System32\GroupPolicy\Machine")
Call objReg.SetStringValue(HKLM, "SOFTWARE\Microsoft\Windows\CurrentVersion\Group Policy\Scripts\Startup\0", "DisplayName", "Local Group Policy")
Call objReg.SetStringValue(HKLM, "SOFTWARE\Microsoft\Windows\CurrentVersion\Group Policy\Scripts\Startup\0", "GPOName", "Local Group Policy")
Call objReg.SetDWORDValue(HKLM, "SOFTWARE\Microsoft\Windows\CurrentVersion\Group Policy\Scripts\Startup\0", "PSScriptOrder", 1)
Call objReg.SetStringValue(HKLM, "SOFTWARE\Microsoft\Windows\CurrentVersion\Group Policy\Scripts\Startup\0\0", "Script", DestinationFolder & "\Disco.ClientBootstrapper.exe")
Call objReg.SetStringValue(HKLM, "SOFTWARE\Microsoft\Windows\CurrentVersion\Group Policy\Scripts\Startup\0\0", "Parameters", "/Uninstall")
Call objReg.SetDWORDValue(HKLM, "SOFTWARE\Microsoft\Windows\CurrentVersion\Group Policy\Scripts\Startup\0\0", "IsPowershell", 0)
Call objReg.SetBinaryValue(HKLM, "SOFTWARE\Microsoft\Windows\CurrentVersion\Group Policy\Scripts\Startup\0\0", "ExecTime", array(0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0))

Call objReg.CreateKey(HKLM, "SOFTWARE\Microsoft\Windows\CurrentVersion\Group Policy\State\Machine\Scripts\Shutdown")
Call objReg.CreateKey(HKLM, "SOFTWARE\Microsoft\Windows\CurrentVersion\Group Policy\State\Machine\Scripts\Startup\0\0")
Call objReg.SetStringValue(HKLM, "SOFTWARE\Microsoft\Windows\CurrentVersion\Group Policy\State\Machine\Scripts\Startup\0", "GPO-ID", "LocalGPO")
Call objReg.SetStringValue(HKLM, "SOFTWARE\Microsoft\Windows\CurrentVersion\Group Policy\State\Machine\Scripts\Startup\0", "SOM-ID", "Local")
Call objReg.SetStringValue(HKLM, "SOFTWARE\Microsoft\Windows\CurrentVersion\Group Policy\State\Machine\Scripts\Startup\0", "FileSysPath", "C:\Windows\System32\GroupPolicy\Machine")
Call objReg.SetStringValue(HKLM, "SOFTWARE\Microsoft\Windows\CurrentVersion\Group Policy\State\Machine\Scripts\Startup\0", "DisplayName", "Local Group Policy")
Call objReg.SetStringValue(HKLM, "SOFTWARE\Microsoft\Windows\CurrentVersion\Group Policy\State\Machine\Scripts\Startup\0", "GPOName", "Local Group Policy")
Call objReg.SetDWORDValue(HKLM, "SOFTWARE\Microsoft\Windows\CurrentVersion\Group Policy\State\Machine\Scripts\Startup\0", "PSScriptOrder", 1)
Call objReg.SetStringValue(HKLM, "SOFTWARE\Microsoft\Windows\CurrentVersion\Group Policy\State\Machine\Scripts\Startup\0\0", "Script", DestinationFolder & "\Disco.ClientBootstrapper.exe")
Call objReg.SetStringValue(HKLM, "SOFTWARE\Microsoft\Windows\CurrentVersion\Group Policy\State\Machine\Scripts\Startup\0\0", "Parameters", "/Uninstall")
Call objReg.SetBinaryValue(HKLM, "SOFTWARE\Microsoft\Windows\CurrentVersion\Group Policy\State\Machine\Scripts\Startup\0\0", "ExecTime", array(0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0))

Set objReg = Nothing