# UserJob API Documentation

## 📋 Tổng quan

API UserJobs hỗ trợ **hai luồng nghiệp vụ chính**:
1. **Application (Ứng tuyển)** - Sinh viên ứng tuyển vào công việc, công ty phản hồi
2. **Invitation (Lời mời)** - Công ty gửi lời mời cho ứng viên, sinh viên phản hồi

**Base URL:** `/api/UserJobs`

---

## 🔄 Phân biệt Type & Status Flow

### Type Values:
| Type | Người tạo | Người phản hồi | Mô tả |
|------|-----------|----------------|-------|
| **Application** | Student | Company | Sinh viên ứng tuyển công việc |
| **Invitation** | Company | Student | Công ty mời ứng viên |

### Status Values:
| Status | Ý nghĩa |
|--------|---------|
| **Pending** | Đợi phản hồi từ bên nhận |
| **Accepted** | Đã chấp nhận (FINAL STATE) |
| **Rejected** | Đã từ chối (FINAL STATE) |

### Status Transitions:
```
Application Flow:
Student tạo (Pending) → Company Accept/Reject → Accepted/Rejected

Invitation Flow:
Company tạo (Pending) → Student Accept/Reject → Accepted/Rejected
```

---

## 🎓 LUỒNG 1: APPLICATION (Student ứng tuyển)

### 1.1 Student xem danh sách công việc gợi ý
```http
GET /api/StudentInfors/JobSuggestions/{studentInforId}?top=10
Authorization: Bearer {student_token}
```

**Response:**
```json
[
  {
    "id": 5,
    "title": "Backend Developer Intern",
    "companyName": "ABC Technology",
    "similarityScore": 0.87
  }
]
```

---

### 1.2 Student ứng tuyển vào công việc
```http
POST /api/UserJobs/Apply
Authorization: Bearer {student_token}
Content-Type: application/json
```

**Request Body:**
```json
{
  "jobId": 5
}
```

**Response:** `200 OK`
```json
{
  "id": 101,
  "userId": "student-guid-123",
  "studentName": "Nguyễn Văn A",
  "studentAvatar": "/avatars/student123.jpg",
  "jobId": 5,
  "jobTitle": "Backend Developer Intern",
  "companyName": "ABC Technology",
  "type": "Application",
  "status": "Pending",
  "updatedBy": "student-guid-123",
  "createdAt": "2025-12-12T10:00:00Z",
  "updatedAt": "2025-12-12T10:00:00Z"
}
```

**Errors:**
- `404` - Không tìm thấy công việc
- `400` - Bạn đã ứng tuyển công việc này rồi

**Notes:**
- `type` tự động = `"Application"`
- `status` tự động = `"Pending"`
- Công ty sẽ nhận được đơn và có thể Accept/Reject

---

### 1.3 Student xem danh sách đơn ứng tuyển đã gửi
```http
GET /api/UserJobs/MyApplications
Authorization: Bearer {student_token}
```

**Response:** `200 OK`
```json
[
  {
    "id": 101,
    "userId": "student-guid-123",
    "studentName": "Nguyễn Văn A",
    "studentAvatar": "/avatars/student123.jpg",
    "jobId": 5,
    "jobTitle": "Backend Developer Intern",
    "companyName": "ABC Technology",
    "type": "Application",
    "status": "Pending",
    "updatedBy": "student-guid-123",
    "createdAt": "2025-12-12T10:00:00Z",
    "updatedAt": "2025-12-12T10:00:00Z"
  }
]
```

**Notes:**
- Chỉ hiển thị các đơn có `type = "Application"`
- Sắp xếp theo thời gian mới nhất

---

### 1.4 Student xem lời mời từ công ty
```http
GET /api/UserJobs/MyInvitations
Authorization: Bearer {student_token}
```

**Response:** `200 OK`
```json
[
  {
    "id": 102,
    "userId": "student-guid-123",
    "studentName": "Nguyễn Văn A",
    "studentAvatar": "/avatars/student123.jpg",
    "jobId": 8,
    "jobTitle": "Frontend Developer",
    "companyName": "XYZ Corp",
    "type": "Invitation",
    "status": "Pending",
    "updatedBy": "company-guid-789",
    "createdAt": "2025-12-12T15:00:00Z",
    "updatedAt": "2025-12-12T15:00:00Z"
  }
]
```

**Notes:**
- Chỉ hiển thị các lời mời có `type = "Invitation"`

