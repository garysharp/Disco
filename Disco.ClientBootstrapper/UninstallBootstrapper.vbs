Option Explicit

On Error Resume Next

Dim objWMIService, objWMIProcesses, objFSO, objShell
Dim WaitForProcessID, DeleteDirectory, GroupPolicyScriptLocation

'WaitForProcessID = CInt(WScript.Arguments.Named.Item("WaitForProcessID"))
DeleteDirectory = Mid(WScript.ScriptFullName, 1, InStrRev(WScript.ScriptFullName, "\") - 1)

'If WaitForProcessID > 0 Then
'    Set objWMIService = GetObject("winmgmts:{impersonationLevel=impersonate}!\\.\root\cimv2")
'    Do
'        Set objWMIProcesses = objWMIService.ExecQuery("SELECT ProcessId FROM Win32_Process WHERE ProcessId=" & WaitForProcessID)
'        If objWMIProcesses.Count = 0 Then
'            Exit Do
'        End If
'        WScript.Sleep 500
'    Loop
'    Err.Clear
'End If
'Set objWMIService = Nothing
'Set objWMIProcesses = Nothing

Set objShell = CreateObject("WScript.Shell")
Set objFSO = CreateObject("Scripting.FileSystemObject")

Do
    Call Err.Clear()
    If objFSO.FolderExists(DeleteDirectory) Then
        objFSO.DeleteFolder DeleteDirectory, True
    End If
    WScript.Sleep 1000
Loop Until Err.Number = 0

GroupPolicyScriptLocation = objShell.ExpandEnvironmentStrings("%WinDir%\System32\GroupPolicy\Machine\Scripts\scripts.ini")
If objFSO.FileExists(GroupPolicyScriptLocation) Then
	Call objFSO.DeleteFile(GroupPolicyScriptLocation)
End If

Set objFSO = Nothing

objShell.RegDelete("HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon\HideStartupScripts")
objShell.RegDelete("HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon\RunStartupScriptSync")
objShell.RegDelete("HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Group Policy\Scripts\Shutdown\")
objShell.RegDelete("HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Group Policy\Scripts\Startup\0\0\")
objShell.RegDelete("HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Group Policy\Scripts\Startup\0\")
objShell.RegDelete("HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Group Policy\Scripts\Startup\")
objShell.RegDelete("HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Group Policy\State\Machine\Scripts\Shutdown\")
objShell.RegDelete("HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Group Policy\State\Machine\Scripts\Startup\0\0\")
objShell.RegDelete("HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Group Policy\State\Machine\Scripts\Startup\0\")
objShell.RegDelete("HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Group Policy\State\Machine\Scripts\Startup\")

Set objShell = Nothing