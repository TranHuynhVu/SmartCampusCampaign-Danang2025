# API Documentation - Job Matching System

## Overview
Hệ thống matching việc làm sử dụng AI để gợi ý công việc phù hợp cho sinh viên và ứng viên phù hợp cho doanh nghiệp.

---

## 🎓 STUDENT FLOW - Luồng nghiệp vụ Sinh viên

### Flow tổng quan:
1. Đăng ký tài khoản
2. Đăng nhập
3. Tạo hồ sơ sinh viên (upload CV PDF hoặc nhập thủ công)
4. Hệ thống tự động phân tích CV và tạo embedding
5. Xem gợi ý công việc phù hợp

---

### 1. Đăng ký tài khoản Student
**Endpoint:** `POST /api/Auth/Register`

**Request Body:**
```json
{
  "fullName": "Nguyễn Văn A",
  "email": "student@example.com",
  "password": "Password@123",
  "confirmPassword": "Password@123",
  "skills": "Python, Java, React",
  "rolesInStartup": "Developer",
  "categoryInvests": "Technology"
}
```

**Response:** `200 OK`
```json
{
  "message": "User registered successfully."
}
```

---

### 2. Đăng nhập
**Endpoint:** `POST /api/Auth/Login`

**Request Body:**
```json
{
  "email": "student@example.com",
  "password": "Password@123"
}
```

**Response:** `200 OK`
```json
{
  "access_token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refresh_token": "550e8400-e29b-41d4-a716-446655440000",
  "token_type": "Bearer",
  "expires_in": 3600,
  "scope": "Student"
}
```

**Lưu ý:** Lưu `access_token` và gửi trong header: `Authorization: Bearer {access_token}`

---

### 3. Tạo hồ sơ sinh viên
**Endpoint:** `POST /api/StudentInfors`

**Authorization:** `Bearer Token` (Role: Student)

**Request Body:**
```json
{
  "name": "Nguyễn Văn A",
  "resumeUrl": "https://example.com/resume.pdf",
  "gpa": "3.8",
  "skills": "Python, Java, React, Node.js, MongoDB",
  "archievements": "Giải Nhất Hackathon 2024, Dean's List 2023",
  "yearOfStudy": "2025",
  "major": "Computer Science",
  "languages": "Vietnamese (Native), English (IELTS 7.5)",
  "certifications": "AWS Certified Developer, Google Cloud Associate",
  "experiences": "Internship at ABC Company (6 months), Freelance Developer",
  "projects": "E-commerce Website, AI Chatbot, Mobile App"
}
```

**Response:** `201 Created`
```json
{
  "id": 1,
  "userId": "user-guid-123",
  "name": "Nguyễn Văn A",
  "resumeUrl": "https://example.com/resume.pdf",
  "gpa": "3.8",
  "skills": "Python, Java, React, Node.js, MongoDB",
  "archievements": "Giải Nhất Hackathon 2024, Dean's List 2023",
  "yearOfStudy": "2025",
  "major": "Computer Science",
  "languages": "Vietnamese (Native), English (IELTS 7.5)",
  "certifications": "AWS Certified Developer, Google Cloud Associate",
  "experiences": "Internship at ABC Company (6 months), Freelance Developer",
  "projects": "E-commerce Website, AI Chatbot, Mobile App",
  "createdAt": "2025-12-11T10:30:00Z",
  "updatedAt": null
}
```

**Lưu ý:** 
- Hệ thống tự động tạo **embedding vector** từ Skills, Major, Experiences, Projects, Certifications
- Embedding được lưu vào database để phục vụ matching

---

### 4. Xem hồ sơ của mình
**Endpoint:** `GET /api/StudentInfors/MyProfile`

**Authorization:** `Bearer Token` (Role: Student)

**Response:** `200 OK`
```json
{
  "id": 1,
  "userId": "user-guid-123",
  "name": "Nguyễn Văn A",
  "resumeUrl": "https://example.com/resume.pdf",
  "gpa": "3.8",
  "skills": "Python, Java, React, Node.js, MongoDB",
  "major": "Computer Science",
  // ... các trường khác
}
```

---

### 5. Cập nhật hồ sơ sinh viên
**Endpoint:** `PUT /api/StudentInfors/{id}`

**Authorization:** `Bearer Token` (Role: Student)

**Request Body:** Giống như POST (truyền đầy đủ các trường)

**Response:** `204 No Content`

**Lưu ý:** Embedding sẽ được tự động cập nhật lại

---

### 6. ⭐ Xem gợi ý công việc phù hợp (AI Matching)
**Endpoint:** `GET /api/StudentInfors/JobSuggestions/{studentInforId}?top=10`

**Authorization:** `Bearer Token` (Role: Student hoặc Admin)

**Query Parameters:**
- `top` (optional): Số lượng gợi ý trả về (default: 10)

