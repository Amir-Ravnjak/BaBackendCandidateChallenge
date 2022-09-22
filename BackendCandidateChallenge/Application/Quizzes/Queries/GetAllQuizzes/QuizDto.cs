using Application.Common.Mappings;
using Domain.Entities;

namespace Application.Quizzes.Queries.GetAllQuizzes
{
    public class QuizDto : IMapFrom<Quiz>
    {

        public long Id { get; set; }
        public string Title { get; set; }
    }
}
