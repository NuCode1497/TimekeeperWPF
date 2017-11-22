namespace TimekeeperDAL.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<TimekeeperDAL.EF.TimeKeeperContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(TimekeeperDAL.EF.TimeKeeperContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //
            //    context.People.AddOrUpdate(
            //      p => p.FullName,
            //      new Person { FullName = "Andrew Peters" },
            //      new Person { FullName = "Brice Lambson" },
            //      new Person { FullName = "Rowan Miller" }
            //    );
            //

            context.TaskTypes.AddOrUpdate(
                t => t.Name,
                new EF.TaskType { Name = "Work" },
                new EF.TaskType { Name = "Play" },
                new EF.TaskType { Name = "Eat" },
                new EF.TaskType { Name = "Sleep" },
                new EF.TaskType { Name = "Chore" },
                new EF.TaskType { Name = "Note" }
            );

            context.Resources.AddOrUpdate(
                r => r.Name,
                new EF.Resource { Name = "Second" },
                new EF.Resource { Name = "Minute" },
                new EF.Resource { Name = "Hour" },
                new EF.Resource { Name = "Day" },
                new EF.Resource { Name = "Week" },
                new EF.Resource { Name = "Month" },
                new EF.Resource { Name = "Year" },
                new EF.Resource { Name = "Dollar" },
                new EF.Resource { Name = "Try" },
                new EF.Resource { Name = "Gallon" },
                new EF.Resource { Name = "Pound" }
            );

            context.Labels.AddOrUpdate(
                l => l.Name,
                new EF.Label { Name = "Holiday" },
                new EF.Label { Name = "Night" },
                new EF.Label { Name = "Sunny" },
                new EF.Label { Name = "Rainy" },
                new EF.Label { Name = "Green Eyes" },
                new EF.Label { Name = "Classroom" },
                new EF.Label { Name = "Car" },
                new EF.Label { Name = "City" }
            );
        }
    }
}
