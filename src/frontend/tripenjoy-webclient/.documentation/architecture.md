# Cấu trúc hệ thống Frontend - TripEnjoy Webclient

Đây là tài liệu mô tả cấu trúc thư mục của dự án Vue.js, được xây dựng bằng Vite. Việc tuân thủ cấu trúc này giúp dự án dễ dàng bảo trì, mở rộng và làm việc nhóm.

## Cấu trúc thư mục chính (`src/`)

```
src/
├── assets/             # Chứa các tài sản tĩnh (hình ảnh, styles)
├── components/         # Chứa các component có thể tái sử dụng
├── layouts/            # Chứa các component bố cục chính (ví dụ: DefaultLayout)
├── router/             # Cấu hình Vue Router (điều hướng trang)
├── services/           # Logic giao tiếp với API backend
├── views/ (hoặc pages/) # Các component đại diện cho một trang (page)
├── App.vue             # Component gốc của ứng dụng
└── main.js             # Điểm vào (entry point) của ứng dụng
```

### Chi tiết các thư mục

#### `src/assets`
- Chứa các tài sản cần được xử lý bởi Vite.
- **`images/`**: Lưu trữ hình ảnh được sử dụng trong các component. Cần được `import` để sử dụng.
- **`styles/`**: Chứa các file CSS/SCSS toàn cục. `main.css` là file style chính.

#### `src/layouts`
- Chứa các component định hình bố cục chung cho trang web, ví dụ như `DefaultLayout.vue` có chứa header, footer và một khu vực `<router-view />` để hiển thị nội dung trang.

#### `src/router`
- **`index.js`**: File duy nhất định nghĩa tất cả các routes của ứng dụng. Sử dụng lazy loading (`() => import(...)`) để tối ưu hiệu suất.

#### `src/services`
- Tách biệt logic giao tiếp với API.
- **`apiClient.js`**: Cấu hình một `axios` instance tập trung. Tất cả các thiết lập chung như `baseURL`, `headers` được định nghĩa ở đây.
- **`authService.js`**: Chứa các hàm cụ thể để gọi đến các endpoint xác thực (đăng nhập, đăng ký, v.v.).

#### `src/views`
- Mỗi file `.vue` trong này đại diện cho một trang hoàn chỉnh.
- Các trang này thường import và kết hợp các component nhỏ hơn lại với nhau.
- Ví dụ: `LoginView.vue`, `SignUpView.vue`, `HomeView.vue`.

---
*Tài liệu này nên được cập nhật khi có sự thay đổi lớn về cấu trúc.*
