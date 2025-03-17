namespace GuidanceTracker.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class fixedCascadeDelete : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Tickets", "GuidanceTeacherId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Tickets", "LecturerId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Tickets", "StudentId", "dbo.AspNetUsers");
            AlterColumn("dbo.Tickets", "TicketDescription", c => c.String());
            AddForeignKey("dbo.Tickets", "GuidanceTeacherId", "dbo.AspNetUsers", "Id");
            AddForeignKey("dbo.Tickets", "LecturerId", "dbo.AspNetUsers", "Id");
            AddForeignKey("dbo.Tickets", "StudentId", "dbo.AspNetUsers", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Tickets", "StudentId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Tickets", "LecturerId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Tickets", "GuidanceTeacherId", "dbo.AspNetUsers");
            AlterColumn("dbo.Tickets", "TicketDescription", c => c.String(nullable: false));
            AddForeignKey("dbo.Tickets", "StudentId", "dbo.AspNetUsers", "Id", cascadeDelete: true);
            AddForeignKey("dbo.Tickets", "LecturerId", "dbo.AspNetUsers", "Id", cascadeDelete: true);
            AddForeignKey("dbo.Tickets", "GuidanceTeacherId", "dbo.AspNetUsers", "Id", cascadeDelete: true);
        }
    }
}
