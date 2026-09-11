'use client';

import React, { useState } from 'react';
import Link from 'next/link';
import { PermissionGate } from '@/components/auth/PermissionGate';
import {
  TestTubes,
  Plus,
  Search,
  Filter,
  CheckCircle2,
  Clock,
  AlertTriangle,
  QrCode,
  FileText,
  ChevronRight,
  Sparkles,
  ArrowRight,
  User,
  FlaskConical,
  X
} from 'lucide-react';

interface LabOrder {
  id: string;
  orderNumber: string;
  patientName: string;
  mrn: string;
  doctor: string;
  testName: string;
  category: string;
  specimenType: 'Blood (Serum)' | 'Whole Blood (EDTA)' | 'Urine' | 'Sputum';
  barcode: string;
  status: 'ordered' | 'collected' | 'processing' | 'ready';
  priority: 'routine' | 'urgent' | 'stat';
  date: string;
}

const INITIAL_ORDERS: LabOrder[] = [
  {
    id: 'ord-1',
    orderNumber: 'LAB-ORD-9104',
    patientName: 'Tariq Al-Mansoor',
    mrn: 'P-10024',
    doctor: 'Dr. Sarah Al-Mansoor',
    testName: 'Comprehensive Metabolic Panel (CMP) + HbA1c',
    category: 'Clinical Biochemistry',
    specimenType: 'Blood (Serum)',
    barcode: 'BC-881920-A',
    status: 'ready',
    priority: 'urgent',
    date: 'Today, 08:30 AM'
  },
  {
    id: 'ord-2',
    orderNumber: 'LAB-ORD-9105',
    patientName: 'Nour Mostafa',
    mrn: 'P-10022',
    doctor: 'Dr. Sarah Al-Mansoor',
    testName: 'Complete Blood Count (CBC) with Differential',
    category: 'Hematology',
    specimenType: 'Whole Blood (EDTA)',
    barcode: 'BC-881921-B',
    status: 'processing',
    priority: 'routine',
    date: 'Today, 09:15 AM'
  },
  {
    id: 'ord-3',
    orderNumber: 'LAB-ORD-9106',
    patientName: 'Omar Al-Husseini',
    mrn: 'P-10021',
    doctor: 'Dr. Sarah Al-Mansoor',
    testName: 'Lipid Profile (Total, LDL, HDL, Triglycerides)',
    category: 'Clinical Biochemistry',
    specimenType: 'Blood (Serum)',
    barcode: 'BC-881922-C',
    status: 'collected',
    priority: 'routine',
    date: 'Today, 09:45 AM'
  },
  {
    id: 'ord-4',
    orderNumber: 'LAB-ORD-9107',
    patientName: 'Fatima Al-Shehri',
    mrn: 'P-10027',
    doctor: 'Pediatric Consultant',
    testName: 'Urine Routine & Microscopic Examination',
    category: 'Urinalysis',
    specimenType: 'Urine',
    barcode: 'BC-881923-D',
    status: 'ordered',
    priority: 'routine',
    date: 'Today, 10:20 AM'
  }
];

