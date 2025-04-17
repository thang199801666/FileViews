using FileViews.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FileViews.Services
{
    public class LocalFileService : IFileService
    {
        public void Connect()
        {
            // Không cần kết nối cho local file system
        }

        public bool CanConnect()
        {
            return true; // Luôn có thể truy cập
        }

        public IEnumerable<FileItem> ListFiles(string path)
        {
            try
            {
                var dirInfo = new DirectoryInfo(path);
                return dirInfo.GetFileSystemInfos()
                    .Select(f => new FileItem
                    {
                        Name = f.Name,
                        FullPath = f.FullName,
                        IsDirectory = f is DirectoryInfo,
                        Size = f is FileInfo file ? file.Length : 0,
                        LastModified = f.LastWriteTime
                    });
            }
            catch (Exception ex)
            {
                throw new IOException($"Error listing files in {path}: {ex.Message}");
            }
        }

        public void DownloadFile(FileItem file, string localPath)
        {
            if (!file.IsDirectory)
            {
                File.Copy(file.FullPath, localPath, true);
            }
        }

        public void OpenFile(FileItem file)
        {
            if (!file.IsDirectory)
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(file.FullPath) { UseShellExecute = true });
            }
        }

        public void WriteFileAsText(string filePath, string content)
        {
            File.WriteAllText(filePath, content);
        }

        public MemoryStream ReadFileAsMemoryStream(string remotePath)
        {
            return null;
        }

        public void Dispose()
        {
            // Không cần dọn dẹp
        }
    }
}
