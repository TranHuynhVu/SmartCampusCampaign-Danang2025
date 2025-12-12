# UserJob API Documentation

## 📋 Tổng quan

API UserJobs hỗ trợ **hai luồng nghiệp vụ chính**:
1. **Student Apply** - Sinh viên nộp đơn ứng tuyển vào công việc
2. **Company Recruit** - Công ty mời ứng viên làm việc

**Base URL:** `/api/UserJobs`

---

## 🔄 Status Flow (Luồng trạng thái)

### Status Values:
| Status | Ý nghĩa |
|--------|---------|
| **Applied** | Đơn đã được tạo (Student apply hoặc Company invite) |
| **Reviewing** | Đang được xem xét bởi bên đối diện |
| **Accepted** | Đã chấp nhận (FINAL STATE) |
| **Rejected** | Đã từ chối (FINAL STATE) |
| **Withdrawn** | Đã rút lại (FINAL STATE) |

### Status Transitions:
```
Applied → Reviewing → Accepted/Rejected
   ↓
Withdrawn
```

---

## 🎓 LUỒNG 1: STUDENT APPLY

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

### 1.2 Student nộp đơn ứng tuyển
```http
POST /api/UserJobs
Authorization: Bearer {student_token}
Content-Type: application/json
```

**Request Body:**
```json
{
  "jobId": 5,
  "status": "Applied"
}
```

**Response:** `201 Created`
```json
{
  "id": 101,
  "userId": "student-guid-123",
  "jobId": 5,
  "jobTitle": "Backend Developer Intern",
  "companyName": "ABC Technology",
  "status": "Applied",
  "updatedBy": "student-guid-123",
  "createdAt": "2025-01-15T10:00:00Z",
  "updatedAt": "2025-01-15T10:00:00Z"
}
```

**Notes:**
- Status tự động set thành `"Applied"`
- `updatedBy` = Student UserId (đánh dấu Student là người tạo)

---

### 1.3 Student xem danh sách đơn đã nộp
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
    "jobId": 5,
    "jobTitle": "Backend Developer Intern",
    "companyName": "ABC Technology",
    "status": "Applied",
    "updatedBy": "student-guid-123",
    "createdAt": "2025-01-15T10:00:00Z",
    "updatedAt": "2025-01-15T10:00:00Z"
  }
]
```

**Notes:**
- Chỉ hiển thị các đơn mà Student là người tạo
- Logic: `userId == updatedBy` tại thời điểm tạo

---

### 1.4 Student rút đơn ứng tuyển
```http
PUT /api/UserJobs/{id}
Authorization: Bearer {student_token}
Content-Type: application/json
```

**Request Body:**
```json
{
  "status": "Withdrawn"
}
```

**Response:** `200 OK`
```json
{
  "id": 101,
  "status": "Withdrawn",
  "updatedBy": "student-guid-123",
  "updatedAt": "2025-01-15T11:00:00Z"
}
```

**Business Rules:**
- Chỉ Student tạo đơn mới có quyền rút
- Có thể rút khi Status = `Applied` hoặc `Reviewing`
- Không thể rút khi Status = `Accepted`, `Rejected`, `Withdrawn`

---

### 1.5 Company xem danh sách đơn ứng tuyển
```http
GET /api/UserJobs/JobApplications/{jobId}
Authorization: Bearer {company_token}
```

**Response:** `200 OK`
```json
[
  {
    "id": 101,
    "userId": "student-guid-123",
    "jobId": 5,
    "jobTitle": "Backend Developer Intern",
    "companyName": "ABC Technology",
    "status": "Applied",
    "updatedBy": "student-guid-123",
    "createdAt": "2025-01-15T10:00:00Z",
    "updatedAt": "2025-01-15T10:00:00Z"
  }
]
```

**Authorization:**
- Company chỉ xem được đơn cho công việc của mình
- Admin xem được tất cả

---

### 1.6 Company chuyển trạng thái đơn thành Reviewing
```http
PUT /api/UserJobs/{id}
Authorization: Bearer {company_token}
Content-Type: application/json
```

**Request Body:**
```json
{
  "status": "Reviewing"
}
```

**Response:** `200 OK`

---

### 1.7 Company chấp nhận ứng viên
```http
PUT /api/UserJobs/{id}
Authorization: Bearer {company_token}
Content-Type: application/json
```

**Request Body:**
```json
{
  "status": "Accepted"
}
```

**Response:** `200 OK`
```json
{
  "id": 101,
  "status": "Accepted",
  "updatedBy": "company-guid-456",
  "updatedAt": "2025-01-15T14:00:00Z"
}
```

**Notes:**
- Trạng thái `Accepted` là **FINAL STATE**
- Không thể thay đổi sau khi Accepted

---

### 1.8 Company từ chối ứng viên
```http
PUT /api/UserJobs/{id}
Authorization: Bearer {company_token}
Content-Type: application/json
```

**Request Body:**
```json
{
  "status": "Rejected"
}
```

**Response:** `200 OK`

**Notes:**
- Trạng thái `Rejected` là **FINAL STATE**

---

## 🏢 LUỒNG 2: COMPANY RECRUIT

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
    "skills": "Python, Django, PostgreSQL",
    "gpa": "3.8",
    "major": "Computer Science",
    "similarityScore": 0.89
  }
]
```

