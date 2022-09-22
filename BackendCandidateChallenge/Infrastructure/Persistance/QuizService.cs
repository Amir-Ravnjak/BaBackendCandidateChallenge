using Application.Common.Interfaces;
using Dapper;
using Domain.Entities;
using System.Data;

namespace Infrastructure.Persistance
{
    public class QuizService : IQuizService
    {
        private readonly IDbConnection _connection;

        public QuizService(IDbConnection connection)
        {
            _connection = connection;
        }

        public async Task<IEnumerable<Quiz>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var sqlQuery = "SELECT * FROM Quiz";
            var queryCommand = new CommandDefinition(sqlQuery, cancellationToken: cancellationToken);
            return await _connection.QueryAsync<Quiz>(queryCommand);
        }

        public Task<Quiz> CreateAsync(Quiz quiz, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Quiz> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Quiz> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Quiz> UpdateAsync(Quiz quiz, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
