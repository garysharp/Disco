namespace Disco.Data.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using Disco.Data.Repository;

    internal sealed class Configuration : DbMigrationsConfiguration<DiscoDataContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(DiscoDataContext context)
        {
            context.SeedDatabase();
        }
    }
}
