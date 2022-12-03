
# description of script

# publish sysinternals.installer in single file mode
dotnet publish --configuration release --use-current-runtime --self-contained /p:PublishSingleFile=true ../sysinternals.installer/sysinternals.installer.csproj
