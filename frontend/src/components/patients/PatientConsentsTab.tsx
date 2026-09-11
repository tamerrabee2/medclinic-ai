'use client';

import React, { useState, useEffect } from 'react';
import {
  ShieldCheck,
  ShieldAlert,
  ShieldX,
  Clock,
  CheckCircle2,
  AlertTriangle,
  RotateCcw,
  FileSignature,
  PlusCircle,
  History,
  Lock,
  UserCheck,
  Calendar,
  Sparkles
} from 'lucide-react';
import { ApiClient } from '@/lib/api';
import { useLanguage } from '@/lib/i18n/LanguageContext';
import { useAuth } from '@/lib/auth';

export interface ConsentItem {
  id: string;
  consentType: string | number;
  isGranted: boolean;
  grantedAt: string;
  expiresAt: string | null;
  witnessUserId?: string | null;
  grantedByUserId?: string | null;
  revokedByUserId?: string | null;
  revokedAt?: string | null;
  revocationReason?: string | null;
  notes?: string | null;
  isActive: boolean;
  isLegalHold?: boolean;
  legalHoldReason?: string | null;
}

export interface ConsentAuditEvent {
  id: string;
  eventType: string | number;
  consentType: string | number;
  performedByName?: string | null;
  reason?: string | null;
  details?: string | null;
  timestamp: string;
  ipAddress?: string | null;
}

const CONSENT_TYPES = [
  { id: 0, key: 'GeneralCare', enTitle: 'General Clinical Care', arTitle: 'الرعاية الطبية العامة', desc: 'Standard diagnosis, treatment, and medical exams' },
  { id: 1, key: 'AiAssistedCare', enTitle: 'AI-Assisted Diagnostics & Care', arTitle: 'الرعاية والتشخيص بالذكاء الاصطناعي', desc: 'Machine learning inference, radiological CDS, and biomarker analytics' },
  { id: 2, key: 'DataSharing', enTitle: 'Clinical Data Sharing', arTitle: 'مشاركة البيانات السريرية', desc: 'Secure transmission to affiliated labs and partner hospitals' },
  { id: 3, key: 'Research', enTitle: 'Medical & Scientific Research', arTitle: 'الأبحاث الطبية والأكاديمية', desc: 'Anonymized observational trials and statistical registry inclusion' },
  { id: 4, key: 'Telemedicine', enTitle: 'Telehealth & Remote Consultations', arTitle: 'الاستشارات الطبية عن بعد', desc: 'Encrypted video encounters, remote vitals, and virtual triage' },
  { id: 5, key: 'Marketing', enTitle: 'Educational & Health Alerts', arTitle: 'الإشعارات والتوعية الصحية', desc: 'Appointment reminders, preventive care bulletins, and health SMS' },
];

