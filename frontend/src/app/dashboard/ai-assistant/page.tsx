'use client';

import React, { useState, useRef, useEffect } from 'react';
import { ApiClient } from '@/lib/api';
import {
  Sparkles,
  Send,
  Loader2,
  Stethoscope,
  Bot,
  User,
  ShieldCheck,
  RefreshCw,
  Search,
  CheckCircle2,
  X,
  Lock,
  ChevronDown,
  AlertTriangle
} from 'lucide-react';
import { AiConsentBlockingModal } from '@/components/ai/AiConsentBlockingModal';

interface Message {
  id: string;
  role: 'user' | 'assistant' | 'system';
  content: string;
  createdAt: string;
}

interface PatientOption {
  id: string;
  fullName: string;
  fileNumber: string;
}

export default function AIAssistantPage() {
  const [messages, setMessages] = useState<Message[]>([
    {
      id: 'welcome',
      role: 'assistant',
      content:
        "Hello Dr. Sarah. I am your Clinical AI Copilot. I can assist you in evaluating lab abnormalities, reviewing radiology reports, verifying drug interactions, or formatting medical summaries.\n\n*Clinical Note: All AI outputs are for decision support only and require your professional physician judgment.*",
      createdAt: '2026-09-04T09:00:00.000Z',
    },
  ]);
  const [input, setInput] = useState('');
  const [loading, setLoading] = useState(false);
  const [conversationId, setConversationId] = useState<string | undefined>(undefined);

  // Dynamic Patient Context & Consent Gate State
  const [patients, setPatients] = useState<PatientOption[]>([]);
  const [selectedPatientId, setSelectedPatientId] = useState<string>('');
  const [selectedPatientName, setSelectedPatientName] = useState<string>('');
  const [selectedPatientFileNumber, setSelectedPatientFileNumber] = useState<string>('');
  const [isPatientDropdownOpen, setIsPatientDropdownOpen] = useState(false);
  const [patientSearchQuery, setPatientSearchQuery] = useState('');
  const [loadingPatients, setLoadingPatients] = useState(false);
  const [isConsentModalOpen, setIsConsentModalOpen] = useState(false);

  // Strict Medical AI Safety Error State
  const [aiError, setAiError] = useState<{
    message: string;
    detail?: string;
    failedPrompt?: string;
  } | null>(null);

  const messagesEndRef = useRef<HTMLDivElement>(null);
  const patientDropdownRef = useRef<HTMLDivElement>(null);
  const searchRequestIdRef = useRef<number>(0);
  const searchAbortControllerRef = useRef<AbortController | null>(null);
  const isInitialMountRef = useRef<boolean>(true);

  // Clear any legacy client-side AI keys to guarantee client privacy and security
  useEffect(() => {
    if (typeof window !== 'undefined') {
      localStorage.removeItem('medclinic_ai_key');
      localStorage.removeItem('medclinic_ai_provider');
      localStorage.removeItem('medclinic_ai_model');
    }
  }, []);

  // Fetch real patient list from API with AbortSignal and stale-response protection
  const fetchPatients = async (searchTerm = '', requestId?: number, signal?: AbortSignal) => {
    setLoadingPatients(true);
    try {
      const res: any = await ApiClient.getPatients(searchTerm, 1, 20, signal);
      if (requestId !== undefined && requestId !== searchRequestIdRef.current) {
        return; // Discard stale response
      }
      const list = res?.data?.items || res?.items || [];
      const mapped = list.map((p: any) => ({
        id: p.id,
        fullName: p.fullName || `${p.firstName || ''} ${p.lastName || ''}`.trim() || 'Unnamed Patient',
        fileNumber: p.fileNumber || p.mrn || p.nationalId || 'No MRN'
      }));
      setPatients(mapped);
    } catch (err: any) {
      if (err?.name === 'AbortError') {
        return; // Request was aborted by newer query, do not overwrite or clear state
      }
      setPatients([]);
    } finally {
      if (!signal?.aborted) {
        setLoadingPatients(false);
      }
    }
  };

  // Initial fetch on mount (single decoupled load)
  useEffect(() => {
    const controller = new AbortController();
    fetchPatients('', undefined, controller.signal);
    return () => {
      controller.abort();
    };
  }, []);

  // Debounced patient search (300ms) with >= 2 chars check and AbortController cancellation
  useEffect(() => {
    // Skip on initial mount to avoid duplicate fetch on page load
    if (isInitialMountRef.current) {
      isInitialMountRef.current = false;
      return;
    }

    const trimmed = patientSearchQuery.trim();

    if (trimmed.length === 1) {
      return; // Do not query on single character
    }

    const timer = setTimeout(() => {
      // Abort previous in-flight search request
      if (searchAbortControllerRef.current) {
        searchAbortControllerRef.current.abort();
      }
      const controller = new AbortController();
      searchAbortControllerRef.current = controller;
      const currentId = ++searchRequestIdRef.current;

      fetchPatients(trimmed, currentId, controller.signal);
    }, 300);

    return () => {
      clearTimeout(timer);
    };
  }, [patientSearchQuery]);

  // Close patient dropdown on outside click
  useEffect(() => {
    const handleClickOutside = (e: MouseEvent) => {
      if (patientDropdownRef.current && !patientDropdownRef.current.contains(e.target as Node)) {
        setIsPatientDropdownOpen(false);
      }
    };
    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, []);

  const scrollToBottom = () => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  };

  useEffect(() => {
    scrollToBottom();
  }, [messages, loading]);

  const handleSelectPatient = (patient: PatientOption | null) => {
    if (!patient) {
      setSelectedPatientId('');
      setSelectedPatientName('');
      setSelectedPatientFileNumber('');
    } else {
      setSelectedPatientId(patient.id);
      setSelectedPatientName(patient.fullName);
      setSelectedPatientFileNumber(patient.fileNumber);
    }
    setIsPatientDropdownOpen(false);
  };

  const handleSend = async (textToSend?: string) => {
    const text = textToSend || input;
    if (!text.trim() || loading) return;

    setAiError(null);

    const userMsg: Message = {
      id: Math.random().toString(),
      role: 'user',
      content: text,
      createdAt: new Date().toISOString(),
    };

    setMessages((prev) => [...prev, userMsg]);
    setInput('');
    setLoading(true);

    // Consent Anti-Bypass Guardrail: verify consent if patient context is attached
    if (selectedPatientId) {
      try {
        const consentStatus = await ApiClient.getActiveConsent(selectedPatientId, 1);
        if (!consentStatus?.hasActiveConsent) {
          setIsConsentModalOpen(true);
          setLoading(false);
          return;
        }
      } catch (err: any) {
        if (err.message?.includes('403') || err.message?.includes('consent_required')) {
          setIsConsentModalOpen(true);
          setLoading(false);
          return;
        }
      }
    }

    try {
      // Route all AI calls strictly through ASP.NET backend gateway
      const res = await ApiClient.sendAIMessage(text, conversationId, selectedPatientId || undefined);
      if (res && res.messages) {
        setConversationId(res.id);
        const lastMsg = res.messages[res.messages.length - 1];
        setMessages((prev) => [
          ...prev,
          {
            id: lastMsg.id || Math.random().toString(),
            role: 'assistant',
            content: lastMsg.content,
            createdAt: lastMsg.createdAt || new Date().toISOString(),
          },
        ]);
      } else {
        throw new Error('No response returned from AI Gateway');
      }
    } catch (err: any) {
      if (err.message?.includes('consent_required') || err.message?.includes('403')) {
        setIsConsentModalOpen(true);
        setLoading(false);
        return;
      }

      // STRICT SAFETY COMPLIANCE: Never fake or simulate clinical recommendations when AI is unavailable
      setAiError({
        message: 'Clinical AI service is temporarily unavailable. No clinical AI assessment was generated.',
        detail: 'Please retry or proceed using standard independent clinical workflow.',
        failedPrompt: text,
      });
    } finally {
      setLoading(false);
    }
  };

  const quickPrompts = [
    'Evaluate HbA1c 8.4% and elevated fasting glucose',
    'Check penicillin allergy cross-reactivity with Cephalosporins',
    'Summarize imaging report for CT Chest with contrast',
    'Draft clinical notes for follow-up hypertension review',
  ];

  return (
    <div className="flex flex-col h-[calc(100vh-6rem)] max-w-6xl mx-auto space-y-4">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 glass-panel p-4 rounded-2xl border-slate-800 shrink-0">
        <div>
          <div className="flex items-center gap-2">
            <h1 className="text-xl font-bold text-white flex items-center gap-2">
              <Sparkles className="w-5 h-5 text-sky-400" />
              Clinical AI Doctor Copilot
            </h1>
            <span className="px-2 py-0.5 rounded-full text-[10px] font-mono font-bold bg-sky-500/20 text-sky-300 border border-sky-500/30">
              Module 16 & 17 Gateway
            </span>
          </div>
          <p className="text-xs text-slate-400 mt-0.5">
            Real-time medical decision support, diagnostic correlations, and pharmacological verification.
          </p>
        </div>

        {/* Dynamic Patient Context & Server Gateway Status */}
        <div className="flex flex-wrap items-center gap-2">
          {/* Real Patient Search & Select */}
          <div className="relative" ref={patientDropdownRef}>
            <button
              type="button"
              onClick={() => setIsPatientDropdownOpen(!isPatientDropdownOpen)}
              className="flex items-center gap-2 bg-slate-900 border border-slate-700 hover:border-sky-500/50 rounded-xl px-3 py-2 text-xs transition"
            >
              <User className="w-3.5 h-3.5 text-sky-400 shrink-0" />
              <div className="text-left">
                {selectedPatientId ? (
                  <span className="text-white font-medium">
                    {selectedPatientName} <span className="text-slate-400">({selectedPatientFileNumber})</span>
                  </span>
                ) : (
                  <span className="text-slate-400">Attach Patient Context...</span>
                )}
              </div>
              <ChevronDown className="w-3.5 h-3.5 text-slate-400 ml-1" />
            </button>

            {/* Dropdown Menu */}
            {isPatientDropdownOpen && (
              <div className="absolute right-0 mt-2 w-80 bg-slate-950 border border-slate-800 rounded-2xl shadow-2xl z-50 p-2 space-y-2">
                <div className="relative">
                  <Search className="w-3.5 h-3.5 text-slate-400 absolute left-2.5 top-2.5" />
                  <input
                    type="text"
                    value={patientSearchQuery}
                    onChange={(e) => setPatientSearchQuery(e.target.value)}
                    placeholder="Search patient by name or MRN (min 2 chars)..."
                    className="w-full pl-8 pr-3 py-1.5 bg-slate-900 border border-slate-800 rounded-xl text-xs text-white placeholder-slate-500 focus:outline-none focus:border-sky-500"
                    autoFocus
                  />
                </div>

                {patientSearchQuery.trim().length === 1 && (
                  <div className="px-3 py-1.5 text-[11px] text-amber-400/80 bg-amber-500/10 rounded-lg">
                    Type at least 2 characters to search...
                  </div>
                )}

                <div className="max-h-56 overflow-y-auto space-y-1">
                  <button
                    type="button"
                    onClick={() => handleSelectPatient(null)}
                    className={`w-full text-left px-3 py-2 rounded-xl text-xs flex items-center justify-between transition ${
                      !selectedPatientId ? 'bg-sky-500/20 text-sky-300 font-semibold' : 'text-slate-400 hover:bg-slate-900'
                    }`}
                  >
                    <span>No Patient Context (General Clinical Guidance)</span>
                    {!selectedPatientId && <CheckCircle2 className="w-3.5 h-3.5 text-sky-400" />}
                  </button>

                  {loadingPatients ? (
                    <div className="p-4 text-center text-xs text-slate-500 flex items-center justify-center gap-2">
                      <Loader2 className="w-3.5 h-3.5 animate-spin text-sky-400" />
                      <span>Searching patients...</span>
                    </div>
                  ) : patients.length === 0 ? (
                    <div className="p-4 text-center text-xs text-slate-500">
                      No matching patients found.
                    </div>
                  ) : (
                    patients.map((p) => (
                      <button
                        key={p.id}
                        type="button"
                        onClick={() => handleSelectPatient(p)}
                        className={`w-full text-left px-3 py-2 rounded-xl text-xs flex items-center justify-between transition ${
                          selectedPatientId === p.id
                            ? 'bg-sky-500/20 text-sky-300 font-semibold'
                            : 'text-slate-200 hover:bg-slate-900'
                        }`}
                      >
                        <div>
                          <div className="font-medium text-white">{p.fullName}</div>
                          <div className="text-[10px] text-slate-400 font-mono">File: {p.fileNumber}</div>
                        </div>
                        {selectedPatientId === p.id && <CheckCircle2 className="w-3.5 h-3.5 text-sky-400" />}
                      </button>
                    ))
                  )}
                </div>
              </div>
            )}
          </div>

          {/* Secure Backend AI Gateway Badge */}
          <div className="px-3 py-2 rounded-xl bg-slate-900 border border-slate-700 text-xs font-semibold text-emerald-400 flex items-center gap-2">
            <Lock className="w-3.5 h-3.5 text-emerald-400" />
            <span>Secure Server Gateway</span>
            <span className="w-1.5 h-1.5 rounded-full bg-emerald-400 animate-pulse"></span>
          </div>

          {/* Reset Session */}
          <button
            onClick={() => {
              setAiError(null);
              setMessages([
                {
                  id: 'welcome',
                  role: 'assistant',
                  content:
                    "Chat history reset. How can I assist you in your clinical evaluation today?",
                  createdAt: new Date().toISOString(),
                },
              ]);
            }}
            className="p-2 rounded-xl bg-slate-900 border border-slate-800 text-slate-400 hover:text-white hover:bg-slate-800 transition"
            title="Reset Session"
          >
            <RefreshCw className="w-4 h-4" />
          </button>
        </div>
      </div>

      {/* Safety Banner */}
      <div className="flex items-center justify-between p-3 rounded-xl bg-sky-950/40 border border-sky-500/20 text-xs text-sky-300 shrink-0">
        <div className="flex items-center gap-2">
          <ShieldCheck className="w-4 h-4 text-sky-400 shrink-0" />
          <span>
            <b>Medical Safety Mode:</b> AI outputs are assistive and require attending physician sign-off. Patient PII is automatically de-identified.
          </span>
        </div>
        <span className="text-[10px] font-mono text-slate-400 hidden sm:inline">
          Backend Gateway: Active
        </span>
      </div>

      {/* Messages Feed */}
      <div className="flex-1 overflow-y-auto space-y-4 pr-2">
        {messages.map((m) => (
          <div
            key={m.id}
            className={`flex gap-3 max-w-3xl ${
              m.role === 'user' ? 'ml-auto flex-row-reverse' : 'mr-auto'
            }`}
          >
            <div
              className={`w-8 h-8 rounded-xl flex items-center justify-center shrink-0 text-white font-bold text-xs ${
                m.role === 'user'
                  ? 'bg-gradient-to-tr from-sky-500 to-indigo-600'
                  : 'bg-slate-800 border border-slate-700 text-sky-400'
              }`}
            >
              {m.role === 'user' ? <User className="w-4 h-4" /> : <Bot className="w-4 h-4" />}
            </div>

            <div
              className={`p-4 rounded-2xl text-sm leading-relaxed ${
                m.role === 'user'
                  ? 'bg-gradient-to-br from-sky-600 to-indigo-600 text-white shadow-md'
                  : 'bg-slate-900/90 border border-slate-800 text-slate-200'
              }`}
            >
              <div className="whitespace-pre-wrap">{m.content}</div>
              <div
                suppressHydrationWarning
                className={`text-[10px] mt-2 font-mono ${
                  m.role === 'user' ? 'text-sky-200 text-right' : 'text-slate-400'
                }`}
              >
                {new Date(m.createdAt).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
              </div>
            </div>
          </div>
        ))}

        {loading && (
          <div className="flex gap-3 max-w-xl mr-auto">
            <div className="w-8 h-8 rounded-xl bg-sky-500/20 text-sky-300 border border-sky-500/30 flex items-center justify-center shrink-0">
              <Bot className="w-4 h-4" />
            </div>
            <div className="p-4 rounded-2xl bg-slate-900 border border-slate-800 flex items-center gap-2 text-sky-400 text-xs font-medium">
              <Loader2 className="w-4 h-4 animate-spin" />
              <span>Querying Clinical AI Assistant for decision support...</span>
            </div>
          </div>
        )}

        <div ref={messagesEndRef} />
      </div>

      {/* Safe AI Outage / Failure Banner */}
      {aiError && (
        <div className="p-3.5 rounded-2xl bg-amber-500/10 border border-amber-500/30 text-amber-200 text-xs flex flex-col sm:flex-row items-start sm:items-center justify-between gap-3 shrink-0 shadow-lg">
          <div className="flex items-center gap-2.5">
            <AlertTriangle className="w-4 h-4 text-amber-400 shrink-0" />
            <div>
              <div className="font-bold text-amber-300">{aiError.message}</div>
              <div className="text-[11px] text-amber-400/80">{aiError.detail}</div>
            </div>
          </div>
          <div className="flex items-center gap-2 self-end sm:self-center">
            {aiError.failedPrompt && (
              <button
                type="button"
                onClick={() => {
                  const p = aiError.failedPrompt;
                  setAiError(null);
                  if (p) handleSend(p);
                }}
                className="px-3 py-1.5 rounded-xl bg-amber-500/20 hover:bg-amber-500/30 border border-amber-500/40 text-amber-300 font-semibold text-xs transition flex items-center gap-1.5"
              >
                <RefreshCw className="w-3.5 h-3.5" />
                <span>Retry Request</span>
              </button>
            )}
            <button
              type="button"
              onClick={() => setAiError(null)}
              className="p-1.5 rounded-lg text-amber-400 hover:text-white hover:bg-amber-500/20 transition"
              title="Dismiss"
            >
              <X className="w-4 h-4" />
            </button>
          </div>
        </div>
      )}

      {/* Suggested Fast Prompts */}
      <div className="flex items-center gap-2 overflow-x-auto py-1 shrink-0">
        {quickPrompts.map((q, idx) => (
          <button
            key={idx}
            onClick={() => handleSend(q)}
            className="px-3 py-1.5 rounded-full glass-panel hover:border-sky-500/40 text-slate-300 hover:text-sky-300 text-xs font-medium whitespace-nowrap transition shrink-0"
          >
            {q}
          </button>
        ))}
      </div>

      {/* Message Input Box */}
      <form
        onSubmit={(e) => {
          e.preventDefault();
          handleSend();
        }}
        className="flex gap-2 shrink-0"
      >
        <input
          type="text"
          value={input}
          onChange={(e) => setInput(e.target.value)}
          placeholder="Ask AI Copilot about symptoms, lab results, differential diagnosis, or guidelines..."
          className="flex-1 px-4 py-3 bg-slate-900/90 border border-slate-800 rounded-xl text-sm text-white placeholder-slate-500 focus:outline-none focus:border-sky-500 transition shadow-inner"
        />
        <button
          type="submit"
          disabled={loading || !input.trim()}
          className="px-5 py-3 rounded-xl bg-gradient-to-r from-sky-500 to-indigo-600 hover:from-sky-400 hover:to-indigo-500 disabled:opacity-50 text-white font-medium text-sm shadow-md shadow-sky-500/20 transition flex items-center justify-center gap-2"
        >
          <Send className="w-4 h-4" />
          <span className="hidden sm:inline">Send</span>
        </button>
      </form>

      {/* AI Consent Blocking Guard Modal */}
      <AiConsentBlockingModal
        isOpen={isConsentModalOpen}
        onClose={() => setIsConsentModalOpen(false)}
        patientId={selectedPatientId}
        patientName={selectedPatientName}
        onConsentGranted={() => {
          setIsConsentModalOpen(false);
        }}
      />
    </div>
  );
}
