using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Vote.Messaging;
using Vote.Workers;

namespace Vote
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
                .AddRazorPagesOptions(options =>
                {
                    options.Conventions.ConfigureFilter(new IgnoreAntiforgeryTokenAttribute());
                });
            
            var loggerFactory = new LoggerFactory()
                .AddConsole();

            services.AddTransient<IMessageQueue, MessageQueue>()
                .AddSingleton(loggerFactory)
                .AddLogging()
                .AddSingleton<IQueueWorker, QueueWorker>();
        }
        
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }
                                 
            app.UseStaticFiles();
            app.UseMvc();

            var worker = app.ApplicationServices.GetService<IQueueWorker>();
            Task.Run(() =>
            {
                worker.Start();
            });
        }
    }
}