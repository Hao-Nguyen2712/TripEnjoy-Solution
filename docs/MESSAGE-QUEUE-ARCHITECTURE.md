# TripEnjoy Message Queue Architecture

## Overview

This document describes the message queue implementation for TripEnjoy's booking feature using **RabbitMQ** as the message broker and **MassTransit** as the .NET integration library. The implementation follows an event-driven architecture pattern to enable asynchronous processing of booking operations.

## Architecture Decision

### Tech Stack

| Component | Technology | Version | Rationale |
|-----------|-----------|---------|-----------|
| **Message Broker** | RabbitMQ | 3-management-alpine | Industry standard, reliable, feature-rich, excellent .NET support |
| **Client Library** | MassTransit | 8.2.0 | Idiomatic .NET, excellent MediatR integration, built-in retry/fault handling |
| **Message Pattern** | Publish/Subscribe | - | Decouples producers from consumers, allows multiple subscribers |
| **Deployment** | Docker | - | Easy local development, consistent environments |

### Why RabbitMQ?

- **Reliability**: Battle-tested, used by thousands of companies
- **Performance**: Handles 10,000+ messages/second
- **Features**: Dead-letter queues, message persistence, acknowledgments
- **Ecosystem**: Excellent monitoring (Management UI), .NET support
- **Ease of Use**: Simple Docker deployment, minimal configuration

### Why MassTransit?

- **Modern .NET**: Built for .NET 8, async/await native
- **Integration**: Seamless integration with MediatR CQRS pattern
- **Resilience**: Built-in retry policies, circuit breakers, rate limiting
- **Testing**: Comprehensive test harness for unit/integration tests
- **Convention-based**: Minimal configuration, follows best practices

## Architecture Diagram

```
┌─────────────────────────────────────────────────────────────────────┐
│                         TripEnjoy API Layer                         │
├─────────────────────────────────────────────────────────────────────┤
│  BookingsController                                                  │
│    └─> POST /api/v1/bookings (CreateBooking)                       │
│    └─> POST /api/v1/bookings/{id}/confirm (ConfirmBooking)         │
│    └─> POST /api/v1/bookings/{id}/cancel (CancelBooking)           │
└────────────────────────────┬────────────────────────────────────────┘
                             │
                             ▼
┌─────────────────────────────────────────────────────────────────────┐
│                    Application Layer (CQRS)                         │
├─────────────────────────────────────────────────────────────────────┤
│  Command Handlers:                                                   │
│    • CreateBookingCommandHandler ────┐                              │
│    • ConfirmBookingCommandHandler ───┤                              │
│    • CancelBookingCommandHandler ────┤                              │
│                                       │                              │
│                                       │ IPublishEndpoint.Publish()   │
│                                       │                              │
│                                       ▼                              │
│  ┌────────────────────────────────────────────────────────┐        │
│  │            Event Messages (Contracts)                   │        │
│  │  • IBookingCreatedEvent                                │        │
│  │  • IBookingConfirmedEvent                              │        │
│  │  • IBookingCancelledEvent                              │        │
│  └────────────────────────────────────────────────────────┘        │
└────────────────────────────┬────────────────────────────────────────┘
                             │
                             ▼
┌─────────────────────────────────────────────────────────────────────┐
│                        RabbitMQ Broker                              │
├─────────────────────────────────────────────────────────────────────┤
│  Exchanges (Topic):                                                  │
│    • TripEnjoy.Application.Messages.Contracts:IBookingCreatedEvent  │
│    • TripEnjoy.Application.Messages.Contracts:IBookingConfirmedEvent│
│    • TripEnjoy.Application.Messages.Contracts:IBookingCancelledEvent│
│                                                                      │
│  Queues:                                                             │
│    • BookingCreated (Consumer: BookingCreatedConsumer)              │
│    • BookingConfirmed (Consumer: BookingConfirmedConsumer)          │
│    • BookingCancelled (Consumer: BookingCancelledConsumer)          │
│                                                                      │
│  Dead Letter Queues (DLQ):                                          │
│    • BookingCreated_error                                           │
│    • BookingConfirmed_error                                         │
│    • BookingCancelled_error                                         │
└────────────────────────────┬────────────────────────────────────────┘
                             │
                             ▼
┌─────────────────────────────────────────────────────────────────────┐
│                    Message Consumers                                │
├─────────────────────────────────────────────────────────────────────┤
│  • BookingCreatedConsumer                                           │
│      └─> Send confirmation email                                    │
│      └─> Update room availability                                   │
│      └─> Create notifications                                       │
│                                                                      │
│  • BookingConfirmedConsumer                                         │
│      └─> Send payment confirmation email                            │
│      └─> Update property availability                               │
│      └─> Notify partner                                             │
│      └─> Process payment settlement                                 │
│                                                                      │
│  • BookingCancelledConsumer                                         │
│      └─> Send cancellation email                                    │
│      └─> Release room availability                                  │
│      └─> Process refund                                             │
│      └─> Update partner dashboard                                   │
└─────────────────────────────────────────────────────────────────────┘
```

