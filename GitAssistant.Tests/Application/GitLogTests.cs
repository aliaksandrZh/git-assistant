using GitAssistant.Application.Services;
using GitAssistant.Application.Factories;
using GitAssistant.Application.Models;

namespace GitAssistant.Tests.Application;


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
}
