using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceBase.OData.Web.Models
{
    /// <summary>
    /// A hero
    /// A hero may or may not have super powers (i.e. not all heroes are superheroes)
    /// </summary>
    public class Hero
    {
        /// <summary>
        /// Hero primary identifier
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The name of the hero
        /// </summary>
        public string Name { get; set; }
    }
}
