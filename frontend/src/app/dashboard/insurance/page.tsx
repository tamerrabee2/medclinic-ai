"use client";

import { useState } from "react";
import { useMutation } from "@tanstack/react-query";
import { Shield, Send, Loader2, Search } from "lucide-react";

export default function InsurancePage() {
  const [payloadJson, setPayloadJson] = useState('{\n  "invoiceCode": "INV-001",\n  "diagnosis": "Hypertension"\n}');
  const [claim, setClaim] = useState<any>(null);

  const submitMutation = useMutation({
    mutationFn: async () => {
      const res = await fetch("/api/v1/insurance/claims", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          patientId: crypto.randomUUID(),
          invoiceId: crypto.randomUUID(),
          policyId: crypto.randomUUID(),
          payloadJson,
        }),
      });
      return res.json();
    },
    onSuccess: setClaim
  });

  const statusMutation = useMutation({
    mutationFn: async () => {
      const res = await fetch(`/api/v1/insurance/claims/${claim.id}/status`, { method: "POST" });
      return res.json();
    },
    onSuccess: setClaim
  });

  return (
    <div className="max-w-4xl mx-auto p-6 space-y-6">
      <div>
        <h1 className="text-xl font-semibold text-gray-900 dark:text-white">Insurance Claims</h1>
        <p className="text-sm text-gray-500">Submit claims to payer gateways and track approval outcomes.</p>
      </div>

      <div className="rounded-2xl border border-gray-200 bg-white dark:bg-gray-900 dark:border-gray-700 p-5 space-y-4">
        <div className="flex items-center gap-2 text-gray-900 dark:text-white font-medium">
          <Shield className="h-4 w-4 text-blue-600" /> Claim Payload
        </div>
        <textarea
          value={payloadJson}
          onChange={(e) => setPayloadJson(e.target.value)}
          rows={10}
          className="w-full rounded-xl border border-gray-200 px-3 py-3 text-sm dark:border-gray-700 dark:bg-gray-800"
        />
        <div className="flex gap-2">
          <button onClick={() => submitMutation.mutate()} className="inline-flex items-center gap-2 rounded-xl bg-blue-600 px-4 py-2 text-white hover:bg-blue-700">
            {submitMutation.isPending ? <Loader2 className="h-4 w-4 animate-spin" /> : <Send className="h-4 w-4" />} Submit Claim
          </button>
          {claim && (
            <button onClick={() => statusMutation.mutate()} className="inline-flex items-center gap-2 rounded-xl bg-teal-600 px-4 py-2 text-white hover:bg-teal-700">
              {statusMutation.isPending ? <Loader2 className="h-4 w-4 animate-spin" /> : <Search className="h-4 w-4" />} Check Status
            </button>
          )}
        </div>
      </div>

      {claim && (
        <div className="rounded-2xl border border-gray-200 bg-white dark:bg-gray-900 dark:border-gray-700 p-5 space-y-2">
          <p className="text-sm font-medium text-gray-900 dark:text-white">Claim #{claim.claimNumber}</p>
          <p className="text-sm text-gray-500">Status: {claim.status}</p>
          <p className="text-sm text-gray-500">Claimed: {claim.claimedAmount}</p>
          <p className="text-sm text-gray-500">Approved: {claim.approvedAmount}</p>
          {claim.responseJson && <pre className="overflow-auto rounded-xl bg-gray-50 dark:bg-gray-800 p-4 text-xs">{claim.responseJson}</pre>}
        </div>
      )}
    </div>
  );
}
