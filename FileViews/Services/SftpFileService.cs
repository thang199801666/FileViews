using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text; // Thêm để dùng StringBuilder
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using FileViews.Models;
using Renci.SshNet.Sftp;
using System.Windows.Media;

namespace FileViews.Services
{
    public class SftpFileService : IFileService
    {
        private readonly string _host;
        private readonly int _port;
        private readonly string _username;
        private readonly string _password;
        private SftpClient _sftpClient;
        private bool _isConnected;
        private readonly ImageSource _folderIcon;
        private readonly Dictionary<string, ImageSource> _fileIconCache;
        public string DefaultPath { get; private set; }

        public SftpFileService(string host, int port, string username, string password)
        {
            _host = host;
            _port = port;
            _username = username;
            _password = password;

            _folderIcon = GetSystemIcon("folder", true) ?? CreateFallbackIcon(true);
            _fileIconCache = new Dictionary<string, ImageSource>(StringComparer.OrdinalIgnoreCase)
            {
                { "", GetSystemIcon("defaultfile", false) ?? CreateFallbackIcon(false) }
            };
        }

        public void Connect()
        {
            if (_sftpClient == null || !_sftpClient.IsConnected)
            {
                _sftpClient = new SftpClient(_host, _port, _username, _password);
                _sftpClient.Connect();
                _isConnected = true;
                DefaultPath = _sftpClient.WorkingDirectory;
            }
        }

        public bool CanConnect()
        {
            return !string.IsNullOrEmpty(_host) &&
                   !string.IsNullOrEmpty(_username) &&
                   !string.IsNullOrEmpty(_password);
        }

        public IEnumerable<FileItem> ListFiles(string path)
        {
            if (!_isConnected)
                throw new InvalidOperationException("Not connected to SFTP server.");

            return _sftpClient.ListDirectory(path)
                .Where(f => f.Name != ".")
                .Select(f => new FileItem
                {
                    Name = f.Name,
                    FullPath = f.FullName,
                    IsDirectory = f.IsDirectory,
                    Size = f.Length,
                    LastModified = f.LastWriteTime,
                    Icon = f.IsDirectory ? _folderIcon : GetFileIconByExtension(Path.GetExtension(f.Name)),
                    Permissions = GetPosixPermissions(f.Attributes) // Sửa để trả về rwxr-xr-x
                });
        }

        public void DownloadFile(FileItem file, string localPath)
        {
            if (!_isConnected)
                throw new InvalidOperationException("Not connected to SFTP server.");

            if (!file.IsDirectory)
            {
                using (var fileStream = File.OpenWrite(localPath))
                {
                    _sftpClient.DownloadFile(file.FullPath, fileStream);
                }
            }
        }

        public void OpenFile(FileItem file)
        {
            if (file.IsDirectory)
            {
                throw new InvalidOperationException("Cannot open directory directly in SFTP.");
            }
            else
            {
                string localPath = Path.Combine(Path.GetTempPath(), file.Name);
                DownloadFile(file, localPath);
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(localPath) { UseShellExecute = true });
            }
        }

        public void SaveFile(FileItem file, string content, Encoding encoding = null)
        {
            if (!_isConnected) return;

            encoding ??= Encoding.UTF8;

            using (var memoryStream = new MemoryStream(encoding.GetBytes(content)))
            {
                _sftpClient.UploadFile(memoryStream, file.FullPath, true);
            }
        }

        public void WriteFileAsText(string remotePath, string content)
        {
            if (!_isConnected) return;
            using (var memoryStream = new MemoryStream())
            {
                using (var writer = new StreamWriter(memoryStream))
                {
                    writer.Write(content);
                    writer.Flush();
                    memoryStream.Position = 0;
                    _sftpClient.UploadFile(memoryStream, remotePath, true);
                }  
            }
        }

        /// <summary>
        /// Reads a remote SFTP file into memory.
        /// Caller is responsible for disposing the returned stream.
        /// </summary>
        public MemoryStream ReadFileAsMemoryStream(string remotePath)
        {
            // Get the file size from SFTP server
            long fileSize = _sftpClient.GetAttributes(remotePath).Size;

            // Preallocate MemoryStream with the file size
            var memoryStream = new MemoryStream((int)fileSize); // Cast to int, because MemoryStream size should be < 2GB

            using (var sftpStream = _sftpClient.OpenRead(remotePath))
            {
                sftpStream.CopyTo(memoryStream);
            }

            memoryStream.Position = 0; // Reset position for reading
            return memoryStream;
        }

