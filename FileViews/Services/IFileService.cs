using FileViews.Models;
using System;
using System.Collections.Generic;

namespace FileViews.Services
{
    public interface IFileService : IDisposable
    {
        void Connect();
        bool CanConnect();
        IEnumerable<FileItem> ListFiles(string path);
        void DownloadFile(FileItem file, string localPath);
        void OpenFile(FileItem file);
    }
}
