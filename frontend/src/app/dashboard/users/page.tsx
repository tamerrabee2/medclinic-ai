'use client';

import React, { useState } from 'react';
import {
  UserCheck,
  UserPlus,
  Shield,
  KeyRound,
  Search,
  MoreVertical,
  Mail,
  Phone,
  Building2,
  Lock,
  CheckCircle2,
  AlertTriangle,
  X,
  RefreshCw,
  Eye,
  EyeOff,
  UserX,
  ShieldCheck,
  Smartphone,
  Check
} from 'lucide-react';

interface UserAccount {
  id: string;
  name: string;
  username: string;
  email: string;
  phone: string;
  role: 'SuperAdmin' | 'ClinicAdmin' | 'Doctor' | 'Nurse' | 'Receptionist' | 'LabTechnician' | 'Radiologist' | 'Accountant';
  clinicName: string;
  status: 'active' | 'suspended' | 'pending';
  mfaEnabled: boolean;
  lastLogin: string;
  createdAt: string;
  directPermissions: string[];
}

const INITIAL_USERS: UserAccount[] = [
  {
    id: 'usr-101',
    name: 'Dr. Sarah Al-Mansoor',
    username: 'dr.sarah',
    email: 'sarah.mansoor@medclinic.ai',
    phone: '+966 55 123 4567',
    role: 'Doctor',
    clinicName: 'Main Medical Center',
    status: 'active',
    mfaEnabled: true,
    lastLogin: 'Today, 08:30 AM',
    createdAt: '2026-01-10',
    directPermissions: ['Patients.ReadWrite', 'EMR.Prescribe', 'AI.FullAnalysis', 'Visits.SignOff']
  },
  {
    id: 'usr-102',
    name: 'Eng. Hisham Al-Hadi',
    username: 'admin.hisham',
    email: 'admin@medclinic.ai',
    phone: '+966 50 111 2233',
    role: 'ClinicAdmin',
    clinicName: 'Main Medical Center',
    status: 'active',
    mfaEnabled: true,
    lastLogin: 'Today, 09:12 AM',
    createdAt: '2025-11-01',
    directPermissions: ['Users.Manage', 'Roles.Assign', 'Clinic.Settings', 'Audit.View', 'Billing.Full']
  },
  {
    id: 'usr-103',
    name: 'Dr. Tariq Ziyad',
    username: 'dr.tariq',
    email: 'tariq.ziyad@medclinic.ai',
    phone: '+966 55 987 6543',
    role: 'Doctor',
    clinicName: 'Dental Specialty Branch',
    status: 'active',
    mfaEnabled: false,
    lastLogin: 'Yesterday, 04:15 PM',
    createdAt: '2026-02-15',
    directPermissions: ['Patients.ReadWrite', 'EMR.Dental', 'AI.FullAnalysis']
  },
  {
    id: 'usr-104',
    name: 'Mona Al-Ghamdi',
    username: 'mona.reception',
    email: 'mona.ghamdi@medclinic.ai',
    phone: '+966 50 444 3322',
    role: 'Receptionist',
    clinicName: 'Main Medical Center',
    status: 'active',
    mfaEnabled: true,
    lastLogin: 'Today, 07:55 AM',
    createdAt: '2026-03-01',
    directPermissions: ['Patients.ReadWrite', 'Appointments.Manage', 'Billing.Basic']
  },
  {
    id: 'usr-105',
    name: 'Khaled Al-Otaibi',
    username: 'khaled.lab',
    email: 'khaled.otaibi@medclinic.ai',
    phone: '+966 54 888 7766',
    role: 'LabTechnician',
    clinicName: 'Central Diagnostic Lab',
    status: 'active',
    mfaEnabled: true,
    lastLogin: 'Sep 03, 11:20 AM',
    createdAt: '2026-04-12',
    directPermissions: ['Laboratory.Orders', 'Laboratory.UploadResults', 'AI.LabAnalyzer']
  },
  {
    id: 'usr-106',
    name: 'Dr. Reem Al-Qahtani',
    username: 'dr.reem',
    email: 'reem.qahtani@medclinic.ai',
    phone: '+966 56 333 9988',
    role: 'Radiologist',
    clinicName: 'Imaging & PACS Suite',
    status: 'pending',
    mfaEnabled: false,
    lastLogin: 'Never',
    createdAt: '2026-09-02',
    directPermissions: ['Radiology.PACS', 'Radiology.Report', 'AI.Vision']
  },
  {
    id: 'usr-107',
    name: 'Faisal Al-Shehri',
    username: 'faisal.acc',
    email: 'faisal.shehri@medclinic.ai',
    phone: '+966 53 222 1199',
    role: 'Accountant',
    clinicName: 'Main Medical Center',
    status: 'suspended',
    mfaEnabled: true,
    lastLogin: 'Aug 28, 02:00 PM',
    createdAt: '2026-01-20',
    directPermissions: ['Billing.Full', 'Reports.Financial', 'Claims.Insurance']
  }
];

