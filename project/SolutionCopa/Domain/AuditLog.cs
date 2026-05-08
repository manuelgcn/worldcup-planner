using System;
using ProjectCopa.Exceptions;

namespace ProjectCopa
{
    public sealed class AuditLog
    {
        public DateTime Timestamp { get; }
        public string Action { get; }
        public string Details { get; }
        public int TournamentSettingsId { get; }
        public string? AffectedEntityType { get; }
        public int? AffectedEntityId { get; }

        private AuditLog(
            DateTime timestampUtc,
            string action,
            string details,
            int tournamentSettingsId,
            string? affectedEntityType = null,
            int? affectedEntityId = null)
        {
            Timestamp = timestampUtc;
            Action = action;
            Details = details;
            TournamentSettingsId = tournamentSettingsId;
            AffectedEntityType = affectedEntityType;
            AffectedEntityId = affectedEntityId;
        }

        private static void EnsureActionIsValid(string? action)
        {
            if (string.IsNullOrWhiteSpace(action))
            {
                throw new AuditLogException("Action is required for an AuditLog.");
            }
        }

        private static string NormalizeAction(string action)
        {
            return action.Trim().ToUpperInvariant();
        }

        // Factory method: enforces invariants and centralizes creation
        public static AuditLog Create(
            string action,
            string? details,
            int tournamentSettingsId,
            string? affectedEntityType = null,
            int? affectedEntityId = null)
        {
            EnsureActionIsValid(action);

            string cleanAction = NormalizeAction(action);
            string cleanDetails = details ?? string.Empty;
            DateTime timestampUtc = DateTime.UtcNow;

            return new AuditLog(timestampUtc, cleanAction, cleanDetails, tournamentSettingsId, affectedEntityType, affectedEntityId);
        }
    }
}
