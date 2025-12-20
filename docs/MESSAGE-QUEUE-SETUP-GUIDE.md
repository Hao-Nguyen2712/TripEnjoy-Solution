# Message Queue Developer Setup Guide

## Quick Start

Get the message queue system running on your local machine in under 5 minutes.

### Prerequisites

- ✅ .NET 8 SDK installed
- ✅ Docker Desktop installed and running
- ✅ Git repository cloned

### Step 1: Start RabbitMQ

```bash
# Navigate to project root
cd TripEnjoy-Solution

# Start RabbitMQ container
docker-compose -f docker-compose.rabbitmq.yml up -d

# Verify RabbitMQ is running
docker ps | grep rabbitmq
```

**Expected Output:**
```
CONTAINER ID   IMAGE                           STATUS
abc123def456   rabbitmq:3-management-alpine   Up 30 seconds
```

### Step 2: Access RabbitMQ Management UI

1. Open browser: http://localhost:15672
2. Login credentials:
   - **Username**: `guest`
   - **Password**: `guest`

3. Verify dashboard loads successfully

### Step 3: Run the Application

```bash
# Build the solution
dotnet build TripEnjoyServer.sln

# Run the API
dotnet run --project src/TripEnjoyServer/TripEnjoy.Api
```

**Expected Output:**
```
info: MassTransit[0]
      Configured endpoint BookingCreated, Consumer: BookingCreatedConsumer
info: MassTransit[0]
      Configured endpoint BookingConfirmed, Consumer: BookingConfirmedConsumer
info: MassTransit[0]
      Configured endpoint BookingCancelled, Consumer: BookingCancelledConsumer
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:7199
```

### Step 4: Test Message Publishing

#### Option A: Using Swagger UI

1. Navigate to: https://localhost:7199/swagger
2. Authenticate with JWT token
3. Execute POST `/api/v1/bookings` endpoint
4. Check application logs for message publishing

#### Option B: Using curl

```bash
# Create a booking (replace TOKEN with actual JWT token)
curl -X POST "https://localhost:7199/api/v1/bookings" \
  -H "Authorization: Bearer {TOKEN}" \
  -H "Content-Type: application/json" \
  -d '{
    "propertyId": "guid-here",
    "checkInDate": "2026-01-15",
    "checkOutDate": "2026-01-18",
    "numberOfGuests": 2,
    "specialRequests": "Late check-in",
    "bookingDetails": [
      {
        "roomTypeId": "guid-here",
        "quantity": 1,
        "pricePerNight": 100,
        "discountAmount": 0
      }
    ]
  }'
```

### Step 5: Verify Message Processing

#### Check Application Logs

```bash
# Filter for booking events
dotnet run --project src/TripEnjoyServer/TripEnjoy.Api | grep "Booking"
```

**Expected Output:**
```
[INFO] Successfully created booking abc-123
[INFO] Published BookingCreated event for BookingId: abc-123
[INFO] Processing BookingCreated event for BookingId: abc-123
[INFO] Successfully processed BookingCreated event for BookingId: abc-123
```

#### Check RabbitMQ Management UI

1. Go to **Queues** tab
2. You should see queues:
   - `BookingCreated`
   - `BookingConfirmed`
   - `BookingCancelled`
3. Click on a queue to see message statistics

## Running Tests

### Unit Tests

```bash
# Run all message consumer tests
dotnet test --filter "FullyQualifiedName~Messages"

# Run specific test class
dotnet test --filter "BookingCreatedConsumerTests"

# Run with detailed output
dotnet test --filter "FullyQualifiedName~Messages" --verbosity detailed
```

**Expected Output:**
```
Passed!  - Failed:     0, Passed:     5, Skipped:     0, Total:     5
```

### Integration Tests (Requires RabbitMQ)

```bash
# Start RabbitMQ first
docker-compose -f docker-compose.rabbitmq.yml up -d

# Run integration tests
dotnet test --filter "Category=Integration" --verbosity normal
```

## Configuration

### Local Development (appsettings.Development.json)

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

### Docker Environment Variables

If using environment variables instead of appsettings:

