export type Language = 'en' | 'ar';

export interface TranslationDictionary {
  // Navigation & Shell
  dashboard: string;
  patients: string;
  appointments: string;
  visits: string;
  clinicalPractice: string;
  aiAndDiagnostics: string;
  operationsAndManagement: string;
  aiAssistant: string;
  aiLabAnalyzer: string;
  labOrdersDesk: string;
  radiologyPACS: string;
  prescriptions: string;
  canvas: string;
  dentalChart: string;
  billing: string;
  analytics: string;
  clinicStaff: string;
  userAccounts: string;
  auditLogs: string;
  notifications: string;
  externalLabs: string;
  insurance: string;
  reports: string;
  voiceScribe: string;
  settings: string;
  logout: string;
  searchPlaceholder: string;
  commandPalette: string;
  apiOnline: string;
  aiEngineActive: string;
  doctorWorkspace: string;

  // Common Actions & Badges
  save: string;
  cancel: string;
  delete: string;
  edit: string;
  view: string;
  create: string;
  addNew: string;
  filter: string;
  export: string;
  refresh: string;
  status: string;
  actions: string;
  close: string;
  approved: string;
  pending: string;
  critical: string;
  normal: string;
  warning: string;

  // Dashboard Overview
  goodMorning: string;
  todaysOverview: string;
  todaysSchedule: string;
  aiPatientBrief: string;
  aiTasks: string;
  totalPatients: string;
  completedVisits: string;
  aiReviewsNeeded: string;
  labResultsPending: string;
  imagingStudiesWaiting: string;
  voiceNotesToReview: string;

  // Clinical & EMR
  vitals: string;
  bloodPressure: string;
  heartRate: string;
  spo2: string;
  temperature: string;
  respiratoryRate: string;
  soapNotes: string;
  subjective: string;
  objective: string;
  assessment: string;
  plan: string;
  chiefComplaint: string;
  diagnoses: string;
  medications: string;
  allergies: string;
  contraindications: string;
  physicianSignOff: string;
}

