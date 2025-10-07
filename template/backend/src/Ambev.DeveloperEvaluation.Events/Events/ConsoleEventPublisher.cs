using Ambev.DeveloperEvaluation.Domain.Common.Interfaces;

namespace Ambev.DeveloperEvaluation.Events.Events
{
    public class ConsoleEventPublisher : IEventPublisher
    {
        public Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken)
        {
            Console.WriteLine($"[EVENT] {typeof(TEvent).Name} published at {DateTime.UtcNow}");
            return Task.CompletedTask;
        }
    }
}