```bash
export RABBITMQ__HOST=localhost
export RABBITMQ__PORT=5672
export RABBITMQ__USERNAME=guest
export RABBITMQ__PASSWORD=guest

dotnet run --project src/TripEnjoyServer/TripEnjoy.Api
```

## Development Workflow

### Making Changes to Consumers

1. **Edit Consumer Code**
   ```csharp
   // src/TripEnjoyServer/TripEnjoy.Application/Messages/Consumers/
   // BookingCreatedConsumer.cs
   
   public async Task Consume(ConsumeContext<IBookingCreatedEvent> context)
   {
       // Add your processing logic here
   }
   ```

2. **Add Unit Test**
   ```csharp
   // src/TripEnjoyServer/TripEnjoy.Test/UnitTests/Messages/
   // BookingCreatedConsumerTests.cs
   
   [Fact]
   public async Task Consume_NewScenario_ProcessesCorrectly()
   {
       // Test your changes
   }
   ```

3. **Run Tests**
   ```bash
   dotnet test --filter "BookingCreatedConsumerTests"
   ```

4. **Test Locally**
   ```bash
   dotnet run --project src/TripEnjoyServer/TripEnjoy.Api
   # Trigger booking creation via Swagger/API
   # Watch logs for consumer processing
   ```

### Adding New Events

1. **Create Event Contract**
   ```csharp
   // src/TripEnjoyServer/TripEnjoy.Application/Messages/Contracts/
   // INewEvent.cs
   
   public interface INewEvent
   {
       Guid Id { get; }
       DateTime OccurredAt { get; }
   }
   ```

2. **Create Event Implementation**
   ```csharp
   // src/TripEnjoyServer/TripEnjoy.Application/Messages/Events/
   // NewEvent.cs
   
   public class NewEvent : INewEvent
   {
       public Guid Id { get; set; }
       public DateTime OccurredAt { get; set; }
   }
   ```

3. **Create Consumer**
   ```csharp
   // src/TripEnjoyServer/TripEnjoy.Application/Messages/Consumers/
   // NewEventConsumer.cs
   
   public class NewEventConsumer : IConsumer<INewEvent>
   {
       public async Task Consume(ConsumeContext<INewEvent> context)
       {
           // Handle event
       }
   }
   ```

4. **Register Consumer in DependencyInjection**
   ```csharp
   // src/TripEnjoyServer/TripEnjoy.Infrastructure/DependencyInjection.cs
   
   services.AddMassTransit(x =>
   {
       x.AddConsumer<NewEventConsumer>();
       // ... existing consumers
   });
   ```

5. **Publish Event**
   ```csharp
   // In your command handler
   await _publishEndpoint.Publish<INewEvent>(new NewEvent
   {
       Id = Guid.NewGuid(),
       OccurredAt = DateTime.UtcNow
   });
   ```

## Debugging Tips

### Enable Verbose Logging

```json
// appsettings.Development.json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "MassTransit": "Debug",
      "RabbitMQ.Client": "Debug"
    }
  }
}
```

### View Message Details in RabbitMQ

1. Go to RabbitMQ Management UI
2. Click on **Queues** tab
3. Click on a queue name (e.g., `BookingCreated`)
4. Scroll to **Get messages** section
5. Click **Get Message(s)** to inspect message payload

### Simulate Consumer Failure

```csharp
public class BookingCreatedConsumer : IConsumer<IBookingCreatedEvent>
{
    public async Task Consume(ConsumeContext<IBookingCreatedEvent> context)
    {
        // Simulate failure to test retry logic
        throw new InvalidOperationException("Simulated failure");
    }
}
```

Restart application and trigger booking creation. Watch logs for retry attempts.

### Monitor Queue Depth

```bash
# Watch queue depth in real-time
watch -n 1 'docker exec tripenjoy-rabbitmq rabbitmqctl list_queues'
```

## Troubleshooting

### Issue: RabbitMQ Container Not Starting

**Error:**
```
Error response from daemon: Conflict. The container name "/tripenjoy-rabbitmq" is already in use
```

**Solution:**
```bash
# Stop and remove existing container
docker stop tripenjoy-rabbitmq
docker rm tripenjoy-rabbitmq

# Start again
docker-compose -f docker-compose.rabbitmq.yml up -d
```

### Issue: Cannot Connect to RabbitMQ

