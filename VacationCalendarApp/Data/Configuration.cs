//using Microsoft.AspNetCore.Identity;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore.Metadata.Builders;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using VacationCalendarApp.Models;

//namespace VacationCalendarApp.Data
//{
//    public class ApplicationRoleConfiguration : IEntityTypeConfiguration<IdentityRole>
//    {
//        public void Configure(EntityTypeBuilder<IdentityRole> builder)
//        {
//            builder.ToTable("AspNetRoles");

//            builder.HasData
//            (
//                new IdentityRole
//                {
//                    Name = "Admin"
//                },
//                new IdentityRole
//                {
//                    Name = "Employee"
//                }
//            ); ;
//        }
//    }

//    public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
//    {
//        public void Configure(EntityTypeBuilder<ApplicationUser> builder)
//        {
//            builder.ToTable("AspNetUsers");
//            builder.HasData
//            (
//                new ApplicationUser
//                {
//                    UserName = "tina.horv@gmail.com",
//                    PasswordHash = "Pass12345!"
//                },
//                new ApplicationUser
//                {
//                    UserName = "admin.app@gmail.com",
//                    PasswordHash = "Pass12345!"
//                },
//                new ApplicationUser
//                {
//                    UserName = "userNotEmployee.app@gmail.com",
//                    PasswordHash = "Pass12345!"
//                },
//                new ApplicationUser
//                {
//                    UserName = "userEmployee.app@gmail.com",
//                    PasswordHash = "Pass12345!"
//                }
//            );
//        }
//    }

//    public class ApplicationUserRoleConfiguration : IEntityTypeConfiguration<IdentityUserRole<>
//    {
//        public void Configure(EntityTypeBuilder<IdentityUserRole> builder)
//        {
//            builder.ToTable("AspNetUserRoles");

//            builder.HasData
//            (
//                new IdentityRole
//                {
//                    Name = "Admin"
//                },
//                new IdentityRole
//                {
//                    Name = "Employee"
//                }
//            ); ;
//        }
//    }

//    public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
//    {
//        public void Configure(EntityTypeBuilder<Employee> builder)
//        {
//            throw new NotImplementedException();
//        }
//    }


//}
