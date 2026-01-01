using System.Diagnostics;

namespace GitAssistant.Core.Factories;

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
        LogShortStack($"Test repo created {path}");
        cleanup = () =>
        {
            Directory.Delete(path, true);

            if (Directory.Exists(path))
            {
                LogShortStack($"Something went wrong! {path}");
            }
            else
            {
                LogShortStack($"Test repo deleted {path}");
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

    // TODO: Logger / Coomon
    private static void LogShortStack(string message)
    {
        var st = new StackTrace(skipFrames: 1, fNeedFileInfo: false);
        var frames = st.GetFrames();

        if (frames == null)
        {
            Console.WriteLine(message);
            return;
        }

        var myMethods = frames
            .Select(f => f.GetMethod())
            .Where(m => m != null)
            .Where(m => m.DeclaringType != null &&
                        m.DeclaringType.Namespace != null &&
                        m.DeclaringType.Namespace.StartsWith("GitAssistant"))
            .Select(m => m.Name)
            .Distinct()
            .ToArray();

        Console.WriteLine($"{message} (called from: {string.Join(" -> ", myMethods)})");
    }
}
