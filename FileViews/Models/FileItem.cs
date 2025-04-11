using System.Windows.Media;

namespace FileViews.Models
{
    public class FileItem
    {
        public string Name { get; set; }
        public string FullPath { get; set; }
        public bool IsDirectory { get; set; }
        public long Size { get; set; }
        public DateTime LastModified { get; set; }
        public ImageSource Icon { get; set; }
        public string Permissions { get; set; } // Thêm thuộc tính Permissions
    }
}