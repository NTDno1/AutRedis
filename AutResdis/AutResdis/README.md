# AutResdis - Hệ thống Authentication với Redis

## Mô tả dự án

AutResdis là một dự án ASP.NET Core 9.0 triển khai hệ thống đăng nhập với Redis để quản lý session và kiểm soát thiết bị đăng nhập. Dự án này giải quyết các yêu cầu:

- **Đăng nhập an toàn**: Sử dụng JWT token và Redis để lưu trữ session
- **Kiểm soát thiết bị**: Theo dõi và kiểm soát việc đăng nhập từ nhiều thiết bị
- **Quản lý session**: Lưu trữ session trong Redis với thời gian hết hạn
- **Bảo mật**: Xác thực token và kiểm tra tính hợp lệ của session

## Tính năng chính

### 1. Authentication System
- Đăng nhập với username/password
- Tạo JWT token
- Lưu trữ session trong Redis
- Kiểm tra tính hợp lệ của token

### 2. Device Management
- Tự động tạo Device ID dựa trên User-Agent và IP Address
- Kiểm tra xem user có đang đăng nhập trên thiết bị khác không
- Cho phép đăng xuất tất cả thiết bị khác
- Theo dõi thông tin thiết bị (User-Agent, IP Address)

### 3. Session Management
- Lưu trữ session trong Redis với TTL
- Kiểm tra session còn hợp lệ không
- Xóa session khi đăng xuất
- Quản lý nhiều session cùng lúc

## Cấu trúc dự án

```
AutResdis/
├── Controllers/
│   ├── AuthController.cs          # Xử lý đăng nhập/đăng xuất
│   ├── UserController.cs          # Quản lý user
│   └── WeatherForecastController.cs
├── Models/
│   ├── User.cs                    # Model User
│   ├── LoginRequest.cs            # Request đăng nhập
│   ├── LoginResponse.cs           # Response đăng nhập
│   └── UserSession.cs             # Thông tin session
├── Services/
│   ├── IAuthService.cs            # Interface Auth Service
│   ├── AuthService.cs             # Implementation Auth Service
│   ├── IRedisService.cs           # Interface Redis Service
│   ├── RedisService.cs            # Implementation Redis Service
│   ├── IJwtService.cs             # Interface JWT Service
│   └── JwtService.cs              # Implementation JWT Service
├── Data/
│   └── ApplicationDbContext.cs    # Entity Framework Context
├── Configuration/
│   ├── JwtSettings.cs             # Cấu hình JWT
│   └── RedisSettings.cs           # Cấu hình Redis
├── appsettings.json               # Cấu hình ứng dụng
└── Program.cs                     # Entry point và cấu hình services
```

## Cài đặt và chạy dự án

### Yêu cầu hệ thống
- .NET 9.0 SDK
- SQL Server hoặc LocalDB
- Redis Server

### Bước 1: Cài đặt Redis
```bash
# Windows (sử dụng WSL hoặc Docker)
docker run -d -p 6379:6379 redis:latest

# Hoặc cài đặt Redis trên Windows
# Tải từ: https://github.com/microsoftarchive/redis/releases
```

### Bước 2: Cài đặt dependencies
```bash
dotnet restore
```

### Bước 3: Cấu hình database
- Đảm bảo SQL Server đang chạy
- Cập nhật connection string trong `appsettings.json` nếu cần

### Bước 4: Chạy dự án
```bash
dotnet run
```

## API Endpoints

### Authentication

#### POST /api/auth/login
Đăng nhập user
```json
{
  "username": "testuser",
  "password": "password123",
  "deviceId": "optional-device-id"
}
```

Response:
```json
{
  "success": true,
  "token": "jwt-token-here",
  "refreshToken": "refresh-token-here",
  "expiresAt": "2024-01-01T12:00:00Z",
  "username": "testuser",
  "message": "Login successful"
}
```

#### POST /api/auth/logout
Đăng xuất (yêu cầu Authorization header)
```
Authorization: Bearer <jwt-token>
```

#### POST /api/auth/logout-other-devices
Đăng xuất tất cả thiết bị khác (yêu cầu Authorization header)

#### GET /api/auth/validate-session
Kiểm tra session còn hợp lệ không (yêu cầu Authorization header)

#### GET /api/auth/active-sessions
Lấy danh sách session đang hoạt động (yêu cầu Authorization header)

### User Management

#### POST /api/user/register
Đăng ký user mới
```json
{
  "username": "newuser",
  "email": "user@example.com",
  "password": "password123"
}
```

#### GET /api/user/profile
Lấy thông tin profile (yêu cầu Authorization header)

## Cách hoạt động

### 1. Quy trình đăng nhập
1. User gửi request đăng nhập với username/password
2. Server xác thực thông tin đăng nhập
3. Tạo JWT token và refresh token
4. Tạo UserSession với thông tin thiết bị
5. Lưu session vào Redis với TTL
6. Trả về token cho client

### 2. Kiểm tra session
1. Client gửi request với JWT token trong Authorization header
2. Server xác thực JWT token
3. Kiểm tra session có tồn tại trong Redis không
4. Kiểm tra session còn hợp lệ không (chưa hết hạn)
5. Kiểm tra Device ID nếu cần
6. Cho phép hoặc từ chối request

### 3. Quản lý thiết bị
- Mỗi lần đăng nhập, server tạo Device ID dựa trên User-Agent và IP
- Kiểm tra xem user có đang đăng nhập trên thiết bị này không
- Cho phép đăng xuất tất cả thiết bị khác
- Theo dõi thông tin thiết bị để bảo mật

### 4. Redis Storage Structure
```
AutResdis:session:{token} -> UserSession JSON
AutResdis:user:{userId}:sessions -> Set of active tokens
AutResdis:user:{userId}:device:{deviceId} -> Device session info
```

## Bảo mật

### JWT Token
- Sử dụng secret key mạnh (ít nhất 32 ký tự)
- Có thời gian hết hạn (60 phút)
- Validate issuer, audience, và signature

### Redis Security
- Session có TTL tự động
- Không lưu trữ thông tin nhạy cảm
- Sử dụng instance name để namespace

### Device Tracking
- Tạo Device ID từ User-Agent và IP Address
- Kiểm tra tính nhất quán của thiết bị
- Cho phép user kiểm soát session

## Monitoring và Logging

Dự án sử dụng structured logging để theo dõi:
- Đăng nhập/đăng xuất
- Tạo/xóa session
- Lỗi authentication
- Thay đổi thiết bị

## Troubleshooting

### Redis Connection Error
- Kiểm tra Redis server có đang chạy không
- Kiểm tra connection string trong appsettings.json
- Đảm bảo port 6379 không bị block

### Database Connection Error
- Kiểm tra SQL Server connection string
- Đảm bảo database có thể tạo được
- Kiểm tra quyền truy cập database

### JWT Token Issues
- Kiểm tra secret key trong appsettings.json
- Đảm bảo issuer và audience đúng
- Kiểm tra thời gian hết hạn token

## Phát triển thêm

### Thêm tính năng
- Refresh token mechanism
- Password reset functionality
- Email verification
- Two-factor authentication
- Rate limiting
- Audit logging

### Tối ưu hóa
- Redis clustering
- Database indexing
- Caching strategies
- Performance monitoring

## Liên hệ

Nếu có câu hỏi hoặc góp ý, vui lòng tạo issue trên repository. 