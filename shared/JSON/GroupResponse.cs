// mostly generated through the excellent "Paste JSON as Code" extension (https://marketplace.visualstudio.com/items?itemName=quicktype.quicktype)
using System.Text.Json.Serialization;

namespace planner_exandimport_wasm.shared.JSON
{
    public partial class GroupResponse
    {
        [JsonPropertyName("@odata.context")]
        public string? OdataContext { get; set; }

        [JsonPropertyName("value")]
        public Group[]? Groups { get; set; }
    }

    public partial class Group : IComparable<Group>
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("deletedDateTime")]
        public object? DeletedDateTime { get; set; }

        [JsonPropertyName("classification")]
        public object? Classification { get; set; }

        [JsonPropertyName("createdDateTime")]
        public DateTimeOffset CreatedDateTime { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("displayName")]
        public string? DisplayName { get; set; }

        [JsonPropertyName("groupTypes")]
        public string[]? GroupTypes { get; set; }

        [JsonPropertyName("mail")]
        public string? Mail { get; set; }

        [JsonPropertyName("mailEnabled")]
        public bool MailEnabled { get; set; }

        [JsonPropertyName("mailNickname")]
        public string? MailNickname { get; set; }

        [JsonPropertyName("membershipRule")]
        public object? MembershipRule { get; set; }

        [JsonPropertyName("membershipRuleProcessingState")]
        public object? MembershipRuleProcessingState { get; set; }

        [JsonPropertyName("onPremisesLastSyncDateTime")]
        public object? OnPremisesLastSyncDateTime { get; set; }

        [JsonPropertyName("onPremisesSecurityIdentifier")]
        public object? OnPremisesSecurityIdentifier { get; set; }

        [JsonPropertyName("onPremisesSyncEnabled")]
        public object? OnPremisesSyncEnabled { get; set; }

        [JsonPropertyName("preferredDataLocation")]
        public object? PreferredDataLocation { get; set; }

        [JsonPropertyName("preferredLanguage")]
        public object? PreferredLanguage { get; set; }

        [JsonPropertyName("proxyAddresses")]
        public string[]? ProxyAddresses { get; set; }

        [JsonPropertyName("renewedDateTime")]
        public DateTimeOffset RenewedDateTime { get; set; }

        [JsonPropertyName("resourceBehaviorOptions")]
        public string[]? ResourceBehaviorOptions { get; set; }

        [JsonPropertyName("resourceProvisioningOptions")]
        public object[]? ResourceProvisioningOptions { get; set; }

        [JsonPropertyName("securityEnabled")]
        public bool SecurityEnabled { get; set; }

        [JsonPropertyName("theme")]
        public object? Theme { get; set; }

        [JsonPropertyName("visibility")]
        public string? Visibility { get; set; }

        [JsonPropertyName("onPremisesProvisioningErrors")]
        public object[]? OnPremisesProvisioningErrors { get; set; }

        [JsonPropertyName("plans")]
        public Plan[]? Plans { get; set; }

        public int CompareTo(Group? other)
        {
            if (this.DisplayName == null || other == null || other.DisplayName == null)
                return 0;
            return this.DisplayName.CompareTo(other.DisplayName);
        }
    }
}