export function PatientConsentsTab({ patientId }: { patientId: string }) {
  const { t, language } = useLanguage();
  const { user, hasPermission } = useAuth();
  const isRtl = language === 'ar';

  const [consents, setConsents] = useState<ConsentItem[]>([]);
  const [auditEvents, setAuditEvents] = useState<ConsentAuditEvent[]>([]);
  const [loading, setLoading] = useState(true);
  const [actionLoading, setActionLoading] = useState(false);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  // Modals state
  const [isGrantOpen, setIsGrantOpen] = useState(false);
  const [selectedTypeForGrant, setSelectedTypeForGrant] = useState<number>(1); // default AI
  const [grantNotes, setGrantNotes] = useState('');
  const [grantDurationDays, setGrantDurationDays] = useState<number>(365);

  const [isRevokeOpen, setIsRevokeOpen] = useState(false);
  const [selectedConsentForRevoke, setSelectedConsentForRevoke] = useState<ConsentItem | null>(null);
  const [revocationReason, setRevocationReason] = useState('');

  // RBAC checks - Authoritative permission based
  const canManageConsents = hasPermission('PatientConsents.Manage');
  const canRevokeConsents = hasPermission('PatientConsents.Revoke');

  const loadData = async () => {
    try {
      setLoading(true);
      setErrorMessage(null);
      const res = await ApiClient.getPatientConsents(patientId);
      const items = Array.isArray(res) ? res : res?.items || [];
      setConsents(items);

      try {
        const auditRes = await ApiClient.getConsentAuditTrail(patientId);
        const trail = Array.isArray(auditRes) ? auditRes : [];
        setAuditEvents(trail);
      } catch {
        // Audit trail might be empty or restricted
      }
    } catch (err: any) {
      setErrorMessage(err.message || 'Failed to load consent records');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    if (patientId) {
      loadData();
    }
  }, [patientId]);

  const handleGrantSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      setActionLoading(true);
      setErrorMessage(null);

      const expiresAt = grantDurationDays > 0
        ? new Date(Date.now() + grantDurationDays * 86400000).toISOString()
        : null;

      await ApiClient.recordConsent(patientId, {
        consentType: selectedTypeForGrant,
        isGranted: true,
        expiresAt,
        notes: grantNotes.trim() || undefined
      });

      setIsGrantOpen(false);
      setGrantNotes('');
      await loadData();
    } catch (err: any) {
      setErrorMessage(err.message || 'Failed to record consent');
    } finally {
      setActionLoading(false);
    }
  };

  const handleRevokeSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!selectedConsentForRevoke) return;
    if (!revocationReason.trim()) {
      setErrorMessage(isRtl ? 'سبب الإلغاء إلزامي' : 'Revocation reason is strictly mandatory');
      return;
    }

    try {
      setActionLoading(true);
      setErrorMessage(null);

      await ApiClient.revokeConsent(
        patientId,
        selectedConsentForRevoke.id,
        revocationReason.trim()
      );

      setIsRevokeOpen(false);
      setSelectedConsentForRevoke(null);
      setRevocationReason('');
      await loadData();
    } catch (err: any) {
      setErrorMessage(err.message || 'Failed to revoke consent');
    } finally {
      setActionLoading(false);
    }
  };

  const getConsentStatus = (c?: ConsentItem) => {
    if (!c) return { label: t.consentMissing, color: 'text-slate-400 bg-slate-800/80 border-slate-700', state: 'missing' };
    if (c.revokedAt) return { label: t.consentRevoked, color: 'text-rose-400 bg-rose-500/10 border-rose-500/30', state: 'revoked' };
    if (c.expiresAt && new Date(c.expiresAt) < new Date()) {
      return { label: t.consentExpired, color: 'text-amber-400 bg-amber-500/10 border-amber-500/30', state: 'expired' };
    }
    if (c.isGranted && c.isActive) {
      return { label: t.consentActive, color: 'text-emerald-400 bg-emerald-500/10 border-emerald-500/30', state: 'active' };
    }
    return { label: t.consentMissing, color: 'text-slate-400 bg-slate-800/80 border-slate-700', state: 'missing' };
  };

  return (
    <div className="space-y-6">
      {/* Banner / Header */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 glass-panel p-5 rounded-2xl border-slate-800">
        <div>
          <div className="flex items-center gap-2">
            <ShieldCheck className="w-5 h-5 text-teal-400" />
            <h2 className="text-lg font-bold text-white">
              {t.consentsAndCompliance}
            </h2>
            <span className="px-2 py-0.5 rounded text-[10px] font-bold bg-teal-500/20 text-teal-300 border border-teal-500/30">
              HIPAA & GDPR Module
            </span>
          </div>
          <p className="text-xs text-slate-400 mt-1">
            {isRtl
              ? 'إدارة تفويضات وموافقات المريض القانونية لحوكمة البيانات والرعاية السريرية المدعومة بالذكاء الاصطناعي.'
              : 'Legal patient consent tracking, immutable regulatory audit logs, and AI capability access guards.'}
          </p>
        </div>

        {canManageConsents && (
          <button
            onClick={() => {
              setSelectedTypeForGrant(1);
              setIsGrantOpen(true);
            }}
            className="px-4 py-2 rounded-xl bg-gradient-to-r from-teal-600 to-sky-600 hover:from-teal-500 hover:to-sky-500 text-xs font-semibold text-white flex items-center gap-2 transition shadow-lg shadow-teal-600/20 self-start sm:self-auto"
          >
            <PlusCircle className="w-4 h-4" />
            <span>{t.grantConsent}</span>
          </button>
        )}
      </div>

      {errorMessage && (
        <div className="p-4 rounded-xl bg-rose-500/10 border border-rose-500/30 text-xs text-rose-300 flex items-center gap-3">
          <AlertTriangle className="w-4 h-4 text-rose-400 flex-shrink-0" />
          <span>{errorMessage}</span>
        </div>
      )}

      {/* Grid of 6 Standard Consent Categories */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
        {CONSENT_TYPES.map((typeMeta) => {
          // Find matching consent record by type
          const record = consents.find((c) => {
            const cTypeStr = String(c.consentType).toLowerCase();
            return cTypeStr === typeMeta.key.toLowerCase() || cTypeStr === String(typeMeta.id);
          });

          const status = getConsentStatus(record);
          const isAiConsent = typeMeta.key === 'AiAssistedCare';

          return (
            <div
              key={typeMeta.id}
              className={`glass-panel p-5 rounded-2xl border transition relative flex flex-col justify-between ${
                isAiConsent ? 'border-sky-500/40 bg-sky-950/10' : 'border-slate-800'
              }`}
            >
              <div>
                <div className="flex items-start justify-between gap-2">
                  <div className="flex items-center gap-2">
                    <div className={`w-8 h-8 rounded-lg flex items-center justify-center ${
                      isAiConsent ? 'bg-sky-500/20 text-sky-400' : 'bg-slate-800 text-slate-300'
                    }`}>
                      {isAiConsent ? <Sparkles className="w-4 h-4" /> : <FileSignature className="w-4 h-4" />}
                    </div>
                    <div>
                      <h3 className="text-sm font-bold text-white">
                        {isRtl ? typeMeta.arTitle : typeMeta.enTitle}
                      </h3>
                      <span className="text-[10px] text-slate-500 font-mono">
                        {typeMeta.key}
                      </span>
                    </div>
                  </div>

                  <span className={`px-2 py-0.5 rounded-full text-[10px] font-bold border flex items-center gap-1 ${status.color}`}>
                    {status.state === 'active' && <CheckCircle2 className="w-3 h-3" />}
                    {status.state === 'revoked' && <ShieldX className="w-3 h-3" />}
                    {status.state === 'expired' && <Clock className="w-3 h-3" />}
                    <span>{status.label}</span>
                  </span>
                </div>

                <p className="text-xs text-slate-400 mt-3 leading-relaxed">
                  {typeMeta.desc}
                </p>

                {/* Record Details if exists */}
                {record && (
                  <div className="mt-4 pt-3 border-t border-slate-800/80 space-y-1.5 text-[11px] text-slate-400">
                    <div className="flex justify-between">
                      <span>{isRtl ? 'تاريخ التوقيع:' : 'Granted At:'}</span>
                      <span className="text-slate-200 font-mono">
                        {new Date(record.grantedAt).toLocaleDateString()}
                      </span>
                    </div>
                    <div className="flex justify-between">
                      <span>{isRtl ? 'تاريخ الانتهاء:' : 'Expires:'}</span>
                      <span className="text-slate-200 font-mono">
                        {record.expiresAt ? new Date(record.expiresAt).toLocaleDateString() : (isRtl ? 'دائم' : 'Permanent')}
                      </span>
                    </div>
                    {record.revokedAt && (
                      <div className="p-2 rounded-lg bg-rose-500/10 border border-rose-500/20 text-rose-300 mt-2">
                        <div className="font-semibold text-[10px]">{isRtl ? 'سبب الإلغاء:' : 'Revocation Reason:'}</div>
                        <div className="italic text-[10px]">{record.revocationReason || 'N/A'}</div>
                      </div>
                    )}
                    {record.isLegalHold && (
                      <div className="px-2 py-1 rounded bg-amber-500/10 border border-amber-500/30 text-amber-300 text-[10px] flex items-center gap-1 font-semibold mt-2">
                        <Lock className="w-3 h-3 text-amber-400" />
                        <span>{t.legalHold}: {record.legalHoldReason || 'Active Compliance Hold'}</span>
                      </div>
                    )}
                  </div>
                )}
              </div>

              {/* Action Buttons */}
              <div className="mt-5 pt-3 border-t border-slate-800/80 flex items-center gap-2">
                {record?.isActive && canRevokeConsents ? (
                  <button
                    onClick={() => {
                      setSelectedConsentForRevoke(record);
                      setIsRevokeOpen(true);
                    }}
                    className="w-full py-1.5 px-3 rounded-lg border border-rose-500/30 hover:bg-rose-500/20 text-rose-300 text-xs font-semibold flex items-center justify-center gap-1.5 transition"
                  >
                    <ShieldX className="w-3.5 h-3.5" />
                    <span>{t.revokeConsent}</span>
                  </button>
                ) : canManageConsents ? (
                  <button
                    onClick={() => {
                      setSelectedTypeForGrant(typeMeta.id);
                      setIsGrantOpen(true);
                    }}
                    className="w-full py-1.5 px-3 rounded-lg bg-slate-800 hover:bg-slate-700 text-slate-200 text-xs font-semibold flex items-center justify-center gap-1.5 transition border border-slate-700"
                  >
                    <FileSignature className="w-3.5 h-3.5 text-teal-400" />
                    <span>{t.grantConsent}</span>
                  </button>
                ) : (
                  <span className="text-[10px] text-slate-500 italic text-center w-full">
                    {isRtl ? 'عرض فقط' : 'Read-only'}
                  </span>
                )}
              </div>
            </div>
          );
        })}
      </div>

      {/* Visual Consent Audit Trail / Timeline */}
      <div className="glass-panel p-6 rounded-2xl border-slate-800 space-y-4 mt-8">
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-2">
            <History className="w-4 h-4 text-sky-400" />
            <h3 className="text-sm font-bold text-white">
              {isRtl ? 'سجل أحداث الموافقة غير القابل للتعديل (Audit Trail)' : 'Immutable Consent Lifecycle Audit Trail'}
            </h3>
          </div>
          <span className="text-xs text-slate-400 font-mono">
            {auditEvents.length} {isRtl ? 'أحداث موثقة' : 'Recorded Events'}
          </span>
        </div>

        {auditEvents.length === 0 ? (
          <div className="p-8 text-center text-xs text-slate-500">
            {isRtl ? 'لا توجد أحداث تدقيق مسجلة حتى الآن.' : 'No audit events recorded yet for this patient.'}
          </div>
        ) : (
          <div className="relative pl-6 space-y-4 before:absolute before:left-2 before:top-2 before:bottom-2 before:w-0.5 before:bg-slate-800">
            {auditEvents.map((ev) => {
              const isRevoke = String(ev.eventType).toLowerCase().includes('revoke') || String(ev.eventType) === '1';
              const isHoldApplied = String(ev.eventType).toLowerCase().includes('legalholdapplied') || String(ev.eventType) === '4';
              const isHoldReleased = String(ev.eventType).toLowerCase().includes('legalholdreleased') || String(ev.eventType) === '5';
              const badgeColor = isRevoke
                ? 'border-rose-500 text-rose-400'
                : isHoldApplied
                ? 'border-amber-500 text-amber-400'
                : isHoldReleased
                ? 'border-indigo-500 text-indigo-400'
                : 'border-teal-500 text-teal-400';

              const eventLabel = isHoldApplied
                ? (isRtl ? 'تطبيق حجز قانوني (Legal Hold)' : 'Legal Hold Applied')
                : isHoldReleased
                ? (isRtl ? 'فك الحجز القانوني (Hold Released)' : 'Legal Hold Released')
                : String(ev.eventType);

              return (
                <div key={ev.id} className="relative group">
                  <div className={`absolute -left-6 top-1 w-4 h-4 rounded-full border-2 flex items-center justify-center bg-slate-900 ${
                    badgeColor.split(' ')[0]
                  }`} />
                  <div className="p-3 rounded-xl bg-slate-900/60 border border-slate-800 flex flex-col sm:flex-row sm:items-center justify-between gap-2">
                    <div>
                      <div className="flex items-center gap-2">
                        <span className={`text-xs font-bold ${badgeColor.split(' ')[1]}`}>
                          {eventLabel}
                        </span>
                        <span className="text-xs text-slate-300 font-medium">
                          ({String(ev.consentType)})
                        </span>
                        {ev.performedByName && (
                          <span className="text-[11px] text-slate-400 flex items-center gap-1">
                            <UserCheck className="w-3 h-3 text-sky-400" />
                            {ev.performedByName}
                          </span>
                        )}
                      </div>
                      {ev.reason && (
                        <div className="text-xs text-slate-400 mt-1">
                          <b className="text-slate-300">{isRtl ? 'السبب:' : 'Reason:'}</b> {ev.reason}
                        </div>
                      )}
                    </div>
                    <div className="text-[11px] text-slate-500 font-mono whitespace-nowrap">
                      {new Date(ev.timestamp).toLocaleString()}
                    </div>
                  </div>
                </div>
              );
            })}
          </div>
        )}
      </div>

      {/* Grant Modal */}
      {isGrantOpen && (
        <div className="fixed inset-0 z-50 bg-black/70 backdrop-blur-sm flex items-center justify-center p-4">
          <div className="glass-panel w-full max-w-md p-6 rounded-2xl border-slate-700 bg-slate-900 space-y-4">
            <h3 className="text-base font-bold text-white flex items-center gap-2">
              <FileSignature className="w-5 h-5 text-teal-400" />
              <span>{t.grantConsent}</span>
            </h3>

            <form onSubmit={handleGrantSubmit} className="space-y-4 text-xs">
              <div>
                <label className="block text-slate-300 font-semibold mb-1">
                  {isRtl ? 'نوع الموافقة المطلوب تسجيلها' : 'Consent Scope'}
                </label>
                <select
                  value={selectedTypeForGrant}
                  onChange={(e) => setSelectedTypeForGrant(Number(e.target.value))}
                  className="w-full p-2.5 rounded-xl bg-slate-800 border border-slate-700 text-slate-200 focus:outline-none focus:border-teal-500"
                >
                  {CONSENT_TYPES.map((ct) => (
                    <option key={ct.id} value={ct.id}>
                      {isRtl ? ct.arTitle : ct.enTitle} ({ct.key})
                    </option>
                  ))}
                </select>
              </div>

              <div>
                <label className="block text-slate-300 font-semibold mb-1">
                  {isRtl ? 'فترة الصلاحية' : 'Validity Duration'}
                </label>
                <select
                  value={grantDurationDays}
                  onChange={(e) => setGrantDurationDays(Number(e.target.value))}
                  className="w-full p-2.5 rounded-xl bg-slate-800 border border-slate-700 text-slate-200 focus:outline-none focus:border-teal-500"
                >
                  <option value={365}>{isRtl ? 'سنة واحدة (موصى بها للمرضى)' : '1 Year (Standard Clinical)'}</option>
                  <option value={180}>{isRtl ? '6 أشهر' : '6 Months'}</option>
                  <option value={30}>{isRtl ? '30 يومًا (مؤقت)' : '30 Days (Temporary / Trial)'}</option>
                  <option value={0}>{isRtl ? 'دائمة حتى الإلغاء' : 'Indefinite / Until Revoked'}</option>
                </select>
              </div>

              <div>
                <label className="block text-slate-300 font-semibold mb-1">
                  {isRtl ? 'ملاحظات وتوثيق سريري (اختياري)' : 'Clinical Notes / Witness Remarks (Optional)'}
                </label>
                <textarea
                  value={grantNotes}
                  onChange={(e) => setGrantNotes(e.target.value)}
                  rows={3}
                  placeholder={isRtl ? 'تم توضيح شروط الرعاية والموافقة للمريض شفهيًا وكتابيًا...' : 'Patient reviewed and understood the treatment protocol...'}
                  className="w-full p-2.5 rounded-xl bg-slate-800 border border-slate-700 text-slate-200 focus:outline-none focus:border-teal-500 placeholder:text-slate-500"
                />
              </div>

              <div className="flex items-center justify-end gap-2 pt-3 border-t border-slate-800">
                <button
                  type="button"
                  onClick={() => setIsGrantOpen(false)}
                  className="px-4 py-2 rounded-xl bg-slate-800 hover:bg-slate-700 text-slate-300 font-semibold"
                >
                  {t.cancel}
                </button>
                <button
                  type="submit"
                  disabled={actionLoading}
                  className="px-4 py-2 rounded-xl bg-teal-600 hover:bg-teal-500 text-white font-semibold flex items-center gap-1.5 disabled:opacity-50"
                >
                  {actionLoading ? <RotateCcw className="w-4 h-4 animate-spin" /> : <CheckCircle2 className="w-4 h-4" />}
                  <span>{isRtl ? 'تأكيد وحفظ الموافقة' : 'Sign & Authorize'}</span>
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* Revoke Modal */}
      {isRevokeOpen && (
        <div className="fixed inset-0 z-50 bg-black/70 backdrop-blur-sm flex items-center justify-center p-4">
          <div className="glass-panel w-full max-w-md p-6 rounded-2xl border-rose-500/40 bg-slate-900 space-y-4">
            <h3 className="text-base font-bold text-rose-400 flex items-center gap-2">
              <ShieldAlert className="w-5 h-5 text-rose-400" />
              <span>{t.revokeConsent}</span>
            </h3>

            <p className="text-xs text-slate-300 leading-relaxed">
              {isRtl
                ? 'تحذير أمني: إلغاء الموافقة سيوقف فورًا كافة خدمات الذكاء الاصطناعي ومعالجة البيانات السريرية المرتبطة بهذا المريض، وسيتم توثيق الإلغاء في سجل الامتثال الدائم.'
                : 'Warning: Revoking consent will immediately disable clinical AI diagnostic execution for this patient and create an immutable compliance audit record.'}
            </p>

            <form onSubmit={handleRevokeSubmit} className="space-y-4 text-xs">
              <div>
                <label className="block text-slate-300 font-semibold mb-1">
                  {t.revocationReason} <span className="text-rose-400">*</span>
                </label>
                <textarea
                  value={revocationReason}
                  onChange={(e) => setRevocationReason(e.target.value)}
                  rows={3}
                  required
                  placeholder={isRtl ? 'طلب المريض سحب الموافقة رسميًا...' : 'Patient opted out / revoked consent upon request...'}
                  className="w-full p-2.5 rounded-xl bg-slate-800 border border-slate-700 text-slate-200 focus:outline-none focus:border-rose-500 placeholder:text-slate-500"
                />
              </div>

              <div className="flex items-center justify-end gap-2 pt-3 border-t border-slate-800">
                <button
                  type="button"
                  onClick={() => {
                    setIsRevokeOpen(false);
                    setSelectedConsentForRevoke(null);
                  }}
                  className="px-4 py-2 rounded-xl bg-slate-800 hover:bg-slate-700 text-slate-300 font-semibold"
                >
                  {t.cancel}
                </button>
                <button
                  type="submit"
                  disabled={actionLoading || !revocationReason.trim()}
                  className="px-4 py-2 rounded-xl bg-rose-600 hover:bg-rose-500 text-white font-semibold flex items-center gap-1.5 disabled:opacity-50"
                >
                  {actionLoading ? <RotateCcw className="w-4 h-4 animate-spin" /> : <ShieldX className="w-4 h-4" />}
                  <span>{isRtl ? 'تنفيذ الإلغاء فورًا' : 'Confirm Immediate Revocation'}</span>
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}
