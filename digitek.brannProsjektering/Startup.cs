using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CamundaClient;
using digitek.brannProsjektering.Models;
using digitek.brannProsjektering.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;

namespace digitek.brannProsjektering
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        readonly string Localhost = "localhost";
        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy(Localhost,
                    builder =>
                    {
                        builder.WithOrigins("http://localhost",
                                "http://www.noko.com")
                            .AllowAnyHeader()
                            .AllowAnyMethod();
                    });
            });

            services.AddDbContext<ApplicationDbContext>(option => option.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            //services.Configure<CookiePolicyOptions>(options =>
            //{
            //    // This lambda determines whether user consent for non-essential cookies is needed for a given request.
            //    options.CheckConsentNeeded = context => true;
            //    options.MinimumSameSitePolicy = SameSiteMode.None;
            //});

            var description = @" # Brann API 

Vi ber om følgende tilleggsinformasjon når du bruker API:
    *	Navn
    *	E-postadresse 
    *	Bedriftsnavn/org.nr.

Behovet er knyttet til muligheten for å føre statistikk over bruk av tjenesten for å dokumentere nytteverdi, undersøke avvik og feil som oppstår, og i den forbindelsen å kunne kontakte brukeren av tjenesten, samt å samle inn informasjon som grunnlag for forbedring av tjenesten funksjonelt eller innholdsmessig. 
Den innsamlede informasjon vil ikke bli brukt til annet enn de her angitte formålene og vil kun være tilgjengelig for DiBK og våre databehandlere. Ønsker du å få slettet informasjonen, så ta kontakt med [Dibk](https://dibk.no/) E-post: digitek@dibk-utvikling.atlassian.net .
Det er helt frivillig å legge inn de 3 elementene, tjenesten vil ikke påvirkes av om de er angitt eller ikke.

**Brann API – kun til test; løsningen er under utvikling!**
*API’et leverer et begrenset uttrekk av krav og preaksepterte ytelser for sikkerhet ved brann i TEK17 med veiledning. 
Unntak fra reglene er ikke tatt med, og uttrekket kan derfor ikke brukes ved prosjektering*
";
            //services.AddCors(); //https://stackoverflow.com/a/44379971
           


            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.AddSwaggerGen(c =>
                {
                    c.SwaggerDoc("v3", new Info { Title = "DigiTEK17", Description = description });
                    var xmlPath = System.AppDomain.CurrentDomain.BaseDirectory + @"digitek.brannProsjektering.xml";
                    c.IncludeXmlComments(xmlPath);
                }
                );

            // get configuration from appsettings.json - use as singleton
            AppSettings appSettings = Configuration.GetSection("AppSettings").Get<AppSettings>();
            services.AddSingleton(appSettings);
            
            services.AddTransient<ICamundaEngineClient, CamundaEngineClient>();
            services.AddTransient<IDbServices, DbServices>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }


            app.UseCors(Localhost);

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            //app.UseCookiePolicy();


            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
            app.UseSwagger();
            app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v3/swagger.json","Core API");
                }
                
                );
        }
    }
}
