using AzilEdu.Shared.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AzilEdu.Api.Data;

public static class AppUserSeeder
{
    public static async Task SeedAsync(AzilEduDbContext db)
    {
        if (await db.AppUsers.AnyAsync())
            return;

        var hasher = new PasswordHasher<AppUser>();

        var admin = new AppUser
        {
            Email = "admin@aziledu.local",
            DisplayName = "AzilEdu Admin"
        };
        admin.PasswordHash = hasher.HashPassword(admin, "Admin123!");

        var firstEmployeeId = await db.Employees
            .OrderBy(item => item.Id)
            .Select(item => (int?)item.Id)
            .FirstOrDefaultAsync();

        var employee = new AppUser
        {
            Email = "employee@aziledu.local",
            DisplayName = "Djelatnik azila",
            EmployeeId = firstEmployeeId
        };
        employee.PasswordHash = hasher.HashPassword(employee, "Employee123!");

        var firstVolunteerId = await db.Volunteers
            .OrderBy(item => item.Id)
            .Select(item => (int?)item.Id)
            .FirstOrDefaultAsync();

        var volunteer = new AppUser
        {
            Email = "volunteer@aziledu.local",
            DisplayName = "Demo volonter",
            VolunteerId = firstVolunteerId
        };
        volunteer.PasswordHash = hasher.HashPassword(volunteer, "Volunteer123!");

        var firstDonorId = await db.Donors
            .OrderBy(item => item.Id)
            .Select(item => (int?)item.Id)
            .FirstOrDefaultAsync();

        var donor = new AppUser
        {
            Email = "donor@aziledu.local",
            DisplayName = "Demo donator",
            DonorId = firstDonorId
        };
        donor.PasswordHash = hasher.HashPassword(donor, "Donor123!");

        db.AppUsers.AddRange(admin, employee, volunteer, donor);
        await db.SaveChangesAsync();

        db.AppUserRoles.AddRange(
            new AppUserRole { AppUserId = admin.Id, AppRoleId = 1 },
            new AppUserRole { AppUserId = admin.Id, AppRoleId = 2 },
            new AppUserRole { AppUserId = employee.Id, AppRoleId = 1 },
            new AppUserRole { AppUserId = employee.Id, AppRoleId = 3 },
            new AppUserRole { AppUserId = volunteer.Id, AppRoleId = 1 },
            new AppUserRole { AppUserId = volunteer.Id, AppRoleId = 4 },
            new AppUserRole { AppUserId = donor.Id, AppRoleId = 1 },
            new AppUserRole { AppUserId = donor.Id, AppRoleId = 5 }
        );

        await db.SaveChangesAsync();
    }
}