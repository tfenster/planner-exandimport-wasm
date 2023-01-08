using planner_exandimport_wasm.shared.JSON;

namespace planner_exandimport_wasm.shared;

public interface IPlanner
{
    Group[]? GetGroups(string? groupSearch = null);
    Plan[]? GetPlans(string? groupId);
    Plan? GetPlanDetails(string? groupId, string? planId);
    Plan? DuplicatePlan(string? sourceGroupId, string? sourcePlanId, string? targetGroupId, string? targetPlanId, DuplicationAdjustments? duplicationAdjustments);
    GraphUser? GetGraphUser(string? userIdOrEmail);
}