using System.Text;
using System.Text.Json;
using Fermyon.Spin.Sdk;
using planner_exandimport_wasm.shared.JSON;
using planner_exandimport_wasm.backend.JSON;
using Microsoft.Extensions.Logging;
using planner_exandimport_wasm.shared;

namespace planner_exandimport_wasm
{
    // connects to the MS Graph API (https://graph.microsoft.com) and ex- and imports groups, plans, tasks, details etc.
    public class Planner : IPlanner
    {
        // URLs and settings for the Graph connection
        private const string GRAPH_ENDPOINT = "https://graph.microsoft.com";
        private const string PLANNER_SUB = "/v1.0/planner/";
        private const string GROUPS_SUB = "/v1.0/groups/";
        private const string USERS_SUB = "/v1.0/users/";
        private const string RESOURCE_ID = GRAPH_ENDPOINT;
        public static string CLIENT_ID = "";
        public static string TENANT = "";
        private static Dictionary<string, string> users = new Dictionary<string, string>();
        private static Dictionary<string, GraphUser> graphUsers = new Dictionary<string, GraphUser>();
        private string token = "";

        public Planner(string token)
        {
            this.token = token;
        }

        // export a plan and optionally output it as json
        /*public Plan[]? Export(bool output = true, bool allowMultiSelect = false, bool retrieveTaskDetail = true)
        {
            Plan[] plans = SelectPlan(allowMultiSelect);
            if (!allowMultiSelect && plans.Length > 1)
            {
                Console.WriteLine("You can only select 1 plan in this case");
                return null;
            }

            var httpClient = PreparePlannerClient();
            foreach (Plan plan in plans)
            {
                // get all buckets, tasks and task details
                var buckets = GraphResponse<BucketResponse>.Get("plans/" + plan.Id + "/buckets", httpClient)?.Buckets;
                var tasks = GraphResponse<TaskResponse>.Get("plans/" + plan.Id + "/tasks", httpClient)?.Tasks;

                if (retrieveTaskDetail && tasks != null)
                    foreach (var task in tasks)
                        task.TaskDetail = GraphResponse<TaskDetailResponse>.Get("tasks/" + task.Id + "/details", httpClient);

                // put tasks in buckets so that the plan object has all data hierarchically
                if (buckets != null && tasks != null)
                    foreach (var bucket in buckets)
                        bucket.Tasks = tasks.Where(t => t.BucketId == bucket.Id).ToArray();

                plan.Buckets = buckets;
            }

            if (output)
            {
                foreach (Plan plan in plans)
                    Console.WriteLine(JsonSerializer.Serialize(plan));
            }

            return plans;
        }

        internal void DuplicateBucket(Bucket bucket, string newBucketName)
        {
            bucket.Name = newBucketName;
            if (bucket.PlanId != null)
            {
                var httpClient = PreparePlannerClient();
                CreateBucket(bucket.PlanId, bucket, true, httpClient, null);
            }

        }*/

        // export a plan and import everything into a new plan
        /*public void Import(bool addAssignments)
        {
            Console.WriteLine("Step 1: Select the plan to export");
            var exportedPlan = Export(false)?.FirstOrDefault();
            if (exportedPlan == null || exportedPlan.Buckets == null)
                return;

            Console.WriteLine("Step 2: Select the plan in which you want to import");
            var targetPlan = SelectPlan(false).FirstOrDefault();
            if (targetPlan == null)
                return;

            var httpClient = PreparePlannerClient();
            // buckets and tasks are always added at the beginning, therefore reversing the order when importing, otherwise e.g. the
            // last exported bucket would become the first bucket in the imported plan
            exportedPlan.Buckets = exportedPlan.Buckets.Reverse().ToArray();

            // create buckets and tasks and then set details for the created tasks (can't be done in one step)
            foreach (Bucket bucket in exportedPlan.Buckets)
                if (targetPlan.Id != null)
                    CreateBucket(targetPlan.Id, bucket, addAssignments, httpClient, null);

            Console.WriteLine("Import is done");
        }*/

