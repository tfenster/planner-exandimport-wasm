using planner_exandimport_wasm.shared.JSON;

namespace planner_exandimport_wasm.shared;

public interface IPlanner
{
    Task<Group[]?> GetGroups(string? groupSearch = null);
    Task<Plan[]?> GetPlans(string? groupId);
    Task<Plan?> GetPlanDetails(string? groupId, string? planId);
    Task<Plan?> DuplicatePlan(string? sourceGroupId, string? sourcePlanId, string? targetGroupId, string? targetPlanId, DuplicationAdjustments? duplicationAdjustments);
    Task<string?> DuplicateBucket(string? targetPlanId, BucketWithDuplicationAdjustments bucketWithDuplicationAdjustments);
    Task<GraphUser?> GetGraphUser(string? userIdOrEmail);
}