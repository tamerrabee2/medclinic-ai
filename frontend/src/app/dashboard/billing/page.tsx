'use client';

import React, { useState } from 'react';
import {
  ReceiptText,
  DollarSign,
  CreditCard,
  Building,
  TrendingUp,
  Search,
  Plus,
  Filter,
  CheckCircle2,
  Clock,
  AlertCircle,
  FileSpreadsheet,
  Printer,
  X,
  User,
  ShieldCheck,
  ChevronRight
} from 'lucide-react';
import { RouteGuard } from '@/components/auth/RouteGuard';
import { PermissionGate } from '@/components/auth/PermissionGate';

interface Invoice {
  id: string;
  invoiceNumber: string;
  patientName: string;
  mrn: string;
  date: string;
  totalAmount: number;
  insuranceShare: number;
  patientShare: number;
  paidAmount: number;
  status: 'paid' | 'partial' | 'unpaid' | 'insurance_pending';
  insuranceProvider?: string;
  services: string[];
}

const INITIAL_INVOICES: Invoice[] = [
  {
    id: 'inv-1',
    invoiceNumber: 'INV-2026-0041',
    patientName: 'Tariq Al-Mansoor',
    mrn: 'P-10024',
    date: 'Today, 10:30 AM',
    totalAmount: 950,
    insuranceShare: 760,
    patientShare: 190,
    paidAmount: 190,
    status: 'paid',
    insuranceProvider: 'Bupa Arabia (Gold)',
    services: ['Internal Med Consultation', 'Comprehensive Metabolic Panel', 'HbA1c Assay']
  },
  {
    id: 'inv-2',
    invoiceNumber: 'INV-2026-0040',
    patientName: 'Layla Al-Otaibi',
    mrn: 'P-10025',
    date: 'Today, 09:15 AM',
    totalAmount: 1400,
    insuranceShare: 0,
    patientShare: 1400,
    paidAmount: 700,
    status: 'partial',
    services: ['Dental Root Canal Treatment (Tooth #16)', 'Periapical Digital Radiograph']
  },
  {
    id: 'inv-3',
    invoiceNumber: 'INV-2026-0039',
    patientName: 'Omar Farooq',
    mrn: 'P-10026',
    date: 'Yesterday',
    totalAmount: 850,
    insuranceShare: 680,
    patientShare: 170,
    paidAmount: 0,
    status: 'insurance_pending',
    insuranceProvider: 'Tawuniya (VIP)',
    services: ['Digital Chest X-Ray PA', 'Spirometry Pulmonary Function', 'Consultation']
  },
  {
    id: 'inv-4',
    invoiceNumber: 'INV-2026-0038',
    patientName: 'Fatima Al-Shehri',
    mrn: 'P-10027',
    date: 'Sep 02, 2026',
    totalAmount: 350,
    insuranceShare: 0,
    patientShare: 350,
    paidAmount: 0,
    status: 'unpaid',
    services: ['Pediatric Specialist Consultation', 'Vaccination Administration']
  }
];

const AVAILABLE_SERVICES = [
  { name: 'Specialist Physician Consultation', price: 300, category: 'Consultation' },
  { name: 'Dental Root Canal Treatment', price: 1200, category: 'Dental' },
  { name: 'Comprehensive Metabolic Panel (CMP)', price: 450, category: 'Laboratory' },
  { name: 'Digital Chest X-Ray (PA View)', price: 400, category: 'Radiology' },
  { name: 'HbA1c Glycemic Test', price: 200, category: 'Laboratory' },
  { name: 'Dental Cleaning & Ultrasonic Scaling', price: 350, category: 'Dental' }
];

