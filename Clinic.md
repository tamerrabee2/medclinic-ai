طبعاً. الأفضل هنا أن نُبقي الواجهة حديثة، لكن نجعل **الـ Backend بالكامل C# / .NET 10** مع PostgreSQL وEntity Framework Core، ونصمم طبقة الذكاء الاصطناعي بحيث يمكن تغيير مزود الـAI مستقبلاً.

هذه نسخة كاملة ومحسّنة من البرومبت، جاهزة للنسخ إلى AI Coding Agent لديه صلاحية GitHub:

# 🏥 MedClinic AI

## منصة متكاملة لإدارة العيادات الطبية والذكاء الاصطناعي

أنت Senior Software Architect + Senior C#/.NET Backend Engineer + Full-Stack Engineer + AI Engineer + DevOps Engineer + UI/UX Designer.

مهمتك هي **إنشاء وتنفيذ تطبيق طبي احترافي Production-Ready بالكامل، وإنشاء Repository على GitHub، وكتابة الكود وتشغيل الاختبارات وإصلاح الأخطاء ورفع المشروع إلى GitHub**.

لا أريد مجرد Prototype أو Wireframe أو شرح نظري.

أريد مشروعاً حقيقياً قابلاً للتشغيل والتطوير والنشر.

---

# 1. اسم المشروع

اسم المشروع:

**MedClinic AI**

وصف المشروع:

> AI-powered Medical Clinic Management Platform

منصة SaaS حديثة لإدارة:

* العيادات.
* الأطباء.
* الموظفين.
* المرضى.
* المواعيد.
* الملفات الطبية.
* الزيارات.
* الوصفات.
* التحاليل.
* الأشعة.
* الفواتير.
* التقارير.
* الذكاء الاصطناعي.
* الرسم الطبي.
* الخرائط التفاعلية للجسم.
* الصور الطبية.
* مساعد الطبيب الذكي.

---

# 2. التقنية الأساسية المطلوبة

## Backend — إلزامي

استخدم:

**C# + .NET 10 + ASP.NET Core Web API**

ولا تستخدم Node.js كـ Backend رئيسي.

الـ Backend بالكامل يجب أن يكون مبنياً باستخدام:

* C#
* .NET 10
* ASP.NET Core Web API
* Entity Framework Core
* PostgreSQL
* ASP.NET Core Identity أو نظام Authentication آمن مبني على .NET
* JWT / Refresh Tokens أو نظام Authentication مناسب
* FluentValidation أو Validation architecture مناسبة
* Serilog
* Swagger / OpenAPI
* Health Checks

---

# 3. Backend Architecture

استخدم Clean Architecture / Modular Architecture.

اقترح الهيكل التالي:

```text
src/
├── MedClinic.API/
│   ├── Controllers/
│   ├── Middleware/
│   ├── Filters/
│   ├── Extensions/
│   ├── Program.cs
│   └── appsettings.json
│
├── MedClinic.Application/
│   ├── Common/
│   ├── DTOs/
│   ├── Features/
│   │   ├── Auth/
│   │   ├── Clinics/
│   │   ├── Doctors/
│   │   ├── Patients/
│   │   ├── Appointments/
│   │   ├── MedicalRecords/
│   │   ├── Prescriptions/
│   │   ├── Laboratory/
│   │   ├── Radiology/
│   │   ├── AI/
│   │   ├── Billing/
│   │   └── Notifications/
│   ├── Interfaces/
│   └── Services/
│
├── MedClinic.Domain/
│   ├── Entities/
│   ├── ValueObjects/
│   ├── Enums/
│   ├── Events/
│   └── Exceptions/
│
├── MedClinic.Infrastructure/
│   ├── Persistence/
│   ├── Identity/
│   ├── AI/
│   ├── Storage/
│   ├── Notifications/
│   ├── BackgroundJobs/
│   └── ExternalServices/
│
└── MedClinic.Shared/
```

استخدم Dependency Injection بشكل صحيح.

لا تجعل Controllers تحتوي على Business Logic.

---

# 4. Frontend

يمكن استخدام:

* Next.js
* React
* TypeScript
* Tailwind CSS
* shadcn/ui
* TanStack Query
* React Hook Form
* Zod
* Lucide Icons

الـ Frontend يتواصل مع:

