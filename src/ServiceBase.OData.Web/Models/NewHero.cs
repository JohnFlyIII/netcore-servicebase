using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceBase.OData.Web.Models
{
    /// <summary>
    /// A hero to be saved
    /// A hero may or may not have super powers (i.e. not all heroes are superheroes)
    /// </summary>
    public class NewHero
    {      
        /// <summary>
        /// The name of the hero
        /// </summary>
        public string Name { get; set; }
    }
}