---

### 1.5 Student phản hồi lời mời (Accept/Reject)

**Cách 1: Sử dụng endpoint chung (cần body)**
```http
PUT /api/UserJobs/RespondInvitation/{id}
Authorization: Bearer {student_token}
Content-Type: application/json
```

**Request Body:**
```json
{
  "status": "Accepted"
}
```
hoặc
```json
{
  "status": "Rejected"
}
```

**Response:** `200 OK`
```json
{
  "message": "Lời mời đã được chấp nhận."
}
```

**Cách 2: Sử dụng endpoint riêng (không cần body) - Khuyên dùng**

**Chấp nhận lời mời:**
```http
PUT /api/UserJobs/AcceptInvitation/{id}
Authorization: Bearer {student_token}
```

**Response:** `200 OK`
```json
{
  "message": "Lời mời đã được chấp nhận."
}
```

**Từ chối lời mời:**
```http
PUT /api/UserJobs/RejectInvitation/{id}
Authorization: Bearer {student_token}
```

**Response:** `200 OK`
```json
{
  "message": "Lời mời đã được từ chối."
}
```

**Errors (cả 3 endpoint):**
- `404` - Không tìm thấy lời mời
- `400` - "Đây không phải là lời mời" (nếu Type != "Invitation")
- `400` - "Lời mời này đã được phản hồi rồi" (nếu Status != "Pending")
- `403` - Không có quyền (không phải lời mời cho bạn)

**Notes:**
- Chỉ có thể phản hồi khi `status = "Pending"`
- Endpoint riêng (AcceptInvitation/RejectInvitation) đơn giản hơn vì không cần gửi body

---

### 1.6 Student xóa đơn ứng tuyển (khi Pending)
```http
DELETE /api/UserJobs/{id}
Authorization: Bearer {student_token}
```

**Response:** `204 No Content`

**Business Rules:**
- Chỉ xóa được đơn Application của mình khi Status = "Pending"
- Không thể xóa khi đã Accepted/Rejected

---

## 🏢 LUỒNG 2: INVITATION (Company mời ứng viên)


### 2.1 Company xem danh sách ứng viên gợi ý
```http
GET /api/Jobs/CandidateSuggestions/{jobId}?top=10
Authorization: Bearer {company_token}
```

**Response:**
```json
[
  {
    "id": 1,
    "userId": "student-guid-123",
    "name": "Nguyễn Văn A",
    "avatarUrl": "/avatars/student123.jpg",
    "skills": "Python, Django, PostgreSQL",
    "gpa": "3.8",
    "major": "Computer Science",
    "similarityScore": 0.89
  }
]
```

**Notes:**
- Chỉ hiển thị ứng viên có `openToWork = true`
- Sắp xếp theo độ tương đồng giảm dần

---

### 2.2 Company gửi lời mời cho ứng viên
```http
POST /api/UserJobs/SendInvitation
Authorization: Bearer {company_token}
Content-Type: application/json
```

**Request Body:**
```json
{
  "studentUserId": "student-guid-123",
  "jobId": 5
}
```

**Response:** `200 OK`
```json
{
  "id": 102,
  "userId": "student-guid-123",
  "studentName": "Nguyễn Văn A",
  "studentAvatar": "/avatars/student123.jpg",
  "jobId": 5,
  "jobTitle": "Backend Developer Intern",
  "companyName": "ABC Technology",
  "type": "Invitation",
  "status": "Pending",
  "updatedBy": "company-guid-456",
  "createdAt": "2025-12-12T15:00:00Z",
  "updatedAt": "2025-12-12T15:00:00Z"
}
```

**Errors:**
- `404` - Không tìm thấy công việc
- `404` - Không tìm thấy ứng viên
- `403` - Công việc không thuộc công ty của bạn
- `400` - "Đã có quan hệ tuyển dụng với ứng viên này cho công việc này"

**Notes:**
- `type` tự động = `"Invitation"`
- `status` tự động = `"Pending"`
- Sinh viên sẽ nhận được lời mời và có thể Accept/Reject

---

### 2.3 Company xem lời mời đã gửi
```http
GET /api/UserJobs/SentInvitations
Authorization: Bearer {company_token}
```

