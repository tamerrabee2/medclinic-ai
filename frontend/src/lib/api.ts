const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5000';

export interface ApiResponse<T = any> {
  data?: T;
  error?: string;
  success: boolean;
}

export class ApiClient {
  private static getToken(): string | null {
    if (typeof window === 'undefined') return null;
    return localStorage.getItem('medclinic_token');
  }

  private static getClinicId(): string | null {
    if (typeof window === 'undefined') return null;
    return localStorage.getItem('medclinic_clinic_id');
  }

  public static async request<T = any>(
    endpoint: string,
    options: RequestInit = {}
  ): Promise<T> {
    const token = this.getToken();
    const clinicId = this.getClinicId();

    const headers: Record<string, string> = {
      'Content-Type': 'application/json',
      Accept: 'application/json',
      ...(options.headers as Record<string, string>),
    };

    if (token) {
      headers['Authorization'] = `Bearer ${token}`;
    }

    if (clinicId) {
      headers['X-Clinic-Id'] = clinicId;
    }

    const url = endpoint.startsWith('http')
      ? endpoint
      : `${API_BASE_URL}${endpoint.startsWith('/') ? endpoint : `/${endpoint}`}`;

    const res = await fetch(url, {
      ...options,
      headers,
    });

    if (res.status === 401) {
      const isDemoToken = !token || token.startsWith('demo_') || token.includes('demo');
      // Only clear storage and redirect for real expired backend sessions, never in demo mode
      if (!isDemoToken && typeof window !== 'undefined' && !window.location.pathname.includes('/login')) {
        localStorage.removeItem('medclinic_token');
        localStorage.removeItem('medclinic_user');
        window.location.href = '/login';
      }
      throw new Error('Unauthorized');
    }

    if (!res.ok) {
      let errorMsg = `HTTP Error ${res.status}`;
      try {
        const errorJson = await res.json();
        errorMsg = errorJson.message || errorJson.title || JSON.stringify(errorJson);
      } catch {
        // use default error message
      }
      throw new Error(errorMsg);
    }

    if (res.status === 204) {
      return {} as T;
    }

    return res.json();
  }

  // Auth
  public static async login(credentials: { email: string; password: string }) {
    return this.request('/api/v1/auth/login', {
      method: 'POST',
      body: JSON.stringify(credentials),
    });
  }

  // Patients
  public static async getPatients(query: string = '', page = 1, pageSize = 20) {
    const token = this.getToken();
    if (token?.startsWith('demo_')) {
      const allDemo = [
        { id: '1', mrn: 'MED-10024', firstName: 'Omar', lastName: 'Al-Husseini', phone: '+966 50 123 4567', gender: 'Male', age: 42, bloodGroup: 'O+', lastVisit: '2026-09-02' },
        { id: '2', mrn: 'MED-10025', firstName: 'Nour', lastName: 'Mostafa', phone: '+966 54 234 5678', gender: 'Female', age: 31, bloodGroup: 'A+', lastVisit: '2026-08-28' },
        { id: '3', mrn: 'MED-10026', firstName: 'Laila', lastName: 'Mahmoud', phone: '+966 56 345 6789', gender: 'Female', age: 26, bloodGroup: 'B+', lastVisit: '2026-09-01' },
        { id: '4', mrn: 'MED-10027', firstName: 'Khaled', lastName: 'bin Walid', phone: '+966 59 456 7890', gender: 'Male', age: 55, bloodGroup: 'AB+', lastVisit: '2026-08-15' },
        { id: '5', mrn: 'MED-10028', firstName: 'Fatima', lastName: 'Zahra', phone: '+966 55 567 8901', gender: 'Female', age: 38, bloodGroup: 'O-', lastVisit: '2026-09-03' },
      ];
      const filtered = query
        ? allDemo.filter((p) => (p.firstName + ' ' + p.lastName).toLowerCase().includes(query.toLowerCase()) || p.mrn.includes(query))
        : allDemo;
      return { items: filtered, totalCount: filtered.length, pageNumber: page, pageSize };
    }

    const params = new URLSearchParams({ pageNumber: String(page), pageSize: String(pageSize) });
    if (query) params.append('searchTerm', query);
    return this.request(`/api/v1/patients?${params.toString()}`);
  }

  public static async getPatient(id: string) {
    const token = this.getToken();
    if (token?.startsWith('demo_')) {
      return {
        id,
        mrn: 'MED-10024',
        firstName: 'Omar',
        lastName: 'Al-Husseini',
        phone: '+966 50 123 4567',
        gender: 'Male',
        dateOfBirth: '1984-05-12',
        bloodGroup: 'O+',
        allergies: 'Penicillin, NSAIDs',
        chronicConditions: 'Type 2 Diabetes, Hypertension',
      };
    }
    return this.request(`/api/v1/patients/${id}`);
  }

  public static async createPatient(patient: any) {
    return this.request('/api/v1/patients', {
      method: 'POST',
      body: JSON.stringify(patient),
    });
  }

  // Appointments
  public static async getAppointments(date?: string, doctorId?: string) {
    const token = this.getToken();
    if (token?.startsWith('demo_')) {
      return [
        { id: 'app-1', patientName: 'Omar Al-Husseini', time: '09:00 AM', status: 'Completed', type: 'Consultation', doctor: 'Dr. Sarah' },
        { id: 'app-2', patientName: 'Nour Mostafa', time: '09:30 AM', status: 'In Consultation', type: 'Follow-up', doctor: 'Dr. Sarah' },
        { id: 'app-3', patientName: 'Laila Mahmoud', time: '10:15 AM', status: 'Waiting', type: 'Dental Examination', doctor: 'Dr. Sarah' },
        { id: 'app-4', patientName: 'Khaled bin Walid', time: '11:00 AM', status: 'Confirmed', type: 'Lab Review', doctor: 'Dr. Sarah' },
      ];
    }
    const params = new URLSearchParams();
    if (date) params.append('date', date);
    if (doctorId) params.append('doctorId', doctorId);
    return this.request(`/api/v1/appointments?${params.toString()}`);
  }

  public static async createAppointment(appointment: any) {
    return this.request('/api/v1/appointments', {
      method: 'POST',
      body: JSON.stringify(appointment),
    });
  }

  // AI
  public static async sendAIMessage(message: string, conversationId?: string, patientContextId?: string) {
    return this.request('/api/v1/ai/chat', {
      method: 'POST',
      body: JSON.stringify({
        message,
        conversationId,
        patientContextId,
      }),
    });
  }

  public static async getAIConversations() {
    return this.request('/api/v1/ai/conversations');
  }

  // Dental
  public static async getDentalChart(patientId: string) {
    return this.request(`/api/v1/dental/patients/${patientId}`);
  }

  public static async upsertDentalRecord(record: any) {
    return this.request('/api/v1/dental', {
      method: 'POST',
      body: JSON.stringify(record),
    });
  }

  // Canvas
  public static async getCanvasAnnotations(imageId: string) {
    return this.request(`/api/v1/canvas/images/${imageId}/annotations`);
  }

  public static async addCanvasAnnotation(annotation: any) {
    return this.request('/api/v1/canvas/annotations', {
      method: 'POST',
      body: JSON.stringify(annotation),
    });
  }
}