export default function BillingPage() {
  const [invoices, setInvoices] = useState<Invoice[]>(INITIAL_INVOICES);
  const [searchQuery, setSearchQuery] = useState('');
  const [statusFilter, setStatusFilter] = useState('all');
  const [showCreateModal, setShowCreateModal] = useState(false);

  // New Invoice Form States
  const [newPatient, setNewPatient] = useState('Tariq Al-Mansoor');
  const [newMrn, setNewMrn] = useState('P-10024');
  const [selectedServices, setSelectedServices] = useState<string[]>([AVAILABLE_SERVICES[0].name]);
  const [insuranceCoverage, setInsuranceCoverage] = useState<number>(80);

  const calculateTotals = () => {
    let subtotal = 0;
    selectedServices.forEach((sName) => {
      const match = AVAILABLE_SERVICES.find((s) => s.name === sName);
      if (match) subtotal += match.price;
    });
    const tax = subtotal * 0.15; // 15% VAT
    const total = subtotal + tax;
    const insuranceShare = (total * insuranceCoverage) / 100;
    const patientShare = total - insuranceShare;
    return { subtotal, tax, total, insuranceShare, patientShare };
  };

  const totals = calculateTotals();

  const handleCreateInvoice = (e: React.FormEvent) => {
    e.preventDefault();
    const newInv: Invoice = {
      id: `inv-${Date.now()}`,
      invoiceNumber: `INV-2026-00${invoices.length + 42}`,
      patientName: newPatient,
      mrn: newMrn,
      date: 'Just now',
      totalAmount: Math.round(totals.total),
      insuranceShare: Math.round(totals.insuranceShare),
      patientShare: Math.round(totals.patientShare),
      paidAmount: 0,
      status: insuranceCoverage > 0 ? 'insurance_pending' : 'unpaid',
      insuranceProvider: insuranceCoverage > 0 ? 'Bupa Arabia' : undefined,
      services: selectedServices
    };
    setInvoices([newInv, ...invoices]);
    setShowCreateModal(false);
  };

  const handleMarkAsPaid = (id: string) => {
    setInvoices(
      invoices.map((inv) =>
        inv.id === id ? { ...inv, paidAmount: inv.patientShare, status: 'paid' } : inv
      )
    );
  };

  const filteredInvoices = invoices.filter((inv) => {
    const matchesSearch =
      inv.patientName.toLowerCase().includes(searchQuery.toLowerCase()) ||
      inv.mrn.toLowerCase().includes(searchQuery.toLowerCase()) ||
      inv.invoiceNumber.toLowerCase().includes(searchQuery.toLowerCase());
    const matchesStatus = statusFilter === 'all' || inv.status === statusFilter;
    return matchesSearch && matchesStatus;
  });

  const totalInvoiced = invoices.reduce((acc, i) => acc + i.totalAmount, 0);
  const totalCollected = invoices.reduce((acc, i) => acc + i.paidAmount, 0);
  const totalPending = totalInvoiced - totalCollected;

  return (
    <RouteGuard anyPermissions={['Billing.Read', 'Billing.Create']}>
      <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
        <div>
          <div className="flex items-center gap-2">
            <span className="px-2 py-0.5 rounded text-[11px] font-bold bg-amber-500/20 text-amber-300 border border-amber-500/30">
              Module 33
            </span>
            <span className="text-xs text-slate-400">Financial Ledger & Insurance</span>
          </div>
          <h1 className="text-2xl md:text-3xl font-bold text-white mt-1 flex items-center gap-3">
            <ReceiptText className="w-8 h-8 text-amber-400" />
            Billing, Invoices & Claims Operations
          </h1>
          <p className="text-sm text-slate-400 mt-0.5">
            Real-time medical billing, insurance co-pay splits, and multi-tenant ledger tracking.
          </p>
        </div>

        <PermissionGate permission="Billing.Create">
          <button
            onClick={() => setShowCreateModal(true)}
            className="px-5 py-2.5 rounded-xl bg-gradient-to-r from-amber-500 to-orange-500 hover:from-amber-400 hover:to-orange-400 text-sm font-semibold text-white shadow-lg shadow-amber-500/20 flex items-center gap-2 transition"
          >
            <Plus className="w-4 h-4" />
            <span>Generate New Invoice</span>
          </button>
        </PermissionGate>
      </div>

      {/* Financial Metric Cards */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
        <div className="glass-panel p-5 rounded-2xl border-slate-800 flex items-center justify-between">
          <div>
            <div className="text-xs font-semibold text-slate-400">Total Billed (MTD)</div>
            <div className="text-2xl font-black text-white mt-1 font-mono">
              {totalInvoiced.toLocaleString()} <span className="text-xs font-normal text-slate-400">SAR</span>
            </div>
            <div className="text-[11px] text-emerald-400 flex items-center gap-1 mt-1 font-medium">
              <TrendingUp className="w-3 h-3" /> +14.2% vs last month
            </div>
          </div>
          <div className="w-12 h-12 rounded-xl bg-sky-500/10 border border-sky-500/30 flex items-center justify-center text-sky-400">
            <DollarSign className="w-6 h-6" />
          </div>
        </div>

        <div className="glass-panel p-5 rounded-2xl border-slate-800 flex items-center justify-between">
          <div>
            <div className="text-xs font-semibold text-slate-400">Collected Cash Revenue</div>
            <div className="text-2xl font-black text-emerald-400 mt-1 font-mono">
              {totalCollected.toLocaleString()} <span className="text-xs font-normal text-slate-400">SAR</span>
            </div>
            <div className="text-[11px] text-slate-400 mt-1">Settled at Point of Care</div>
          </div>
          <div className="w-12 h-12 rounded-xl bg-emerald-500/10 border border-emerald-500/30 flex items-center justify-center text-emerald-400">
            <CreditCard className="w-6 h-6" />
          </div>
        </div>

        <div className="glass-panel p-5 rounded-2xl border-slate-800 flex items-center justify-between">
          <div>
            <div className="text-xs font-semibold text-slate-400">Outstanding Receivable</div>
            <div className="text-2xl font-black text-rose-400 mt-1 font-mono">
              {totalPending.toLocaleString()} <span className="text-xs font-normal text-slate-400">SAR</span>
            </div>
            <div className="text-[11px] text-rose-400/80 mt-1">Pending clearance</div>
          </div>
          <div className="w-12 h-12 rounded-xl bg-rose-500/10 border border-rose-500/30 flex items-center justify-center text-rose-400">
            <AlertCircle className="w-6 h-6" />
          </div>
        </div>

        <div className="glass-panel p-5 rounded-2xl border-slate-800 flex items-center justify-between">
          <div>
            <div className="text-xs font-semibold text-slate-400">Insurance Claims Batch</div>
            <div className="text-2xl font-black text-indigo-400 mt-1 font-mono">
              1,440 <span className="text-xs font-normal text-slate-400">SAR</span>
            </div>
            <div className="text-[11px] text-indigo-300 mt-1">CHI / NPHIES Pre-authorized</div>
          </div>
          <div className="w-12 h-12 rounded-xl bg-indigo-500/10 border border-indigo-500/30 flex items-center justify-center text-indigo-400">
            <Building className="w-6 h-6" />
          </div>
        </div>
      </div>

      {/* Ledger Table & Filters */}
      <div className="glass-panel p-5 rounded-2xl border-slate-800 space-y-4">
        {/* Table Controls */}
        <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3">
          <div className="relative max-w-sm w-full">
            <Search className="w-4 h-4 absolute left-3 top-3 text-slate-400" />
            <input
              type="text"
              placeholder="Search by invoice #, patient, or MRN..."
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              className="w-full pl-9 pr-4 py-2 rounded-xl bg-slate-900 border border-slate-800 text-xs text-white placeholder-slate-500 focus:outline-none focus:border-amber-500"
            />
          </div>

          {/* Status Filter Tabs */}
          <div className="flex items-center gap-1 bg-slate-900/80 p-1 rounded-xl border border-slate-800 text-xs">
            {[
              { id: 'all', label: 'All Invoices' },
              { id: 'paid', label: 'Settled' },
              { id: 'partial', label: 'Partial' },
              { id: 'insurance_pending', label: 'Insurance Pending' },
              { id: 'unpaid', label: 'Unpaid' }
            ].map((tab) => (
              <button
                key={tab.id}
                onClick={() => setStatusFilter(tab.id)}
                className={`px-3 py-1 rounded-lg font-medium transition ${
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

        {/* Invoices Table */}
        <div className="overflow-x-auto">
          <table className="w-full text-left text-sm">
            <thead>
              <tr className="border-b border-slate-800 text-xs font-semibold text-slate-400 uppercase tracking-wider">
                <th className="py-3 px-3">Invoice #</th>
                <th className="py-3 px-3">Patient</th>
                <th className="py-3 px-3">Date</th>
                <th className="py-3 px-3">Services Included</th>
                <th className="py-3 px-3">Total (SAR)</th>
                <th className="py-3 px-3">Patient Co-Pay</th>
                <th className="py-3 px-3">Status</th>
                <th className="py-3 px-3 text-right">Actions</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-800/60">
              {filteredInvoices.map((inv) => (
                <tr key={inv.id} className="hover:bg-slate-900/40 transition">
                  <td className="py-3 px-3 font-mono font-bold text-sky-400 text-xs">
                    {inv.invoiceNumber}
                  </td>

                  <td className="py-3 px-3">
                    <div className="font-semibold text-slate-200 text-xs">{inv.patientName}</div>
                    <div className="text-[11px] text-slate-500 font-mono">{inv.mrn}</div>
                  </td>

                  <td className="py-3 px-3 text-xs text-slate-400 whitespace-nowrap">
                    {inv.date}
                  </td>

                  <td className="py-3 px-3 text-xs text-slate-300 max-w-xs truncate">
                    {inv.services.join(', ')}
                  </td>

                  <td className="py-3 px-3 font-mono font-bold text-white text-xs">
                    {inv.totalAmount.toLocaleString()}
                  </td>

                  <td className="py-3 px-3 font-mono text-xs text-amber-400 font-semibold">
                    {inv.patientShare.toLocaleString()} SAR
                  </td>

                  <td className="py-3 px-3 whitespace-nowrap">
                    {inv.status === 'paid' && (
                      <span className="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-xs font-medium bg-emerald-500/10 text-emerald-400 border border-emerald-500/20">
                        <CheckCircle2 className="w-3 h-3" /> Settled
                      </span>
                    )}
                    {inv.status === 'partial' && (
                      <span className="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-xs font-medium bg-cyan-500/10 text-cyan-400 border border-cyan-500/20">
                        <Clock className="w-3 h-3" /> Partially Paid
                      </span>
                    )}
                    {inv.status === 'insurance_pending' && (
                      <span className="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-xs font-medium bg-indigo-500/10 text-indigo-400 border border-indigo-500/20">
                        <Building className="w-3 h-3" /> Insurance Pending
                      </span>
                    )}
                    {inv.status === 'unpaid' && (
                      <span className="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-xs font-bold bg-rose-500/15 text-rose-400 border border-rose-500/30">
                        <AlertCircle className="w-3 h-3" /> Unpaid
                      </span>
                    )}
                  </td>

                  <td className="py-3 px-3 text-right whitespace-nowrap">
                    {inv.status !== 'paid' ? (
                      <button
                        onClick={() => handleMarkAsPaid(inv.id)}
                        className="px-3 py-1 rounded-lg bg-emerald-600 hover:bg-emerald-500 text-white text-xs font-semibold transition"
                      >
                        Settle Payment
                      </button>
                    ) : (
                      <button
                        onClick={() => window.print()}
                        className="px-3 py-1 rounded-lg bg-slate-800 hover:bg-slate-700 text-slate-300 text-xs font-medium transition inline-flex items-center gap-1"
                      >
                        <Printer className="w-3 h-3" /> Receipt
                      </button>
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>

      {/* Generate New Invoice Modal */}
      {showCreateModal && (
        <div className="fixed inset-0 z-50 bg-black/80 backdrop-blur-sm flex items-center justify-center p-4">
          <div className="glass-panel bg-slate-950 border border-slate-800 text-slate-100 max-w-xl w-full rounded-2xl p-6 shadow-2xl space-y-5 relative">
            <button
              onClick={() => setShowCreateModal(false)}
              className="absolute top-4 right-4 p-2 rounded-lg text-slate-400 hover:text-white transition"
            >
              <X className="w-5 h-5" />
            </button>

            <h2 className="text-lg font-bold text-white flex items-center gap-2">
              <ReceiptText className="w-5 h-5 text-amber-400" />
              Generate Clinical Encounter Invoice
            </h2>

            <form onSubmit={handleCreateInvoice} className="space-y-4 text-xs">
              <div className="grid grid-cols-2 gap-3">
                <div>
                  <label className="text-slate-400 block mb-1">Patient Name</label>
                  <input
                    type="text"
                    value={newPatient}
                    onChange={(e) => setNewPatient(e.target.value)}
                    className="w-full px-3 py-2 rounded-xl bg-slate-900 border border-slate-800 text-white focus:outline-none focus:border-amber-500"
                    required
                  />
                </div>
                <div>
                  <label className="text-slate-400 block mb-1">Medical Record # (MRN)</label>
                  <input
                    type="text"
                    value={newMrn}
                    onChange={(e) => setNewMrn(e.target.value)}
                    className="w-full px-3 py-2 rounded-xl bg-slate-900 border border-slate-800 text-white focus:outline-none focus:border-amber-500"
                    required
                  />
                </div>
              </div>

              <div>
                <label className="text-slate-400 block mb-1">Select Billable Procedures & Tests</label>
                <div className="space-y-1.5 max-h-40 overflow-y-auto pr-1">
                  {AVAILABLE_SERVICES.map((s) => {
                    const isSelected = selectedServices.includes(s.name);
                    return (
                      <div
                        key={s.name}
                        onClick={() => {
                          if (isSelected) {
                            setSelectedServices(selectedServices.filter((item) => item !== s.name));
                          } else {
                            setSelectedServices([...selectedServices, s.name]);
                          }
                        }}
                        className={`p-2 rounded-xl border cursor-pointer flex items-center justify-between transition ${
                          isSelected
                            ? 'bg-amber-500/15 border-amber-500/40 text-amber-200'
                            : 'bg-slate-900/40 border-slate-800 text-slate-300'
                        }`}
                      >
                        <div>
                          <div className="font-semibold">{s.name}</div>
                          <div className="text-[10px] text-slate-400">{s.category}</div>
                        </div>
                        <div className="font-mono font-bold">{s.price} SAR</div>
                      </div>
                    );
                  })}
                </div>
              </div>

              {/* Insurance Coverage Slider */}
              <div className="p-3.5 rounded-xl bg-slate-900/60 border border-slate-800 space-y-2">
                <div className="flex items-center justify-between">
                  <span className="text-slate-300 font-medium">Insurance Coverage (Co-pay Split)</span>
                  <span className="font-mono font-bold text-sky-400">{insuranceCoverage}%</span>
                </div>
                <input
                  type="range"
                  min="0"
                  max="100"
                  step="5"
                  value={insuranceCoverage}
                  onChange={(e) => setInsuranceCoverage(Number(e.target.value))}
                  className="w-full accent-amber-400 h-1.5 bg-slate-800 rounded-lg cursor-pointer"
                />
                <div className="flex justify-between text-[10px] text-slate-500">
                  <span>0% (Cash Self-Pay)</span>
                  <span>50%</span>
                  <span>80% (Bupa / Tawuniya)</span>
                  <span>100% (Full CHI)</span>
                </div>
              </div>

              {/* Total Summary */}
              <div className="p-3.5 rounded-xl bg-amber-500/10 border border-amber-500/20 text-xs space-y-1.5 font-mono">
                <div className="flex justify-between text-slate-400">
                  <span>Procedures Subtotal:</span>
                  <span>{totals.subtotal} SAR</span>
                </div>
                <div className="flex justify-between text-slate-400">
                  <span>VAT (15%):</span>
                  <span>{totals.tax.toFixed(1)} SAR</span>
                </div>
                <div className="flex justify-between text-white font-bold text-sm pt-1 border-t border-slate-800">
                  <span>Grand Total:</span>
                  <span>{totals.total.toFixed(1)} SAR</span>
                </div>
                <div className="flex justify-between text-indigo-400">
                  <span>Insurance Receivable ({insuranceCoverage}%):</span>
                  <span>{totals.insuranceShare.toFixed(1)} SAR</span>
                </div>
                <div className="flex justify-between text-amber-400 font-bold">
                  <span>Patient Co-Pay Due:</span>
                  <span>{totals.patientShare.toFixed(1)} SAR</span>
                </div>
              </div>

              <button
                type="submit"
                disabled={selectedServices.length === 0}
                className="w-full py-3 rounded-xl bg-gradient-to-r from-amber-500 to-orange-500 hover:from-amber-400 hover:to-orange-400 text-white font-bold text-xs shadow-lg shadow-amber-500/20 transition disabled:opacity-50"
              >
                Confirm & Issue Invoice
              </button>
            </form>
          </div>
        </div>
      )}
      </div>
    </RouteGuard>
  );
}
