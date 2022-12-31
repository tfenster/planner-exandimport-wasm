// mostly generated through the excellent "Paste JSON as Code" extension (https://marketplace.visualstudio.com/items?itemName=quicktype.quicktype)
using System.Text.Json.Serialization;

namespace planner_exandimport_wasm.JSON
{
    public partial class BucketResponse
    {
        [JsonPropertyName("@odata.context")]
        public string? OdataContext { get; set; }

        [JsonPropertyName("@odata.count")]
        public long OdataCount { get; set; }

        [JsonPropertyName("@odata.nextLink")]
        public string? OdataNextLink { get; set; }

        [JsonPropertyName("value")]
        public Bucket[]? Buckets { get; set; }
    }

    public partial class Bucket
    {
        [JsonPropertyName("@odata.etag")]
        public string? OdataEtag { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("planId")]
        public string? PlanId { get; set; }

        [JsonPropertyName("orderHint")]
        public string? OrderHint { get; set; }

        [JsonPropertyName("id")]
        public string? Id { get; set; }

        public PlannerTask[]? Tasks { get; set; }

        public void Sanitize()
        {
            this.OdataEtag = null;
            this.Id = null;
            if (Tasks != null)
                foreach (var task in Tasks)
                    task.Sanitize();
        }
    }
}