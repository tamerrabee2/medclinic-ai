"use client";

import { useParams } from "next/navigation";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useState } from "react";
import { UserRound, Send, KeyRound } from "lucide-react";

export default function PatientPortalPage() {
  const { id: patientId } = useParams<{ id: string }>();
  const qc = useQueryClient();

  const [email, setEmail] = useState("");
  const [passwordHash, setPasswordHash] = useState("");
  const [subject, setSubject] = useState("");
  const [body, setBody] = useState("");

  const { data } = useQuery({
    queryKey: ["patient-portal-messages", patientId],
    queryFn: () => fetch(`/api/v1/patients/${patientId}/portal/messages`).then(r => r.json())
  });

  const provision = useMutation({
    mutationFn: () => fetch(`/api/v1/patients/${patientId}/portal/provision`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ email, passwordHash })
    }).then(r => r.json())
  });

  const sendMessage = useMutation({
    mutationFn: () => fetch(`/api/v1/patients/${patientId}/portal/messages`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ senderType: "Clinic", subject, body })
    }).then(r => r.json()),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["patient-portal-messages", patientId] })
  });

  return (
    <div className="max-w-5xl mx-auto p-6 space-y-6">
      <div>
        <h1 className="text-xl font-semibold text-gray-900 dark:text-white">Patient Portal</h1>
        <p className="text-sm text-gray-500">Provision portal access and exchange secure messages with the patient.</p>
      </div>

      <div className="grid gap-6 md:grid-cols-2">
        <div className="rounded-2xl border border-gray-200 bg-white dark:bg-gray-900 dark:border-gray-700 p-5 space-y-4">
          <div className="flex items-center gap-2 text-gray-900 dark:text-white font-medium"><KeyRound className="h-4 w-4 text-blue-600" /> Provision Access</div>
          <input value={email} onChange={(e) => setEmail(e.target.value)} placeholder="patient@example.com" className="w-full rounded-xl border border-gray-200 px-3 py-2 text-sm dark:border-gray-700 dark:bg-gray-800" />
          <input value={passwordHash} onChange={(e) => setPasswordHash(e.target.value)} placeholder="Password hash" className="w-full rounded-xl border border-gray-200 px-3 py-2 text-sm dark:border-gray-700 dark:bg-gray-800" />
          <button onClick={() => provision.mutate()} className="inline-flex items-center gap-2 rounded-xl bg-blue-600 px-4 py-2 text-white hover:bg-blue-700">
            <UserRound className="h-4 w-4" /> Save Access
          </button>
        </div>

        <div className="rounded-2xl border border-gray-200 bg-white dark:bg-gray-900 dark:border-gray-700 p-5 space-y-4">
          <div className="text-gray-900 dark:text-white font-medium">Send Message</div>
          <input value={subject} onChange={(e) => setSubject(e.target.value)} placeholder="Subject" className="w-full rounded-xl border border-gray-200 px-3 py-2 text-sm dark:border-gray-700 dark:bg-gray-800" />
          <textarea value={body} onChange={(e) => setBody(e.target.value)} rows={6} placeholder="Message body" className="w-full rounded-xl border border-gray-200 px-3 py-3 text-sm dark:border-gray-700 dark:bg-gray-800" />
          <button onClick={() => sendMessage.mutate()} className="inline-flex items-center gap-2 rounded-xl bg-teal-600 px-4 py-2 text-white hover:bg-teal-700">
            <Send className="h-4 w-4" /> Send
          </button>
        </div>
      </div>

      <div className="rounded-2xl border border-gray-200 bg-white dark:bg-gray-900 dark:border-gray-700 overflow-hidden">
        <div className="px-5 py-4 border-b border-gray-100 dark:border-gray-800 font-medium text-gray-900 dark:text-white">Messages</div>
        <div className="divide-y divide-gray-100 dark:divide-gray-800">
          {(data ?? []).map((msg: any) => (
            <div key={msg.id} className="p-5">
              <div className="flex items-center justify-between gap-4">
                <p className="text-sm font-medium text-gray-900 dark:text-white">{msg.subject}</p>
                <span className="text-xs text-gray-400">{new Date(msg.createdAt).toLocaleString()}</span>
              </div>
              <p className="text-xs text-gray-400 mt-1">Sender: {msg.senderType}</p>
              <p className="text-sm text-gray-600 dark:text-gray-300 mt-2">{msg.body}</p>
            </div>
          ))}
          {(data?.length ?? 0) === 0 && <div className="p-5 text-sm text-gray-500">No portal messages yet.</div>}
        </div>
      </div>
    </div>
  );
}
