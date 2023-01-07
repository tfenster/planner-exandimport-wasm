// mostly generated through the excellent "Paste JSON as Code" extension (https://marketplace.visualstudio.com/items?itemName=quicktype.quicktype)
using System.Text.Json.Serialization;

namespace planner_exandimport_wasm.JSON
{
    public partial class UserResponse
    {
        [JsonPropertyName("@odata.context")]
        public Uri? OdataContext { get; set; }

        [JsonPropertyName("businessPhones")]
        public object[]? BusinessPhones { get; set; }

        [JsonPropertyName("displayName")]
        public string? DisplayName { get; set; }

        [JsonPropertyName("givenName")]
        public object? GivenName { get; set; }

        [JsonPropertyName("jobTitle")]
        public object? JobTitle { get; set; }

        [JsonPropertyName("mail")]
        public string? Mail { get; set; }

        [JsonPropertyName("mobilePhone")]
        public object? MobilePhone { get; set; }

        [JsonPropertyName("officeLocation")]
        public object? OfficeLocation { get; set; }

        [JsonPropertyName("preferredLanguage")]
        public object? PreferredLanguage { get; set; }

        [JsonPropertyName("surname")]
        public object? Surname { get; set; }

        [JsonPropertyName("userPrincipalName")]
        public string? UserPrincipalName { get; set; }

        [JsonPropertyName("id")]
        public Guid Id { get; set; }
    }
}