using System.IO;
using System.Text.Json;

using PhonerLiteSync.Model;

namespace PhonerLiteSync
{
    public class Helper
    {
        public void SaveSettings(PathGui settings)
        {
            try
            {
                var jsonString = JsonSerializer.Serialize(settings);
                File.WriteAllText(CsvHandler.SavePath, jsonString);
            }
            catch
            {
                // ignored
            }
        }

        public PathGui LoadSettings()
        {
            try
            {
                var jsonString = File.ReadAllText(CsvHandler.SavePath);
                return JsonSerializer.Deserialize<PathGui>(jsonString);
            }
            catch
            {
                return null;
            }
        }
    }
}
