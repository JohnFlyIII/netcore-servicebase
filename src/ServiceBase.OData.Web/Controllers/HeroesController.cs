using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using ServiceBase.OData.Core.Entities;
using ServiceBase.OData.Core.Interfaces;
using ServiceBase.OData.Web.Models;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using static Microsoft.AspNetCore.Http.StatusCodes;

namespace ServiceBase.OData.Web.Controllers
{
    [ApiVersion("1.0")]
    [ODataRoutePrefix("Heroes")]
    public class HeroesController : ODataController
    {
        private readonly IHeroesRepository _heroesRepository;
        private readonly ILogger<HeroesController> _logger;

        public HeroesController(ILogger<HeroesController> logger, IHeroesRepository heroesRepository)
        {
            _logger = logger;
            _heroesRepository = heroesRepository;
        }

        /// <summary>
        /// (OData Queryable) Creates a new hero
        /// </summary>
        /// <param name="newHero"></param>
        [EnableQuery]
        [ODataRoute]
        [Produces("application/json")]
        [ProducesResponseType(typeof(Hero), Status201Created)]
        public async Task<IActionResult> Post([FromBody]NewHero newHero)
        {
            //TODO : Rev
            try
            {
                var heroEntity = new HeroEntity
                {
                    Id = Guid.NewGuid(),
                    Name = newHero.Name
                };

                await _heroesRepository.CreateHero(heroEntity).ConfigureAwait(false);

                var hero = Mapper.Map<Hero>(heroEntity);

                return Created(hero);
            }
            catch (Exception ex)
            {
                if (ex is ArgumentNullException)
                {
                    return BadRequest();
                }

                _logger.LogError(ex, "Error creating hero.");
                return StatusCode(500);
            }
        }

        /// <summary>
        /// (OData Queryable) Retrieves a list of heroes
        /// </summary>
        [ODataRoute]
        [EnableQuery]
        [Produces("application/json")]
        [ProducesResponseType(typeof(Hero), Status200OK)]
        public async Task<ActionResult<IEnumerable<Hero>>> Get()
        {
            try
            {
                return await _heroesRepository.AllHeroes().ProjectTo<Hero>().ToListAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failure retrieving heroes.");
                return StatusCode(Status500InternalServerError);
            }
        }

        /// <summary>
        /// (OData Queryable) Retrieve a single hero
        /// </summary>
        /// <param name="key">The unique identifier for the hero</param>
        /// <returns></returns>
        [ODataRoute("({key})")]
        [EnableQuery]
        [Produces("application/json")]
        [ProducesResponseType(typeof(Hero), Status200OK)]
        [ProducesResponseType(Status404NotFound)]
        public async Task<ActionResult<Hero>> Get(Guid key)
        {
            try
            {
                var heroEntity = await _heroesRepository.Find(key).ConfigureAwait(false);

                if (heroEntity == null)
                {
                    return NotFound();
                }
                return Mapper.Map<Hero>(heroEntity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failure retrieving hero.");
                return StatusCode(Status500InternalServerError);
            }
        }

        /// <summary>
        /// (OData Queryable) Create or update a hero
        /// </summary>
        /// <param name="key">unique identifier for a hero</param>
        /// <param name="updatedHero">updated hero information</param>
        /// <returns></returns>
        [ODataRoute("({key})")]
        [EnableQuery]
        [Produces("application/json")]
        [ProducesResponseType(Status404NotFound)]
        [ProducesResponseType(Status500InternalServerError)]
        [ProducesResponseType(Status400BadRequest)]
        [ProducesResponseType(typeof(Hero), Status200OK)]
        public async Task<IActionResult> Patch(Guid key, [FromBody] NewHero updatedHero)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var heroEntity = await _heroesRepository.Find(key).ConfigureAwait(false);

                if (heroEntity == null)
                {
                    return NotFound();
                }
                else
                {
                    heroEntity.Name = updatedHero.Name;

                    await _heroesRepository.UpdateHero(heroEntity).ConfigureAwait(false);

                    var hero = Mapper.Map<Hero>(heroEntity);

                    return Ok(hero);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failure on hero.");
                return StatusCode(Status500InternalServerError);
            }
        }

        /// <summary>
        /// (OData Queryable) Create or replace a hero
        /// </summary>
        /// <param name="key">unique identifier for a hero</param>
        /// <param name="newOrUpdatedHero">hero to create or update</param>
        /// <returns></returns>
        [ODataRoute("({key})")]
        [EnableQuery]
        [Produces("application/json")]
        [ProducesResponseType(typeof(Hero), Status201Created)]
        [ProducesResponseType(typeof(Hero), Status200OK)]
        [ProducesResponseType(Status500InternalServerError)]
        public async Task<IActionResult> Put(Guid key, [FromBody] NewHero newOrUpdatedHero)
        {
            try
            {
                var heroEntity = await _heroesRepository.Find(key).ConfigureAwait(false);

                if (heroEntity == null)
                {
                    heroEntity = new HeroEntity
                    {
                        Id = key,
                        Name = newOrUpdatedHero.Name
                    };

                    await _heroesRepository.CreateHero(heroEntity).ConfigureAwait(false);

                    var hero = Mapper.Map<Hero>(heroEntity);

                    return Created(hero);
                }
                else
                {
                    heroEntity.Name = newOrUpdatedHero.Name;

                    await _heroesRepository.UpdateHero(heroEntity).ConfigureAwait(false);

                    var hero = Mapper.Map<Hero>(heroEntity);

                    return Ok(hero);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failure on hero.");
                return StatusCode(Status500InternalServerError);
            }
        }

        /// <summary>
        /// (OData Queryable) Delete a hero
        /// </summary>
        /// <param name="key">unique identifier for a hero</param>      
        /// <returns></returns>
        [ODataRoute("({key})")]
        [EnableQuery]
        [Produces("application/json")]
        [ProducesResponseType(Status500InternalServerError)]
        [ProducesResponseType(Status404NotFound)]
        [ProducesResponseType(Status204NoContent)]
        public async Task<IActionResult> Delete(Guid key)
        {
            try
            {
                var heroEntity = await _heroesRepository.Find(key).ConfigureAwait(false);

                if (heroEntity == null)
                {
                    return NotFound();
                }

                await _heroesRepository.Delete(key).ConfigureAwait(false);

                return StatusCode(Status204NoContent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failure on hero.");
                return StatusCode(Status500InternalServerError);
            }
        }

        /// <summary>
        /// Returns the number of heroes that have names longer than the specified length
        /// </summary>
        /// <param name="length">value</param>
        [HttpGet]
        [Produces("application/json")]
        [ProducesResponseType(typeof(int), Status200OK)]
        public IActionResult NamesLongerThan(int length)
        {
            return Ok(_heroesRepository.AllHeroes().Where(hero => hero.Name.Length > length).Count());
        }

        /// <summary>
        /// Attacks all heroes..... makes them mindwiped drones of a master psychic master
        /// </summary>
        /// <param name="parameters">Action parameters</param>
        /// <returns></returns>
        [HttpPost]
        [Produces("application/json")]
        [ProducesResponseType(typeof(int), Status200OK)]
        public async Task<IActionResult> HivemindAttack([FromBody]ODataActionParameters parameters)
        {
            var heroes = _heroesRepository.AllHeroes().ToList();

            foreach(var hero in heroes)
            {
                hero.Name = parameters["master"].ToString();
                await _heroesRepository.UpdateHero(hero).ConfigureAwait(false);
            }
            return Ok(heroes.Count);
        }
    }
}