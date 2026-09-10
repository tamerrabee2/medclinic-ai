'use client';

import React, { useState, useEffect, useMemo } from 'react';
import {
  Building2,
  Users,
  Sparkles,
  ScanLine,
  Activity,
  Plus,
  Search,
  Filter,
  ShieldCheck,
  AlertTriangle,
  CheckCircle2,
  XCircle,
  PauseCircle,
  PlayCircle,
  ArrowUpRight,
  TrendingUp,
  History,
  Layers,
  ChevronRight,
  Clock,
  RefreshCw,
  X,
  CreditCard,
  Sliders,
  FileText
} from 'lucide-react';
import { ApiClient } from '@/lib/api';
import { useAuth } from '@/lib/auth';

interface SuperAdminOverview {
  totalClinics: number;
  activeClinics: number;
  trialClinics: number;
  suspendedClinics: number;
  cancelledClinics: number;
  totalDoctors: number;
  totalPatients: number;
  totalAiRequests: number;
  totalDicomStudies: number;
  clinicsByTier: Record<string, number>;
}

interface TenantItem {
  id: string;
  name: string;
  slug: string;
  email?: string;
  phone?: string;
  city?: string;
  country?: string;
  lifecycleStatus: number | string;
  billingStatus: number | string;
  complianceStatus: number | string;
  trialEndsAt?: string;
  suspendedAt?: string;
  suspensionReason?: string;
  planCode: string;
  tier: number | string;
  doctorsCount: number;
  patientsCount: number;
  createdAt: string;
}

interface SubscriptionPlanItem {
  id: string;
  code: string;
  name: string;
  description?: string;
  tier: number | string;
  monthlyPrice: number;
  annualPrice: number;
  currency: string;
  maxDoctors: number;
  maxUsers: number;
  maxPatients: number;
  maxStorageBytes: number;
  monthlyAiRequestsLimit: number;
  maxDicomStudiesMonthly: number;
  featuresJson: string;
}

interface LifecycleAuditEvent {
  id: string;
  clinicId: string;
  eventType: number;
  eventTypeName: string;
  oldValue?: string;
  newValue?: string;
  reason?: string;
  performedByUserName?: string;
  timestamp: string;
  ipAddress?: string;
}

interface TenantDetailData {
  clinic: any;
  subscription: {
    clinicId: string;
    clinicName: string;
    lifecycleStatus: number | string;
    billingStatus: number | string;
    complianceStatus: number | string;
    suspensionReason?: string;
    planCode: string;
    planName: string;
    tier: number | string;
    billingCycle: number | string;
    subscriptionStatus: number | string;
    trialEndsAt?: string;
    enabledFeatures: string[];
    quotas: Array<{
      metricType: number;
      metricName: string;
      currentValue: number;
      quotaLimit: number;
      usagePercentage: number;
      isExceeded: boolean;
    }>;
  };
  recentAuditEvents: LifecycleAuditEvent[];
}