        private void CreateBucket(string targetPlanId, Bucket bucket, bool addAssignments, HttpRequest httpClient, DuplicationAdjustments? duplicationAdjustments)
        {
            if (bucket.Tasks == null)
                return;
            bucket.PlanId = targetPlanId;

            // reset all order hints as the exported values don't work
            bucket.OrderHint = " !";
            bucket.Sanitize();
            var bucketCopy = JsonSerializer.Deserialize<Bucket>(JsonSerializer.Serialize(bucket));
            bucketCopy!.Tasks = null;
            var newBucket = GraphResponse<Bucket>.Post("buckets", httpClient, bucketCopy);
            if (newBucket == null)
                return;

            // if we are too quick the created bucket is not available yet, make sure it is there
            Thread.Sleep(5 * 1000);
            var verifyNewBucket = GraphResponse<Bucket>.Get("buckets/" + newBucket.Id, httpClient);

            bucket.Tasks = bucket.Tasks.Reverse().ToArray();
            foreach (PlannerTask task in bucket.Tasks)
            {
                task.PlanId = targetPlanId;
                task.BucketId = newBucket.Id;
                task.OrderHint = " !";

                // assignments contain the users assigned to a task
                if (addAssignments && task.Assignments != null)
                {
                    foreach (Assignment assignment in task.Assignments.Values)
                        assignment.OrderHint = " !";

                    // change assignments if requested
                    if (duplicationAdjustments?.AssignmentReplacements != null)
                    {
                        var newAssignments = new Dictionary<string, Assignment>();
                        foreach (var assignmentKey in task.Assignments.Keys)
                        {
                            var matchingAssignmentReplacement = duplicationAdjustments.AssignmentReplacements.Where(ar => ar.OriginalAssignment == assignmentKey && !task.Assignments.ContainsKey(ar.ReplacementAssignment!)).FirstOrDefault();
                            if (matchingAssignmentReplacement != null)
                            {
                                newAssignments.Add(matchingAssignmentReplacement.ReplacementAssignment!, task.Assignments.GetValueOrDefault(assignmentKey)!);
                            }
                            else
                            {
                                newAssignments.Add(assignmentKey, task.Assignments.GetValueOrDefault(assignmentKey)!);
                            }
                        }
                        task.Assignments = newAssignments;
                    }
                }
                else
                    task.Assignments = new Dictionary<string, Assignment>();

                // change dates if requested
                if (duplicationAdjustments?.DateAdjustment != null)
                {
                    var difference = duplicationAdjustments.DateAdjustment.AdjustedReferenceDate - duplicationAdjustments.DateAdjustment.OriginalReferenceDate;
                    if (task.StartDateTime != null)
                        if (task.StartDateTime == duplicationAdjustments.DateAdjustment.ReplaceWithTodayDate)
                            task.StartDateTime = DateTimeOffset.Now;
                        else
                            task.StartDateTime = task.StartDateTime + difference;
                    if (task.DueDateTime != null)
                        if (task.DueDateTime == duplicationAdjustments.DateAdjustment.ReplaceWithTodayDate)
                            task.DueDateTime = DateTimeOffset.Now;
                        else
                            task.DueDateTime = task.DueDateTime + difference;
                }

                var newTask = GraphResponse<PlannerTask>.Post("tasks", httpClient, task);
                // remember new task id for next loop
                task.Id = newTask!.Id;
            }

            // if we are too quick the created tasks are not available yet
            Thread.Sleep(5 * 1000);

            foreach (PlannerTask task in bucket.Tasks)
            {
                var newTaskDetailsResponse = GraphResponse<TaskDetailResponse>.Get("tasks/" + task.Id + "/details", httpClient);
                if (newTaskDetailsResponse != null && task.TaskDetail != null)
                {
                    if (task.TaskDetail.Checklist != null)
                        foreach (var checklist in task.TaskDetail.Checklist.Values)
                            checklist.OrderHint = " !";
                    if (task.TaskDetail.References != null)
                        foreach (var reference in task.TaskDetail.References.Values)
                            // same as order hint
                            reference.PreviewPriority = " !";

                    var updatedTaskDetailsResponse = GraphResponse<TaskDetailResponse>.Patch("tasks/" + task.Id + "/details", httpClient, task.TaskDetail, newTaskDetailsResponse.OdataEtag!);
                }
            }
        }

