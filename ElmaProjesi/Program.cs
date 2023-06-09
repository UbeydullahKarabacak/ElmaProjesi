using ElmaProjesi.BusinessLayer.Abstract;
using ElmaProjesi.BusinessLayer.Concrete;
using ElmaProjesi.DataAccessLayer.Abstract;
using ElmaProjesi.DataAccessLayer.Concrete;
using ElmaProjesi.WebUI.EmailServices;
using ElmaProjesi.WebUI.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ElmaProjesi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddDbContext<ApplicationContext>(option => option.UseSqlServer("Server=DESKTOP-TUMHS1A\\NA;Database=ElmaProject;Integrated Security=true"));
            builder.Services.AddIdentity<User, IdentityRole>().AddEntityFrameworkStores<ApplicationContext>().AddDefaultTokenProviders();

            builder.Services.Configure<IdentityOptions>(options =>
            {
                // password
                options.Password.RequireDigit = true;   // password^de mutlaka say�sal bir de�er olmal�.
                options.Password.RequireLowercase = true; //password'de mutlaka k���k harf olmal�.
                options.Password.RequireUppercase = true;   // password'de mutlaka b�y�k harf olmal�.
                options.Password.RequiredLength = 6;    // password en az 6 karakter olmal�.
                options.Password.RequireNonAlphanumeric = true; // rakam ve harf d���nda farkl� bir karakterin de password i�inde olmas� gerekiyor. �rn: nokta gibi, @ gibi, %,-,_ gibi karakterler...

                // lockout : Kullan�c� hesab�n�n klilitlenip kilitlenmemsi ile ilgili.
                options.Lockout.MaxFailedAccessAttempts = 5;    // yanl�� parolay� 5 kere girilebilir. Sonra hesap kilitlenir.
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5); // Hesap kilitlendikten 5 dakika sonra kullan�c� giri� yapmay� deneyebilir.

                //User
                options.User.RequireUniqueEmail = true;   // her kullan�c� tek bir email adresi ile sisteme girebilir. Yani uniq email kullan�l�r. Ayn� email ile 2 hesap a��lamaz.
                options.SignIn.RequireConfirmedEmail = false; // true olursa kullan�c� �ye olur fakat email'ini mutlaka onaylamas� gerekir. false olursa �ye olup hemen sisteme girebilir.
                options.SignIn.RequireConfirmedPhoneNumber = false; // True olursa telefon bilgisi i�in onay ister

            });

            // 4-1: Cookie ayarlar�: Cookie (�erez): Kullan�c�n�n taray�c�s�na b�rak�lan bir bilgi diyebiliriz k�saca. 
            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Account/Login";   // sisteme login olmad�ysak bizi login sayfas�na y�nlendiriyor. login olduysak da bize e�siz bir say� �retiyor.. Bu say�y� server taraf�nda session'da tutuluyor, Cilent taraf�nda ise cookie i�inde tutluyor.. Kullan�c� bir i�lem yapt�ktan sonra belirli bir s�re sonunda bu bilgi siliniyor. Belirtilen �zelliklere g�re, bu veri belirtilen s�re i�inde tekrar bir i�lem yap�l�rsa, tekrar login olmam�za gerek kalm�yor. Fakat s�re bittikten sonra tekrar login olmam�z gerekiyor.
                options.LogoutPath = "/Account/Logout";  //��k�� i�lemi yapt���mda cookie taray�c�dan silinecek Ve tekrar bir i�lem yapmak istedi�imde login sayfas�na y�nlendirilece�im.
                options.AccessDeniedPath = "/Account/Accessdenied"; // Yetkisiz i�lem yap�ld���nda �al��acak olan Action.. �rne�in s�radan bir kullan�c� Admin ile ilgili bir sayfaya ula�maya �al��t���nda �al��acak.

                options.SlidingExpiration = true; // �rne�in sisteme girdim i�lem yapt�m ve bekledim. varsay�lan de�er 20dakika. 20dakikadan sonra cookie'de bu bilgi silenecek. E�er 20 dakika i�inde tekrardan bir i�lem yaparsam bu s�re tekrardan 20 dakika olarak ayarlanacak. False olursa login olduktan sonra 20 dakika sonunda cookie silinecektir.
                options.ExpireTimeSpan = TimeSpan.FromMinutes(300); // default s�resi 20dakika..

                options.Cookie = new CookieBuilder
                { HttpOnly = true, Name = ".Elma.Security.Cookie" };
                // HttpOnly = true sadece http ile istek geldi�inde ula��labilir olsun diyoruz.
                // Name propertry'si ile de Cookie'ye �zel bir isim verebiliyoruz.
            });

            // IoC Container

            builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
            builder.Services.AddScoped<ICategoryService, CategoryManager>();

            builder.Services.AddScoped<ISubCategoryRepository, SubCategoryRepository>();
            builder.Services.AddScoped<ISubCategoryService, SubCategoryManager>();

            builder.Services.AddScoped<IFilterRepository, FilterRepository>();
            builder.Services.AddScoped<IFilterService, FilterManager>();

            //Email Settings
            builder.Services.AddScoped<IEmailSender, EmailSender>(x =>
            new EmailSender("smtp.office365.com", 587, true, "deneme1246435@hotmail.com", "d1246435*")
            );

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            else
            {
                MyInitialData.Seed();
            }


            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllerRoute(
                name: "admin",
                pattern: "admin/addcategory",
                defaults: new { controller = "Admin", action = "CategoryCreate" }
                );

            app.MapControllerRoute(
                name: "admin",
                pattern: "admin/categories",
                defaults: new { controller = "Admin", action = "CategoryList" }
                );

            app.MapControllerRoute(
                name: "categories",
                pattern: "/categories",
                defaults: new { controller = "Category", action = "Index" }
                );

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}