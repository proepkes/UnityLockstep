del ..\Unity\Assets\Integration\Lockstep.*
xcopy "Game" "..\Unity\Assets\GitIgnored\Game\" /exclude:excludeList.txt /y /S
xcopy "Common" "..\Unity\Assets\GitIgnored\Common\" /exclude:excludeList.txt /y /S
xcopy "Core.Logic" "..\Unity\Assets\GitIgnored\Core.Logic\" /exclude:excludeList.txt /y /S
xcopy "Core.State" "..\Unity\Assets\GitIgnored\Core.State\" /exclude:excludeList.txt /y /S
xcopy "Network" "..\Unity\Assets\GitIgnored\Network\" /exclude:excludeList.txt /y /S
xcopy "Network.Client" "..\Unity\Assets\GitIgnored\Network.Client\" /exclude:excludeList.txt /y /S