```text
Next.js
    ↓
ASP.NET Core Web API
    ↓
Application Layer
    ↓
Domain
    ↓
Infrastructure
    ↓
PostgreSQL / Storage / AI
```

---

# 5. API Design

أنشئ RESTful API احترافي.

مثلاً:

```text
/api/v1/auth
/api/v1/clinics
/api/v1/users
/api/v1/doctors
/api/v1/patients
/api/v1/appointments
/api/v1/medical-records
/api/v1/visits
/api/v1/prescriptions
/api/v1/laboratory
/api/v1/radiology
/api/v1/ai
/api/v1/billing
/api/v1/notifications
```

استخدم API Versioning.

جميع الـ APIs يجب أن تحتوي على:

* Validation.
* Authentication.
* Authorization.
* Error handling.
* Logging.
* Pagination عند الحاجة.
* Filtering.
* Sorting.
* Search.

---

# 6. Standard API Response

استخدم Response structure موحداً.

مثلاً:

```csharp
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public List<string>? Errors { get; set; }
}
```

ويجب أن يكون هناك Global Exception Handling Middleware.

لا ترجع Stack Trace للمستخدم.

---

# 7. Authentication

أنشئ نظام Authentication آمن.

يدعم:

* Login.
* Register.
* Logout.
* Refresh Token.
* Forgot Password.
* Reset Password.
* Email Verification.
* Session management.

استخدم:

* Password hashing.
* Secure tokens.
* Token expiration.
* Refresh token rotation إذا كان مناسباً.

لا تضع secrets في source code.

---

# 8. Roles & Permissions

أنشئ RBAC.

Roles:

```text
SuperAdmin
ClinicAdmin
Doctor
Nurse
Receptionist
LabTechnician
Radiologist
Accountant
Patient
```

مع Permission System.

مثلاً:

```text
Patients.Read
Patients.Create
Patients.Update
Patients.Delete

MedicalRecords.Read
MedicalRecords.Create
MedicalRecords.Update

AI.Analysis
AI.Reports

Billing.Read
Billing.Create
Billing.Update
```

لا تعتمد على Role فقط إذا كانت Permission system أكثر دقة.

---

# 9. Multi-Tenant Architecture

التطبيق SaaS.

يجب أن تكون كل عيادة Tenant مستقلاً.

مثلاً:

```text
Clinic
 ├── Doctors
 ├── Staff
 ├── Patients
 ├── Appointments
 ├── Medical Records
 ├── Billing
 └── AI Reports
```

يجب منع Clinic A من الوصول إلى بيانات Clinic B.

طبّق Tenant Isolation على مستوى:

* API.
* Application Layer.
* Database queries.

لا تعتمد فقط على إخفاء البيانات في Frontend.

---

# 10. Database

استخدم:

**PostgreSQL + Entity Framework Core**

الجداول الأساسية:

```text
Users
Roles
Permissions
Clinics
ClinicMembers
Doctors
Patients
PatientContacts
MedicalRecords
Visits
Diagnoses
Medications
Prescriptions
PrescriptionItems
Appointments
LabOrders
LabResults
LabResultItems
RadiologyStudies
MedicalImages
AIAnalyses
AIReports
MedicalAnnotations
BodyAnnotations
Invoices
InvoiceItems
Payments
Notifications
AuditLogs
AIConversations
AIConversationMessages
```

استخدم:

* Primary Keys.
* Foreign Keys.
* Indexes.
* Unique constraints.
* Soft Delete عند الحاجة.
* CreatedAt.
* UpdatedAt.
* CreatedBy.
* UpdatedBy.

---

# 11. EF Core

أنشئ:

```text
ApplicationDbContext
```

واستخدم:

* Fluent API.
* Entity configurations.
* Migrations.
* Seed data.

لا تضع كل إعدادات Entity داخل DbContext.

استخدم:

```text
Configurations/
```

لكل Entity.

---

# 12. Patient Management

أنشئ نظاماً متكاملاً للمرضى.

Patient Profile:

```text
Personal Information
Contact Information
Emergency Contact
Allergies
Chronic Conditions
Medications
Medical History
Visits
Diagnoses
Prescriptions
Lab Results
Radiology
AI Reports
Appointments
Invoices
Documents
```

اعرض Timeline للتاريخ الطبي.

---

# 13. Medical Records

أنشئ EMR.

