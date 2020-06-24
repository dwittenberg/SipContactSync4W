dotnet publish -r win-x64 -c Release /p:PublishSingleFile=true /p:PublishTrimmed=true -o ./bin/ --self-contained
del ./bin/*.pdb