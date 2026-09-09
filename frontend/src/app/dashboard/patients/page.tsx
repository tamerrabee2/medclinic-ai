'use client';

import React, { useState, useEffect } from 'react';
import { ApiClient } from '@/lib/api';
import {
  Users,
  Search,
  Plus,
  Filter,
  Phone,
  Calendar,
  ChevronRight,
  Loader2,
  X,
  Stethoscope,
  Heart
} from 'lucide-react';

interface Patient {
  id: string;
  mrn?: string;
  firstName: string;
  lastName: string;
  phone?: string;
  gender: string;
  age?: number;
  dateOfBirth?: string;
  bloodGroup?: string;
  lastVisit?: string;
}

export default function PatientsPage() {
  const [patients, setPatients] = useState<Patient[]>([]);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState('');
  const [showModal, setShowModal] = useState(false);

  // New patient form
  const [form, setForm] = useState({
    firstName: '',
    lastName: '',
    phone: '',
    gender: 'Male',
    dateOfBirth: '1990-01-01',
    bloodGroup: 'O+',
  });

  const loadPatients = async (query = '') => {
    setLoading(true);
    try {
      const data = await ApiClient.getPatients(query);
      if (data && data.items) {
        setPatients(data.items);
      } else if (Array.isArray(data)) {
        setPatients(data);
      } else {
        throw new Error('Fallback to demo');
      }
    } catch {
      // Demo fallback data
      setPatients([
        { id: '1', mrn: 'MED-10024', firstName: 'Omar', lastName: 'Al-Husseini', phone: '+966 50 123 4567', gender: 'Male', age: 42, bloodGroup: 'O+', lastVisit: '2026-09-02' },
        { id: '2', mrn: 'MED-10025', firstName: 'Nour', lastName: 'Mostafa', phone: '+966 54 234 5678', gender: 'Female', age: 31, bloodGroup: 'A+', lastVisit: '2026-08-28' },
        { id: '3', mrn: 'MED-10026', firstName: 'Laila', lastName: 'Mahmoud', phone: '+966 56 345 6789', gender: 'Female', age: 26, bloodGroup: 'B+', lastVisit: '2026-09-01' },
        { id: '4', mrn: 'MED-10027', firstName: 'Khaled', lastName: 'bin Walid', phone: '+966 59 456 7890', gender: 'Male', age: 55, bloodGroup: 'AB+', lastVisit: '2026-08-15' },
        { id: '5', mrn: 'MED-10028', firstName: 'Fatima', lastName: 'Zahra', phone: '+966 55 567 8901', gender: 'Female', age: 38, bloodGroup: 'O-', lastVisit: '2026-09-03' },
      ]);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadPatients();
  }, []);

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault();
    loadPatients(search);
  };

  const handleCreatePatient = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      await ApiClient.createPatient(form);
      setShowModal(false);
      loadPatients();
    } catch {
      // Add locally for demo
      const newP: Patient = {
        id: Math.random().toString(),
        mrn: `MED-${Math.floor(10000 + Math.random() * 90000)}`,
        firstName: form.firstName,
        lastName: form.lastName,
        phone: form.phone,
        gender: form.gender,
        age: 30,
        bloodGroup: form.bloodGroup,
        lastVisit: 'Just now',
      };
      setPatients([newP, ...patients]);
      setShowModal(false);
    }
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold tracking-tight text-white flex items-center gap-2">
            <Users className="w-6 h-6 text-sky-400" />
            <span>Patient Registry</span>
          </h1>
          <p className="text-sm text-slate-400 mt-1">
            Complete electronic health records and medical directory.
          </p>
        </div>

        <button
          onClick={() => setShowModal(true)}
          className="flex items-center gap-2 px-4 py-2.5 rounded-xl bg-gradient-to-r from-sky-500 to-indigo-600 hover:from-sky-400 hover:to-indigo-500 text-white font-medium text-sm shadow-md shadow-sky-500/20 transition self-start sm:self-auto"
        >
          <Plus className="w-4 h-4" />
          <span>Add New Patient</span>
        </button>
      </div>

      {/* Search Bar */}
      <form onSubmit={handleSearch} className="flex gap-3">
        <div className="relative flex-1">
          <Search className="w-4 h-4 absolute left-3.5 top-1/2 -translate-y-1/2 text-slate-500" />
          <input
            type="text"
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            placeholder="Search by patient name, phone, or MRN..."
            className="w-full pl-10 pr-4 py-2.5 bg-slate-900/60 border border-slate-800 rounded-xl text-sm text-white placeholder-slate-500 focus:outline-none focus:border-sky-500 transition"
          />
        </div>
        <button
          type="submit"
          className="px-5 py-2.5 rounded-xl glass-panel-interactive text-sm font-medium text-slate-200"
        >
          Search
        </button>
      </form>

      {/* Patients Table */}
      <div className="glass-panel rounded-2xl border-slate-800 overflow-hidden shadow-xl">
        {loading ? (
          <div className="p-12 flex items-center justify-center text-sky-400 gap-2">
            <Loader2 className="w-6 h-6 animate-spin" />
            <span>Loading clinical patient registry...</span>
          </div>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm">
              <thead className="bg-slate-900/80 text-xs uppercase tracking-wider text-slate-400 border-b border-slate-800">
                <tr>
                  <th className="py-3.5 px-4 font-semibold">MRN</th>
                  <th className="py-3.5 px-4 font-semibold">Patient Name</th>
                  <th className="py-3.5 px-4 font-semibold">Demographics</th>
                  <th className="py-3.5 px-4 font-semibold">Contact</th>
                  <th className="py-3.5 px-4 font-semibold">Blood Group</th>
                  <th className="py-3.5 px-4 font-semibold">Last Visit</th>
                  <th className="py-3.5 px-4 text-right font-semibold">Clinical Tools</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-800/60">
                {patients.map((p) => (
                  <tr key={p.id} className="hover:bg-slate-900/40 transition">
                    <td className="py-3.5 px-4 font-mono text-xs text-sky-400 font-semibold">
                      {p.mrn || 'MED-NEW'}
                    </td>
                    <td className="py-3.5 px-4">
                      <div className="font-semibold text-slate-200">
                        {p.firstName} {p.lastName}
                      </div>
                    </td>
                    <td className="py-3.5 px-4 text-slate-300">
                      <span className="capitalize">{p.gender}</span>, {p.age || '32'} yrs
                    </td>
                    <td className="py-3.5 px-4 text-slate-400 font-mono text-xs">
                      {p.phone || 'N/A'}
                    </td>
                    <td className="py-3.5 px-4">
                      <span className="px-2 py-0.5 rounded-full bg-rose-500/15 border border-rose-500/30 text-rose-300 text-xs font-semibold">
                        {p.bloodGroup || 'O+'}
                      </span>
                    </td>
                    <td className="py-3.5 px-4 text-slate-400 text-xs">
                      {p.lastVisit || 'None recorded'}
                    </td>
                    <td className="py-3.5 px-4 text-right space-x-2">
                      <a
                        href="/dashboard/canvas"
                        className="inline-block px-2.5 py-1 rounded-lg glass-panel hover:border-sky-500/40 text-sky-400 text-xs font-medium transition"
                        title="Open Canvas Annotation"
                      >
                        Body Map
                      </a>
                      <a
                        href="/dashboard/dental"
                        className="inline-block px-2.5 py-1 rounded-lg glass-panel hover:border-indigo-500/40 text-indigo-400 text-xs font-medium transition"
                        title="Open Dental Chart"
                      >
                        Dental
                      </a>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>

      {/* Add Patient Modal */}
      {showModal && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-slate-950/80 backdrop-blur-sm">
          <div className="glass-panel w-full max-w-lg p-6 rounded-3xl border-slate-700 shadow-2xl space-y-5 animate-in fade-in zoom-in-95 duration-200">
            <div className="flex items-center justify-between border-b border-slate-800 pb-3">
              <h2 className="text-lg font-bold text-white flex items-center gap-2">
                <Plus className="w-5 h-5 text-sky-400" />
                <span>Register New Patient</span>
              </h2>
              <button
                onClick={() => setShowModal(false)}
                className="p-1 rounded-lg text-slate-400 hover:text-white"
              >
                <X className="w-5 h-5" />
              </button>
            </div>

            <form onSubmit={handleCreatePatient} className="space-y-4">
              <div className="grid grid-cols-2 gap-4">
                <div className="space-y-1">
                  <label className="text-xs text-slate-300 font-semibold">First Name</label>
                  <input
                    type="text"
                    required
                    value={form.firstName}
                    onChange={(e) => setForm({ ...form, firstName: e.target.value })}
                    className="w-full px-3 py-2 bg-slate-900 border border-slate-700 rounded-xl text-sm text-white focus:border-sky-500 focus:outline-none"
                    placeholder="e.g. Tariq"
                  />
                </div>
                <div className="space-y-1">
                  <label className="text-xs text-slate-300 font-semibold">Last Name</label>
                  <input
                    type="text"
                    required
                    value={form.lastName}
                    onChange={(e) => setForm({ ...form, lastName: e.target.value })}
                    className="w-full px-3 py-2 bg-slate-900 border border-slate-700 rounded-xl text-sm text-white focus:border-sky-500 focus:outline-none"
                    placeholder="e.g. Al-Basha"
                  />
                </div>
              </div>

              <div className="grid grid-cols-2 gap-4">
                <div className="space-y-1">
                  <label className="text-xs text-slate-300 font-semibold">Phone</label>
                  <input
                    type="tel"
                    required
                    value={form.phone}
                    onChange={(e) => setForm({ ...form, phone: e.target.value })}
                    className="w-full px-3 py-2 bg-slate-900 border border-slate-700 rounded-xl text-sm text-white focus:border-sky-500 focus:outline-none"
                    placeholder="0501234567"
                  />
                </div>
                <div className="space-y-1">
                  <label className="text-xs text-slate-300 font-semibold">Gender</label>
                  <select
                    value={form.gender}
                    onChange={(e) => setForm({ ...form, gender: e.target.value })}
                    className="w-full px-3 py-2 bg-slate-900 border border-slate-700 rounded-xl text-sm text-white focus:border-sky-500 focus:outline-none"
                  >
                    <option value="Male">Male</option>
                    <option value="Female">Female</option>
                  </select>
                </div>
              </div>

              <div className="grid grid-cols-2 gap-4">
                <div className="space-y-1">
                  <label className="text-xs text-slate-300 font-semibold">Date of Birth</label>
                  <input
                    type="date"
                    value={form.dateOfBirth}
                    onChange={(e) => setForm({ ...form, dateOfBirth: e.target.value })}
                    className="w-full px-3 py-2 bg-slate-900 border border-slate-700 rounded-xl text-sm text-white focus:border-sky-500 focus:outline-none"
                  />
                </div>
                <div className="space-y-1">
                  <label className="text-xs text-slate-300 font-semibold">Blood Group</label>
                  <select
                    value={form.bloodGroup}
                    onChange={(e) => setForm({ ...form, bloodGroup: e.target.value })}
                    className="w-full px-3 py-2 bg-slate-900 border border-slate-700 rounded-xl text-sm text-white focus:border-sky-500 focus:outline-none"
                  >
                    <option value="O+">O+</option>
                    <option value="O-">O-</option>
                    <option value="A+">A+</option>
                    <option value="A-">A-</option>
                    <option value="B+">B+</option>
                    <option value="B-">B-</option>
                    <option value="AB+">AB+</option>
                    <option value="AB-">AB-</option>
                  </select>
                </div>
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
                  Save Patient
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}
