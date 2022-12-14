using System.Collections.Generic;
using System.Data;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using QuizService.Model;
using QuizService.Model.Domain;
using System.Linq;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using System.Threading;
using Application.Quizzes.Queries.GetAllQuizzes;

namespace QuizService.Controllers;

[Route("api/quizzes")]
public class QuizController : Controller
{
    private ISender _mediator;
    protected ISender Mediator => _mediator ??= HttpContext.RequestServices.GetService<ISender>();

    //left this field and constructor so the rest of actions works.
    private readonly IDbConnection _connection;

    public QuizController(IDbConnection connection)
    {
        _connection = connection;
    }

    // GET api/quizzes
    [HttpGet]
    public async Task<ActionResult<IEnumerable<QuizDto>>> Get(CancellationToken cancellationToken)
    {
        return Ok(await Mediator.Send(new GetAllQuizesQuery()));
    }

    //First, actions aren't supposed to be filled with all logic. You shouldn't be writing queries in them.
    //You should make controller for each functional logic. Questions should have separate controller, answers too. If you continue adding more features or tables, this one will get too big to maintain.

    //Either make use of mediatR and send requests and just handle response, or make services for each domain object, and costume services, so your action gets max 3-5 lines of code big.

    //Make use of asynchronous features of C# (async/await), so your main thread doesn't get blocked. It matters a lot once you start getting more requests.
    //All network related operations have async implementation, so you should use it.


    // GET api/quizzes/5
    [HttpGet("{id}")]
    public object Get(int id)
    {
        const string quizSql = "SELECT * FROM Quiz WHERE Id = @Id;";
        var quiz = _connection.QuerySingleOrDefault<Quiz>(quizSql, new { Id = id });
        if (quiz == null)
            return NotFound();
        const string questionsSql = "SELECT * FROM Question WHERE QuizId = @QuizId;";
        var questions = _connection.Query<Question>(questionsSql, new { QuizId = id });

        //TODO: Check if there are any questions. Maybe quiz is just made, and there are 0 questions. If there are no questions, we shouldn't query for answers.

        const string answersSql = "SELECT a.Id, a.Text, a.QuestionId FROM Answer a INNER JOIN Question q ON a.QuestionId = q.Id WHERE q.QuizId = @QuizId;";
        var answers = _connection.Query<Answer>(answersSql, new { QuizId = id })
            .Aggregate(new Dictionary<int, IList<Answer>>(), (dict, answer) =>
            {
                if (!dict.ContainsKey(answer.QuestionId))
                    dict.Add(answer.QuestionId, new List<Answer>());
                dict[answer.QuestionId].Add(answer);
                return dict;
            });
        return new QuizResponseModel
        {
            Id = quiz.Id,
            Title = quiz.Title,
            Questions = questions.Select(question => new QuizResponseModel.QuestionItem
            {
                Id = question.Id,
                Text = question.Text,
                Answers = answers.ContainsKey(question.Id)
                    ? answers[question.Id].Select(answer => new QuizResponseModel.AnswerItem
                    {
                        Id = answer.Id,
                        Text = answer.Text
                    })
                    : new QuizResponseModel.AnswerItem[0],
                CorrectAnswerId = question.CorrectAnswerId
            }),
            Links = new Dictionary<string, string>
            {
                {"self", $"/api/quizzes/{id}"},
                {"questions", $"/api/quizzes/{id}/questions"}
            }
        };
    }

    // POST api/quizzes
    [HttpPost]
    public IActionResult Post([FromBody] QuizCreateModel value)
    {
        var sql = $"INSERT INTO Quiz (Title) VALUES('{value.Title}'); SELECT LAST_INSERT_ROWID();";
        var id = _connection.ExecuteScalar(sql);
        return Created($"/api/quizzes/{id}", null);
    }

    // PUT api/quizzes/5
    [HttpPut("{id}")]
    public IActionResult Put(int id, [FromBody] QuizUpdateModel value)
    {
        //By conventions, on put methods, body models also have property Id, and we should check if it is equal to id from route.        
        const string sql = "UPDATE Quiz SET Title = @Title WHERE Id = @Id";
        int rowsUpdated = _connection.Execute(sql, new { Id = id, Title = value.Title });
        if (rowsUpdated == 0)
            return NotFound();
        return NoContent();
    }

