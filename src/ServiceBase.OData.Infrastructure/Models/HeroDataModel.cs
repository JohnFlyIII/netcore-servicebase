using System;

namespace ServiceBase.OData.Infrastructure.Models
{
    public class HeroDataModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string DbModelOnlyField { get; set; }
    }
}