الطبيب يستطيع إنشاء Visit.

تحتوي الزيارة على:

```text
Chief Complaint
Symptoms
Vitals
Physical Examination
Diagnosis
Differential Diagnosis
Treatment Plan
Prescription
Lab Orders
Radiology Orders
Follow-up
Doctor Notes
```

احفظ كل زيارة في Patient Timeline.

---

# 14. Appointments

أنشئ Appointment System.

يدعم:

* Calendar.
* Day.
* Week.
* Month.
* Drag & Drop.
* Create.
* Update.
* Cancel.
* Reschedule.
* Doctor availability.
* Patient availability.
* Appointment status.

Statuses:

```text
Scheduled
Confirmed
CheckedIn
InProgress
Completed
Cancelled
NoShow
```

---

# 15. Doctor Dashboard

Dashboard خاص بالطبيب.

يعرض:

* Today's appointments.
* Upcoming patients.
* Recent patients.
* Pending lab results.
* New radiology reports.
* AI analyses.
* Follow-ups.
* Alerts.

---

# 16. AI Architecture

هذه نقطة أساسية.

لا تجعل Business Logic تعتمد مباشرة على OpenAI أو Gemini أو أي مزود.

أنشئ abstraction:

```csharp
public interface IAIProvider
{
    Task<MedicalImageAnalysisResult> AnalyzeMedicalImageAsync(
        MedicalImageInput input,
        CancellationToken cancellationToken);

    Task<LabAnalysisResult> AnalyzeLabResultsAsync(
        LabAnalysisInput input,
        CancellationToken cancellationToken);

    Task<PatientSummaryResult> SummarizePatientAsync(
        PatientSummaryInput input,
        CancellationToken cancellationToken);

    Task<MedicalReportResult> GenerateMedicalReportAsync(
        MedicalReportInput input,
        CancellationToken cancellationToken);

    Task<AIChatResponse> ChatAsync(
        AIChatRequest request,
        CancellationToken cancellationToken);
}
```

ثم:

```text
IAIProvider
   │
   ├── OpenAIProvider
   ├── GeminiProvider
   ├── AnthropicProvider
   └── LocalAIProvider
```

بحيث يمكن تغيير AI provider دون تغيير باقي التطبيق.

---

# 17. AI Gateway

أنشئ:

```text
AI Gateway
```

مسؤول عن:

* Authentication.
* Authorization.
* Provider selection.
* Prompt management.
* Request validation.
* Data minimization.
* Rate limiting.
* Logging بدون بيانات حساسة.
* Error handling.
* Retry.
* Timeout.
* Cost tracking إن أمكن.

---

# 18. Medical Image Analysis

أضف ميزة:

**AI Medical Imaging Analysis**

يمكن رفع:

* X-Ray.
* CT.
* MRI.
* Ultrasound.
* Medical images.
* DICOM عند توفر دعم مناسب.

Workflow:

```text
Upload Image
      ↓
Validate
      ↓
Secure Storage
      ↓
Create Analysis Job
      ↓
AI Processing
      ↓
Extract Findings
      ↓
Generate Report
      ↓
Doctor Review
      ↓
Approve / Edit
      ↓
Save to Medical Record
```

لا تجعل HTTP request ينتظر عملية AI طويلة.

استخدم Background Jobs.

مثلاً:

```text
Hangfire
```

أو بديل مناسب.

---

# 19. AI Imaging Report

النتيجة يجب أن تكون Structured.

مثلاً:

```json
{
  "summary": "...",
  "findings": [],
  "observations": [],
  "regionsOfInterest": [],
  "confidence": null,
  "recommendationsForReview": [],
  "requiresDoctorReview": true
}
```

لا تسمح للـ AI باعتماد التشخيص النهائي تلقائياً.

---

# 20. Medical AI Safety

هذه قاعدة إلزامية.

يجب أن يظهر في نتائج AI:

> AI-generated analysis. This content is intended for clinical decision support only and must be reviewed by a qualified healthcare professional.

AI لا يجوز له:

* اعتماد diagnosis نهائي.
* تعديل Medical Record النهائي تلقائياً.
* إصدار Prescription تلقائياً.
* إعطاء قرار طبي ملزم.

يجب أن يكون هناك:

**Doctor Review**

ثم:

**Approve**

أو:

**Edit**

