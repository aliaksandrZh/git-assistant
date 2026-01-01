using GitAssistant.Core.Services;
using GitAssistant.Core.Factories;
using GitAssistant.Core.Models;
using Moq;
using GitAssistant.Core.Interface;

namespace GitAssistant.Tests.Core;


public class GitLogTests
{
    [Fact]
    public async Task Returns_commits_from_repo()
    {

        var repoPath = GitRepositoryFactory.Create(out var cleanup);
        try
        {
            // Arrange
            var runner = new ProcessRunner();
            var gitRunner = new GitCommandRunner(runner);
            var service = new GitLogService(gitRunner);

            // act
            var commits = await service.GetCommitsAsync(new GitLog { Path = repoPath }, default);

            // assert
            Assert.NotEmpty(commits);
        }
        finally
        {
            cleanup();
        }
    }

    [Fact]
    public async Task SearchAsync_WithEmptyQuery_ReturnsAllCommits()
    {
        var mockRunner = new Mock<IGitCommandRunner>();
        mockRunner
            .Setup(r => r.RunAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GitCommandResult(
                0,
                @"1|Alice|2026-01-01 10:00:00|initial commit
                  2|Bob|2026-01-01 11:00:00|second commit",
                ""
            ));

        var service = new GitLogService(mockRunner.Object);

        var options = new GitLog { Path = "", Query = null };

        // Act
        var result = await service.SearchAsync(options, CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("initial commit", result[0].Message);
        Assert.Equal("second commit", result[1].Message);
    }

    [Fact]
    public async Task SearchAsync_WithQuery_ReturnsMatchingCommits()
    {
        // Arrange
        var mockRunner = new Mock<IGitCommandRunner>();
        mockRunner
            .Setup(r => r.RunAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GitCommandResult(
                0,
                @"1|Alice|2026-01-01 10:00:00|initial commit
                  2|Bob|2026-01-01 11:00:00|second commit #1
                  3|Bob|2026-01-01 11:00:00|third commit #1",
                ""
            ));

        var service = new GitLogService(mockRunner.Object);

        var options = new GitLog { Path = "", Query = "1" };

        // Act
        var result = await service.SearchAsync(options, CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("second commit #1", result[0].Message);
        Assert.Equal("third commit #1", result[1].Message);
    }

    [Fact]
    public async Task SearchAsync_QueryIsCaseInsensitive()
    {
        // Arrange
        var mockRunner = new Mock<IGitCommandRunner>();
        mockRunner
            .Setup(r => r.RunAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GitCommandResult(
                0,
                @"1|Alice|2026-01-01 10:00:00|initial commit
                  2|Bob|2026-01-01 11:00:00|second commit #1 async
                  3|Bob|2026-01-01 11:00:00|third commit #1 Async",
                ""
            ));

        var service = new GitLogService(mockRunner.Object);

        var options = new GitLog { Path = "", Query = "AsYnC" };

        // Act
        var result = await service.SearchAsync(options, CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("second commit #1 async", result[0].Message);
        Assert.Equal("third commit #1 Async", result[1].Message);
    }
}
