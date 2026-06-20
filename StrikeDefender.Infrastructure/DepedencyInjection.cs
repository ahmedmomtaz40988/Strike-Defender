using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StrikeDefender.Application.Common.Authorization;
using StrikeDefender.Application.Common.Interfaces;
using StrikeDefender.Application.Payments.Commands.PaymentWebhook;
using StrikeDefender.Domain.Attacks;
using StrikeDefender.Domain.Payments;
using StrikeDefender.Domain.Plans;
using StrikeDefender.Domain.Rules;
using StrikeDefender.Domain.Subscriptions;
using StrikeDefender.Domain.Users;
using StrikeDefender.Infrastructure.AttackResults.Persistance;
using StrikeDefender.Infrastructure.Attacks.Persistance;
using StrikeDefender.Infrastructure.Common.Persistence.Data;
using StrikeDefender.Infrastructure.Common.Persistence.Seeding;
using StrikeDefender.Infrastructure.Dashboard;
using StrikeDefender.Infrastructure.ExternalServices.AI.Configurations;
using StrikeDefender.Infrastructure.ExternalServices.AI.Helpers;
using StrikeDefender.Infrastructure.ExternalServices.AI.Providers;
using StrikeDefender.Infrastructure.ExternalServices.AI.Services;
using StrikeDefender.Infrastructure.ExternalServices.Payment;
using StrikeDefender.Infrastructure.ExternalServices.Payment.Helper;
using StrikeDefender.Infrastructure.Payments.Persistence;
using StrikeDefender.Infrastructure.Plans.Persistance;
using StrikeDefender.Infrastructure.Rules.Persistance;
using StrikeDefender.Infrastructure.Service.FuzzzySearch;
using StrikeDefender.Infrastructure.Services.Files;
using StrikeDefender.Infrastructure.Subscriptions.Persistance;
using StrikeDefender.Infrastructure.SuccessfulAttacks.Persistance;
using Web.Infrastructure.Service.Auth;
using Web.Infrastructure.Users.Persistence;

namespace StrikeDefender.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            return services
                .AddPersistence()
                .AddDatabaseConfig(configuration)
                .AddIdentityConfig()
                .AddAuthorizationConfig()
                .AddPaymentServices(configuration)
                .AddAIServices(configuration);
        }

        public static IServiceCollection AddPersistence(this IServiceCollection services)
        {
            services.AddScoped<IUnitOfWork>(serviceProvider => serviceProvider.GetRequiredService<StrikeDefenderDbContext>());
            services.AddScoped<RoleSeeder>();
            services.AddScoped<IFuzzySearchRepository, FuzzySearchRepository>();
            services.AddScoped<IFileHelperService, FileHelper>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ISubscriptionAccessService, SubscriptionRepository>();
            services.AddScoped<IPlanRepository, PlanRepository>();
            services.AddScoped<IAttackRepository, AttackRepository>();
            services.AddScoped<ISuccessfulAttackRepository, SuccessfulAttackRepository>();
            services.AddScoped<IRuleRepository, RuleRepository>();
            services.AddScoped<IDashboardRepository, DashboardRepository>();
            services.AddScoped<IGenericRepository<Plan>, PlanRepository>();
            services.AddScoped<IGenericRepository<Attack>, AttackRepository>();
            services.AddScoped<IGenericRepository<WafRule>, RuleRepository>();
            services.AddScoped<IGenericRepository<ParsedWafRule>, ParsedWafRuleRepository>();
            services.AddScoped<IGenericRepository<AttackResult>, AttackResultRepository>();
            services.AddScoped<IGenericRepository<SuccessfulAttack>, SuccessfulAttackRepository>();
            services.AddScoped<IGenericRepository<Subscription>, SubscriptionRepository>();

            return services;
        }
        private static IServiceCollection AddDatabaseConfig(this IServiceCollection services, IConfiguration configuration)
        {
     var connectionString = configuration.GetConnectionString("DefaultConnection")
         // var connectionString = configuration.GetConnectionString("localhostConnection")
                ??
            throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            services.AddDbContext<StrikeDefenderDbContext>(options =>
                options.UseSqlServer(connectionString));

            return services;
        }

        public static IServiceCollection AddAIServices(
           this IServiceCollection services,
           IConfiguration config)
        {
            // 🔧 Options
            services.Configure<GeminiOptions>(
                config.GetSection("Gemini"));

            services.Configure<OpenRouterOptions>(
                config.GetSection("OpenRouter"));

            services.Configure<GroqOptions>(
                config.GetSection("Groq"));

            // ⚙️ Helpers
            services.AddSingleton<AiRateLimiter>();
            services.AddSingleton<AiUsageTracker>();

            // 🌐 Http Clients (لكل Provider)
            services.AddHttpClient<GeminiProvider>(c =>
            {
                c.Timeout = TimeSpan.FromSeconds(60);
            });

            services.AddHttpClient<OpenRouterProvider>(c =>
            {
                c.Timeout = TimeSpan.FromSeconds(60);
            });

            services.AddHttpClient<GroqProvider>(c =>
            {
                c.Timeout = TimeSpan.FromSeconds(60);
            });

         
            services.AddScoped<IAiProvider, GeminiProvider>();
            services.AddScoped<IAiProvider, OpenRouterProvider>();
            services.AddScoped<IAiProvider, GroqProvider>();

            
            services.AddScoped<IAiEngineService, AiEngineService>();

            return services;
        }
        public static IServiceCollection AddPaymentServices(
             this IServiceCollection services,
             IConfiguration config)
        {
            // 🔧 1. Bind Settings
            services.Configure<PaymobSettings>(
                config.GetSection("Paymob"));

            // 🌐 2. HttpClient (PaymentService)
            services.AddHttpClient<IPaymentService, PaymentService>();

            // 🧠 3. Helpers / Services
            services.AddScoped<IPaymobTokenProvider, PaymobTokenProvider>();
            //services.AddScoped<IPaymobHmacValidator, PaymobHmacValidator>();

            // 🗄️ 4. Repository
            services.AddScoped<IPaymentTransactionRepository, PaymentRepository>();
            services.AddScoped<IGenericRepository<PaymentTransaction>, PaymentRepository>();
            return services;
        }
        private static IServiceCollection AddIdentityConfig(this IServiceCollection services)
        {
            services.AddIdentityCore<AppUser>()
       .AddRoles<IdentityRole>()
       .AddEntityFrameworkStores<StrikeDefenderDbContext>()
       .AddDefaultTokenProviders();

            return services;
        }

        private static IServiceCollection AddAuthorizationConfig(this IServiceCollection services)
        {
            services.AddScoped<IAuthorizationHandler, PermissionHandler>();

            services.AddAuthorization(options =>
            {
                var permissions = typeof(Permissions)
                    .GetFields()
                    .Select(f => f.GetValue(null)?.ToString());

                foreach (var permission in permissions)
                {
                    options.AddPolicy(permission!, policy =>
                        policy.Requirements.Add(
                            new PermissionRequirement(permission!)));
                }
            });

            return services;
        }

    }
}