**Response:** `200 OK`
```json
[
  {
    "id": 102,
    "userId": "student-guid-123",
    "studentName": "Nguyễn Văn A",
    "studentAvatar": "/avatars/student123.jpg",
    "jobId": 5,
    "jobTitle": "Backend Developer Intern",
    "companyName": "ABC Technology",
    "type": "Invitation",
    "status": "Pending",
    "updatedBy": "company-guid-456",
    "createdAt": "2025-12-12T15:00:00Z",
    "updatedAt": "2025-12-12T15:00:00Z"
  }
]
```

**Notes:**
- Hiển thị tất cả lời mời của các công việc thuộc công ty
- Chỉ hiển thị các bản ghi có `type = "Invitation"`

---

### 2.4 Company xem đơn ứng tuyển cho 1 công việc
```http
GET /api/UserJobs/ReceivedApplications/{jobId}
Authorization: Bearer {company_token}
```

**Response:** `200 OK`
```json
[
  {
    "id": 101,
    "userId": "student-guid-789",
    "studentName": "Trần Thị B",
    "studentAvatar": "/avatars/student789.jpg",
    "jobId": 5,
    "jobTitle": "Backend Developer Intern",
    "companyName": "ABC Technology",
    "type": "Application",
    "status": "Pending",
    "updatedBy": "student-guid-789",
    "createdAt": "2025-12-12T09:00:00Z",
    "updatedAt": "2025-12-12T09:00:00Z"
  }
]
```

**Authorization:**
- Chỉ Company sở hữu công việc mới xem được
- Admin xem được tất cả

**Notes:**
- Chỉ hiển thị các bản ghi có `type = "Application"`

---

### 2.5 Company xem tất cả đơn ứng tuyển
```http
GET /api/UserJobs/AllReceivedApplications
Authorization: Bearer {company_token}
```

**Response:** `200 OK`
```json
[
  {
    "id": 101,
    "userId": "student-guid-789",
    "studentName": "Trần Thị B",
    "studentAvatar": "/avatars/student789.jpg",
    "jobId": 5,
    "jobTitle": "Backend Developer Intern",
    "companyName": "ABC Technology",
    "type": "Application",
    "status": "Pending",
    "updatedBy": "student-guid-789",
    "createdAt": "2025-12-12T09:00:00Z",
    "updatedAt": "2025-12-12T09:00:00Z"
  },
  {
    "id": 103,
    "userId": "student-guid-456",
    "studentName": "Lê Văn C",
    "studentAvatar": "/avatars/student456.jpg",
    "jobId": 8,
    "jobTitle": "Frontend Developer",
    "companyName": "ABC Technology",
    "type": "Application",
    "status": "Accepted",
    "updatedBy": "company-guid-456",
    "createdAt": "2025-12-11T10:00:00Z",
    "updatedAt": "2025-12-11T14:00:00Z"
  }
]
```

**Notes:**
- Hiển thị tất cả đơn ứng tuyển cho TẤT CẢ công việc của công ty
- Chỉ hiển thị các bản ghi có `type = "Application"`

---

### 2.6 Company phản hồi đơn ứng tuyển (Accept/Reject)

**Cách 1: Sử dụng endpoint chung (cần body)**
```http
PUT /api/UserJobs/RespondApplication/{id}
Authorization: Bearer {company_token}
Content-Type: application/json
```

**Request Body:**
```json
{
  "status": "Accepted"
}
```
hoặc
```json
{
  "status": "Rejected"
}
```

**Response:** `200 OK`
```json
{
  "message": "Đơn ứng tuyển đã được chấp nhận."
}
```

**Cách 2: Sử dụng endpoint riêng (không cần body) - Khuyên dùng**

**Chấp nhận đơn ứng tuyển:**
```http
PUT /api/UserJobs/AcceptApplication/{id}
Authorization: Bearer {company_token}
```

**Response:** `200 OK`
```json
{
  "message": "Đơn ứng tuyển đã được chấp nhận."
}
```

**Từ chối đơn ứng tuyển:**
```http
PUT /api/UserJobs/RejectApplication/{id}
Authorization: Bearer {company_token}
```

**Response:** `200 OK`
```json
{
  "message": "Đơn ứng tuyển đã được từ chối."
}
```

**Errors (cả 3 endpoint):**
- `404` - Không tìm thấy đơn ứng tuyển
- `403` - Không có quyền (công việc không thuộc công ty bạn)
- `400` - "Đây không phải là đơn ứng tuyển" (nếu Type != "Application")
- `400` - "Đơn ứng tuyển này đã được phản hồi rồi" (nếu Status != "Pending")

