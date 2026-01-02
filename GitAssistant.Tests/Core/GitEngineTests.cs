using System.Threading;
using System.Threading.Tasks;
using GitAssistant.Core.Interface;
using GitAssistant.Core.Models;
using GitAssistant.Core.Services;
using Moq;
using Xunit;

public sealed class GitEngineTests
{
    private readonly Mock<IGitLogService> _logServiceMock;
    private readonly Mock<IGitCommandRunner> _runnerMock;
    private readonly GitEngine _sut;

    public GitEngineTests()
    {
        _logServiceMock = new Mock<IGitLogService>(MockBehavior.Strict);
        _runnerMock = new Mock<IGitCommandRunner>(MockBehavior.Strict);

        _sut = new GitEngine(_logServiceMock.Object, _runnerMock.Object);
    }

    [Fact]
    public async Task GetCommitsAsync_calls_log_service_and_returns_result()
    {
        // arrange
        var options = new GitLog { Path = string.Empty };
        var token = CancellationToken.None;

        IReadOnlyList<GitCommit> expected = new[]
        {
            new GitCommit("1", "A1", DateTime.Now, "m1"),
            new GitCommit("2", "A2", DateTime.Now, "m2"),
        };

        _logServiceMock
            .Setup(x => x.GetCommitsAsync(options, token))
            .ReturnsAsync(expected);

        // act
        var result = await _sut.GetCommitsAsync(options, token);

        // assert
        Assert.Same(expected, result);

        _logServiceMock.Verify(
            x => x.GetCommitsAsync(options, token),
            Times.Once);
    }

    [Fact]
    public async Task SearchCommitsAsync_calls_log_service_and_returns_result()
    {
        // arrange
        var options = new GitLog { Path = string.Empty };
        var token = CancellationToken.None;

        IReadOnlyList<GitCommit> expected = new[]
        {
            new GitCommit("1", "A1", DateTime.Now, "m1"),
        };

        _logServiceMock
            .Setup(x => x.SearchAsync(options, token))
            .ReturnsAsync(expected);

        // act
        var result = await _sut.SearchCommitsAsync(options, token);

        // assert
        Assert.Same(expected, result);

        _logServiceMock.Verify(
            x => x.SearchAsync(options, token),
            Times.Once);
    }

    [Fact]
    public async Task ValidateAsync_directory_does_not_exist_returns_DirectoryNotFound()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        var result = await _sut.ValidateAsync(path, CancellationToken.None);

        Assert.Equal(GitRepositoryStatus.DirectoryNotFound, result);

        _runnerMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ValidateAsync_exit_code_zero_returns_Valid()
    {
        var path = CreateTempDirectory();

        try
        {
            _runnerMock
               .Setup(r => r.RunAsync(
                   "rev-parse --is-inside-work-tree",
                   path,
                   It.IsAny<CancellationToken>()))
               .ReturnsAsync(new GitCommandResult(0, string.Empty, string.Empty));

            var result = await _sut.ValidateAsync(path, CancellationToken.None);

            Assert.Equal(GitRepositoryStatus.Valid, result);
        }
        finally
        {
            Directory.Delete(path, true);
        }
    }

    [Fact]
    public async Task ValidateAsync_not_a_git_repository_returns_NotAGitRepository()
    {
        var path = CreateTempDirectory();

        try
        {
            _runnerMock
                       .Setup(r => r.RunAsync(
                           It.IsAny<string>(),
                           path,
                           It.IsAny<CancellationToken>()))
                       .ReturnsAsync(new GitCommandResult(128, string.Empty, "fatal: not a git repository"));

            var result = await _sut.ValidateAsync(path, CancellationToken.None);

            Assert.Equal(GitRepositoryStatus.NotAGitRepository, result);
        }
        finally
        {
            Directory.Delete(path, true);
        }
    }

    [Fact]
    public async Task ValidateAsync_git_not_installed_returns_GitNotInstalled()
    {
        var path = CreateTempDirectory();

        try
        {
            _runnerMock
               .Setup(r => r.RunAsync(
                   It.IsAny<string>(),
                   path,
                   It.IsAny<CancellationToken>()))
               .ReturnsAsync(new GitCommandResult(1, string.Empty, "git: not found"));
            var result = await _sut.ValidateAsync(path, CancellationToken.None);

            Assert.Equal(GitRepositoryStatus.GitNotInstalled, result);
        }
        finally
        {
            Directory.Delete(path, true);
        }

    }

    [Fact]
    public async Task ValidateAsync_unknown_error_returns_UnknownError()
    {
        var path = CreateTempDirectory();

        try
        {
            _runnerMock
           .Setup(r => r.RunAsync(
               It.IsAny<string>(),
               path,
               It.IsAny<CancellationToken>()))
           .ReturnsAsync(new GitCommandResult(1, string.Empty, "some weird error"));

            var result = await _sut.ValidateAsync(path, CancellationToken.None);

            Assert.Equal(GitRepositoryStatus.UnknownError, result);
        }
        finally
        {
            Directory.Delete(path, true);
        }
    }


    private static string CreateTempDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(path);
        return path;
    }
}
