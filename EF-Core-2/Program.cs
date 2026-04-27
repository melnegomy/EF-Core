using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public enum BadgeTier
{
    Standard,
    VIP
}

public class Address
{
    public string Street { get; set; }
    public string City { get; set; }
    public string Country { get; set; }
    public string PostalCode { get; set; }
}

[Table("Organizers")]
public class Organizer
{
    [Key]
    public int OrganizerId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; }

    public string CompanyName { get; set; }

    public bool IsVerified { get; set; }

    public OrganizerProfile Profile { get; set; }

    public List<Event> Events { get; set; } = new List<Event>();
}

[Table("OrganizerProfiles")]
public class OrganizerProfile
{
    [Key]
    [ForeignKey("Organizer")]
    public int OrganizerId { get; set; }

    public string Biography { get; set; }

    public string WebsiteUrl { get; set; }

    public string LogoPath { get; set; }

    public Organizer Organizer { get; set; }
}

public class Attendee
{
    public int AttendeeId { get; set; }

    public string FullName { get; set; }

    public string Email { get; set; }

    public Address Address { get; set; }

    public Badge Badge { get; set; }

    public List<Registration> Registrations { get; set; } = new List<Registration>();
}

public class Registration
{
    public int AttendeeId { get; set; }

    public int EventId { get; set; }

    public string Note { get; set; }

    public DateTime RegistrationDate { get; set; }

    public Attendee Attendee { get; set; }

    public Event Event { get; set; }
}

public class Event
{
    public int EventId { get; set; }

    public string Title { get; set; }

    public string Description { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public int MaxAttendees { get; set; }

    public int OrganizerId { get; set; }

    public int? ParentEventId { get; set; }

    public Organizer Organizer { get; set; }

    public Event ParentEvent { get; set; }

    public List<Event> Sessions { get; set; } = new List<Event>();

    public List<Registration> Registrations { get; set; } = new List<Registration>();
}

public class Badge
{
    public int BadgeId { get; set; }

    public string BadgeNumber { get; set; }

    public DateTime IssuedDate { get; set; }

    public BadgeTier Tier { get; set; }

    public int AttendeeId { get; set; }

    public Attendee Attendee { get; set; }
}

public class EventConfiguration : IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> builder)
    {
        builder.ToTable("Events");

        builder.HasKey(x => x.EventId);

        builder.Property(x => x.Title).IsRequired().HasMaxLength(300);

        builder.Property(x => x.Description).IsRequired();

        builder.Property<DateTime>("CreatedAt")
            .HasDefaultValueSql("GETDATE()");

        builder.HasOne(x => x.ParentEvent)
            .WithMany(x => x.Sessions)
            .HasForeignKey(x => x.ParentEventId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Organizer)
            .WithMany(x => x.Events)
            .HasForeignKey(x => x.OrganizerId);
    }
}

public class BadgeConfiguration : IEntityTypeConfiguration<Badge>
{
    public void Configure(EntityTypeBuilder<Badge> builder)
    {
        builder.ToTable("Badges");

        builder.HasKey(x => x.BadgeId);

        builder.Property(x => x.BadgeNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(x => x.BadgeNumber).IsUnique();

        builder.Property(x => x.Tier)
            .HasConversion<string>();

        builder.HasOne(x => x.Attendee)
            .WithOne(x => x.Badge)
            .HasForeignKey<Badge>(x => x.AttendeeId);
    }
}

public class EventHubDbContext : DbContext
{
    public DbSet<Organizer> Organizers { get; set; }
    public DbSet<OrganizerProfile> OrganizerProfiles { get; set; }
    public DbSet<Event> Events { get; set; }
    public DbSet<Attendee> Attendees { get; set; }
    public DbSet<Badge> Badges { get; set; }
    public DbSet<Registration> Registrations { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(
            "Server=.;Database=EventHubDb;Trusted_Connection=True;TrustServerCertificate=True;");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(EventHubDbContext).Assembly);

        modelBuilder.Entity<Attendee>(entity =>
        {
            entity.ToTable("Attendees");

            entity.HasKey(x => x.AttendeeId);

            entity.Property(x => x.FullName)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(x => x.Email)
                .IsRequired();

            entity.HasIndex(x => x.Email).IsUnique();

            entity.OwnsOne(x => x.Address, a =>
            {
                a.Property(p => p.Street).HasColumnName("Street");
                a.Property(p => p.City).HasColumnName("City");
                a.Property(p => p.Country).HasColumnName("Country");
                a.Property(p => p.PostalCode).HasColumnName("PostalCode");
            });
        });

        modelBuilder.Entity<Registration>(entity =>
        {
            entity.ToTable("Registrations");

            entity.HasKey(x => new { x.AttendeeId, x.EventId });

            entity.HasOne(x => x.Attendee)
                .WithMany(x => x.Registrations)
                .HasForeignKey(x => x.AttendeeId);

            entity.HasOne(x => x.Event)
                .WithMany(x => x.Registrations)
                .HasForeignKey(x => x.EventId)
                .OnDelete(DeleteBehavior.NoAction);
        });
    }
}

internal class Program
{
    static async Task Main(string[] args)
    {
        using EventHubDbContext db = new EventHubDbContext();

        await db.Database.MigrateAsync();

        if (!db.Organizers.Any())
        {
            Organizer org1 = new Organizer
            {
                Name = "Ahmed Hassan",
                CompanyName = "Tech Egypt",
                IsVerified = true,
                Profile = new OrganizerProfile
                {
                    Biography = "Tech Events Organizer",
                    WebsiteUrl = "https://tech.com"
                }
            };

            Event event1 = new Event
            {
                Title = "Tech Conference",
                Description = "Big Event",
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(2),
                MaxAttendees = 500,
                Organizer = org1
            };

            Attendee at1 = new Attendee
            {
                FullName = "Sara Mohamed",
                Email = "sara@gmail.com",
                Address = new Address
                {
                    Street = "Nasr St",
                    City = "Cairo",
                    Country = "Egypt",
                    PostalCode = "12345"
                }
            };

            Registration reg1 = new Registration
            {
                Attendee = at1,
                Event = event1,
                RegistrationDate = DateTime.Now
            };

            Badge b1 = new Badge
            {
                BadgeNumber = "B1001",
                Tier = BadgeTier.VIP,
                IssuedDate = DateTime.Now,
                Attendee = at1
            };

            db.Add(org1);
            db.Add(event1);
            db.Add(at1);
            db.Add(reg1);
            db.Add(b1);

            await db.SaveChangesAsync();
        }

        var data = await db.Events
            .Include(x => x.Organizer)
            .Include(x => x.Registrations)
            .ThenInclude(x => x.Attendee)
            .ToListAsync();

        foreach (var item in data)
        {
            Console.WriteLine(item.Title);
            Console.WriteLine(item.Organizer.Name);
            Console.WriteLine(item.StartDate);

            foreach (var r in item.Registrations)
            {
                Console.WriteLine(r.Attendee.FullName);
            }

            Console.WriteLine("----------------");
        }

        Console.ReadLine();
    }
}