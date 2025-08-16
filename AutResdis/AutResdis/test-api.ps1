# Test AutResdis API
Write-Host "Testing AutResdis API..." -ForegroundColor Green

# Test login
Write-Host "`n1. Testing login..." -ForegroundColor Yellow
try {
    $loginBody = @{
        username = "admin"
        password = "admin123"
    } | ConvertTo-Json

    $loginResponse = Invoke-RestMethod -Uri "http://localhost:5045/api/auth/login" -Method POST -ContentType "application/json" -Body $loginBody
    
    if ($loginResponse.success) {
        Write-Host "Login successful!" -ForegroundColor Green
        Write-Host "Token: $($loginResponse.token)" -ForegroundColor Cyan
        
        $token = $loginResponse.token
        
        # Test get profile
        Write-Host "`n2. Testing get profile..." -ForegroundColor Yellow
        try {
            $headers = @{
                "Authorization" = "Bearer $token"
            }
            
            $profileResponse = Invoke-RestMethod -Uri "http://localhost:5045/api/user/profile" -Method GET -Headers $headers
            Write-Host "Profile retrieved successfully!" -ForegroundColor Green
            Write-Host "Username: $($profileResponse.username)" -ForegroundColor Cyan
            Write-Host "Email: $($profileResponse.email)" -ForegroundColor Cyan
        }
        catch {
            Write-Host "Failed to get profile: $($_.Exception.Message)" -ForegroundColor Red
        }
        
        # Test validate session
        Write-Host "`n3. Testing session validation..." -ForegroundColor Yellow
        try {
            $sessionResponse = Invoke-RestMethod -Uri "http://localhost:5045/api/auth/validate-session" -Method GET -Headers $headers
            Write-Host "Session validation successful!" -ForegroundColor Green
            Write-Host "Response: $($sessionResponse | ConvertTo-Json)" -ForegroundColor Cyan
        }
        catch {
            Write-Host "Failed to validate session: $($_.Exception.Message)" -ForegroundColor Red
        }
    }
    else {
        Write-Host "Login failed: $($loginResponse.message)" -ForegroundColor Red
    }
}
catch {
    Write-Host "Error during login: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`nAPI testing completed!" -ForegroundColor Green 