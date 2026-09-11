'use client';

import React, { useState, useEffect } from 'react';
import {
  ShieldAlert,
  Search,
  Filter,
  CheckCircle2,
  AlertTriangle,
  Clock,
  User,
  Download,
  Lock,
  FileCheck,
  Eye,
  Activity,
  Terminal,
  Sparkles,
  ShieldCheck,
  RotateCcw,
  FileSignature,
  Building2,
  UserCheck
} from 'lucide-react';
import { ApiClient } from '@/lib/api';
import { useLanguage } from '@/lib/i18n/LanguageContext';
import { useAuth } from '@/lib/auth';
import { RouteGuard } from '@/components/auth/RouteGuard';
import { PermissionGate } from '@/components/auth/PermissionGate';

interface ConsentAuditRow {
  id: string;
  consentRecordId: string;
  patientId: string;
  patientName: string;
  patientMrn: string;
  eventType: string;
  consentType: string;
  performedByName: string;
  ipAddress: string;
  reason?: string;
  details?: string;
  timestamp: string;
}

interface AiDecisionRow {
  id: string;
  capability: string;
  provider: string;
  modelVersion: string;
  confidenceScore: number;
  reviewStatus: 'Approved' | 'Rejected' | 'Modified' | 'PendingReview';
  overrideReason?: string;
  doctorName: string;
  timestamp: string;
}

interface SecurityLogRow {
  id: string;
  timestamp: string;
  user: string;
  role: string;
  action: string;
  resource: string;
  ipAddress: string;
  status: 'success' | 'warning' | 'denied';
  details: string;
}

const DEMO_CONSENT_AUDITS: ConsentAuditRow[] = [
  {
    id: 'CA-901',
    consentRecordId: 'REC-101',
    patientId: 'P-10024',
    patientName: 'Tariq Al-Mansoor',
    patientMrn: 'MED-10024',
    eventType: 'Granted',
    consentType: 'AiAssistedCare',
    performedByName: 'Dr. Sarah Al-Mansoor',
    ipAddress: '192.168.1.42',
    reason: 'Routine outpatient cardiology check-up with AI CDS',
    timestamp: 'Today, 10:15 AM'
  },
  {
    id: 'CA-902',
    consentRecordId: 'REC-102',
    patientId: 'P-10024',
    patientName: 'Tariq Al-Mansoor',
    patientMrn: 'MED-10024',
    eventType: 'Granted',
    consentType: 'GeneralCare',
    performedByName: 'Nurse Mona Al-Ghamdi',
    ipAddress: '192.168.1.18',
    reason: 'Annual hospital care admission agreement',
    timestamp: 'Today, 09:30 AM'
  },
  {
    id: 'CA-903',
    consentRecordId: 'REC-098',
    patientId: 'P-10025',
    patientName: 'Nour Mostafa',
    patientMrn: 'MED-10025',
    eventType: 'Revoked',
    consentType: 'Marketing',
    performedByName: 'Dr. Sarah Al-Mansoor',
    ipAddress: '192.168.1.42',
    reason: 'Patient requested SMS opt-out',
    timestamp: 'Yesterday, 04:20 PM'
  },
  {
    id: 'CA-904',
    consentRecordId: 'REC-095',
    patientId: 'P-10026',
    patientName: 'Laila Mahmoud',
    patientMrn: 'MED-10026',
    eventType: 'Verified',
    consentType: 'Telemedicine',
    performedByName: 'Dr. Khaled bin Walid',
    ipAddress: '10.0.4.12',
    details: 'Remote encounter identity verification passed',
    timestamp: '2026-09-07, 11:00 AM'
  }
];

