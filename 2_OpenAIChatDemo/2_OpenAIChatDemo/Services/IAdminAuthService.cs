using _2_OpenAIChatDemo.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace _2_OpenAIChatDemo.Services
{
    public interface IAdminAuthService
    {
        Task<bool> ValidateUserAsync(string username, string password);
    }
}
