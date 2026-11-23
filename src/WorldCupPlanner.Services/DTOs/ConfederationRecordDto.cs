using WorldCupPlanner.Domain;

namespace WorldCupPlanner.Services.DTOs;

// Teams cuenta los equipos de la confederacion que ya jugaron. Las victorias, empates y derrotas
// son por presentacion: un partido entre dos equipos de la misma confederacion suma dos.
public sealed record ConfederationRecordDto(
    Confederation Confederation,
    int Teams,
    int Wins,
    int Draws,
    int Losses,
    double PointsPerMatch);