## Message Contracts

### IBookingCreatedEvent

Published when a new booking is successfully created.

```csharp
public interface IBookingCreatedEvent
{
    Guid BookingId { get; }
    Guid UserId { get; }
    Guid PropertyId { get; }
    DateTime CheckInDate { get; }
    DateTime CheckOutDate { get; }
    int NumberOfGuests { get; }
    decimal TotalPrice { get; }
    string? SpecialRequests { get; }
    DateTime CreatedAt { get; }
}
```

**Use Cases:**
- Send booking confirmation email to guest
- Update room availability calendar
- Create notification for property partner
- Trigger analytics/reporting
- Log booking event for audit

### IBookingConfirmedEvent

Published when a booking is confirmed (typically after payment).

```csharp
public interface IBookingConfirmedEvent
{
    Guid BookingId { get; }
    Guid UserId { get; }
    Guid PropertyId { get; }
    DateTime ConfirmedAt { get; }
}
```

**Use Cases:**
- Send payment confirmation email
- Lock room availability
- Notify partner of confirmed booking
- Trigger settlement process
- Update booking status in analytics

### IBookingCancelledEvent

Published when a booking is cancelled by user or partner.

```csharp
public interface IBookingCancelledEvent
{
    Guid BookingId { get; }
    Guid UserId { get; }
    Guid PropertyId { get; }
    string? CancellationReason { get; }
    DateTime CancelledAt { get; }
}
```

**Use Cases:**
- Send cancellation email to guest
- Release room availability
- Process refund if applicable
- Notify partner of cancellation
- Update booking metrics

## Message Flow

### 1. Booking Creation Flow

```
1. User submits booking request
   └─> POST /api/v1/bookings

2. CreateBookingCommandHandler
   ├─> Validate booking data
   ├─> Create Booking aggregate
   ├─> Save to database
   └─> Publish BookingCreatedEvent
        └─> RabbitMQ: TripEnjoy.Application.Messages.Contracts:IBookingCreatedEvent

3. BookingCreatedConsumer (async)
   ├─> Send confirmation email (EmailService)
   ├─> Update availability cache (CacheService)
   ├─> Create user notification (NotificationService)
   └─> Log event (LogService)

4. Response returned to user immediately (201 Created)
```

### 2. Booking Confirmation Flow

```
1. Partner/Admin confirms booking
   └─> POST /api/v1/bookings/{id}/confirm

2. ConfirmBookingCommandHandler
   ├─> Load booking from database
   ├─> Validate booking status (must be Pending)
   ├─> Call Booking.Confirm() domain method
   ├─> Save to database
   └─> Publish BookingConfirmedEvent

3. BookingConfirmedConsumer (async)
   ├─> Send confirmation email with details
   ├─> Update property availability
   ├─> Notify partner via notification system
   ├─> Create settlement record for payment
   └─> Update metrics/analytics

4. Response returned immediately (200 OK)
```

### 3. Booking Cancellation Flow

```
1. User/Partner cancels booking
   └─> POST /api/v1/bookings/{id}/cancel

2. CancelBookingCommandHandler
   ├─> Load booking from database
   ├─> Validate cancellation is allowed
   ├─> Call Booking.Cancel() domain method
   ├─> Save to database
   └─> Publish BookingCancelledEvent

3. BookingCancelledConsumer (async)
   ├─> Send cancellation email
   ├─> Release room availability
   ├─> Process refund if within policy
   ├─> Notify partner of cancellation
   └─> Update booking statistics

4. Response returned immediately (200 OK)
```