**Notes:**
- Chỉ có thể phản hồi khi `status = "Pending"`
- Endpoint riêng (AcceptApplication/RejectApplication) đơn giản hơn vì không cần gửi body

---

### 2.7 Company xóa lời mời (khi Pending)
```http
DELETE /api/UserJobs/{id}
Authorization: Bearer {company_token}
```

**Response:** `204 No Content`

**Business Rules:**
- Chỉ xóa được lời mời Invitation của mình khi Status = "Pending"
- Không thể xóa khi đã Accepted/Rejected

---

## 🔍 API CHUNG

### 3.1 Xem chi tiết một UserJob
```http
GET /api/UserJobs/{id}
Authorization: Bearer {token}
```

**Response:** `200 OK`
```json
{
  "id": 101,
  "userId": "student-guid-123",
  "studentName": "Nguyễn Văn A",
  "studentAvatar": "/avatars/student123.jpg",
  "jobId": 5,
  "jobTitle": "Backend Developer Intern",
  "companyName": "ABC Technology",
  "type": "Application",
  "status": "Pending",
  "updatedBy": "student-guid-123",
  "createdAt": "2025-12-12T10:00:00Z",
  "updatedAt": "2025-12-12T10:00:00Z"
}
```

**Authorization:**
- Student: Chỉ xem được UserJob của mình (userId)
- Company: Chỉ xem được UserJob liên quan đến công việc của mình
- Admin: Xem được tất cả

**Errors:**
- `404` - Không tìm thấy
- `403` - Không có quyền truy cập

---

## 📊 Bảng tổng hợp API

| Endpoint | Method | Role | Mô tả |
|----------|--------|------|-------|
| `/Apply` | POST | Student | Ứng tuyển công việc |
| `/MyApplications` | GET | Student | Xem đơn đã gửi |
| `/MyInvitations` | GET | Student | Xem lời mời nhận được |
| `/RespondInvitation/{id}` | PUT | Student | Phản hồi lời mời (cần body) |
| `/AcceptInvitation/{id}` | PUT | Student | Chấp nhận lời mời (không cần body) |
| `/RejectInvitation/{id}` | PUT | Student | Từ chối lời mời (không cần body) |
| `/SendInvitation` | POST | Company | Gửi lời mời |
| `/SentInvitations` | GET | Company | Xem lời mời đã gửi |
| `/ReceivedApplications/{jobId}` | GET | Company | Xem đơn của 1 job |
| `/AllReceivedApplications` | GET | Company | Xem tất cả đơn |
| `/RespondApplication/{id}` | PUT | Company | Phản hồi đơn ứng tuyển (cần body) |
| `/AcceptApplication/{id}` | PUT | Company | Chấp nhận đơn (không cần body) |
| `/RejectApplication/{id}` | PUT | Company | Từ chối đơn (không cần body) |
| `/{id}` | GET | All | Xem chi tiết |
| `/{id}` | DELETE | Student/Company | Xóa (soft delete) |

---

## ⚠️ Lưu ý quan trọng

### Phân quyền:
1. **Student** chỉ thao tác với:
   - Applications (đơn ứng tuyển) của mình
   - Invitations (lời mời) gửi đến mình

2. **Company** chỉ thao tác với:
   - Applications gửi đến công việc của mình
   - Invitations do mình gửi đi

3. **Admin** có toàn quyền

### Quy tắc nghiệp vụ:
- **Type = "Application"**: Student tạo → Company phản hồi
- **Type = "Invitation"**: Company tạo → Student phản hồi
- **Status = "Pending"**: Đợi phản hồi, có thể xóa
- **Status = "Accepted/Rejected"**: Trạng thái cuối, không thể thay đổi

### Validation:
- Không thể ứng tuyển/mời 2 lần cho cùng 1 cặp (Student, Job)
- Chỉ người nhận (recipient) mới có quyền Accept/Reject
- Chỉ người tạo mới có quyền xóa (khi Pending)

---

## 🔐 Authentication

Tất cả API đều yêu cầu JWT Bearer Token:

