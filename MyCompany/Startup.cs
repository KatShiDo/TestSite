using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MyCompany.Domain;
using MyCompany.Domain.Repositories.Abstract;
using MyCompany.Domain.Repositories.EntityFramework;
using MyCompany.Service;

namespace MyCompany
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public Startup(IConfiguration configuration) => Configuration = configuration;

        public void ConfigureServices(IServiceCollection services)
        {
            //подключение конфига из appsettings.json
            Configuration.Bind("Project", new Config());
            
            //подключение нужного функционала приложения в качестве сервисов
            services.AddTransient<ItextFieldsRepository, EFTextFieldsRepository>();
            services.AddTransient<IServiceItemsRepository, EFServiceItemsRepository>();
            services.AddTransient<DataManager>();
            
            //подключение контекста БД
            services.AddDbContext<AppDbContext>(x => x.UseSqlServer(Config.ConnectionString));
                
            //настройка identity системы
            services.AddIdentity<IdentityUser, IdentityRole>(opts =>
            {
                opts.User.RequireUniqueEmail = true;
                opts.Password.RequiredLength = 6;
                opts.Password.RequireNonAlphanumeric = false;
                opts.Password.RequireLowercase = false;
                opts.Password.RequireUppercase = false;
                opts.Password.RequireDigit = false;
            }).AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders();
            
            //настройка authentication cookie
            services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.Name = "myCompanyAuth";
                options.Cookie.HttpOnly = true;
                options.LoginPath = "/account/login";
                options.AccessDeniedPath = "/account/accessdenied";
                options.SlidingExpiration = true;
            });

            //добавление поддержки контроллеров и представлений (MVC)
            services.AddControllersWithViews()
                //добавление совместимости с последней версией asp.net core mvc
                .SetCompatibilityVersion(CompatibilityVersion.Latest).AddSessionStateTempDataProvider();
        }
        
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            //в процессе разработки важно видеть подробную информацию об ошибках
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            
            //подключение поддержки статичных файлов в приложении (css, js и т.д.)
            app.UseStaticFiles();

            //подключение системы маршрутизации
            app.UseRouting();
            
            //подключение аутентификации и авторизации
            app.UseCookiePolicy();
            app.UseAuthentication();
            app.UseAuthorization();

            //регистрация нужных маршрутов (эндпоинтов)
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}