const AVAILABLE_PERMISSIONS = [
  { id: 'Patients.ReadWrite', label: 'إدارة المرضى (Patients.ReadWrite)', desc: 'عرض وإضافة وتعديل بيانات المرضى' },
  { id: 'EMR.Prescribe', label: 'الوصفات والتشخيص (EMR.Prescribe)', desc: 'إصدار الوصفات الطبية وتسجيل التشخيصات' },
  { id: 'AI.FullAnalysis', label: 'الذكاء الاصطناعي (AI.FullAnalysis)', desc: 'استخدام المساعد الطبي ومحلل الأشعة والمختبر' },
  { id: 'Laboratory.Manage', label: 'المختبر والتحاليل (Laboratory.Manage)', desc: 'إدخال نتائج الفحوصات وإصدار تقارير المختبر' },
  { id: 'Radiology.PACS', label: 'الأشعة والتصوير (Radiology.PACS)', desc: 'عرض صور الأشعة التشخيصية واعتماد التقارير' },
  { id: 'Billing.Full', label: 'الفواتير والمالية (Billing.Full)', desc: 'إصدار الفواتير والمطالبات التأمينية والتحصيل' },
  { id: 'Users.Manage', label: 'إدارة المستخدمين (Users.Manage)', desc: 'إنشاء حسابات المستخدمين وتعيين الأدوار والصلاحيات' },
  { id: 'Audit.View', label: 'سجلات الأمان والتدقيق (Audit.View)', desc: 'الاطلاع على سجلات العمليات والأمان والامتثال' }
];