        /*public static void ExportToCSV()
        {
            // export the plan
            Console.WriteLine("Select the plan(s) to export");
            var exportedPlans = Export(false, true, false);

            // convert the plan to CSV
            StringWriter csvString = new StringWriter();
            csvString.WriteLine("Plan;Bucket;Task;Zugewiesen an;Fällig am;Erledigt am;Erledigt von;Erstellt am;Erstellt von");
            foreach (Plan exportedPlan in exportedPlans)
            {
                foreach (var bucket in exportedPlan.Buckets)
                {
                    foreach (var task in bucket.Tasks)
                    {
                        var completedAt = "";
                        if (task.CompletedDateTime != null)
                        {
                            completedAt = task.CompletedDateTime.ToString();
                        }
                        var completedBy = GetUserForEdBy(task.CompletedBy) ?? "--";

                        var dueAt = "";
                        if (task.DueDateTime != null)
                        {
                            dueAt = task.DueDateTime.ToString();
                        }
                        var createdBy = GetUserForEdBy(task.CreatedBy) ?? "--";

                        var assignedTo = GetAssigned(task.Assignments);

                        if (task.CompletedDateTime == null) { }
                        csvString.WriteLine($"{exportedPlan.Title};{bucket.Name};{task.Title};{assignedTo};{dueAt};{completedAt};{completedBy};{task.CreatedDateTime};{createdBy}");
                    }
                }
            }

            var filename = "ExportedPlans.csv";
            if (exportedPlans.Length == 1) filename = exportedPlans[0].Title + ".csv";
            File.WriteAllText(filename, csvString.ToString());
        }*/

        /*private Bucket? SelectBucket()
        {
            var exportedPlan = Export(false)?.FirstOrDefault();
            if (exportedPlan == null || exportedPlan.Buckets == null)
                return null;

            for (int i = 0; i < exportedPlan.Buckets.Length; i++)
                Console.WriteLine("(" + i + ") " + exportedPlan.Buckets[i].Name);

            string selectedBucketS = ""; // FIXME Program.GetInput("Which bucket do you want to use: ");

            int selectedBucket = -1;
            if (int.TryParse(selectedBucketS, out selectedBucket))
            {
                return exportedPlan.Buckets[selectedBucket];
            }
            throw new Exception("Please select a bucket");
        }*/

        public Group[]? GetGroups(string? groupSearch = null)
        {
            var httpClient = PrepareGroupsClient();

            var searchString = "?$filter=groupTypes/any(c:c+eq+'Unified')";
            if (!string.IsNullOrEmpty(groupSearch))
                searchString += $" and startswith(displayName, '{groupSearch}')";
            var groupsResult = GraphResponse<GroupResponse>.Get(searchString, httpClient);
            if (groupsResult == null || groupsResult.Groups == null)
                return null;
            else
                return groupsResult.Groups;
        }

        public Plan[]? GetPlans(string? groupId)
        {
            if (string.IsNullOrEmpty(groupId))
                return null;

            var httpRequest = PrepareGroupsClient();
            var plansResult = GraphResponse<PlanResponse>.Get($"{groupId}/planner/plans", httpRequest);
            if (plansResult == null || plansResult.Plans == null)
                return null;

            return plansResult.Plans;
        }

