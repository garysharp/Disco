namespace Disco.Data.Migrations
{
    using System.Data.Entity.Migrations;
    using Disco.Data.Repository;

    internal sealed class Configuration : DbMigrationsConfiguration<DiscoDataContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(DiscoDataContext Database)
        {
            Database.SeedDatabase();
        }
    }
}
