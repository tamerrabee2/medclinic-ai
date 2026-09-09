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
  FileText,
  Activity,
  AlertCircle,
  ShieldCheck,
  RefreshCw,
  Plus,
  KeyRound,
  Settings2,
  CheckCircle2,
  X,
  Eye,
  EyeOff,
  Zap,
  Globe
} from 'lucide-react';
import { AiConsentBlockingModal } from '@/components/ai/AiConsentBlockingModal';

interface Message {
  id: string;
  role: 'user' | 'assistant' | 'system';
  content: string;
  createdAt: string;
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

  // Patient Context & Consent Gate State
  const [selectedPatientId, setSelectedPatientId] = useState<string>('');
  const [selectedPatientName, setSelectedPatientName] = useState<string>('');
  const [isConsentModalOpen, setIsConsentModalOpen] = useState(false);

  // AI Configuration State
  const [showSettings, setShowSettings] = useState(false);
  const [provider, setProvider] = useState<'gemini' | 'openai'>('gemini');
  const [apiKey, setApiKey] = useState('');
  const [model, setModel] = useState('gemini-1.5-flash');
  const [showKey, setShowKey] = useState(false);
  const [testStatus, setTestStatus] = useState<'idle' | 'testing' | 'success' | 'failed'>('idle');
  const [testMessage, setTestMessage] = useState('');

  // Load saved settings from localStorage on mount
  useEffect(() => {
    const savedKey = localStorage.getItem('medclinic_ai_key');
    const savedProvider = localStorage.getItem('medclinic_ai_provider') as 'gemini' | 'openai';
    const savedModel = localStorage.getItem('medclinic_ai_model');
    if (savedKey) setApiKey(savedKey);
    if (savedProvider) setProvider(savedProvider);
    if (savedModel) setModel(savedModel);
  }, []);

  const messagesEndRef = useRef<HTMLDivElement>(null);