**Response:** `200 OK`
```json
[
  {
    "id": 5,
    "title": "Backend Developer Intern",
    "description": "Tuyển thực tập sinh Backend Developer...",
    "salaryRange": "5,000,000 - 8,000,000 VND",
    "dayOfWeek": "Thứ 2 - Thứ 6",
    "timeOfDay": "8:00 - 17:00",
    "benefits": "Laptop, Team building, Insurance",
    "requirements": "Python, Django, REST API, MySQL",
    "niceToHave": "Docker, AWS, Microservices",
    "companyInforId": 2,
    "companyName": "ABC Technology",
    "location": "Hà Nội",
    "status": "Active",
    "categoryId": 1,
    "categoryName": "Information Technology",
    "createdAt": "2025-12-10T09:00:00Z",
    "similarityScore": 0.87
  },
  {
    "id": 12,
    "title": "Full Stack Developer",
    "description": "...",
    "similarityScore": 0.82
  }
]
```

**Lưu ý:** 
- Kết quả được sắp xếp theo `similarityScore` giảm dần (1.0 = khớp 100%)
- Score từ 0.7 trở lên được coi là khá phù hợp
- Score từ 0.85 trở lên là rất phù hợp

---

## 🏢 COMPANY FLOW - Luồng nghiệp vụ Doanh nghiệp

### Flow tổng quan:
1. Đăng ký tài khoản Company
2. Đăng nhập
3. Tạo thông tin công ty
4. Tạo tin tuyển dụng (Job)
5. Hệ thống tự động tạo embedding cho Job
6. Xem gợi ý ứng viên phù hợp

---

### 1. Đăng ký tài khoản Company
**Endpoint:** `POST /api/Auth/Register`

**Request Body:**
```json
{
  "fullName": "Công ty ABC",
  "email": "company@abc.com",
  "password": "Password@123",
  "confirmPassword": "Password@123"
}
```

**Lưu ý:** Sau khi đăng ký, Admin cần cấp role "Company" cho tài khoản này

---

### 2. Tạo thông tin công ty
**Endpoint:** `POST /api/CompanyInfors`

**Authorization:** `Bearer Token` (Role: Company)

**Request Body:**
```json
{
  "companyName": "ABC Technology Company",
  "companyWebsite": "https://abc.com",
  "logoUrl": "https://abc.com/logo.png",
  "location": "Hà Nội",
  "description": "Công ty công nghệ hàng đầu Việt Nam..."
}
```

**Response:** `201 Created`

---

### 3. Tạo tin tuyển dụng (Job)
**Endpoint:** `POST /api/Jobs`

**Authorization:** `Bearer Token` (Role: Company)

**Request Body:**
```json
{
  "title": "Backend Developer Intern",
  "description": "Chúng tôi đang tìm kiếm Backend Developer thực tập...",
  "salaryRange": "5,000,000 - 8,000,000 VND",
  "dayOfWeek": "Thứ 2 - Thứ 6",
  "timeOfDay": "8:00 - 17:00",
  "benefits": "Laptop, Team building, Bảo hiểm",
  "requirements": "Python, Django, REST API, MySQL, Git",
  "niceToHave": "Docker, AWS, Microservices, CI/CD",
  "location": "Hà Nội",
  "status": "Active",
  "categoryId": 1
}
```

**Response:** `201 Created`
```json
{
  "id": 5,
  "title": "Backend Developer Intern",
  "description": "...",
  "requirements": "Python, Django, REST API, MySQL, Git",
  "niceToHave": "Docker, AWS, Microservices, CI/CD",
  "companyInforId": 2,
  "companyName": "ABC Technology Company",
  "categoryId": 1,
  "categoryName": "Information Technology",
  "createdAt": "2025-12-11T10:00:00Z"
}
```

**Lưu ý:** 
- Hệ thống tự động tạo **embedding vector** từ Requirements và NiceToHave
- Embedding được lưu để phục vụ matching với ứng viên

---

### 4. Xem danh sách công việc của công ty
**Endpoint:** `GET /api/Jobs/MyJobs`

**Authorization:** `Bearer Token` (Role: Company)

**Response:** `200 OK`
```json
[
  {
    "id": 5,
    "title": "Backend Developer Intern",
    "description": "...",
    "status": "Active",
    // ... các trường khác
  }
]
```

---

### 5. Cập nhật tin tuyển dụng
**Endpoint:** `PUT /api/Jobs/{id}`

**Authorization:** `Bearer Token` (Role: Company)

**Request Body:** Giống như POST

**Response:** `204 No Content`

**Lưu ý:** Embedding sẽ được tự động cập nhật lại

---

### 6. ⭐ Xem gợi ý ứng viên phù hợp (AI Matching)
**Endpoint:** `GET /api/Jobs/CandidateSuggestions/{jobId}?top=10`

**Authorization:** `Bearer Token` (Role: Company hoặc Admin)

**Query Parameters:**
- `top` (optional): Số lượng gợi ý trả về (default: 10)

