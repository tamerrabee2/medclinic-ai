"use client";

import { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { BarChart3, TrendingUp, Users, Calendar, FlaskConical, Image, Brain, DollarSign, Loader2, Download } from "lucide-react";

const PRESETS = [
  { label: "Today", days: 0 },
  { label: "Last 7 days", days: 7 },
  { label: "Last 30 days", days: 30 },
  { label: "Last 90 days", days: 90 },
];

function StatCard({ icon, label, value, sub, color }: {
  icon: React.ReactNode; label: string; value: string | number; sub?: string; color: string;
}) {
  return (
    <div className="rounded-2xl border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800 p-5">
      <div className="flex items-center gap-3 mb-3">
        <div className={`w-9 h-9 rounded-xl flex items-center justify-center ${color}`}>{icon}</div>
        <p className="text-sm text-gray-500 dark:text-gray-400">{label}</p>
      </div>
      <p className="text-2xl font-bold text-gray-900 dark:text-white">{value}</p>
      {sub && <p className="text-xs text-gray-400 mt-1">{sub}</p>}
    </div>
  );
}

export default function ReportsPage() {
  const [preset, setPreset] = useState(30);
  const to = new Date();
  const from = new Date();
  from.setDate(from.getDate() - preset);

  const { data, isLoading } = useQuery({
    queryKey: ["reports", preset],
    queryFn: () =>
      fetch(`/api/v1/reports?from=${from.toISOString()}&to=${to.toISOString()}&type=General`).then((r) => r.json()),
  });

  return (
    <div className="p-6 space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-xl font-semibold text-gray-900 dark:text-white">Reports & Analytics</h1>
          <p className="text-sm text-gray-500">Clinic performance overview</p>
        </div>
        <div className="flex items-center gap-2">
          {PRESETS.map((p) => (
            <button
              key={p.days}
              onClick={() => setPreset(p.days)}
              className={`px-3 py-1.5 rounded-lg text-sm font-medium transition-colors ${
                preset === p.days
                  ? "bg-blue-600 text-white"
                  : "bg-gray-100 dark:bg-gray-700 text-gray-600 dark:text-gray-300 hover:bg-gray-200"
              }`}
            >
              {p.label}
            </button>
          ))}
          <button className="flex items-center gap-1.5 px-3 py-1.5 rounded-lg bg-gray-100 dark:bg-gray-700 text-sm text-gray-600 dark:text-gray-300">
            <Download className="w-4 h-4" /> Export
          </button>
        </div>
      </div>

      {isLoading ? (
        <div className="flex justify-center py-16"><Loader2 className="w-8 h-8 text-blue-600 animate-spin" /></div>
      ) : data ? (
        <>
          {/* Stats Grid */}
          <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
            <StatCard icon={<Users className="w-4 h-4 text-blue-600" />} label="New Patients" value={data.totalPatients} color="bg-blue-50 dark:bg-blue-900/20" />
            <StatCard icon={<Calendar className="w-4 h-4 text-green-600" />} label="Appointments" value={data.totalAppointments} sub={`${data.completedVisits} completed`} color="bg-green-50 dark:bg-green-900/20" />
            <StatCard icon={<DollarSign className="w-4 h-4 text-teal-600" />} label="Revenue" value={`$${data.totalRevenue?.toLocaleString()}`} sub={`$${data.collectedRevenue?.toLocaleString()} collected`} color="bg-teal-50 dark:bg-teal-900/20" />
            <StatCard icon={<Brain className="w-4 h-4 text-purple-600" />} label="AI Interactions" value={data.aiInteractions} color="bg-purple-50 dark:bg-purple-900/20" />
          </div>

          <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
            <StatCard icon={<FlaskConical className="w-4 h-4 text-yellow-600" />} label="Lab Orders" value={data.totalLabOrders} color="bg-yellow-50 dark:bg-yellow-900/20" />
            <StatCard icon={<Image className="w-4 h-4 text-indigo-600" />} label="Imaging Studies" value={data.totalImagingStudies} color="bg-indigo-50 dark:bg-indigo-900/20" />
            <StatCard icon={<TrendingUp className="w-4 h-4 text-orange-600" />} label="Pending Follow-ups" value={data.pendingFollowUps} color="bg-orange-50 dark:bg-orange-900/20" />
            <StatCard icon={<BarChart3 className="w-4 h-4 text-red-600" />} label="Pending Revenue" value={`$${data.pendingRevenue?.toLocaleString()}`} color="bg-red-50 dark:bg-red-900/20" />
          </div>

          {/* Daily Chart Placeholder */}
          {data.dailyStats?.length > 0 && (
            <div className="rounded-2xl border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800 p-6">
              <h2 className="text-sm font-semibold text-gray-700 dark:text-gray-300 mb-4">Daily Appointments</h2>
              <div className="flex items-end gap-1 h-32">
                {data.dailyStats.map((d: any, i: number) => {
                  const max = Math.max(...data.dailyStats.map((s: any) => s.appointments), 1);
                  const h = (d.appointments / max) * 100;
                  return (
                    <div key={i} className="flex-1 flex flex-col items-center gap-1">
                      <div
                        className="w-full rounded-t-sm bg-blue-400 dark:bg-blue-500 transition-all"
                        style={{ height: `${h}%` }}
                        title={`${d.appointments} appointments`}
                      />
                    </div>
                  );
                })}
              </div>
            </div>
          )}
        </>
      ) : null}
    </div>
  );
}
