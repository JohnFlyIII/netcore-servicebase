using System;

namespace ServiceBase.OData.Core.Entities
{
    public class HeroEntity
    {
        public Guid Id { get; set; }
        public string Name { get; set; }

        public HeroEntity()
        {
            Id = Guid.NewGuid();
        }
    }
}
