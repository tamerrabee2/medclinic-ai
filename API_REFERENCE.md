# 📡 API Reference — MedClinic AI

Base URL: `https://your-domain/api/v1`  
All endpoints require `Authorization: Bearer {token}` unless marked 🔓

---

## 🔐 Auth

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| POST | `/auth/register` | 🔓 | Register new clinic + admin |
| POST | `/auth/login` | 🔓 | Login, get JWT + refresh token |
| POST | `/auth/refresh` | 🔓 | Refresh access token |
| POST | `/auth/logout` | ✅ | Revoke refresh token |
| POST | `/auth/forgot-password` | 🔓 | Send password reset email |
| POST | `/auth/reset-password` | 🔓 | Reset password with token |
| GET | `/auth/me` | ✅ | Get current user profile |

---

## 👤 Patients

| Method | Endpoint | Description |
|---|---|---|
| GET | `/patients` | List patients (paginated, searchable) |
| POST | `/patients` | Create patient |
| GET | `/patients/{id}` | Get patient details |
| PUT | `/patients/{id}` | Update patient |
| DELETE | `/patients/{id}` | Soft delete patient |
| GET | `/patients/{id}/timeline` | Full medical timeline |
| GET | `/patients/{id}/visits` | Patient's visits |
| GET | `/patients/{id}/prescriptions` | Active prescriptions |

---

## 📅 Appointments

| Method | Endpoint | Description |
|---|---|---|
| GET | `/appointments` | List appointments |
| POST | `/appointments` | Create appointment |
| GET | `/appointments/{id}` | Get appointment |
| PUT | `/appointments/{id}` | Update appointment |
| POST | `/appointments/{id}/cancel` | Cancel appointment |
| POST | `/appointments/{id}/complete` | Mark as completed |

---

## 🩺 Visits / EMR

| Method | Endpoint | Description |
|---|---|---|
| GET | `/visits` | List visits |
| POST | `/visits` | Start new visit |
| GET | `/visits/{id}` | Get visit with full EMR |
| PUT | `/visits/{id}` | Update visit (SOAP notes, diagnosis) |
| POST | `/visits/{id}/vitals` | Record vital signs |
| POST | `/visits/{id}/complete` | Complete visit |

---

## 🤖 AI Assistant

| Method | Endpoint | Description |
|---|---|---|
| GET | `/ai/info` | AI features & disclaimer 🔓 |
| GET | `/ai/conversations` | List user's conversations |
| GET | `/ai/conversations/{id}` | Get conversation with history |
| POST | `/ai/chat` | Send message to Dr. AI |
| DELETE | `/ai/conversations/{id}` | Delete conversation |
| POST | `/ai/analyze/lab` | Analyze lab results |
| POST | `/ai/analyze/patient-summary` | Generate patient summary |
| POST | `/ai/analyze/image` | Analyze medical image |

### Chat Request Example
```json
POST /api/v1/ai/chat
{
  "conversationId": null,
  "message": "What are the common causes of elevated creatinine?",
  "patientContextId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```

### Chat Response Example
```json
{
  "success": true,
  "data": {
    "id": "conv-uuid",
    "title": "What are the common causes of elevated...",
    "messages": [
      { "role": "user", "content": "What are the common causes of elevated creatinine?" },
      { "role": "assistant", "content": "Elevated creatinine may indicate...\n\n⚠️ AI-generated content..." }
    ]
  }
}
```

---

## 🎨 Medical Canvas

| Method | Endpoint | Description |
|---|---|---|
| GET | `/canvas/images/{imageId}/annotations` | Get image annotations |
| POST | `/canvas/images/{imageId}/annotations` | Add annotation |
| DELETE | `/canvas/annotations/{id}` | Delete annotation |
| POST | `/canvas/save` | Save full canvas state |
| GET | `/canvas/body-map/{visitId}` | Get body map pins |
| POST | `/canvas/body-map` | Add body map pin |
| DELETE | `/canvas/body-map/annotations/{id}` | Remove body map pin |
| GET | `/canvas/dental/{patientId}` | Get dental chart |
| PUT | `/canvas/dental` | Upsert tooth record |
| DELETE | `/canvas/dental/{id}` | Delete tooth record |

---

## 💰 Billing

| Method | Endpoint | Description |
|---|---|---|
| GET | `/invoices` | List invoices |
| POST | `/invoices` | Create invoice |
| GET | `/invoices/{id}` | Get invoice |
| POST | `/invoices/{id}/send` | Send invoice to patient |
| GET | `/payments` | List payments |
| POST | `/payments` | Record payment |
| GET | `/billing-reports/summary` | Revenue summary |
| GET | `/billing-reports/outstanding` | Outstanding invoices |

---

## Standard Response Format

### Success
```json
{ "success": true, "data": { ... } }
```

### Paginated
```json
{
  "success": true,
  "data": {
    "items": [...],
    "page": 1,
    "pageSize": 20,
    "totalCount": 150,
    "totalPages": 8
  }
}
```

### Error
```json
{ "success": false, "error": "Patient not found.", "code": "NOT_FOUND" }
```