---

### 2.2 Company mời ứng viên
```http
POST /api/UserJobs/InviteCandidate
Authorization: Bearer {company_token}
Content-Type: application/json
```

**Request Body:**
```json
{
  "userId": "student-guid-123",
  "jobId": 5
}
```

**Response:** `201 Created`
```json
{
  "id": 102,
  "userId": "student-guid-123",
  "jobId": 5,
  "jobTitle": "Backend Developer Intern",
  "companyName": "ABC Technology",
  "status": "Applied",
  "updatedBy": "company-guid-456",
  "createdAt": "2025-01-15T10:00:00Z",
  "updatedAt": "2025-01-15T10:00:00Z"
}
```

**Notes:**
- Status tự động set thành `"Applied"`
- `updatedBy` = Company UserId (đánh dấu Company là người tạo)
- Phân biệt với Student Apply qua `userId != updatedBy`

**Validation:**
- Company phải sở hữu Job
- Student phải tồn tại trong hệ thống
- Không được tồn tại quan hệ trước đó

---

### 2.3 Student xem lời mời từ Company
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
    "jobId": 5,
    "jobTitle": "Backend Developer Intern",
    "companyName": "ABC Technology",
    "status": "Applied",
    "updatedBy": "company-guid-456",
    "createdAt": "2025-01-15T10:00:00Z",
    "updatedAt": "2025-01-15T10:00:00Z"
  }
]
```

**Notes:**
- Chỉ hiển thị lời mời từ Company
- Logic: `userId != updatedBy` tại thời điểm tạo

---

### 2.4 Student chuyển trạng thái lời mời thành Reviewing
```http
PUT /api/UserJobs/{id}
Authorization: Bearer {student_token}
Content-Type: application/json
```

**Request Body:**
```json
{
  "status": "Reviewing"
}
```

**Response:** `200 OK`

---

### 2.5 Student chấp nhận lời mời
```http
PUT /api/UserJobs/{id}
Authorization: Bearer {student_token}
Content-Type: application/json
```

**Request Body:**
```json
{
  "status": "Accepted"
}
```

**Response:** `200 OK`
```json
{
  "id": 102,
  "status": "Accepted",
  "updatedBy": "student-guid-123",
  "updatedAt": "2025-01-15T12:00:00Z"
}
```

---

### 2.6 Student từ chối lời mời
```http
PUT /api/UserJobs/{id}
Authorization: Bearer {student_token}
Content-Type: application/json
```

**Request Body:**
```json
{
  "status": "Rejected"
}
```

**Response:** `200 OK`

---

### 2.7 Company rút lời mời
```http
PUT /api/UserJobs/{id}
Authorization: Bearer {company_token}
Content-Type: application/json
```

**Request Body:**
```json
{
  "status": "Withdrawn"
}
```

**Response:** `200 OK`

**Business Rules:**
- Chỉ Company tạo lời mời mới có quyền rút
- Có thể rút khi Status = `Applied` hoặc `Reviewing`

---

## 📊 API Tổng hợp

### GET /api/UserJobs
**Role:** Admin only

Lấy tất cả UserJobs trong hệ thống.

---

### GET /api/UserJobs/{id}
**Role:** Student (owner), Company (job owner), Admin

Lấy chi tiết một UserJob.

**Authorization:**
- Student: Chỉ xem được UserJob của mình
- Company: Chỉ xem được UserJob cho công việc của mình
- Admin: Xem tất cả

---

### DELETE /api/UserJobs/{id}
**Role:** Student (owner), Admin

Xóa mềm (soft delete) UserJob.

**Request:**
```http
DELETE /api/UserJobs/{id}
Authorization: Bearer {token}
```

**Response:** `204 No Content`

**Business Rules:**
- Chỉ Student tạo đơn hoặc Admin có quyền xóa
- Company không thể xóa (chỉ có thể Withdrawn)

---

## 🛡️ Business Rules Summary

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