---

# 21. AI Lab Analyzer

اسم الميزة:

**AI Lab Analyzer**

يمكن رفع:

* PDF.
* JPG.
* PNG.
* Lab report.

الـ system يقوم بـ:

1. OCR عند الحاجة.
2. استخراج القيم.
3. Normalization.
4. Reference range.
5. تحديد abnormal values.
6. مقارنة النتائج السابقة.
7. Trend analysis.
8. إنشاء Summary.

مثلاً:

```text
Hemoglobin
Current: 11.2
Previous: 12.1
Reference: 13-17
Status: Below Reference Range
Trend: Decreasing
```

---

# 22. AI Patient Summary

أضف زر:

**Generate AI Patient Summary**

يقوم بتلخيص:

* Medical history.
* Previous visits.
* Diagnoses.
* Medications.
* Lab trends.
* Radiology.
* Important notes.

النتيجة يجب أن تكون قابلة للمراجعة.

---

# 23. AI Medical Assistant

أنشئ:

**Dr. AI Assistant**

يمكن للطبيب السؤال:

```text
Summarize this patient's history.

Analyze the latest lab results.

Compare current and previous results.

Summarize the latest radiology report.

Draft a clinical visit summary.

List important abnormalities.

Prepare questions that the doctor may consider during the visit.
```

لا تسمح له بإعطاء نفسه صلاحيات تعديل السجلات.

---

# 24. AI Chat

واجهة Chat حديثة.

Features:

* Streaming.
* Markdown.
* Attachments.
* Image support.
* Patient context.
* Conversation history.
* New conversation.
* Delete conversation.
* Export conversation.

---

# 25. Medical Canvas

أنشئ:

**Interactive Medical Canvas**

يمكن للطبيب فتح صورة طبية والرسم عليها.

Tools:

```text
Pen
Brush
Arrow
Line
Rectangle
Circle
Text
Measurement
Eraser
Undo
Redo
Zoom
Pan
```

يجب أن تكون جميع العمليات قابلة للحفظ.

احفظ:

```text
Original Image
+
Annotation Data
+
Annotated Preview
```

لا تعدل الصورة الأصلية.

---

# 26. Annotation Model

أنشئ Entity:

```csharp
MedicalAnnotation
```

وتحتوي مثلاً:

```text
Id
MedicalImageId
DoctorId
Type
Coordinates
Color
Thickness
Text
CreatedAt
UpdatedAt
```

ويجب أن يكون نظام Annotation قابلاً للتوسع.

---

# 27. Medical Image Viewer

أنشئ Viewer احترافي:

* Zoom.
* Pan.
* Rotate.
* Brightness.
* Contrast.
* Grayscale.
* Invert.
* Fullscreen.
* Measurements.
* Annotation.

إذا تم دعم DICOM، استخدم مكتبة متخصصة وموثوقة بدلاً من بناء DICOM parser من الصفر.

---

# 28. Interactive Body Map

أنشئ:

**Interactive Human Body Map**

استخدم SVG تفاعلي.

المناطق:

```text
Head
Neck
Chest
Abdomen
Back
Left Arm
Right Arm
Left Leg
Right Leg
```

عند الضغط على جزء من الجسم يستطيع الطبيب:

* Add symptom.
* Add pain level.
* Add note.
* Add diagnosis.
* Add annotation.

---

# 29. Dental Module

أنشئ أساساً لـ:

**Dental Management**

Interactive Teeth Chart.

كل سن يمكن تحديده وإضافة:

```text
Cavity
Filling
Crown
Missing
Implant
Root Canal
Extraction
Notes
```

---

# 30. Laboratory Module

يدعم:

* Lab orders.
* Lab samples.
* Lab results.
* Reference ranges.
* Abnormal flags.
* Result history.
* Charts.

---

# 31. Radiology Module

يدعم:

* Radiology orders.
* Studies.
* Images.
* Reports.
* AI analysis.
* Doctor review.
* Annotations.

---

# 32. Prescription System

أنشئ Prescription Builder.

يحتوي على:

```text
Medication
Dose
Frequency
Duration
Route
Instructions
Notes
```

مع:

* PDF.
* Print.
* Prescription history.

---

# 33. Billing

أنشئ Billing Module.

يدعم:

```text
Invoices
Invoice Items
Services
Discounts
Taxes
Payments
Outstanding Balance
Refunds
```

