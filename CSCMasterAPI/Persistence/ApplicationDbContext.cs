

using CSCMasterAPI.Persistence.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Reflection.Emit;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace CSCMasterAPI.Persistence
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> User => Set<User>();
        public DbSet<District> District => Set<District>();
        public DbSet<UserDistrict> UserDistrict => Set<UserDistrict>();
        public DbSet<Member> Member => Set<Member>();
        public DbSet<Entrollment> Entrollment => Set<Entrollment>();


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.EnableSensitiveDataLogging();
            optionsBuilder.LogTo(Console.WriteLine);
        }
        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.Properties<decimal>().HavePrecision(18, 6);
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            DefaultPreSetup(modelBuilder);
            UserModelCreating(modelBuilder.Entity<User>());
            DistrictModelCreating(modelBuilder.Entity<District>());
            UserDistrictModelCreating(modelBuilder.Entity<UserDistrict>());
            MemberModelCreating(modelBuilder.Entity<Member>());
            EntrollmentModelCreating(modelBuilder.Entity<Entrollment>());
            DbInitializeRecord(modelBuilder);
        }
        private void DefaultPreSetup(ModelBuilder modelBuilder)
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties()
                    .Where(p => p.ClrType == typeof(DateTimeOffset)
                             || p.ClrType == typeof(DateTimeOffset?)))
                {
                    property.SetColumnType("datetimeoffset(3)");
                }
            }
        }
        private void UserModelCreating(EntityTypeBuilder<User> userBuilder)
        {
            userBuilder.HasKey(e => e.Id);
            userBuilder.Property(e => e.Id).ValueGeneratedOnAdd();
        }

        private void DistrictModelCreating(EntityTypeBuilder<District> districtBuilder)
        {
            districtBuilder.HasKey(e => e.Id);
            districtBuilder.Property(e => e.Id).ValueGeneratedOnAdd();
        }

        private void UserDistrictModelCreating(EntityTypeBuilder<UserDistrict> userDistrictBuilder)
        {
            userDistrictBuilder.HasKey(e => new { e.UserId, e.DistrictId });
            userDistrictBuilder.HasOne<User>().WithMany().HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Restrict);
            userDistrictBuilder.HasOne<District>().WithMany().HasForeignKey(e => e.DistrictId).OnDelete(DeleteBehavior.Restrict);
        }

        private void MemberModelCreating(EntityTypeBuilder<Member> memberBuilder)
        {
            memberBuilder.HasKey(e => e.Id);
            memberBuilder.Property(e => e.Id).ValueGeneratedOnAdd();
            memberBuilder.HasOne<User>().WithMany().HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Restrict);
            memberBuilder.HasOne<District>().WithMany().HasForeignKey(e => e.DistrictId).OnDelete(DeleteBehavior.Restrict);
        }

        private void EntrollmentModelCreating(EntityTypeBuilder<Entrollment> entrollmentBuilder)
        {
            entrollmentBuilder.HasKey(e => e.Id);
            entrollmentBuilder.Property(e => e.Id).ValueGeneratedOnAdd();
            entrollmentBuilder.HasOne<User>().WithMany().HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Restrict);
            entrollmentBuilder.HasOne<Member>().WithMany().HasForeignKey(e => e.MemberId).OnDelete(DeleteBehavior.Restrict);
            entrollmentBuilder.HasOne<District>().WithMany().HasForeignKey(e => e.DistrictId).OnDelete(DeleteBehavior.Restrict);
        }
        private void DbInitializeRecord(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<District>().HasData(new
            {
                Id = 1,
                Name = "Thiruvarur"
            });
            modelBuilder.Entity<District>().HasData(new
            {
                Id = 2,
                Name = "Nagapattinam"
            });
            modelBuilder.Entity<User>().HasData(new
            {
                Id = 1,
                Name = "Retinue",
                Phone = "9943135008",
                Aadhaar = "123456789012",
                Status = true,
                Username = "Redmin",
                Password = "Red@123",
                LoginAllowed = true
            });

            modelBuilder.Entity<User>().HasData(new
            {
                Id = 2,
                Name = "Karthi",
                Phone = "9444488760",
                Aadhaar = "123456789013",
                Status = true,
                Username = "Karthi",
                Password = "Karthi123",
                LoginAllowed = true
            });

            modelBuilder.Entity<UserDistrict>().HasData(new
            {
                UserId = 1,
                DistrictId = 1
            });
            modelBuilder.Entity<UserDistrict>().HasData(new
            {
                UserId = 1,
                DistrictId = 2
            });
            modelBuilder.Entity<UserDistrict>().HasData(new
            {
                UserId = 2,
                DistrictId = 1
            });
            modelBuilder.Entity<UserDistrict>().HasData(new
            {
                UserId = 2,
                DistrictId = 2
            });

            //Thiruvarur
            modelBuilder.Entity<Member>().HasData(
            new Member
            {
                Id = 1,
                DistrictId = 1,
                UserId = 2,
                District = "Thiruvarur",
                Block = "Mannargudi",
                Name = "PRAKATHI V",
                Phone = "8428516150",
                StationId = "40288",
                Password = "40288@",
                LoginAllowed = true,
                Aadhaar = "781191892916",
                Mode = "Online",
                OnboardDate = new DateTime(2026, 2, 23),
                Status = true
            },
            new Member
            {
                Id = 2,
                DistrictId = 1,
                UserId = 2,
                District = "Thiruvarur",
                Block = "KODAVSAL",
                Name = "VIVEKA RAJASEKARAN",
                Phone = "9688129202",
                StationId = "40287",
                Password = "40287@",
                LoginAllowed = true,
                Aadhaar = "414969174894",
                Mode = "Online",
                OnboardDate = new DateTime(2026, 2, 23),
                Status = true
            },
            new Member
            {
                Id = 3,
                DistrictId = 1,
                UserId = 2,
                District = "Thiruvarur",
                Block = "THIRUTHURAIPOONDI\r\n",
                Name = "PRAVINSHA ELAVARASAN",
                Phone = "9047595234",
                StationId = "40286",
                Password = "40286@",
                LoginAllowed = true,
                Aadhaar = "520549764245",
                Mode = "Online",
                OnboardDate = new DateTime(2026, 2, 23),
                Status = true
            },
            new Member
            {
                Id = 4,
                DistrictId = 1,
                UserId = 2,
                District = "Thiruvarur",
                Block = "Mannargudi",
                Name = "S SOUNDARAPANDIAN",
                Phone = "7904784390",
                StationId = "32115",
                Password = "32115@",
                LoginAllowed = true,
                Aadhaar = "741948749615",
                Mode = "Online",
                OnboardDate = new DateTime(2026, 2, 23),
                Status = true
            },
            new Member
            {
                Id = 5,
                DistrictId = 1,
                UserId = 2,
                District = "Thiruvarur",
                Block = "MUTHUPET\r\n",
                Name = "Ganesh anbazhagan",
                Phone = "9047430909",
                StationId = "52142",
                Password = "52142@",
                LoginAllowed = true,
                Aadhaar = "283319238556",
                Mode = "Online",
                OnboardDate = new DateTime(2026, 2, 23),
                Status = true
            },
            new Member
            {
                Id = 6,
                DistrictId = 1,
                UserId = 2,
                District = "Thiruvarur",
                Block = "Mannargudi",
                Name = "Mohamed Yunus M",
                Phone = "7200882629",
                StationId = "8228",
                Password = "8228@",
                LoginAllowed = true,
                Aadhaar = "677251519950",
                Mode = "Online",
                OnboardDate = new DateTime(2026, 2, 23),
                Status = true
            },
            new Member
            {
                Id = 7,
                DistrictId = 1,
                UserId = 2,
                District = "Thiruvarur",
                Block = "NEEDAMANGALAM",
                Name = "Ramesh Prabu",
                Phone = "9245329720",
                StationId = "8224",
                Password = "8224@",
                LoginAllowed = true,
                Aadhaar = "401851195105",
                Mode = "Online",
                OnboardDate = new DateTime(2026, 2, 23),
                Status = true
            },
            new Member
            {
                Id = 8,
                DistrictId = 1,
                UserId = 2,
                District = "Thiruvarur",
                Block = "NEEDAMANGALAM",
                Name = "GANESH R",
                Phone = "8760849391",
                StationId = "8219",
                Password = "8219@",
                LoginAllowed = true,
                Aadhaar = "516873296730",
                Mode = "Online",
                OnboardDate = new DateTime(2026, 2, 23),
                Status = true
            },
            new Member
            {
                Id = 9,
                DistrictId = 1,
                UserId = 2,
                District = "Thiruvarur",
                Block = "KODAVASAL",
                Name = "SHANMUGAPRIYA P",
                Phone = "9688925880",
                StationId = "11274",
                Password = "11274@",
                LoginAllowed = true,
                Aadhaar = "924108073256",
                Mode = "Online",
                OnboardDate = new DateTime(2026, 2, 23),
                Status = true
            },
            new Member
            {
                Id = 10,
                DistrictId = 1,
                UserId = 2,
                District = "Thiruvarur",
                Block = "MANNARGUDI",
                Name = "KUMARAN GANAPATHI",
                Phone = "8870207988",
                StationId = "11272",
                Password = "11272@",
                LoginAllowed = true,
                Aadhaar = "568932655898",
                Mode = "Online",
                OnboardDate = new DateTime(2026, 2, 23),
                Status = true
            },
            new Member
            {
                Id = 11,
                DistrictId = 1,
                UserId = 2,
                District = "Thiruvarur",
                Block = "Mannargudi",
                Name = "Sumathi",
                Phone = "9042626755",
                StationId = "8222",
                Password = "8222@",
                LoginAllowed = true,
                Aadhaar = "828526901867",
                Mode = "Online",
                OnboardDate = new DateTime(2026, 2, 23),
                Status = true
            },
            new Member
            {
                Id = 12,
                DistrictId = 1,
                UserId = 2,
                District = "Thiruvarur",
                Block = "MUTHUPET",
                Name = "KaniMozhi",
                Phone = "9003330783",
                StationId = "8219",
                Password = "8219@",
                LoginAllowed = true,
                Aadhaar = "984494354407",
                Mode = "Online",
                OnboardDate = new DateTime(2026, 2, 23),
                Status = true
            },
            new Member
            {
                Id = 13,
                DistrictId = 1,
                UserId = 2,
                District = "Thiruvarur",
                Block = "Mannargudi",
                Name = "ATCHIYA MURUGANANDHAM",
                Phone = "9751861115",
                StationId = "11179",
                Password = "11179@",
                LoginAllowed = true,
                Aadhaar = "700887772985",
                Mode = "Online",
                OnboardDate = new DateTime(2026, 2, 23),
                Status = true
            },
            new Member
            {
                Id = 14,
                DistrictId = 1,
                UserId = 2,
                District = "Thiruvarur",
                Block = "Thiruvarur",
                Name = "SATHEESH THAMBIYAN",
                Phone = "9543642406",
                StationId = "40362",
                Password = "40362@",
                LoginAllowed = true,
                Aadhaar = "885383877726",
                Mode = "Online",
                OnboardDate = new DateTime(2026, 2, 23),
                Status = true
            },
            new Member
            {
                Id = 15,
                DistrictId = 1,
                UserId = 2,
                District = "Thiruvarur",
                Block = "THIRUTHURAIPOONDI",
                Name = "Govindaraj",
                Phone = "9751867179",
                StationId = "52012",
                Password = "52012@",
                LoginAllowed = true,
                Aadhaar = "283019815916",
                Mode = "Online",
                OnboardDate = new DateTime(2026, 2, 23),
                Status = true
            },
            new Member
            {
                Id = 16,
                DistrictId = 1,
                UserId = 2,
                District = "Thiruvarur",
                Block = "NEEDAMANGALAM",
                Name = "SUBISH SUGUMARAN",
                Phone = "8270741713",
                StationId = "11176",
                Password = "11176@",
                LoginAllowed = true,
                Aadhaar = "712083934553",
                Mode = "Online",
                OnboardDate = new DateTime(2026, 2, 23),
                Status = true
            });

            //Nagapattinam
            modelBuilder.Entity<Member>().HasData(
            new Member
            {
                Id = 17,
                DistrictId = 2,
                UserId = 2,
                District = "NAGAPATTINAM",
                Block = "NAGOOR",
                Name = "kaviyarasan",
                Phone = "8072537753",
                StationId = "40361",
                Password = "40361@",
                LoginAllowed = true,
                Aadhaar = "256646693269",
                Mode = "Online",
                OnboardDate = new DateTime(2026, 2, 23),
                Status = true
            },
            new Member
            {
                Id = 18,
                DistrictId = 2,
                UserId = 2,
                District = "NAGAPATTINAM",
                Block = "KEELAIYUR",
                Name = "Mathiyarasi Vengadachalam",
                Phone = "9943092275",
                StationId = "11276",
                Password = "11276@",
                LoginAllowed = true,
                Aadhaar = "544354172245",
                Mode = "Online",
                OnboardDate = new DateTime(2026, 2, 23),
                Status = true
            },
            new Member
            {
                Id = 19,
                DistrictId = 2,
                UserId = 2,
                District = "NAGAPATTINAM",
                Block = "KEEVALUR",
                Name = "saranya",
                Phone = "7339328559",
                StationId = "40360",
                Password = "40360@",
                LoginAllowed = true,
                Aadhaar = "672146031183",
                Mode = "Online",
                OnboardDate = new DateTime(2026, 2, 23),
                Status = true
            },
            new Member
            {
                Id = 20,
                DistrictId = 2,
                UserId = 2,
                District = "NAGAPATTINAM",
                Block = "NAGAPATTINAM",
                Name = "yuvashri",
                Phone = "8056064961",
                StationId = "40359",
                Password = "40359@",
                LoginAllowed = true,
                Aadhaar = "245634561191",
                Mode = "Online",
                OnboardDate = new DateTime(2026, 2, 23),
                Status = true
            });
        }
    }
}
