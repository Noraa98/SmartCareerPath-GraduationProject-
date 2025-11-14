using MediatR;

namespace SmartCareerPath.Application.PipelineBehaviors
{
    public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
       where TRequest : IRequest<TResponse>
    {
        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            var requestName = typeof(TRequest).Name;
            Console.WriteLine($"[Request] {requestName} - {DateTime.UtcNow}");

            try
            {
                var response = await next();
                Console.WriteLine($"[Success] {requestName} - {DateTime.UtcNow}");
                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error] {requestName} - {ex.Message}");
                throw;
            }
        }
    }
}
