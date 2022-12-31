// mostly generated through the excellent "Paste JSON as Code" extension (https://marketplace.visualstudio.com/items?itemName=quicktype.quicktype)
using System.Text.Json.Serialization;

namespace planner_exandimport_wasm.JSON
{

    public partial class DuplicationAdjustments
    {
        [JsonPropertyName("assignmentReplacements")]
        public AssignmentReplacement[]? AssignmentReplacements { get; set; }

        [JsonPropertyName("dateAdjustment")]
        public DateAdjustment? DateAdjustment { get; set; }
    }

    public partial class AssignmentReplacement
    {
        [JsonPropertyName("originalAssignment")]
        public string? OriginalAssignment { get; set; }

        [JsonPropertyName("replacementAssignment")]
        public string? ReplacementAssignment { get; set; }
    }

    public partial class DateAdjustment
    {
        [JsonPropertyName("originalReferenceDate")]
        public DateTimeOffset OriginalReferenceDate { get; set; }

        [JsonPropertyName("adjustedReferenceDate")]
        public DateTimeOffset AdjustedReferenceDate { get; set; }

        [JsonPropertyName("replaceWithTodayDate")]
        public DateTimeOffset ReplaceWithTodayDate { get; set; }
    }
}