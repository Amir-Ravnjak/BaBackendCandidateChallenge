using Domain.Entities;

namespace Application.Common.Interfaces
{
    public interface IQuizService
    {
        Task<IEnumerable<Quiz>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Quiz> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<Quiz> CreateAsync(Quiz quiz, CancellationToken cancellationToken = default);
        Task<Quiz> UpdateAsync(Quiz quiz, CancellationToken cancellationToken = default);
        Task<Quiz> DeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}
