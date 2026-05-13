namespace Services;

public static class AuditActions
{
    public const string CreateUser = "CREATE_USER";

    public const string UpdateUser = "UPDATE_USER";

    public const string DeleteUser = "DELETE_USER";

    public const string AssignUserRole = "ASSIGN_USER_ROLE";

    public const string CreateMatch = "CREATE_MATCH";

    public const string UpdateMatch = "UPDATE_MATCH";

    public const string DeleteMatch = "DELETE_MATCH";

    public const string SimulateMatch = "SIMULATE_MATCH";

    public const string PublishMatchResult = "PUBLISH_MATCH_RESULT";

    public const string CreateTeam = "CREATE_TEAM";

    public const string UpdateTeam = "UPDATE_TEAM";

    public const string DeleteTeam = "DELETE_TEAM";

    public const string CreateStadium = "CREATE_STADIUM";

    public const string UpdateStadium = "UPDATE_STADIUM";

    public const string DeleteStadium = "DELETE_STADIUM";

    public const string CreateGroup = "CREATE_GROUP";

    public const string DeleteGroup = "DELETE_GROUP";

    public const string CreateJourney = "CREATE_JOURNEY";

    public const string DeleteJourney = "DELETE_JOURNEY";

    public const string GenerateFixture = "GENERATE_FIXTURE";
    
    public const string RemoveUserRoles = "REMOVE_ROLES";

    public const string RegenerateFixture = "REGENERATE_FIXTURE";

    public const string ClearFixture = "CLEAR_FIXTURE";

    public const string GenerateKnockout = "GENERATE_KNOCKOUT";

    public const string SimulateRound = "SIMULATE_ROUND";

    public const string SimulateGroupStageAndGenerateKnockout = "SIMULATE_GROUP_STAGE_AND_GENERATE_KNOCKOUT";
}
