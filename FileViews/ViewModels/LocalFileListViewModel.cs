using FileViews.Services;
using System;

namespace FileViews.ViewModels
{
    public class LocalFileListViewModel : FileListViewModelBase
    {
        public LocalFileListViewModel()
            : base(new LocalFileService())
        {
            CurrentPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
            IsConnected = true; // Local file system luôn sẵn sàng
            ExecuteRefresh(null);
        }
    }
}
