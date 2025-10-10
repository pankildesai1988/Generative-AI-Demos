using ArNir.Core.DTOs.Feedback;
using ArNir.Core.Entities;
using ArNir.Data;
using ArNir.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ArNir.Services
{
    public class FeedbackService : IFeedbackService
    {
        private readonly ArNirDbContext _context;

        public FeedbackService(ArNirDbContext context)
        {
            _context = context;
        }

        public async Task<FeedbackDto> AddFeedbackAsync(FeedbackDto dto)
        {
            var entity = new Feedback
            {
                HistoryId = dto.HistoryId,
                Rating = dto.Rating,
                Comments = dto.Comments
            };

            _context.Feedbacks.Add(entity);
            await _context.SaveChangesAsync();

            return new FeedbackDto
            {
                HistoryId = entity.HistoryId,
                Rating = entity.Rating,
                Comments = entity.Comments
            };
        }

        public async Task<IEnumerable<FeedbackDto>> GetAllAsync()
        {
            var feedbacks = await _context.Feedbacks
                .OrderByDescending(f => f.CreatedAt)
                .Select(f => new FeedbackDto
                {
                    HistoryId = f.HistoryId,
                    Rating = f.Rating,
                    Comments = f.Comments
                })
                .ToListAsync();

            return feedbacks;
        }

        public async Task<double> GetAverageRatingAsync()
        {
            if (!await _context.Feedbacks.AnyAsync()) return 0;
            return await _context.Feedbacks.AverageAsync(f => f.Rating);
        }
    }
}