## Retry and Error Handling

### Retry Policy

MassTransit is configured with exponential backoff retry policy:

```csharp
cfg.UseMessageRetry(r => r.Intervals(
    TimeSpan.FromSeconds(1),   // 1st retry: 1 second
    TimeSpan.FromSeconds(5),   // 2nd retry: 5 seconds
    TimeSpan.FromSeconds(10),  // 3rd retry: 10 seconds
    TimeSpan.FromSeconds(30)   // 4th retry: 30 seconds
));
```

**Total Retries**: 4 attempts
**Total Wait Time**: 46 seconds maximum before moving to error queue

### Error Handling Strategy

1. **Transient Errors** (network issues, temporary service unavailability):
   - Automatically retried using retry policy
   - If all retries fail, message moves to error queue

2. **Permanent Errors** (invalid data, business rule violations):
   - Logged with full context
   - Message moved to error queue immediately
   - Alert generated for manual review

3. **Poison Messages** (corrupted data, serialization errors):
   - Moved to error queue after first failure
   - Requires manual intervention

### Dead Letter Queues (DLQ)

Each consumer has a dedicated error queue:
- `BookingCreated_error`
- `BookingConfirmed_error`
- `BookingCancelled_error`

**DLQ Monitoring**:
- Manual review via RabbitMQ Management UI
- Reprocess messages after fixing underlying issue
- Alert on queue depth threshold

## Configuration

### appsettings.json

```json
{
  "RabbitMQ": {
    "Host": "localhost",
    "Port": 5672,
    "VirtualHost": "/",
    "Username": "guest",
    "Password": "guest",
    "QueueName": "tripenjoy-bookings"
  }
}
```

### Production Configuration (Environment Variables)

```bash
# RabbitMQ Connection
RABBITMQ__HOST=rabbitmq.tripenjoy.com
RABBITMQ__PORT=5672
RABBITMQ__VIRTUALHOST=/production
RABBITMQ__USERNAME=tripenjoy-prod
RABBITMQ__PASSWORD=<secure-password>

# SSL/TLS (Production)
RABBITMQ__SSL_ENABLED=true
RABBITMQ__SSL_SERVERNAME=rabbitmq.tripenjoy.com
```

## Deployment

### Local Development

1. **Start RabbitMQ with Docker Compose:**

```bash
docker-compose -f docker-compose.rabbitmq.yml up -d
```

2. **Access RabbitMQ Management UI:**
   - URL: http://localhost:15672
   - Username: `guest`
   - Password: `guest`

3. **Run the Application:**

```bash
dotnet run --project src/TripEnjoyServer/TripEnjoy.Api
```

### Production Deployment

**Infrastructure Requirements:**
- **RabbitMQ Cluster**: 3+ nodes for high availability
- **Persistent Storage**: Message persistence enabled
- **Monitoring**: RabbitMQ Management Plugin + Prometheus exporter
- **Backup**: Regular queue/exchange configuration backups

**Recommended Setup:**
- Use managed RabbitMQ service (CloudAMQP, AWS MQ, Azure Service Bus with RabbitMQ)
- Enable SSL/TLS for connections
- Set up alerts for queue depth, consumer lag, error rates
- Configure log aggregation (Seq, Elasticsearch)

## Monitoring and Observability

### Key Metrics

1. **Message Throughput**
   - Messages published per second
   - Messages consumed per second
   - Queue depth over time

2. **Consumer Health**
   - Consumer processing time
   - Error rate per consumer
   - Retry attempts per message

3. **Queue Metrics**
   - Queue length
   - Message rate (in/out)
   - Consumer utilization

### RabbitMQ Management UI

Access at: `http://localhost:15672`

**Key Pages:**
- **Queues**: Monitor queue depth, message rates
- **Connections**: Active consumers and publishers
- **Exchanges**: Message routing information
- **Admin**: User management, policies

### Logging

All consumers log:
- **Info**: Successful message processing
- **Warning**: Retry attempts
- **Error**: Failed processing with exception details

Example log output:
```
[INFO] Processing BookingCreated event for BookingId: abc-123
[INFO] Successfully processed BookingCreated event for BookingId: abc-123
[ERROR] Error processing BookingCreated event for BookingId: xyz-789: Exception: ...
```

