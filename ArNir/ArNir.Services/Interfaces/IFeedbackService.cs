using ArNir.Core.DTOs.Feedback;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArNir.Services.Interfaces
{
    public interface IFeedbackService
    {
        Task<FeedbackDto> AddFeedbackAsync(FeedbackDto dto);
        Task<IEnumerable<FeedbackDto>> GetAllAsync();
        Task<double> GetAverageRatingAsync();
    }

}
