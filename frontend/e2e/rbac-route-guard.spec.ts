import { test, expect } from '@playwright/test';

test.describe('RBAC Centralized Route Guard & Direct URL Enforcement Matrix', () => {

  test.describe('Receptionist Direct URL Restrictions', () => {
    test.beforeEach(async ({ page }) => {
      await page.addInitScript(() => {
        localStorage.setItem('medclinic_token', 'demo_token_receptionist');
        localStorage.setItem(
          'medclinic_user',
          JSON.stringify({
            id: 'demo-reception-1',
            email: 'reception@medclinic.ai',
            firstName: 'Sara',
            lastName: 'Al-Harbi',
            roles: ['Receptionist'],
          })
        );
      });
    });

    const blockedRoutes = [
      { name: 'Visits', url: '/dashboard/visits' },
      { name: 'Prescriptions', url: '/dashboard/prescriptions' },
      { name: 'Laboratory', url: '/dashboard/laboratory' },
      { name: 'Lab Analyzer', url: '/dashboard/lab-analyzer' },
      { name: 'External Labs', url: '/dashboard/external-labs' },
      { name: 'Radiology PACS', url: '/dashboard/radiology' },
      { name: 'Medical Canvas', url: '/dashboard/canvas' },
      { name: 'Dental Chart', url: '/dashboard/dental' },
      { name: 'AI Assistant', url: '/dashboard/ai-assistant' },
      { name: 'Voice Scribe', url: '/dashboard/ai-assistant/voice-scribe' },
      { name: 'Analytics BI', url: '/dashboard/analytics' },
      { name: 'Reports', url: '/dashboard/reports' },
      { name: 'User Management', url: '/dashboard/users' },
      { name: 'Clinic Staff', url: '/dashboard/staff' },
      { name: 'Audit Logs', url: '/dashboard/audit-logs' },
      { name: 'Super Admin Console', url: '/dashboard/superadmin' },
    ];

    for (const route of blockedRoutes) {
      test(`Receptionist attempting direct URL access to ${route.name} (${route.url}) is blocked and redirected to /unauthorized`, async ({ page }) => {
        await page.goto(route.url);
        await page.waitForURL('**/unauthorized');
        await expect(page.getByText('Access Restricted')).toBeVisible();
        await expect(page.getByText('HTTP 403 · FORBIDDEN')).toBeVisible();
        await expect(page.getByText('Receptionist', { exact: true })).toBeVisible();
      });
    }

    test('Receptionist can freely access permitted routes', async ({ page }) => {
      test.setTimeout(60000);
      const allowedRoutes = [
        '/dashboard',
        '/dashboard/patients',
        '/dashboard/appointments',
        '/dashboard/billing',
        '/dashboard/notifications',
      ];

      for (const url of allowedRoutes) {
        await page.goto(url);
        expect(page.url()).not.toContain('/unauthorized');
        await expect(page.locator('aside')).toBeVisible();
      }
    });
  });

  test.describe('Nurse Direct URL Restrictions', () => {
    test.beforeEach(async ({ page }) => {
      await page.addInitScript(() => {
        localStorage.setItem('medclinic_token', 'demo_token_nurse');
        localStorage.setItem(
          'medclinic_user',
          JSON.stringify({
            id: 'demo-nurse-1',
            email: 'nurse@medclinic.ai',
            firstName: 'Mona',
            lastName: 'Al-Otaibi',
            roles: ['Nurse'],
          })
        );
      });
    });

    test('Nurse is blocked from SuperAdmin, Users, Staff, AI Assistant, and Audit Logs', async ({ page }) => {
      test.setTimeout(60000);
      const nurseBlocked = [
        '/dashboard/superadmin',
        '/dashboard/users',
        '/dashboard/staff',
        '/dashboard/ai-assistant',
        '/dashboard/audit-logs',
      ];

      for (const url of nurseBlocked) {
        await page.goto(url);
        await page.waitForURL('**/unauthorized');
        await expect(page.getByText('Access Restricted')).toBeVisible();
        await expect(page.getByText('Nurse', { exact: true })).toBeVisible();
      }
    });

    test('Nurse can access clinical encounters, prescriptions list, laboratory, and radiology', async ({ page }) => {
      test.setTimeout(60000);
      const nurseAllowed = [
        '/dashboard/visits',
        '/dashboard/prescriptions',
        '/dashboard/laboratory',
        '/dashboard/radiology',
        '/dashboard/canvas',
        '/dashboard/dental',
      ];

      for (const url of nurseAllowed) {
        await page.goto(url);
        expect(page.url()).not.toContain('/unauthorized');
        await expect(page.locator('aside')).toBeVisible();
      }
    });
  });

  test.describe('Doctor Direct URL Restrictions', () => {
    test.beforeEach(async ({ page }) => {
      await page.addInitScript(() => {
        localStorage.setItem('medclinic_token', 'demo_token_doctor');
        localStorage.setItem(
          'medclinic_user',
          JSON.stringify({
            id: 'demo-doctor-1',
            email: 'doctor@medclinic.ai',
            firstName: 'Dr. Sarah',
            lastName: 'Al-Mansoor',
            roles: ['Doctor'],
          })
        );
      });
    });

    test('Doctor is blocked from Super Admin console, User Accounts, and Staff Management', async ({ page }) => {
      const doctorBlocked = [
        '/dashboard/superadmin',
        '/dashboard/users',
        '/dashboard/staff',
      ];

      for (const url of doctorBlocked) {
        await page.goto(url);
        await page.waitForURL('**/unauthorized');
        await expect(page.getByText('Access Restricted')).toBeVisible();
        await expect(page.getByText('Doctor', { exact: true })).toBeVisible();
      }
    });

    test('Doctor can access all clinical, diagnostic, AI assistant and audit log screens', async ({ page }) => {
      const doctorAllowed = [
        '/dashboard/visits',
        '/dashboard/prescriptions',
        '/dashboard/ai-assistant',
        '/dashboard/radiology',
        '/dashboard/audit-logs',
      ];

      for (const url of doctorAllowed) {
        await page.goto(url);
        expect(page.url()).not.toContain('/unauthorized');
        await expect(page.locator('aside')).toBeVisible();
      }
    });
  });

  test.describe('SuperAdmin Full System Access', () => {
    test('SuperAdmin can directly access any dashboard route without redirection', async ({ page }) => {
      await page.addInitScript(() => {
        localStorage.setItem('medclinic_token', 'demo_token_superadmin');
        localStorage.setItem(
          'medclinic_user',
          JSON.stringify({
            id: 'demo-superadmin-1',
            email: 'superadmin@medclinic.ai',
            firstName: 'Platform',
            lastName: 'SuperAdmin',
            roles: ['SuperAdmin'],
          })
        );
      });

      const saRoutes = [
        '/dashboard/superadmin',
        '/dashboard/audit-logs',
        '/dashboard/users',
        '/dashboard/billing',
        '/dashboard/visits',
      ];

      for (const url of saRoutes) {
        await page.goto(url);
        expect(page.url()).not.toContain('/unauthorized');
      }
    });
  });

  test.describe('Authentication Requirement', () => {
    test('Unauthenticated user navigating directly to any protected route is redirected to /login', async ({ page }) => {
      await page.addInitScript(() => {
        localStorage.clear();
      });

      await page.goto('/dashboard/visits');
      await page.waitForURL('**/login');
      await expect(page.locator('input[type="password"]')).toBeVisible();
    });
  });
});
