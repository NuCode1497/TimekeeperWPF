namespace TimekeeperDAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Notes",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    DateTime = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                    Text = c.String(nullable: false, maxLength: 150, unicode: false),
                    Type = c.String(nullable: false, maxLength: 50, unicode: false),
                    RowVersion = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "timestamp"),
                })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "dbo.TaskTypes",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    Type = c.String(nullable: false, maxLength: 50, unicode: false),
                    RowVersion = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "timestamp"),
                })
                .PrimaryKey(t => t.Id);

        }
        
        public override void Down()
        {
            DropTable("dbo.TaskTypes");
            DropTable("dbo.Notes");
        }
    }
}
