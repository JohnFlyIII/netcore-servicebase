using ServiceBase.OData.Web.Models;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNetCore.Mvc;

namespace ServiceBase.OData.Web.Configuration.ModelConfigurations
{
    /// <summary>
    /// OData model configuration for Heroes
    /// </summary>
    public class HeroModelConfiguration : IModelConfiguration
    {
        /// <summary>
        /// Applies model configurations using the provided builder for the specified API version.
        /// </summary>
        /// <param name="builder">The <see cref="ODataModelBuilder">builder</see> used to apply configurations.</param>
        /// <param name="apiVersion">The <see cref="ApiVersion">API version</see> associated with the <paramref name="builder"/>.</param>
        public void Apply(ODataModelBuilder builder, ApiVersion apiVersion)
        {
            var hero = builder.EntitySet<Hero>("Heroes").EntityType.HasKey(h => h.Id);
            ConfigureNamesLongerThanFunction(hero);

            var hivemindAttackAction = hero.Collection.Action("HivemindAttack");

            hivemindAttackAction.Parameter<string>("master");
            hivemindAttackAction.Parameter<string>("secret");
            hivemindAttackAction.Returns<int>();
        }

        private static void ConfigureNamesLongerThanFunction(EntityTypeConfiguration<Hero> hero)
        {
            var namesLongerThanFunction = hero.Collection.Function("NamesLongerThan");

            namesLongerThanFunction.Parameter<int>("Length");
            namesLongerThanFunction.Returns<int>();
        }
    }
}
