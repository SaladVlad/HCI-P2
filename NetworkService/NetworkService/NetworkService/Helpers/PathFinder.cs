using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NetworkService.Helpers
{
    public static class PathFinder
    {
        public static string FindPath(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
                throw new ArgumentNullException(nameof(relativePath), "Relative path cannot be null or empty.");

            // Check if the provided path is already absolute
            if (Path.IsPathRooted(relativePath) && relativePath[0]!='/')
                return relativePath;

            // Get the directory where the executing assembly is located
            string assemblyLocation = Assembly.GetExecutingAssembly().Location;
            string assemblyDirectory = Path.GetDirectoryName(assemblyLocation);

            // Navigate back two folders from the root directory
            string rootDirectory = Path.Combine(assemblyDirectory, "..\\..\\");

            // Combine the root directory with the relative path to get the absolute path
            string absolutePath = Path.Combine(rootDirectory, relativePath);
            return Path.GetFullPath(absolutePath);
        }
    }
}
