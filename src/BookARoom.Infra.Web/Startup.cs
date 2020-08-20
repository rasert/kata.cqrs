using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookARoom.Domain;
using BookARoom.Domain.ReadModel;
using BookARoom.Infra.MessageBus;
using BookARoom.Infra.ReadModel.Adapters;
using BookARoom.Infra.WriteModel;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BookARoom.Infra.Web
{
    public class Startup
    {
        private IWebHostEnvironment env;
        
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            this.env = env;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Build the WRITE side (we use a bus for it) ----------
            var bus = new FakeBus();
            var bookingRepository = new BookingAndClientsRepository();

            var bookingHandler = CompositionRootHelper.BuildTheWriteModelHexagon(bookingRepository, bookingRepository, bus, bus);

            // Build the READ side ---------------------------------
            var hotelsAdapter = new HotelsAndRoomsAdapter($"{env.WebRootPath}/hotels/", bus);
            hotelsAdapter.LoadAllHotelsFiles();
            
            var readFacade = CompositionRootHelper.BuildTheReadModelHexagon(hotelsAdapter, hotelsAdapter, null, bus);

            // Registers all services to the MVC IoC framework
            services.AddSingleton<ISendCommands>(bus);
            services.AddSingleton<IQueryBookingOptions>(readFacade);
            services.AddSingleton<IProvideHotel>(readFacade);
            services.AddSingleton<IProvideReservations>(readFacade);

            // Add framework services.
            // services.AddMvc();
            services.AddControllersWithViews();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            // app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            // app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}