using Services.DTOs;

namespace Services.Interfaces;

// Caso de uso de lectura del estado del fixture para la pagina /fixture: arma el snapshot
// (settings, equipos, grupos, partidos y rango de fechas) sin que la pagina conozca los
// repositorios de DataAccess.
public interface IFixtureViewService
{
    FixtureSnapshotDto GetSnapshot();
}
