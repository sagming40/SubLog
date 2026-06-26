using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SubLog.Model;

namespace SubLog.Data
{
    public class SubLogDbContext : DbContext
    {
        // DbSet<T> 하나 = DB 테이블 하나
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<Category>     Categories    { get; set; }
        public DbSet<AppSetting>   AppSettings   { get; set; } // ✅ Task 3-2 추가

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlite("Data Source=sublog.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // 앱 첫 실행 시 기본 카테고리 자동 삽입 (Seed Data)
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "영상", ColorHex = "#E74C3C" },
                new Category { Id = 2, Name = "음악", ColorHex = "#3498DB" },
                new Category { Id = 3, Name = "업무", ColorHex = "#2ECC71" },
                new Category { Id = 4, Name = "게임", ColorHex = "#9B59B6" },
                new Category { Id = 5, Name = "기타", ColorHex = "#95A5A6" }
            );
        }
    }
}
