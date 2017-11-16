namespace TimekeeperDAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Revert : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.Filterables", new[] { "Name" });
        }
        
        public override void Down()
        {
            CreateIndex("dbo.Filterables", "Name", unique: true);
        }
    }
}
