using LLama.Abstractions;
using System.Threading.Tasks;

namespace LLama.Web.Services
{
    /// <summary>
    /// Service for managing language Models
    /// </summary>
    public interface IModelService
    {
        ILLamaExecutor GetExecutor();
    }
}