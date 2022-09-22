using Application.Common.Interfaces;
using Infrastructure.Persistance;
using System.Data;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {

            services.AddSingleton(InitializeDb());
            services.AddScoped(typeof(IQuizService), typeof(QuizService));

            return services;
        }
        private static IDbConnection InitializeDb()
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            connection.Open();

            // Migrate up
            var assembly = typeof(DependencyInjection).GetTypeInfo().Assembly;
            var migrationResourceNames = assembly.GetManifestResourceNames()
                .Where(x => x.EndsWith(".sql"))
                .OrderBy(x => x);
            if (!migrationResourceNames.Any()) throw new System.Exception("No migration files found!");
            foreach (var resourceName in migrationResourceNames)
            {
                var sql = GetResourceText(assembly, resourceName);
                var command = connection.CreateCommand();
                command.CommandText = sql;
                command.ExecuteNonQuery();
            }

            return connection;
        }

        private static string GetResourceText(Assembly assembly, string resourceName)
        {
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}