namespace TimekeeperDAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CheckIns : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.CheckIns",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Text = c.String(nullable: false, maxLength: 10),
                        DateTime = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        RowVersion = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "timestamp"),
                        TimeTask_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.TimeTasks", t => t.TimeTask_Id)
                .Index(t => t.TimeTask_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.CheckIns", "TimeTask_Id", "dbo.TimeTasks");
            DropIndex("dbo.CheckIns", new[] { "TimeTask_Id" });
            DropTable("dbo.CheckIns");
        }
    }
}
