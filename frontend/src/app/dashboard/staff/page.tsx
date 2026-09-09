'use client';

import React, { useState } from 'react';
import {
  Users,
  Building2,
  ShieldCheck,
  UserPlus,
  Search,
  Mail,
  Phone,
  CheckCircle2,
  Clock,
  MoreVertical,
  X,
  Stethoscope,
  KeyRound,
  Sliders
} from 'lucide-react';

interface StaffMember {
  id: string;
  name: string;
  email: string;
  phone: string;
  role: string;
  department: string;
  status: 'active' | 'leave';
  permissions: string[];
}

const INITIAL_STAFF: StaffMember[] = [
  {
    id: 'usr-1',
    name: 'Dr. Sarah Al-Mansoor',
    email: 'sarah.mansoor@medclinic.ai',
    phone: '+966 55 123 4567',
    role: 'Doctor',
    department: 'Internal Medicine',
    status: 'active',
    permissions: ['Patients.Full', 'EMR.Prescribe', 'AI.FullAnalysis', 'Billing.View']
  },
  {
    id: 'usr-2',
    name: 'Dr. Tariq Ziyad',
    email: 'tariq.ziyad@medclinic.ai',
    phone: '+966 55 987 6543',
    role: 'Doctor',
    department: 'Dental Surgery',
    status: 'active',
    permissions: ['Patients.Full', 'EMR.Dental', 'AI.FullAnalysis', 'Billing.View']
  },
  {
    id: 'usr-3',
    name: 'Mona Al-Ghamdi',
    email: 'mona.ghamdi@medclinic.ai',
    phone: '+966 50 444 3322',
    role: 'Receptionist',
    department: 'Front Desk',
    status: 'active',
    permissions: ['Patients.ReadWrite', 'Appointments.Manage', 'Billing.Basic']
  },
  {
    id: 'usr-4',
    name: 'Eng. Hisham Al-Hadi',
    email: 'admin@medclinic.ai',
    phone: '+966 50 111 2233',
    role: 'ClinicAdmin',
    department: 'Executive Administration',
    status: 'active',
    permissions: ['Clinic.Settings', 'Users.Manage', 'Audit.View', 'Billing.Full']
  }
];

