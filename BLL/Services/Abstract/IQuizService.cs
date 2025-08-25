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

        // Bir sonraki soruyu üret (seed setinden döngü mantığını bir sonraki adımda tamamlarız)
        Task<QuizQuestionDto?> GetNextQuestionAsync(int quizRunId, CancellationToken ct = default);

        Task<SubmitAnswerResult> SubmitAnswerAsync(SubmitAnswerRequest request, CancellationToken ct = default);

        Task<QuizSummaryDto> FinishAsync(int quizRunId, CancellationToken ct = default);

        Task ChangeModeAsync(int quizRunId, QuizMode newMode, CancellationToken ct = default);
        Task<QuizMode> GetRunModeAsync(int quizRunId, CancellationToken ct = default);

        Task<WordHintDto> GetHintsAsync(int englishWordId, CancellationToken ct = default);

        Task<WordHintDto> GetHintsAsync(int quizRunId, int englishWordId, CancellationToken ct = default);

    }
}
