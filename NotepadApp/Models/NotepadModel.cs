using System.IO;

namespace NotepadApp.Models
{
    public class NotepadModel
    {
        public string Text { get; set; }

        public void LoadTextFromStream(MemoryStream memoryStream)
        {
            using (StreamReader reader = new StreamReader(memoryStream))
            {
                Text = reader.ReadToEnd();
            }
        }

        public void SaveTextToStream(MemoryStream memoryStream)
        {
            using (StreamWriter writer = new StreamWriter(memoryStream))
            {
                writer.Write(Text);
                writer.Flush();
            }
        }
    }
}
