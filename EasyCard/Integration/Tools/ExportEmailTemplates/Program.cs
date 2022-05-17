using BasicServices;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace ExportEmailTemplates
{
    class Program
    {
        static void Main(string[] args)
        {
            IConfiguration Configuration = new ConfigurationBuilder()
              .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
              .Build();

            var appConfig = Configuration.GetSection("AppConfig").Get<ApplicationSettings>();

            var emailTemplateService = new EmailTemplatesStorageService(appConfig.DefaultStorageConnectionString);

            foreach (var template in emailTemplateService.GetAll().Result)
            {
                Console.WriteLine(template.TemplateCode);

                var fileName = Path.Combine(args[0], $"{template.TemplateCode}.emailtemplate.txt");

                File.WriteAllText(fileName, template.BodyTemplate);
            }
        }
    }
}
