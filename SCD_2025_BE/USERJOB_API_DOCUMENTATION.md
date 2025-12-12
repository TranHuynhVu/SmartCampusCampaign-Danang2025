# UserJob API Documentation

## ?? T?ng quan

API UserJobs h? tr? **hai lu?ng nghi?p v? chính**:
1. **Student Apply** - Sinh viên n?p ??n ?ng tuy?n vào công vi?c
2. **Company Recruit** - Công ty m?i ?ng viên làm vi?c

**Base URL:** `/api/UserJobs`

---

## ?? Status Flow (Lu?ng tr?ng thái)

### Status Values:
| Status | Ý ngh?a |
|--------|---------|
| **Applied** | ??n ?ã ???c t?o (Student apply ho?c Company invite) |
| **Reviewing** | ?ang ???c xem xét b?i bên ??i di?n |
| **Accepted** | ?ã ch?p nh?n (FINAL STATE) |
| **Rejected** | ?ã t? ch?i (FINAL STATE) |
| **Withdrawn** | ?ã rút l?i (FINAL STATE) |

### Status Transitions:
```
Applied ? Reviewing ? Accepted/Rejected
   ?
Withdrawn
```

---

## ?? LU?NG 1: STUDENT APPLY

### 1.1 Student xem danh sách công vi?c g?i ý
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

### 1.2 Student n?p ??n ?ng tuy?n
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
- Status t? ??ng set thành `"Applied"`
- `updatedBy` = Student UserId (?ánh d?u Student là ng??i t?o)

---

### 1.3 Student xem danh sách ??n ?ã n?p
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
- Ch? hi?n th? các ??n mà Student là ng??i t?o
- Logic: `userId == updatedBy` t?i th?i ?i?m t?o

---

### 1.4 Student rút ??n ?ng tuy?n
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
- Ch? Student t?o ??n m?i có quy?n rút
- Có th? rút khi Status = `Applied` ho?c `Reviewing`
- Không th? rút khi Status = `Accepted`, `Rejected`, `Withdrawn`

---

### 1.5 Company xem danh sách ??n ?ng tuy?n
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
- Company ch? xem ???c ??n cho công vi?c c?a mình
- Admin xem ???c t?t c?

---

### 1.6 Company chuy?n tr?ng thái ??n thành Reviewing
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

### 1.7 Company ch?p nh?n ?ng viên
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
- Tr?ng thái `Accepted` là **FINAL STATE**
- Không th? thay ??i sau khi Accepted

---

### 1.8 Company t? ch?i ?ng viên
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
- Tr?ng thái `Rejected` là **FINAL STATE**

---

## ?? LU?NG 2: COMPANY RECRUIT

### 2.1 Company xem danh sách ?ng viên g?i ý
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
    "name": "Nguy?n V?n A",
    "skills": "Python, Django, PostgreSQL",
    "gpa": "3.8",
    "major": "Computer Science",
    "similarityScore": 0.89
  }
]
```

---

### 2.2 Company m?i ?ng viên
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
- Status t? ??ng set thành `"Applied"`
- `updatedBy` = Company UserId (?ánh d?u Company là ng??i t?o)
- Phân bi?t v?i Student Apply qua `userId != updatedBy`

**Validation:**
- Company ph?i s? h?u Job
- Student ph?i t?n t?i trong h? th?ng
- Không ???c t?n t?i quan h? tr??c ?ó

---

### 2.3 Student xem l?i m?i t? Company
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
- Ch? hi?n th? l?i m?i t? Company
- Logic: `userId != updatedBy` t?i th?i ?i?m t?o

---

### 2.4 Student chuy?n tr?ng thái l?i m?i thành Reviewing
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

### 2.5 Student ch?p nh?n l?i m?i
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

### 2.6 Student t? ch?i l?i m?i
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

### 2.7 Company rút l?i m?i
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
- Ch? Company t?o l?i m?i m?i có quy?n rút
- Có th? rút khi Status = `Applied` ho?c `Reviewing`

---

## ?? API T?ng h?p

### GET /api/UserJobs
**Role:** Admin only

L?y t?t c? UserJobs trong h? th?ng.

---

### GET /api/UserJobs/{id}
**Role:** Student (owner), Company (job owner), Admin

L?y chi ti?t m?t UserJob.

**Authorization:**
- Student: Ch? xem ???c UserJob c?a mình
- Company: Ch? xem ???c UserJob cho công vi?c c?a mình
- Admin: Xem t?t c?

---

### DELETE /api/UserJobs/{id}
**Role:** Student (owner), Admin

Xóa m?m (soft delete) UserJob.

**Request:**
```http
DELETE /api/UserJobs/{id}
Authorization: Bearer {token}
```

**Response:** `204 No Content`

**Business Rules:**
- Ch? Student t?o ??n ho?c Admin có quy?n xóa
- Company không th? xóa (ch? có th? Withdrawn)

---

## ??? Business Rules Summary

### Rule 1: Final States Cannot Be Changed
```
Accepted ? ? Không th? thay ??i
Rejected ? ? Không th? thay ??i
Withdrawn ? ? Không th? thay ??i
```

### Rule 2: Valid Status Transitions
```
Applied ? Reviewing ?
Applied ? Accepted ?
Applied ? Rejected ?
Applied ? Withdrawn ?