Dashboard مالي.

---

# 34. Notifications

أنشئ:

Notification Center.

أنواع الإشعارات:

```text
Appointment Reminder
New Lab Result
New Radiology Result
AI Analysis Complete
Follow-up Reminder
Invoice Created
Payment Received
```

---

# 35. Audit Logging

بما أن النظام طبي، أنشئ:

**Audit Log System**

يسجل:

```text
User Login
Patient Viewed
Patient Created
Medical Record Created
Medical Record Updated
Prescription Created
Image Uploaded
AI Analysis Requested
AI Report Reviewed
AI Report Approved
Invoice Created
Payment Created
```

لا تسجل المعلومات الطبية الحساسة داخل log إذا لم تكن ضرورية.

---

# 36. Security

طبّق:

* HTTPS.
* Secure authentication.
* Password hashing.
* JWT security.
* Refresh tokens.
* RBAC.
* Tenant isolation.
* Rate limiting.
* Input validation.
* File validation.
* Malware scanning architecture عند الحاجة.
* Secure headers.
* CORS policy.
* SQL injection protection.
* XSS protection.
* CSRF protection حسب architecture.
* Encryption in transit.
* Encryption at rest عندما تكون البنية التحتية داعمة لذلك.
* Secure secret management.

---

# 37. File Upload Security

لا تسمح برفع أي ملف عشوائياً.

تحقق من:

* Extension.
* MIME type.
* File signature.
* File size.
* Storage path.
* Authorization.

لا تجعل ملفات المرضى متاحة عبر public URLs.

استخدم signed URLs أو protected download endpoints.

---

# 38. Storage

أنشئ abstraction:

```csharp
public interface IFileStorage
{
    Task<string> UploadAsync(...);
    Task<Stream> DownloadAsync(...);
    Task DeleteAsync(...);
}
```

ثم يمكن استخدام:

```text
LocalStorage
S3Storage
AzureBlobStorage
SupabaseStorage
```

بدون تغيير Business Logic.

---

# 39. Background Jobs

استخدم Background Jobs للعمليات الطويلة:

* AI analysis.
* OCR.
* PDF generation.
* Email.
* Notifications.
* Image processing.

مثلاً:

```text
Hangfire
```

مع PostgreSQL إذا كان مناسباً.

---

# 40. Caching

استخدم Redis عندما يكون مناسباً.

مثلاً:

* Sessions.
* Cache.
* Rate limiting.
* Background jobs إذا تطلب architecture ذلك.

---

# 41. Observability

أضف:

* Structured logging.
* Serilog.
* Health checks.
* Metrics architecture.
* Correlation ID.
* Request tracing.

لا تظهر البيانات الطبية في logs.

---

# 42. Frontend Design

أريد UI Premium.

ليس Dashboard تقليدي.

Style:

```text
Modern
Premium
Medical
Minimal
Professional
Fast
Accessible
```

يدعم:

* Light Mode.
* Dark Mode.
* Arabic.
* English.
* RTL.
* LTR.
* Responsive.

---

# 43. Landing Page

أنشئ Landing Page:

Hero:

> Smart Healthcare Management Powered by AI

Sections:

* Features.
* AI capabilities.
* Medical imaging.
* EMR.
* Lab analysis.
* Interactive canvas.
* Security.
* Pricing.
* FAQ.
* Contact.

---

# 44. Global Search

أضف:

`Ctrl + K`

والبحث في:

```text
Patients
Doctors
Appointments
Medical Records
Lab Results
Radiology
Prescriptions
Invoices
```

---

# 45. UX

استخدم:

* Skeleton loading.
* Empty states.
* Error states.
* Toast.
* Confirmation dialogs.
* Progress indicators.
* Retry.
* Optimistic updates عند الحاجة.

---

# 46. AI Processing UI

عند تحليل صورة:

```text
Uploading
    ↓
Validating
    ↓
Processing
    ↓
AI Analysis
    ↓
Generating Report
    ↓
Doctor Review
    ↓
Completed
```

اعرض حالة العملية بشكل تفاعلي.

---

# 47. API Documentation

استخدم:

**Swagger / OpenAPI**

يجب أن تكون جميع endpoints موثقة.

أضف:

* Request models.
* Response models.
* Authentication.
* Error responses.

