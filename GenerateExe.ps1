dotnet publish -r win-x64 -c Release /p:PublishSingleFile=true -o ./bin/ --self-contained
del ./bin/*.pdb