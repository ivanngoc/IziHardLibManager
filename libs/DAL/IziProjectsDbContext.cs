using System.Text.Json.Serialization;
using IziHardGames.DotNetProjects;
using Microsoft.EntityFrameworkCore;

namespace IziLibrary.Database.DataBase.EfCore
{
    public class IziProjectsDbContext : DbContext
    {
        public static Guid laptop = Guid.Parse("a77b9c62-0dea-4035-a9c9-1ed09999794d");
        public static Guid desktopVn = Guid.Parse("80afa9a5-f561-4a64-bab1-14654fa76145");
        public static Guid desktopKem = Guid.Parse("429862e8-12d6-470d-8808-592c573e34af");
        public DbSet<EntityCsproj> Csprojs { get; set; }
        public DbSet<CsProjectAtDevice> ProjectsAtDevice { get; set; }
        public DbSet<IziProject> Projects { get; set; }
        public DbSet<Device> Devices { get; set; }
        public DbSet<DeviceSettings> DeviceSettings { get; set; }

        public IziProjectsDbContext(DbContextOptions<IziProjectsDbContext> opt) : base(opt)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Device>().HasOne(x => x.Settings).WithOne(x => x.Device).HasForeignKey<DeviceSettings>(x => x.Id).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<DeviceSettings>().HasOne(x => x.Device).WithOne(x => x.Settings).HasForeignKey<Device>(x => x.Id).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<EntityCsproj>(x =>
            {
                x.HasKey(x => x.EntityCsprojId);
                x.Property(x => x.EntityCsprojId).HasConversion(x => x.Guid, x => CsprojId.Create(x));
                x.HasMany(x => x.CsProjectAtDevices).WithOne(x => x.EntityCsproj).HasForeignKey(x => x.EntityCsprojId);
            });
            modelBuilder.Entity<CsProjectAtDevice>(x =>
            {
                x.HasKey(x => x.Id);
                x.Property(x => x.EntityCsprojId).HasConversion(x => x.Guid, x => CsprojId.Create(x));
            });
        }

        public async Task Init()
        {


            Devices.Add(new Device()
            {
                Id = laptop,
                Settings = new DeviceSettings()
                {
                    Id = laptop,
                    SourceDirs = new[] { "C:\\Users\\ivan\\Documents\\.csharp",
                        //"Z:\\.izhg-lib" ,
                        //"Z:\\.izhg-refs" ,
                        //"Z:\\[Projects] C#" ,
                    },
                }
            });

            Devices.Add(new Device()
            {
                Id = desktopVn,
                Settings = new DeviceSettings()
                {
                    Id = desktopVn,
                    SourceDirs = new[] { "", },
                }
            });

            Devices.Add(new Device()
            {
                Id = desktopKem,
                Settings = new DeviceSettings()
                {
                    Id = desktopKem,
                    SourceDirs = new[] { "", },
                }
            });

            await this.SaveChangesAsync();
        }
    }

    public class IziProject
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public ICollection<Device> Devices { get; set; } = null!;
    }

    public class Device
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int DeviceSettingsId { get; set; }
        [JsonIgnore] public DeviceSettings Settings { get; set; } = null!;
        public ICollection<IziProject> IziProjects { get; set; } = null!;
    }

    public class DeviceSettings
    {
        public Guid Id { get; set; }
        public string[] SourceDirs { get; set; } = Array.Empty<string>();
        [JsonIgnore] public Device Device { get; set; } = null!;
    }
}