---

# 48. Testing

أنشئ Tests باستخدام .NET testing ecosystem.

استخدم:

* xUnit.
* FluentAssertions.
* Moq أو NSubstitute.
* Integration testing.
* WebApplicationFactory.
* Testcontainers عند الحاجة.

اختبر:

### Unit Tests

* Authentication.
* Authorization.
* Tenant isolation.
* Patients.
* Appointments.
* Medical Records.
* Prescriptions.
* AI services.
* Billing.

### Integration Tests

* PostgreSQL.
* API.
* Authentication.
* Database.

### E2E

اختبر:

```text
Login
↓
Create Clinic
↓
Create Doctor
↓
Create Patient
↓
Create Appointment
↓
Create Visit
↓
Upload Lab Result
↓
Run AI Analysis
↓
Upload Medical Image
↓
Annotate Image
↓
Review AI Report
↓
Create Prescription
↓
Create Invoice
```

---

# 49. Demo Data

أنشئ Seed Data وهمية.

مثلاً:

```text
Demo Clinic
Demo Doctors
Demo Staff
Demo Patients
Demo Appointments
Demo Lab Results
Demo Radiology
Demo Prescriptions
Demo Invoices
```

ممنوع استخدام بيانات مرضى حقيقية.

---

# 50. Docker

أنشئ:

```text
Dockerfile
docker-compose.yml
```

لتشغيل:

```text
Frontend
Backend
PostgreSQL
Redis
```

بحيث يستطيع المطور تشغيل المشروع محلياً بسهولة.

---

# 51. GitHub Actions

أنشئ:

```text
.github/
└── workflows/
    ├── ci.yml
    ├── backend-tests.yml
    ├── frontend-tests.yml
    └── build.yml
```

عند:

* Push.
* Pull Request.

نفذ:

```text
Restore
Build
Lint
Type Check
Unit Tests
Integration Tests
Frontend Tests
Production Build
```

---

# 52. GitHub Repository

أنشئ Repository:

```text
medclinic-ai
```

نظمه باحتراف.

أنشئ:

```text
README.md
ARCHITECTURE.md
SECURITY.md
CONTRIBUTING.md
LICENSE
.env.example
.gitignore
docker-compose.yml
Dockerfile
```

لا ترفع:

```text
.env
API Keys
Passwords
Certificates
Private Keys
Patient Data
```

---

# 53. Git Commits

استخدم commits منظمة:

```text
chore: initialize repository

feat: implement clean architecture

feat: add authentication and authorization

feat: add multi-tenant clinic management

feat: add patient management

feat: add medical records

feat: add appointments

feat: add prescriptions

feat: add laboratory module

feat: add radiology module

feat: add AI gateway

feat: add AI medical image analysis

feat: add AI lab analyzer

feat: add medical canvas

feat: add interactive body map

feat: add billing

feat: add notifications

test: add backend integration tests

test: add frontend e2e tests

docs: add architecture documentation

ci: add GitHub Actions
```

---

# 54. Environment Variables

أنشئ:

```text
.env.example
```

مثلاً:

```text
DATABASE_CONNECTION_STRING=

JWT_SECRET=

REDIS_CONNECTION_STRING=

STORAGE_PROVIDER=

STORAGE_CONNECTION_STRING=

AI_PROVIDER=

OPENAI_API_KEY=

GEMINI_API_KEY=

ANTHROPIC_API_KEY=

APPLICATION_URL=
```

لا تضع أي قيمة سرية حقيقية.

---

# 55. AI Free / Local Mode

أريد أن يكون التطبيق قابلاً للتشغيل بدون اشتراك AI مدفوع أثناء التطوير.

أنشئ:

```text
MockAIProvider
```

للاختبارات وDevelopment.

وإذا كان بالإمكان دعم Local AI:

```text
LocalAIProvider
```

بحيث يمكن تشغيل موديل محلي.

لكن:

**لا تدّعِ أن تحليل الأشعة الطبي الحقيقي مجاني إذا كان يعتمد على API مدفوع.**

وضّح في README الفرق بين:

```text
Demo AI
Local AI
Cloud AI
```

---

# 56. Medical AI Disclaimer

في جميع أجزاء النظام التي تستخدم AI أظهر بوضوح أن:

> AI-generated content is for clinical decision support only and must be reviewed by a qualified healthcare professional.

