del /q "..\Unity\Assets\GitIgnored\*" /y
FOR /D %%p IN ("..\Unity\Assets\GitIgnored\*.*") DO rmdir "%%p" /s /q