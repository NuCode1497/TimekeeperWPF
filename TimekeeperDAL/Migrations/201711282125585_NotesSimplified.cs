namespace TimekeeperDAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class NotesSimplified : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Notes", "Id", "dbo.TypedLabeledEntities");
            DropIndex("dbo.Notes", new[] { "Id" });
            DropPrimaryKey("dbo.Notes");
            AddColumn("dbo.Notes", "RowVersion", c => c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "timestamp"));
            AlterColumn("dbo.Notes", "Id", c => c.Int(nullable: false, identity: true));
            AddPrimaryKey("dbo.Notes", "Id");
            DropColumn("dbo.Notes", "Dimension");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Notes", "Dimension", c => c.Int(nullable: false));
            DropPrimaryKey("dbo.Notes");
            AlterColumn("dbo.Notes", "Id", c => c.Int(nullable: false));
            DropColumn("dbo.Notes", "RowVersion");
            AddPrimaryKey("dbo.Notes", "Id");
            CreateIndex("dbo.Notes", "Id");
            AddForeignKey("dbo.Notes", "Id", "dbo.TypedLabeledEntities", "Id");
        }
    }
}
