using MediatR;

namespace TripEnjoy.Application.Common.Interfaces;


public interface IAuditableCommand<out TResponse> : IRequest<TResponse>
{
    // In the future, you could add properties here to provide more context for auditing.
    // For example: 
    // Guid GetEntityId();
    // string GetAuditDescription();
}
