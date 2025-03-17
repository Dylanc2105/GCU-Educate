namespace GuidanceTracker.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Courses",
                c => new
                    {
                        CourseId = c.Int(nullable: false, identity: true),
                        CourseName = c.String(),
                        CourseDescription = c.String(),
                    })
                .PrimaryKey(t => t.CourseId);
            
            CreateTable(
                "dbo.Modules",
                c => new
                    {
                        ModuleId = c.Int(nullable: false, identity: true),
                        ModuleName = c.String(),
                        ModuleDescription = c.String(),
                        CourseId = c.Int(nullable: false),
                        LecturerId = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.ModuleId)
                .ForeignKey("dbo.Courses", t => t.CourseId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.LecturerId)
                .Index(t => t.CourseId)
                .Index(t => t.LecturerId);
            
            CreateTable(
                "dbo.AspNetUsers",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        FirstName = c.String(),
                        LastName = c.String(),
                        Street = c.String(),
                        City = c.String(),
                        Postcode = c.String(),
                        RegistredAt = c.DateTime(nullable: false),
                        Email = c.String(maxLength: 256),
                        EmailConfirmed = c.Boolean(nullable: false),
                        PasswordHash = c.String(),
                        SecurityStamp = c.String(),
                        PhoneNumber = c.String(),
                        PhoneNumberConfirmed = c.Boolean(nullable: false),
                        TwoFactorEnabled = c.Boolean(nullable: false),
                        LockoutEndDateUtc = c.DateTime(),
                        LockoutEnabled = c.Boolean(nullable: false),
                        AccessFailedCount = c.Int(nullable: false),
                        UserName = c.String(nullable: false, maxLength: 256),
                        GuidanceTeacherId = c.String(maxLength: 128),
                        CourseId = c.Int(),
                        Discriminator = c.String(nullable: false, maxLength: 128),
                        Module_ModuleId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Courses", t => t.CourseId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.GuidanceTeacherId)
                .ForeignKey("dbo.Modules", t => t.Module_ModuleId)
                .Index(t => t.UserName, unique: true, name: "UserNameIndex")
                .Index(t => t.GuidanceTeacherId)
                .Index(t => t.CourseId)
                .Index(t => t.Module_ModuleId);
            
            CreateTable(
                "dbo.AspNetUserClaims",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        ClaimType = c.String(),
                        ClaimValue = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUserLogins",
                c => new
                    {
                        LoginProvider = c.String(nullable: false, maxLength: 128),
                        ProviderKey = c.String(nullable: false, maxLength: 128),
                        UserId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.LoginProvider, t.ProviderKey, t.UserId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUserRoles",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128),
                        RoleId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.UserId, t.RoleId })
                .ForeignKey("dbo.AspNetRoles", t => t.RoleId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.RoleId);
            
            CreateTable(
                "dbo.Tickets",
                c => new
                    {
                        TicketId = c.Int(nullable: false, identity: true),
                        TicketTitle = c.String(),
                        TicketDescription = c.String(),
                        TicketStatus = c.String(),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(nullable: false),
                        LecturerId = c.String(maxLength: 128),
                        StudentId = c.String(maxLength: 128),
                        GuidanceTeacherId = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.TicketId)
                .ForeignKey("dbo.AspNetUsers", t => t.StudentId)
                .ForeignKey("dbo.AspNetUsers", t => t.GuidanceTeacherId)
                .ForeignKey("dbo.AspNetUsers", t => t.LecturerId)
                .Index(t => t.LecturerId)
                .Index(t => t.StudentId)
                .Index(t => t.GuidanceTeacherId);
            
            CreateTable(
                "dbo.Sessions",
                c => new
                    {
                        SessionId = c.Int(nullable: false, identity: true),
                        SessionDate = c.DateTime(nullable: false),
                        SessionStatus = c.String(),
                        SessionNotes = c.String(),
                        StudentId = c.String(maxLength: 128),
                        GuidanceTeacherId = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.SessionId)
                .ForeignKey("dbo.AspNetUsers", t => t.GuidanceTeacherId)
                .ForeignKey("dbo.AspNetUsers", t => t.StudentId)
                .Index(t => t.StudentId)
                .Index(t => t.GuidanceTeacherId);
            
            CreateTable(
                "dbo.AspNetRoles",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true, name: "RoleNameIndex");
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AspNetUserRoles", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserLogins", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserClaims", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserRoles", "RoleId", "dbo.AspNetRoles");
            DropForeignKey("dbo.AspNetUsers", "Module_ModuleId", "dbo.Modules");
            DropForeignKey("dbo.Tickets", "LecturerId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Tickets", "GuidanceTeacherId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Tickets", "StudentId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Sessions", "StudentId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUsers", "GuidanceTeacherId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUsers", "CourseId", "dbo.Courses");
            DropForeignKey("dbo.Sessions", "GuidanceTeacherId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Modules", "LecturerId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Modules", "CourseId", "dbo.Courses");
            DropIndex("dbo.AspNetRoles", "RoleNameIndex");
            DropIndex("dbo.Sessions", new[] { "GuidanceTeacherId" });
            DropIndex("dbo.Sessions", new[] { "StudentId" });
            DropIndex("dbo.Tickets", new[] { "GuidanceTeacherId" });
            DropIndex("dbo.Tickets", new[] { "StudentId" });
            DropIndex("dbo.Tickets", new[] { "LecturerId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "RoleId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "UserId" });
            DropIndex("dbo.AspNetUserLogins", new[] { "UserId" });
            DropIndex("dbo.AspNetUserClaims", new[] { "UserId" });
            DropIndex("dbo.AspNetUsers", new[] { "Module_ModuleId" });
            DropIndex("dbo.AspNetUsers", new[] { "CourseId" });
            DropIndex("dbo.AspNetUsers", new[] { "GuidanceTeacherId" });
            DropIndex("dbo.AspNetUsers", "UserNameIndex");
            DropIndex("dbo.Modules", new[] { "LecturerId" });
            DropIndex("dbo.Modules", new[] { "CourseId" });
            DropTable("dbo.AspNetRoles");
            DropTable("dbo.Sessions");
            DropTable("dbo.Tickets");
            DropTable("dbo.AspNetUserRoles");
            DropTable("dbo.AspNetUserLogins");
            DropTable("dbo.AspNetUserClaims");
            DropTable("dbo.AspNetUsers");
            DropTable("dbo.Modules");
            DropTable("dbo.Courses");
        }
    }
}
