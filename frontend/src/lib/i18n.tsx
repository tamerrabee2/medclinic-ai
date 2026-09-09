'use client';

import React, { createContext, useContext, useState, useEffect } from 'react';

type Language = 'ar' | 'en';

interface I18nContextType {
  lang: Language;
  dir: 'rtl' | 'ltr';
  setLang: (lang: Language) => void;
  t: (key: string) => string;
}

const translations: Record<Language, Record<string, string>> = {
  en: {
    // Nav
    'nav.overview': 'Overview',
    'nav.patients': 'Patients & EMR',
    'nav.appointments': 'Appointments',
    'nav.canvas': 'Medical Canvas',
    'nav.dental': 'Dental Chart',
    'nav.ai': 'AI Clinical Copilot',
    'nav.lab': 'AI Lab Analyzer',
    'nav.radiology': 'Radiology & Imaging',
    'nav.prescriptions': 'Prescriptions',
    'nav.billing': 'Billing & Invoices',
    // Actions
    'action.search': 'Search (Ctrl + K)',
    'action.newAppointment': 'Book Appointment',
    'action.newPatient': 'New Patient',
    'action.save': 'Save Changes',
    'action.cancel': 'Cancel',
    'action.review': 'Doctor Review',
    'action.approve': 'Approve & Sign',
    // Clinical
    'clinic.connected': 'API v1 Online',
    'clinic.aiReady': 'AI Gateway Ready',
  },
  ar: {
    // Nav
    'nav.overview': 'نظرة عامة',
    'nav.patients': 'سجل المرضى والملف الطبي',
    'nav.appointments': 'جدول المواعيد',
    'nav.canvas': 'الكانفاس وخريطة الجسم',
    'nav.dental': 'مخطط الأسنان FDI',
    'nav.ai': 'مساعد الطبيب الذكي',
    'nav.lab': 'محلل التحاليل الذكي',
    'nav.radiology': 'الأشعة والتصوير الطبي',
    'nav.prescriptions': 'الوصفات الطبية',
    'nav.billing': 'الفواتير والمالية',
    // Actions
    'action.search': 'بحث شامل (Ctrl + K)',
    'action.newAppointment': 'حجز موعد',
    'action.newPatient': 'مريض جديد',
    'action.save': 'حفظ التعديلات',
    'action.cancel': 'إلغاء',
    'action.review': 'مراجعة الطبيب',
    'action.approve': 'اعتماد وتوقيع',
    // Clinical
    'clinic.connected': 'النظام متصل',
    'clinic.aiReady': 'بوابة الذكاء الاصطناعي جاهزة',
  },
};

const I18nContext = createContext<I18nContextType | undefined>(undefined);

export const I18nProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [lang, setLangState] = useState<Language>('ar'); // Default to Arabic per Arabic prompt

  useEffect(() => {
    const saved = localStorage.getItem('medclinic_lang') as Language;
    if (saved === 'ar' || saved === 'en') {
      setLangState(saved);
      document.documentElement.dir = saved === 'ar' ? 'rtl' : 'ltr';
      document.documentElement.lang = saved;
    } else {
      document.documentElement.dir = 'rtl';
      document.documentElement.lang = 'ar';
    }
  }, []);

  const setLang = (newLang: Language) => {
    setLangState(newLang);
    localStorage.setItem('medclinic_lang', newLang);
    document.documentElement.dir = newLang === 'ar' ? 'rtl' : 'ltr';
    document.documentElement.lang = newLang;
  };

  const t = (key: string): string => {
    return translations[lang][key] || key;
  };

  return (
    <I18nContext.Provider
      value={{
        lang,
        dir: lang === 'ar' ? 'rtl' : 'ltr',
        setLang,
        t,
      }}
    >
      {children}
    </I18nContext.Provider>
  );
};

export const useI18n = () => {
  const context = useContext(I18nContext);
  if (!context) {
    throw new Error('useI18n must be used within an I18nProvider');
  }
  return context;
};
