﻿using System;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using AzureStorage.Tables;
using Common.Log;
using Lykke.Common.ApiLibrary.Middleware;
using Lykke.Common.ApiLibrary.Swagger;
using Lykke.Logs;
using Lykke.Service.IcoApi.Core.Services;
using Lykke.Service.IcoApi.Core.Settings;
using Lykke.Service.IcoApi.Modules;
using Lykke.SettingsReader;
using Lykke.SlackNotification.AzureQueue;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Lykke.Service.IcoApi.Infrastructure;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Microsoft.AspNetCore.Http.Features;
using Lykke.AzureStorage.Tables.Entity.Metamodel;
using Lykke.AzureStorage.Tables.Entity.Metamodel.Providers;
using Stripe;

namespace Lykke.Service.IcoApi
{
    public class Startup
    {
        public IHostingEnvironment Environment { get; }
        public IContainer ApplicationContainer { get; private set; }
        public IConfigurationRoot Configuration { get; }
        public ILog Log { get; private set; }

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddEnvironmentVariables();
            Configuration = builder.Build();

            Environment = env;
        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            try
            {
                services
                    .AddMemoryCache()
                    .AddMvc()
                    .AddJsonOptions(options =>
                    {
                        options.SerializerSettings.Converters.Add(new StringEnumConverter { CamelCaseText = true });
                        options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                    });

                services.Configure<FormOptions>(x =>
                {
                    x.ValueLengthLimit = int.MaxValue;
                    x.MultipartBodyLengthLimit = int.MaxValue; // In case of multipart
                });

                services.AddSwaggerGen(options =>
                {
                    options.DefaultLykkeConfiguration("v1", "Ico API");
                    options.DescribeAllEnumsAsStrings();
                    options.DescribeStringEnumsInCamelCase();
                    options.OperationFilter<AddSwaggerInvestorAuthHeaderParameter>();
                    options.OperationFilter<AddSwaggerAdminAuthHeaderParameter>();
                    options.OperationFilter<FormFileOperationFilter>();
                });

                services.AddCors();

                EntityMetamodel.Configure(new AnnotationsBasedMetamodelProvider());

                var builder = new ContainerBuilder();
                var appSettings = Configuration.LoadSettings<AppSettings>();

                Log = CreateLogWithSlack(services, appSettings);

                builder.RegisterInstance(Log)
                    .As<ILog>()
                    .SingleInstance();

                builder.RegisterModule(new ServiceModule(appSettings.Nested(x => x.IcoApiService), Log));
                builder.Populate(services);
                ApplicationContainer = builder.Build();

                StripeConfiguration.SetApiKey(appSettings.CurrentValue.IcoApiService.StripeSecretKey);

                return new AutofacServiceProvider(ApplicationContainer);
            }
            catch (Exception ex)
            {
                Log?.WriteFatalErrorAsync(nameof(Startup), nameof(ConfigureServices), "", ex).Wait();
                throw;
            }
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplicationLifetime appLifetime)
        {
            try
            {
                if (env.IsDevelopment())
                {
                    app.UseDeveloperExceptionPage();
                }

                app.UseLykkeMiddleware("IcoApi", ex => new {Message = "Technical problem"});

                app.UseCors(builder => builder
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader());

                app.UseMvc();
                app.UseSwagger();
                app.UseSwaggerUI(x =>
                {
                    x.RoutePrefix = "swagger/ui";
                    x.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
                });
                app.UseStaticFiles(new StaticFileOptions()
                {
                    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(
                            System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), @"Html")),
                    RequestPath = new Microsoft.AspNetCore.Http.PathString("/html")
                });

                appLifetime.ApplicationStarted.Register(() => StartApplication().Wait());
                appLifetime.ApplicationStopping.Register(() => StopApplication().Wait());
                appLifetime.ApplicationStopped.Register(() => CleanUp().Wait());
            }
            catch (Exception ex)
            {
                Log?.WriteFatalErrorAsync(nameof(Startup), nameof(ConfigureServices), "", ex).Wait();
                throw;
            }
        }

        private async Task StartApplication()
        {
            try
            {
                await ApplicationContainer.Resolve<IStartupManager>().StartAsync();

                await Log.WriteMonitorAsync("", "", "Started");
            }
            catch (Exception ex)
            {
                await Log.WriteFatalErrorAsync(nameof(Startup), nameof(StartApplication), "", ex);
                throw;
            }
        }

        private async Task StopApplication()
        {
            try
            {
                await ApplicationContainer.Resolve<IShutdownManager>().StopAsync();
            }
            catch (Exception ex)
            {
                if (Log != null)
                {
                    await Log.WriteFatalErrorAsync(nameof(Startup), nameof(StopApplication), "", ex);
                }
                throw;
            }
        }

        private async Task CleanUp()
        {
            try
            {
                // NOTE: Service can't recieve and process requests here, so you can destroy all resources
                
                if (Log != null)
                {
                    await Log.WriteMonitorAsync("", "", "Terminating");
                }
                
                ApplicationContainer.Dispose();
            }
            catch (Exception ex)
            {
                if (Log != null)
                {
                    await Log.WriteFatalErrorAsync(nameof(Startup), nameof(CleanUp), "", ex);
                    (Log as IDisposable)?.Dispose();
                }
                throw;
            }
        }

        private static ILog CreateLogWithSlack(IServiceCollection services, IReloadingManager<AppSettings> settings)
        {
            var consoleLogger = new LogToConsole();
            var aggregateLogger = new AggregateLogger();

            aggregateLogger.AddLog(consoleLogger);

            // Creating slack notification service, which logs own azure queue processing messages to aggregate log
            var slackService = services.UseSlackNotificationsSenderViaAzureQueue(new AzureQueueIntegration.AzureQueueSettings
            {
                ConnectionString = settings.CurrentValue.SlackNotifications.AzureQueue.ConnectionString,
                QueueName = settings.CurrentValue.SlackNotifications.AzureQueue.QueueName
            }, aggregateLogger);

            var dbLogConnectionStringManager = settings.Nested(x => x.IcoApiService.Db.LogsConnString);
            var dbLogConnectionString = dbLogConnectionStringManager.CurrentValue;

            // Creating azure storage logger, which logs own messages to concole log
            if (!string.IsNullOrEmpty(dbLogConnectionString) && !(dbLogConnectionString.StartsWith("${") && dbLogConnectionString.EndsWith("}")))
            {
                var persistenceManager = new LykkeLogToAzureStoragePersistenceManager(
                    AzureTableStorage<LogEntity>.Create(dbLogConnectionStringManager, "IcoApiLog", consoleLogger),
                    consoleLogger);

                var slackNotificationsManager = new LykkeLogToAzureSlackNotificationsManager(slackService, consoleLogger);

                var azureStorageLogger = new LykkeLogToAzureStorage(
                    persistenceManager,
                    slackNotificationsManager,
                    consoleLogger);

                azureStorageLogger.Start();

                aggregateLogger.AddLog(azureStorageLogger);
            }

            aggregateLogger.AddIcoSlackLog(slackService);

            return aggregateLogger;
        }
    }
}
