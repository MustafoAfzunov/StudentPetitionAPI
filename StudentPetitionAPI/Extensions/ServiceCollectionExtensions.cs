using System.Reflection;
using System.Globalization;
using System.Text;
using System.Text.Json.Serialization;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Localization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using StudentPetitionAPI.Infrastructure.Authentication;
using StudentPetitionAPI.Application.Mappings;
using StudentPetitionAPI.Infrastructure.Repositories;
using StudentPetitionAPI.Application.Interfaces;
using StudentPetitionAPI.Infrastructure.Serialization;
using StudentPetitionAPI.Application.Services;
using StudentPetitionAPI.Swagger;
using StudentPetitionAPI.Application.Validators;

namespace StudentPetitionAPI.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IStudentRepository, StudentRepository>();
        services.AddScoped<IPetitionRepository, PetitionRepository>();

        return services;
    }

    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IStudentService, StudentService>();
        services.AddScoped<IPetitionService, PetitionService>();

        return services;
    }

    public static IServiceCollection AddMapping(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(MappingProfile));

        return services;
    }

    public static IServiceCollection AddValidation(this IServiceCollection services)
    {
        services
            .AddFluentValidationAutoValidation()
            .AddValidatorsFromAssemblyContaining<CreateStudentRequestValidator>();

        return services;
    }

    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

        var jwtSettings = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
            ?? throw new InvalidOperationException("JWT settings are missing from configuration.");

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
                    RoleClaimType = System.Security.Claims.ClaimTypes.Role,
                    ClockSkew = TimeSpan.FromMinutes(1)
                };
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy(Policies.StudentOnly, policy =>
                policy.RequireRole(Roles.Student));

            options.AddPolicy(Policies.ReviewerOnly, policy =>
                policy.RequireRole(Roles.Reviewer));

            options.AddPolicy(Policies.CanCreateStudents, policy =>
                policy.RequireRole(Roles.Student));

            options.AddPolicy(Policies.CanViewStudents, policy =>
                policy.RequireRole(Roles.Student, Roles.Reviewer));

            options.AddPolicy(Policies.CanCreatePetitions, policy =>
                policy.RequireRole(Roles.Student));

            options.AddPolicy(Policies.CanManageOwnPetitions, policy =>
                policy.RequireRole(Roles.Student));

            options.AddPolicy(Policies.CanViewPetitions, policy =>
                policy.RequireRole(Roles.Student, Roles.Reviewer));

            options.AddPolicy(Policies.CanReviewPetitions, policy =>
                policy.RequireRole(Roles.Reviewer));
        });

        return services;
    }

    public static IServiceCollection AddApiControllers(this IServiceCollection services)
    {
        services
            .AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                options.JsonSerializerOptions.Converters.Add(new MmDdYyyyDateTimeConverter());
                options.JsonSerializerOptions.Converters.Add(new MmDdYyyyNullableDateTimeConverter());
            });

        return services;
    }

    public static IServiceCollection AddAppLocalization(this IServiceCollection services)
    {
        services.AddLocalization();

        services.Configure<RequestLocalizationOptions>(options =>
        {
            var supportedCultures = new[]
            {
                new CultureInfo("en-US")
            };

            options.DefaultRequestCulture = new RequestCulture("en-US");
            options.SupportedCultures = supportedCultures;
            options.SupportedUICultures = supportedCultures;
            options.ApplyCurrentCultureToResponseHeaders = true;
        });

        return services;
    }

    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Student Petition API",
                Version = "v1",
                Description = """
                    REST API for submitting and reviewing student petitions
                    (course retake, academic leave, major change, and other requests).

                    Authenticate via `POST /api/auth/login`, then use **Authorize** with the returned Bearer token.
                    Demo credentials are documented in the project README (Development only).
                    """,
                Contact = new OpenApiContact
                {
                    Name = "Student Petition API"
                }
            });

            options.EnableAnnotations();
            options.DescribeAllParametersInCamelCase();
            options.SchemaFilter<EnumSchemaFilter>();
            options.OperationFilter<RequestExamplesOperationFilter>();
            options.SupportNonNullableReferenceTypes();

            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
            }

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "JWT Authorization header using the Bearer scheme. Example: `Bearer {token}`"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
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

            options.TagActionsBy(api =>
            {
                if (api.GroupName is not null)
                {
                    return [api.GroupName];
                }

                return api.ActionDescriptor.RouteValues.TryGetValue("controller", out var controller)
                    ? [controller!]
                    : ["Default"];
            });

            options.DocInclusionPredicate((_, _) => true);
        });

        return services;
    }
}