export default function StaffManagementPage() {
  const [staff, setStaff] = useState<StaffMember[]>(INITIAL_STAFF);
  const [searchQuery, setSearchQuery] = useState('');
  const [roleFilter, setRoleFilter] = useState('all');
  const [showAddModal, setShowAddModal] = useState(false);

  // New staff form state
  const [newName, setNewName] = useState('');
  const [newEmail, setNewEmail] = useState('');
  const [newPhone, setNewPhone] = useState('');
  const [newRole, setNewRole] = useState('Doctor');
  const [newDept, setNewDept] = useState('Internal Medicine');

  const handleAddStaff = (e: React.FormEvent) => {
    e.preventDefault();
    const newMember: StaffMember = {
      id: `usr-${Date.now()}`,
      name: newName,
      email: newEmail,
      phone: newPhone,
      role: newRole,
      department: newDept,
      status: 'active',
      permissions: ['Patients.Read', 'Appointments.View']
    };
    setStaff([...staff, newMember]);
    setShowAddModal(false);
    setNewName('');
    setNewEmail('');
    setNewPhone('');
  };

  const filtered = staff.filter((s) => {
    const matchesSearch =
      s.name.toLowerCase().includes(searchQuery.toLowerCase()) ||
      s.email.toLowerCase().includes(searchQuery.toLowerCase()) ||
      s.department.toLowerCase().includes(searchQuery.toLowerCase());
    const matchesRole = roleFilter === 'all' || s.role === roleFilter;
    return matchesSearch && matchesRole;
  });

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
        <div>
          <div className="flex items-center gap-2">
            <span className="px-2 py-0.5 rounded text-[11px] font-bold bg-indigo-500/20 text-indigo-300 border border-indigo-500/30">
              Module 8 & 9
            </span>
            <span className="text-xs text-slate-400">Multi-Tenancy & Access Control</span>
          </div>
          <h1 className="text-2xl md:text-3xl font-bold text-white mt-1 flex items-center gap-3">
            <Users className="w-8 h-8 text-sky-400" />
            Clinic Staff, Doctors & RBAC Permissions
          </h1>
          <p className="text-sm text-slate-400 mt-0.5">
            Role-Based Access Control (RBAC), multi-specialty clinical staff roster, and permission matrices.
          </p>
        </div>

        <button
          onClick={() => setShowAddModal(true)}
          className="px-5 py-2.5 rounded-xl bg-gradient-to-r from-sky-500 to-indigo-600 hover:from-sky-400 hover:to-indigo-500 text-sm font-semibold text-white shadow-lg shadow-sky-500/20 flex items-center gap-2 transition"
        >
          <UserPlus className="w-4 h-4" />
          <span>Add Staff Member</span>
        </button>
      </div>

      {/* Clinic Profile Card */}
      <div className="glass-panel p-5 rounded-2xl border-slate-800 flex flex-wrap items-center justify-between gap-4">
        <div className="flex items-center gap-3">
          <div className="w-12 h-12 rounded-xl bg-sky-500/20 border border-sky-500/40 flex items-center justify-center text-sky-300">
            <Building2 className="w-6 h-6" />
          </div>
          <div>
            <div className="text-base font-bold text-white">Al-Amal Medical Center (Tenant #1)</div>
            <div className="text-xs text-slate-400">
              License: <span className="font-mono text-slate-300">#MOH-SA-8819</span> • Working Hours: <span className="text-slate-300">08:00 AM - 10:00 PM</span>
            </div>
          </div>
        </div>

        <div className="flex items-center gap-2">
          <span className="px-3 py-1 rounded-full bg-emerald-500/10 border border-emerald-500/30 text-emerald-400 text-xs font-semibold flex items-center gap-1.5">
            <ShieldCheck className="w-3.5 h-3.5" />
            <span>Multi-Tenant Isolated</span>
          </span>
        </div>
      </div>

      {/* Staff Roster Table */}
      <div className="glass-panel p-5 rounded-2xl border-slate-800 space-y-4">
        {/* Controls */}
        <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3">
          <div className="relative max-w-sm w-full">
            <Search className="w-4 h-4 absolute left-3 top-3 text-slate-500" />
            <input
              type="text"
              placeholder="Search staff by name, email, department..."
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              className="w-full pl-9 pr-4 py-2 rounded-xl bg-slate-900 border border-slate-800 text-xs text-white placeholder-slate-500 focus:outline-none focus:border-sky-500"
            />
          </div>

          <div className="flex items-center gap-1 bg-slate-900/80 p-1 rounded-xl border border-slate-800 text-xs">
            {['all', 'Doctor', 'Receptionist', 'ClinicAdmin'].map((r) => (
              <button
                key={r}
                onClick={() => setRoleFilter(r)}
                className={`px-3 py-1 rounded-lg font-semibold transition ${
                  roleFilter === r
                    ? 'bg-sky-500 text-white shadow-sm'
                    : 'text-slate-400 hover:text-slate-200'
                }`}
              >
                {r === 'all' ? 'All Roles' : r}
              </button>
            ))}
          </div>
        </div>

        {/* Staff Table */}
        <div className="overflow-x-auto">
          <table className="w-full text-left text-sm">
            <thead>
              <tr className="border-b border-slate-800 text-xs font-semibold text-slate-400 uppercase tracking-wider">
                <th className="py-3 px-3">Staff Member</th>
                <th className="py-3 px-3">System Role</th>
                <th className="py-3 px-3">Department</th>
                <th className="py-3 px-3">Contact</th>
                <th className="py-3 px-3">Granted Permissions</th>
                <th className="py-3 px-3">Status</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-800/60 text-xs">
              {filtered.map((member) => (
                <tr key={member.id} className="hover:bg-slate-900/40 transition">
                  <td className="py-3 px-3">
                    <div className="font-bold text-white text-sm">{member.name}</div>
                    <div className="text-[11px] text-slate-400">{member.email}</div>
                  </td>

                  <td className="py-3 px-3">
                    <span className="px-2.5 py-1 rounded-full text-xs font-semibold bg-indigo-500/15 border border-indigo-500/30 text-indigo-300">
                      {member.role}
                    </span>
                  </td>

                  <td className="py-3 px-3 text-slate-300 font-medium">
                    {member.department}
                  </td>

                  <td className="py-3 px-3 font-mono text-slate-400">
                    {member.phone}
                  </td>

                  <td className="py-3 px-3">
                    <div className="flex flex-wrap gap-1">
                      {member.permissions.map((p, idx) => (
                        <span
                          key={idx}
                          className="px-2 py-0.5 rounded bg-slate-900 text-slate-400 text-[10px] font-mono border border-slate-800"
                        >
                          {p}
                        </span>
                      ))}
                    </div>
                  </td>

                  <td className="py-3 px-3">
                    <span className="inline-flex items-center gap-1 text-emerald-400 font-medium">
                      <CheckCircle2 className="w-3.5 h-3.5" /> Active
                    </span>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>

      {/* Add Staff Modal */}
      {showAddModal && (
        <div className="fixed inset-0 z-50 bg-black/80 backdrop-blur-sm flex items-center justify-center p-4">
          <div className="glass-panel bg-slate-950 border border-slate-800 text-slate-100 max-w-md w-full rounded-2xl p-6 shadow-2xl space-y-4 relative">
            <button
              onClick={() => setShowAddModal(false)}
              className="absolute top-4 right-4 p-2 rounded-lg text-slate-400 hover:text-white transition"
            >
              <X className="w-5 h-5" />
            </button>

            <h2 className="text-base font-bold text-white flex items-center gap-2">
              <UserPlus className="w-5 h-5 text-sky-400" />
              Register New Medical / Clinical Staff
            </h2>

            <form onSubmit={handleAddStaff} className="space-y-3 text-xs">
              <div>
                <label className="text-slate-400 block mb-1">Full Name</label>
                <input
                  type="text"
                  required
                  placeholder="e.g. Dr. Ahmed Al-Harbi"
                  value={newName}
                  onChange={(e) => setNewName(e.target.value)}
                  className="w-full px-3 py-2 rounded-xl bg-slate-900 border border-slate-800 text-white focus:outline-none focus:border-sky-500"
                />
              </div>

              <div>
                <label className="text-slate-400 block mb-1">Email Address</label>
                <input
                  type="email"
                  required
                  placeholder="ahmed.harbi@medclinic.ai"
                  value={newEmail}
                  onChange={(e) => setNewEmail(e.target.value)}
                  className="w-full px-3 py-2 rounded-xl bg-slate-900 border border-slate-800 text-white focus:outline-none focus:border-sky-500"
                />
              </div>

              <div>
                <label className="text-slate-400 block mb-1">Phone Number</label>
                <input
                  type="text"
                  required
                  placeholder="+966 50 000 0000"
                  value={newPhone}
                  onChange={(e) => setNewPhone(e.target.value)}
                  className="w-full px-3 py-2 rounded-xl bg-slate-900 border border-slate-800 text-white focus:outline-none focus:border-sky-500"
                />
              </div>

              <div className="grid grid-cols-2 gap-3">
                <div>
                  <label className="text-slate-400 block mb-1">Assigned Role</label>
                  <select
                    value={newRole}
                    onChange={(e) => setNewRole(e.target.value)}
                    className="w-full px-3 py-2 rounded-xl bg-slate-900 border border-slate-800 text-white focus:outline-none focus:border-sky-500"
                  >
                    <option>Doctor</option>
                    <option>ClinicAdmin</option>
                    <option>Nurse</option>
                    <option>Receptionist</option>
                    <option>LabTechnician</option>
                    <option>Radiologist</option>
                  </select>
                </div>

                <div>
                  <label className="text-slate-400 block mb-1">Department</label>
                  <select
                    value={newDept}
                    onChange={(e) => setNewDept(e.target.value)}
                    className="w-full px-3 py-2 rounded-xl bg-slate-900 border border-slate-800 text-white focus:outline-none focus:border-sky-500"
                  >
                    <option>Internal Medicine</option>
                    <option>Dental Surgery</option>
                    <option>Front Desk</option>
                    <option>Laboratory</option>
                    <option>Radiology PACS</option>
                  </select>
                </div>
              </div>

              <button
                type="submit"
                className="w-full py-2.5 rounded-xl bg-sky-600 hover:bg-sky-500 text-white font-bold text-xs shadow-lg shadow-sky-600/20 transition mt-2"
              >
                Create Staff Account
              </button>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}
