using BankingPaymentsAPI.Models;

using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace BankingPaymentsAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

       
        public DbSet<User> Users { get; set; }
        public DbSet<Bank> Banks { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<Beneficiary> Beneficiaries { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<SalaryBatch> SalaryBatches { get; set; }
        public DbSet<SalaryPayment> SalaryPayments { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<ReportRequest> ReportRequests { get; set; }

     
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User: enforce unique email
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Bank: enforce unique bank code
            modelBuilder.Entity<Bank>()
                .HasIndex(b => b.BankCode)
                .IsUnique();

            // Client: enforce unique client code per bank
            modelBuilder.Entity<Client>()
                .HasIndex(c => new { c.BankId, c.ClientCode })
                .IsUnique();
            // SalaryBatch → Client (no cascade)
            modelBuilder.Entity<SalaryBatch>()
                .HasOne(sb => sb.Client)
                .WithMany(c => c.SalaryBatches)
                .HasForeignKey(sb => sb.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            // SalaryPayment → Employee (cascade delete)
            modelBuilder.Entity<SalaryPayment>()
                .HasOne(sp => sp.Employee)
                .WithMany()
                .HasForeignKey(sp => sp.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            // SalaryPayment → SalaryBatch (set null on delete)
            modelBuilder.Entity<SalaryPayment>()
                .HasOne(sp => sp.SalaryBatch)
                .WithMany(sb => sb.Items)
                .HasForeignKey(sp => sp.SalaryBatchId)
                .OnDelete(DeleteBehavior.SetNull);



        }
    }
}
