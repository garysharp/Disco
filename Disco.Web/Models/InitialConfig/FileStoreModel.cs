using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;

namespace Disco.Web.Models.InitialConfig
{
    public class FileStoreModel
    {
        public FileStoreModel()
        {
            DirectoryModel = FileStoreModel.FileStoreDirectoryModel.DirectoryRoots();
        }

        [Required(), CustomValidation(typeof(FileStoreModel), "DirectoryPermissionRequired")]
        public string FileStoreLocation { get; set; }

        public FileStoreDirectoryModel DirectoryModel { get; set; }

        public void ExpandDirectoryModel()
        {
            if (!string.IsNullOrWhiteSpace(FileStoreLocation))
            {
                var branches = FileStoreLocation.ToUpper().Split(Path.DirectorySeparatorChar);
                var branchesCase = FileStoreLocation.Split(Path.DirectorySeparatorChar);
                FileStoreDirectoryModel branchModel;
                FileStoreDirectoryModel branchParent = DirectoryModel;
                for (int i = 0; i < branches.Length; i++)
                {
                    var branch = branches[i];
                    if (branchParent.SubDirectories.TryGetValue(branch, out branchModel))
                    {
                        branchModel.ExpandSubDirectories();
                    }
                    else
                    {
                        // Drive Letter doesn't exist
                        if (branchParent.Path == null)
                            break;

                        // New
                        branchModel = FileStoreDirectoryModel.FromNew(branchesCase[i], Path.Combine(branchParent.Path, branchesCase[i]));
                        branchParent.SubDirectories.Add(branchModel.Name.ToUpper(), branchModel);
                    }

                    branchParent = branchModel;
                }
            }
        }

        public static ValidationResult DirectoryPermissionRequired(object value, ValidationContext validationContext)
        {
            var instance = validationContext.ObjectInstance as FileStoreModel;

            if (instance != null && !string.IsNullOrEmpty(instance.FileStoreLocation))
            {
                var stringValue = value as string;

                DirectoryInfo info = new DirectoryInfo(stringValue);

                if (!info.Exists)
                {
                    // Try and Create
                    try
                    {
                        info.Create();
                    }
                    catch (Exception ex)
                    {
                        return new ValidationResult($"Unable to Create Directory '{info.FullName}'; [{ex.GetType().Name}] {ex.Message}");
                    }
                }
            }

            return ValidationResult.Success;
        }


        public class FileStoreDirectoryModel
        {
            public string Name { get; set; }
            public string Path { get; set; }
            public bool IsNew { get; set; }
            public bool Selectable { get; set; }
            public Dictionary<string, FileStoreDirectoryModel> SubDirectories { get; set; }

            internal static FileStoreDirectoryModel FromInfo(DirectoryInfo info)
            {
                return new FileStoreDirectoryModel()
                {
                    Name = info.Name,
                    Path = info.FullName,
                    IsNew = false,
                    Selectable = (info.Root.Name != info.Name)
                };
            }
            internal static FileStoreDirectoryModel FromNew(string Name, string FullPath)
            {
                return new FileStoreDirectoryModel()
                {
                    Name = Name,
                    Path = FullPath,
                    IsNew = true,
                    Selectable = true,
                    SubDirectories = new Dictionary<string, FileStoreDirectoryModel>()
                };
            }
            internal static FileStoreDirectoryModel FromInfo(DriveInfo info)
            {
                return new FileStoreDirectoryModel()
                {
                    Name = info.Name.Substring(0, 2),
                    Path = info.RootDirectory.Name,
                    IsNew = false,
                    Selectable = false
                };
            }
            internal static FileStoreDirectoryModel FromPath(string DirectoryPath, bool ExpandSubDirectories)
            {
                DirectoryInfo info = new DirectoryInfo(DirectoryPath);

                if (info.Exists)
                {
                    var fsd = FromInfo(info);

                    if (ExpandSubDirectories)
                        fsd.ExpandSubDirectories();

                    return fsd;
                }
                else
                    return null;
            }

            internal static FileStoreDirectoryModel DirectoryRoots()
            {
                var root = new FileStoreDirectoryModel()
                {
                    Name = "ROOT",
                    Path = null,
                    IsNew = false,
                    Selectable = false,
                    SubDirectories = new Dictionary<string, FileStoreDirectoryModel>()
                };

                foreach (var driveInfo in DriveInfo.GetDrives())
                {
                    if (driveInfo.DriveType == DriveType.Fixed)
                        root.SubDirectories.Add(driveInfo.Name.Substring(0, 2).ToUpper(), FileStoreDirectoryModel.FromInfo(driveInfo));
                }

                return root;
            }

            internal void ExpandSubDirectories()
            {
                if (SubDirectories == null)
                {
                    SubDirectories = new Dictionary<string, FileStoreDirectoryModel>();
                    if (!IsNew)
                    {
                        var dirInfo = new DirectoryInfo(Path);
                        if (dirInfo.Exists)
                        {
                            foreach (var subDir in dirInfo.EnumerateDirectories())
                            {
                                if (((subDir.Attributes & FileAttributes.System) != FileAttributes.System) &&
                                    ((subDir.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden))
                                {
                                    SubDirectories.Add(subDir.Name.ToUpper(), FileStoreDirectoryModel.FromInfo(subDir));
                                }
                            }
                        }
                        else
                        {
                            IsNew = true;
                        }
                    }
                }
            }
        }
    }
}