using System.Collections.Generic;
using System.Threading.Tasks;
using ArNir.Core.DTOs.Intelligence;

namespace ArNir.Services.Interfaces
{
    public interface INotificationService
    {
        Task<List<AlertDto>> GetActiveAlertsAsync(string? provider = null);
    }
}