    // DELETE api/quizzes/5
    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        const string sql = "DELETE FROM Quiz WHERE Id = @Id";
        int rowsDeleted = _connection.Execute(sql, new { Id = id });
        if (rowsDeleted == 0)
            return NotFound();
        return NoContent();
    }

    // POST api/quizzes/5/questions
    [HttpPost]
    [Route("{id}/questions")]
    public IActionResult PostQuestion(int id, [FromBody] QuestionCreateModel value)
    {
        const string quizSql = "SELECT * FROM Quiz WHERE Id = @Id;";
        var quiz = _connection.QuerySingleOrDefault<Quiz>(quizSql, new { Id = id });
        if (quiz == null)
            return NotFound();

        const string sql = "INSERT INTO Question (Text, QuizId) VALUES(@Text, @QuizId); SELECT LAST_INSERT_ROWID();";
        var questionId = _connection.ExecuteScalar(sql, new { Text = value.Text, QuizId = id });
        return Created($"/api/quizzes/{id}/questions/{questionId}", null);
    }

    // PUT api/quizzes/5/questions/6
    [HttpPut("{id}/questions/{qid}")]
    public IActionResult PutQuestion(int id, int qid, [FromBody] QuestionUpdateModel value)
    {
        //quiz id is not used, thus we should avoid unnecessary variables. This should be fixed by moving questions related features to separate controller
        //TODO: Check if answer with given Id exists.
        const string sql = "UPDATE Question SET Text = @Text, CorrectAnswerId = @CorrectAnswerId WHERE Id = @QuestionId";
        int rowsUpdated = _connection.Execute(sql, new { QuestionId = qid, Text = value.Text, CorrectAnswerId = value.CorrectAnswerId });
        if (rowsUpdated == 0)
            return NotFound();
        return NoContent();
    }

    // DELETE api/quizzes/5/questions/6
    [HttpDelete]
    [Route("{id}/questions/{qid}")]
    public IActionResult DeleteQuestion(int id, int qid)
    {
        //quiz id is not used, thus we should avoid unnecessary variables. This should be fixed by moving questions related features to separate controller
        const string sql = "DELETE FROM Question WHERE Id = @QuestionId";
        _connection.ExecuteScalar(sql, new { QuestionId = qid });
        return NoContent();
    }

    // POST api/quizzes/5/questions/6/answers
    [HttpPost]
    [Route("{id}/questions/{qid}/answers")]
    public IActionResult PostAnswer(int id, int qid, [FromBody] AnswerCreateModel value)
    {
        //TODO: check if question with given id exists before trying to insert answer
        const string sql = "INSERT INTO Answer (Text, QuestionId) VALUES(@Text, @QuestionId); SELECT LAST_INSERT_ROWID();";
        var answerId = _connection.ExecuteScalar(sql, new { Text = value.Text, QuestionId = qid });
        return Created($"/api/quizzes/{id}/questions/{qid}/answers/{answerId}", null);
    }

    // PUT api/quizzes/5/questions/6/answers/7
    [HttpPut("{id}/questions/{qid}/answers/{aid}")]
    public IActionResult PutAnswer(int id, int qid, int aid, [FromBody] AnswerUpdateModel value)
    {
        const string sql = "UPDATE Answer SET Text = @Text WHERE Id = @AnswerId";
        int rowsUpdated = _connection.Execute(sql, new { AnswerId = qid, Text = value.Text });
        if (rowsUpdated == 0)
            return NotFound();
        return NoContent();
    }

    // DELETE api/quizzes/5/questions/6/answers/7
    [HttpDelete]
    [Route("{id}/questions/{qid}/answers/{aid}")]
    public IActionResult DeleteAnswer(int id, int qid, int aid)
    {
        //TODO: check if this answer was correct answer for some question. If so, update correctAnswerId of that question to null.
        const string sql = "DELETE FROM Answer WHERE Id = @AnswerId";
        _connection.ExecuteScalar(sql, new { AnswerId = aid });
        return NoContent();
    }
}