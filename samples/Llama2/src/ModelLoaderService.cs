using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace LLama.Web.Services
{
    public class ModelLoaderService : IHostedService 
    {
        private readonly IModelService _modelService;

        public ModelLoaderService(IModelService modelService)
        {
            _modelService = modelService;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
        }
    }
}