export default function SuperAdminPage() {
  const { user } = useAuth();
  const [activeTab, setActiveTab] = useState<'overview' | 'tenants' | 'plans'>('overview');
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);

  // Data States
  const [overview, setOverview] = useState<SuperAdminOverview | null>(null);
  const [tenants, setTenants] = useState<TenantItem[]>([]);
  const [plans, setPlans] = useState<SubscriptionPlanItem[]>([]);

  // Search & Filters
  const [searchQuery, setSearchQuery] = useState('');
  const [statusFilter, setStatusFilter] = useState<'all' | 'Active' | 'Trial' | 'Suspended' | 'Cancelled'>('all');

  // Modals
  const [isProvisionOpen, setIsProvisionOpen] = useState(false);
  const [isDetailOpen, setIsDetailOpen] = useState(false);
  const [isSuspendOpen, setIsSuspendOpen] = useState(false);
  const [selectedTenant, setSelectedTenant] = useState<TenantItem | null>(null);
  const [tenantDetail, setTenantDetail] = useState<TenantDetailData | null>(null);
  const [detailLoading, setDetailLoading] = useState(false);

  // Form State: Provisioning
  const [provName, setProvName] = useState('');
  const [provSlug, setProvSlug] = useState('');
  const [provEmail, setProvEmail] = useState('');
  const [provPhone, setProvPhone] = useState('');
  const [provCity, setProvCity] = useState('');
  const [provCountry, setProvCountry] = useState('US');
  const [provPlan, setProvPlan] = useState('pro');
  const [provCycle, setProvCycle] = useState<1 | 2>(1); // 1 = Monthly, 2 = Annual
  const [provTrialDays, setProvTrialDays] = useState(14);
  const [adminFirst, setAdminFirst] = useState('');
  const [adminLast, setAdminLast] = useState('');
  const [adminEmail, setAdminEmail] = useState('');
  const [adminPassword, setAdminPassword] = useState('Admin@123!');
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [formError, setFormError] = useState<string | null>(null);
  const [suspendReason, setSuspendReason] = useState('');

  // Auto-generate slug from name
  const handleNameChange = (val: string) => {
    setProvName(val);
    const slug = val
      .toLowerCase()
      .trim()
      .replace(/[^a-z0-9]+/g, '-')
      .replace(/^-+|-+$/g, '');
    setProvSlug(slug);
  };

  const fetchData = async () => {
    setRefreshing(true);
    try {
      // 1. Overview
      const ovRes = await ApiClient.request<{ success: boolean; data: SuperAdminOverview }>('/api/v1/superadmin/overview')
        .catch(() => null);

      if (ovRes?.data) {
        setOverview(ovRes.data);
      } else {
        // Mock fallback for preview if backend is spinning up
        setOverview({
          totalClinics: 12,
          activeClinics: 9,
          trialClinics: 2,
          suspendedClinics: 1,
          cancelledClinics: 0,
          totalDoctors: 48,
          totalPatients: 3820,
          totalAiRequests: 14250,
          totalDicomStudies: 620,
          clinicsByTier: { Pro: 8, Basic: 3, Enterprise: 1 }
        });
      }

      // 2. Tenants
      const tenRes = await ApiClient.request<{ success: boolean; data: { items: TenantItem[] } }>('/api/v1/superadmin/tenants?pageSize=50')
        .catch(() => null);

      if (tenRes?.data?.items) {
        setTenants(tenRes.data.items);
      } else {
        setTenants([
          {
            id: 'c1',
            name: 'Al-Amal Medical Center',
            slug: 'al-amal',
            email: 'admin@alamal.com',
            city: 'Cairo',
            country: 'EG',
            lifecycleStatus: 2, // Active
            billingStatus: 1,
            complianceStatus: 1,
            planCode: 'pro',
            tier: 2,
            doctorsCount: 12,
            patientsCount: 1450,
            createdAt: new Date().toISOString()
          },
          {
            id: 'c2',
            name: 'Nile Heart Hospital',
            slug: 'nile-heart',
            email: 'contact@nileheart.com',
            city: 'Alexandria',
            country: 'EG',
            lifecycleStatus: 1, // Trial
            billingStatus: 1,
            complianceStatus: 1,
            trialEndsAt: new Date(Date.now() + 10 * 86400000).toISOString(),
            planCode: 'pro',
            tier: 2,
            doctorsCount: 6,
            patientsCount: 310,
            createdAt: new Date().toISOString()
          },
          {
            id: 'c3',
            name: 'Oasis Dental Practice',
            slug: 'oasis-dental',
            email: 'info@oasisdental.com',
            city: 'Dubai',
            country: 'AE',
            lifecycleStatus: 3, // Suspended
            billingStatus: 2,
            complianceStatus: 1,
            suspendedAt: new Date().toISOString(),
            suspensionReason: 'Overdue invoice settlement',
            planCode: 'basic',
            tier: 1,
            doctorsCount: 2,
            patientsCount: 120,
            createdAt: new Date().toISOString()
          }
        ]);
      }

      // 3. Plans
      const planRes = await ApiClient.request<{ success: boolean; data: SubscriptionPlanItem[] }>('/api/v1/superadmin/plans')
        .catch(() => null);

      if (planRes?.data) {
        setPlans(planRes.data);
      }
    } finally {
      setLoading(false);
      setRefreshing(false);
    }
  };

  useEffect(() => {
    fetchData();
  }, []);

  const openTenantDetail = async (tenant: TenantItem) => {
    setSelectedTenant(tenant);
    setIsDetailOpen(true);
    setDetailLoading(true);
    try {
      const res = await ApiClient.request<{ success: boolean; data: TenantDetailData }>(`/api/v1/superadmin/tenants/${tenant.id}`)
        .catch(() => null);
      if (res?.data) {
        setTenantDetail(res.data);
      } else {
        // Mock fallback
        setTenantDetail({
          clinic: tenant,
          subscription: {
            clinicId: tenant.id,
            clinicName: tenant.name,
            lifecycleStatus: tenant.lifecycleStatus,
            billingStatus: tenant.billingStatus,
            complianceStatus: tenant.complianceStatus,
            planCode: tenant.planCode,
            planName: tenant.planCode.toUpperCase() + ' Tier',
            tier: tenant.tier,
            billingCycle: 1,
            subscriptionStatus: 2,
            enabledFeatures: ['ai_copilot', 'dicom_pacs', 'external_labs', 'patient_portal'],
            quotas: [
              { metricType: 2, metricName: 'Doctors', currentValue: tenant.doctorsCount, quotaLimit: 15, usagePercentage: (tenant.doctorsCount / 15) * 100, isExceeded: false },
              { metricType: 3, metricName: 'Patients', currentValue: tenant.patientsCount, quotaLimit: 5000, usagePercentage: (tenant.patientsCount / 5000) * 100, isExceeded: false },
              { metricType: 5, metricName: 'AI Inferences', currentValue: 840, quotaLimit: 1500, usagePercentage: 56.0, isExceeded: false },
              { metricType: 6, metricName: 'DICOM Studies', currentValue: 34, quotaLimit: 100, usagePercentage: 34.0, isExceeded: false }
            ]
          },
          recentAuditEvents: [
            { id: '1', clinicId: tenant.id, eventType: 1, eventTypeName: 'Created', newValue: 'Provisioned with Pro Plan', timestamp: tenant.createdAt },
            ...(tenant.suspendedAt ? [{ id: '2', clinicId: tenant.id, eventType: 4, eventTypeName: 'Suspended', reason: tenant.suspensionReason, timestamp: tenant.suspendedAt }] : [])
          ]
        });
      }
    } finally {
      setDetailLoading(false);
    }
  };

  const handleProvisionSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsSubmitting(true);
    setFormError(null);

    try {
      const payload = {
        name: provName,
        slug: provSlug,
        email: provEmail || undefined,
        phone: provPhone || undefined,
        city: provCity || undefined,
        country: provCountry,
        planCode: provPlan,
        billingCycle: provCycle,
        trialDays: provTrialDays,
        adminUser: adminEmail ? {
          firstName: adminFirst || 'Clinic',
          lastName: adminLast || 'Admin',
          email: adminEmail,
          password: adminPassword
        } : undefined
      };

      await ApiClient.request('/api/v1/superadmin/tenants/provision', {
        method: 'POST',
        body: JSON.stringify(payload)
      });

      setIsProvisionOpen(false);
      // Reset form
      setProvName('');
      setProvSlug('');
      setProvEmail('');
      setAdminEmail('');
      fetchData();
    } catch (err: any) {
      setFormError(err?.message || 'Failed to provision clinic.');
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleSuspendTenant = async () => {
    if (!selectedTenant) return;
    setIsSubmitting(true);
    try {
      await ApiClient.request(`/api/v1/superadmin/tenants/${selectedTenant.id}/suspend`, {
        method: 'POST',
        body: JSON.stringify({ reason: suspendReason || 'Administrative suspension' })
      });
      setIsSuspendOpen(false);
      setSuspendReason('');
      fetchData();
    } catch (err: any) {
      alert(err?.message || 'Failed to suspend clinic.');
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleReactivateTenant = async (tenantId: string) => {
    if (!confirm('Are you sure you want to reactivate operational access for this clinic?')) return;
    try {
      await ApiClient.request(`/api/v1/superadmin/tenants/${tenantId}/reactivate`, {
        method: 'POST',
        body: JSON.stringify({ reason: 'Reactivated by Super Administrator' })
      });
      fetchData();
    } catch (err: any) {
      alert(err?.message || 'Failed to reactivate clinic.');
    }
  };

  const filteredTenants = useMemo(() => {
    return tenants.filter(t => {
      const matchesSearch = !searchQuery ||
        t.name.toLowerCase().includes(searchQuery.toLowerCase()) ||
        t.slug.toLowerCase().includes(searchQuery.toLowerCase()) ||
        (t.email && t.email.toLowerCase().includes(searchQuery.toLowerCase()));

      let matchesStatus = true;
      if (statusFilter === 'Active') matchesStatus = t.lifecycleStatus === 2 || String(t.lifecycleStatus).toLowerCase() === 'active';
      else if (statusFilter === 'Trial') matchesStatus = t.lifecycleStatus === 1 || String(t.lifecycleStatus).toLowerCase() === 'trial';
      else if (statusFilter === 'Suspended') matchesStatus = t.lifecycleStatus === 3 || String(t.lifecycleStatus).toLowerCase() === 'suspended';
      else if (statusFilter === 'Cancelled') matchesStatus = t.lifecycleStatus === 4 || String(t.lifecycleStatus).toLowerCase() === 'cancelled';

      return matchesSearch && matchesStatus;
    });
  }, [tenants, searchQuery, statusFilter]);

  const getStatusBadge = (status: number | string) => {
    const s = String(status).toLowerCase();
    if (s === '2' || s === 'active') {
      return <span className="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-xs font-semibold bg-emerald-500/10 text-emerald-400 border border-emerald-500/20"><CheckCircle2 className="w-3 h-3" /> Active</span>;
    }
    if (s === '1' || s === 'trial') {
      return <span className="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-xs font-semibold bg-amber-500/10 text-amber-400 border border-amber-500/20"><Clock className="w-3 h-3" /> Trial</span>;
    }
    if (s === '3' || s === 'suspended') {
      return <span className="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-xs font-semibold bg-rose-500/10 text-rose-400 border border-rose-500/20"><PauseCircle className="w-3 h-3" /> Suspended</span>;
    }
    return <span className="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-xs font-semibold bg-slate-500/10 text-slate-400 border border-slate-500/20"><XCircle className="w-3 h-3" /> Cancelled</span>;
  };

  return (
    <div className="p-6 max-w-7xl mx-auto space-y-6 animate-fade-in">
      {/* Header Banner */}
      <div className="glass-panel p-6 rounded-2xl border-slate-800 flex flex-col md:flex-row items-start md:items-center justify-between gap-4 bg-gradient-to-r from-slate-950 via-indigo-950/20 to-slate-950">
        <div>
          <div className="flex items-center gap-2.5">
            <span className="p-2 rounded-xl bg-indigo-500/20 text-indigo-400 border border-indigo-500/30">
              <ShieldCheck className="w-6 h-6" />
            </span>
            <div>
              <h1 className="text-xl font-bold text-white tracking-tight">Super Admin Platform Console</h1>
              <p className="text-xs text-slate-400 mt-0.5">Multi-Tenant Governance, Lifecycle Orchestration, and Entitlement Enforcement</p>
            </div>
          </div>
        </div>

        <div className="flex items-center gap-3 w-full md:w-auto">
          <button
            onClick={fetchData}
            disabled={refreshing}
            className="px-3.5 py-2 rounded-xl bg-slate-900 border border-slate-800 text-slate-300 hover:text-white hover:bg-slate-800 transition flex items-center gap-2 text-xs font-medium"
            id="btn-refresh-superadmin"
          >
            <RefreshCw className={`w-3.5 h-3.5 ${refreshing ? 'animate-spin' : ''}`} />
            Refresh
          </button>

          <button
            onClick={() => setIsProvisionOpen(true)}
            className="px-4 py-2 rounded-xl bg-gradient-to-r from-sky-500 to-indigo-600 hover:from-sky-400 hover:to-indigo-500 text-white font-medium text-xs shadow-lg shadow-sky-500/25 transition flex items-center gap-2"
            id="btn-open-provision-modal"
          >
            <Plus className="w-4 h-4" />
            Provision Clinic
          </button>
        </div>
      </div>

      {/* Navigation Tabs */}
      <div className="flex items-center gap-2 border-b border-slate-800/80 pb-2">
        <button
          onClick={() => setActiveTab('overview')}
          className={`px-4 py-2 rounded-xl text-xs font-semibold transition ${
            activeTab === 'overview'
              ? 'bg-sky-500/15 text-sky-400 border border-sky-500/30 shadow-sm'
              : 'text-slate-400 hover:text-slate-200 hover:bg-slate-900/50'
          }`}
          id="tab-superadmin-overview"
        >
          <div className="flex items-center gap-2">
            <Activity className="w-3.5 h-3.5" />
            Platform Overview
          </div>
        </button>

        <button
          onClick={() => setActiveTab('tenants')}
          className={`px-4 py-2 rounded-xl text-xs font-semibold transition ${
            activeTab === 'tenants'
              ? 'bg-sky-500/15 text-sky-400 border border-sky-500/30 shadow-sm'
              : 'text-slate-400 hover:text-slate-200 hover:bg-slate-900/50'
          }`}
          id="tab-superadmin-tenants"
        >
          <div className="flex items-center gap-2">
            <Building2 className="w-3.5 h-3.5" />
            Tenant Clinics ({tenants.length})
          </div>
        </button>

        <button
          onClick={() => setActiveTab('plans')}
          className={`px-4 py-2 rounded-xl text-xs font-semibold transition ${
            activeTab === 'plans'
              ? 'bg-sky-500/15 text-sky-400 border border-sky-500/30 shadow-sm'
              : 'text-slate-400 hover:text-slate-200 hover:bg-slate-900/50'
          }`}
          id="tab-superadmin-plans"
        >
          <div className="flex items-center gap-2">
            <CreditCard className="w-3.5 h-3.5" />
            Subscription Catalog
          </div>
        </button>
      </div>

      {/* TAB 1: OVERVIEW */}
      {activeTab === 'overview' && overview && (
        <div className="space-y-6">
          {/* KPI Stat Cards */}
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
            <div className="glass-panel p-4 rounded-xl border-slate-800 relative overflow-hidden" id="stat-total-clinics">
              <div className="flex items-center justify-between">
                <span className="text-xs font-medium text-slate-400">Total Clinics</span>
                <span className="p-2 rounded-lg bg-sky-500/10 text-sky-400 border border-sky-500/20"><Building2 className="w-4 h-4" /></span>
              </div>
              <div className="text-2xl font-bold text-white mt-2">{overview.totalClinics}</div>
              <div className="flex items-center gap-2 mt-2 text-[11px] text-slate-400">
                <span className="text-emerald-400 font-medium">{overview.activeClinics} Active</span> •
                <span className="text-amber-400 font-medium">{overview.trialClinics} Trial</span> •
                <span className="text-rose-400 font-medium">{overview.suspendedClinics} Suspended</span>
              </div>
            </div>

            <div className="glass-panel p-4 rounded-xl border-slate-800 relative overflow-hidden" id="stat-total-doctors">
              <div className="flex items-center justify-between">
                <span className="text-xs font-medium text-slate-400">Healthcare Providers</span>
                <span className="p-2 rounded-lg bg-indigo-500/10 text-indigo-400 border border-indigo-500/20"><Users className="w-4 h-4" /></span>
              </div>
              <div className="text-2xl font-bold text-white mt-2">{overview.totalDoctors}</div>
              <div className="text-[11px] text-slate-400 mt-2">Platform clinical specialists</div>
            </div>

            <div className="glass-panel p-4 rounded-xl border-slate-800 relative overflow-hidden" id="stat-ai-inferences">
              <div className="flex items-center justify-between">
                <span className="text-xs font-medium text-slate-400">Monthly AI Inferences</span>
                <span className="p-2 rounded-lg bg-purple-500/10 text-purple-400 border border-purple-500/20"><Sparkles className="w-4 h-4" /></span>
              </div>
              <div className="text-2xl font-bold text-white mt-2">{overview.totalAiRequests.toLocaleString()}</div>
              <div className="text-[11px] text-slate-400 mt-2">Clinical decision support & copilot</div>
            </div>

            <div className="glass-panel p-4 rounded-xl border-slate-800 relative overflow-hidden" id="stat-dicom-studies">
              <div className="flex items-center justify-between">
                <span className="text-xs font-medium text-slate-400">PACS DICOM Studies</span>
                <span className="p-2 rounded-lg bg-emerald-500/10 text-emerald-400 border border-emerald-500/20"><ScanLine className="w-4 h-4" /></span>
              </div>
              <div className="text-2xl font-bold text-white mt-2">{overview.totalDicomStudies.toLocaleString()}</div>
              <div className="text-[11px] text-slate-400 mt-2">Radiology imaging series processed</div>
            </div>
          </div>

          {/* Tier Distribution Card */}
          <div className="glass-panel p-5 rounded-2xl border-slate-800 space-y-4">
            <h3 className="text-sm font-semibold text-white flex items-center gap-2">
              <Layers className="w-4 h-4 text-sky-400" />
              Active Subscriptions by Tier
            </h3>

            <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
              <div className="p-4 rounded-xl bg-slate-900/60 border border-slate-800 flex items-center justify-between">
                <div>
                  <div className="text-xs font-bold text-slate-400 uppercase tracking-wider">Basic Tier</div>
                  <div className="text-xl font-bold text-white mt-1">{overview.clinicsByTier['Basic'] || 0} Clinics</div>
                  <div className="text-[11px] text-slate-500">$49/mo • Solo Practices</div>
                </div>
                <div className="w-10 h-10 rounded-full bg-slate-800 border border-slate-700 flex items-center justify-center text-slate-300 font-bold">
                  B
                </div>
              </div>

              <div className="p-4 rounded-xl bg-slate-900/60 border border-sky-500/30 flex items-center justify-between relative overflow-hidden">
                <div className="absolute top-0 right-0 w-16 h-16 bg-sky-500/10 rounded-full blur-xl pointer-events-none" />
                <div>
                  <div className="text-xs font-bold text-sky-400 uppercase tracking-wider">Pro Tier</div>
                  <div className="text-xl font-bold text-white mt-1">{overview.clinicsByTier['Pro'] || 0} Clinics</div>
                  <div className="text-[11px] text-slate-400">$149/mo • AI Copilot + DICOM</div>
                </div>
                <div className="w-10 h-10 rounded-full bg-sky-500/20 border border-sky-500/40 flex items-center justify-center text-sky-300 font-bold">
                  P
                </div>
              </div>

              <div className="p-4 rounded-xl bg-slate-900/60 border border-indigo-500/30 flex items-center justify-between relative overflow-hidden">
                <div className="absolute top-0 right-0 w-16 h-16 bg-indigo-500/10 rounded-full blur-xl pointer-events-none" />
                <div>
                  <div className="text-xs font-bold text-indigo-400 uppercase tracking-wider">Enterprise Tier</div>
                  <div className="text-xl font-bold text-white mt-1">{overview.clinicsByTier['Enterprise'] || 0} Clinics</div>
                  <div className="text-[11px] text-slate-400">$499/mo • Unlimited Polyclinics</div>
                </div>
                <div className="w-10 h-10 rounded-full bg-indigo-500/20 border border-indigo-500/40 flex items-center justify-center text-indigo-300 font-bold">
                  E
                </div>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* TAB 2: TENANT DIRECTORY */}
      {activeTab === 'tenants' && (
        <div className="space-y-4">
          {/* Controls Bar */}
          <div className="glass-panel p-4 rounded-xl border-slate-800 flex flex-col md:flex-row items-center justify-between gap-4">
            <div className="relative w-full md:w-80">
              <Search className="w-4 h-4 text-slate-500 absolute left-3.5 top-1/2 -translate-y-1/2" />
              <input
                type="text"
                placeholder="Search clinics by name, slug, or email..."
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                className="w-full bg-slate-900 border border-slate-800 rounded-xl pl-10 pr-3.5 py-2 text-xs text-white placeholder-slate-500 focus:outline-none focus:border-sky-500 transition"
                id="tenant-search-input"
              />
            </div>

            {/* Filter Pills */}
            <div className="flex items-center gap-1.5 w-full md:w-auto overflow-x-auto">
              {(['all', 'Active', 'Trial', 'Suspended', 'Cancelled'] as const).map((filter) => (
                <button
                  key={filter}
                  onClick={() => setStatusFilter(filter)}
                  className={`px-3 py-1.5 rounded-lg text-xs font-medium transition shrink-0 ${
                    statusFilter === filter
                      ? 'bg-sky-500/20 text-sky-400 border border-sky-500/30'
                      : 'text-slate-400 hover:text-white hover:bg-slate-800/50'
                  }`}
                  id={`filter-status-${filter.toLowerCase()}`}
                >
                  {filter}
                </button>
              ))}
            </div>
          </div>

          {/* Tenants Table */}
          <div className="glass-panel rounded-2xl border-slate-800 overflow-hidden">
            <div className="overflow-x-auto">
              <table className="w-full text-left text-xs text-slate-300" id="table-tenants">
                <thead className="bg-slate-900/80 text-slate-400 uppercase tracking-wider text-[10px] font-mono border-b border-slate-800">
                  <tr>
                    <th className="py-3.5 px-4">Clinic / Tenant</th>
                    <th className="py-3.5 px-4">Plan & Tier</th>
                    <th className="py-3.5 px-4">Lifecycle Status</th>
                    <th className="py-3.5 px-4">Capacity</th>
                    <th className="py-3.5 px-4">Created</th>
                    <th className="py-3.5 px-4 text-right">Actions</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-slate-800/60">
                  {filteredTenants.length === 0 ? (
                    <tr>
                      <td colSpan={6} className="py-8 text-center text-slate-500">
                        No clinic tenants match the current query or filter.
                      </td>
                    </tr>
                  ) : (
                    filteredTenants.map((t) => {
                      const isSuspended = t.lifecycleStatus === 3 || String(t.lifecycleStatus).toLowerCase() === 'suspended';

                      return (
                        <tr key={t.id} className="hover:bg-slate-900/40 transition">
                          <td className="py-3.5 px-4">
                            <div className="font-semibold text-white">{t.name}</div>
                            <div className="text-[11px] text-slate-500 font-mono">
                              /{t.slug} • {t.city || 'Global'}{t.country ? `, ${t.country}` : ''}
                            </div>
                          </td>

                          <td className="py-3.5 px-4">
                            <span className="px-2 py-0.5 rounded-md text-[11px] font-bold bg-indigo-500/10 text-indigo-400 border border-indigo-500/20 uppercase font-mono">
                              {t.planCode || 'Pro'}
                            </span>
                          </td>

                          <td className="py-3.5 px-4">
                            {getStatusBadge(t.lifecycleStatus)}
                            {t.trialEndsAt && (
                              <div className="text-[10px] text-amber-500/80 mt-1">
                                Ends: {new Date(t.trialEndsAt).toLocaleDateString()}
                              </div>
                            )}
                            {t.suspensionReason && (
                              <div className="text-[10px] text-rose-400/80 mt-0.5 truncate max-w-[180px]" title={t.suspensionReason}>
                                Reason: {t.suspensionReason}
                              </div>
                            )}
                          </td>

                          <td className="py-3.5 px-4">
                            <div className="text-slate-300 font-mono">
                              {t.doctorsCount} Drs • {t.patientsCount} Pts
                            </div>
                          </td>

                          <td className="py-3.5 px-4 text-slate-400">
                            {new Date(t.createdAt).toLocaleDateString()}
                          </td>

                          <td className="py-3.5 px-4 text-right">
                            <div className="flex items-center justify-end gap-1.5">
                              <button
                                onClick={() => openTenantDetail(t)}
                                className="px-2.5 py-1.5 rounded-lg bg-slate-800 hover:bg-slate-700 text-slate-200 text-xs font-medium transition"
                                id={`btn-view-tenant-${t.id}`}
                              >
                                Details & Audit
                              </button>

                              {isSuspended ? (
                                <button
                                  onClick={() => handleReactivateTenant(t.id)}
                                  className="px-2.5 py-1.5 rounded-lg bg-emerald-500/10 hover:bg-emerald-500/20 text-emerald-400 border border-emerald-500/30 text-xs font-medium transition flex items-center gap-1"
                                  id={`btn-reactivate-${t.id}`}
                                >
                                  <PlayCircle className="w-3.5 h-3.5" />
                                  Reactivate
                                </button>
                              ) : (
                                <button
                                  onClick={() => {
                                    setSelectedTenant(t);
                                    setIsSuspendOpen(true);
                                  }}
                                  className="px-2.5 py-1.5 rounded-lg bg-rose-500/10 hover:bg-rose-500/20 text-rose-400 border border-rose-500/30 text-xs font-medium transition flex items-center gap-1"
                                  id={`btn-suspend-${t.id}`}
                                >
                                  <PauseCircle className="w-3.5 h-3.5" />
                                  Suspend
                                </button>
                              )}
                            </div>
                          </td>
                        </tr>
                      );
                    })
                  )}
                </tbody>
              </table>
            </div>
          </div>
        </div>
      )}

      {/* TAB 3: PLAN CATALOG */}
      {activeTab === 'plans' && (
        <div className="space-y-4">
          <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
            {(plans.length > 0 ? plans : [
              {
                id: '1',
                code: 'basic',
                name: 'Basic Clinic',
                description: 'Essential clinical workflow for solo practices and small clinics.',
                tier: 1,
                monthlyPrice: 49,
                annualPrice: 490,
                currency: 'USD',
                maxDoctors: 3,
                maxUsers: 5,
                maxPatients: 500,
                maxStorageBytes: 5368709120,
                monthlyAiRequestsLimit: 100,
                maxDicomStudiesMonthly: 0,
                featuresJson: '["patient_portal"]'
              },
              {
                id: '2',
                code: 'pro',
                name: 'Professional Clinic',
                description: 'Advanced AI decision support, DICOM PACS, and high throughput.',
                tier: 2,
                monthlyPrice: 149,
                annualPrice: 1490,
                currency: 'USD',
                maxDoctors: 15,
                maxUsers: 25,
                maxPatients: 5000,
                maxStorageBytes: 53687091200,
                monthlyAiRequestsLimit: 1500,
                maxDicomStudiesMonthly: 100,
                featuresJson: '["ai_copilot","dicom_pacs","external_labs","patient_portal","advanced_reports"]'
              },
              {
                id: '3',
                code: 'enterprise',
                name: 'Enterprise Hospital',
                description: 'Unlimited capacity, dedicated infrastructure, API access, and white-labeling.',
                tier: 3,
                monthlyPrice: 499,
                annualPrice: 4990,
                currency: 'USD',
                maxDoctors: 0,
                maxUsers: 0,
                maxPatients: 0,
                maxStorageBytes: 0,
                monthlyAiRequestsLimit: 0,
                maxDicomStudiesMonthly: 0,
                featuresJson: '["ai_copilot","dicom_pacs","dental","external_labs","insurance","patient_portal","advanced_reports","custom_branding","api_access"]'
              }
            ]).map((p) => {
              let features: string[] = [];
              try {
                features = JSON.parse(p.featuresJson);
              } catch {}

              return (
                <div key={p.id} className="glass-panel p-6 rounded-2xl border-slate-800 flex flex-col justify-between relative overflow-hidden">
                  <div>
                    <div className="flex items-center justify-between">
                      <span className="text-xs font-bold text-sky-400 uppercase font-mono">{p.code}</span>
                      <span className="px-2 py-0.5 rounded-full text-[10px] font-semibold bg-emerald-500/10 text-emerald-400 border border-emerald-500/20">Active</span>
                    </div>
                    <h3 className="text-lg font-bold text-white mt-2">{p.name}</h3>
                    <p className="text-xs text-slate-400 mt-1">{p.description}</p>

                    <div className="mt-4 pt-4 border-t border-slate-800">
                      <div className="text-2xl font-black text-white">
                        ${p.monthlyPrice} <span className="text-xs font-normal text-slate-400">/ month</span>
                      </div>
                      <div className="text-[11px] text-slate-500 mt-0.5">
                        ${p.annualPrice} / year (save 2 months)
                      </div>
                    </div>

                    <div className="mt-4 space-y-2 text-xs">
                      <div className="flex justify-between py-1 border-b border-slate-800/40 text-slate-300">
                        <span>Doctors:</span>
                        <span className="font-mono font-semibold">{p.maxDoctors === 0 ? 'Unlimited' : p.maxDoctors}</span>
                      </div>
                      <div className="flex justify-between py-1 border-b border-slate-800/40 text-slate-300">
                        <span>Patients:</span>
                        <span className="font-mono font-semibold">{p.maxPatients === 0 ? 'Unlimited' : p.maxPatients}</span>
                      </div>
                      <div className="flex justify-between py-1 border-b border-slate-800/40 text-slate-300">
                        <span>Monthly AI Inferences:</span>
                        <span className="font-mono font-semibold">{p.monthlyAiRequestsLimit === 0 ? 'Unlimited' : p.monthlyAiRequestsLimit}</span>
                      </div>
                      <div className="flex justify-between py-1 border-b border-slate-800/40 text-slate-300">
                        <span>DICOM Studies:</span>
                        <span className="font-mono font-semibold">{p.maxDicomStudiesMonthly === 0 ? (p.tier === 1 ? 'None' : 'Unlimited') : p.maxDicomStudiesMonthly}</span>
                      </div>
                    </div>

                    <div className="mt-4">
                      <div className="text-[10px] font-bold text-slate-400 uppercase font-mono mb-2">Included Modules:</div>
                      <div className="flex flex-wrap gap-1.5">
                        {features.map((f) => (
                          <span key={f} className="px-2 py-0.5 rounded-md bg-slate-900 border border-slate-800 text-[11px] text-slate-300">
                            {f.replace('_', ' ')}
                          </span>
                        ))}
                      </div>
                    </div>
                  </div>
                </div>
              );
            })}
          </div>
        </div>
      )}

      {/* MODAL: PROVISION NEW CLINIC */}
      {isProvisionOpen && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-slate-950/80 backdrop-blur-sm" id="modal-provision-clinic">
          <div className="glass-panel w-full max-w-2xl rounded-2xl border-slate-800 max-h-[90vh] overflow-y-auto p-6 space-y-5">
            <div className="flex items-center justify-between border-b border-slate-800 pb-4">
              <div className="flex items-center gap-2">
                <Building2 className="w-5 h-5 text-sky-400" />
                <h2 className="text-base font-bold text-white">Provision New Clinic Tenant</h2>
              </div>
              <button onClick={() => setIsProvisionOpen(false)} className="text-slate-400 hover:text-white">
                <X className="w-5 h-5" />
              </button>
            </div>

            {formError && (
              <div className="p-3 rounded-xl bg-rose-500/10 border border-rose-500/20 text-rose-400 text-xs flex items-center gap-2">
                <AlertTriangle className="w-4 h-4 shrink-0" />
                <span>{formError}</span>
              </div>
            )}

            <form onSubmit={handleProvisionSubmit} className="space-y-4 text-xs">
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div>
                  <label className="block text-slate-400 font-medium mb-1">Clinic Name *</label>
                  <input
                    type="text"
                    required
                    placeholder="e.g. Al-Nour Polyclinic"
                    value={provName}
                    onChange={(e) => handleNameChange(e.target.value)}
                    className="w-full bg-slate-900 border border-slate-800 rounded-xl px-3 py-2 text-white placeholder-slate-600 focus:outline-none focus:border-sky-500"
                    id="input-clinic-name"
                  />
                </div>

                <div>
                  <label className="block text-slate-400 font-medium mb-1">Subdomain / Slug *</label>
                  <input
                    type="text"
                    required
                    placeholder="al-nour"
                    value={provSlug}
                    onChange={(e) => setProvSlug(e.target.value)}
                    className="w-full bg-slate-900 border border-slate-800 rounded-xl px-3 py-2 text-white placeholder-slate-600 focus:outline-none focus:border-sky-500 font-mono"
                    id="input-clinic-slug"
                  />
                </div>
              </div>

              <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                <div>
                  <label className="block text-slate-400 font-medium mb-1">Email</label>
                  <input
                    type="email"
                    placeholder="contact@clinic.com"
                    value={provEmail}
                    onChange={(e) => setProvEmail(e.target.value)}
                    className="w-full bg-slate-900 border border-slate-800 rounded-xl px-3 py-2 text-white placeholder-slate-600 focus:outline-none focus:border-sky-500"
                    id="input-clinic-email"
                  />
                </div>

                <div>
                  <label className="block text-slate-400 font-medium mb-1">Phone</label>
                  <input
                    type="text"
                    placeholder="+1 555-0199"
                    value={provPhone}
                    onChange={(e) => setProvPhone(e.target.value)}
                    className="w-full bg-slate-900 border border-slate-800 rounded-xl px-3 py-2 text-white placeholder-slate-600 focus:outline-none focus:border-sky-500"
                    id="input-clinic-phone"
                  />
                </div>

                <div>
                  <label className="block text-slate-400 font-medium mb-1">City</label>
                  <input
                    type="text"
                    placeholder="City"
                    value={provCity}
                    onChange={(e) => setProvCity(e.target.value)}
                    className="w-full bg-slate-900 border border-slate-800 rounded-xl px-3 py-2 text-white placeholder-slate-600 focus:outline-none focus:border-sky-500"
                    id="input-clinic-city"
                  />
                </div>
              </div>

              {/* Plan & Trial Terms */}
              <div className="p-4 rounded-xl bg-slate-900/60 border border-slate-800 space-y-3">
                <div className="text-xs font-bold text-white flex items-center gap-1.5">
                  <CreditCard className="w-4 h-4 text-sky-400" />
                  Subscription Plan & Terms
                </div>

                <div className="grid grid-cols-1 md:grid-cols-3 gap-3">
                  <div>
                    <label className="block text-slate-400 mb-1">Initial Plan</label>
                    <select
                      value={provPlan}
                      onChange={(e) => setProvPlan(e.target.value)}
                      className="w-full bg-slate-950 border border-slate-800 rounded-xl px-3 py-2 text-white focus:outline-none focus:border-sky-500"
                      id="select-clinic-plan"
                    >
                      <option value="basic">Basic ($49/mo)</option>
                      <option value="pro">Pro ($149/mo)</option>
                      <option value="enterprise">Enterprise ($499/mo)</option>
                    </select>
                  </div>

                  <div>
                    <label className="block text-slate-400 mb-1">Billing Cycle</label>
                    <select
                      value={provCycle}
                      onChange={(e) => setProvCycle(Number(e.target.value) as 1 | 2)}
                      className="w-full bg-slate-950 border border-slate-800 rounded-xl px-3 py-2 text-white focus:outline-none focus:border-sky-500"
                      id="select-clinic-cycle"
                    >
                      <option value={1}>Monthly</option>
                      <option value={2}>Annual</option>
                    </select>
                  </div>

                  <div>
                    <label className="block text-slate-400 mb-1">Trial Period (Days)</label>
                    <input
                      type="number"
                      min={0}
                      max={90}
                      value={provTrialDays}
                      onChange={(e) => setProvTrialDays(Number(e.target.value))}
                      className="w-full bg-slate-950 border border-slate-800 rounded-xl px-3 py-2 text-white focus:outline-none focus:border-sky-500"
                      id="input-clinic-trial-days"
                    />
                  </div>
                </div>
              </div>

              {/* Initial Admin User */}
              <div className="p-4 rounded-xl bg-slate-900/60 border border-slate-800 space-y-3">
                <div className="text-xs font-bold text-white flex items-center gap-1.5">
                  <Users className="w-4 h-4 text-indigo-400" />
                  Initial Clinic Administrator (Optional)
                </div>

                <div className="grid grid-cols-1 md:grid-cols-2 gap-3">
                  <div>
                    <label className="block text-slate-400 mb-1">Admin First Name</label>
                    <input
                      type="text"
                      placeholder="Doctor / Admin"
                      value={adminFirst}
                      onChange={(e) => setAdminFirst(e.target.value)}
                      className="w-full bg-slate-950 border border-slate-800 rounded-xl px-3 py-2 text-white focus:outline-none focus:border-sky-500"
                      id="input-admin-first"
                    />
                  </div>

                  <div>
                    <label className="block text-slate-400 mb-1">Admin Last Name</label>
                    <input
                      type="text"
                      placeholder="Name"
                      value={adminLast}
                      onChange={(e) => setAdminLast(e.target.value)}
                      className="w-full bg-slate-950 border border-slate-800 rounded-xl px-3 py-2 text-white focus:outline-none focus:border-sky-500"
                      id="input-admin-last"
                    />
                  </div>
                </div>

                <div className="grid grid-cols-1 md:grid-cols-2 gap-3">
                  <div>
                    <label className="block text-slate-400 mb-1">Admin Email</label>
                    <input
                      type="email"
                      placeholder="admin@newclinic.com"
                      value={adminEmail}
                      onChange={(e) => setAdminEmail(e.target.value)}
                      className="w-full bg-slate-950 border border-slate-800 rounded-xl px-3 py-2 text-white focus:outline-none focus:border-sky-500"
                      id="input-admin-email"
                    />
                  </div>

                  <div>
                    <label className="block text-slate-400 mb-1">Temporary Password</label>
                    <input
                      type="password"
                      placeholder="Admin@123!"
                      value={adminPassword}
                      onChange={(e) => setAdminPassword(e.target.value)}
                      className="w-full bg-slate-950 border border-slate-800 rounded-xl px-3 py-2 text-white focus:outline-none focus:border-sky-500"
                      id="input-admin-password"
                    />
                  </div>
                </div>
              </div>

              <div className="flex items-center justify-end gap-3 pt-3 border-t border-slate-800">
                <button
                  type="button"
                  onClick={() => setIsProvisionOpen(false)}
                  className="px-4 py-2 rounded-xl bg-slate-900 text-slate-400 hover:text-white transition"
                >
                  Cancel
                </button>
                <button
                  type="submit"
                  disabled={isSubmitting}
                  className="px-5 py-2 rounded-xl bg-gradient-to-r from-sky-500 to-indigo-600 hover:from-sky-400 hover:to-indigo-500 text-white font-semibold transition flex items-center gap-2"
                  id="btn-submit-provision"
                >
                  {isSubmitting ? <RefreshCw className="w-4 h-4 animate-spin" /> : <Plus className="w-4 h-4" />}
                  Confirm & Provision
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* MODAL: TENANT DETAIL & IMMUTABLE AUDIT TRAIL */}
      {isDetailOpen && selectedTenant && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-slate-950/80 backdrop-blur-sm" id="modal-tenant-detail">
          <div className="glass-panel w-full max-w-3xl rounded-2xl border-slate-800 max-h-[90vh] overflow-y-auto p-6 space-y-6">
            <div className="flex items-center justify-between border-b border-slate-800 pb-4">
              <div>
                <div className="flex items-center gap-2">
                  <h2 className="text-base font-bold text-white">{selectedTenant.name}</h2>
                  {getStatusBadge(selectedTenant.lifecycleStatus)}
                </div>
                <div className="text-xs text-slate-400 font-mono mt-0.5">
                  ID: {selectedTenant.id} • Slug: /{selectedTenant.slug}
                </div>
              </div>

              <button onClick={() => setIsDetailOpen(false)} className="text-slate-400 hover:text-white">
                <X className="w-5 h-5" />
              </button>
            </div>

            {detailLoading ? (
              <div className="py-12 flex justify-center text-slate-500">
                <RefreshCw className="w-6 h-6 animate-spin" />
              </div>
            ) : tenantDetail ? (
              <div className="space-y-6 text-xs">
                {/* Quotas Breakdown */}
                <div>
                  <h3 className="font-bold text-white flex items-center gap-2 mb-3">
                    <Sliders className="w-4 h-4 text-sky-400" />
                    Enforced Plan Quotas & Utilization
                  </h3>

                  <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
                    {tenantDetail.subscription.quotas.map((q) => (
                      <div key={q.metricType} className="p-3.5 rounded-xl bg-slate-900 border border-slate-800">
                        <div className="flex justify-between font-medium">
                          <span className="text-slate-300">{q.metricName}</span>
                          <span className="text-white font-mono font-bold">
                            {q.currentValue.toLocaleString()} / {q.quotaLimit === 0 ? '∞' : q.quotaLimit.toLocaleString()}
                          </span>
                        </div>

                        <div className="w-full bg-slate-800 rounded-full h-2 mt-2 overflow-hidden">
                          <div
                            className={`h-full rounded-full transition-all ${
                              q.isExceeded ? 'bg-rose-500' : q.usagePercentage > 80 ? 'bg-amber-500' : 'bg-sky-500'
                            }`}
                            style={{ width: `${Math.min(100, q.usagePercentage)}%` }}
                          />
                        </div>
                        <div className="text-right text-[10px] text-slate-500 mt-1 font-mono">
                          {q.quotaLimit === 0 ? 'Unlimited Plan Limit' : `${q.usagePercentage.toFixed(1)}% Used`}
                        </div>
                      </div>
                    ))}
                  </div>
                </div>

                {/* Enabled Modules */}
                <div>
                  <h3 className="font-bold text-white mb-2">Enabled Module Entitlements</h3>
                  <div className="flex flex-wrap gap-1.5">
                    {tenantDetail.subscription.enabledFeatures.map((f) => (
                      <span key={f} className="px-2.5 py-1 rounded-lg bg-sky-500/10 border border-sky-500/20 text-sky-400 font-mono text-[11px]">
                        {f}
                      </span>
                    ))}
                  </div>
                </div>

                {/* Immutable Lifecycle Audit Trail */}
                <div>
                  <h3 className="font-bold text-white flex items-center gap-2 mb-3">
                    <History className="w-4 h-4 text-purple-400" />
                    Immutable Lifecycle Audit Events
                  </h3>

                  <div className="space-y-2 max-h-60 overflow-y-auto pr-1">
                    {tenantDetail.recentAuditEvents.length === 0 ? (
                      <div className="text-slate-500 text-center py-4">No audit events recorded yet.</div>
                    ) : (
                      tenantDetail.recentAuditEvents.map((evt) => (
                        <div key={evt.id} className="p-3 rounded-xl bg-slate-900 border border-slate-800 space-y-1">
                          <div className="flex items-center justify-between">
                            <span className="font-bold text-purple-300 font-mono text-[11px]">
                              {evt.eventTypeName}
                            </span>
                            <span className="text-slate-500 text-[10px]">
                              {new Date(evt.timestamp).toLocaleString()}
                            </span>
                          </div>

                          {evt.reason && (
                            <div className="text-slate-300 text-[11px]">
                              <span className="text-slate-500">Reason:</span> {evt.reason}
                            </div>
                          )}

                          {evt.newValue && (
                            <div className="text-slate-400 text-[11px] font-mono">
                              {evt.newValue}
                            </div>
                          )}
                        </div>
                      ))
                    )}
                  </div>
                </div>
              </div>
            ) : null}

            <div className="flex justify-end pt-3 border-t border-slate-800">
              <button
                onClick={() => setIsDetailOpen(false)}
                className="px-4 py-2 rounded-xl bg-slate-900 text-slate-300 hover:text-white"
              >
                Close
              </button>
            </div>
          </div>
        </div>
      )}

      {/* MODAL: SUSPEND CLINIC */}
      {isSuspendOpen && selectedTenant && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-slate-950/80 backdrop-blur-sm" id="modal-suspend-clinic">
          <div className="glass-panel w-full max-w-md rounded-2xl border-slate-800 p-6 space-y-4">
            <div className="flex items-center gap-2.5 text-rose-400">
              <PauseCircle className="w-6 h-6" />
              <h2 className="text-base font-bold text-white">Suspend Clinic Operational Access</h2>
            </div>

            <p className="text-xs text-slate-300">
              You are suspending <span className="font-bold text-white">{selectedTenant.name}</span>.
            </p>

            <div className="p-3 rounded-xl bg-amber-500/10 border border-amber-500/20 text-amber-300 text-[11px] space-y-1">
              <div className="font-bold flex items-center gap-1.5">
                <ShieldCheck className="w-3.5 h-3.5 text-amber-400" />
                Tenant Suspension Access Matrix Enforcement:
              </div>
              <p>
                Staff will retain <strong>read-only access to existing medical records</strong> and patients can still <strong>revoke consent</strong>. All mutations (appointments, visits, AI inference, DICOM uploads) will be blocked with HTTP 403.
              </p>
            </div>

            <div>
              <label className="block text-xs font-medium text-slate-400 mb-1">Suspension Reason *</label>
              <textarea
                rows={3}
                required
                placeholder="Specify the reason for this administrative suspension..."
                value={suspendReason}
                onChange={(e) => setSuspendReason(e.target.value)}
                className="w-full bg-slate-900 border border-slate-800 rounded-xl p-2.5 text-xs text-white focus:outline-none focus:border-rose-500"
                id="input-suspend-reason"
              />
            </div>

            <div className="flex items-center justify-end gap-2.5 pt-2">
              <button
                type="button"
                onClick={() => setIsSuspendOpen(false)}
                className="px-4 py-2 rounded-xl bg-slate-900 text-slate-400 hover:text-white text-xs"
              >
                Cancel
              </button>
              <button
                type="button"
                disabled={!suspendReason.trim() || isSubmitting}
                onClick={handleSuspendTenant}
                className="px-4 py-2 rounded-xl bg-rose-600 hover:bg-rose-500 text-white font-medium text-xs transition"
                id="btn-confirm-suspend"
              >
                Confirm Suspension
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