```http
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

Token có chứa:
- `ClaimTypes.NameIdentifier` - User ID
- `ClaimTypes.Role` - Student/Company/Admin

---

## 📝 Error Responses

### 400 Bad Request
```json
{
  "message": "Bạn đã ứng tuyển công việc này rồi."
}
```

### 401 Unauthorized
```json
{
  "message": "Unauthorized"
}
```

### 403 Forbidden
```json
{
  "message": "Forbidden"
}
```

### 404 Not Found
```json
{
  "message": "Không tìm thấy công việc."
}
```

---

## 💡 Use Cases (Ví dụ thực tế)

### Use Case 1: Sinh viên ứng tuyển công việc
1. Student gọi `GET /api/StudentInfors/JobSuggestions/{studentInforId}` để xem công việc phù hợp
2. Chọn công việc, gọi `POST /api/UserJobs/Apply` với `jobId`
3. Hệ thống tạo UserJob với `Type="Application"`, `Status="Pending"`
4. Company nhận thông báo, xem đơn qua `GET /api/UserJobs/ReceivedApplications/{jobId}`
5. Company quyết định: `PUT /api/UserJobs/RespondApplication/{id}` với `status="Accepted"` hoặc `"Rejected"`

### Use Case 2: Công ty mời ứng viên
1. Company gọi `GET /api/Jobs/CandidateSuggestions/{jobId}` để xem ứng viên phù hợp
2. Chọn ứng viên, gọi `POST /api/UserJobs/SendInvitation` với `studentUserId` và `jobId`
3. Hệ thống tạo UserJob với `Type="Invitation"`, `Status="Pending"`
4. Student nhận thông báo, xem lời mời qua `GET /api/UserJobs/MyInvitations`
5. Student quyết định: `PUT /api/UserJobs/RespondInvitation/{id}` với `status="Accepted"` hoặc `"Rejected"`

### Use Case 3: Hủy đơn/lời mời
- Student muốn hủy đơn ứng tuyển đang Pending: `DELETE /api/UserJobs/{id}`
- Company muốn hủy lời mời đang Pending: `DELETE /api/UserJobs/{id}`
- Chỉ xóa được khi Status còn là "Pending"

---

## 🔄 Flow Diagram

```
APPLICATION FLOW:
┌─────────┐                 ┌─────────┐
│ Student │                 │ Company │
└────┬────┘                 └────┬────┘
     │                           │
     │  POST /Apply              │
     │──────────────────────────>│
     │  (Type=Application,       │
     │   Status=Pending)         │
     │                           │
     │                      GET /ReceivedApplications
     │                           │
     │                      PUT /RespondApplication
     │<──────────────────────────│
     │  (Status=Accepted/        │
     │   Rejected)               │
     └                           ┘

INVITATION FLOW:
┌─────────┐                 ┌─────────┐
│ Student │                 │ Company │
└────┬────┘                 └────┬────┘
     │                           │
     │         POST /SendInvitation
     │<──────────────────────────│
     │  (Type=Invitation,        │
     │   Status=Pending)         │
     │                           │
GET /MyInvitations                │
     │                           │
PUT /RespondInvitation            │
     │──────────────────────────>│
     │  (Status=Accepted/        │
     │   Rejected)               │
     └                           ┘
```

---

## 📱 Frontend Integration Guide

### Màn hình Student - Trang ứng tuyển
```javascript
// 1. Lấy danh sách công việc gợi ý
const jobs = await fetch('/api/StudentInfors/JobSuggestions/1?top=10', {
  headers: { 'Authorization': `Bearer ${studentToken}` }
});

