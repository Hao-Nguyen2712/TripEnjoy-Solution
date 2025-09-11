# Nhật ký tiến độ - TripEnjoy Webclient

Đây là file ghi lại các công việc đã được thực hiện trong quá trình xây dựng dự án.

## Giai đoạn 1: Thiết lập cấu trúc và luồng xác thực cơ bản

**Ngày bắt đầu:** 10/09/2025

### Các công việc đã hoàn thành:

1.  **Thiết lập cấu trúc thư mục chuyên nghiệp:**
    *   Tái cấu trúc lại dự án Vue mặc định.
    *   Tạo các thư mục: `assets`, `layouts`, `router`, `services`, `views`.

2.  **Xây dựng các trang xác thực:**
    *   Tạo trang `LoginView.vue` (Đăng nhập).
    *   Tạo trang `SignUpView.vue` (Đăng ký).
    *   Tạo trang `ForgotPasswordView.vue` (Quên mật khẩu).
    *   Tạo trang `OtpVerificationView.vue` (Xác thực OTP).

3.  **Tích hợp giao diện (UI):**
    *   Tích hợp template HTML/CSS/JS có sẵn vào các trang.
    *   Xử lý các vấn đề về đường dẫn tài sản (`public` vs `src/assets`).
    *   Tinh chỉnh CSS để giao diện hiển thị đúng và đẹp mắt (căn giữa, khoảng cách).

4.  **Thiết lập điều hướng (Routing):**
    *   Cài đặt và cấu hình `vue-router`.
    *   Thêm routes cho tất cả các trang đã tạo.
    *   Sử dụng `<router-link>` để điều hướng giữa các trang.

5.  **Thêm logic cho Form:**
    *   Cài đặt và cấu hình `vue-toastification` để hiển thị thông báo popup.
    *   Thêm logic validation (kiểm tra rỗng, định dạng email, khớp mật khẩu) cho các form.
    *   Vô hiệu hóa validation mặc định của trình duyệt để đảm bảo giao diện nhất quán.

6.  **Tách biệt logic API:**
    *   Cài đặt `axios`.
    *   Tạo `services/apiClient.js` để cấu hình `axios` tập trung.
    *   Tạo `services/authService.js` để chứa hàm gọi API đăng nhập.
    *   Tái cấu trúc `LoginView.vue` để gọi API thông qua `authService`.

### Tình trạng hiện tại:

- Đã hoàn thành luồng giao diện cho việc đăng ký, đăng nhập, và quên mật khẩu.
- Logic gọi API đã được tích hợp vào trang đăng nhập và được cấu trúc một cách gọn gàng.
- Cần tích hợp API cho các chức năng còn lại (đăng ký, quên mật khẩu, xác thực OTP).

---
*File này cần được cập nhật sau mỗi lần hoàn thành một tính năng lớn.*
