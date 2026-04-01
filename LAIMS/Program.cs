using LAIMS;
using LAIMS.Data;
using LAIMS.Interfaces;
using LAIMS.Interfaces.Banking;
using LAIMS.Interfaces.BatchJobs;
using LAIMS.Interfaces.BusinessRules;
using LAIMS.Interfaces.Claims;
using LAIMS.Interfaces.Commissions;
using LAIMS.Interfaces.Documents;
using LAIMS.Interfaces.Investments;
using LAIMS.Interfaces.Lifeproducts;
using LAIMS.Interfaces.Membership;
using LAIMS.Interfaces.Policies;
using LAIMS.Interfaces.Premiums;
using LAIMS.Interfaces.Questionnaires;
using LAIMS.Interfaces.Utilities;
using LAIMS.Models.Security;
using LAIMS.Repositories.Banking;
using LAIMS.Repositories.BatchJobs;
using LAIMS.Repositories.BusinessRules;
using LAIMS.Repositories.Claims;
using LAIMS.Repositories.Commissions;
using LAIMS.Repositories.Documents;
using LAIMS.Repositories.Investments;
using LAIMS.Repositories.Lifeproducts;
using LAIMS.Repositories.Membership;
using LAIMS.Repositories.Policies;
using LAIMS.Repositories.Premiums;
using LAIMS.Repositories.Questionnaires;
using LAIMS.Repositories.Utilities;
using LAIMS.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString,
      sqlServerOptionsAction: sqlOptions =>
      {
          // Enable transient error handling
          sqlOptions.EnableRetryOnFailure(
              maxRetryCount: 5, // Number of retry attempts (default is 5)
              maxRetryDelay: TimeSpan.FromSeconds(30), // Maximum delay between retries
              errorNumbersToAdd: null); // You can specify additional SQL error numbers to consider transient
      })); 
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
var identitySettings = new IdentitySettings();
builder.Configuration.Bind("IdentitySettings", identitySettings);
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.User.RequireUniqueEmail = false;
    // Password settings
    options.Password.RequiredLength = 12; //identitySettings.Password.RequiredLength;
    options.Password.RequireDigit = identitySettings.Password.RequireDigit;
    options.Password.RequireNonAlphanumeric = identitySettings.Password.RequireNonAlphanumeric;
    options.Password.RequireUppercase = identitySettings.Password.RequireUppercase;
    options.Password.RequireLowercase = identitySettings.Password.RequireLowercase;
    options.Password.RequiredUniqueChars = identitySettings.Password.RequiredUniqueChars;

    // Lockout settings
    // options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(identitySettings.Lockout.DefaultLockoutTimeSpanInMinutes);
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromDays(365 * 100); // 100 years
    options.Lockout.MaxFailedAccessAttempts = identitySettings.Lockout.MaxFailedAccessAttempts;
    options.Lockout.AllowedForNewUsers = identitySettings.Lockout.AllowedForNewUsers;
})
 .AddRoles<IdentityRole>()
 .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddRazorPages();