**Error:**
```
MassTransit.RabbitMqTransport.RabbitMqConnectionException: Connect failed: localhost:5672
```

**Solutions:**
1. Verify RabbitMQ is running: `docker ps | grep rabbitmq`
2. Check RabbitMQ logs: `docker logs tripenjoy-rabbitmq`
3. Verify port 5672 is not blocked by firewall
4. Test connection: `telnet localhost 5672`

### Issue: Consumer Not Processing Messages

**Symptoms:**
- Messages appear in queue but aren't consumed
- No consumer logs in application

**Solutions:**
1. Verify consumer is registered in `DependencyInjection.cs`
2. Check application logs for consumer startup: `Configured endpoint {QueueName}`
3. Verify no exceptions during consumer registration
4. Check RabbitMQ Management UI > Queues > Consumer count

### Issue: Messages Going to Error Queue

**Symptoms:**
- Messages appear in `{QueueName}_error` queue
- Consumer logs show exceptions

**Solutions:**
1. Check consumer implementation for bugs
2. Review exception details in logs
3. Verify external dependencies (database, cache) are available
4. Check message payload structure matches contract
5. Manually reprocess messages from error queue after fixing issue

## Performance Testing

### Load Testing with k6

```javascript
// load-test.js
import http from 'k6/http';
import { check } from 'k6';

export let options = {
  stages: [
    { duration: '30s', target: 20 },  // Ramp up to 20 users
    { duration: '1m', target: 50 },   // Ramp up to 50 users
    { duration: '30s', target: 0 },   // Ramp down to 0 users
  ],
};

export default function () {
  let payload = JSON.stringify({
    propertyId: 'test-property-id',
    checkInDate: '2026-01-15',
    checkOutDate: '2026-01-18',
    numberOfGuests: 2,
    bookingDetails: [{
      roomTypeId: 'test-room-type-id',
      quantity: 1,
      pricePerNight: 100
    }]
  });

  let params = {
    headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer {TOKEN}',
    },
  };

  let res = http.post('https://localhost:7199/api/v1/bookings', payload, params);
  check(res, { 'status was 201': (r) => r.status == 201 });
}
```

**Run Test:**
```bash
k6 run load-test.js
```

### Monitor During Load Test

1. **RabbitMQ Management UI**: Watch message rates
2. **Application Logs**: Monitor processing times
3. **Docker Stats**: Check container resource usage
   ```bash
   docker stats tripenjoy-rabbitmq
   ```

## Best Practices

### ✅ Do's

1. **Always log event processing** with context (BookingId, UserId)
2. **Use structured logging** for easy searching
3. **Handle errors gracefully** - don't crash on invalid data
4. **Publish events after database commit** to avoid inconsistency
5. **Use interfaces for message contracts** (enables versioning)
6. **Write tests for consumers** before implementing business logic

### ❌ Don'ts

1. **Don't block consumer threads** with synchronous I/O
2. **Don't retry indefinitely** - use bounded retry policies
3. **Don't ignore error queues** - monitor and alert
4. **Don't include sensitive data** in message payloads
5. **Don't modify message contracts** without versioning
6. **Don't forget to start RabbitMQ** before running tests

## Additional Resources

### Documentation
- [MESSAGE-QUEUE-ARCHITECTURE.md](./MESSAGE-QUEUE-ARCHITECTURE.md) - Complete architecture guide
- [TripEnjoy-Project-Context.md](./TripEnjoy-Project-Context.md) - Project overview
- [MassTransit Documentation](https://masstransit.io/documentation/concepts)
- [RabbitMQ Tutorials](https://www.rabbitmq.com/getstarted.html)

### Tools
- **RabbitMQ Management UI**: http://localhost:15672
- **Swagger API**: https://localhost:7199/swagger
- **Hangfire Dashboard**: https://localhost:7199/hangfire

### Support
- Internal Slack: `#tripenjoy-engineering`
- GitHub Issues: [TripEnjoy-Solution/issues](https://github.com/Hao-Nguyen2712/TripEnjoy-Solution/issues)
- Email: engineering@tripenjoy.com

---

**Last Updated**: December 20, 2025
**Version**: 1.0.0
**Maintained By**: TripEnjoy Engineering Team
