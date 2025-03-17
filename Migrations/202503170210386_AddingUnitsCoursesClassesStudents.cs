namespace GuidanceTracker.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddingUnitsCoursesClassesStudents : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Units", "ClassId", "dbo.Classes");
            DropIndex("dbo.Units", new[] { "ClassId" });
            CreateTable(
                "dbo.UnitClasses",
                c => new
                    {
                        Unit_UnitId = c.Int(nullable: false),
                        Class_ClassId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.Unit_UnitId, t.Class_ClassId })
                .ForeignKey("dbo.Units", t => t.Unit_UnitId, cascadeDelete: true)
                .ForeignKey("dbo.Classes", t => t.Class_ClassId, cascadeDelete: true)
                .Index(t => t.Unit_UnitId)
                .Index(t => t.Class_ClassId);
            
            DropColumn("dbo.Units", "ClassId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Units", "ClassId", c => c.Int(nullable: false));
            DropForeignKey("dbo.UnitClasses", "Class_ClassId", "dbo.Classes");
            DropForeignKey("dbo.UnitClasses", "Unit_UnitId", "dbo.Units");
            DropIndex("dbo.UnitClasses", new[] { "Class_ClassId" });
            DropIndex("dbo.UnitClasses", new[] { "Unit_UnitId" });
            DropTable("dbo.UnitClasses");
            CreateIndex("dbo.Units", "ClassId");
            AddForeignKey("dbo.Units", "ClassId", "dbo.Classes", "ClassId", cascadeDelete: true);
        }
    }
}
