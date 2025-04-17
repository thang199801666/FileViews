using FileViews.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FileViews.Services
{
    public interface IFileService : IDisposable
    {
        void Connect();
        bool CanConnect();
        IEnumerable<FileItem> ListFiles(string path);
        void DownloadFile(FileItem file, string localPath);
        void OpenFile(FileItem file);
        void WriteFileAsText(string remotePath, string content);
        MemoryStream ReadFileAsMemoryStream(string remotePath);
    }
}
