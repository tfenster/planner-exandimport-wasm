// mostly generated through the excellent "Paste JSON as Code" extension (https://marketplace.visualstudio.com/items?itemName=quicktype.quicktype)
using System.Text.Json.Serialization;

namespace planner_exandimport_wasm.JSON
{
    public partial class TaskDetailResponse
    {
        [JsonPropertyName("@odata.context")]
        public string? OdataContext { get; set; }

        [JsonPropertyName("@odata.etag")]
        public string? OdataEtag { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("previewType")]
        public string? PreviewType { get; set; }

        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("references")]
        public Dictionary<string, Reference>? References { get; set; }

        [JsonPropertyName("checklist")]
        public Dictionary<string, Checklist>? Checklist { get; set; }

        public void Sanitize()
        {
            this.OdataEtag = null;
            this.OdataContext = null;
            this.Id = null;

            if (References != null)
                foreach (var reference in References.Values)
                    reference.Sanitize();

            if (Checklist != null)
                foreach (var checklist in Checklist.Values)
                    checklist.Sanitize();
        }
    }

    public partial class Checklist
    {
        [JsonPropertyName("@odata.type")]
        public string? OdataType { get; set; }

        [JsonPropertyName("isChecked")]
        public bool IsChecked { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("orderHint")]
        public string? OrderHint { get; set; }

        [JsonPropertyName("lastModifiedDateTime")]
        public DateTimeOffset? LastModifiedDateTime { get; set; }

        [JsonPropertyName("lastModifiedBy")]
        public LastModifiedBy? LastModifiedBy { get; set; }

        public void Sanitize()
        {
            this.LastModifiedBy = null;
            this.LastModifiedDateTime = null;
        }
    }

    public partial class LastModifiedBy
    {
        [JsonPropertyName("user")]
        public User? User { get; set; }
    }

    public partial class User
    {
        [JsonPropertyName("displayName")]
        public object? DisplayName { get; set; }

        [JsonPropertyName("id")]
        public string? Id { get; set; }
    }

    public partial class Reference
    {
        [JsonPropertyName("@odata.type")]
        public string? OdataType { get; set; }

        [JsonPropertyName("alias")]
        public string? Alias { get; set; }

        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("previewPriority")]
        public string? PreviewPriority { get; set; }

        [JsonPropertyName("lastModifiedDateTime")]
        public DateTimeOffset? LastModifiedDateTime { get; set; }

        [JsonPropertyName("lastModifiedBy")]
        public LastModifiedBy? LastModifiedBy { get; set; }

        public void Sanitize()
        {
            this.LastModifiedBy = null;
            this.LastModifiedDateTime = null;
        }
    }
}
