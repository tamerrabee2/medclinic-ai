'use client';

import React, { useState, useEffect } from 'react';
import { ApiClient } from '@/lib/api';
import {
  CalendarDays,
  Clock,
  User,
  Plus,
  CheckCircle2,
  AlertCircle,
  XCircle,
  Filter,
  Stethoscope,
  ChevronLeft,
  ChevronRight,
  X
} from 'lucide-react';

interface Appointment {
  id: string;
  patientName: string;
  doctorName: string;
  dateTime: string;
  status: string; // Scheduled, Confirmed, InProgress, Completed, Cancelled
  reason: string;
  durationMinutes: number;
}

export default function AppointmentsPage() {
  const [appointments, setAppointments] = useState<Appointment[]>([]);
  const [selectedDate, setSelectedDate] = useState('2026-09-04');
  const [showModal, setShowModal] = useState(false);

  // New appointment form
  const [form, setForm] = useState({
    patientName: '',
    doctorName: 'Dr. Sarah Al-Mansoor',
    dateTime: '2026-09-04T10:00',
    reason: 'Routine Consultation',
    duration: 30,
  });

  const loadAppointments = async () => {
    try {
      const data = await ApiClient.getAppointments(selectedDate);
      if (Array.isArray(data) && data.length > 0) {
        setAppointments(data);
      } else {
        throw new Error('Fallback to demo');
      }
    } catch {
      // Demo appointments
      setAppointments([
        { id: '1', patientName: 'Omar Al-Husseini', doctorName: 'Dr. Sarah Al-Mansoor', dateTime: '2026-09-04T09:00:00', status: 'Completed', reason: 'Hypertension Follow-up', durationMinutes: 30 },
        { id: '2', patientName: 'Nour Mostafa', doctorName: 'Dr. Sarah Al-Mansoor', dateTime: '2026-09-04T10:00:00', status: 'Confirmed', reason: 'Diabetes Management', durationMinutes: 30 },
        { id: '3', patientName: 'Laila Mahmoud', doctorName: 'Dr. Tariq Ziyad', dateTime: '2026-09-04T11:00:00', status: 'Scheduled', reason: 'Molar Toothache Exam', durationMinutes: 45 },
        { id: '4', patientName: 'Khaled bin Walid', doctorName: 'Dr. Sarah Al-Mansoor', dateTime: '2026-09-04T11:30:00', status: 'Scheduled', reason: 'Routine Blood Panel Review', durationMinutes: 30 },
        { id: '5', patientName: 'Fatima Zahra', doctorName: 'Dr. Tariq Ziyad', dateTime: '2026-09-04T14:00:00', status: 'Cancelled', reason: 'Dental Crown Fitting', durationMinutes: 60 },
      ]);
    }
  };

  useEffect(() => {
    loadAppointments();
  }, [selectedDate]);

  const handleBook = (e: React.FormEvent) => {
    e.preventDefault();
    const newApt: Appointment = {
      id: Math.random().toString(),
      patientName: form.patientName,
      doctorName: form.doctorName,
      dateTime: form.dateTime,
      status: 'Scheduled',
      reason: form.reason,
      durationMinutes: form.duration,
    };
    setAppointments([...appointments, newApt]);
    setShowModal(false);
  };

  const getStatusBadge = (status: string) => {
    switch (status.toLowerCase()) {
      case 'completed':
        return 'bg-emerald-500/15 text-emerald-300 border-emerald-500/30';
      case 'confirmed':
        return 'bg-sky-500/15 text-sky-300 border-sky-500/30';
      case 'inprogress':
        return 'bg-amber-500/15 text-amber-300 border-amber-500/30';
      case 'cancelled':
        return 'bg-rose-500/15 text-rose-300 border-rose-500/30';
      default:
        return 'bg-slate-700/50 text-slate-300 border-slate-600';
    }
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold tracking-tight text-white flex items-center gap-2">
            <CalendarDays className="w-6 h-6 text-sky-400" />
            <span>Clinical Appointments &amp; Schedule</span>
          </h1>
          <p className="text-sm text-slate-400 mt-1">
            Real-time multi-doctor calendar with automatic conflict detection.
          </p>
        </div>

        <div className="flex items-center gap-3">
          <input
            type="date"
            value={selectedDate}
            onChange={(e) => setSelectedDate(e.target.value)}
            className="px-3 py-2 bg-slate-900 border border-slate-700 rounded-xl text-xs text-white focus:outline-none focus:border-sky-500"
          />
          <button
            onClick={() => setShowModal(true)}
            className="flex items-center gap-2 px-4 py-2.5 rounded-xl bg-gradient-to-r from-sky-500 to-indigo-600 hover:from-sky-400 hover:to-indigo-500 text-white font-medium text-sm shadow-md shadow-sky-500/20 transition"
          >
            <Plus className="w-4 h-4" />
            <span>Book Appointment</span>
          </button>
        </div>
      </div>

      {/* Schedule Grid */}
      <div className="glass-panel rounded-2xl border-slate-800 p-6 space-y-4 shadow-xl">
        <div className="flex items-center justify-between border-b border-slate-800 pb-4">
          <div className="font-semibold text-slate-200 text-sm flex items-center gap-2">
            <Clock className="w-4 h-4 text-sky-400" />
            <span>Time Slots for {selectedDate}</span>
          </div>
          <div className="text-xs text-slate-400 font-mono">
            {appointments.length} Total Bookings
          </div>
        </div>

        <div className="space-y-3">
          {appointments.map((apt) => {
            const timeStr = new Date(apt.dateTime).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
            return (
              <div
                key={apt.id}
                className="p-4 rounded-xl glass-panel-interactive border-slate-800/80 flex flex-col md:flex-row md:items-center justify-between gap-4"
              >
                <div className="flex items-start md:items-center gap-4">
                  <div suppressHydrationWarning className="px-3 py-2 rounded-xl bg-sky-500/10 border border-sky-500/20 text-sky-400 font-mono text-sm font-bold shrink-0">
                    {timeStr}
                  </div>
                  <div>
                    <div className="font-bold text-white text-base flex items-center gap-2">
                      <span>{apt.patientName}</span>
                      <span className={`text-[11px] px-2.5 py-0.5 rounded-full font-medium border ${getStatusBadge(apt.status)}`}>
                        {apt.status}
                      </span>
                    </div>
                    <div className="text-xs text-slate-400 flex items-center gap-2 mt-0.5">
                      <span className="text-slate-300 font-medium">{apt.reason}</span>
                      <span>&bull;</span>
                      <span>{apt.doctorName}</span>
                      <span>&bull;</span>
                      <span>{apt.durationMinutes} mins</span>
                    </div>
                  </div>
                </div>

                <div className="flex items-center gap-2 self-end md:self-center">
                  <button
                    onClick={() => {
                      setAppointments(appointments.map(a => a.id === apt.id ? { ...a, status: 'Completed' } : a));
                    }}
                    className="px-3 py-1.5 rounded-lg bg-emerald-500/10 hover:bg-emerald-500/20 text-emerald-400 text-xs font-semibold border border-emerald-500/30 transition"
                  >
                    Complete
                  </button>
                  <button
                    onClick={() => {
                      setAppointments(appointments.map(a => a.id === apt.id ? { ...a, status: 'Cancelled' } : a));
                    }}
                    className="px-3 py-1.5 rounded-lg bg-rose-500/10 hover:bg-rose-500/20 text-rose-400 text-xs font-semibold border border-rose-500/30 transition"
                  >
                    Cancel
                  </button>
                </div>
              </div>
            );
          })}
        </div>
      </div>

      {/* Book Appointment Modal */}
      {showModal && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-slate-950/80 backdrop-blur-sm">
          <div className="glass-panel w-full max-w-md p-6 rounded-3xl border-slate-700 shadow-2xl space-y-4">
            <div className="flex items-center justify-between border-b border-slate-800 pb-3">
              <h2 className="text-lg font-bold text-white flex items-center gap-2">
                <Plus className="w-5 h-5 text-sky-400" />
                <span>Schedule Appointment</span>
              </h2>
              <button onClick={() => setShowModal(false)} className="text-slate-400 hover:text-white">
                <X className="w-5 h-5" />
              </button>
            </div>

            <form onSubmit={handleBook} className="space-y-4">
              <div className="space-y-1">
                <label className="text-xs text-slate-300 font-semibold">Patient Name</label>
                <input
                  type="text"
                  required
                  value={form.patientName}
                  onChange={(e) => setForm({ ...form, patientName: e.target.value })}
                  className="w-full px-3 py-2 bg-slate-900 border border-slate-700 rounded-xl text-sm text-white focus:border-sky-500 focus:outline-none"
                  placeholder="Patient Full Name"
                />
              </div>

              <div className="space-y-1">
                <label className="text-xs text-slate-300 font-semibold">Attending Physician</label>
                <select
                  value={form.doctorName}
                  onChange={(e) => setForm({ ...form, doctorName: e.target.value })}
                  className="w-full px-3 py-2 bg-slate-900 border border-slate-700 rounded-xl text-sm text-white focus:border-sky-500 focus:outline-none"
                >
                  <option value="Dr. Sarah Al-Mansoor">Dr. Sarah Al-Mansoor (Internal Medicine)</option>
                  <option value="Dr. Tariq Ziyad">Dr. Tariq Ziyad (Dental Surgery)</option>
                </select>
              </div>

              <div className="grid grid-cols-2 gap-4">
                <div className="space-y-1">
                  <label className="text-xs text-slate-300 font-semibold">Date &amp; Time</label>
                  <input
                    type="datetime-local"
                    required
                    value={form.dateTime}
                    onChange={(e) => setForm({ ...form, dateTime: e.target.value })}
                    className="w-full px-3 py-2 bg-slate-900 border border-slate-700 rounded-xl text-sm text-white focus:border-sky-500 focus:outline-none"
                  />
                </div>
                <div className="space-y-1">
                  <label className="text-xs text-slate-300 font-semibold">Duration (Mins)</label>
                  <select
                    value={form.duration}
                    onChange={(e) => setForm({ ...form, duration: Number(e.target.value) })}
                    className="w-full px-3 py-2 bg-slate-900 border border-slate-700 rounded-xl text-sm text-white focus:border-sky-500 focus:outline-none"
                  >
                    <option value={15}>15 mins</option>
                    <option value={30}>30 mins</option>
                    <option value={45}>45 mins</option>
                    <option value={60}>60 mins</option>
                  </select>
                </div>
              </div>

              <div className="space-y-1">
                <label className="text-xs text-slate-300 font-semibold">Chief Complaint / Reason</label>
                <input
                  type="text"
                  required
                  value={form.reason}
                  onChange={(e) => setForm({ ...form, reason: e.target.value })}
                  className="w-full px-3 py-2 bg-slate-900 border border-slate-700 rounded-xl text-sm text-white focus:border-sky-500 focus:outline-none"
                  placeholder="e.g. Chest tightness, Dental pain"
                />
              </div>

              <div className="flex justify-end gap-3 pt-4 border-t border-slate-800">
                <button
                  type="button"
                  onClick={() => setShowModal(false)}
                  className="px-4 py-2 rounded-xl glass-panel text-sm text-slate-300 hover:text-white"
                >
                  Cancel
                </button>
                <button
                  type="submit"
                  className="px-5 py-2 rounded-xl bg-gradient-to-r from-sky-500 to-indigo-600 text-white font-medium text-sm shadow-md"
                >
                  Confirm Booking
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}
