Option Explicit

Dim objShell, BootstrapperLocation

Set objShell = CreateObject("WScript.Shell")

BootstrapperLocation = Mid(WScript.ScriptFullName, 1, InStrRev(WScript.ScriptFullName, "\")) & "\Disco.ClientBootstrapper.exe"

Call objShell.Run("""" & BootstrapperLocation & """ /Install", , True)

WScript.Echo "Disco ICT Client Bootstrapper Installed"