ولا تجعل التطبيق يدّعي أن AI قادر على استبدال الطبيب.

---

# 57. Performance

اهتم بـ:

* Async/await.
* CancellationToken.
* Pagination.
* Database indexes.
* EF Core query optimization.
* No N+1 queries.
* Caching.
* Background jobs.
* Streaming.
* Image optimization.

---

# 58. C# Coding Standards

التزم بـ:

* Nullable Reference Types.
* Async methods.
* CancellationToken.
* Dependency Injection.
* SOLID.
* Clean Architecture.
* Records عند الحاجة.
* DTOs.
* Interfaces.
* Explicit validation.
* XML documentation للـ public APIs عند الحاجة.

تجنب:

```text
any
dynamic
God Classes
God Controllers
Static Service Locator
Hardcoded configuration
Hardcoded secrets
```

---

# 59. Database Migration

استخدم EF Core migrations.

مثلاً:

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

وثّق الطريقة في README.

---

# 60. Health Checks

أضف:

```text
/health
/health/ready
/health/live
```

تحقق من:

* API.
* Database.
* Redis.
* Storage.
* AI Provider عند الحاجة.

---

# 61. Error Handling

أنشئ Error Response موحداً.

مثلاً:

```json
{
  "success": false,
  "message": "An unexpected error occurred.",
  "errors": [],
  "traceId": "..."
}
```

لا تظهر:

* Stack traces.
* Database errors.
* API secrets.
* Internal paths.

---

# 62. Localization

أضف:

```text
Arabic
English
```

ويجب أن يدعم التطبيق:

```text
RTL
LTR
```

بشكل حقيقي، وليس مجرد ترجمة النصوص.

---

# 63. Doctor Drawing Experience

اجعل تجربة الرسم ممتازة.

الطبيب يستطيع:

1. فتح الصورة.
2. Zoom.
3. Pan.
4. رسم.
5. إضافة Arrow.
6. تحديد Region.
7. إضافة Text.
8. Measurement.
9. Undo.
10. Redo.
11. حفظ.
12. مقارنة Original vs Annotated.

---

# 64. AI + Drawing Integration

اجعل AI والـ Canvas يعملان معاً.

مثلاً:

```text
Medical Image
       ↓
AI Analysis
       ↓
Detected Region
       ↓
Display Region on Canvas
       ↓
Doctor Annotation
       ↓
Doctor Review
       ↓
Save Final Report
```

إذا كان AI provider لا يدعم تحديد المناطق، لا تخترع bounding boxes غير موجودة.

في هذه الحالة اعرض النتائج كنص منظم فقط.

---

# 65. Patient Timeline

أنشئ Timeline:

```text
2026-09-03
│
├── Doctor Visit
├── Lab Test
├── AI Lab Analysis
├── X-Ray
├── AI Radiology Analysis
├── Prescription
└── Follow-up
```

يجب أن تكون Interactive.

---

# 66. Dashboard Analytics

أضف Charts:

* Patients over time.
* Appointments.
* Revenue.
* Lab results.
* AI analyses.
* Follow-up statistics.

---

# 67. Architecture Documentation

اكتب `ARCHITECTURE.md` واشرح:

```text
Frontend
   ↓
ASP.NET Core API
   ↓
Application
   ↓
Domain
   ↓
Infrastructure
   ↓
PostgreSQL

External:
AI
Storage
Email
Notifications
Redis
```

---

# 68. Final Acceptance Criteria

لا تعتبر المشروع مكتملاً إلا إذا تحقق:

```text
[ ] GitHub Repository Created
[ ] Clean Architecture
[ ] ASP.NET Core Backend
[ ] C# Backend
[ ] .NET 10
[ ] PostgreSQL
[ ] EF Core
[ ] Authentication
[ ] RBAC
[ ] Multi-tenancy
[ ] Patient Management
[ ] Doctor Management
[ ] Appointment Management
[ ] EMR
[ ] Prescriptions
[ ] Laboratory
[ ] Radiology
[ ] Medical Image Viewer
[ ] AI Gateway
[ ] AI Provider Abstraction
[ ] Mock AI Provider
[ ] AI Lab Analysis
[ ] AI Medical Image Analysis integration
[ ] AI Medical Assistant
[ ] AI Chat
[ ] Medical Canvas
[ ] Body Map
[ ] Dental Module Foundation
[ ] Billing
[ ] Notifications
[ ] Audit Logs
[ ] Arabic
[ ] English
[ ] RTL
[ ] LTR
[ ] Dark Mode
[ ] Responsive UI
[ ] Swagger
[ ] Health Checks
[ ] Docker
[ ] GitHub Actions
[ ] Unit Tests
[ ] Integration Tests
[ ] E2E Tests
[ ] README
[ ] Architecture Documentation
[ ] Security Documentation
[ ] No Secrets in GitHub
[ ] Production Build Successful
```

