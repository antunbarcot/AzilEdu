using AzilEdu.Api.Data;
using AzilEdu.Shared.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<AzilEduDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AzilEduDbContext>();

    await db.Database.MigrateAsync();

    if (!await db.Animals.AnyAsync())
    {
        db.Animals.AddRange(
            new Animal
            {
                Name = "Luna",
                Species = "Pas",
                Breed = "Labrador",
                Gender = "Ženka",
                Age = 3,
                ArrivalDate = new DateTime(2025, 10, 12),
                IsAdopted = false,
                ImageUrl = "/images/animals/luna.webp",
                Description = "Mirna i druželjubiva kujica koja voli šetnje."
            },
            new Animal
            {
                Name = "Maza",
                Species = "Mačka",
                Breed = "Domaća kratkodlaka",
                Gender = "Ženka",
                Age = 2,
                ArrivalDate = new DateTime(2025, 11, 5),
                IsAdopted = true,
                ImageUrl = "/images/animals/maza.webp",
                Description = "Zaigrana mačka naviknuta na boravak u zatvorenom prostoru."
            },
            new Animal
            {
                Name = "Rex",
                Species = "Pas",
                Breed = "Njemački ovčar",
                Gender = "Mužjak",
                Age = 5,
                ArrivalDate = new DateTime(2026, 1, 20),
                IsAdopted = false,
                ImageUrl = "/images/animals/rex.webp",
                Description = "Aktivan pas koji traži iskusnijeg vlasnika."
            },
            new Animal
            {
                Name = "Nala",
                Species = "Mačka",
                Breed = "Maine Coon mješanac",
                Gender = "Ženka",
                Age = null,
                ArrivalDate = new DateTime(2026, 2, 3),
                IsAdopted = false,
                ImageUrl = "/images/animals/nala.webp",
                Description = "Mlada mačka pronađena bez poznate povijesti."
            },
            new Animal
            {
                Name = "Tobi",
                Species = "Pas",
                Breed = "Mješanac",
                Gender = "Mužjak",
                Age = 1,
                ArrivalDate = null,
                IsAdopted = false,
                ImageUrl = "/images/animals/tobi.webp",
                Description = "Vesel pas kojem datum dolaska još nije potvrđen."
            },
            new Animal
            {
                Name = "Bruno",
                Species = "Pas",
                Breed = "Bigl",
                Gender = "Mužjak",
                Age = 4,
                ArrivalDate = new DateTime(2025, 9, 18),
                IsAdopted = true,
                ImageUrl = "/images/animals/bruno.webp",
                Description = "Udomljen pas koji ostaje u evidenciji azila."
            }
        );

        await db.SaveChangesAsync();
    }
    if (!await db.HousingUnits.AnyAsync())
    {
        db.HousingUnits.AddRange(
            new HousingUnit
            {
                Name = "Boks za pse 1",
                UnitType = "Pas",
                Capacity = 4,
                Occupied = 4,
                LastCleanedAt = new DateTime(2026, 6, 20),
                IsActive = true,
                ImageUrl = "/images/housing-units/box-1.webp",
                Note = "Aktivna jedinica, trenutno puna."
            },
            new HousingUnit
            {
                Name = "Boks za pse 2",
                UnitType = "Pas",
                Capacity = 5,
                Occupied = 2,
                LastCleanedAt = new DateTime(2026, 6, 25),
                IsActive = true,
                ImageUrl = "/images/housing-units/box-2.webp",
                Note = "Aktivna jedinica, ima slobodnih mjesta."
            },
            new HousingUnit
            {
                Name = "Prostor za mačke",
                UnitType = "Mačka",
                Capacity = 6,
                Occupied = 3,
                LastCleanedAt = new DateTime(2026, 6, 22),
                IsActive = true,
                ImageUrl = "/images/housing-units/cat-room.webp",
                Note = "Zajednički prostor za mačke."
            },
            new HousingUnit
            {
                Name = "Karantena 1",
                UnitType = "Karantena",
                Capacity = 2,
                Occupied = 1,
                LastCleanedAt = null,
                IsActive = true,
                ImageUrl = "/images/housing-units/quarantine.webp",
                Note = "Datum zadnjeg čišćenja nije unesen."
            },
            new HousingUnit
            {
                Name = "Boks za pse 3",
                UnitType = "Pas",
                Capacity = 3,
                Occupied = 0,
                LastCleanedAt = new DateTime(2026, 5, 30),
                IsActive = false,
                ImageUrl = "/images/housing-units/inactive-unit.webp",
                Note = "Neaktivna jedinica, trenutno se ne koristi."
            },
            new HousingUnit
            {
                Name = "Prostor za sitne životinje",
                UnitType = "Glodavci",
                Capacity = 8,
                Occupied = 5,
                LastCleanedAt = new DateTime(2026, 6, 28),
                IsActive = true,
                ImageUrl = "/images/housing-units/yard-unit.webp",
                Note = "Prostor za zečeve i glodavce."
            }
        );
        await db.SaveChangesAsync();
    }
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
