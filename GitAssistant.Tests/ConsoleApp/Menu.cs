using System.IO;
using System.Threading;
using System.Threading.Tasks;
using GitAssistant.Console.StateMachines;
using GitAssistant.Console.Enums;
using GitAssistant.Console.Menu;
using GitAssistant.Core.Interface;
using Moq;
using Xunit;
using System;
using System.Collections.Generic;
using GitAssistant.Core.Models;
using System.Linq;

public class MenuTests
{
    [Fact]
    public async Task RunAsync_SearchFlow_CompletesSuccessfully()
    {
        // Arrange
        var gitMock = new Mock<IGitEngine>();
        var sm = new StateMachine();

        var commits = new List<GitCommit>
        {
            new GitCommit("1", "Alice", DateTime.Now, "initial commit"),
            new GitCommit("2", "Bob", DateTime.Now, "second commit")
        };

        gitMock.Setup(g => g.SearchCommitsAsync(It.IsAny<GitLog>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(commits);

        var input = new StringReader("test-repo\n1\ncommit\n10\n2\ny\n");
        Console.SetIn(input);
        var output = new StringWriter();
        Console.SetOut(output);

        var menu = new Menu(gitMock.Object, sm);

        // Act
        await menu.RunAsync();

        // Assert
        var consoleOutput = output.ToString();
        Assert.Contains("Enter git repository path:", consoleOutput);
        Assert.Contains("Menu:", consoleOutput);
        Assert.Contains("Commits:", consoleOutput);
        Assert.Contains("initial commit", consoleOutput);
        Assert.Contains("second commit", consoleOutput);
        Assert.Equal(State.ExitConfirmed, sm.CurrentState);
    }
}
