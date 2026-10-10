using OfficeDays.Services;

namespace OfficeDays.Features.Planning.GetPlanning.Contracts;

public sealed record GetPlanningResponse(DateOnly Today, int UpcomingPlannedDays, IReadOnlyList<PlanningDay> Days);
