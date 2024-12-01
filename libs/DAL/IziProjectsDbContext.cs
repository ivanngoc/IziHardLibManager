using System.Text.Json.Serialization;
using IziHardGames.Asmdefs;
using IziHardGames.CrossTables;
using IziHardGames.DotNetProjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace IziLibrary.Database.DataBase.EfCore
{
    public class IziProjectsDbContext : DbContext
    {
        public static Guid laptop = Guid.Parse("a77b9c62-0dea-4035-a9c9-1ed09999794d");
        public static Guid desktopVn = Guid.Parse("80afa9a5-f561-4a64-bab1-14654fa76145");
        public static Guid desktopKem = Guid.Parse("429862e8-12d6-470d-8808-592c573e34af");

        public DbSet<EntityCsproj> Csprojs { get; set; }
        public DbSet<CsprojRelation> Relations { get; set; }
        public DbSet<CsprojRelationAtDevice> RelationsAtDevice { get; set; }
        public DbSet<CsProjectAtDevice> ProjectsAtDevice { get; set; }
        public DbSet<IziProject> Projects { get; set; }
        public DbSet<Device> Devices { get; set; }
        public DbSet<DeviceSettings> DeviceSettings { get; set; }


        // Unity3d Assembly defenitions
        public DbSet<EntityAsmdef> Asmdefs { get; set; }
        public DbSet<EntityAsmdefAtDevice> AsmdefsAtDevice { get; set; }
        public DbSet<RelationAsmdef> RelationAsmdefs { get; set; }
        public DbSet<RelationAsmdefAtDevice> RelationAsmdefsAtDevice { get; set; }
        public DbSet<AsmdefXCsproj> AsmdefXCsproj { get; set; }

        public DbSet<EntityMeta> Metas { get; set; }
        public DbSet<EntityMetaAtDevice> MetasAtDevice { get; set; }

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
                x.Property(x => x.RepoGitHub).HasMaxLength(256);
                x.HasKey(x => x.EntityCsprojId);
                x.Property(x => x.EntityCsprojId).HasConversion<CsprojIdConverter>();
                x.HasMany(x => x.CsProjectAtDevices).WithOne(x => x.EntityCsproj).HasForeignKey(x => x.EntityCsprojId);
            });

            modelBuilder.Entity<CsProjectAtDevice>(x =>
            {
                x.HasKey(x => new { x.DeviceId, x.EntityCsprojId });
                x.Property(x => x.EntityCsprojId).HasConversion(x => x.Guid, x => CsprojId.Create(x));
                x.HasOne(x => x.Device).WithMany(x => x.Csprojects).HasForeignKey(x => x.DeviceId);
            });

            modelBuilder.Entity<CsprojRelation>(x =>
            {
                x.HasKey(x => x.Id);
                x.Property(x => x.CheckDateTime).HasConversion<NullableDateTimeOffsetConverter>().HasColumnType("timestamptz");
                x.HasOne(x => x.Parent).WithMany(x => x.AsChild).HasForeignKey(x => x.ParentId);
                x.HasOne(x => x.Child).WithMany(x => x.AsParent).HasForeignKey(x => x.ChildId);
                x.HasIndex(x => new { x.ParentId, x.ChildId }).HasFilter($"\"{nameof(CsprojRelation.ParentId)}\" IS NOT NULL AND \"{nameof(CsprojRelation.ChildId)}\" IS NOT NULL").IsUnique();//.HasDatabaseName("IX_");
                x.Property(x => x.ParentId).HasConversion(x => ((Guid?)x), x => CsprojId.Create(x));
                x.Property(x => x.ChildId).HasConversion(x => ((Guid?)x), x => CsprojId.Create(x));
            });

            modelBuilder.Entity<CsprojRelationAtDevice>(x =>
            {
                x.HasKey(x => x.Id);
                x.HasIndex(x => new { x.RelationId, x.DeviceId }).IsUnique();
                x.HasOne(x => x.Relation).WithMany(x => x.RelationsAtDevice).HasForeignKey(x => x.RelationId);
                x.HasOne(x => x.Device).WithMany(x => x.Relations).HasForeignKey(x => x.DeviceId);
                x.HasIndex(x => x.Include);
            });

            modelBuilder.Entity<EntityAsmdef>(x =>
            {
                x.HasKey(x => x.EntityAsmdefId);
                x.Property(x => x.EntityAsmdefId).HasConversion<AsmdefIdConverter>();
                //x.Property(x => x.EntityAsmdefId).HasConversion(x => x.Guid, x => AsmdefId.Create(x));

                x.Property(x => x.MetaId).HasConversion<MetaIdConverterNullable>();
                x.HasOne(x => x.Meta).WithOne(x => x.Asmdef).HasForeignKey<EntityAsmdef>(x => x.MetaId);
                x.HasMany(x => x.AsmdefsAtDevice).WithOne(x => x.Asmdef).HasForeignKey(x => x.AsmdefId);
            });

            modelBuilder.Entity<EntityAsmdefAtDevice>(x =>
            {
                x.HasKey(x => new { x.DeviceId, x.AsmdefId });
                x.Property(x => x.AsmdefId).HasConversion<AsmdefIdConverter>();
                x.HasOne(x => x.Asmdef).WithMany(x => x.AsmdefsAtDevice).HasForeignKey(x => x.AsmdefId).OnDelete(DeleteBehavior.Cascade);
                x.HasOne(x => x.Device).WithMany(x => x.Asmdefs).HasForeignKey(x => x.DeviceId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<RelationAsmdef>(x =>
            {
                x.HasKey(x => x.Id);
                x.HasIndex(x => new { x.FromId, x.ToId }).HasFilter($"\"{nameof(RelationAsmdef.FromId)}\" IS NOT NULL AND \"{nameof(RelationAsmdef.ToId)}\" IS NOT NULL").IsUnique();
                x.HasOne(x => x.From).WithMany(x => x.AsParent).HasForeignKey(x => x.FromId);
                x.HasOne(x => x.To).WithMany(x => x.AsChild).HasForeignKey(x => x.ToId);
                x.Property(x => x.FromId).HasConversion<AsmdefIdConverterNullable>();
                x.Property(x => x.ToId).HasConversion<AsmdefIdConverterNullable>();

            });

            modelBuilder.Entity<RelationAsmdefAtDevice>(x =>
            {
                x.HasKey(x => new { x.DeviceId, x.RelationId });
                x.HasOne(x => x.Relation).WithMany(x => x.RelationsAtDevice).HasForeignKey(x => x.RelationId);
                x.HasOne(x => x.Device).WithMany(x => x.RelationsAsmdef).HasForeignKey(x => x.DeviceId);
            });

            modelBuilder.Entity<AsmdefXCsproj>(x =>
            {
                x.HasKey(x => new { x.AsmdefId, x.CsprojId });
                x.HasOne(x => x.Csproj).WithMany(x => x.AsmdefXCsprojs).HasForeignKey(x => x.CsprojId);
                x.HasOne(x => x.Asmdef).WithMany(x => x.AsmdefXCsprojs).HasForeignKey(x => x.AsmdefId);
                x.Property(x => x.CsprojId).HasConversion<CsprojIdConverter>();
                x.Property(x => x.AsmdefId).HasConversion<AsmdefIdConverter>();
            });

            modelBuilder.Entity<EntityMeta>(x =>
            {
                x.HasKey(x => x.MetaId);
                x.Property(x => x.MetaId).HasConversion<MetaIdConverter>();
            });

            modelBuilder.Entity<EntityMetaAtDevice>(x =>
            {
                x.HasKey(x => new { x.DeviceId, x.MetaId });
                x.Property(x => x.MetaId).HasConversion<MetaIdConverter>();
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
        public ICollection<CsProjectAtDevice> Csprojects { get; set; } = null!;
        public ICollection<CsprojRelationAtDevice> Relations { get; set; } = null!;

        public ICollection<EntityAsmdefAtDevice> Asmdefs { get; set; } = null!;
        public ICollection<RelationAsmdefAtDevice> RelationsAsmdef { get; set; } = null!;
    }

    public class DeviceSettings
    {
        public Guid Id { get; set; }
        public string[] SourceDirs { get; set; } = Array.Empty<string>();
        [JsonIgnore] public Device Device { get; set; } = null!;
    }

    /// <summary>
    /// a value converter that ensures you can run TMDS (integration tests) in timezones with UTC offset != 0
    /// </summary>
    /// </summary>
    /// <remarks>
    /// Copied from https://github.com/npgsql/npgsql/issues/4176#issuecomment-1064313552
    /// </remarks>
    internal class DateTimeOffsetConverter : ValueConverter<DateTimeOffset, DateTimeOffset>
    {
        public DateTimeOffsetConverter()
            : base(
                d => d.ToUniversalTime(),
                d => d.ToUniversalTime())
        {
        }
    }
    /// <summary>
    /// <see cref="DateTimeOffsetConverter"/> but for nullable
    /// </summary>
    internal class NullableDateTimeOffsetConverter : ValueConverter<DateTimeOffset?, DateTimeOffset?>
    {
        public NullableDateTimeOffsetConverter()
            : base(
                d => d == null ? null : d.Value.ToUniversalTime(),
                d => d == null ? null : d.Value.ToUniversalTime())
        {
        }
    }

    internal class AsmdefIdConverter : ValueConverter<AsmdefId, Guid>
    {
        public AsmdefIdConverter()
            : base(
                x => x.Guid,
                x => AsmdefId.Create(x))
        {
        }
    }

    internal class AsmdefIdConverterNullable : ValueConverter<AsmdefId?, Guid?>
    {
        public AsmdefIdConverterNullable()
            : base(
                x => x.HasValue ? (Guid?)x.Value.Guid : null,
                x => x.HasValue ? AsmdefId.Create(x.Value) : null)
        {
        }
    }
    internal class CsprojIdConverter : ValueConverter<CsprojId, Guid>
    {
        public CsprojIdConverter()
            : base(
                x => x.Guid,
                x => CsprojId.Create(x))
        {
        }
    }

    internal class CsprojIdConverterNullable : ValueConverter<CsprojId?, Guid?>
    {
        public CsprojIdConverterNullable()
            : base(
                x => x.HasValue ? (Guid?)x.Value.Guid : null,
                x => x.HasValue ? CsprojId.Create(x.Value) : null)
        {
        }
    }

    internal class MetaIdConverter : ValueConverter<MetaId, Guid>
    {
        public MetaIdConverter()
            : base(
                x => x.Guid,
                x => MetaId.Create(x))
        {
        }
    }

    internal class MetaIdConverterNullable : ValueConverter<MetaId?, Guid?>
    {
        public MetaIdConverterNullable()
            : base(
                x => x.HasValue ? (Guid?)x.Value.Guid : null,
                x => x.HasValue ? MetaId.Create(x.Value) : null)
        {
        }
    }
}
