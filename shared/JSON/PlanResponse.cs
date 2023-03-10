// mostly generated through the excellent "Paste JSON as Code" extension (https://marketplace.visualstudio.com/items?itemName=quicktype.quicktype)
using System.Text.Json.Serialization;

namespace planner_exandimport_wasm.shared.JSON
{
    public partial class PlanResponse
    {
        [JsonPropertyName("@odata.context")]
        public string? OdataContext { get; set; }

        [JsonPropertyName("@odata.count")]
        public long OdataCount { get; set; }

        [JsonPropertyName("@odata.nextLink")]
        public string? OdataNextLink { get; set; }

        [JsonPropertyName("value")]
        public Plan[]? Plans { get; set; }
    }

    public partial class Plan : IComparable<Plan>
    {
        [JsonPropertyName("@odata.etag")]
        public string? OdataEtag { get; set; }

        [JsonPropertyName("createdDateTime")]
        public DateTimeOffset CreatedDateTime { get; set; }

        [JsonPropertyName("owner")]
        public string? Owner { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("createdBy")]
        public CreatedBy? CreatedBy { get; set; }

        [JsonPropertyName("createdByGraphUser")]
        public GraphUser? CreatedByGraphUser { get; set; }

        public Bucket[]? Buckets { get; set; }

        public void Sanitize()
        {
            if (Buckets != null)
                foreach (var bucket in Buckets)
                    bucket.Sanitize();
        }

        public async Task ExpandUsers(IPlanner planner)
        {
            if (CreatedBy?.User != null)
                CreatedByGraphUser = await planner.GetGraphUser(CreatedBy.User.Id);
        }

        public int CompareTo(Plan? other)
        {
            if (this.Title == null || other == null || other.Title == null)
                return 0;
            return this.Title.CompareTo(other.Title);
        }
    }

    public partial class CreatedBy
    {
        [JsonPropertyName("user")]
        public Application? User { get; set; }

        [JsonPropertyName("application")]
        public Application? Application { get; set; }
    }

    public partial class Application
    {
        [JsonPropertyName("displayName")]
        public string? DisplayName { get; set; }

        [JsonPropertyName("id")]
        public string? Id { get; set; }
    }
}