**Response:** `200 OK`
```json
[
  {
    "id": 1,
    "userId": "user-guid-123",
    "name": "Nguyễn Văn A",
    "resumeUrl": "https://example.com/resume.pdf",
    "gpa": "3.8",
    "skills": "Python, Java, React, Node.js, MongoDB",
    "major": "Computer Science",
    "yearOfStudy": "2025",
    "experiences": "Internship at XYZ Company (6 months), Freelance Developer",
    "projects": "E-commerce Website, AI Chatbot, Mobile App",
    "certifications": "AWS Certified Developer, Google Cloud Associate",
    "languages": "Vietnamese (Native), English (IELTS 7.5)",
    "createdAt": "2025-12-10T08:00:00Z",
    "similarityScore": 0.89
  },
  {
    "id": 3,
    "name": "Trần Thị B",
    "skills": "Python, Django, PostgreSQL",
    "similarityScore": 0.85
  }
]
```

**Lưu ý:** 
- Kết quả được sắp xếp theo `similarityScore` giảm dần
- Company có thể chủ động liên hệ với ứng viên có điểm cao

---

## 🔐 Authentication APIs

### Refresh Token
**Endpoint:** `POST /api/Auth/Refresh`

**Request Body:**
```json
{
  "token": "refresh-token-string"
}
```

**Response:** `200 OK`
```json
{
  "access_token": "new-access-token",
  "refresh_token": "new-refresh-token",
  "token_type": "Bearer",
  "expires_in": 3600,
  "scope": "Student"
}
```

---

### Logout
**Endpoint:** `POST /api/Auth/Logout`

**Request Body:**
```json
{
  "token": "refresh-token-string"
}
```

**Response:** `200 OK`

---

## 📋 Common APIs

### Lấy danh sách Categories
**Endpoint:** `GET /api/Categories`

**Response:** `200 OK`
```json
[
  {
    "id": 1,
    "name": "Information Technology",
    "description": "IT jobs"
  },
  {
    "id": 2,
    "name": "Marketing",
    "description": "Marketing jobs"
  }
]
```

---

### Xem danh sách công việc (Public)
**Endpoint:** `GET /api/Jobs?status=Active&categoryId=1&location=Hà Nội`

**Authorization:** Không cần (AllowAnonymous)

**Query Parameters:**
- `status` (optional): Active, Inactive, Closed
- `categoryId` (optional): Lọc theo category
- `location` (optional): Lọc theo địa điểm

**Response:** `200 OK`

---

### Xem chi tiết công việc
**Endpoint:** `GET /api/Jobs/{id}`

**Authorization:** Không cần (AllowAnonymous)

**Response:** `200 OK`

---

## 🎯 AI/ML Features

### Cách hoạt động của Matching System:

1. **Embedding Generation:**
   - Sử dụng Google Gemini `text-embedding-004` model
   - Chuyển đổi text thành vector 768 chiều
   - Student: từ Skills, Major, Experiences, Projects, Certifications
   - Job: từ Requirements và NiceToHave

2. **Similarity Calculation:**
   - Sử dụng Cosine Similarity
   - Công thức: `similarity = dot(A, B) / (||A|| * ||B||)`
   - Kết quả từ 0 đến 1 (càng cao càng giống)

3. **Ranking:**
   - Sắp xếp theo similarity score giảm dần
   - Trả về top N kết quả phù hợp nhất

---

## ⚙️ Error Responses

### 400 Bad Request
```json
{
  "message": "Validation error message"
}
```

### 401 Unauthorized
```json
{
  "message": "Invalid token or unauthorized"
}
```

### 403 Forbidden
```json
{
  "message": "You don't have permission"
}
```

### 404 Not Found
```json
{
  "message": "Resource not found"
}
```

---

## 🔧 Configuration

**Base URL:** `https://localhost:7019/api`

**Authorization Header:**
```
Authorization: Bearer {access_token}
```

**Content-Type:**
```
Content-Type: application/json
```

---

## 📝 Notes cho Frontend Developer

1. **Token Management:**
   - Lưu access_token và refresh_token sau khi login
   - Tự động gửi access_token trong header cho các request cần auth
   - Implement auto-refresh khi token hết hạn (expires_in: 3600s = 1 giờ)

2. **Role-based UI:**
   - Student: Hiển thị tạo hồ sơ, xem gợi ý công việc
   - Company: Hiển thị tạo job, xem gợi ý ứng viên
   - Admin: Toàn quyền

3. **Similarity Score Display:**
   - 0.9 - 1.0: Rất phù hợp (màu xanh đậm)
   - 0.7 - 0.89: Khá phù hợp (màu xanh nhạt)
   - 0.5 - 0.69: Trung bình (màu vàng)
   - < 0.5: Ít phù hợp (màu xám)

4. **Best Practices:**
   - Debounce search/filter inputs
   - Implement pagination cho danh sách
   - Show loading state khi call AI matching APIs (có thể mất 2-5s)
   - Cache danh sách categories
   - Validate form data trước khi submit

5. **Performance:**
   - Matching APIs có thể mất vài giây do gọi Google Gemini API
   - Nên hiển thị loading indicator
   - Consider caching kết quả suggest trong một khoảng thời gian ngắn

---

**Version:** 1.0  
**Last Updated:** December 11, 2025  
**Contact:** Backend Team
