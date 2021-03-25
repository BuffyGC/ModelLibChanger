using System.Text.Json.Serialization;

namespace ModelLibChanger.Model
{

    public class Content : BaseModel
    {
        [JsonPropertyName("path")]
        public string Path { get => path; set => SetField(ref path, value) ; }
        private string path;

        [JsonPropertyName("size")]
        public long Size { get => size; set => SetField(ref size, value); }
        private long size;

        [JsonPropertyName("date")]
        public long Date { get => date; set => SetField(ref date, value); }
        private long date;
    }
}