builder.Services.Configure<SecurityStampValidatorOptions>(options =>
{
    options.ValidationInterval = TimeSpan.Zero; // Validates the security stamp on every request
}); 
//.AddRazorPagesOptions(options=>options.Conventions.AddPageRoute("/Products/Index", "products")); 
builder.Services.AddScoped<IUserList, UserLists>();
builder.Services.AddScoped<IUploadData,UploadData  >();
builder.Services.AddScoped<IProductRepository, ProductRepository >();
builder.Services.AddScoped<IPolicyTypeRepository, PolicyTypeRepository>();
builder.Services.AddScoped<IPolicyTypesLinesRepository, PolicyTypesLinesRepository>(); 
builder.Services.AddScoped<IExpenseTypeRepository, ExpenseTypeRepository>();
builder.Services.AddScoped<IPolicyTypesExpenseLineRepository, PolicyTypesExpenseLineRepository>();
builder.Services.AddScoped<ICurrencyRepository, CurrencyRepository>();
builder.Services.AddScoped<IDocumentsRepository, DocumentsRepository>();
builder.Services.AddScoped<IPolicyTypeDocumentsRepository, PolicyTypeDocumentsRepository>();
builder.Services.AddScoped<IPolicyTypeLinesBenefitsRepository, PolicyTypeLinesBenefitsRepository>();
builder.Services.AddScoped<IPolicyTypeLinesBenefitsDocumentsRepository, PolicyTypeLinesBenefitsDocumentsRepository>();
builder.Services.AddScoped<IMemberRepository, MemberRepository>();
builder.Services.AddScoped<IRelationshipRepository, RelationshipRepository>();
builder.Services.AddScoped<IGenderRepository, GenderRepository>();
builder.Services.AddScoped<ITitleRepository, TitleRepository>();
builder.Services.AddScoped<IMaritalStatusRepository, MaritalStatusRepository>();
builder.Services.AddScoped<ICountryRepository, CountryRepository>();
builder.Services.AddScoped<ICityRepository, CityRepository>();
builder.Services.AddScoped<IContactTypeRepository, ContactTypeRepository>();
builder.Services.AddScoped<IMemberContactRepository, MemberContactRepository>();
builder.Services.AddScoped<IMediaUploadRepository, MediaUploadRepository>();
builder.Services.AddScoped<IQuestionRepository, QuestionRepository>();
builder.Services.AddScoped<IQuestionExpectedResponseRepository, QuestionExpectedResponseRepository>();
builder.Services.AddScoped<IQuestionnaireRepository, QuestionnaireRepository>();
builder.Services.AddScoped<IQuestionnaireQsnsRepository, QuestionnaireQsnsRepository>();
builder.Services.AddScoped<IQuestionnaireResponseRepository , QuestionnaireResponseRepository>();
builder.Services.AddScoped<IQuestionnaireResponseLineRepository , QuestionnaireResponseLineRepository>();
builder.Services.AddScoped<IProductDocumentRepository , ProductDocumentsRepository>();
builder.Services.AddScoped<IProductQuestionnaireRepository, ProductQuestionnaireRepository>(); 
builder.Services.AddScoped<IPolicyTypesExpenseRepository , PolicyTypesExpenseRepository>();
builder.Services.AddScoped<ILIRoleRepository, LIRoleRepository>();
builder.Services.AddScoped<IPolicyRepository, PolicyRepository>();
builder.Services.AddScoped<IPolicyBeneficiaryRepository, PolicyBeneficiaryRepository>();
builder.Services.AddScoped<IPolicyBeneficiaryLineRepository,  PolicyBeneficiaryLineRepository>();
builder.Services.AddScoped<IPolicyTypesQuestionnairesRepository, PolicyTypesQuestionnairesRepository>();
builder.Services.AddScoped<IPolicyTypeCommissionRepository, PolicyTypeCommissionRepository>();
builder.Services.AddScoped<IPolicyPremiumRepository, PolicyPremiumRepository>();
builder.Services.AddScoped<IPolicyPremiumLineRepository, PolicyPremiumLineRepository>();
builder.Services.AddScoped<IPaymentProviderRepository, PaymentProviderRepository>();
builder.Services.AddScoped<IMemberBankAccountRepository, MemberBankAccountRepository>();
builder.Services.AddScoped<IPaymentTypeRepository, PaymentTypeRepository>();
builder.Services.AddScoped<IBusinessRuleRepository,BusinessRuleRepository>();
builder.Services.AddScoped<IObjectRuleRepository, ObjectRuleRepository>();
builder.Services.AddScoped<IEmploymentRepository, EmploymentRepository>();
builder.Services.AddScoped<IIntermediaryRepository,  IntermediaryRepository>(); 
builder.Services.AddScoped<IIntermediaryTypeRepository,  IntermediaryTypeRepository>();
builder.Services.AddScoped<IPremiumCollectionConfigHeaderRepository,PremiumCollectionConfigHeaderRepository>();
builder.Services.AddScoped<IHolidayRepository, HolidayRepository>();
builder.Services.AddScoped<IExcelUploadColumnRepository, ExcelUploadColumnRepository>();
builder.Services.AddScoped<IExcelUploadDataRepository, ExcelUploadDataRepository>();
builder.Services.AddScoped <IPolicyCommissionRepository,PolicyCommissionRepository >();
builder.Services.AddScoped<IPolicyPremiumLinesCommissionRepository, PolicyPremiumLinesCommissionRepository>();
builder.Services.AddScoped<IProcessPayments, ProcessPayments>();
builder.Services.AddScoped<IJobsRepository, JobsRepository>();
builder.Services.AddScoped<IBilledPremiumRepository, BilledPremiumRepository>();
builder.Services.AddScoped<IPolicyClaimRepository,PolicyClaimRepository>();
builder.Services.AddScoped <IPolicyClaimsLineRepository,PolicyClaimsLineRepository  >();
builder.Services.AddScoped<IPremiumWaiverRepository, PremiumWaiverRepository>();
builder.Services.AddScoped<IUnitTrustRepository, UnitTrustRepository>();
builder.Services.AddScoped<IUnitsPricesListRepository, UnitsPricesListRepository>();
builder.Services.AddScoped<IPolicyUnitsRepository, PolicyUnitsRepository>();
builder.Services.AddScoped<IPolicyUnitsLinesRepository, PolicyUnitsLinesRepository>();  
builder.Services.AddScoped<IBankRepository,BankRepository>();
builder.Services.AddScoped<IBankBranchRepository, BankBranchRepository>();
builder.Services.AddScoped<IMemberStatusRepository, MemberStatusRepository>();
builder.Services.AddScoped<IStatiiRepository, StatiiRepository>();
builder.Services.AddScoped<IStatiiReasonsRepository, StatiiReasonsRepository>();
builder.Services.AddScoped<IBillingHeaderRepository, BillingHeaderRepository>();
builder.Services.AddScoped<ISuspenseProcessing, SuspenseProcessing>();
builder.Services.AddScoped<IBillingMessageRepository,BillingMessageRepository >();
builder.Services.AddScoped<IExchangeRateRepository,ExchangeRateRepository>();
builder.Services.AddScoped<IReversalHeaderRepository , ReversalHeaderRepository >();
builder.Services.AddScoped<IReversalLineRepository, ReversalLineRepository >();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IPolicyStatiiStagingRepository, PolicyStatiiStagingRepository>();
builder.Services.AddScoped<IObjectRulesStatiiHistoryRepository, ObjectRulesStatiiHistoryRepository>();
builder.Services.AddScoped<IUnitTrustsLineRepository, UnitTrustsLineRepository>();
builder.Services.AddScoped<ICustomValidator, CustomValidator>();
builder.Services.AddScoped<IAuditRepository, AuditRepository>();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Set session timeout
    options.Cookie.HttpOnly = true; // Make the session cookie accessible only through HTTP
    options.Cookie.IsEssential = true; // Ensure the session cookie is always sent
});
var app = builder.Build();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.Use(async (context, next) => { context.Response.Headers.Add("X-Frame-Options", "SAMEORIGIN"); 
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Remove("X-Powered-By"); // Remove X-Powered-By header
    await next(); });

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseEndpoints(endpoints =>
{
    endpoints.MapGet("/Identity/Account/Register", context => Task.Factory.StartNew(() => context.Response.Redirect("/Identity/Account/Login", true, true)));
    endpoints.MapPost("/Identity/Account/Register", context => Task.Factory.StartNew(() => context.Response.Redirect("/Identity/Account/Login", true, true)));
});
app.MapRazorPages();
//await app.CreateRolesAsync(builder.Configuration);
app.Run();
