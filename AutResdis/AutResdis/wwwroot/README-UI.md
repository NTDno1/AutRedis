# Hướng dẫn sử dụng giao diện AutResdis

## Tổng quan

Giao diện web của AutResdis được xây dựng với HTML5, CSS3 và JavaScript thuần, cung cấp trải nghiệm người dùng hiện đại và responsive cho hệ thống authentication với Redis.

## Cấu trúc file

```
wwwroot/
├── index.html          # Trang chính
├── demo.html           # Trang demo và test API
├── css/
│   ├── site.css        # CSS chính
│   └── icons.css       # CSS cho icons và animations
├── js/
│   └── app.js          # JavaScript chính
└── README-UI.md        # File này
```

## Tính năng giao diện

### 1. Trang chủ (index.html)
- **Header**: Logo và navigation menu
- **Welcome Section**: Giới thiệu dự án với các nút đăng nhập/đăng ký
- **Responsive Design**: Tự động điều chỉnh theo kích thước màn hình

### 2. Form đăng nhập
- **Giao diện đẹp**: Thiết kế hiện đại với glassmorphism effect
- **Validation**: Kiểm tra form trước khi submit
- **Loading states**: Hiển thị trạng thái đang xử lý
- **Error handling**: Hiển thị thông báo lỗi rõ ràng

### 3. Form đăng ký
- **Tương tự đăng nhập**: Thiết kế nhất quán
- **Field validation**: Kiểm tra username, email, password
- **Success feedback**: Thông báo đăng ký thành công

### 4. Dashboard
- **User info**: Hiển thị thông tin user đang đăng nhập
- **Session management**: Quản lý các session đang hoạt động
- **Device tracking**: Theo dõi thiết bị đăng nhập
- **Actions**: Các nút thao tác (refresh, logout other devices)

### 5. Session Cards
- **Visual status**: Hiển thị trạng thái session (active/expired)
- **Device info**: Thông tin thiết bị, IP, thời gian
- **Hover effects**: Hiệu ứng khi di chuột qua

## Các hiệu ứng và animations

### 1. CSS Animations
- **Fade In**: Xuất hiện mượt mà
- **Slide In**: Trượt từ trái sang phải
- **Hover Effects**: Nâng lên khi hover
- **Loading Spinner**: Xoay khi đang tải

### 2. Transitions
- **Smooth transitions**: Chuyển đổi mượt mà
- **Transform effects**: Biến đổi kích thước và vị trí
- **Color transitions**: Thay đổi màu sắc mượt mà

### 3. Responsive Design
- **Mobile-first**: Tối ưu cho thiết bị di động
- **Breakpoints**: Điều chỉnh layout theo kích thước màn hình
- **Touch-friendly**: Dễ sử dụng trên màn hình cảm ứng

## Cách sử dụng

### 1. Khởi chạy
1. Chạy dự án: `dotnet run`
2. Mở trình duyệt: `https://localhost:7001`
3. Giao diện sẽ tự động load

### 2. Đăng ký tài khoản
1. Click "Đăng ký" trên trang chủ
2. Điền thông tin: username, email, password
3. Click "Đăng ký"
4. Hệ thống sẽ tạo tài khoản và chuyển về form đăng nhập

### 3. Đăng nhập
1. Click "Đăng nhập" trên trang chủ
2. Nhập username và password
3. Click "Đăng nhập"
4. Nếu thành công, sẽ chuyển đến Dashboard

### 4. Sử dụng Dashboard
1. **Xem thông tin user**: Username và email hiển thị ở góc phải
2. **Quản lý session**: Xem danh sách session đang hoạt động
3. **Refresh sessions**: Click "Làm mới danh sách session"
4. **Logout other devices**: Click "Đăng xuất tất cả thiết bị khác"

### 5. Test API (demo.html)
1. Truy cập `/demo.html`
2. Sử dụng các nút test để kiểm tra API endpoints
3. Xem kết quả trong phần "Test Results"

## Tùy chỉnh giao diện

### 1. Thay đổi màu sắc
Chỉnh sửa file `css/site.css`:
```css
:root {
    --primary-color: #667eea;
    --secondary-color: #764ba2;
    --success-color: #28a745;
    --danger-color: #dc3545;
}
```

### 2. Thay đổi font
```css
body {
    font-family: 'Your Font', sans-serif;
}
```

### 3. Thay đổi animations
Chỉnh sửa file `css/icons.css`:
```css
.fade-in {
    animation: fadeIn 0.8s ease-in; /* Thay đổi thời gian */
}
```

### 4. Thêm icons mới
Sử dụng emoji hoặc font icons:
```html
<span class="icon">🚀</span>
```

## Troubleshooting

### 1. Giao diện không load
- Kiểm tra console browser để xem lỗi JavaScript
- Đảm bảo các file CSS và JS được load đúng
- Kiểm tra đường dẫn file

### 2. Responsive không hoạt động
- Kiểm tra viewport meta tag
- Đảm bảo CSS media queries được viết đúng
- Test trên nhiều kích thước màn hình

### 3. Animations không mượt
- Kiểm tra CSS animations được định nghĩa
- Đảm bảo browser hỗ trợ CSS animations
- Kiểm tra performance trên thiết bị yếu

## Best Practices

### 1. Performance
- Sử dụng CSS transforms thay vì thay đổi layout
- Tối ưu hóa images và assets
- Sử dụng lazy loading cho content lớn

### 2. Accessibility
- Sử dụng semantic HTML
- Đảm bảo contrast ratio đủ cao
- Hỗ trợ keyboard navigation

### 3. SEO
- Sử dụng meta tags đầy đủ
- Semantic HTML structure
- Fast loading times

## Phát triển thêm

### 1. Thêm trang mới
1. Tạo file HTML mới trong `wwwroot/`
2. Thêm CSS styles vào `css/site.css`
3. Thêm JavaScript logic vào `js/app.js`

### 2. Thêm components
1. Tạo CSS classes cho component
2. Thêm HTML template
3. Thêm JavaScript functionality

### 3. Thêm themes
1. Tạo CSS variables cho colors
2. Tạo theme switcher
3. Lưu theme preference vào localStorage

## Kết luận

Giao diện AutResdis được thiết kế để dễ sử dụng, đẹp mắt và responsive. Với cấu trúc rõ ràng và code dễ bảo trì, bạn có thể dễ dàng tùy chỉnh và mở rộng theo nhu cầu. 