using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using ServiceBase.OData.Core.Entities;
using ServiceBase.OData.Core.Interfaces;
using ServiceBase.OData.Infrastructure.Data;
using ServiceBase.OData.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace ServiceBase.OData.Infrastructure.Repositories
{
    public class HeroesRepository : IHeroesRepository
    {
        private readonly HeroesContext _context;

        public HeroesRepository(HeroesContext context)
        {
            _context = context;
        }

        public IQueryable<HeroEntity> AllHeroes()
        {
            return _context
                .Heroes
                .AsNoTracking()
                .ProjectTo<HeroEntity>()
                .AsQueryable();
        }

        public async Task CreateHero(HeroEntity heroEntity)
        {
            var heroDataModel = Mapper.Map<HeroDataModel>(heroEntity);

            await _context.Heroes.AddAsync(heroDataModel).ConfigureAwait(false);
            await _context.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task Delete(Guid id)
        {
            var heroToDelete = await _context.Heroes.FirstOrDefaultAsync(hero => hero.Id == id).ConfigureAwait(false);

            _context.Heroes.Remove(heroToDelete);

            await _context.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task<HeroEntity> Find(Guid id)
        {
            var heroDataModel = await _context
                                    .Heroes
                                    .AsNoTracking()
                                    .FirstOrDefaultAsync(hero => hero.Id == id)
                                    .ConfigureAwait(false);

            return Mapper.Map<HeroEntity>(heroDataModel);
        }

        public async Task<HeroEntity> UpdateHero(HeroEntity heroEntity)
        {
            var heroDataModel = await _context
                                    .Heroes
                                    .FirstOrDefaultAsync(hero => hero.Id == heroEntity.Id)
                                    .ConfigureAwait(false);

            heroDataModel.Name = heroEntity.Name;

            await _context.SaveChangesAsync();

            return Mapper.Map<HeroEntity>(heroDataModel);
        }
    }
}
