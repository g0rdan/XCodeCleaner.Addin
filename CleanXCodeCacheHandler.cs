using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using MonoDevelop.Components.Commands;

using MonoDevelop.Ide;
using MonoDevelop.Ide.Gui;

namespace XCodeCleanerAddin
{
    public class CleanXCodeCacheHandler : CommandHandler
    {
        protected override void Run()
        {
            Task.Run(async () => {

                var username = await GetUserName();
                if (string.IsNullOrWhiteSpace(username))
                    return;

                username = username.Replace(Environment.NewLine, string.Empty);
                var path = $"/Users/{username}/Library/Developer/Xcode/DerivedData/";

                if (Directory.Exists(path))
                {
                    foreach (var innerDirectory in Directory.EnumerateDirectories(path))
                    {
                        try
                        {
                            Directory.Delete(innerDirectory, true);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"{innerDirectory} folder hasn't been deleted: {ex.Message}");
                        }
                    }
                }
            });
        }

        async Task<string> GetUserName()
        {
            using (var getUserNameProcess = new Process())
            {
                getUserNameProcess.StartInfo = new ProcessStartInfo("id")
                {
                    Arguments = "-un",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };

                var hasStarted = getUserNameProcess.Start();
                if (hasStarted)
                {
                    using (var outputStream = getUserNameProcess.StandardOutput)
                    {
                        return await outputStream.ReadToEndAsync();
                    }
                }
            }

            return string.Empty;
        }
    }

    public enum XcodeCleanerCommands
    {
        CleanXCodeCache
    }
}
