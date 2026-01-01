DOTNET_ENVIRONMENT=Development dotnet run --project consumers/GitAssistant.Console
dotnet test --filter LoadBaseSettings_ShouldLoadCorrectly --logger "console;verbosity=detailed" -e DOTNET_ENVIRONMENT=Production
