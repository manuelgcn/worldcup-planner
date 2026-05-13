using ProjectCopa;

namespace DataAccess.Interfaces
{
    public interface IGroupMatchRepository
    {
        Match? GetById(int id);
        IReadOnlyCollection<Match> GetByGroup(string groupName);
        IReadOnlyCollection<Match> GetAll();
        void Add(Match match);
        void Update(Match match);
        void Delete(Match match);
        bool ExistsById(int id);
    }
}