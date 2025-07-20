using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;

namespace Disco.Services.Plugins.ManifestGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                throw new ArgumentException("Only one command-line argument is expected, the path to the plugin assembly");
            }

            var assemblyPath = args[0];
            var assemblyFileInfo = new FileInfo(assemblyPath);

            // Ensure File Exists
            if (!assemblyFileInfo.Exists)
            {
                throw new ArgumentException($"File not found at: {assemblyFileInfo.FullName}");
            }

            Console.WriteLine("Disco ICT Plugin: Generating Manifest");

            AppDomain.CurrentDomain.AssemblyLoad += CurrentDomain_AssemblyLoad;
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

            // Load Plugin Assembly
            Assembly assembly;
            try
            {
                assembly = Assembly.LoadFile(assemblyFileInfo.FullName);
            }
            catch (Exception ex)
            {
                throw new Exception($"Unable to load the Assembly: {assemblyFileInfo.FullName}", ex);
            }

            // Load Reference Assemblies
            foreach (var referenceAssembly in assemblyFileInfo.Directory.GetFiles("*.dll"))
            {
                if (referenceAssembly.FullName != assemblyFileInfo.FullName)
                {
                    if (!PluginManifest.PluginExcludedAssemblies.Any(excludeAssembly => referenceAssembly.Name.StartsWith(excludeAssembly, StringComparison.OrdinalIgnoreCase)))
                    {
                        try
                        {

                            Assembly.LoadFile(referenceAssembly.FullName);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Disco ICT Plugin: Warning: Unable to load reference '{referenceAssembly.FullName}'; {ex.Message}");
                        }
                    }
                }
            }

            // Create Manifest
            var manifest = PluginManifest.FromPluginAssembly(assembly);

            var manifestFilePath = Path.Combine(assemblyFileInfo.DirectoryName, "manifest.json");

            File.WriteAllText(manifestFilePath, manifest.ToManifestFile());

            Console.WriteLine("Disco ICT Plugin: Manifest Created");

            Console.WriteLine("Disco ICT Plugin: Building Package");

            var packageFileName = $"{manifest.Id}-{manifest.Version}.discoPlugin";
            var packageFilePath = Path.Combine(assemblyFileInfo.DirectoryName, packageFileName);

            // Delete Existing Package Files
            foreach (var existingPackages in assemblyFileInfo.Directory.EnumerateFiles($"{manifest.Id}*.discoPlugin"))
            {
                existingPackages.Delete();
            }

            // Exclude Disco ICT Provided Assemblies
            List<string> excludedFiles = PluginManifest.PluginExcludedAssemblies.ToList();

            // Exclude the Package File
            excludedFiles.Add(packageFileName);

            using (FileStream packageStream = new FileStream(packageFilePath, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None))
            {
                using (ZipArchive package = new ZipArchive(packageStream, ZipArchiveMode.Create))
                {
                    BuildZipPackage(package, string.Empty, assemblyFileInfo.Directory, excludedFiles);
                }
            }

            Console.WriteLine($"Disco ICT Plugin: Package Build: '{packageFileName}'");

        }

        static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            Console.WriteLine($"CurrentDomain_AssemblyResolve: {args.Name} - {args.RequestingAssembly.FullName}");

            foreach (var loadedAssembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (loadedAssembly.FullName == args.Name)
                    return loadedAssembly;
            }

            return null;
        }

        static void CurrentDomain_AssemblyLoad(object sender, AssemblyLoadEventArgs args)
        {
            Console.WriteLine($"CurrentDomain_AssemblyLoad: {args.LoadedAssembly.FullName} - {args.LoadedAssembly.Location}");
        }

        static void BuildZipPackage(ZipArchive package, string relativePath, DirectoryInfo directory, List<string> excludedFiles)
        {
            foreach (var subDirectory in directory.EnumerateDirectories())
            {
                BuildZipPackage(package, string.Concat(relativePath, subDirectory.Name, "\\"), subDirectory, excludedFiles);
            }

            foreach (var file in directory.EnumerateFiles())
            {
                if (!excludedFiles.Any(excludeRule => file.Name.StartsWith(excludeRule, StringComparison.OrdinalIgnoreCase)))
                {
                    var archiveEntry = package.CreateEntry(string.Concat(relativePath, file.Name), CompressionLevel.Fastest);

                    using (var archiveStream = archiveEntry.Open())
                    {
                        using (var fileStream = file.OpenRead())
                        {
                            fileStream.CopyTo(archiveStream);
                        }
                    }
                }
            }
        }
    }
}
