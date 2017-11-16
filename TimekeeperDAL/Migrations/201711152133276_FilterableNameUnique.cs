namespace TimekeeperDAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FilterableNameUnique : DbMigration
    {
        public override void Up()
        {
            CreateIndex("dbo.Filterables", "Name", unique: true);
        }
        
        public override void Down()
        {
            DropIndex("dbo.Filterables", new[] { "Name" });
        }
    }
}
