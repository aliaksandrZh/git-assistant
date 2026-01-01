using GitAssistant.Application.Services;
using GitAssistant.Application.Factories;

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
            var runner = new GitCommandRunner();
            var service = new GitLogService(runner);

            // act
            var commits = await service.GetCommitsAsync(repoPath, null, default);

            // assert
            Assert.NotEmpty(commits);
        }
        finally
        {
            cleanup();
        }
    }
}
