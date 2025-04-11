using System;
using Renci.SshNet;

class Program
{
    static void Main(string[] args)
    {
        string host = "localhost";
        int port = 2223;
        string username = "myuser";
        string password = "mypassword";
        string remoteDirectory = "/home/myuser";

        using (var sftp = new SftpClient(host, port, username, password))
        {
            try
            {
                sftp.Connect();
                Console.WriteLine("✅ Connected to SFTP server");

                var files = sftp.ListDirectory(remoteDirectory);

                Console.WriteLine($"📂 Files in {remoteDirectory}:");
                foreach (var file in files)
                {
                    // Skip . and ..
                    if (file.Name != "." && file.Name != "..")
                    {
                        Console.WriteLine($"- {file.Name} ({file.Length} bytes)");
                    }
                }

                sftp.Disconnect();
                Console.WriteLine("❎ Disconnected");
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error: " + ex.Message);
            }
        }
    }
}
