using BLL.DTO;
using MODEL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BLL.DTO.QuizDtos;

namespace BLL.Services.Abstract
{
    public interface IQuizService
    {
        Task<StartQuizResult> StartAsync(StartQuizRequest request, CancellationToken ct = default);
        Task<QuizQuestionDto?> GetNextQuestionAsync(int quizRunId, CancellationToken ct = default);
        Task<SubmitAnswerResult> SubmitAnswerAsync(SubmitAnswerRequest request, CancellationToken ct = default);
        Task<QuizSummaryDto> FinishAsync(int quizRunId, CancellationToken ct = default);
        Task ChangeModeAsync(int quizRunId, QuizMode newMode, CancellationToken ct = default);
        Task<QuizMode> GetRunModeAsync(int quizRunId, CancellationToken ct = default);
        Task<WordHintDto> GetHintsAsync(int quizRunId, int englishWordId, CancellationToken ct = default);
        Task<StartQuizResult> StartFromSeedAsync(StartFromSeedRequest req, CancellationToken ct = default);
        Task<List<QuizRunSummaryDto>> GetRecentRunsAsync(int take = 20, CancellationToken ct = default);
        Task<QuizRunDetailDto> GetRunDetailAsync(int quizRunId, CancellationToken ct = default);
        Task SetRunPracticeAsync(int quizRunId, bool isPractice, CancellationToken ct = default);
        Task DeleteRunAsync(int quizRunId, CancellationToken ct = default);

    }

}