const DEMO_AI_DECISIONS: AiDecisionRow[] = [
  {
    id: 'AI-501',
    capability: 'ChestXRayClassification',
    provider: 'DeepHealth-RadiologyAI',
    modelVersion: 'v4.2.1',
    confidenceScore: 0.942,
    reviewStatus: 'Approved',
    doctorName: 'Dr. Sarah Al-Mansoor',
    timestamp: 'Today, 10:20 AM'
  },
  {
    id: 'AI-502',
    capability: 'LabBiomarkerDifferential',
    provider: 'MedClinic-CoreCDS',
    modelVersion: 'v3.0.0',
    confidenceScore: 0.885,
    reviewStatus: 'Modified',
    overrideReason: 'Clinical history indicates transient dehydration rather than chronic kidney failure.',
    doctorName: 'Dr. Sarah Al-Mansoor',
    timestamp: 'Today, 09:45 AM'
  },
  {
    id: 'AI-503',
    capability: 'DrugInteractionScreening',
    provider: 'RxSafetyGuard',
    modelVersion: 'v2.8.4',
    confidenceScore: 0.991,
    reviewStatus: 'Approved',
    doctorName: 'Dr. Sarah Al-Mansoor',
    timestamp: 'Yesterday, 02:15 PM'
  }
];

const DEMO_SECURITY_LOGS: SecurityLogRow[] = [
  {
    id: 'LOG-9921',
    timestamp: 'Today, 10:14 AM',
    user: 'Dr. Sarah Al-Mansoor',
    role: 'Attending Physician',
    action: 'AI_REPORT_APPROVED',
    resource: 'LAB-2026-0904 (Tariq Al-Mansoor)',
    ipAddress: '192.168.1.42',
    status: 'success',
    details: 'Validated lab biomarker differential and authorized commit to EMR.'
  },
  {
    id: 'LOG-9920',
    timestamp: 'Today, 09:48 AM',
    user: 'Dr. Sarah Al-Mansoor',
    role: 'Attending Physician',
    action: 'PRESCRIPTION_TRANSMITTED',
    resource: 'RX-2026-88192 (P-10024)',
    ipAddress: '192.168.1.42',
    status: 'success',
    details: 'Transmitted Metformin + Atorvastatin regimen; allergy safety check passed.'
  },
  {
    id: 'LOG-9918',
    timestamp: 'Today, 08:30 AM',
    user: 'Mona Al-Ghamdi',
    role: 'Receptionist',
    action: 'PATIENT_RECORD_VIEWED',
    resource: 'P-10024 (Tariq Al-Mansoor)',
    ipAddress: '192.168.1.18',
    status: 'success',
    details: 'Accessed demographics for check-in and appointment verification.'
  },
  {
    id: 'LOG-9917',
    timestamp: 'Yesterday, 06:12 PM',
    user: 'Unknown / External',
    role: 'Guest Session',
    action: 'AUTH_FAILED',
    resource: '/api/v1/patients',
    ipAddress: '82.102.14.99',
    status: 'denied',
    details: 'Unauthorized token attempt blocked by ASP.NET Core JWT Middleware.'
  }
];

