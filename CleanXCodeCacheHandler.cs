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
            Task.WaitAll(
                CleanDerivedData(),
                CleanVarFolders()
            );
        }

        Task CleanDerivedData()
        {
            return Task.Run(async () => {

                var username = await GetUserName();
                if (string.IsNullOrWhiteSpace(username))
                {
                    Console.WriteLine($"XCode Cleaner: can't find user name in the system");
                    return;
                }

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
                            Console.WriteLine($"XCode Cleaner: {innerDirectory} folder hasn't been deleted: {ex.Message}");
                        }
                    }
                }
                else
                    Console.WriteLine($"XCode Cleaner: {path} path doesn't exist");
            });
        }

        /// <summary>
        /// TODO: actually, usual instance of vs4mac doesn't have permissions
        /// to delete folders under /var folder
        /// </summary>
        Task CleanVarFolders()
        {
            return Task.Run(() => {

                var path = "/var/folders";
                if (Directory.Exists(path))
                {
                    foreach (var innerDirectoryPath in Directory.EnumerateDirectories(path))
                    {
                        var innerDirectory = Directory.CreateDirectory(innerDirectoryPath);
                        foreach (var file in innerDirectory.EnumerateFiles())
                        {
                            try
                            {
                                file.IsReadOnly = false;
                                file.Delete();
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"XCode Cleaner: {file} file hasn't been deleted: {ex.Message}");
                            }
                        }
                        foreach (var folder in innerDirectory.EnumerateDirectories())
                        {
                            try
                            {
                                folder.Delete();
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"XCode Cleaner: {folder} fodler folder hasn't been cleaned: {ex.Message}");
                            }
                        }
                    }
                }
                else
                    Console.WriteLine($"XCode Cleaner: {path} path doesn't exist");
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
