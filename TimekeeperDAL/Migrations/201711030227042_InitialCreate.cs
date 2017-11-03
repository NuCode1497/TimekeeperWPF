namespace TimekeeperDAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Allocations",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Amount = c.Long(nullable: false),
                        RowVersion = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "timestamp"),
                        Resource_Id = c.Int(nullable: false),
                        TimeTask_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Filterables", t => t.Resource_Id, cascadeDelete: true)
                .ForeignKey("dbo.Filterables", t => t.TimeTask_Id)
                .Index(t => t.Resource_Id)
                .Index(t => t.TimeTask_Id);
            
            CreateTable(
                "dbo.Filterables",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 50),
                        RowVersion = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "timestamp"),
                        Start = c.DateTime(precision: 7, storeType: "datetime2"),
                        End = c.DateTime(precision: 7, storeType: "datetime2"),
                        Description = c.String(maxLength: 150),
                        Priority = c.Int(),
                        RaiseOnReschedule = c.Boolean(),
                        AsksForReschedule = c.Boolean(),
                        CanReschedule = c.Boolean(),
                        AsksForCheckin = c.Boolean(),
                        CanBePushed = c.Boolean(),
                        CanInflate = c.Boolean(),
                        CanDeflate = c.Boolean(),
                        CanFill = c.Boolean(),
                        CanBeEarly = c.Boolean(),
                        CanBeLate = c.Boolean(),
                        Dimension = c.Int(),
                        PowerLevel = c.Int(),
                        DateTime = c.DateTime(precision: 7, storeType: "datetime2"),
                        Text = c.String(maxLength: 150),
                        Discriminator = c.String(nullable: false, maxLength: 128),
                        TaskType_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Filterables", t => t.TaskType_Id)
                .Index(t => t.TaskType_Id);
            
            CreateTable(
                "dbo.Filters",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Include = c.Boolean(nullable: false),
                        RowVersion = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "timestamp"),
                        Filterable_Id = c.Int(nullable: false),
                        TimeTask_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Filterables", t => t.Filterable_Id, cascadeDelete: true)
                .ForeignKey("dbo.Filterables", t => t.TimeTask_Id)
                .Index(t => t.Filterable_Id)
                .Index(t => t.TimeTask_Id);
            
            CreateTable(
                "dbo.TimePatternClauses",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TimeProperty = c.String(nullable: false),
                        Equivalency = c.String(nullable: false),
                        TimePropertyValue = c.String(nullable: false),
                        RowVersion = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "timestamp"),
                        TimePattern_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Filterables", t => t.TimePattern_Id, cascadeDelete: true)
                .Index(t => t.TimePattern_Id);
            
            CreateTable(
                "dbo.LabeledEntityLabels",
                c => new
                    {
                        LabeledEntity_Id = c.Int(nullable: false),
                        Label_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.LabeledEntity_Id, t.Label_Id })
                .ForeignKey("dbo.Filterables", t => t.LabeledEntity_Id)
                .ForeignKey("dbo.Filterables", t => t.Label_Id)
                .Index(t => t.LabeledEntity_Id)
                .Index(t => t.Label_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Filters", "TimeTask_Id", "dbo.Filterables");
            DropForeignKey("dbo.Filters", "Filterable_Id", "dbo.Filterables");
            DropForeignKey("dbo.Filterables", "TaskType_Id", "dbo.Filterables");
            DropForeignKey("dbo.TimePatternClauses", "TimePattern_Id", "dbo.Filterables");
            DropForeignKey("dbo.LabeledEntityLabels", "Label_Id", "dbo.Filterables");
            DropForeignKey("dbo.LabeledEntityLabels", "LabeledEntity_Id", "dbo.Filterables");
            DropForeignKey("dbo.Allocations", "TimeTask_Id", "dbo.Filterables");
            DropForeignKey("dbo.Allocations", "Resource_Id", "dbo.Filterables");
            DropIndex("dbo.LabeledEntityLabels", new[] { "Label_Id" });
            DropIndex("dbo.LabeledEntityLabels", new[] { "LabeledEntity_Id" });
            DropIndex("dbo.TimePatternClauses", new[] { "TimePattern_Id" });
            DropIndex("dbo.Filters", new[] { "TimeTask_Id" });
            DropIndex("dbo.Filters", new[] { "Filterable_Id" });
            DropIndex("dbo.Filterables", new[] { "TaskType_Id" });
            DropIndex("dbo.Allocations", new[] { "TimeTask_Id" });
            DropIndex("dbo.Allocations", new[] { "Resource_Id" });
            DropTable("dbo.LabeledEntityLabels");
            DropTable("dbo.TimePatternClauses");
            DropTable("dbo.Filters");
            DropTable("dbo.Filterables");
            DropTable("dbo.Allocations");
        }
    }
}