export const translations: Record<Language, TranslationDictionary> = {
  en: {
    // Navigation & Shell
    dashboard: 'Dashboard',
    patients: 'Patients',
    appointments: 'Appointments',
    visits: 'Clinical Encounters',
    clinicalPractice: 'Clinical Practice',
    aiAndDiagnostics: 'AI & Diagnostics',
    operationsAndManagement: 'Operations & Management',
    aiAssistant: 'Clinical AI Copilot',
    aiLabAnalyzer: 'AI Lab Analyzer',
    labOrdersDesk: 'Lab Orders Desk',
    radiologyPACS: 'Radiology & PACS',
    prescriptions: 'E-Prescriptions',
    canvas: 'Medical Canvas',
    dentalChart: 'FDI Dental Chart',
    billing: 'Billing & Claims',
    analytics: 'Executive BI',
    clinicStaff: 'Clinic Staff & Roles',
    userAccounts: 'User Accounts (IAM)',
    auditLogs: 'Audit Trail (HIPAA)',
    notifications: 'Notifications',
    externalLabs: 'External Labs',
    insurance: 'Insurance & Claims',
    reports: 'Clinic Reports',
    voiceScribe: 'Voice AI Scribe',
    settings: 'Settings',
    logout: 'Sign Out',
    searchPlaceholder: 'Search patients, files, diagnoses (Ctrl + K)...',
    commandPalette: 'Command Palette',
    apiOnline: '.NET 10 API Online',
    aiEngineActive: 'AI Engine Active',
    doctorWorkspace: 'AI Clinical Workspace',

    // Common Actions & Badges
    save: 'Save Changes',
    cancel: 'Cancel',
    delete: 'Delete',
    edit: 'Edit',
    view: 'View',
    create: 'Create',
    addNew: 'Add New',
    filter: 'Filter',
    export: 'Export',
    refresh: 'Refresh',
    status: 'Status',
    actions: 'Actions',
    close: 'Close',
    approved: 'Approved',
    pending: 'Pending',
    critical: 'Critical Alert',
    normal: 'Normal',
    warning: 'Warning',

    // Dashboard Overview
    goodMorning: 'Good Morning',
    todaysOverview: "Today's Clinical Overview",
    todaysSchedule: "Today's Schedule",
    aiPatientBrief: 'AI Patient Brief',
    aiTasks: 'AI Assisted Tasks',
    totalPatients: 'Total Patients',
    completedVisits: 'Completed Visits',
    aiReviewsNeeded: 'AI Reviews Needed',
    labResultsPending: 'Pending Lab Results',
    imagingStudiesWaiting: 'Imaging Studies Waiting',
    voiceNotesToReview: 'Voice Notes To Review',

    // Clinical & EMR
    vitals: 'Vitals Strip',
    bloodPressure: 'Blood Pressure',
    heartRate: 'Heart Rate',
    spo2: 'SpO2 Oxygen',
    temperature: 'Body Temp',
    respiratoryRate: 'Respiratory Rate',
    soapNotes: 'SOAP Notes Editor',
    subjective: 'Subjective (History)',
    objective: 'Objective (Vitals & Exam)',
    assessment: 'Assessment (ICD-10)',
    plan: 'Plan & Therapeutics',
    chiefComplaint: 'Chief Complaint',
    diagnoses: 'Diagnoses',
    medications: 'Medications',
    allergies: 'Allergies Alert',
    contraindications: 'Contraindication Guardrail',
    physicianSignOff: 'Physician Official Sign-off'
  },
  ar: {
    // Navigation & Shell
    dashboard: 'لوحة التحكم',
    patients: 'سجل المرضى',
    appointments: 'جدول المواعيد',
    visits: 'الزيارات السريرية',
    clinicalPractice: 'الممارسة السريرية',
    aiAndDiagnostics: 'الذكاء الاصطناعي والتشخيص',
    operationsAndManagement: 'العمليات والإدارة',
    aiAssistant: 'المساعد الطبي الذكي (Copilot)',
    aiLabAnalyzer: 'محلل المختبرات الذكي',
    labOrdersDesk: 'مكتب طلبات المختبر',
    radiologyPACS: 'مستعرض الأشعة (PACS)',
    prescriptions: 'الوصفات الإلكترونية',
    canvas: 'لوحة الرسم الطبي',
    dentalChart: 'مخطط الأسنان (FDI)',
    billing: 'الفواتير والمطالبات',
    analytics: 'التحليلات والمؤشرات',
    clinicStaff: 'طاقم العيادة والصلاحيات',
    userAccounts: 'إدارة حسابات المستخدمين',
    auditLogs: 'سجل التدقيق الأمني (HIPAA)',
    notifications: 'مركز الإشعارات',
    externalLabs: 'المختبرات المرجعية الخارجية',
    insurance: 'التأمين الطبي والمطالبات',
    reports: 'تقارير العيادة',
    voiceScribe: 'الكاتب الصوتي الطبي',
    settings: 'الإعدادات',
    logout: 'تسجيل الخروج',
    searchPlaceholder: 'ابحث عن مريض، ملف، أو تشخيص (Ctrl + K)...',
    commandPalette: 'لوحة الأوامر السريعة',
    apiOnline: 'خادم .NET 10 متصل',
    aiEngineActive: 'محرك الذكاء الاصطناعي نشط',
    doctorWorkspace: 'مساحة العمل السريرية الذكية',

    // Common Actions & Badges
    save: 'حفظ التغييرات',
    cancel: 'إلغاء',
    delete: 'حذف',
    edit: 'تعديل',
    view: 'عرض',
    create: 'إنشاء',
    addNew: 'إضافة جديد',
    filter: 'تصفية',
    export: 'تصدير',
    refresh: 'تحديث',
    status: 'الحالة',
    actions: 'الإجراءات',
    close: 'إغلاق',
    approved: 'معتمد',
    pending: 'قيد الانتظار',
    critical: 'تنبيه حرج',
    normal: 'طبيعي',
    warning: 'تحذير',

    // Dashboard Overview
    goodMorning: 'صباح الخير',
    todaysOverview: 'نظرة عامة على عيادة اليوم',
    todaysSchedule: 'جدول مواعيد اليوم',
    aiPatientBrief: 'الملخص الطبي الذكي للمريض',
    aiTasks: 'مهام الذكاء الاصطناعي',
    totalPatients: 'إجمالي المرضى',
    completedVisits: 'الزيارات المكتملة',
    aiReviewsNeeded: 'مراجعات الذكاء الاصطناعي',
    labResultsPending: 'نتائج مختبر معلقة',
    imagingStudiesWaiting: 'دراسات إشعاعية بانتظار المراجعة',
    voiceNotesToReview: 'ملاحظات صوتية بانتظار الاعتماد',

    // Clinical & EMR
    vitals: 'شريط المؤشرات الحيوية',
    bloodPressure: 'ضغط الدم',
    heartRate: 'النبض',
    spo2: 'تشبع الأكسجين (SpO2)',
    temperature: 'درجة الحرارة',
    respiratoryRate: 'معدل التنفس',
    soapNotes: 'محرر ملاحظات SOAP',
    subjective: 'الشكوى وتاريخ المرض (Subjective)',
    objective: 'الفحص والمؤشرات (Objective)',
    assessment: 'التقييم والتشخيص (Assessment)',
    plan: 'خطة العلاج والدواء (Plan)',
    chiefComplaint: 'الشكوى الرئيسية',
    diagnoses: 'التشخيصات السريرية',
    medications: 'الأدوية الحالية',
    allergies: 'تنبيه الحساسية',
    contraindications: 'حاجز منع التداخلات الدوائية',
    physicianSignOff: 'اعتماد وتوقيع الطبيب الرسمي'
  }
};
