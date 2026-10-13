namespace WorldCupPlanner.Services;

public static class AuditActions
{
    public const string CreateUser = "CREATE_USER";

    public const string UpdateUser = "UPDATE_USER";

    public const string DeleteUser = "DELETE_USER";

    public const string AssignUserRole = "ASSIGN_USER_ROLE";

    public const string UpgradePasswordHash = "UPGRADE_PASSWORD_HASH";

    public const string CreateMatch = "CREATE_MATCH";

    public const string UpdateMatch = "UPDATE_MATCH";

    public const string DeleteMatch = "DELETE_MATCH";

    public const string SimulateMatch = "SIMULATE_MATCH";

    // Simulacion individual ejecutada con exito (registra motor y semilla);
    // las operaciones agregadas conservan sus acciones (SIMULATE_ROUND, etc.).
    public const string SimulationExecuted = "SIMULATION_EXECUTED";

    public const string RankingUpdated = "RANKING_UPDATED";

    public const string PublishMatchResult = "PUBLISH_MATCH_RESULT";

    public const string IncidentAdded = "INCIDENT_ADDED";

    public const string IncidentRemoved = "INCIDENT_REMOVED";

    public const string CreateTeam = "CREATE_TEAM";

    public const string UpdateTeam = "UPDATE_TEAM";

    public const string DeleteTeam = "DELETE_TEAM";

    public const string AssignTeamFlags = "ASSIGN_TEAM_FLAGS";

    public const string ImportTeams = "IMPORT_TEAMS";

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

    public const string DeleteFixture = "DELETE_FIXTURE";

    public const string GenerateKnockout = "GENERATE_KNOCKOUT";

    public const string SimulateRound = "SIMULATE_ROUND";

    public const string AdvanceRound = "ADVANCE_ROUND";

    public const string SimulateGroupStage = "SIMULATE_GROUP_STAGE";

    public const string SimulateGroupStageAndGenerateKnockout = "SIMULATE_GROUP_STAGE_AND_GENERATE_KNOCKOUT";

    public const string NotificationRead = "NOTIFICATION_READ";

    public const string NotificationReadAll = "NOTIFICATION_READ_ALL";

    public const string ExportFixtureCsv = "EXPORT_FIXTURE_CSV";

    public const string ExportFixtureXlsx = "EXPORT_FIXTURE_XLSX";

    public const string ExportAuditCsv = "EXPORT_AUDIT_CSV";

    public const string ExportAuditXlsx = "EXPORT_AUDIT_XLSX";
}
