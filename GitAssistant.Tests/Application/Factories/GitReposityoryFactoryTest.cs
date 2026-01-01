using GitAssistant.Core.Factories;
using System.Diagnostics;
using Xunit;

namespace GitAssistant.Tests.Factories;

public class GitRepositoryFactoryTests
{
    [Fact]
    public void Create_ShouldCreateDirectory()
    {
        var path = GitRepositoryFactory.Create();

        try
        {
            Assert.NotNull(path);
            Assert.True(Directory.Exists(path));
        }
        finally
        {
            Directory.Delete(path, true);
        }
    }

    [Fact]
    public void Create_ShouldInitializeGitRepository()
    {
        var path = GitRepositoryFactory.Create();

        try
        {
            Assert.True(Directory.Exists(Path.Combine(path, ".git")));
        }
        finally
        {
            Directory.Delete(path, true);
        }
    }

    [Fact]
    public void Create_ShouldCreateInitialCommits()
    {
        var path = GitRepositoryFactory.Create();

        try
        {
            var commits = RunGit("log --oneline", path);
            var lines = commits.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

            Assert.Equal(2, lines.Length);
            Assert.Contains(lines, l => l.Contains("initial commit"));
            Assert.Contains(lines, l => l.Contains("second commit"));
        }
        finally
        {
            Directory.Delete(path, true);
        }
    }

    [Fact]
    public void Create_ShouldCreateFileWithExpectedContent()
    {
        var path = GitRepositoryFactory.Create();

        try
        {
            var filePath = Path.Combine(path, "file.txt");
            Assert.True(File.Exists(filePath));

            var content = File.ReadAllText(filePath);
            Assert.Contains("initial", content);
            Assert.Contains("change", content);
        }
        finally
        {
            Directory.Delete(path, true);
        }
    }

    [Fact]
    public void Create_WithCleanup_ShouldDeleteRepository()
    {
        var path = GitRepositoryFactory.Create(out var cleanup);

        Assert.True(Directory.Exists(path));

        cleanup();

        Assert.False(Directory.Exists(path));
    }

    [Fact]
    public void Create_ShouldThrowIfGitIsNotAvailable()
    {
        if (GitExists())
            return;

        // act & assert
        Assert.Throws<InvalidOperationException>(() => GitRepositoryFactory.Create());
    }

    private static string RunGit(string args, string workingDir)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "git",
            Arguments = args,
            WorkingDirectory = workingDir,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        using var process = Process.Start(psi)!;
        process.WaitForExit();

        return process.StandardOutput.ReadToEnd();
    }

    private static bool GitExists()
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = "--version",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi)!;
            process.WaitForExit();
            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }
}