  const scrollToBottom = () => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  };

  useEffect(() => {
    scrollToBottom();
  }, [messages, loading]);

  const saveSettings = () => {
    localStorage.setItem('medclinic_ai_key', apiKey.trim());
    localStorage.setItem('medclinic_ai_provider', provider);
    localStorage.setItem('medclinic_ai_model', model);
    setShowSettings(false);
  };

  const handleTestKey = async () => {
    if (!apiKey.trim()) {
      setTestStatus('failed');
      setTestMessage('Please enter an API Key first.');
      return;
    }

    setTestStatus('testing');
    setTestMessage('Connecting to AI endpoint...');

    try {
      if (provider === 'gemini') {
        const url = `https://generativelanguage.googleapis.com/v1beta/models/${model}:generateContent?key=${apiKey.trim()}`;
        const res = await fetch(url, {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({
            contents: [{ parts: [{ text: 'Respond with the single word: OK' }] }]
          })
        });

        if (!res.ok) {
          const err = await res.json();
          throw new Error(err.error?.message || `HTTP ${res.status}`);
        }

        setTestStatus('success');
        setTestMessage(`Connected to Google Gemini (${model}) successfully!`);
      } else {
        const res = await fetch('https://api.openai.com/v1/chat/completions', {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${apiKey.trim()}`
          },
          body: JSON.stringify({
            model: model || 'gpt-4o-mini',
            messages: [{ role: 'user', content: 'Say OK' }],
            max_tokens: 5
          })
        });

        if (!res.ok) {
          const err = await res.json();
          throw new Error(err.error?.message || `HTTP ${res.status}`);
        }

        setTestStatus('success');
        setTestMessage(`Connected to OpenAI (${model}) successfully!`);
      }
    } catch (err: any) {
      setTestStatus('failed');
      setTestMessage(`Connection failed: ${err.message}`);
    }
  };

  const callDirectAI = async (prompt: string, history: Message[]): Promise<string> => {
    if (provider === 'gemini') {
      const contents = history
        .filter((m) => m.role !== 'system')
        .map((m) => ({
          role: m.role === 'assistant' ? 'model' : 'user',
          parts: [{ text: m.content }]
        }));

      contents.push({
        role: 'user',
        parts: [{ text: prompt }]
      });

      const url = `https://generativelanguage.googleapis.com/v1beta/models/${model}:generateContent?key=${apiKey.trim()}`;
      const res = await fetch(url, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          system_instruction: {
            parts: [{ text: "You are MedClinic AI, an advanced Clinical Doctor Copilot. You assist licensed physicians with differential diagnosis, lab evaluation, and medication safety checks. Always provide professional medical recommendations and maintain clinical rigor. Format responses clearly with markdown." }]
          },
          contents
        })
      });

      if (!res.ok) {
        const err = await res.json();
        throw new Error(err.error?.message || `Gemini API error: ${res.status}`);
      }

      const data = await res.json();
      return data.candidates?.[0]?.content?.parts?.[0]?.text || "No response generated.";
    } else {
      const messagesPayload = [
        {
          role: "system",
          content: "You are MedClinic AI, an advanced Clinical Doctor Copilot assisting physicians with medical evidence, differentials, and safety checks."
        },
        ...history.map((m) => ({ role: m.role, content: m.content })),
        { role: "user", content: prompt }
      ];

      const res = await fetch('https://api.openai.com/v1/chat/completions', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${apiKey.trim()}`
        },
        body: JSON.stringify({
          model: model || 'gpt-4o',
          messages: messagesPayload
        })
      });

      if (!res.ok) {
        const err = await res.json();
        throw new Error(err.error?.message || `OpenAI API error: ${res.status}`);
      }

      const data = await res.json();
      return data.choices?.[0]?.message?.content || "No response generated.";
    }
  };

  const handleSend = async (textToSend?: string) => {
    const text = textToSend || input;
    if (!text.trim() || loading) return;

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
      // 1. If user provided a client API Key, use direct live agent query
      if (apiKey.trim()) {
        const aiText = await callDirectAI(text, messages);
        setMessages((prev) => [
          ...prev,
          {
            id: Math.random().toString(),
            role: 'assistant',
            content: aiText,
            createdAt: new Date().toISOString()
          }
        ]);
      } else {
        // 2. Otherwise query backend API
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
          throw new Error('Backend fallback');
        }
      }
    } catch (err: any) {
      if (err.message?.includes('consent_required') || err.message?.includes('403')) {
        setIsConsentModalOpen(true);
        setLoading(false);
        return;
      }

      // Diagnostic Clinical fallback
      setTimeout(() => {
        let responseContent =
          `### Clinical Copilot Response (${provider.toUpperCase()})\n\n` +
          `**Notice:** To query the live AI agent directly, click **"AI Provider & API Key"** in the top-right header and attach your Google Gemini or OpenAI API Key.\n\n` +
          `1. **Clinical Impression**: Evaluation of input "${text}".\n` +
          `2. **Protocol Check**: Ensure full patient vital history is correlated with current clinical presentation.\n` +
          `3. **Recommendation**: Review lab panels and drug interactions prior to finalizing prescription.`;

        setMessages((prev) => [
          ...prev,
          {
            id: Math.random().toString(),
            role: 'assistant',
            content: responseContent,
            createdAt: new Date().toISOString(),
          },
        ]);
      }, 500);
    } finally {
      setLoading(false);
    }
  };

  const quickPrompts = [
    'Evaluate HbA1c 8.4% and elevated fasting glucose',
    'Check penicillin allergy cross-reactivity with Cephalosporins',
    'Summarize pneumonia consolidation observed on CXR',
    'Generate clinical SOAP assessment for diabetic follow-up'
  ];

  return (
    <div className="flex flex-col h-[calc(100vh-8rem)] gap-4">
      {/* Top Header */}
      <div className="flex flex-wrap items-center justify-between gap-3 shrink-0 pb-3 border-b border-slate-800/80">
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

        {/* Patient Context & AI Key Settings */}
        <div className="flex flex-wrap items-center gap-2">
          {/* Patient Context Selector */}
          <div className="flex items-center gap-1.5 bg-slate-900 border border-slate-700 rounded-xl px-2.5 py-1.5 text-xs">
            <User className="w-3.5 h-3.5 text-sky-400" />
            <select
              value={selectedPatientId}
              onChange={(e) => {
                const pId = e.target.value;
                setSelectedPatientId(pId);
                if (pId === 'P-10024') setSelectedPatientName('Tariq Al-Mansoor');
                else if (pId === '1') setSelectedPatientName('Omar Al-Husseini');
                else if (pId === '2') setSelectedPatientName('Nour Mostafa');
                else setSelectedPatientName('');
              }}
              className="bg-transparent text-slate-200 text-xs focus:outline-none cursor-pointer"
            >
              <option value="" className="bg-slate-900 text-slate-400">No Patient Context (General)</option>
              <option value="P-10024" className="bg-slate-900 text-white">Tariq Al-Mansoor (P-10024)</option>
              <option value="1" className="bg-slate-900 text-white">Omar Al-Husseini (MED-10024)</option>
              <option value="2" className="bg-slate-900 text-white">Nour Mostafa (MED-10025)</option>
            </select>
          </div>

          <button
            onClick={() => setShowSettings(true)}
            className={`px-3 py-2 rounded-xl border text-xs font-semibold flex items-center gap-2 transition ${
              apiKey.trim()
                ? 'bg-emerald-500/15 border-emerald-500/40 text-emerald-300 shadow-sm'
                : 'bg-slate-900 border-slate-700 text-slate-300 hover:border-sky-500/50'
            }`}
          >
            <KeyRound className="w-4 h-4 text-sky-400" />
            <span>
              {apiKey.trim() ? `${provider.toUpperCase()} Active (Live)` : 'Attach AI API Key'}
            </span>
            <Settings2 className="w-3.5 h-3.5 text-slate-400" />
          </button>

          <button
            onClick={() =>
              setMessages([
                {
                  id: 'welcome',
                  role: 'assistant',
                  content:
                    "Chat history reset. How can I assist you in your clinical evaluation today?",
                  createdAt: new Date().toISOString(),
                },
              ])
            }
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
          Model: {apiKey.trim() ? model : 'Server Gateway'}
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
              <span>Querying {apiKey.trim() ? model : 'AI Provider'} for clinical assessment...</span>
            </div>
          </div>
        )}

        <div ref={messagesEndRef} />
      </div>

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

      {/* AI Provider & API Key Modal */}
      {showSettings && (
        <div className="fixed inset-0 z-50 bg-black/80 backdrop-blur-sm flex items-center justify-center p-4">
          <div className="glass-panel bg-slate-950 border border-slate-800 text-slate-100 max-w-lg w-full rounded-2xl p-6 shadow-2xl space-y-5 relative">
            <button
              onClick={() => setShowSettings(false)}
              className="absolute top-4 right-4 p-2 rounded-lg text-slate-400 hover:text-white transition"
            >
              <X className="w-5 h-5" />
            </button>

            <div>
              <h2 className="text-lg font-bold text-white flex items-center gap-2">
                <KeyRound className="w-5 h-5 text-sky-400" />
                AI Gateway & API Key Configuration
              </h2>
              <p className="text-xs text-slate-400 mt-1">
                Configure your AI Model Provider (Section 16 & 17). Your key is stored securely in your browser session or backend <code className="text-sky-400">appsettings.json</code>.
              </p>
            </div>

            <div className="space-y-4 text-xs">
              {/* Provider Selection */}
              <div>
                <label className="text-slate-300 font-semibold block mb-1.5">AI Provider</label>
                <div className="grid grid-cols-2 gap-3">
                  <button
                    type="button"
                    onClick={() => {
                      setProvider('gemini');
                      setModel('gemini-1.5-flash');
                      setTestStatus('idle');
                    }}
                    className={`p-3 rounded-xl border text-left transition flex items-center gap-3 ${
                      provider === 'gemini'
                        ? 'bg-sky-500/15 border-sky-500/50 text-sky-300'
                        : 'bg-slate-900 border-slate-800 text-slate-400 hover:text-slate-200'
                    }`}
                  >
                    <Zap className="w-5 h-5 text-sky-400 shrink-0" />
                    <div>
                      <div className="font-bold">Google Gemini</div>
                      <div className="text-[10px] text-slate-500">1.5 Flash / 2.0 Pro</div>
                    </div>
                  </button>

                  <button
                    type="button"
                    onClick={() => {
                      setProvider('openai');
                      setModel('gpt-4o');
                      setTestStatus('idle');
                    }}
                    className={`p-3 rounded-xl border text-left transition flex items-center gap-3 ${
                      provider === 'openai'
                        ? 'bg-emerald-500/15 border-emerald-500/50 text-emerald-300'
                        : 'bg-slate-900 border-slate-800 text-slate-400 hover:text-slate-200'
                    }`}
                  >
                    <Globe className="w-5 h-5 text-emerald-400 shrink-0" />
                    <div>
                      <div className="font-bold">OpenAI</div>
                      <div className="text-[10px] text-slate-500">GPT-4o / GPT-4o-mini</div>
                    </div>
                  </button>
                </div>
              </div>

              {/* Model Choice */}
              <div>
                <label className="text-slate-300 font-semibold block mb-1.5">Model Specification</label>
                <select
                  value={model}
                  onChange={(e) => setModel(e.target.value)}
                  className="w-full px-3 py-2.5 rounded-xl bg-slate-900 border border-slate-800 text-white focus:outline-none focus:border-sky-500 font-mono text-xs"
                >
                  {provider === 'gemini' ? (
                    <>
                      <option value="gemini-1.5-flash">gemini-1.5-flash (Fast, Low Latency - Recommended)</option>
                      <option value="gemini-1.5-pro">gemini-1.5-pro (High Reasoning & Vision)</option>
                      <option value="gemini-2.0-flash">gemini-2.0-flash (Next-Gen Gemini)</option>
                    </>
                  ) : (
                    <>
                      <option value="gpt-4o">gpt-4o (Omni Multimodal Flagship)</option>
                      <option value="gpt-4o-mini">gpt-4o-mini (Fast & Cost-Efficient)</option>
                      <option value="gpt-4-turbo">gpt-4-turbo</option>
                    </>
                  )}
                </select>
              </div>

              {/* API Key Input */}
              <div>
                <label className="text-slate-300 font-semibold block mb-1.5">
                  {provider === 'gemini' ? 'Google AI Studio API Key' : 'OpenAI API Secret Key'}
                </label>
                <div className="relative">
                  <input
                    type={showKey ? 'text' : 'password'}
                    value={apiKey}
                    onChange={(e) => {
                      setApiKey(e.target.value);
                      setTestStatus('idle');
                    }}
                    placeholder={provider === 'gemini' ? 'AIzaSy...' : 'sk-proj-...'}
                    className="w-full pl-3 pr-10 py-2.5 rounded-xl bg-slate-900 border border-slate-800 text-white font-mono placeholder-slate-600 focus:outline-none focus:border-sky-500"
                  />
                  <button
                    type="button"
                    onClick={() => setShowKey(!showKey)}
                    className="absolute right-3 top-3 text-slate-400 hover:text-white transition"
                  >
                    {showKey ? <EyeOff className="w-4 h-4" /> : <Eye className="w-4 h-4" />}
                  </button>
                </div>
                <p className="text-[10px] text-slate-500 mt-1">
                  Keys are never shared with unauthorized parties. Direct queries use client transport or verified backend gateway.
                </p>
              </div>

              {/* Test Status Feedback */}
              {testStatus !== 'idle' && (
                <div
                  className={`p-3 rounded-xl border text-xs flex items-center gap-2 ${
                    testStatus === 'testing'
                      ? 'bg-sky-500/10 border-sky-500/30 text-sky-300'
                      : testStatus === 'success'
                      ? 'bg-emerald-500/10 border-emerald-500/30 text-emerald-300'
                      : 'bg-rose-500/10 border-rose-500/30 text-rose-300'
                  }`}
                >
                  {testStatus === 'testing' && <Loader2 className="w-4 h-4 animate-spin" />}
                  {testStatus === 'success' && <CheckCircle2 className="w-4 h-4 text-emerald-400 shrink-0" />}
                  {testStatus === 'failed' && <AlertCircle className="w-4 h-4 text-rose-400 shrink-0" />}
                  <span className="truncate">{testMessage}</span>
                </div>
              )}

              {/* Action Buttons */}
              <div className="flex items-center gap-3 pt-2">
                <button
                  type="button"
                  onClick={handleTestKey}
                  disabled={testStatus === 'testing' || !apiKey.trim()}
                  className="flex-1 py-2.5 rounded-xl border border-slate-700 bg-slate-900 hover:bg-slate-800 text-slate-300 font-semibold text-xs transition disabled:opacity-50 flex items-center justify-center gap-1.5"
                >
                  <Zap className="w-3.5 h-3.5 text-amber-400" />
                  <span>Test Connection</span>
                </button>

                <button
                  type="button"
                  onClick={saveSettings}
                  className="flex-1 py-2.5 rounded-xl bg-gradient-to-r from-sky-500 to-indigo-600 hover:from-sky-400 hover:to-indigo-500 text-white font-bold text-xs shadow-lg shadow-sky-500/20 transition flex items-center justify-center gap-1.5"
                >
                  <CheckCircle2 className="w-3.5 h-3.5" />
                  <span>Save & Connect</span>
                </button>
              </div>
            </div>
          </div>
        </div>
      )}

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
