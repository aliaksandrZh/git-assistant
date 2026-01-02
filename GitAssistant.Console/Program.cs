using GitAssistant.Console.Menu;
using GitAssistant.Console.StateMachines;
using GitAssistant.Core;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
services.AddApplicationServices();

services.AddSingleton<StateMachine>();
services.AddSingleton<Menu>();

var provider = services.BuildServiceProvider();
var menu = provider.GetRequiredService<Menu>();

await menu.RunAsync();
