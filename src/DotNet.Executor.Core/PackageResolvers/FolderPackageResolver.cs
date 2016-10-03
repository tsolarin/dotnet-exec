using System;
using System.IO;
using System.Linq;
using DotNet.Executor.Core.Utils;

namespace DotNet.Executor.Core.PackageResolvers
{
    internal class FolderPackageResolver : PackageResolver
    {

        public FolderPackageResolver(DirectoryInfo packagesFolder, string source) : base(packagesFolder, source) { }

        protected override void Acquire()
        {
            var sourceFolder = new DirectoryInfo(this.Source);
            FileInfo projectJson = sourceFolder.GetFiles().FirstOrDefault(f => f.Name == "project.json");

            if (projectJson == null)
                throw new Exception("No project.json found in source folder");

            bool restore = ProcessRunner.RunProcess("dotnet", "restore", projectJson.FullName);
            if (!restore)
                throw new Exception("Package restore for project failed");

            string packageName = ProjectParser.GetPackageName(projectJson);

            this.PackageFolder = this.PackagesFolder.GetDirectories().FirstOrDefault(d => d.Name == packageName);
            if (this.PackageFolder != null)
                throw new Exception("Package already exists");

            this.PackageFolder = this.PackagesFolder.CreateSubdirectory(packageName);
            bool build = ProcessRunner.RunProcess("dotnet", "build", projectJson.FullName,
                "-c Release", "-f netcoreapp1.0", "-o " + this.PackageFolder.FullName);
            if (!build)
                throw new Exception("Project Compilation failed");
        }
    }
}