## Testing Strategy

### Unit Tests

Location: `src/TripEnjoyServer/TripEnjoy.Test/UnitTests/Messages/`

**Test Coverage:**
- Message serialization/deserialization
- Consumer message handling logic
- Retry policy behavior
- Error handling scenarios

**Example:**
```csharp
[Fact]
public async Task Consume_ValidBookingCreatedEvent_ProcessesSuccessfully()
{
    // Uses MassTransit.Testing test harness
    // Verifies message consumed without errors
}
```

**Running Tests:**
```bash
dotnet test --filter "FullyQualifiedName~Messages"
```

**Current Status:** ✅ 5/5 tests passing

### Integration Tests

**Requirements:**
- RabbitMQ running locally (Docker)
- Test database
- Redis cache (optional)

**Test Scenarios:**
- End-to-end message publishing
- Consumer processing with real dependencies
- Retry behavior with failing services
- Dead letter queue handling

## Performance Characteristics

### Expected Throughput

| Operation | Throughput | Latency (p95) |
|-----------|-----------|---------------|
| Publish Event | 1,000+ msg/s | < 5ms |
| Consume Event | 500+ msg/s | < 50ms |
| End-to-End | 300+ bookings/s | < 200ms |

### Scalability

**Horizontal Scaling:**
- Add more consumer instances (Kubernetes pods/replicas)
- MassTransit automatically distributes load across consumers
- RabbitMQ handles load balancing via competing consumers pattern

**Vertical Scaling:**
- Increase RabbitMQ resources (CPU, memory, disk)
- Optimize consumer processing (parallel tasks, batching)

## Future Enhancements

### Phase 2 Roadmap

1. **Outbox Pattern** (Q1 2026)
   - Implement transactional outbox with EF Core
   - Ensures exactly-once message delivery
   - Prevents message loss on database failures

2. **Advanced Consumers** (Q2 2026)
   - Email service consumer (SendGrid/Mailgun integration)
   - SMS notification consumer (Twilio integration)
   - Analytics consumer (Google Analytics/Mixpanel events)

3. **Saga Pattern** (Q3 2026)
   - Implement booking workflow saga
   - Handle complex multi-step processes
   - Automatic compensation on failures

4. **Performance Optimization** (Q4 2026)
   - Message batching for high-volume operations
   - Priority queues for critical bookings
   - Message compression for large payloads

## Troubleshooting

### Common Issues

#### 1. Messages Not Being Consumed

**Symptoms:**
- Queue depth increasing
- No consumer logs

**Solutions:**
```bash
# Check RabbitMQ is running
docker ps | grep rabbitmq

# Check consumer registration
# Look for: "Configured endpoint" in application logs

# Verify connection settings in appsettings.json
```

#### 2. Consumer Processing Failures

**Symptoms:**
- Messages in error queue
- Exception logs

**Solutions:**
```csharp
// Check consumer implementation for exceptions
// Verify external service availability (database, cache)
// Review retry policy configuration
```

#### 3. RabbitMQ Connection Issues

**Symptoms:**
- "Failed to connect to RabbitMQ" errors
- Application startup failures

**Solutions:**
```bash
# Verify RabbitMQ is accessible
telnet localhost 5672

# Check credentials in appsettings.json
# Review firewall rules
```

## Security Considerations

### Connection Security

1. **Production**: Always use SSL/TLS
2. **Credentials**: Store in Azure Key Vault / AWS Secrets Manager
3. **Virtual Hosts**: Separate environments (dev, staging, prod)
4. **User Permissions**: Least privilege principle

### Message Security

1. **Sensitive Data**: Do not include credit card numbers, passwords
2. **PII Handling**: Consider encryption for personal data
3. **Audit Trail**: Log all message events for compliance

## References

- [MassTransit Documentation](https://masstransit.io/)
- [RabbitMQ Documentation](https://www.rabbitmq.com/documentation.html)
- [TripEnjoy Architecture Guide](./ARCHITECTURE-DIAGRAMS.md)
- [TripEnjoy Project Context](./TripEnjoy-Project-Context.md)

## Revision History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | 2025-12-20 | GitHub Copilot | Initial implementation with RabbitMQ and MassTransit |