        public Plan? GetPlanDetails(string? groupId, string? planId)
        {
            if (string.IsNullOrEmpty(groupId) || string.IsNullOrEmpty(planId))
                return null;

            var httpRequestPlan = PrepareGroupsClient();
            var plan = GraphResponse<Plan>.Get($"{groupId}/planner/plans/{planId}", httpRequestPlan);
            Handler._logger.LogDebug($"plan: {JsonSerializer.Serialize(plan)}");
            if (plan == null)
                return null;

            var httpRequestPlanner = PreparePlannerClient();
            // get all buckets, tasks and task details
            var buckets = GraphResponse<BucketResponse>.Get($"plans/{planId}/buckets", httpRequestPlanner)?.Buckets;
            Handler._logger.LogDebug($"buckets before: {JsonSerializer.Serialize(buckets)}");
            var tasks = GraphResponse<TaskResponse>.Get($"plans/{planId}/tasks", httpRequestPlanner)?.Tasks;
            Handler._logger.LogDebug($"tasks before: {JsonSerializer.Serialize(tasks)}");
            if (tasks != null)
                foreach (var task in tasks)
                {
                    Handler._logger.LogDebug($"task id: {task.Id}");
                    task.TaskDetail = GraphResponse<TaskDetailResponse>.Get($"tasks/{task.Id}/details", httpRequestPlanner);
                }
            Handler._logger.LogDebug($"tasks after: {JsonSerializer.Serialize(tasks)}");

            // put tasks in buckets so that the plan object has all data hierarchically
            if (buckets != null && tasks != null)
                foreach (var bucket in buckets)
                    bucket.Tasks = tasks.Where(t => t.BucketId == bucket.Id).ToArray();
            Handler._logger.LogDebug($"buckets after: {JsonSerializer.Serialize(buckets)}");

            plan.Buckets = buckets;

            return plan;
        }

        public Plan? DuplicatePlan(string? sourceGroupId, string? sourcePlanId, string? targetGroupId, string? targetPlanId, DuplicationAdjustments? duplicationAdjustments)
        {
            if (string.IsNullOrEmpty(sourceGroupId) || string.IsNullOrEmpty(sourcePlanId) || string.IsNullOrEmpty(targetGroupId) || string.IsNullOrEmpty(targetPlanId))
                return null;

            var sourcePlanDetails = GetPlanDetails(sourceGroupId, sourcePlanId);
            if (sourcePlanDetails == null)
                return null;
            sourcePlanDetails.Sanitize(this);

            var httpClient = PreparePlannerClient();
            // buckets and tasks are always added at the beginning, therefore reversing the order when importing, otherwise e.g. the
            // last exported bucket would become the first bucket in the imported plan
            if (sourcePlanDetails.Buckets != null)
            {
                sourcePlanDetails.Buckets = sourcePlanDetails.Buckets.Reverse().ToArray();

                // create buckets and tasks and then set details for the created tasks (can't be done in one step)
                foreach (Bucket bucket in sourcePlanDetails.Buckets)
                    CreateBucket(targetPlanId, bucket, true, httpClient, duplicationAdjustments);
            }

            return GetPlanDetails(targetGroupId, targetPlanId);
        }

        public GraphUser? GetGraphUser(string? userIdOrEmail)
        {
            if (string.IsNullOrEmpty(userIdOrEmail))
                return null;

            GraphUser? graphUser = null;
            if (graphUsers.TryGetValue(userIdOrEmail, out graphUser))
                return graphUser;

            var httpRequest = PrepareUsersClient();
            graphUser = GraphResponse<GraphUser>.Get($"{userIdOrEmail}", httpRequest);
            if (graphUser != null)
                graphUsers.Add(userIdOrEmail, graphUser);

            return graphUser;
        }