// 2. Ứng tuyển
const applyResult = await fetch('/api/UserJobs/Apply', {
  method: 'POST',
  headers: {
    'Authorization': `Bearer ${studentToken}`,
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({ jobId: 5 })
});

// 3. Xem đơn đã gửi
const myApps = await fetch('/api/UserJobs/MyApplications', {
  headers: { 'Authorization': `Bearer ${studentToken}` }
});
```

### Màn hình Student - Trang lời mời
```javascript
// 1. Xem lời mời
const invitations = await fetch('/api/UserJobs/MyInvitations', {
  headers: { 'Authorization': `Bearer ${studentToken}` }
});

// 2. Chấp nhận lời mời (Cách đơn giản - Khuyên dùng)
const acceptResult = await fetch('/api/UserJobs/AcceptInvitation/102', {
  method: 'PUT',
  headers: {
    'Authorization': `Bearer ${studentToken}`
  }
});

// 3. Từ chối lời mời (Cách đơn giản - Khuyên dùng)
const rejectResult = await fetch('/api/UserJobs/RejectInvitation/102', {
  method: 'PUT',
  headers: {
    'Authorization': `Bearer ${studentToken}`
  }
});

// Hoặc dùng cách cũ (cần body)
const respondResult = await fetch('/api/UserJobs/RespondInvitation/102', {
  method: 'PUT',
  headers: {
    'Authorization': `Bearer ${studentToken}`,
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({ status: 'Accepted' })
});
```

### Màn hình Company - Trang tuyển dụng
```javascript
// 1. Lấy ứng viên gợi ý
const candidates = await fetch('/api/Jobs/CandidateSuggestions/5?top=10', {
  headers: { 'Authorization': `Bearer ${companyToken}` }
});

// 2. Gửi lời mời
const inviteResult = await fetch('/api/UserJobs/SendInvitation', {
  method: 'POST',
  headers: {
    'Authorization': `Bearer ${companyToken}`,
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({
    studentUserId: 'student-guid-123',
    jobId: 5
  })
});

// 3. Xem đơn ứng tuyển
const applications = await fetch('/api/UserJobs/ReceivedApplications/5', {
  headers: { 'Authorization': `Bearer ${companyToken}` }
});

// 4. Chấp nhận đơn (Cách đơn giản - Khuyên dùng)
const acceptApp = await fetch('/api/UserJobs/AcceptApplication/101', {
  method: 'PUT',
  headers: {
    'Authorization': `Bearer ${companyToken}`
  }
});

// 5. Từ chối đơn (Cách đơn giản - Khuyên dùng)
const rejectApp = await fetch('/api/UserJobs/RejectApplication/101', {
  method: 'PUT',
  headers: {
    'Authorization': `Bearer ${companyToken}`
  }
});

// Hoặc dùng cách cũ (cần body)
const respondApp = await fetch('/api/UserJobs/RespondApplication/101', {
  method: 'PUT',
  headers: {
    'Authorization': `Bearer ${companyToken}`,
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({ status: 'Accepted' })
});
```

---

## 🧪 Testing Guide

### Test Student Flow
```bash
# 1. Register & Login as Student
POST /api/Auth/RegisterStudent
POST /api/Auth/Login

# 2. Apply for job
POST /api/UserJobs/Apply
Body: { "jobId": 5 }

# 3. Check applications
GET /api/UserJobs/MyApplications

# 4. Check invitations
GET /api/UserJobs/MyInvitations

# 5. Chấp nhận lời mời (Cách đơn giản)
PUT /api/UserJobs/AcceptInvitation/102

# 6. Từ chối lời mời (Cách đơn giản)
PUT /api/UserJobs/RejectInvitation/102

# Hoặc dùng cách cũ (cần body)
PUT /api/UserJobs/RespondInvitation/102
Body: { "status": "Accepted" }
```

### Test Company Flow
```bash
# 1. Register & Login as Company
POST /api/Auth/RegisterCompany
POST /api/Auth/Login

# 2. Send invitation
POST /api/UserJobs/SendInvitation
Body: { "studentUserId": "...", "jobId": 5 }

# 3. Check sent invitations
GET /api/UserJobs/SentInvitations

# 4. Check received applications
GET /api/UserJobs/ReceivedApplications/5

# 5. Chấp nhận đơn ứng tuyển (Cách đơn giản)
PUT /api/UserJobs/AcceptApplication/101

# 6. Từ chối đơn ứng tuyển (Cách đơn giản)
PUT /api/UserJobs/RejectApplication/101

# Hoặc dùng cách cũ (cần body)
PUT /api/UserJobs/RespondApplication/101
Body: { "status": "Accepted" }
```

---

## ❓ FAQ

**Q: Sinh viên có thể ứng tuyển nhiều lần vào cùng 1 công việc không?**  
A: Không. Hệ thống sẽ trả về lỗi 400 "Bạn đã ứng tuyển công việc này rồi".

**Q: Công ty có thể gửi nhiều lời mời cho cùng 1 sinh viên cho cùng 1 công việc không?**  
A: Không. Hệ thống kiểm tra và trả về lỗi 400 nếu đã có quan hệ tuyển dụng.

**Q: Sau khi Accept/Reject có thể thay đổi quyết định không?**  
A: Không. Accept và Reject là trạng thái cuối, không thể thay đổi.

**Q: Student có thể xóa đơn ứng tuyển đã được Company Accept không?**  
A: Không. Chỉ xóa được khi Status = "Pending".

**Q: Company có thể xem thông tin sinh viên khi nhận đơn ứng tuyển không?**  
A: Có. Response bao gồm `studentName` và `studentAvatar` để hiển thị thông tin cơ bản.

**Q: Làm sao phân biệt đơn ứng tuyển và lời mời?**  
A: Dựa vào field `type`:
- `"Application"` = Sinh viên ứng tuyển
- `"Invitation"` = Công ty mời

**Q: Ai có quyền Accept/Reject?**  
A:
- Application: Company có quyền Accept/Reject
- Invitation: Student có quyền Accept/Reject
- Nguyên tắc: Người nhận (recipient) mới có quyền phản hồi

**Q: Có bao nhiêu cách để Accept/Reject?**  
A: Có 2 cách:
- **Cách 1 (Khuyên dùng):** Dùng endpoint riêng `/AcceptInvitation/{id}`, `/RejectInvitation/{id}`, `/AcceptApplication/{id}`, `/RejectApplication/{id}` - Không cần body, đơn giản hơn
- **Cách 2:** Dùng endpoint chung `/RespondInvitation/{id}`, `/RespondApplication/{id}` - Cần gửi body `{ "status": "Accepted" }` hoặc `{ "status": "Rejected" }`

---

**Cập nhật lần cuối:** 12/12/2025  
**Phiên bản:** 2.1 (Thêm Accept/Reject endpoints)

### Rule 1: Final States Cannot Be Changed
```
Accepted → ❌ Không thể thay đổi
Rejected → ❌ Không thể thay đổi
Withdrawn → ❌ Không thể thay đổi
```

### Rule 2: Valid Status Transitions
```
Applied → Reviewing ✅
Applied → Accepted ✅
Applied → Rejected ✅
Applied → Withdrawn ✅

Reviewing → Accepted ✅
Reviewing → Rejected ✅
Reviewing → Withdrawn ✅

Other transitions → ❌
```

### Rule 3: Withdrawn Only by Creator
- **Student Apply:** Chỉ Student có quyền Withdrawn
- **Company Recruit:** Chỉ Company có quyền Withdrawn

### Rule 4: Accept/Reject by Opposite Party
- **Student Apply:** Chỉ Company có quyền Accept/Reject
- **Company Recruit:** Chỉ Student có quyền Accept/Reject

---

## 🎯 Phân biệt Student Apply vs Company Recruit

Sử dụng logic:

```javascript
// Frontend logic
const isStudentInitiated = (userJob) => {
  const isNewRecord = userJob.createdAt === userJob.updatedAt;
  return isNewRecord && userJob.userId === userJob.updatedBy;
};

const isCompanyInitiated = (userJob) => {
  const isNewRecord = userJob.createdAt === userJob.updatedAt;
  return isNewRecord && userJob.userId !== userJob.updatedBy;
};
```

**Ví dụ:**
```json
// Student Apply
{
  "userId": "student-123",
  "updatedBy": "student-123",  // ← Giống nhau
  "createdAt": "2025-01-15T10:00:00Z",
  "updatedAt": "2025-01-15T10:00:00Z"  // ← Bằng nhau
}

// Company Recruit
{
  "userId": "student-123",
  "updatedBy": "company-456",  // ← Khác nhau
  "createdAt": "2025-01-15T10:00:00Z",
  "updatedAt": "2025-01-15T10:00:00Z"  // ← Bằng nhau
}
```

---

## ⚠️ Error Responses

### 400 Bad Request
```json
{
  "message": "Không thể thay đổi trạng thái 'Accepted'. Đây là trạng thái kết thúc."
}
```

### 401 Unauthorized
```json
{
  "message": "Invalid token"
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
  "message": "Không tìm thấy đơn ứng tuyển."
}
```

---

## 📝 Frontend Implementation Guide

### React Example: Student Apply Flow

```typescript
// 1. Student xem gợi ý và apply
const applyForJob = async (jobId: number) => {
  const response = await fetch('/api/UserJobs', {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({
      jobId: jobId,
      status: 'Applied'
    })
  });
  
  if (response.ok) {
    const data = await response.json();
    console.log('Applied successfully:', data);
  }
};

// 2. Student xem danh sách đơn đã nộp
const getMyApplications = async () => {
  const response = await fetch('/api/UserJobs/MyApplications', {
    headers: {
      'Authorization': `Bearer ${token}`
    }
  });
  
  const applications = await response.json();
  return applications;
};

// 3. Student rút đơn
const withdrawApplication = async (id: number) => {
  const response = await fetch(`/api/UserJobs/${id}`, {
    method: 'PUT',
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({
      status: 'Withdrawn'
    })
  });
  
  if (response.ok) {
    console.log('Withdrawn successfully');
  }
};
```

### React Example: Company Recruit Flow

```typescript
// 1. Company xem gợi ý ứng viên
const getCandidateSuggestions = async (jobId: number) => {
  const response = await fetch(`/api/Jobs/CandidateSuggestions/${jobId}?top=10`, {
    headers: {
      'Authorization': `Bearer ${companyToken}`
    }
  });
  
  const candidates = await response.json();
  return candidates;
};

// 2. Company mời ứng viên
const inviteCandidate = async (userId: string, jobId: number) => {
  const response = await fetch('/api/UserJobs/InviteCandidate', {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${companyToken}`,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({
      userId: userId,
      jobId: jobId
    })
  });
  
  if (response.ok) {
    const data = await response.json();
    console.log('Invited successfully:', data);
  }
};

// 3. Company xem danh sách ứng viên cho job
const getJobApplications = async (jobId: number) => {
  const response = await fetch(`/api/UserJobs/JobApplications/${jobId}`, {
    headers: {
      'Authorization': `Bearer ${companyToken}`
    }
  });
  
  const applications = await response.json();
  return applications;
};

// 4. Company chấp nhận/từ chối ứng viên
const updateApplicationStatus = async (id: number, status: string) => {
  const response = await fetch(`/api/UserJobs/${id}`, {
    method: 'PUT',
    headers: {
      'Authorization': `Bearer ${companyToken}`,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({
      status: status  // 'Accepted' or 'Rejected'
    })
  });
  
  if (response.ok) {
    console.log('Status updated successfully');
  }
};
```

---

## 🎨 UI/UX Recommendations

### Status Display Colors:
- **Applied:** 🔵 Blue (Chờ xử lý)
- **Reviewing:** 🟡 Yellow (Đang xem xét)
- **Accepted:** 🟢 Green (Thành công)
- **Rejected:** 🔴 Red (Thất bại)
- **Withdrawn:** ⚫ Gray (Đã hủy)

### Action Buttons by Role:

**Student viewing own application:**
- Applied/Reviewing → Button: "Rút đơn" (Withdrawn)
- Accepted/Rejected/Withdrawn → No action

**Student viewing company invitation:**
- Applied → Buttons: "Cân nhắc" (Reviewing), "Từ chối" (Rejected)
- Reviewing → Buttons: "Chấp nhận" (Accepted), "Từ chối" (Rejected)
- Accepted/Rejected/Withdrawn → No action

**Company viewing student application:**
- Applied → Buttons: "Xem xét" (Reviewing), "Chấp nhận" (Accepted), "Từ chối" (Rejected)
- Reviewing → Buttons: "Chấp nhận" (Accepted), "Từ chối" (Rejected)
- Accepted/Rejected/Withdrawn → No action

**Company viewing own invitation:**
- Applied/Reviewing → Button: "Rút lời mời" (Withdrawn)
- Accepted/Rejected/Withdrawn → No action

---

## 📈 Testing Scenarios

### Test Case 1: Student Apply Success Flow
1. Student POST `/api/UserJobs` với jobId → Status = Applied ✅
2. Company PUT status → Reviewing ✅
3. Company PUT status → Accepted ✅
4. Student/Company PUT status → Error (Final state) ✅

### Test Case 2: Company Recruit Success Flow
1. Company POST `/api/UserJobs/InviteCandidate` → Status = Applied ✅
2. Student PUT status → Reviewing ✅
3. Student PUT status → Accepted ✅

### Test Case 3: Student Withdraw
1. Student POST apply → Status = Applied ✅
2. Student PUT status → Withdrawn ✅
3. Company PUT status → Error (Final state) ✅

### Test Case 4: Invalid Transitions
1. Student POST apply → Applied ✅
2. Student PUT status → Accepted ❌ (Error: Sinh viên không thể tự chấp nhận)
3. Company PUT status → Withdrawn ❌ (Error: Chỉ người tạo mới rút được)

---

**Version:** 1.0  
**Last Updated:** January 15, 2025  
**Contact:** Backend Team
