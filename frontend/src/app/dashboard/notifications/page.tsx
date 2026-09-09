'use client';

import React, { useState } from 'react';
import Link from 'next/link';
import {
  Bell,
  CheckCircle2,
  AlertTriangle,
  Clock,
  Calendar,
  TestTubes,
  ScanLine,
  Pill,
  Trash2,
  CheckCheck,
  Filter,
  ArrowRight,
  ShieldAlert,
  ChevronRight
} from 'lucide-react';

interface NotificationItem {
  id: string;
  title: string;
  body: string;
  type: 'alert' | 'lab' | 'radiology' | 'appointment' | 'prescription' | 'billing';
  isRead: boolean;
  time: string;
  link?: string;
  priority?: 'high' | 'normal' | 'urgent';
}

const INITIAL_NOTIFICATIONS: NotificationItem[] = [
  {
    id: 'notif-1',
    title: 'Critical Lab Biomarker Alert (Tariq Al-Mansoor)',
    body: 'HbA1c level at 8.4% and fasting blood glucose 168 mg/dL requires prompt therapy adjustment.',
    type: 'lab',
    isRead: false,
    time: '10 minutes ago',
    link: '/dashboard/lab-analyzer',
    priority: 'urgent'
  },
  {
    id: 'notif-2',
    title: 'Radiology Computer Vision Findings Ready',
    body: 'Chest X-Ray PA scan analyzed by AI: Right lower lobe consolidation detected (94.2% confidence).',
    type: 'radiology',
    isRead: false,
    time: '45 minutes ago',
    link: '/dashboard/radiology',
    priority: 'high'
  },
  {
    id: 'notif-3',
    title: 'New Patient Appointment Scheduled',
    body: 'Layla Al-Otaibi booked for Dental Root Canal assessment at 11:30 AM today.',
    type: 'appointment',
    isRead: false,
    time: '2 hours ago',
    link: '/dashboard/appointments',
    priority: 'normal'
  },
  {
    id: 'notif-4',
    title: 'Electronic Prescription Transmitted',
    body: 'Refill for Metformin 850mg & Atorvastatin 20mg approved and sent to internal pharmacy.',
    type: 'prescription',
    isRead: true,
    time: 'Yesterday, 04:15 PM',
    link: '/dashboard/prescriptions',
    priority: 'normal'
  },
  {
    id: 'notif-5',
    title: 'Insurance Co-Pay Claim Settled',
    body: 'Bupa Arabia approved reimbursement for INV-2026-0041 (760 SAR).',
    type: 'billing',
    isRead: true,
    time: 'Yesterday, 02:00 PM',
    link: '/dashboard/billing',
    priority: 'normal'
  }
];

