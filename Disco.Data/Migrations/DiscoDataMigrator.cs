using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity.Migrations;
using System.Data.Entity.Migrations.Infrastructure;
using Disco.Data.Repository;

namespace Disco.Data.Migrations
{
    public static class DiscoDataMigrator
    {

        private static DbMigrator GetMigrator()
        {
            var migContext = new DbMigrationsConfiguration<DiscoDataContext>();
            migContext.MigrationsAssembly = typeof(DiscoDataMigrator).Assembly;
            migContext.MigrationsNamespace = "Disco.Data.Migrations";

            return new DbMigrator(migContext);
        }

        public static void MigrateLatest(bool Seed)
        {
            var migrator = GetMigrator();

            migrator.Update();

            if (Seed)
                SeedDatabase();
        }
        public static void ForceMigration(string TargetMigration, bool Seed)
        {
            var migrator = GetMigrator();

            migrator.Update(TargetMigration);

            if (Seed)
                SeedDatabase();
        }

        public static string MigrationScript(string CurrentMigration, string TargetMigration)
        {
            var migrator = GetMigrator();
            var scriptor = new MigratorScriptingDecorator(migrator);
            return scriptor.ScriptUpdate(CurrentMigration, TargetMigration);
        }

        public static void SeedDatabase()
        {
            // Seed/Update Database
            using (DiscoDataContext database = new DiscoDataContext())
            {
                database.SeedDatabase();
                try
                {
                    database.SaveChanges();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                    throw;
                }
            }
        }

        public static MigrationStatus Status()
        {
            var migrator = GetMigrator();

            var appliedMigrations = migrator.GetDatabaseMigrations().ToList();
            var pendingMigrations = migrator.GetPendingMigrations().ToList();
            var currentMigration = appliedMigrations.LastOrDefault();

            return new MigrationStatus()
            {
                CurrentMigration = currentMigration,
                AppliedMigrations = appliedMigrations,
                PendingMigrations = pendingMigrations,
                AllMigrations = appliedMigrations.Union(pendingMigrations)
            };
        }

        public class MigrationStatus
        {
            public string CurrentMigration { get; internal set; }
            public IEnumerable<string> AppliedMigrations { get; internal set; }
            public IEnumerable<string> PendingMigrations { get; internal set; }
            public IEnumerable<string> AllMigrations { get; internal set; }

            internal MigrationStatus()
            {
                // Private Constructor
            }
        }
    }
}
