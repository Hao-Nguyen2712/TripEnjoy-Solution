# QUY TẮC KIẾN TRÚC DỰ ÁN TRIPENJOY

Tài liệu này định nghĩa các quy tắc và hướng dẫn để phát triển dự án TripEnjoy theo đúng tinh thần của Clean Architecture và Domain-Driven Design (DDD). Mọi thành viên trong nhóm cần tuân thủ các quy tắc này để đảm bảo sự nhất quán, dễ bảo trì và mở rộng của dự án.

## 1. Nguyên tắc cốt lõi: The Dependency Rule

Quy tắc quan trọng nhất: **MÃ NGUỒN Ở CÁC LỚP BÊN TRONG KHÔNG ĐƯỢC PHÉP BIẾT GÌ VỀ CÁC LỚP BÊN NGOÀI.**

- `Domain` là lớp trong cùng, không phụ thuộc vào bất kỳ lớp nào khác.
- `Application` phụ thuộc vào `Domain`.
- `Infrastructure` phụ thuộc vào `Application`.
- `Api` (Presentation) phụ thuộc vào `Application`.

Điều này có nghĩa là các đối tượng trong `Domain` (Entities, Value Objects) không được tham chiếu đến các class, thư viện hay framework của các lớp bên ngoài (ví dụ: không `using Microsoft.EntityFrameworkCore;` trong một Entity).

## 2. Hướng dẫn cho từng lớp (Layer)

### 2.1. Lớp `TripEnjoy.Domain` (Trái tim của ứng dụng)

- **Mục đích:** Chứa tất cả logic và quy tắc nghiệp vụ cốt lõi.
- **Quy tắc:**
    - **Entities:** Đặt trong thư mục `Entities`. Các class phải kế thừa từ `Entity` (nếu có base class). Logic nghiệp vụ liên quan trực tiếp đến trạng thái của entity phải được đặt bên trong chính class entity đó (Rich Domain Model).
        *Ví dụ: Thay vì có một `TripService` để thay đổi trạng thái chuyến đi, hãy có phương thức `trip.Cancel()` bên trong chính entity `Trip`.*
    - **Aggregates:** Đặt trong thư mục `Aggregates`. Mỗi aggregate có một `AggregateRoot`. Mọi tương tác với các thành phần bên trong aggregate phải thông qua `AggregateRoot`. Điều này đảm bảo tính toàn vẹn và nhất quán.
    - **Value Objects:** Đặt trong thư mục `ValueObjects`. Đây là các đối tượng bất biến (immutable), không có định danh riêng. Luôn ưu tiên sử dụng Value Object thay cho các kiểu dữ liệu nguyên thủy để thể hiện rõ hơn ngữ cảnh nghiệp vụ (ví dụ: dùng `Address` object thay vì các trường `string street, string city`...).
    - **Repository Interfaces:** Định nghĩa các interface cho việc truy cập dữ liệu (ví dụ: `ITripRepository`) ngay trong lớp `Domain`. Interface này chỉ định nghĩa các "hợp đồng" mà không quan tâm đến cách cài đặt chi tiết.
    - **Domain Events:** Đặt trong thư mục `Events`. Sử dụng để thông báo về những thay đổi quan trọng trong domain mà các phần khác của hệ thống có thể cần xử lý.

### 2.2. Lớp `TripEnjoy.Application` (Use Cases của ứng dụng)

- **Mục đích:** Điều phối luồng dữ liệu và thực thi các kịch bản sử dụng (use cases).
- **Quy tắc:**
    - **CQRS (Command Query Responsibility Segregation):**
        - Mọi yêu cầu thay đổi dữ liệu (Create, Update, Delete) phải được triển khai dưới dạng **Command** và **CommandHandler**.
        - Mọi yêu cầu truy vấn dữ liệu (Read) phải được triển khai dưới dạng **Query** và **QueryHandler**.
        - Gợi ý: Sử dụng thư viện **MediatR** để gửi các Command và Query từ `Api` và xử lý chúng trong các Handler tương ứng.
    - **Validation:** Sử dụng thư viện **FluentValidation** để kiểm tra tính hợp lệ của các Command trước khi chúng được xử lý. Mỗi Command nên có một `Validator` tương ứng.
    - **Không chứa logic nghiệp vụ:** Các Handler chỉ có nhiệm vụ điều phối: nhận Command/Query, gọi các phương thức trên các `Aggregate` hoặc `Repository`, và trả về kết quả. Toàn bộ logic nghiệp-vụ-cốt-lõi phải nằm trong lớp `Domain`.
    - **DTOs (Data Transfer Objects):** Dữ liệu trả về từ các Query nên là các DTO đơn giản, không phải là các Domain Entity. Điều này tránh việc lộ cấu trúc của Domain ra bên ngoài.

### 2.3. Lớp `TripEnjoy.Infrastructure` (Chi tiết kỹ thuật)

- **Mục đích:** Cung cấp các cài đặt (implementation) cụ thể cho các interface đã được định nghĩa ở lớp trong.
- **Quy tắc:**
    - **Repository Implementation:** Đặt trong `TripEnjoy.Infrastructure.Persistence/Repositories`. Các class ở đây (ví dụ: `TripRepository`) sẽ cài đặt các interface từ lớp `Domain` (ví dụ: `ITripRepository`) bằng cách sử dụng **Entity Framework Core**.
    - **DbContext:** File `TripEnjoyDbContext.cs` là nơi định nghĩa `DbSet` cho các `AggregateRoot` và cấu hình mapping giữa entities và database schema.
    - **Dependency Injection:** Cấu hình việc tiêm phụ thuộc (DI) trong một file `DependencyInjection.cs`. Đăng ký các repository, các dịch vụ bên ngoài (email, payment...) với container DI.
    - **Các dịch vụ ngoài:** Cài đặt các dịch vụ như gửi email, lưu trữ file, cổng thanh toán... tại đây.

### 2.4. Lớp `TripEnjoy.Api` (Giao diện người dùng)

- **Mục đích:** Tiếp nhận các yêu cầu HTTP và trả về các phản hồi HTTP.
- **Quy tắc:**
    - **Controller "mỏng" (Thin Controller):** Các action trong controller chỉ nên làm 3 việc:
        1. Nhận HTTP request và dữ liệu từ body/query string.
        2. Tạo một đối tượng `Command` hoặc `Query` tương ứng.
        3. Gửi nó đi bằng MediatR: `await mediator.Send(command);`
        4. Trả về kết quả dưới dạng `IActionResult` (ví dụ: `Ok()`, `CreatedAtAction()`, `NotFound()`).
    - **Không gọi trực tiếp Repository hay DbContext:** Controller không bao giờ được phép tương tác trực tiếp với lớp Infrastructure. Mọi tương tác phải thông qua các Command/Query của lớp `Application`.
    - **Sử dụng DTOs:** Sử dụng các DTO để định nghĩa cấu trúc dữ liệu cho request body và response body.

---
*Tài liệu này nên được cập nhật thường xuyên khi dự án phát triển và có những quyết định kiến trúc mới.*