export default function LaboratoryDeskPage() {
  const [orders, setOrders] = useState<LabOrder[]>(INITIAL_ORDERS);
  const [searchQuery, setSearchQuery] = useState('');
  const [statusFilter, setStatusFilter] = useState('all');
  const [showOrderModal, setShowOrderModal] = useState(false);

  // New order state
  const [patientName, setPatientName] = useState('Tariq Al-Mansoor');
  const [testName, setTestName] = useState('Thyroid Function Panel (TSH, Free T3, Free T4)');
  const [specimen, setSpecimen] = useState<'Blood (Serum)' | 'Whole Blood (EDTA)' | 'Urine' | 'Sputum'>('Blood (Serum)');
  const [priority, setPriority] = useState<'routine' | 'urgent' | 'stat'>('routine');

  const handleCreateOrder = (e: React.FormEvent) => {
    e.preventDefault();
    const newOrd: LabOrder = {
      id: `ord-${Date.now()}`,
      orderNumber: `LAB-ORD-${orders.length + 9108}`,
      patientName,
      mrn: 'P-10024',
      doctor: 'Dr. Sarah Al-Mansoor',
      testName,
      category: 'Biochemistry',
      specimenType: specimen,
      barcode: `BC-${Math.floor(100000 + Math.random() * 900000)}-X`,
      status: 'ordered',
      priority,
      date: 'Just now'
    };
    setOrders([newOrd, ...orders]);
    setShowOrderModal(false);
  };

  const handleUpdateStatus = (id: string, newStatus: LabOrder['status']) => {
    setOrders(
      orders.map((o) => (o.id === id ? { ...o, status: newStatus } : o))
    );
  };

  const filtered = orders.filter((o) => {
    const matchesSearch =
      o.patientName.toLowerCase().includes(searchQuery.toLowerCase()) ||
      o.testName.toLowerCase().includes(searchQuery.toLowerCase()) ||
      o.orderNumber.toLowerCase().includes(searchQuery.toLowerCase()) ||
      o.barcode.toLowerCase().includes(searchQuery.toLowerCase());
    const matchesStatus = statusFilter === 'all' || o.status === statusFilter;
    return matchesSearch && matchesStatus;
  });

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
        <div>
          <div className="flex items-center gap-2">
            <span className="px-2 py-0.5 rounded text-[11px] font-bold bg-amber-500/20 text-amber-300 border border-amber-500/30">
              Module 30
            </span>
            <span className="text-xs text-slate-400">Diagnostic Laboratory</span>
          </div>
          <h1 className="text-2xl md:text-3xl font-bold text-white mt-1 flex items-center gap-3">
            <TestTubes className="w-8 h-8 text-amber-400" />
            Laboratory Orders & Specimen Tracking Desk
          </h1>
          <p className="text-sm text-slate-400 mt-0.5">
            Phlebotomy collection, barcode tracking, specimen validation, and automated AI analysis correlation.
          </p>
        </div>

        <div className="flex items-center gap-3">
          <Link
            href="/dashboard/lab-analyzer"
            className="px-4 py-2.5 rounded-xl border border-sky-500/30 bg-sky-500/10 hover:bg-sky-500/20 text-xs font-semibold text-sky-300 flex items-center gap-2 transition"
          >
            <Sparkles className="w-4 h-4 text-sky-400" />
            <span>Open AI Lab Analyzer</span>
          </Link>

          <PermissionGate permission="Lab.Create">
            <button
              onClick={() => setShowOrderModal(true)}
              className="px-5 py-2.5 rounded-xl bg-gradient-to-r from-amber-500 to-orange-500 hover:from-amber-400 hover:to-orange-400 text-sm font-semibold text-white shadow-lg shadow-amber-500/20 flex items-center gap-2 transition"
            >
              <Plus className="w-4 h-4" />
              <span>Create Lab Requisition</span>
            </button>
          </PermissionGate>
        </div>
      </div>

      {/* Metric Cards */}
      <div className="grid grid-cols-1 sm:grid-cols-4 gap-4">
        <div className="glass-panel p-5 rounded-2xl border-slate-800 flex items-center justify-between">
          <div>
            <div className="text-xs font-semibold text-slate-400">Total Orders (Today)</div>
            <div className="text-2xl font-black text-white mt-1 font-mono">{orders.length}</div>
            <div className="text-[11px] text-slate-400 mt-1">Samples Active</div>
          </div>
          <div className="w-12 h-12 rounded-xl bg-sky-500/10 border border-sky-500/30 flex items-center justify-center text-sky-400">
            <TestTubes className="w-6 h-6" />
          </div>
        </div>

        <div className="glass-panel p-5 rounded-2xl border-slate-800 flex items-center justify-between">
          <div>
            <div className="text-xs font-semibold text-slate-400">Awaiting Phlebotomy</div>
            <div className="text-2xl font-black text-amber-400 mt-1 font-mono">
              {orders.filter((o) => o.status === 'ordered').length}
            </div>
            <div className="text-[11px] text-amber-400/80 mt-1">Sample pending</div>
          </div>
          <div className="w-12 h-12 rounded-xl bg-amber-500/10 border border-amber-500/30 flex items-center justify-center text-amber-400">
            <Clock className="w-6 h-6" />
          </div>
        </div>

        <div className="glass-panel p-5 rounded-2xl border-slate-800 flex items-center justify-between">
          <div>
            <div className="text-xs font-semibold text-slate-400">In Analyzer Processing</div>
            <div className="text-2xl font-black text-indigo-400 mt-1 font-mono">
              {orders.filter((o) => o.status === 'processing' || o.status === 'collected').length}
            </div>
            <div className="text-[11px] text-indigo-300 mt-1">Running assays</div>
          </div>
          <div className="w-12 h-12 rounded-xl bg-indigo-500/10 border border-indigo-500/30 flex items-center justify-center text-indigo-400">
            <FlaskConical className="w-6 h-6" />
          </div>
        </div>

        <div className="glass-panel p-5 rounded-2xl border-slate-800 flex items-center justify-between">
          <div>
            <div className="text-xs font-semibold text-slate-400">Results Published</div>
            <div className="text-2xl font-black text-emerald-400 mt-1 font-mono">
              {orders.filter((o) => o.status === 'ready').length}
            </div>
            <div className="text-[11px] text-emerald-400 mt-1">Ready for doctor sign-off</div>
          </div>
          <div className="w-12 h-12 rounded-xl bg-emerald-500/10 border border-emerald-500/30 flex items-center justify-center text-emerald-400">
            <CheckCircle2 className="w-6 h-6" />
          </div>
        </div>
      </div>

      {/* Orders Table */}
      <div className="glass-panel p-5 rounded-2xl border-slate-800 space-y-4">
        {/* Controls */}
        <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3">
          <div className="relative max-w-sm w-full">
            <Search className="w-4 h-4 absolute left-3 top-3 text-slate-500" />
            <input
              type="text"
              placeholder="Search by patient, order #, test, barcode..."
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              className="w-full pl-9 pr-4 py-2 rounded-xl bg-slate-900 border border-slate-800 text-xs text-white placeholder-slate-500 focus:outline-none focus:border-amber-500"
            />
          </div>

          <div className="flex items-center gap-1 bg-slate-900/80 p-1 rounded-xl border border-slate-800 text-xs">
            {[
              { id: 'all', label: 'All Orders' },
              { id: 'ordered', label: 'Awaiting Collection' },
              { id: 'processing', label: 'Processing' },
              { id: 'ready', label: 'Ready' }
            ].map((tab) => (
              <button
                key={tab.id}
                onClick={() => setStatusFilter(tab.id)}
                className={`px-3 py-1 rounded-lg font-semibold transition ${
                  statusFilter === tab.id
                    ? 'bg-amber-500 text-white shadow-sm'
                    : 'text-slate-400 hover:text-slate-200'
                }`}
              >
                {tab.label}
              </button>
            ))}
          </div>
        </div>

        {/* Orders Table */}
        <div className="overflow-x-auto">
          <table className="w-full text-left text-sm">
            <thead>
              <tr className="border-b border-slate-800 text-xs font-semibold text-slate-400 uppercase tracking-wider">
                <th className="py-3 px-3">Order Ref</th>
                <th className="py-3 px-3">Patient</th>
                <th className="py-3 px-3">Test Requisition</th>
                <th className="py-3 px-3">Specimen Tube</th>
                <th className="py-3 px-3">Barcode Tracking</th>
                <th className="py-3 px-3">Status</th>
                <th className="py-3 px-3 text-right">Actions</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-800/60 text-xs">
              {filtered.map((order) => (
                <tr key={order.id} className="hover:bg-slate-900/40 transition">
                  <td className="py-3 px-3 font-mono font-bold text-sky-400">
                    {order.orderNumber}
                  </td>

                  <td className="py-3 px-3">
                    <div className="font-bold text-white">{order.patientName}</div>
                    <div className="text-[10px] text-slate-400 font-mono">MRN: {order.mrn}</div>
                  </td>

                  <td className="py-3 px-3">
                    <div className="font-semibold text-slate-200">{order.testName}</div>
                    <div className="text-[10px] text-slate-500">{order.category}</div>
                  </td>

                  <td className="py-3 px-3 text-slate-300">
                    {order.specimenType}
                  </td>

                  <td className="py-3 px-3">
                    <div className="flex items-center gap-1.5 font-mono text-[11px] text-amber-400">
                      <QrCode className="w-3.5 h-3.5" />
                      <span>{order.barcode}</span>
                    </div>
                  </td>

                  <td className="py-3 px-3 whitespace-nowrap">
                    {order.status === 'ordered' && (
                      <span className="px-2.5 py-0.5 rounded-full text-[10px] font-semibold bg-slate-800 text-slate-400 border border-slate-700">
                        Awaiting Sample
                      </span>
                    )}
                    {order.status === 'collected' && (
                      <span className="px-2.5 py-0.5 rounded-full text-[10px] font-semibold bg-amber-500/10 text-amber-400 border border-amber-500/30">
                        Sample In Transit
                      </span>
                    )}
                    {order.status === 'processing' && (
                      <span className="px-2.5 py-0.5 rounded-full text-[10px] font-semibold bg-indigo-500/15 text-indigo-300 border border-indigo-500/30 animate-pulse">
                        Analyzing
                      </span>
                    )}
                    {order.status === 'ready' && (
                      <span className="px-2.5 py-0.5 rounded-full text-[10px] font-bold bg-emerald-500/15 text-emerald-400 border border-emerald-500/30">
                        Results Published
                      </span>
                    )}
                  </td>

                  <td className="py-3 px-3 text-right whitespace-nowrap">
                    {order.status === 'ready' ? (
                      <Link
                        href="/dashboard/lab-analyzer"
                        className="px-3 py-1 rounded-lg bg-sky-600 hover:bg-sky-500 text-white text-xs font-semibold transition inline-flex items-center gap-1 shadow-sm"
                      >
                        <Sparkles className="w-3 h-3" />
                        <span>View Analysis</span>
                      </Link>
                    ) : order.status === 'ordered' ? (
                      <button
                        onClick={() => handleUpdateStatus(order.id, 'collected')}
                        className="px-3 py-1 rounded-lg bg-amber-600 hover:bg-amber-500 text-white text-xs font-semibold transition"
                      >
                        Collect Sample
                      </button>
                    ) : (
                      <button
                        onClick={() => handleUpdateStatus(order.id, 'ready')}
                        className="px-3 py-1 rounded-lg bg-indigo-600 hover:bg-indigo-500 text-white text-xs font-semibold transition"
                      >
                        Publish Results
                      </button>
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>

      {/* Create Order Modal */}
      {showOrderModal && (
        <div className="fixed inset-0 z-50 bg-black/80 backdrop-blur-sm flex items-center justify-center p-4">
          <div className="glass-panel bg-slate-950 border border-slate-800 text-slate-100 max-w-md w-full rounded-2xl p-6 shadow-2xl space-y-4 relative">
            <button
              onClick={() => setShowOrderModal(false)}
              className="absolute top-4 right-4 p-2 rounded-lg text-slate-400 hover:text-white transition"
            >
              <X className="w-5 h-5" />
            </button>

            <h2 className="text-base font-bold text-white flex items-center gap-2">
              <Plus className="w-5 h-5 text-amber-400" />
              Create Laboratory Requisition Order
            </h2>

            <form onSubmit={handleCreateOrder} className="space-y-3 text-xs">
              <div>
                <label className="text-slate-400 block mb-1">Patient</label>
                <input
                  type="text"
                  required
                  value={patientName}
                  onChange={(e) => setPatientName(e.target.value)}
                  className="w-full px-3 py-2 rounded-xl bg-slate-900 border border-slate-800 text-white focus:outline-none focus:border-amber-500"
                />
              </div>

              <div>
                <label className="text-slate-400 block mb-1">Laboratory Assay Panel</label>
                <input
                  type="text"
                  required
                  value={testName}
                  onChange={(e) => setTestName(e.target.value)}
                  className="w-full px-3 py-2 rounded-xl bg-slate-900 border border-slate-800 text-white focus:outline-none focus:border-amber-500"
                />
              </div>

              <div className="grid grid-cols-2 gap-3">
                <div>
                  <label className="text-slate-400 block mb-1">Specimen Tube</label>
                  <select
                    value={specimen}
                    onChange={(e: any) => setSpecimen(e.target.value)}
                    className="w-full px-3 py-2 rounded-xl bg-slate-900 border border-slate-800 text-white focus:outline-none focus:border-amber-500"
                  >
                    <option>Blood (Serum)</option>
                    <option>Whole Blood (EDTA)</option>
                    <option>Urine</option>
                    <option>Sputum</option>
                  </select>
                </div>

                <div>
                  <label className="text-slate-400 block mb-1">Priority</label>
                  <select
                    value={priority}
                    onChange={(e: any) => setPriority(e.target.value)}
                    className="w-full px-3 py-2 rounded-xl bg-slate-900 border border-slate-800 text-white focus:outline-none focus:border-amber-500"
                  >
                    <option value="routine">Routine</option>
                    <option value="urgent">Urgent</option>
                    <option value="stat">STAT (Emergency)</option>
                  </select>
                </div>
              </div>

              <button
                type="submit"
                className="w-full py-2.5 rounded-xl bg-amber-600 hover:bg-amber-500 text-white font-bold text-xs shadow-lg shadow-amber-600/20 transition mt-2"
              >
                Generate Barcode & Order
              </button>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}
