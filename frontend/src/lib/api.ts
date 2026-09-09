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
      if (typeof window !== 'undefined' && !window.location.pathname.includes('/login')) {
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
    const params = new URLSearchParams({ pageNumber: String(page), pageSize: String(pageSize) });
    if (query) params.append('searchTerm', query);
    return this.request(`/api/v1/patients?${params.toString()}`);
  }

  public static async getPatient(id: string) {
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