Reviewing ? Accepted ?
Reviewing ? Rejected ?
Reviewing ? Withdrawn ?

Other transitions ? ?
```

### Rule 3: Withdrawn Only by Creator
- **Student Apply:** Ch? Student có quy?n Withdrawn
- **Company Recruit:** Ch? Company có quy?n Withdrawn

### Rule 4: Accept/Reject by Opposite Party
- **Student Apply:** Ch? Company có quy?n Accept/Reject
- **Company Recruit:** Ch? Student có quy?n Accept/Reject

---

## ?? Phân bi?t Student Apply vs Company Recruit

S? d?ng logic:

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

**Ví d?:**
```json
// Student Apply
{
  "userId": "student-123",
  "updatedBy": "student-123",  // ? Gi?ng nhau
  "createdAt": "2025-01-15T10:00:00Z",
  "updatedAt": "2025-01-15T10:00:00Z"  // ? B?ng nhau
}

// Company Recruit
{
  "userId": "student-123",
  "updatedBy": "company-456",  // ? Khác nhau
  "createdAt": "2025-01-15T10:00:00Z",
  "updatedAt": "2025-01-15T10:00:00Z"  // ? B?ng nhau
}
```

---

## ?? Error Responses

### 400 Bad Request
```json
{
  "message": "Không th? thay ??i tr?ng thái 'Accepted'. ?ây là tr?ng thái k?t thúc."
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
  "message": "Không tìm th?y ??n ?ng tuy?n."
}
```

---

## ?? Frontend Implementation Guide

### React Example: Student Apply Flow

```typescript
// 1. Student xem g?i ý và apply
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

// 2. Student xem danh sách ??n ?ã n?p
const getMyApplications = async () => {
  const response = await fetch('/api/UserJobs/MyApplications', {
    headers: {
      'Authorization': `Bearer ${token}`
    }
  });
  
  const applications = await response.json();
  return applications;
};

// 3. Student rút ??n
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
// 1. Company xem g?i ý ?ng viên
const getCandidateSuggestions = async (jobId: number) => {
  const response = await fetch(`/api/Jobs/CandidateSuggestions/${jobId}?top=10`, {
    headers: {
      'Authorization': `Bearer ${companyToken}`
    }
  });
  
  const candidates = await response.json();
  return candidates;
};

// 2. Company m?i ?ng viên
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

// 3. Company xem danh sách ?ng viên cho job
const getJobApplications = async (jobId: number) => {
  const response = await fetch(`/api/UserJobs/JobApplications/${jobId}`, {
    headers: {
      'Authorization': `Bearer ${companyToken}`
    }
  });
  
  const applications = await response.json();
  return applications;
};

// 4. Company ch?p nh?n/t? ch?i ?ng viên
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

## ?? UI/UX Recommendations

### Status Display Colors:
- **Applied:** ?? Blue (Ch? x? lý)
- **Reviewing:** ?? Yellow (?ang xem xét)
- **Accepted:** ?? Green (Thành công)
- **Rejected:** ?? Red (Th?t b?i)
- **Withdrawn:** ? Gray (?ã h?y)

### Action Buttons by Role:

**Student viewing own application:**
- Applied/Reviewing ? Button: "Rút ??n" (Withdrawn)
- Accepted/Rejected/Withdrawn ? No action

**Student viewing company invitation:**
- Applied ? Buttons: "Cân nh?c" (Reviewing), "T? ch?i" (Rejected)
- Reviewing ? Buttons: "Ch?p nh?n" (Accepted), "T? ch?i" (Rejected)
- Accepted/Rejected/Withdrawn ? No action

**Company viewing student application:**
- Applied ? Buttons: "Xem xét" (Reviewing), "Ch?p nh?n" (Accepted), "T? ch?i" (Rejected)
- Reviewing ? Buttons: "Ch?p nh?n" (Accepted), "T? ch?i" (Rejected)
- Accepted/Rejected/Withdrawn ? No action

**Company viewing own invitation:**
- Applied/Reviewing ? Button: "Rút l?i m?i" (Withdrawn)
- Accepted/Rejected/Withdrawn ? No action

---

## ?? Testing Scenarios

### Test Case 1: Student Apply Success Flow
1. Student POST `/api/UserJobs` v?i jobId ? Status = Applied ?
2. Company PUT status ? Reviewing ?
3. Company PUT status ? Accepted ?
4. Student/Company PUT status ? Error (Final state) ?

### Test Case 2: Company Recruit Success Flow
1. Company POST `/api/UserJobs/InviteCandidate` ? Status = Applied ?
2. Student PUT status ? Reviewing ?
3. Student PUT status ? Accepted ?

### Test Case 3: Student Withdraw
1. Student POST apply ? Status = Applied ?
2. Student PUT status ? Withdrawn ?
3. Company PUT status ? Error (Final state) ?

### Test Case 4: Invalid Transitions
1. Student POST apply ? Applied ?
2. Student PUT status ? Accepted ? (Error: Sinh viên không th? t? ch?p nh?n)
3. Company PUT status ? Withdrawn ? (Error: Ch? ng??i t?o m?i rút ???c)

---

**Version:** 1.0  
**Last Updated:** January 15, 2025  
**Contact:** Backend Team
