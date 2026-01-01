using System.Diagnostics;

namespace GitAssistant.Application.Factories;

public static class GitRepositoryFactory
{
    public static string Create()
    {
        var tempPath = Path.Combine(
            Path.GetTempPath(),
            "git-test-" + Guid.NewGuid()
        );

        Directory.CreateDirectory(tempPath);

        Run("git init", tempPath);
        Run("git config user.email test@test.com", tempPath);
        Run("git config user.name Test User", tempPath);

        File.WriteAllText(Path.Combine(tempPath, "file.txt"), "initial");
        Run("git add .", tempPath);
        Run("git commit -m \"initial commit\"", tempPath);

        File.AppendAllText(Path.Combine(tempPath, "file.txt"), "\nchange");
        Run("git add .", tempPath);
        Run("git commit -m \"second commit\"", tempPath);

        return tempPath;
    }

    public static string Create(out Action cleanup)
    {
        var path = Create();
        Console.WriteLine($"Test repo created {path}");
        cleanup = () =>
        {
            Directory.Delete(path, true);

            if (Directory.Exists(path))
            {
                Console.WriteLine($"Something went wrong! {path}");
            }
            else
            {
                Console.WriteLine($"Test repo deleted {path}");
            }
        };
        return path;
    }


    private static void Run(string command, string workingDir)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "git",
            Arguments = command.Replace("git ", ""),
            WorkingDirectory = workingDir,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        using var process = Process.Start(psi)!;
        process.WaitForExit();

        if (process.ExitCode != 0)
            throw new InvalidOperationException(
                process.StandardError.ReadToEnd());
    }
}