---

# 69. طريقة التنفيذ الإلزامية

ابدأ أولاً بفحص بيئة العمل الحالية وGitHub access.

بعد ذلك:

### Phase 1

Repository + Architecture

### Phase 2

.NET Backend + PostgreSQL + EF Core

### Phase 3

Authentication + RBAC + Multi-tenancy

### Phase 4

Patients + Doctors + Appointments

### Phase 5

EMR + Prescriptions

### Phase 6

Laboratory + Radiology

### Phase 7

AI Gateway + AI Providers

### Phase 8

AI Medical Analysis

### Phase 9

Medical Canvas + Body Map

### Phase 10

Billing + Notifications

### Phase 11

Security + Audit Logs

### Phase 12

Testing

### Phase 13

Docker + CI/CD

### Phase 14

Documentation

### Phase 15

Final QA

---

# 70. قاعدة مهمة جداً

لا تكتفِ بإنشاء ملفات فارغة.

لا تضع:

```text
TODO
Coming Soon
Implement later
Fake implementation
```

إلا إذا كانت هناك ميزة خارج نطاق الإمكانيات الحالية، وفي هذه الحالة أنشئ abstraction حقيقية ومتكاملة تسمح بإضافتها لاحقاً.

إذا احتجت API key:

* استخدم environment variable.
* أضف `.env.example`.
* لا تضع المفتاح في GitHub.

إذا احتجت خدمة مدفوعة:

* لا تدّعِ أنها مجانية.
* أضف Mock/Local option عند الإمكان.

إذا حدث خطأ:

1. اقرأ الخطأ.
2. حدد السبب.
3. أصلح الكود.
4. أعد تشغيل الاختبارات.
5. لا تتجاهل الخطأ.

---

# 71. المطلوب منك الآن

**ابدأ بتنفيذ المشروع فعلياً.**

لا أريد شرحاً نظرياً للمشروع.

قم بإنشاء:

```text
GitHub Repository
+
C# / .NET 10 Backend
+
PostgreSQL
+
EF Core
+
Next.js Frontend
+
AI Architecture
+
Medical AI
+
Interactive Medical UI
+
Testing
+
Docker
+
GitHub Actions
+
Documentation
```

ثم قم بعمل commits منظمة وادفع المشروع إلى GitHub.

وفي النهاية أعطني:

1. رابط GitHub Repository.
2. ملخص Architecture.
3. كيفية تشغيل المشروع محلياً.
4. Environment Variables المطلوبة.
5. كيفية تشغيل Database.
6. كيفية تشغيل Backend.
7. كيفية تشغيل Frontend.
8. كيفية تشغيل AI.
9. نتائج الاختبارات.
10. أي نقاط تحتاج API Keys أو خدمات خارجية.
11. أي قيود معروفة.
12. الخطوات التالية المقترحة.

**الأولوية القصوى:**

الأمان + جودة الكود + قابلية التوسع + تجربة الطبيب + حماية البيانات الطبية + فصل AI عن Business Logic.

ابدأ التنفيذ الآن.

**ملاحظة مهمة:** اخترت في البرومبت **ASP.NET Core Web API + .NET 10 + EF Core + PostgreSQL** للـBackend، مع إبقاء Next.js/React للواجهة. بهذه الطريقة تحصل على Backend قوي جداً وقابل للتوسع، وفي الوقت نفسه تحصل على واجهات تفاعلية ممتازة للـDashboard والـMedical Canvas والـBody Map.

والأهم أنني جعلت الـAI **طبقة مستقلة `IAIProvider`**؛ وهذا مهم جداً لأنك لاحقاً تستطيع تغيير OpenAI/Gemini/موديل محلي بدون إعادة بناء النظام كله.