export default function NotificationsPage() {
  const [notifications, setNotifications] = useState<NotificationItem[]>(INITIAL_NOTIFICATIONS);
  const [filter, setFilter] = useState<'all' | 'unread' | 'urgent'>('all');

  const handleMarkAsRead = (id: string) => {
    setNotifications(
      notifications.map((n) => (n.id === id ? { ...n, isRead: true } : n))
    );
  };

  const handleMarkAllRead = () => {
    setNotifications(notifications.map((n) => ({ ...n, isRead: true })));
  };

  const handleDelete = (id: string) => {
    setNotifications(notifications.filter((n) => n.id !== id));
  };

  const filtered = notifications.filter((n) => {
    if (filter === 'unread') return !n.isRead;
    if (filter === 'urgent') return n.priority === 'urgent' || n.priority === 'high';
    return true;
  });

  const unreadCount = notifications.filter((n) => !n.isRead).length;

  return (
    <div className="space-y-6 max-w-4xl mx-auto">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div>
          <div className="flex items-center gap-2">
            <span className="px-2 py-0.5 rounded text-[11px] font-bold bg-sky-500/20 text-sky-300 border border-sky-500/30">
              Notification Center
            </span>
            {unreadCount > 0 && (
              <span className="px-2 py-0.5 rounded-full text-[11px] font-bold bg-rose-500/20 text-rose-400 border border-rose-500/30">
                {unreadCount} Unread
              </span>
            )}
          </div>
          <h1 className="text-2xl md:text-3xl font-bold text-white mt-1 flex items-center gap-3">
            <Bell className="w-8 h-8 text-sky-400" />
            Clinical Alerts & Notifications
          </h1>
          <p className="text-sm text-slate-400 mt-0.5">
            Real-time medical alerts, critical lab values, diagnostic reports, and patient appointments.
          </p>
        </div>

        {/* Actions */}
        <div className="flex items-center gap-3">
          <button
            onClick={handleMarkAllRead}
            disabled={unreadCount === 0}
            className="px-4 py-2.5 rounded-xl border border-slate-700 bg-slate-900 hover:bg-slate-800 text-xs font-semibold text-slate-300 transition flex items-center gap-2 disabled:opacity-50"
          >
            <CheckCheck className="w-4 h-4 text-emerald-400" />
            <span>Mark All as Read</span>
          </button>
        </div>
      </div>

      {/* Filter Tabs */}
      <div className="flex items-center gap-2 border-b border-slate-800 pb-3 text-xs">
        <button
          onClick={() => setFilter('all')}
          className={`px-3 py-1.5 rounded-lg font-semibold transition ${
            filter === 'all'
              ? 'bg-sky-500 text-white shadow-sm'
              : 'text-slate-400 hover:text-slate-200 bg-slate-900'
          }`}
        >
          All ({notifications.length})
        </button>

        <button
          onClick={() => setFilter('unread')}
          className={`px-3 py-1.5 rounded-lg font-semibold transition flex items-center gap-1.5 ${
            filter === 'unread'
              ? 'bg-sky-500 text-white shadow-sm'
              : 'text-slate-400 hover:text-slate-200 bg-slate-900'
          }`}
        >
          <span>Unread</span>
          {unreadCount > 0 && (
            <span className="w-4 h-4 rounded-full bg-rose-500 text-white text-[10px] flex items-center justify-center font-bold">
              {unreadCount}
            </span>
          )}
        </button>

        <button
          onClick={() => setFilter('urgent')}
          className={`px-3 py-1.5 rounded-lg font-semibold transition ${
            filter === 'urgent'
              ? 'bg-amber-500 text-white shadow-sm'
              : 'text-slate-400 hover:text-slate-200 bg-slate-900'
          }`}
        >
          Critical / Urgent
        </button>
      </div>

      {/* Notifications List */}
      <div className="space-y-3">
        {filtered.length === 0 ? (
          <div className="glass-panel p-12 rounded-2xl border-slate-800 text-center space-y-3">
            <div className="w-12 h-12 rounded-full bg-slate-900 border border-slate-800 flex items-center justify-center mx-auto text-slate-500">
              <CheckCircle2 className="w-6 h-6" />
            </div>
            <div className="text-slate-300 font-semibold text-sm">No notifications found</div>
            <p className="text-slate-500 text-xs max-w-sm mx-auto">
              You are all caught up! New clinical alerts and appointment updates will appear here in real time.
            </p>
          </div>
        ) : (
          filtered.map((item) => {
            let Icon = Bell;
            let iconColor = 'text-sky-400 bg-sky-500/15 border-sky-500/30';
            if (item.type === 'lab') {
              Icon = TestTubes;
              iconColor = 'text-amber-400 bg-amber-500/15 border-amber-500/30';
            } else if (item.type === 'radiology') {
              Icon = ScanLine;
              iconColor = 'text-indigo-400 bg-indigo-500/15 border-indigo-500/30';
            } else if (item.type === 'appointment') {
              Icon = Calendar;
              iconColor = 'text-emerald-400 bg-emerald-500/15 border-emerald-500/30';
            } else if (item.type === 'prescription') {
              Icon = Pill;
              iconColor = 'text-teal-400 bg-teal-500/15 border-teal-500/30';
            }

            return (
              <div
                key={item.id}
                className={`glass-panel p-4 rounded-2xl border transition flex items-start justify-between gap-4 ${
                  !item.isRead
                    ? 'bg-slate-900/90 border-sky-500/40 shadow-md shadow-sky-500/5'
                    : 'bg-slate-950/50 border-slate-800/80 opacity-80'
                }`}
              >
                <div className="flex items-start gap-3.5 flex-1 min-w-0">
                  <div
                    className={`w-10 h-10 rounded-xl border flex items-center justify-center shrink-0 ${iconColor}`}
                  >
                    <Icon className="w-5 h-5" />
                  </div>

                  <div className="space-y-1 min-w-0 flex-1">
                    <div className="flex items-center gap-2 flex-wrap">
                      <span className={`text-sm font-bold ${!item.isRead ? 'text-white' : 'text-slate-300'}`}>
                        {item.title}
                      </span>
                      {item.priority === 'urgent' && (
                        <span className="px-2 py-0.5 rounded text-[10px] font-bold bg-rose-500/20 text-rose-400 border border-rose-500/30">
                          Urgent Alert
                        </span>
                      )}
                      {!item.isRead && (
                        <span className="w-2 h-2 rounded-full bg-sky-400 shrink-0" />
                      )}
                    </div>

                    <p className="text-xs text-slate-400 leading-relaxed">{item.body}</p>

                    <div className="flex items-center gap-3 pt-1 text-[11px] text-slate-500">
                      <span className="flex items-center gap-1 font-mono">
                        <Clock className="w-3 h-3" /> {item.time}
                      </span>
                      {item.link && (
                        <Link
                          href={item.link}
                          onClick={() => handleMarkAsRead(item.id)}
                          className="text-sky-400 hover:text-sky-300 font-medium flex items-center gap-1 transition"
                        >
                          <span>Open Module</span>
                          <ArrowRight className="w-3 h-3" />
                        </Link>
                      )}
                    </div>
                  </div>
                </div>

                <div className="flex items-center gap-1 shrink-0">
                  {!item.isRead && (
                    <button
                      onClick={() => handleMarkAsRead(item.id)}
                      className="p-1.5 rounded-lg text-slate-400 hover:text-emerald-400 hover:bg-emerald-500/10 transition"
                      title="Mark as Read"
                    >
                      <CheckCircle2 className="w-4 h-4" />
                    </button>
                  )}
                  <button
                    onClick={() => handleDelete(item.id)}
                    className="p-1.5 rounded-lg text-slate-400 hover:text-rose-400 hover:bg-rose-500/10 transition"
                    title="Delete Notification"
                  >
                    <Trash2 className="w-4 h-4" />
                  </button>
                </div>
              </div>
            );
          })
        )}
      </div>
    </div>
  );
}
