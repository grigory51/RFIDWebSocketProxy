!include 'LogicLib.nsh'

Name "RFIDWSProxy"
OutFile "RFIDWSProxy.exe"
InstallDir "$PROGRAMFILES\Technopark\RFIDWSProxy"
InstallDirRegKey HKLM "Software\Technopark\RFIDWSProxy" "$INSTDIR"

ShowInstDetails show
ShowUninstDetails show

Section Install
 SetOutPath '$INSTDIR'
   WriteRegStr HKLM "Software\Technopark\RFIDWSProxy" "" "$INSTDIR"
   
   SimpleSC::ExistsService "RFIDWSProxy"
   Pop $0
   ${If} $0 == true
    SimpleSC::StopService "RFIDWSProxy" 0 30
    SimpleSC::RemoveService "RFIDWSProxy"
   ${EndIf}
   
   File '..\bin\Release\RFIDWSProxy.exe'
   File '..\bin\Release\Alchemy.dll'
   File '..\bin\Release\CommandLine.dll'
   File '..\bin\Release\ZReader.dll'
   
   SimpleSC::InstallService "RFIDWSProxy" "RFIDWSProxy" "16" "2" "$PROGRAMFILES\Technopark\RFIDWSProxy\RFIDWSProxy.exe" "" "" ""
   SimpleSC::StartService "RFIDWSProxy" '' 30
sectionend