        public void Dispose()
        {
            if (_sftpClient != null && _sftpClient.IsConnected)
            {
                _sftpClient.Disconnect();
                _sftpClient.Dispose();
            }
        }

        private ImageSource GetFileIconByExtension(string extension)
        {
            if (string.IsNullOrEmpty(extension))
                return _fileIconCache[""];

            extension = extension.ToLowerInvariant();
            if (!extension.StartsWith("."))
                extension = "." + extension;

            if (!_fileIconCache.ContainsKey(extension))
            {
                ImageSource icon = GetSystemIcon($"file{extension}", false) ?? _fileIconCache[""];
                _fileIconCache[extension] = icon;
            }

            return _fileIconCache[extension];
        }

        private ImageSource GetSystemIcon(string fileName, bool isDirectory)
        {
            var shfi = new SHFILEINFO();
            var flags = SHGFI_ICON | SHGFI_SMALLICON | SHGFI_USEFILEATTRIBUTES;
            uint fileAttributes = isDirectory ? FILE_ATTRIBUTE_DIRECTORY : FILE_ATTRIBUTE_NORMAL;

            SHGetFileInfo(fileName, fileAttributes, ref shfi, (uint)Marshal.SizeOf(shfi), flags);

            if (shfi.hIcon != IntPtr.Zero)
            {
                try
                {
                    using (Icon icon = Icon.FromHandle(shfi.hIcon))
                    {
                        using (Bitmap bitmap = icon.ToBitmap())
                        {
                            IntPtr hBitmap = bitmap.GetHbitmap();
                            BitmapSource bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(
                                hBitmap,
                                IntPtr.Zero,
                                System.Windows.Int32Rect.Empty,
                                BitmapSizeOptions.FromEmptyOptions());
                            bitmapSource.Freeze();
                            DeleteObject(hBitmap);
                            return bitmapSource;
                        }
                    }
                }
                finally
                {
                    DestroyIcon(shfi.hIcon);
                }
            }
            return null;
        }

        private ImageSource CreateFallbackIcon(bool isDirectory)
        {
            using (var bitmap = new Bitmap(16, 16))
            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.Clear(isDirectory ? System.Drawing.Color.Yellow : System.Drawing.Color.White);
                graphics.DrawRectangle(Pens.Black, 0, 0, 15, 15);
                if (isDirectory)
                    graphics.FillRectangle(System.Drawing.Brushes.Gold, 2, 2, 12, 12);

                IntPtr hBitmap = bitmap.GetHbitmap();
                BitmapSource bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(
                    hBitmap,
                    IntPtr.Zero,
                    System.Windows.Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
                bitmapSource.Freeze();
                DeleteObject(hBitmap);
                return bitmapSource;
            }
        }

        private string GetPosixPermissions(SftpFileAttributes attributes) // Thêm để chuyển thành rwxr-xr-x
        {
            var sb = new StringBuilder(9);
            // Owner permissions
            sb.Append(attributes.OwnerCanRead ? "r" : "-");
            sb.Append(attributes.OwnerCanWrite ? "w" : "-");
            sb.Append(attributes.OwnerCanExecute ? "x" : "-");
            // Group permissions
            sb.Append(attributes.GroupCanRead ? "r" : "-");
            sb.Append(attributes.GroupCanWrite ? "w" : "-");
            sb.Append(attributes.GroupCanExecute ? "x" : "-");
            // Others permissions
            sb.Append(attributes.OthersCanRead ? "r" : "-");
            sb.Append(attributes.OthersCanWrite ? "w" : "-");
            sb.Append(attributes.OthersCanExecute ? "x" : "-");

            return sb.ToString();
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct SHFILEINFO
        {
            public IntPtr hIcon;
            public int iIcon;
            public uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        }

        private const uint SHGFI_ICON = 0x000000100;
        private const uint SHGFI_SMALLICON = 0x000000001;
        private const uint SHGFI_USEFILEATTRIBUTES = 0x000000010;
        private const uint FILE_ATTRIBUTE_NORMAL = 0x00000080;
        private const uint FILE_ATTRIBUTE_DIRECTORY = 0x00000010;

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbFileInfo, uint uFlags);

        [DllImport("user32.dll")]
        private static extern bool DestroyIcon(IntPtr hIcon);

        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);
    }
}