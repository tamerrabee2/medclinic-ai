'use client';

import React, { createContext, useContext, useEffect, useState } from 'react';
import { Language, TranslationDictionary, translations } from './translations';

interface LanguageContextType {
  language: Language;
  dir: 'ltr' | 'rtl';
  t: TranslationDictionary;
  setLanguage: (lang: Language) => void;
  toggleLanguage: () => void;
}

const LanguageContext = createContext<LanguageContextType | undefined>(undefined);

export function LanguageProvider({ children }: { children: React.ReactNode }) {
  const [language, setLanguageState] = useState<Language>('en');

  useEffect(() => {
    const saved = localStorage.getItem('medclinic_lang') as Language;
    if (saved === 'ar' || saved === 'en') {
      setLanguageState(saved);
      document.documentElement.lang = saved;
      document.documentElement.dir = saved === 'ar' ? 'rtl' : 'ltr';
    }
  }, []);

  const setLanguage = (lang: Language) => {
    setLanguageState(lang);
    localStorage.setItem('medclinic_lang', lang);
    document.documentElement.lang = lang;
    document.documentElement.dir = lang === 'ar' ? 'rtl' : 'ltr';
  };

  const toggleLanguage = () => {
    setLanguage(language === 'en' ? 'ar' : 'en');
  };

  const dir = language === 'ar' ? 'rtl' : 'ltr';
  const t = translations[language];

  return (
    <LanguageContext.Provider value={{ language, dir, t, setLanguage, toggleLanguage }}>
      <div dir={dir} className={language === 'ar' ? 'font-arabic' : ''}>
        {children}
      </div>
    </LanguageContext.Provider>
  );
}

export function useLanguage() {
  const context = useContext(LanguageContext);
  if (!context) {
    throw new Error('useLanguage must be used within a LanguageProvider');
  }
  return context;
}
