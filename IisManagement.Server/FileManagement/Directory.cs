using NLog;

namespace IisManagement.Server
{
    internal static class ImpersonatedFiles
    {

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static bool Exists(string path)
        {
            using (new FileHandling(path))
            {
                return System.IO.Directory.Exists(path);
            }
        }

        public static System.IO.DirectoryInfo CreateDirectory(string path)
        {
            using (new FileHandling(path))
            {
                return System.IO.Directory.CreateDirectory(path);
            }
        }

        public static void Delete(string path, bool recursive = false)
        {
            using (new FileHandling(path))
            {
                System.IO.Directory.Delete(path, recursive);
            }
        }               

        public static void CopyFilesRecursively(string sourceDirectory, string targetDirectory)
        {
            Logger.Info($"Copy Files from {sourceDirectory} to {targetDirectory}");

            using (new FileHandling(sourceDirectory))
            {
                using (new FileHandling(targetDirectory))
                {

                    if (Exists(targetDirectory))
                        CreateDirectory(targetDirectory);

                    CopyFilesRecursively(new System.IO.DirectoryInfo(sourceDirectory), new System.IO.DirectoryInfo(targetDirectory));
                }
            }
            Logger.Info($"Finished Copying Files from {sourceDirectory} to {targetDirectory}");
        }
        private static void CopyFilesRecursively(System.IO.DirectoryInfo source, System.IO.DirectoryInfo target)
        {
            foreach (var dir in source.GetDirectories())
                CopyFilesRecursively(dir, target.CreateSubdirectory(dir.Name));
            foreach (var file in source.GetFiles())
                file.CopyTo(System.IO.Path.Combine(target.FullName, file.Name));
        }
    }
}

