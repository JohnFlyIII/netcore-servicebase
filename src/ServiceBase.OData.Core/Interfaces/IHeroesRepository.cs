using ServiceBase.OData.Core.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceBase.OData.Core.Interfaces
{
    public interface IHeroesRepository
    {
        IQueryable<HeroEntity> AllHeroes();

        Task CreateHero(HeroEntity heroEntity);

        Task<HeroEntity> Find(Guid id);

        Task<HeroEntity> UpdateHero(HeroEntity heroEntity);

        Task Delete(Guid id);
    }
}
