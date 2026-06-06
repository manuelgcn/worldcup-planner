using DataAccess.Interfaces;

namespace DataAccess.Repositories;

public class UnitOfWorkRepository : IUnitOfWork
{
    public void Commit()
    {
        // No-op por diseno: cada repositorio EF Core persiste con su propio SaveChanges (autosave por
        // operacion sobre un contexto efimero). No queda almacenamiento en memoria que confirmar aqui.
        // Una unidad de trabajo real (un DbContext compartido por transaccion) es trabajo de una fase posterior.
    }
}
