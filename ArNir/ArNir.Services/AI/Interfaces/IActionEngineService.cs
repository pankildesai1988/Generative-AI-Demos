using System.Threading.Tasks;

namespace ArNir.Services.Interfaces
{
    /// <summary>
    /// Executes backend contextual actions inferred from chat intelligence.
    /// </summary>
    public interface IActionEngineService
    {
        /// <summary>
        /// Executes the given contextual action (intent) with optional parameters.
        /// </summary>
        /// <param name="intent">Action intent keyword, e.g. 'compare_models', 'view_trends', 'sla_summary'.</param>
        /// <param name="parameter">Optional parameter to pass to the action handler.</param>
        Task<object?> ExecuteActionAsync(string intent, string? parameter = null);
    }
}
