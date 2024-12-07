using ECommerce_web.Areas.Admin.Repository;
using ECommerce_web.Models;
using ECommerce_web.Models.Momo;
using ECommerce_web.Repository;
using ECommerce_web.Services.Momo;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;



internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        //Connect MomoAPI
        builder.Services.Configure<MomoOptionModel>(builder.Configuration.GetSection("MomoAPI"));
        builder.Services.AddScoped<IMomoService, MomoService>();

        // Connection db
        builder.Services.AddDbContext<DataContext>(options =>
        {
            options.UseSqlServer(builder.Configuration["ConnectionStrings:ConnectedDb"]);
        });
        
        //Add Email Sender
        builder.Services.AddTransient<IEmailSender, EmailSender>();
       
        // Add services to the container.
        builder.Services.AddControllersWithViews();

        builder.Services.AddDistributedMemoryCache();

        builder.Services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromMinutes(30);
            options.Cookie.IsEssential = true;
        });

        builder.Services.AddIdentity<AppUserModel,IdentityRole>()
        .AddEntityFrameworkStores<DataContext>().AddDefaultTokenProviders();

        builder.Services.AddRazorPages();

        builder.Services.Configure<IdentityOptions>(options =>
        {
            // Password settings.
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = false;
            options.Password.RequiredLength = 6;
            options.User.RequireUniqueEmail = true;
        });

        //Configuration Login Google Account
        builder.Services.AddAuthentication(options =>
        {
            //options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            //options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            //options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        }).AddCookie().AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
        {
            options.ClientId = builder.Configuration.GetSection("GoogleKeys:ClientId").Value;
            options.ClientSecret = builder.Configuration.GetSection("GoogleKeys:ClientSecret").Value;
        });

        var app = builder.Build();
        app.UseStatusCodePagesWithRedirects("/Home/Error?statuscode={0}");

        app.UseSession();
        app.UseStaticFiles();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
   
        }

        app.UseRouting();

        app.UseStaticFiles();

        app.UseAuthentication(); //Đăng nhập trước

        app.UseAuthorization(); //Kiểm tra quyền sau

   

        app.MapControllerRoute(
            name: "Areas",
            pattern: "{area:exists}/{controller=Product}/{action=Index}/{id?}");

        app.MapControllerRoute(
            name: "category",
            pattern: "/category/{Slug?}",
            defaults: new { controller = "Category", action = "Index" });

        app.MapControllerRoute(
            name: "brand",
            pattern: "/brand/{Slug?}",
            defaults: new { controller = "Brand", action = "Index" });

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        //Sedding Data
        var context = app.Services.CreateScope().ServiceProvider.GetRequiredService<DataContext>();
        SeedData.SeedingData(context);

        app.Run();
    }
}