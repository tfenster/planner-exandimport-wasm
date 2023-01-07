// mostly generated through the excellent "Paste JSON as Code" extension (https://marketplace.visualstudio.com/items?itemName=quicktype.quicktype)
using System.Text.Json.Serialization;

namespace planner_exandimport_wasm.shared.JSON
{

    public partial class TaskResponse
    {
        [JsonPropertyName("@odata.context")]
        public string? OdataContext { get; set; }

        [JsonPropertyName("@odata.count")]
        public long OdataCount { get; set; }

        [JsonPropertyName("@odata.nextLink")]
        public string? OdataNextLink { get; set; }

        [JsonPropertyName("value")]
        public PlannerTask[]? Tasks { get; set; }
    }

    public partial class PlannerTask
    {
        [JsonPropertyName("@odata.etag")]
        public string? OdataEtag { get; set; }

        [JsonPropertyName("planId")]
        public string? PlanId { get; set; }

        [JsonPropertyName("bucketId")]
        public string? BucketId { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("orderHint")]
        public string? OrderHint { get; set; }

        [JsonPropertyName("assigneePriority")]
        public string? AssigneePriority { get; set; }

        [JsonPropertyName("percentComplete")]
        public long PercentComplete { get; set; }

        [JsonPropertyName("startDateTime")]
        public DateTimeOffset? StartDateTime { get; set; }

        [JsonPropertyName("createdDateTime")]
        public DateTimeOffset? CreatedDateTime { get; set; }

        [JsonPropertyName("dueDateTime")]
        public DateTimeOffset? DueDateTime { get; set; }

        [JsonPropertyName("hasDescription")]
        public bool? HasDescription { get; set; }

        [JsonPropertyName("previewType")]
        public string? PreviewType { get; set; }

        [JsonPropertyName("completedDateTime")]
        public DateTimeOffset? CompletedDateTime { get; set; }

        [JsonPropertyName("completedBy")]
        public EdBy? CompletedBy { get; set; }

        [JsonPropertyName("referenceCount")]
        public long? ReferenceCount { get; set; }

        [JsonPropertyName("checklistItemCount")]
        public long? ChecklistItemCount { get; set; }

        [JsonPropertyName("activeChecklistItemCount")]
        public long? ActiveChecklistItemCount { get; set; }

        [JsonPropertyName("conversationThreadId")]
        public string? ConversationThreadId { get; set; }

        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("createdBy")]
        public EdBy? CreatedBy { get; set; }

        [JsonPropertyName("appliedCategories")]
        public AppliedCategories? AppliedCategories { get; set; }

        [JsonPropertyName("assignments")]
        public Dictionary<string, Assignment>? Assignments { get; set; }

        public TaskDetailResponse? TaskDetail { get; set; }

        public void Sanitize()
        {
            //this.TaskDetail = null;
            this.AppliedCategories = null;
            this.CreatedBy = null;
            this.Id = null;
            this.ConversationThreadId = null;
            this.ActiveChecklistItemCount = null;
            this.ChecklistItemCount = null;
            this.ReferenceCount = null;
            this.CompletedBy = null;
            this.CompletedDateTime = null;
            this.PreviewType = null;
            this.HasDescription = null;
            this.CreatedDateTime = null;
            this.AssigneePriority = null;
            this.OdataEtag = null;

            if (Assignments != null)
                foreach (var assignment in Assignments.Values)
                    assignment.Sanitize();

            if (this.TaskDetail != null)
                this.TaskDetail.Sanitize();
        }
    }

    public partial class AppliedCategories
    {
    }

    public partial class Assignment
    {
        [JsonPropertyName("@odata.type")]
        public string? OdataType { get; set; }

        [JsonPropertyName("assignedDateTime")]
        public DateTimeOffset? AssignedDateTime { get; set; }

        [JsonPropertyName("orderHint")]
        public string? OrderHint { get; set; }

        [JsonPropertyName("assignedBy")]
        public EdBy? AssignedBy { get; set; }

        public void Sanitize()
        {
            this.AssignedBy = null;
            this.AssignedDateTime = null;
        }
    }

    public partial class EdBy
    {
        [JsonPropertyName("user")]
        public User? User { get; set; }
    }
}
