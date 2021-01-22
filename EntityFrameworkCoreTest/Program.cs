using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore;
using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using System.Collections.Generic;
using EntityFrameworkCoreTest.Data;
using EntityFrameworkCoreTest.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Debug;

namespace EntityFrameworkCoreTest
{
    class Program
    {
        static void Main(string[] args)
        {
            // 此程序运行后创建数据库并添加数据，不能通过执行命令创建库
            var host = CreateWebHostingBuilder(args).Build();

            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var context = services.GetRequiredService<TestContext>();

                // context.Database.EnsureDeleted();
                // context.Database.EnsureCreated();

                // DbInit.Init(context);
                //Console.WriteLine(context.Database.GenerateCreateScript());

                // context.Users.ForEachAsync(t =>
                //{
                //    Console.WriteLine($"Id={t.Id},name={t.Name},age={t.Age},birthday={t.Birthday}");
                //});

                //测试 datetime条件
                var list = context.Users.Where(t => t.Birthday >= DateTime.Now.AddYears(-2) && t.Birthday < DateTime.Now).ToList();

                list.ForEach(t =>
                    {
                        Console.WriteLine($"Id={t.Id},name={t.Name},age={t.Age},birthday={t.Birthday}");
                    });
            }

            host.Run();
        }
        public static IWebHostBuilder CreateWebHostingBuilder(string[] args) => WebHost.CreateDefaultBuilder(args).UseStartup<Startup>();
    }

    public class Startup
    {
        //日志工厂记录生成的sql
        public static readonly LoggerFactory MyLoggerFactory = new LoggerFactory(new[] {
            new DebugLoggerProvider()
        });
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<TestContext>(options =>
                     options.UseLoggerFactory(MyLoggerFactory)
                     .UseSqlServer(@"Server = .\SQLEXPRESS;Database = Test;User ID = sa;Password = sa;Trusted_Connection = False;"));
        }

        public void Configure(IHostingEnvironment env, IApplicationBuilder app)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }
        }
    }

    public static class DbInit
    {
        public static void Init(TestContext context)
        {
            if (context.Users.Any())
            {
                return;   // DB has been seeded
            }

            var users = new List<User>()
            {
                new User(){Name="test1",Age=22,Birthday=DateTime.Parse("2018-4-1")},
                new User(){Name="test2",Age=23,Birthday=DateTime.Parse("2017-4-1")},
                new User(){Name="test3",Age=25,Birthday=DateTime.Parse("2016-4-1")}
            };

            users.ForEach(t =>
            {
                context.Users.Add(t);
            });

            context.SaveChanges();
        }
    }
}