        // allows the user to search for a group, select the right one and then select the right plan
        private Plan[] SelectPlan(bool allowMultiSelect)
        {
            var httpClient = PrepareGroupsClient();
            bool foundGroup = false;
            while (!foundGroup)
            {
                string groupSearch = ""; // FIXME Program.GetInput("Please enter the start of the name of the group containing your plan: ");
                var groupsResult = GraphResponse<GroupResponse>.Get("?$filter=groupTypes/any(c:c+eq+'Unified') and startswith(displayName, '" + groupSearch + "')", httpClient);
                if (groupsResult == null)
                    return new Plan[0];
                var groups = groupsResult.Groups;
                if (groups == null || groups.Length == 0)
                {
                    Console.WriteLine("Found no matching group");
                }
                else
                {
                    foundGroup = true;

                    Console.WriteLine("Select group:");
                    for (int i = 0; i < groups.Length; i++)
                    {
                        Console.WriteLine("(" + i + ") " + groups[i].DisplayName);
                    }

                    string selectedGroupS = ""; // FIXME Program.GetInput("Which group do you want to use: ");

                    int selectedGroup = -1;
                    if (int.TryParse(selectedGroupS, out selectedGroup))
                    {
                        var plansResult = GraphResponse<PlanResponse>.Get(groups[selectedGroup].Id + "/planner/plans", httpClient);
                        if (plansResult == null || plansResult.Plans == null)
                            return new Plan[0];

                        var plans = plansResult.Plans;

                        Console.WriteLine("Select plan:");
                        for (int i = 0; i < plans.Length; i++)
                        {
                            Console.WriteLine("(" + i + ") " + plans[i].Title);
                        }
                        if (allowMultiSelect)
                            Console.WriteLine("(" + plans.Length + ") All plans");

                        string selectedPlanS = ""; // FIXME Program.GetInput("Which plan do you want to use: ");
                        int selectedPlan = -1;
                        if (int.TryParse(selectedPlanS, out selectedPlan))
                        {
                            if (selectedPlan == plans.Length)
                                return plans;
                            else
                                return new Plan[] { plans[selectedPlan] };
                        }
                    }
                }
            }
            throw new Exception("Please select a plan");
        }

        private HttpRequest PreparePlannerClient()
        {
            return PrepareClient(PLANNER_SUB);
        }

        private HttpRequest PrepareGroupsClient()
        {
            return PrepareClient(GROUPS_SUB);
        }

        private HttpRequest PrepareUsersClient()
        {
            return PrepareClient(USERS_SUB);
        }

        private HttpRequest PrepareClient(string sub)
        {
            var outboundReq = new HttpRequest
            {
                Url = $"{GRAPH_ENDPOINT}{sub}",
                Headers = new Dictionary<string, string>
                {
                    { "Authorization", $"{token}" },
                }
            };
            return outboundReq;
        }

        /*private string GetAssigned(Dictionary<string, Assignment> assignments)
        {
            if (assignments.Values.Count > 0)
            {
                var sb = new StringBuilder();
                var delim = "";
                List<string> added = new List<string>();
                foreach (string assignment in assignments.Keys)
                {
                    var user = GetUserForId(assignment) ?? "--";
                    if (!added.Contains(user))
                    {
                        sb.Append(delim);
                        sb.Append(user);
                        delim = ", ";
                        added.Add(user);
                    }
                }
                return sb.ToString();
            }
            else
            {
                return "--";
            }
        }*/

        /*private string? GetUserForEdBy(EdBy edBy)
        {
            if (edBy == null || edBy.User == null || edBy.User.Id == null)
                return null;
            var id = edBy.User.Id;
            if (users.ContainsKey(id))
                return users[id];
            else
            {
                var user = GetUserForId(id);
                return GetUserForId(id);
            }
        }*/

        /*private string? GetUserForId(string id)
        {
            if (id == null)
                return null;
            if (users.ContainsKey(id))
                return users[id];
            else
            {
                var httpClient = PrepareUsersClient();
                try
                {
                    var user = GraphResponse<GraphUser>.Get(id, httpClient);
                    if (user == null || user.DisplayName == null)
                        return null;
                    users.Add(id, user.DisplayName);
                    return user.DisplayName;
                }
                catch
                {
                    return null;
                }
            }
        }*/
    }
}