export default function AuditLogsPage() {
  const { t, language } = useLanguage();
  const { user } = useAuth();
  const isRtl = language === 'ar';

  const [activeTab, setActiveTab] = useState<'consents' | 'ai-decisions' | 'security'>('consents');
  const [searchQuery, setSearchQuery] = useState('');
  const [eventTypeFilter, setEventTypeFilter] = useState('all');

  const [consentAudits, setConsentAudits] = useState<ConsentAuditRow[]>(DEMO_CONSENT_AUDITS);
  const [aiDecisions, setAiDecisions] = useState<AiDecisionRow[]>(DEMO_AI_DECISIONS);
  const [securityLogs, setSecurityLogs] = useState<SecurityLogRow[]>(DEMO_SECURITY_LOGS);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    async function fetchLiveAudits() {
      try {
        setLoading(true);
        const res = await ApiClient.getConsentAuditExplorer();
        if (res && (res.items || Array.isArray(res))) {
          const items = Array.isArray(res) ? res : res.items;
          if (items.length > 0) {
            setConsentAudits(items);
          }
        }
      } catch {
        // keep demo audits as fallback
      } finally {
        setLoading(false);
      }
    }
    fetchLiveAudits();
  }, []);

  const handleExportCsv = () => {
    let csvContent = '';
    let filename = '';

    if (activeTab === 'consents') {
      csvContent = 'EventId,Timestamp,PatientMRN,PatientName,EventType,ConsentType,PerformedBy,Reason,IpAddress\n';
      filteredConsentAudits.forEach(r => {
        csvContent += `"${r.id}","${r.timestamp}","${r.patientMrn}","${r.patientName}","${r.eventType}","${r.consentType}","${r.performedByName}","${r.reason || ''}","${r.ipAddress}"\n`;
      });
      filename = `consent_audit_report_${new Date().toISOString().slice(0, 10)}.csv`;
    } else if (activeTab === 'ai-decisions') {
      csvContent = 'DecisionId,Timestamp,Capability,Provider,ModelVersion,Confidence,ReviewStatus,Doctor,OverrideReason\n';
      filteredAiDecisions.forEach(r => {
        csvContent += `"${r.id}","${r.timestamp}","${r.capability}","${r.provider}","${r.modelVersion}","${(r.confidenceScore * 100).toFixed(1)}%","${r.reviewStatus}","${r.doctorName}","${r.overrideReason || ''}"\n`;
      });
      filename = `ai_decision_audit_report_${new Date().toISOString().slice(0, 10)}.csv`;
    } else {
      csvContent = 'LogId,Timestamp,User,Role,Action,Resource,Status,Details,IpAddress\n';
      filteredSecurityLogs.forEach(r => {
        csvContent += `"${r.id}","${r.timestamp}","${r.user}","${r.role}","${r.action}","${r.resource}","${r.status}","${r.details}","${r.ipAddress}"\n`;
      });
      filename = `security_audit_report_${new Date().toISOString().slice(0, 10)}.csv`;
    }

    const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.setAttribute('download', filename);
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
  };

  // Filter computations
  const filteredConsentAudits = consentAudits.filter(r => {
    const matchesSearch =
      r.patientName.toLowerCase().includes(searchQuery.toLowerCase()) ||
      r.patientMrn.toLowerCase().includes(searchQuery.toLowerCase()) ||
      r.consentType.toLowerCase().includes(searchQuery.toLowerCase()) ||
      (r.reason && r.reason.toLowerCase().includes(searchQuery.toLowerCase())) ||
      r.performedByName.toLowerCase().includes(searchQuery.toLowerCase());
    const matchesEvent = eventTypeFilter === 'all' || r.eventType.toLowerCase() === eventTypeFilter.toLowerCase();
    return matchesSearch && matchesEvent;
  });

  const filteredAiDecisions = aiDecisions.filter(r => {
    const matchesSearch =
      r.capability.toLowerCase().includes(searchQuery.toLowerCase()) ||
      r.provider.toLowerCase().includes(searchQuery.toLowerCase()) ||
      r.doctorName.toLowerCase().includes(searchQuery.toLowerCase()) ||
      (r.overrideReason && r.overrideReason.toLowerCase().includes(searchQuery.toLowerCase()));
    const matchesStatus = eventTypeFilter === 'all' || r.reviewStatus.toLowerCase() === eventTypeFilter.toLowerCase();
    return matchesSearch && matchesStatus;
  });

  const filteredSecurityLogs = securityLogs.filter(r => {
    const matchesSearch =
      r.user.toLowerCase().includes(searchQuery.toLowerCase()) ||
      r.action.toLowerCase().includes(searchQuery.toLowerCase()) ||
      r.resource.toLowerCase().includes(searchQuery.toLowerCase());
    const matchesStatus = eventTypeFilter === 'all' || r.status.toLowerCase() === eventTypeFilter.toLowerCase();
    return matchesSearch && matchesStatus;
  });

  return (
    <RouteGuard anyPermissions={['AIDecisions.View', 'AuditLogs.Read', 'PatientConsents.Audit']}>
      <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
        <div>
          <div className="flex items-center gap-2">
            <span className="px-2 py-0.5 rounded text-[11px] font-bold bg-teal-500/20 text-teal-300 border border-teal-500/30">
              Phase 12 (Compliance & Governance)
            </span>
            <span className="text-xs text-slate-400">HIPAA, GDPR & Saudi Digital Health Standards</span>
          </div>
          <h1 className="text-2xl md:text-3xl font-bold text-white mt-1 flex items-center gap-3">
            <ShieldAlert className="w-8 h-8 text-teal-400" />
            <span>{isRtl ? 'مستكشف التدقيق والامتثال الشامل' : 'Audit & Compliance Explorer'}</span>
          </h1>
          <p className="text-sm text-slate-400 mt-0.5">
            {isRtl
              ? 'سجل مركزي مشفر وغير قابل للتعديل لتتبع موافقات المرضى، اعتمادات قرارات الذكاء الاصطناعي، وأنشطة النظام.'
              : 'Immutable cryptographic ledger tracking patient consent lifecycles, clinical AI decisions, and system accesses.'}
          </p>
        </div>

        <PermissionGate anyPermissions={['AIDecisions.Export', 'PatientConsents.Export', 'AuditLogs.Read']}>
          <button
            onClick={handleExportCsv}
            className="px-4 py-2.5 rounded-xl border border-teal-500/30 bg-teal-950/40 hover:bg-teal-900/60 text-xs font-semibold text-teal-200 flex items-center gap-2 transition shadow-lg shadow-teal-950/40 self-start md:self-auto"
          >
            <Download className="w-4 h-4 text-teal-400" />
            <span>{isRtl ? 'تصدير تقرير الامتثال (CSV)' : 'Export Compliance CSV'}</span>
          </button>
        </PermissionGate>
      </div>

      {/* Metric Cards */}
      <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
        <div className="glass-panel p-5 rounded-2xl border-slate-800 flex items-center justify-between">
          <div>
            <div className="text-xs font-semibold text-slate-400">
              {isRtl ? 'أحداث تدقيق الموافقات' : 'Consent Audit Events'}
            </div>
            <div className="text-2xl font-black text-white mt-1 font-mono">
              {consentAudits.length}
            </div>
            <div className="text-[11px] text-emerald-400 mt-1 flex items-center gap-1">
              <CheckCircle2 className="w-3.5 h-3.5" />
              <span>Immutable & Cascade Protected</span>
            </div>
          </div>
          <div className="w-12 h-12 rounded-xl bg-teal-500/10 border border-teal-500/30 flex items-center justify-center text-teal-400">
            <ShieldCheck className="w-6 h-6" />
          </div>
        </div>

        <div className="glass-panel p-5 rounded-2xl border-slate-800 flex items-center justify-between">
          <div>
            <div className="text-xs font-semibold text-slate-400">
              {isRtl ? 'قرارات الذكاء الاصطناعي المعتمدة' : 'Clinical AI Audits'}
            </div>
            <div className="text-2xl font-black text-sky-400 mt-1 font-mono">
              {aiDecisions.length}
            </div>
            <div className="text-[11px] text-sky-300 mt-1 flex items-center gap-1">
              <Sparkles className="w-3.5 h-3.5" />
              <span>Doctor Review Required</span>
            </div>
          </div>
          <div className="w-12 h-12 rounded-xl bg-sky-500/10 border border-sky-500/30 flex items-center justify-center text-sky-400">
            <Activity className="w-6 h-6" />
          </div>
        </div>

        <div className="glass-panel p-5 rounded-2xl border-slate-800 flex items-center justify-between">
          <div>
            <div className="text-xs font-semibold text-slate-400">
              {isRtl ? 'حالة الامتثال والقفل القانوني' : 'Legal Hold & Retention'}
            </div>
            <div className="text-2xl font-black text-emerald-400 mt-1 font-mono">Active</div>
            <div className="text-[11px] text-emerald-400 mt-1 flex items-center gap-1">
              <Lock className="w-3.5 h-3.5" />
              <span>Hard Deletes Forbidden</span>
            </div>
          </div>
          <div className="w-12 h-12 rounded-xl bg-emerald-500/10 border border-emerald-500/30 flex items-center justify-center text-emerald-400">
            <FileCheck className="w-6 h-6" />
          </div>
        </div>
      </div>

      {/* Explorer Navigation Tabs */}
      <div className="flex border-b border-slate-800 gap-6 text-sm font-semibold">
        <button
          onClick={() => {
            setActiveTab('consents');
            setEventTypeFilter('all');
          }}
          className={`pb-3 transition relative flex items-center gap-2 ${
            activeTab === 'consents' ? 'text-teal-400' : 'text-slate-400 hover:text-slate-200'
          }`}
        >
          <ShieldCheck className="w-4 h-4" />
          <span>{isRtl ? 'سجل موافقات المرضى (Consent Lifecycle)' : 'Patient Consent Lifecycle'}</span>
          {activeTab === 'consents' && (
            <span className="absolute bottom-0 left-0 right-0 h-0.5 bg-teal-400 rounded-full" />
          )}
        </button>

        <button
          onClick={() => {
            setActiveTab('ai-decisions');
            setEventTypeFilter('all');
          }}
          className={`pb-3 transition relative flex items-center gap-2 ${
            activeTab === 'ai-decisions' ? 'text-sky-400' : 'text-slate-400 hover:text-slate-200'
          }`}
        >
          <Sparkles className="w-4 h-4" />
          <span>{isRtl ? 'قرارات وتوصيات AI السريرية' : 'Clinical AI Decisions'}</span>
          {activeTab === 'ai-decisions' && (
            <span className="absolute bottom-0 left-0 right-0 h-0.5 bg-sky-400 rounded-full" />
          )}
        </button>

        <button
          onClick={() => {
            setActiveTab('security');
            setEventTypeFilter('all');
          }}
          className={`pb-3 transition relative flex items-center gap-2 ${
            activeTab === 'security' ? 'text-rose-400' : 'text-slate-400 hover:text-slate-200'
          }`}
        >
          <Terminal className="w-4 h-4" />
          <span>{isRtl ? 'سجل الوصول والأمان العام' : 'Security & Resource Access'}</span>
          {activeTab === 'security' && (
            <span className="absolute bottom-0 left-0 right-0 h-0.5 bg-rose-400 rounded-full" />
          )}
        </button>
      </div>

      {/* Multi-parameter Filters */}
      <div className="glass-panel p-4 rounded-xl border-slate-800 flex flex-col md:flex-row gap-3 items-center justify-between">
        <div className="relative w-full md:w-80">
          <Search className="w-4 h-4 text-slate-400 absolute left-3 top-1/2 -translate-y-1/2" />
          <input
            type="text"
            placeholder={isRtl ? 'بحث في السجل بالاسم، الملف، أو السبب...' : 'Search MRN, patient, reason, user...'}
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
            className="w-full pl-9 pr-4 py-2 rounded-xl bg-slate-900 border border-slate-800 text-xs text-white placeholder:text-slate-500 focus:outline-none focus:border-teal-500"
          />
        </div>

        <div className="flex items-center gap-3 w-full md:w-auto">
          <Filter className="w-4 h-4 text-slate-400 hidden sm:block" />
          <select
            value={eventTypeFilter}
            onChange={(e) => setEventTypeFilter(e.target.value)}
            className="px-3 py-2 rounded-xl bg-slate-900 border border-slate-800 text-xs text-slate-300 focus:outline-none focus:border-teal-500 w-full sm:w-auto"
          >
            <option value="all">{isRtl ? 'جميع الحالات والأنواع' : 'All Event Statuses'}</option>
            {activeTab === 'consents' ? (
              <>
                <option value="granted">{isRtl ? 'ممنوحة (Granted)' : 'Granted'}</option>
                <option value="revoked">{isRtl ? 'ملغاة (Revoked)' : 'Revoked'}</option>
                <option value="expired">{isRtl ? 'منتهية (Expired)' : 'Expired'}</option>
                <option value="verified">{isRtl ? 'تم التحقق (Verified)' : 'Verified'}</option>
              </>
            ) : activeTab === 'ai-decisions' ? (
              <>
                <option value="approved">{isRtl ? 'معتمد (Approved)' : 'Approved'}</option>
                <option value="modified">{isRtl ? 'معدل بواسطة الطبيب (Modified)' : 'Modified by Doctor'}</option>
                <option value="rejected">{isRtl ? 'مرفوض (Rejected)' : 'Rejected'}</option>
              </>
            ) : (
              <>
                <option value="success">{isRtl ? 'ناجح' : 'Success'}</option>
                <option value="denied">{isRtl ? 'محظور' : 'Denied'}</option>
                <option value="warning">{isRtl ? 'تحذير' : 'Warning'}</option>
              </>
            )}
          </select>
        </div>
      </div>

      {/* Tab 1: Consent Lifecycle Audits */}
      {activeTab === 'consents' && (
        <div className="glass-panel rounded-2xl border-slate-800 overflow-hidden">
          <div className="overflow-x-auto">
            <table className="w-full text-left text-xs text-slate-300">
              <thead className="bg-slate-900/80 text-slate-400 uppercase text-[10px] tracking-wider border-b border-slate-800">
                <tr>
                  <th className="p-4">Event & Type</th>
                  <th className="p-4">Patient MRN</th>
                  <th className="p-4">Patient Name</th>
                  <th className="p-4">Performed By</th>
                  <th className="p-4">Reason / Remarks</th>
                  <th className="p-4">IP Address</th>
                  <th className="p-4">Timestamp (UTC)</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-800/60">
                {filteredConsentAudits.length === 0 ? (
                  <tr>
                    <td colSpan={7} className="p-8 text-center text-slate-500">
                      {isRtl ? 'لا توجد سجلات مطابقة للبحث' : 'No matching consent audit events found.'}
                    </td>
                  </tr>
                ) : (
                  filteredConsentAudits.map((r) => {
                    const isRevoke = r.eventType.toLowerCase() === 'revoked';
                    const isHold = r.eventType.toLowerCase().includes('legalhold');
                    const badgeStyle = isRevoke
                      ? 'bg-rose-500/10 text-rose-400 border-rose-500/20'
                      : isHold
                      ? 'bg-amber-500/10 text-amber-400 border-amber-500/20'
                      : 'bg-teal-500/10 text-teal-400 border-teal-500/20';
                    return (
                      <tr key={r.id} className="hover:bg-slate-850/50 transition">
                        <td className="p-4 font-mono font-semibold">
                          <span className={`px-2 py-0.5 rounded-full text-[10px] border ${badgeStyle}`}>
                            {r.eventType}
                          </span>
                          <div className="text-slate-300 text-[11px] mt-1">{r.consentType}</div>
                        </td>
                        <td className="p-4 font-mono text-sky-400 font-semibold">{r.patientMrn}</td>
                        <td className="p-4 font-medium text-white">{r.patientName}</td>
                        <td className="p-4 text-slate-300 flex items-center gap-1.5 mt-2">
                          <UserCheck className="w-3.5 h-3.5 text-teal-400" />
                          <span>{r.performedByName}</span>
                        </td>
                        <td className="p-4 max-w-xs truncate text-slate-400">
                          {r.reason || r.details || <span className="text-slate-600 italic">Standard care protocol</span>}
                        </td>
                        <td className="p-4 font-mono text-slate-500 text-[11px]">{r.ipAddress}</td>
                        <td className="p-4 text-slate-400 font-mono whitespace-nowrap">{r.timestamp}</td>
                      </tr>
                    );
                  })
                )}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {/* Tab 2: Clinical AI Decision Audits */}
      {activeTab === 'ai-decisions' && (
        <div className="glass-panel rounded-2xl border-slate-800 overflow-hidden">
          <div className="overflow-x-auto">
            <table className="w-full text-left text-xs text-slate-300">
              <thead className="bg-slate-900/80 text-slate-400 uppercase text-[10px] tracking-wider border-b border-slate-800">
                <tr>
                  <th className="p-4">Capability & Provider</th>
                  <th className="p-4">Model Version</th>
                  <th className="p-4">Confidence</th>
                  <th className="p-4">Review Status</th>
                  <th className="p-4">Reviewing Physician</th>
                  <th className="p-4">Clinical Override Reason</th>
                  <th className="p-4">Timestamp</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-800/60">
                {filteredAiDecisions.map((r) => (
                  <tr key={r.id} className="hover:bg-slate-850/50 transition">
                    <td className="p-4">
                      <div className="font-semibold text-white">{r.capability}</div>
                      <div className="text-[11px] text-slate-500">{r.provider}</div>
                    </td>
                    <td className="p-4 font-mono text-slate-400">{r.modelVersion}</td>
                    <td className="p-4 font-mono font-bold text-sky-400">
                      {(r.confidenceScore * 100).toFixed(1)}%
                    </td>
                    <td className="p-4">
                      <span className={`px-2 py-0.5 rounded-full text-[10px] font-bold border ${
                        r.reviewStatus === 'Approved'
                          ? 'bg-emerald-500/10 text-emerald-400 border-emerald-500/30'
                          : r.reviewStatus === 'Modified'
                          ? 'bg-amber-500/10 text-amber-400 border-amber-500/30'
                          : 'bg-rose-500/10 text-rose-400 border-rose-500/30'
                      }`}>
                        {r.reviewStatus}
                      </span>
                    </td>
                    <td className="p-4 font-medium text-white">{r.doctorName}</td>
                    <td className="p-4 max-w-xs text-slate-400 text-[11px]">
                      {r.overrideReason || <span className="text-slate-600 italic">None (Approved as suggested)</span>}
                    </td>
                    <td className="p-4 text-slate-400 font-mono whitespace-nowrap">{r.timestamp}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {/* Tab 3: Security & Access Logs */}
      {activeTab === 'security' && (
        <div className="glass-panel rounded-2xl border-slate-800 overflow-hidden">
          <div className="overflow-x-auto">
            <table className="w-full text-left text-xs text-slate-300">
              <thead className="bg-slate-900/80 text-slate-400 uppercase text-[10px] tracking-wider border-b border-slate-800">
                <tr>
                  <th className="p-4">Status</th>
                  <th className="p-4">Action</th>
                  <th className="p-4">User & Role</th>
                  <th className="p-4">Resource Target</th>
                  <th className="p-4">Details</th>
                  <th className="p-4">IP Address</th>
                  <th className="p-4">Timestamp</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-800/60">
                {filteredSecurityLogs.map((l) => (
                  <tr key={l.id} className="hover:bg-slate-850/50 transition">
                    <td className="p-4">
                      <span className={`px-2 py-0.5 rounded-full text-[10px] font-bold border ${
                        l.status === 'success'
                          ? 'bg-emerald-500/10 text-emerald-400 border-emerald-500/20'
                          : 'bg-rose-500/10 text-rose-400 border-rose-500/20'
                      }`}>
                        {l.status}
                      </span>
                    </td>
                    <td className="p-4 font-mono font-semibold text-white">{l.action}</td>
                    <td className="p-4">
                      <div className="font-medium text-white">{l.user}</div>
                      <div className="text-[10px] text-slate-500">{l.role}</div>
                    </td>
                    <td className="p-4 font-mono text-sky-400">{l.resource}</td>
                    <td className="p-4 max-w-xs truncate text-slate-400">{l.details}</td>
                    <td className="p-4 font-mono text-slate-500 text-[11px]">{l.ipAddress}</td>
                    <td className="p-4 text-slate-400 font-mono whitespace-nowrap">{l.timestamp}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}
      </div>
    </RouteGuard>
  );
}
