using FinSight.GrpcInterceptors;
using IdentityService.Api.Grpc;
using IdentityService.External.Contracts.ServiceContracts;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using ProtoBuf.Grpc.Server;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.EntityFrameworkCore;
using IdentityService.Internal.Contracts.ServiceContracts;
var builder = WebApplication.CreateBuilder(args);

// 1. Load your business assembly exactly like your Grpc engine does
var businessAssembly = BusinessAssemblyLoader.Load("IdentityService.Business");
var grpcManagerServices = builder.Services.AddExternalManagerGrpcServices(typeof(IIdentityManager).Assembly, businessAssembly);

// ==========================================
// 💡 INTERFACE-ONLY SCANNING ENGINE
// ==========================================
var allTypesInBusiness = businessAssembly.GetTypes();

// A. Scan and Register ITenantAccessor implementations as Scoped
var accessorImpl = allTypesInBusiness.FirstOrDefault(t => typeof(ITenantAccessor).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);
if (accessorImpl != null) builder.Services.AddScoped(typeof(ITenantAccessor), accessorImpl);

// B. Scan and Register ITenantConnectionLookup implementations as Singleton
var lookupImpl = allTypesInBusiness.FirstOrDefault(t => typeof(ITenantConnectionLookup).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);
if (lookupImpl != null) builder.Services.AddSingleton(typeof(ITenantConnectionLookup), lookupImpl);

// C. Scan and Register your custom DbContext type dynamically by looking for DbContext inheritance
var dbContextType = allTypesInBusiness.FirstOrDefault(t => t.IsSubclassOf(typeof(DbContext)) && !t.IsAbstract);
if (dbContextType != null) builder.Services.AddScoped(dbContextType);

// D. Scan and Register your IdentityDatabaseRa data access class dynamically by name/convention
var dbRaType = allTypesInBusiness.FirstOrDefault(t => t.Name == "IdentityDatabaseRa" && !t.IsAbstract);
if (dbRaType != null) builder.Services.AddScoped(dbRaType);

builder.Services.AddScoped<TenantGrpcInterceptor>();


// ==========================================
// 2. Code-First gRPC Engine Pipelines Setup
// ==========================================
builder.Services.AddCodeFirstGrpc(options =>
{
    // 🔥 FIXED: Direct strongly-typed injection guarantees symbols load and map!
    options.Interceptors.Add<TenantGrpcInterceptor>();
});
// 🔥 THE FIX: Inject the Interceptor directly into the Core Microsoft gRPC Server Layer
// This intercepts every dynamic MapGrpcService method, regardless of reflection.
builder.Services.Configure<Grpc.AspNetCore.Server.GrpcServiceOptions>(options =>
{
    options.Interceptors.Add<TenantGrpcInterceptor>();
});
// 3. Core Server Pipeline Configurations
builder.Services.AddControllers();
builder.Services.AddHealthChecks();

builder.WebHost.ConfigureKestrel(options =>
{
    options.ConfigureEndpointDefaults(listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
    });
});

builder.Services.AddScoped<CorrelationIdInterceptor>();

// 4. Security Token Signature Verification
var jwtSecretKey = "YourSuperSecretBackupKeyThatIsAtLeast32CharsLong!";
var keyBytes = Encoding.UTF8.GetBytes(jwtSecretKey);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(keyBytes)
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                Console.WriteLine($"[AUTH-EVENT] [1. RECEIVED] Token network packet arrived at boundary.");
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                var username = context.Principal?.Identity?.Name ?? "Anonymous";
                Console.WriteLine($"[AUTH-EVENT] [2. SUCCESS] Token cryptographically verified for User: '{username}'");
                return Task.CompletedTask;
            },
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine($"[AUTH-EVENT] [❌ CRASH] Cryptographic verification failed: {context.Exception.Message}");
                return Task.CompletedTask;
            },
            // 🔥 THE CATCH-ALL PROTECTION CHECKPOINT: Triggers for missing tokens, bad formats, or general 401 rejections
            OnChallenge = context =>
            {
                // 🎯 PLACE YOUR BREAKPOINT HERE! This will trap EVERY single failing network call.
                Console.WriteLine($"[AUTH-EVENT] [🚫 ACCESS DENIED] Issuing 401 Unauthorized challenge back to client.");
                Console.WriteLine($"Reason for failure: {context.Error} | Description: {context.ErrorDescription}");

                // Optional: Stop default framework behavior if you want to custom design the response payload body text
                return Task.CompletedTask;
            }
        };
    });

// Inside Program.cs - Authorization block area
builder.Services.AddAuthorization(options =>
{
    // 💡 Policy 1: Standard Above 21 check (Will PASS for someone born in 2000)
    options.AddPolicy("Above21Policy", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireAssertion(context =>
        {
            var dobClaim = context.User.FindFirst("date_of_birth")?.Value;
            if (DateTime.TryParse(dobClaim, out DateTime dob))
            {
                return dob <= DateTime.Today.AddYears(-21);
            }
            return false;
        });
    });

    // 💡 Policy 2: High Restriction Above 30 check (Will FAIL for someone born in 2000)
    options.AddPolicy("Above30Policy", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireAssertion(context =>
        {
            var dobClaim = context.User.FindFirst("date_of_birth")?.Value;
            if (DateTime.TryParse(dobClaim, out DateTime dob))
            {
                return dob <= DateTime.Today.AddYears(-30); // Born in 2000 is 26 in 2026 -> FAILS
            }
            return false;
        });
    });
});

// ALTERNATIVE CATCH-ALL (If the above gives any trouble, use this bulletproof configuration)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularPortal", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();
app.UseCors("AllowAngularPortal");
app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<TenantHttpInterceptorMiddleware>();

app.MapExternalManagerGrpcServices(grpcManagerServices);
app.MapControllers();
app.MapHealthChecks("/health");
app.Run();
