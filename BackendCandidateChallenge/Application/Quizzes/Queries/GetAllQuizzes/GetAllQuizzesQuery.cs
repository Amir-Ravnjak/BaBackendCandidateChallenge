using Application.Common.Interfaces;
using AutoMapper;
using MediatR;

namespace Application.Quizzes.Queries.GetAllQuizzes
{
    public class GetAllQuizesQuery : IRequest<IEnumerable<QuizDto>>
    {
    }
    public class GetAllQuizesQueryHandler : IRequestHandler<GetAllQuizesQuery, IEnumerable<QuizDto>>
    {
        private readonly IQuizService _quizService;
        private readonly IMapper _mapper;

        public GetAllQuizesQueryHandler(IQuizService quizService, IMapper mapper)
        {
            _quizService = quizService;
            _mapper = mapper;
        }
        public async Task<IEnumerable<QuizDto>> Handle(GetAllQuizesQuery request, CancellationToken cancellationToken)
        {
            var quizzes = await _quizService.GetAllAsync(cancellationToken);
            return _mapper.Map<IEnumerable<QuizDto>>(quizzes);
        }
    }
}
