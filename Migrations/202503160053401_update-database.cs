namespace GuidanceTracker.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updatedatabase : DbMigration
    {
        public override void Up()
        {
           

            DropForeignKey("dbo.Sessions", "GuidanceTeacherId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Modules", "CourseId", "dbo.Courses");
            DropForeignKey("dbo.Modules", "LecturerId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUsers", "Module_ModuleId", "dbo.Modules");
            DropForeignKey("dbo.AspNetUsers", "CourseId", "dbo.Courses");
            DropForeignKey("dbo.Sessions", "StudentId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Tickets", "GuidanceTeacherId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Tickets", "LecturerId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Tickets", "StudentId", "dbo.AspNetUsers");
            DropIndex("dbo.Tickets", new[] { "LecturerId" });
            DropIndex("dbo.Tickets", new[] { "StudentId" });
            DropIndex("dbo.Tickets", new[] { "GuidanceTeacherId" });
            DropIndex("dbo.AspNetUsers", new[] { "CourseId" });
            DropIndex("dbo.AspNetUsers", new[] { "Module_ModuleId" });
            DropIndex("dbo.Sessions", new[] { "StudentId" });
            DropIndex("dbo.Sessions", new[] { "GuidanceTeacherId" });
            DropIndex("dbo.Modules", new[] { "CourseId" });
            DropIndex("dbo.Modules", new[] { "LecturerId" });
            CreateTable(
                "dbo.Appointments",
                c => new
                    {
                        AppointmentId = c.Int(nullable: false, identity: true),
                        AppointmentDate = c.DateTime(nullable: false),
                        AppointmentStatus = c.String(),
                        AppointmentNotes = c.String(),
                        GuidanceTeacherId = c.String(nullable: false, maxLength: 128),
                        StudentId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.AppointmentId)
                .ForeignKey("dbo.AspNetUsers", t => t.GuidanceTeacherId, cascadeDelete: false)
                .ForeignKey("dbo.AspNetUsers", t => t.StudentId, cascadeDelete: false)
                .Index(t => t.GuidanceTeacherId)
                .Index(t => t.StudentId);
            
            CreateTable(
                "dbo.Classes",
                c => new
                    {
                        ClassId = c.Int(nullable: false, identity: true),
                        ClassName = c.String(nullable: false, maxLength: 100),
                        MaxCapacity = c.Int(nullable: false),
                        GuidanceTeacherId = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.ClassId)
                .ForeignKey("dbo.AspNetUsers", t => t.GuidanceTeacherId)
                .Index(t => t.GuidanceTeacherId);
            
            CreateTable(
                "dbo.Enrollments",
                c => new
                    {
                        EnrollmentId = c.Int(nullable: false, identity: true),
                        EnrollmentDate = c.DateTime(nullable: false),
                        Status = c.Int(nullable: false),
                        CourseId = c.Int(nullable: false),
                        ClassId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.EnrollmentId)
                .ForeignKey("dbo.Classes", t => t.ClassId, cascadeDelete: true)
                .ForeignKey("dbo.Courses", t => t.CourseId, cascadeDelete: true)
                .Index(t => t.CourseId)
                .Index(t => t.ClassId);
            
            CreateTable(
                "dbo.Departments",
                c => new
                    {
                        DepartmentId = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false, maxLength: 100),
                    })
                .PrimaryKey(t => t.DepartmentId)
                .ForeignKey("dbo.AspNetUsers", t => t.DepartmentId)
                .Index(t => t.DepartmentId);
            
            CreateTable(
                "dbo.Feedbacks",
                c => new
                    {
                        FeedbackId = c.String(nullable: false, maxLength: 128),
                        DateOfPosting = c.DateTime(nullable: false),
                        SubmissionDate = c.DateTime(nullable: false),
                        Content = c.String(nullable: false),
                        StudentId = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.FeedbackId)
                .ForeignKey("dbo.AspNetUsers", t => t.FeedbackId)
                .Index(t => t.FeedbackId);
            
            CreateTable(
                "dbo.MessageBoards",
                c => new
                    {
                        MessageId = c.Int(nullable: false, identity: true),
                        PostDate = c.DateTime(nullable: false),
                        Content = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.MessageId);
            
            CreateTable(
                "dbo.Timetables",
                c => new
                    {
                        TimetableId = c.Int(nullable: false, identity: true),
                        EffectiveFrom = c.DateTime(nullable: false),
                        EffectiveTo = c.DateTime(nullable: false),
                        Schedule = c.String(),
                        ClassId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.TimetableId)
                .ForeignKey("dbo.Classes", t => t.ClassId, cascadeDelete: true)
                .Index(t => t.ClassId);
            
            CreateTable(
                "dbo.Units",
                c => new
                    {
                        UnitId = c.Int(nullable: false, identity: true),
                        UnitName = c.String(),
                        UnitDescription = c.String(),
                        ClassId = c.Int(nullable: false),
                        LecturerId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.UnitId)
                .ForeignKey("dbo.Classes", t => t.ClassId, cascadeDelete: false)
                .ForeignKey("dbo.AspNetUsers", t => t.LecturerId, cascadeDelete: false)
                .Index(t => t.ClassId)
                .Index(t => t.LecturerId);
            
            CreateTable(
                "dbo.ArchivedComments",
                c => new
                    {
                        ArchivedCommentId = c.Int(nullable: false, identity: true),
                        OriginalCommentId = c.Int(nullable: false),
                        CommentText = c.String(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                        ArchivedAt = c.DateTime(nullable: false),
                        ArchivedTicketId = c.Int(nullable: false),
                        UserId = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.ArchivedCommentId)
                .ForeignKey("dbo.ArchivedTickets", t => t.ArchivedTicketId, cascadeDelete: true)
                .Index(t => t.ArchivedTicketId);
            
            CreateTable(
                "dbo.ArchivedTickets",
                c => new
                    {
                        TicketId = c.Int(nullable: false),
                        ArchivedAt = c.DateTime(nullable: false),
                        ArchivedBy = c.String(),
                    })
                .PrimaryKey(t => t.TicketId)
                .ForeignKey("dbo.Tickets", t => t.TicketId)
                .Index(t => t.TicketId);
            
            CreateTable(
                "dbo.MessageBoardStudents",
                c => new
                    {
                        MessageBoard_MessageId = c.Int(nullable: false),
                        Student_Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.MessageBoard_MessageId, t.Student_Id })
                .ForeignKey("dbo.MessageBoards", t => t.MessageBoard_MessageId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.Student_Id, cascadeDelete: true)
                .Index(t => t.MessageBoard_MessageId)
                .Index(t => t.Student_Id);
            
            AddColumn("dbo.Comments", "ArchivedCommentId", c => c.Int());
            AddColumn("dbo.Tickets", "ArchivedTicketId", c => c.Int());
            AddColumn("dbo.AspNetUsers", "DepartmentId", c => c.Int());
            AddColumn("dbo.AspNetUsers", "ClassId", c => c.Int());
            AddColumn("dbo.AspNetUsers", "FeedbackId", c => c.String());
            AddColumn("dbo.Courses", "CourseReference", c => c.String(nullable: false));
            AddColumn("dbo.Courses", "ModeOfStudy", c => c.String(nullable: false));
            AddColumn("dbo.Courses", "DurationInWeeks", c => c.Int(nullable: false));
            AddColumn("dbo.Courses", "SCQFLevel", c => c.Int(nullable: false));
            AddColumn("dbo.Courses", "Site", c => c.String());
            AddColumn("dbo.Courses", "StartDate", c => c.DateTime(nullable: false));
            AddColumn("dbo.Courses", "EndDate", c => c.DateTime(nullable: false));
            AddColumn("dbo.Courses", "DepartmentId", c => c.String(nullable: false, maxLength: 128));
            AlterColumn("dbo.Tickets", "LecturerId", c => c.String(nullable: false, maxLength: 128));
            AlterColumn("dbo.Tickets", "StudentId", c => c.String(nullable: false, maxLength: 128));
            AlterColumn("dbo.Tickets", "GuidanceTeacherId", c => c.String(nullable: false, maxLength: 128));
            AlterColumn("dbo.Courses", "CourseName", c => c.String(nullable: false));
            CreateIndex("dbo.AspNetUsers", "ClassId");
            CreateIndex("dbo.Courses", "DepartmentId");
            CreateIndex("dbo.Tickets", "LecturerId");
            CreateIndex("dbo.Tickets", "GuidanceTeacherId");
            CreateIndex("dbo.Tickets", "StudentId");
            AddForeignKey("dbo.Courses", "DepartmentId", "dbo.Departments", "DepartmentId", cascadeDelete: true);
            AddForeignKey("dbo.AspNetUsers", "ClassId", "dbo.Classes", "ClassId", cascadeDelete: true);
            AddForeignKey("dbo.Tickets", "GuidanceTeacherId", "dbo.AspNetUsers", "Id", cascadeDelete: false);
            AddForeignKey("dbo.Tickets", "LecturerId", "dbo.AspNetUsers", "Id", cascadeDelete: false);
            AddForeignKey("dbo.Tickets", "StudentId", "dbo.AspNetUsers", "Id", cascadeDelete: false);
            DropColumn("dbo.AspNetUsers", "CourseId");
            DropColumn("dbo.AspNetUsers", "Module_ModuleId");
            DropColumn("dbo.Courses", "CourseDescription");
            DropTable("dbo.Sessions");
            DropTable("dbo.Modules");
        }
        
        public override void Down()
        {
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
                .PrimaryKey(t => t.ModuleId);
            
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
                .PrimaryKey(t => t.SessionId);
            
            AddColumn("dbo.Courses", "CourseDescription", c => c.String());
            AddColumn("dbo.AspNetUsers", "Module_ModuleId", c => c.Int());
            AddColumn("dbo.AspNetUsers", "CourseId", c => c.Int());
            DropForeignKey("dbo.Tickets", "StudentId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Tickets", "LecturerId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Tickets", "GuidanceTeacherId", "dbo.AspNetUsers");
            DropForeignKey("dbo.ArchivedTickets", "TicketId", "dbo.Tickets");
            DropForeignKey("dbo.ArchivedComments", "ArchivedTicketId", "dbo.ArchivedTickets");
            DropForeignKey("dbo.Units", "LecturerId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Units", "ClassId", "dbo.Classes");
            DropForeignKey("dbo.Timetables", "ClassId", "dbo.Classes");
            DropForeignKey("dbo.MessageBoardStudents", "Student_Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.MessageBoardStudents", "MessageBoard_MessageId", "dbo.MessageBoards");
            DropForeignKey("dbo.Feedbacks", "FeedbackId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUsers", "ClassId", "dbo.Classes");
            DropForeignKey("dbo.Appointments", "StudentId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Classes", "GuidanceTeacherId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Enrollments", "CourseId", "dbo.Courses");
            DropForeignKey("dbo.Departments", "DepartmentId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Courses", "DepartmentId", "dbo.Departments");
            DropForeignKey("dbo.Enrollments", "ClassId", "dbo.Classes");
            DropForeignKey("dbo.Appointments", "GuidanceTeacherId", "dbo.AspNetUsers");
            DropIndex("dbo.MessageBoardStudents", new[] { "Student_Id" });
            DropIndex("dbo.MessageBoardStudents", new[] { "MessageBoard_MessageId" });
            DropIndex("dbo.ArchivedTickets", new[] { "TicketId" });
            DropIndex("dbo.ArchivedComments", new[] { "ArchivedTicketId" });
            DropIndex("dbo.Tickets", new[] { "StudentId" });
            DropIndex("dbo.Tickets", new[] { "GuidanceTeacherId" });
            DropIndex("dbo.Tickets", new[] { "LecturerId" });
            DropIndex("dbo.Units", new[] { "LecturerId" });
            DropIndex("dbo.Units", new[] { "ClassId" });
            DropIndex("dbo.Timetables", new[] { "ClassId" });
            DropIndex("dbo.Feedbacks", new[] { "FeedbackId" });
            DropIndex("dbo.Departments", new[] { "DepartmentId" });
            DropIndex("dbo.Courses", new[] { "DepartmentId" });
            DropIndex("dbo.Enrollments", new[] { "ClassId" });
            DropIndex("dbo.Enrollments", new[] { "CourseId" });
            DropIndex("dbo.Classes", new[] { "GuidanceTeacherId" });
            DropIndex("dbo.AspNetUsers", new[] { "ClassId" });
            DropIndex("dbo.Appointments", new[] { "StudentId" });
            DropIndex("dbo.Appointments", new[] { "GuidanceTeacherId" });
            AlterColumn("dbo.Courses", "CourseName", c => c.String());
            AlterColumn("dbo.Tickets", "GuidanceTeacherId", c => c.String(maxLength: 128));
            AlterColumn("dbo.Tickets", "StudentId", c => c.String(maxLength: 128));
            AlterColumn("dbo.Tickets", "LecturerId", c => c.String(maxLength: 128));
            DropColumn("dbo.Courses", "DepartmentId");
            DropColumn("dbo.Courses", "EndDate");
            DropColumn("dbo.Courses", "StartDate");
            DropColumn("dbo.Courses", "Site");
            DropColumn("dbo.Courses", "SCQFLevel");
            DropColumn("dbo.Courses", "DurationInWeeks");
            DropColumn("dbo.Courses", "ModeOfStudy");
            DropColumn("dbo.Courses", "CourseReference");
            DropColumn("dbo.AspNetUsers", "FeedbackId");
            DropColumn("dbo.AspNetUsers", "ClassId");
            DropColumn("dbo.AspNetUsers", "DepartmentId");
            DropColumn("dbo.Tickets", "ArchivedTicketId");
            DropColumn("dbo.Comments", "ArchivedCommentId");
            DropTable("dbo.MessageBoardStudents");
            DropTable("dbo.ArchivedTickets");
            DropTable("dbo.ArchivedComments");
            DropTable("dbo.Units");
            DropTable("dbo.Timetables");
            DropTable("dbo.MessageBoards");
            DropTable("dbo.Feedbacks");
            DropTable("dbo.Departments");
            DropTable("dbo.Enrollments");
            DropTable("dbo.Classes");
            DropTable("dbo.Appointments");
            CreateIndex("dbo.Modules", "LecturerId");
            CreateIndex("dbo.Modules", "CourseId");
            CreateIndex("dbo.Sessions", "GuidanceTeacherId");
            CreateIndex("dbo.Sessions", "StudentId");
            CreateIndex("dbo.AspNetUsers", "Module_ModuleId");
            CreateIndex("dbo.AspNetUsers", "CourseId");
            CreateIndex("dbo.Tickets", "GuidanceTeacherId");
            CreateIndex("dbo.Tickets", "StudentId");
            CreateIndex("dbo.Tickets", "LecturerId");
            AddForeignKey("dbo.Tickets", "StudentId", "dbo.AspNetUsers", "Id");
            AddForeignKey("dbo.Tickets", "LecturerId", "dbo.AspNetUsers", "Id");
            AddForeignKey("dbo.Tickets", "GuidanceTeacherId", "dbo.AspNetUsers", "Id");
            AddForeignKey("dbo.Sessions", "StudentId", "dbo.AspNetUsers", "Id");
            AddForeignKey("dbo.AspNetUsers", "CourseId", "dbo.Courses", "CourseId", cascadeDelete: true);
            AddForeignKey("dbo.AspNetUsers", "Module_ModuleId", "dbo.Modules", "ModuleId");
            AddForeignKey("dbo.Modules", "LecturerId", "dbo.AspNetUsers", "Id");
            AddForeignKey("dbo.Modules", "CourseId", "dbo.Courses", "CourseId", cascadeDelete: true);
            AddForeignKey("dbo.Sessions", "GuidanceTeacherId", "dbo.AspNetUsers", "Id");
        }
    }
}
