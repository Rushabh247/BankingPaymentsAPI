using BankingPaymentsAPI.Data;
using BankingPaymentsAPI.Repository;
using BankingPaymentsAPI.Services;
using BankingPaymentsAPI.Services.Notification;
using BankingPaymentsAPI.Services.PaymentProcessing;
using BankingPaymentsAPI.Services.ReportGeneration;


using BankingPaymentsAPI.Services.Storage;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;

namespace BankingPaymentsAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // -------------------------------
            // DbContext
            // -------------------------------
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // -------------------------------
            // Controllers with JSON options (ignore cycles)
            // -------------------------------
            builder.Services.AddControllers()
                .AddJsonOptions(opt =>
                {
                    opt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                    opt.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                });

            // -------------------------------
            // Swagger
            // -------------------------------
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "BankingPaymentsAPI", Version = "v1" });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Enter JWT token as: Bearer {token}",
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            // -------------------------------
            // JWT Authentication
            // -------------------------------
            var key = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]);
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = builder.Configuration["Jwt:Issuer"],
                        ValidAudience = builder.Configuration["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        RoleClaimType = ClaimTypes.Role,
                        NameClaimType = ClaimTypes.Name
                    };
                });

            builder.Services.AddAuthorization();

            // -------------------------------
            // Dependency Injection (Repositories & Services)
            // -------------------------------
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IUserService, UserService>();

            builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
            builder.Services.AddScoped<ITransactionService, TransactionService>();

            builder.Services.AddScoped<IBankRepository, BankRepository>();
            builder.Services.AddScoped<IBankService, BankService>();

            builder.Services.AddScoped<IClientRepository, ClientRepository>();
            builder.Services.AddScoped<IClientService, ClientService>();

            builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
            builder.Services.AddScoped<IEmployeeService, EmployeeService>();

            builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
            builder.Services.AddScoped<IPaymentService, PaymentService>();

            builder.Services.AddScoped<ISalaryRepository, SalaryRepository>();
            builder.Services.AddScoped<ISalaryService, SalaryService>();

            builder.Services.AddScoped<IBeneficiaryRepository, BeneficiaryRepository>();
            builder.Services.AddScoped<IBeneficiaryService, BeneficiaryService>();

            builder.Services.AddScoped<IReportRepository, ReportRepository>();
            builder.Services.AddScoped<IReportService, ReportService>();
            builder.Services.AddScoped<IReportGeneratorService, CloudinaryReportGeneratorService>(); // <-- updated

            builder.Services.AddScoped<IEmailNotificationService, EmailNotificationService>();

            builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();
            builder.Services.AddScoped<IDocumentService, DocumentService>();

            builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();
            builder.Services.AddScoped<IAuditService, AuditService>();

            builder.Services.AddScoped<IFileStorageService, CloudinaryFileStorageService>();

            builder.Services.AddScoped<IFundTransferService, FundTransferService>();

            builder.Services.AddScoped<IStripePaymentService, StripePaymentService>();

            builder.Services.AddHttpContextAccessor();

            var app = builder.Build();

            // -------------------------------
            // Middleware
            // -------------------------------
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
