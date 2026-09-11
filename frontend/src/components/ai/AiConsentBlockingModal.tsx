'use client';

import React, { useState } from 'react';
import { ShieldAlert, Sparkles, CheckCircle2, RotateCcw, X } from 'lucide-react';
import { ApiClient } from '@/lib/api';
import { useLanguage } from '@/lib/i18n/LanguageContext';
import { useAuth } from '@/lib/auth';

interface AiConsentBlockingModalProps {
  isOpen: boolean;
  onClose: () => void;
  patientId: string;
  patientName?: string;
  onConsentGranted?: () => void;
}

export function AiConsentBlockingModal({
  isOpen,
  onClose,
  patientId,
  patientName,
  onConsentGranted
}: AiConsentBlockingModalProps) {
  const { t, language } = useLanguage();
  const { user, hasPermission } = useAuth();
  const isRtl = language === 'ar';

  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  if (!isOpen) return null;

  const canGrant = hasPermission('PatientConsents.Manage');

  const handleGrantNow = async () => {
    try {
      setLoading(true);
      setError(null);
      await ApiClient.recordConsent(patientId, {
        consentType: 1, // AiAssistedCare
        isGranted: true,
        expiresAt: new Date(Date.now() + 365 * 86400000).toISOString(),
        notes: 'Clinical AI assistance consent granted via clinical CDS prompt.'
      });

      if (onConsentGranted) {
        onConsentGranted();
      }
      onClose();
    } catch (err: any) {
      setError(err.message || 'Failed to grant AI consent');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="fixed inset-0 z-50 bg-black/75 backdrop-blur-sm flex items-center justify-center p-4">
      <div className="glass-panel w-full max-w-md p-6 rounded-2xl border-amber-500/40 bg-slate-900 shadow-2xl space-y-4 relative">
        <button
          onClick={onClose}
          className="absolute top-4 right-4 text-slate-400 hover:text-white p-1 rounded-lg transition"
        >
          <X className="w-4 h-4" />
        </button>

        <div className="w-12 h-12 rounded-2xl bg-amber-500/10 border border-amber-500/30 flex items-center justify-center text-amber-400">
          <ShieldAlert className="w-6 h-6" />
        </div>

        <div>
          <h3 className="text-base font-bold text-white flex items-center gap-2">
            <span>{t.aiConsentRequired}</span>
            <span className="px-2 py-0.5 rounded text-[10px] font-bold bg-amber-500/20 text-amber-300 border border-amber-500/30">
              HIPAA & Clinical Guardrail
            </span>
          </h3>
          <p className="text-xs text-slate-300 mt-2 leading-relaxed">
            {isRtl
              ? `لا يمكن معالجة التشخيص السريري بالذكاء الاصطناعي للمريض (${patientName || 'المحدد'}) لعدم وجود موافقة سارية وموقعة من نوع (الرعاية والتشخيص بالذكاء الاصطناعي).`
              : `Clinical AI diagnostics, CDS biomarkers, and machine learning inference cannot proceed for patient (${patientName || 'selected'}) without active patient authorization.`}
          </p>
        </div>

        {error && (
          <div className="p-3 rounded-xl bg-rose-500/10 border border-rose-500/30 text-xs text-rose-300">
            {error}
          </div>
        )}

        <div className="p-3 rounded-xl bg-slate-800/80 border border-slate-700/80 text-[11px] text-slate-400 space-y-1">
          <div className="flex justify-between">
            <span>{isRtl ? 'حالة الحظر:' : 'Enforcement Guard:'}</span>
            <span className="text-amber-400 font-bold font-mono">HTTP 403 (consent_required)</span>
          </div>
          <div className="flex justify-between">
            <span>{isRtl ? 'الموافقة المطلوبة:' : 'Missing Consent:'}</span>
            <span className="text-slate-200 font-mono">AiAssistedCare (Code: 1)</span>
          </div>
        </div>

        <div className="flex items-center justify-end gap-2 pt-3 border-t border-slate-800">
          <button
            type="button"
            onClick={onClose}
            className="px-4 py-2 rounded-xl bg-slate-800 hover:bg-slate-700 text-slate-300 text-xs font-semibold"
          >
            {t.close}
          </button>

          {canGrant ? (
            <button
              type="button"
              disabled={loading}
              onClick={handleGrantNow}
              className="px-4 py-2 rounded-xl bg-gradient-to-r from-amber-600 to-teal-600 hover:from-amber-500 hover:to-teal-500 text-white text-xs font-semibold flex items-center gap-1.5 shadow-lg shadow-amber-600/20 disabled:opacity-50"
            >
              {loading ? <RotateCcw className="w-4 h-4 animate-spin" /> : <Sparkles className="w-4 h-4" />}
              <span>{isRtl ? 'تسجيل موافقة AI وتفعيل الآن' : 'Authorize AI Consent Now'}</span>
            </button>
          ) : (
            <span className="text-[11px] text-slate-500 italic">
              {isRtl ? 'يتطلب صلاحية طبيب لاعتماد الموافقة' : 'Requires Physician privileges'}
            </span>
          )}
        </div>
      </div>
    </div>
  );
}