export default function UsersManagementPage() {
  const [users, setUsers] = useState<UserAccount[]>(INITIAL_USERS);
  const [searchQuery, setSearchQuery] = useState('');
  const [roleFilter, setRoleFilter] = useState('all');
  const [statusFilter, setStatusFilter] = useState('all');

  // Modal States
  const [showAddModal, setShowAddModal] = useState(false);
  const [showResetPasswordModal, setShowResetPasswordModal] = useState<UserAccount | null>(null);
  const [successToast, setSuccessToast] = useState<string | null>(null);

  // New User Form State
  const [formData, setFormData] = useState({
    name: '',
    username: '',
    email: '',
    phone: '',
    role: 'Doctor' as UserAccount['role'],
    clinicName: 'Main Medical Center',
    tempPassword: '',
    requirePasswordReset: true,
    sendWelcomeEmail: true,
    enableMfa: true,
    selectedPermissions: ['Patients.ReadWrite', 'AI.FullAnalysis']
  });

  const [showPassword, setShowPassword] = useState(false);

  const filteredUsers = users.filter((u) => {
    const matchesSearch =
      u.name.toLowerCase().includes(searchQuery.toLowerCase()) ||
      u.username.toLowerCase().includes(searchQuery.toLowerCase()) ||
      u.email.toLowerCase().includes(searchQuery.toLowerCase()) ||
      u.phone.includes(searchQuery);

    const matchesRole = roleFilter === 'all' || u.role === roleFilter;
    const matchesStatus = statusFilter === 'all' || u.status === statusFilter;

    return matchesSearch && matchesRole && matchesStatus;
  });

  const handleGeneratePassword = () => {
    const chars = 'ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789!@#$%^&*';
    let pwd = '';
    for (let i = 0; i < 12; i++) {
      pwd += chars.charAt(Math.floor(Math.random() * chars.length));
    }
    setFormData((prev) => ({ ...prev, tempPassword: pwd }));
  };

  const handleTogglePermission = (permId: string) => {
    setFormData((prev) => {
      const exists = prev.selectedPermissions.includes(permId);
      return {
        ...prev,
        selectedPermissions: exists
          ? prev.selectedPermissions.filter((p) => p !== permId)
          : [...prev.selectedPermissions, permId]
      };
    });
  };

  const handleCreateUser = (e: React.FormEvent) => {
    e.preventDefault();
    if (!formData.name || !formData.email || !formData.username) return;

    const newUser: UserAccount = {
      id: `usr-${Date.now().toString().slice(-4)}`,
      name: formData.name,
      username: formData.username,
      email: formData.email,
      phone: formData.phone || '+966 50 000 0000',
      role: formData.role,
      clinicName: formData.clinicName,
      status: 'active',
      mfaEnabled: formData.enableMfa,
      lastLogin: 'Never',
      createdAt: new Date().toISOString().split('T')[0],
      directPermissions: formData.selectedPermissions
    };

    setUsers([newUser, ...users]);
    setShowAddModal(false);
    setSuccessToast(`تم إنشاء حساب المستخدم "${newUser.name}" بنجاح وتعيين دور ${newUser.role}`);
    setTimeout(() => setSuccessToast(null), 4000);

    // Reset Form
    setFormData({
      name: '',
      username: '',
      email: '',
      phone: '',
      role: 'Doctor',
      clinicName: 'Main Medical Center',
      tempPassword: '',
      requirePasswordReset: true,
      sendWelcomeEmail: true,
      enableMfa: true,
      selectedPermissions: ['Patients.ReadWrite', 'AI.FullAnalysis']
    });
  };

  const handleToggleUserStatus = (userId: string) => {
    setUsers((prev) =>
      prev.map((u) => {
        if (u.id === userId) {
          const nextStatus = u.status === 'active' ? 'suspended' : 'active';
          return { ...u, status: nextStatus };
        }
        return u;
      })
    );
  };

  const getRoleBadge = (role: string) => {
    switch (role) {
      case 'SuperAdmin':
        return 'bg-purple-500/10 text-purple-400 border-purple-500/30';
      case 'ClinicAdmin':
        return 'bg-indigo-500/10 text-indigo-400 border-indigo-500/30';
      case 'Doctor':
        return 'bg-sky-500/10 text-sky-400 border-sky-500/30';
      case 'Nurse':
        return 'bg-teal-500/10 text-teal-400 border-teal-500/30';
      case 'LabTechnician':
        return 'bg-amber-500/10 text-amber-400 border-amber-500/30';
      case 'Radiologist':
        return 'bg-cyan-500/10 text-cyan-400 border-cyan-500/30';
      case 'Accountant':
        return 'bg-emerald-500/10 text-emerald-400 border-emerald-500/30';
      default:
        return 'bg-slate-500/10 text-slate-400 border-slate-500/30';
    }
  };

  return (
    <div className="space-y-6">
      {/* Toast Notification */}
      {successToast && (
        <div className="fixed top-20 right-6 z-50 bg-emerald-950/90 border border-emerald-500/50 text-emerald-200 px-4 py-3 rounded-xl shadow-2xl flex items-center gap-3 backdrop-blur-md animate-in slide-in-from-top-2">
          <CheckCircle2 className="w-5 h-5 text-emerald-400 shrink-0" />
          <span className="text-sm font-medium">{successToast}</span>
        </div>
      )}

      {/* Header */}
      <div className="flex flex-col md:flex-row md:items-center md:justify-between gap-4">
        <div>
          <div className="flex items-center gap-2 text-xs font-mono text-sky-400 mb-1">
            <ShieldCheck className="w-4 h-4" />
            <span>IDENTITY & ACCESS MANAGEMENT (IAM)</span>
          </div>
          <h1 className="text-2xl font-bold text-white tracking-tight">إدارة المستخدمين والحسابات</h1>
          <p className="text-xs text-slate-400 mt-0.5">
            إدارة حسابات طاقم العيادة، الأدوار، الصلاحيات المباشرة، وحالة التحقق الثنائي (MFA)
          </p>
        </div>

        <div className="flex items-center gap-3">
          <button
            onClick={() => {
              handleGeneratePassword();
              setShowAddModal(true);
            }}
            className="flex items-center gap-2 px-4 py-2.5 rounded-xl bg-gradient-to-r from-sky-500 to-indigo-600 hover:from-sky-400 hover:to-indigo-500 text-white text-xs font-semibold shadow-lg shadow-sky-500/20 transition-all hover:scale-[1.02] active:scale-[0.98]"
          >
            <UserPlus className="w-4 h-4" />
            <span>إضافة مستخدم جديد</span>
          </button>
        </div>
      </div>

      {/* Stats Cards */}
      <div className="grid grid-cols-2 sm:grid-cols-4 gap-4">
        <div className="glass-panel p-4 rounded-xl border-slate-800">
          <div className="flex items-center justify-between text-slate-400 text-xs mb-1">
            <span>إجمالي الحسابات</span>
            <UserCheck className="w-4 h-4 text-sky-400" />
          </div>
          <div className="text-2xl font-bold text-white font-mono">{users.length}</div>
          <div className="text-[10px] text-slate-500 mt-1">كافة العيادات المسجلة</div>
        </div>

        <div className="glass-panel p-4 rounded-xl border-slate-800">
          <div className="flex items-center justify-between text-slate-400 text-xs mb-1">
            <span>الحسابات النشطة</span>
            <CheckCircle2 className="w-4 h-4 text-emerald-400" />
          </div>
          <div className="text-2xl font-bold text-emerald-400 font-mono">
            {users.filter((u) => u.status === 'active').length}
          </div>
          <div className="text-[10px] text-slate-500 mt-1">يمكنهم تسجيل الدخول الآن</div>
        </div>

        <div className="glass-panel p-4 rounded-xl border-slate-800">
          <div className="flex items-center justify-between text-slate-400 text-xs mb-1">
            <span>الأطباء والاستشاريون</span>
            <Shield className="w-4 h-4 text-indigo-400" />
          </div>
          <div className="text-2xl font-bold text-indigo-400 font-mono">
            {users.filter((u) => u.role === 'Doctor').length}
          </div>
          <div className="text-[10px] text-slate-500 mt-1">صلاحيات التشخيص والوصفات</div>
        </div>

        <div className="glass-panel p-4 rounded-xl border-slate-800">
          <div className="flex items-center justify-between text-slate-400 text-xs mb-1">
            <span>التحقق الثنائي (MFA)</span>
            <Smartphone className="w-4 h-4 text-teal-400" />
          </div>
          <div className="text-2xl font-bold text-teal-400 font-mono">
            {users.filter((u) => u.mfaEnabled).length}
          </div>
          <div className="text-[10px] text-slate-500 mt-1">حسابات مؤمنة بنسبة 86%</div>
        </div>
      </div>

      {/* Search & Filters */}
      <div className="glass-panel p-4 rounded-xl border-slate-800 flex flex-col md:flex-row items-center gap-3">
        <div className="relative flex-1 w-full">
          <Search className="w-4 h-4 text-slate-500 absolute left-3 top-1/2 -translate-y-1/2" />
          <input
            type="text"
            placeholder="بحث بالاسم، اسم المستخدم، البريد، أو الهاتف..."
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
            className="w-full pl-9 pr-4 py-2 bg-slate-900/80 border border-slate-800 rounded-lg text-xs text-white placeholder-slate-500 focus:outline-none focus:border-sky-500"
          />
        </div>

        <div className="flex items-center gap-2 w-full md:w-auto">
          <select
            value={roleFilter}
            onChange={(e) => setRoleFilter(e.target.value)}
            className="px-3 py-2 bg-slate-900/80 border border-slate-800 rounded-lg text-xs text-slate-300 focus:outline-none focus:border-sky-500"
          >
            <option value="all">كافة الأدوار (All Roles)</option>
            <option value="SuperAdmin">SuperAdmin</option>
            <option value="ClinicAdmin">ClinicAdmin</option>
            <option value="Doctor">Doctor</option>
            <option value="Nurse">Nurse</option>
            <option value="Receptionist">Receptionist</option>
            <option value="LabTechnician">LabTechnician</option>
            <option value="Radiologist">Radiologist</option>
            <option value="Accountant">Accountant</option>
          </select>

          <select
            value={statusFilter}
            onChange={(e) => setStatusFilter(e.target.value)}
            className="px-3 py-2 bg-slate-900/80 border border-slate-800 rounded-lg text-xs text-slate-300 focus:outline-none focus:border-sky-500"
          >
            <option value="all">كافة الحالات</option>
            <option value="active">نشط (Active)</option>
            <option value="pending">معلق التفعيل (Pending)</option>
            <option value="suspended">موقوف (Suspended)</option>
          </select>
        </div>
      </div>

      {/* Users Table */}
      <div className="glass-panel rounded-xl border-slate-800 overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full text-right text-xs">
            <thead className="bg-slate-900/80 text-slate-400 border-b border-slate-800 font-mono">
              <tr>
                <th className="py-3 px-4 text-left">USER DETAILS</th>
                <th className="py-3 px-4 text-right">الدور والصلاحيات</th>
                <th className="py-3 px-4 text-right">الفرع / العيادة</th>
                <th className="py-3 px-4 text-center">MFA</th>
                <th className="py-3 px-4 text-center">الحالة</th>
                <th className="py-3 px-4 text-left">LAST LOGIN</th>
                <th className="py-3 px-4 text-center">الإجراءات</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-800/60">
              {filteredUsers.length === 0 ? (
                <tr>
                  <td colSpan={7} className="py-8 text-center text-slate-500">
                    لا يوجد مستخدمون يطابقون معايير البحث
                  </td>
                </tr>
              ) : (
                filteredUsers.map((user) => (
                  <tr key={user.id} className="hover:bg-slate-800/30 transition-colors">
                    {/* User Details */}
                    <td className="py-3 px-4 text-left">
                      <div className="flex items-center gap-3">
                        <div className="w-9 h-9 rounded-xl bg-gradient-to-tr from-sky-600/30 to-indigo-600/30 border border-sky-500/20 flex items-center justify-center font-bold text-sky-300 text-xs shrink-0">
                          {user.name
                            .split(' ')
                            .map((n) => n[0])
                            .slice(0, 2)
                            .join('')}
                        </div>
                        <div>
                          <div className="font-semibold text-white">{user.name}</div>
                          <div className="text-[11px] text-slate-400 flex items-center gap-2 mt-0.5">
                            <span className="font-mono text-sky-400">@{user.username}</span>
                            <span>•</span>
                            <span className="truncate">{user.email}</span>
                          </div>
                        </div>
                      </div>
                    </td>

                    {/* Role & Permissions */}
                    <td className="py-3 px-4 text-right">
                      <div className="flex flex-col items-start gap-1">
                        <span
                          className={`px-2 py-0.5 rounded-md border text-[10px] font-semibold font-mono ${getRoleBadge(
                            user.role
                          )}`}
                        >
                          {user.role}
                        </span>
                        <div className="text-[10px] text-slate-500">
                          {user.directPermissions.length} صلاحية مخصصة
                        </div>
                      </div>
                    </td>

                    {/* Clinic / Tenant */}
                    <td className="py-3 px-4 text-right">
                      <div className="flex items-center gap-1.5 text-slate-300">
                        <Building2 className="w-3.5 h-3.5 text-slate-500" />
                        <span>{user.clinicName}</span>
                      </div>
                      <div className="text-[10px] text-slate-500 font-mono mt-0.5">{user.phone}</div>
                    </td>

                    {/* MFA */}
                    <td className="py-3 px-4 text-center">
                      {user.mfaEnabled ? (
                        <span className="inline-flex items-center gap-1 px-2 py-0.5 rounded bg-teal-500/10 text-teal-400 border border-teal-500/30 text-[10px]">
                          <Smartphone className="w-3 h-3" />
                          <span>مفعل</span>
                        </span>
                      ) : (
                        <span className="inline-flex items-center gap-1 px-2 py-0.5 rounded bg-slate-800 text-slate-500 border border-slate-700 text-[10px]">
                          <span>معطل</span>
                        </span>
                      )}
                    </td>

                    {/* Status */}
                    <td className="py-3 px-4 text-center">
                      {user.status === 'active' && (
                        <span className="inline-flex items-center gap-1 px-2 py-0.5 rounded-full bg-emerald-500/10 text-emerald-400 border border-emerald-500/30 text-[10px] font-medium">
                          <span className="w-1.5 h-1.5 rounded-full bg-emerald-400 animate-pulse" />
                          <span>نشط</span>
                        </span>
                      )}
                      {user.status === 'pending' && (
                        <span className="inline-flex items-center gap-1 px-2 py-0.5 rounded-full bg-amber-500/10 text-amber-400 border border-amber-500/30 text-[10px] font-medium">
                          <span className="w-1.5 h-1.5 rounded-full bg-amber-400" />
                          <span>معلق التفعيل</span>
                        </span>
                      )}
                      {user.status === 'suspended' && (
                        <span className="inline-flex items-center gap-1 px-2 py-0.5 rounded-full bg-rose-500/10 text-rose-400 border border-rose-500/30 text-[10px] font-medium">
                          <span className="w-1.5 h-1.5 rounded-full bg-rose-400" />
                          <span>موقوف</span>
                        </span>
                      )}
                    </td>

                    {/* Last Login */}
                    <td className="py-3 px-4 text-left font-mono text-[11px] text-slate-400">
                      {user.lastLogin}
                    </td>

                    {/* Actions */}
                    <td className="py-3 px-4 text-center">
                      <div className="flex items-center justify-center gap-1">
                        <button
                          onClick={() => setShowResetPasswordModal(user)}
                          title="إعادة تعيين كلمة المرور"
                          className="p-1.5 rounded-lg hover:bg-slate-800 text-slate-400 hover:text-amber-400 transition-colors"
                        >
                          <KeyRound className="w-3.5 h-3.5" />
                        </button>
                        <button
                          onClick={() => handleToggleUserStatus(user.id)}
                          title={user.status === 'active' ? 'إيقاف الحساب' : 'تفعيل الحساب'}
                          className={`p-1.5 rounded-lg hover:bg-slate-800 transition-colors ${
                            user.status === 'active'
                              ? 'text-slate-400 hover:text-rose-400'
                              : 'text-slate-400 hover:text-emerald-400'
                          }`}
                        >
                          {user.status === 'active' ? (
                            <UserX className="w-3.5 h-3.5" />
                          ) : (
                            <UserCheck className="w-3.5 h-3.5" />
                          )}
                        </button>
                      </div>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      </div>

      {/* Modal: Add New User */}
      {showAddModal && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-slate-950/80 backdrop-blur-sm animate-in fade-in">
          <div className="glass-panel rounded-2xl border-slate-700 w-full max-w-2xl overflow-hidden shadow-2xl animate-in zoom-in-95">
            {/* Modal Header */}
            <div className="p-5 border-b border-slate-800 flex items-center justify-between bg-slate-900/60">
              <div className="flex items-center gap-3">
                <div className="w-10 h-10 rounded-xl bg-sky-500/10 border border-sky-500/30 flex items-center justify-center text-sky-400">
                  <UserPlus className="w-5 h-5" />
                </div>
                <div>
                  <h3 className="text-base font-bold text-white">إضافة مستخدم جديد للنظام</h3>
                  <p className="text-xs text-slate-400">إنشاء حساب جديد وتعيين الدور والصلاحيات المباشرة</p>
                </div>
              </div>
              <button
                onClick={() => setShowAddModal(false)}
                className="p-1.5 rounded-lg text-slate-400 hover:text-white hover:bg-slate-800"
              >
                <X className="w-5 h-5" />
              </button>
            </div>

            {/* Modal Form */}
            <form onSubmit={handleCreateUser} className="p-6 space-y-4 max-h-[80vh] overflow-y-auto">
              <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                <div>
                  <label className="block text-xs font-medium text-slate-300 mb-1">
                    الاسم الكامل <span className="text-rose-400">*</span>
                  </label>
                  <input
                    type="text"
                    required
                    placeholder="مثال: د. أحمد المنصور"
                    value={formData.name}
                    onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                    className="w-full px-3 py-2 bg-slate-900 border border-slate-800 rounded-lg text-xs text-white placeholder-slate-500 focus:outline-none focus:border-sky-500"
                  />
                </div>

                <div>
                  <label className="block text-xs font-medium text-slate-300 mb-1">
                    اسم المستخدم (Username) <span className="text-rose-400">*</span>
                  </label>
                  <input
                    type="text"
                    required
                    placeholder="مثال: dr.ahmed"
                    value={formData.username}
                    onChange={(e) => setFormData({ ...formData, username: e.target.value })}
                    className="w-full px-3 py-2 bg-slate-900 border border-slate-800 rounded-lg text-xs text-white placeholder-slate-500 focus:outline-none focus:border-sky-500 font-mono"
                  />
                </div>

                <div>
                  <label className="block text-xs font-medium text-slate-300 mb-1">
                    البريد الإلكتروني <span className="text-rose-400">*</span>
                  </label>
                  <input
                    type="email"
                    required
                    placeholder="ahmed@medclinic.ai"
                    value={formData.email}
                    onChange={(e) => setFormData({ ...formData, email: e.target.value })}
                    className="w-full px-3 py-2 bg-slate-900 border border-slate-800 rounded-lg text-xs text-white placeholder-slate-500 focus:outline-none focus:border-sky-500"
                  />
                </div>

                <div>
                  <label className="block text-xs font-medium text-slate-300 mb-1">رقم الهاتف</label>
                  <input
                    type="text"
                    placeholder="+966 50 123 4567"
                    value={formData.phone}
                    onChange={(e) => setFormData({ ...formData, phone: e.target.value })}
                    className="w-full px-3 py-2 bg-slate-900 border border-slate-800 rounded-lg text-xs text-white placeholder-slate-500 focus:outline-none focus:border-sky-500 font-mono"
                  />
                </div>

                <div>
                  <label className="block text-xs font-medium text-slate-300 mb-1">
                    الدور الأساسي (Primary Role) <span className="text-rose-400">*</span>
                  </label>
                  <select
                    value={formData.role}
                    onChange={(e) => setFormData({ ...formData, role: e.target.value as UserAccount['role'] })}
                    className="w-full px-3 py-2 bg-slate-900 border border-slate-800 rounded-lg text-xs text-white focus:outline-none focus:border-sky-500"
                  >
                    <option value="Doctor">Doctor (طبيب ممارس / استشاري)</option>
                    <option value="ClinicAdmin">ClinicAdmin (مدير العيادة)</option>
                    <option value="Nurse">Nurse (تمريض وعناية)</option>
                    <option value="Receptionist">Receptionist (استقبال ومواعيد)</option>
                    <option value="LabTechnician">LabTechnician (فني مختبر وتحاليل)</option>
                    <option value="Radiologist">Radiologist (طبيب / فني أشعة)</option>
                    <option value="Accountant">Accountant (محاسب مالي وتأمين)</option>
                    <option value="SuperAdmin">SuperAdmin (مدير النظام الشامل)</option>
                  </select>
                </div>

                <div>
                  <label className="block text-xs font-medium text-slate-300 mb-1">العيادة / الفرع</label>
                  <select
                    value={formData.clinicName}
                    onChange={(e) => setFormData({ ...formData, clinicName: e.target.value })}
                    className="w-full px-3 py-2 bg-slate-900 border border-slate-800 rounded-lg text-xs text-white focus:outline-none focus:border-sky-500"
                  >
                    <option value="Main Medical Center">Main Medical Center (المركز الرئيسي)</option>
                    <option value="Dental Specialty Branch">Dental Specialty Branch (فرع الأسنان التخصصي)</option>
                    <option value="Central Diagnostic Lab">Central Diagnostic Lab (مختبر التشخيص المركزي)</option>
                    <option value="Imaging & PACS Suite">Imaging & PACS Suite (مجمع الأشعة)</option>
                  </select>
                </div>
              </div>

              {/* Password Generator */}
              <div className="p-3.5 rounded-xl bg-slate-900/60 border border-slate-800 space-y-2">
                <div className="flex items-center justify-between">
                  <label className="text-xs font-medium text-slate-300 flex items-center gap-1.5">
                    <Lock className="w-3.5 h-3.5 text-sky-400" />
                    <span>كلمة المرور المؤقتة</span>
                  </label>
                  <button
                    type="button"
                    onClick={handleGeneratePassword}
                    className="text-[11px] text-sky-400 hover:text-sky-300 flex items-center gap-1"
                  >
                    <RefreshCw className="w-3 h-3" />
                    <span>توليد كلمة مرور قوية</span>
                  </button>
                </div>

                <div className="relative">
                  <input
                    type={showPassword ? 'text' : 'password'}
                    required
                    value={formData.tempPassword}
                    onChange={(e) => setFormData({ ...formData, tempPassword: e.target.value })}
                    className="w-full pl-3 pr-10 py-2 bg-slate-950 border border-slate-800 rounded-lg text-xs text-sky-300 font-mono focus:outline-none focus:border-sky-500"
                  />
                  <button
                    type="button"
                    onClick={() => setShowPassword(!showPassword)}
                    className="absolute right-3 top-1/2 -translate-y-1/2 text-slate-500 hover:text-slate-300"
                  >
                    {showPassword ? <EyeOff className="w-4 h-4" /> : <Eye className="w-4 h-4" />}
                  </button>
                </div>
              </div>

              {/* Direct Permissions Checklist */}
              <div className="space-y-2">
                <div className="text-xs font-medium text-slate-300">الصلاحيات المباشرة (Direct Permissions):</div>
                <div className="grid grid-cols-1 sm:grid-cols-2 gap-2 max-h-44 overflow-y-auto p-2 bg-slate-900/40 rounded-xl border border-slate-800/80">
                  {AVAILABLE_PERMISSIONS.map((perm) => {
                    const isSelected = formData.selectedPermissions.includes(perm.id);
                    return (
                      <div
                        key={perm.id}
                        onClick={() => handleTogglePermission(perm.id)}
                        className={`p-2 rounded-lg border text-xs cursor-pointer transition-all flex items-start gap-2 ${
                          isSelected
                            ? 'bg-sky-500/10 border-sky-500/40 text-white'
                            : 'bg-slate-900/50 border-slate-800 text-slate-400 hover:border-slate-700'
                        }`}
                      >
                        <div
                          className={`w-4 h-4 rounded border mt-0.5 flex items-center justify-center shrink-0 ${
                            isSelected ? 'bg-sky-500 border-sky-500 text-white' : 'border-slate-700'
                          }`}
                        >
                          {isSelected && <Check className="w-3 h-3" />}
                        </div>
                        <div>
                          <div className="font-semibold text-[11px] leading-tight">{perm.label}</div>
                          <div className="text-[10px] text-slate-500 mt-0.5 leading-tight">{perm.desc}</div>
                        </div>
                      </div>
                    );
                  })}
                </div>
              </div>

              {/* Account Security Toggles */}
              <div className="space-y-2 pt-1 border-t border-slate-800 text-xs">
                <label className="flex items-center gap-2 cursor-pointer text-slate-300">
                  <input
                    type="checkbox"
                    checked={formData.requirePasswordReset}
                    onChange={(e) => setFormData({ ...formData, requirePasswordReset: e.target.checked })}
                    className="rounded border-slate-700 text-sky-500 focus:ring-0 bg-slate-900"
                  />
                  <span>إلزام المستخدم بتغيير كلمة المرور عند أول تسجيل دخول</span>
                </label>

                <label className="flex items-center gap-2 cursor-pointer text-slate-300">
                  <input
                    type="checkbox"
                    checked={formData.enableMfa}
                    onChange={(e) => setFormData({ ...formData, enableMfa: e.target.checked })}
                    className="rounded border-slate-700 text-sky-500 focus:ring-0 bg-slate-900"
                  />
                  <span>تفعيل التحقق الثنائي (MFA) عبر تطبيق المصادقة</span>
                </label>

                <label className="flex items-center gap-2 cursor-pointer text-slate-300">
                  <input
                    type="checkbox"
                    checked={formData.sendWelcomeEmail}
                    onChange={(e) => setFormData({ ...formData, sendWelcomeEmail: e.target.checked })}
                    className="rounded border-slate-700 text-sky-500 focus:ring-0 bg-slate-900"
                  />
                  <span>إرسال بريد ترحيبي يحتوي على بيانات الدخول ورابط المنصة</span>
                </label>
              </div>

              {/* Actions */}
              <div className="pt-3 border-t border-slate-800 flex items-center justify-end gap-3">
                <button
                  type="button"
                  onClick={() => setShowAddModal(false)}
                  className="px-4 py-2 rounded-xl text-xs font-semibold text-slate-400 hover:text-white hover:bg-slate-800"
                >
                  إلغاء
                </button>
                <button
                  type="submit"
                  className="px-5 py-2.5 rounded-xl bg-gradient-to-r from-sky-500 to-indigo-600 hover:from-sky-400 hover:to-indigo-500 text-white text-xs font-semibold shadow-lg shadow-sky-500/20"
                >
                  إنشاء الحساب الآن
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* Modal: Reset Password */}
      {showResetPasswordModal && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-slate-950/80 backdrop-blur-sm animate-in fade-in">
          <div className="glass-panel rounded-2xl border-slate-700 w-full max-w-md p-6 space-y-4 shadow-2xl">
            <div className="flex items-center gap-3 text-amber-400">
              <KeyRound className="w-6 h-6 shrink-0" />
              <div>
                <h3 className="text-base font-bold text-white">إعادة تعيين كلمة المرور</h3>
                <p className="text-xs text-slate-400">للمستخدم: {showResetPasswordModal.name}</p>
              </div>
            </div>

            <p className="text-xs text-slate-300 leading-relaxed">
              سيتم إنشاء كلمة مرور مؤقتة عشوائية وإرسالها إلى البريد الإلكتروني المسجل (
              <span className="font-mono text-sky-400">{showResetPasswordModal.email}</span>).
            </p>

            <div className="pt-3 border-t border-slate-800 flex items-center justify-end gap-3">
              <button
                onClick={() => setShowResetPasswordModal(null)}
                className="px-4 py-2 rounded-xl text-xs font-semibold text-slate-400 hover:text-white hover:bg-slate-800"
              >
                إلغاء
              </button>
              <button
                onClick={() => {
                  setSuccessToast(`تم إرسال رابط إعادة تعيين كلمة المرور إلى ${showResetPasswordModal.email}`);
                  setShowResetPasswordModal(null);
                  setTimeout(() => setSuccessToast(null), 4000);
                }}
                className="px-4 py-2 rounded-xl bg-amber-500 hover:bg-amber-400 text-slate-950 text-xs font-bold shadow-lg shadow-amber-500/20"
              >
                تأكيد الإرسال
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
