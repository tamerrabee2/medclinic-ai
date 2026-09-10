import { test, expect } from '@playwright/test';

test.describe('RBAC Navigation & Least Privilege Visibility', () => {
  test('SuperAdmin sees Super Admin platform console and full administrative navigation', async ({ page }) => {
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

    await page.goto('/dashboard');

    // Super Admin link must be visible
    const superAdminLink = page.locator('aside a[href="/dashboard/superadmin"]');
    await expect(superAdminLink).toBeVisible();

    // Standard clinical links should also be visible
    await expect(page.locator('aside a[href="/dashboard/patients"]')).toBeVisible();
    await expect(page.locator('aside a[href="/dashboard/billing"]')).toBeVisible();
    await expect(page.locator('aside a[href="/dashboard/audit-logs"]')).toBeVisible();
  });

  test('Doctor sees clinical tools and AI assistant, but Super Admin console and User management are hidden', async ({ page }) => {
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

    await page.goto('/dashboard');

    // Clinical & AI links must be visible
    await expect(page.locator('aside a[href="/dashboard/patients"]')).toBeVisible();
    await expect(page.locator('aside a[href="/dashboard/visits"]')).toBeVisible();
    await expect(page.locator('aside a[href="/dashboard/prescriptions"]')).toBeVisible();
    await expect(page.locator('aside a[href="/dashboard/ai-assistant"]')).toBeVisible();
    await expect(page.locator('aside a[href="/dashboard/billing"]')).toBeVisible();

    // Super Admin console and User Accounts must NOT be visible
    await expect(page.locator('aside a[href="/dashboard/superadmin"]')).toHaveCount(0);
    await expect(page.locator('aside a[href="/dashboard/users"]')).toHaveCount(0);
  });

  test('Nurse sees patients, visits and labs, but cannot see Super Admin, Prescriptions or AI Assistant', async ({ page }) => {
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

    await page.goto('/dashboard');

    // Authorized links
    await expect(page.locator('aside a[href="/dashboard/patients"]')).toBeVisible();
    await expect(page.locator('aside a[href="/dashboard/visits"]')).toBeVisible();
    await expect(page.locator('aside a[href="/dashboard/appointments"]')).toBeVisible();

    // Unauthorized links must be hidden
    await expect(page.locator('aside a[href="/dashboard/superadmin"]')).toHaveCount(0);
    await expect(page.locator('aside a[href="/dashboard/prescriptions"]')).toHaveCount(0);
    await expect(page.locator('aside a[href="/dashboard/ai-assistant"]')).toHaveCount(0);
    await expect(page.locator('aside a[href="/dashboard/audit-logs"]')).toHaveCount(0);
  });

  test('Receptionist sees front desk items only; visits, diagnostics and super admin are completely hidden', async ({ page }) => {
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

    await page.goto('/dashboard');

    // Receptionist permitted links
    await expect(page.locator('aside a[href="/dashboard/patients"]')).toBeVisible();
    await expect(page.locator('aside a[href="/dashboard/appointments"]')).toBeVisible();
    await expect(page.locator('aside a[href="/dashboard/billing"]')).toBeVisible();

    // Clinical, Diagnostic, and Governance links must be completely hidden
    await expect(page.locator('aside a[href="/dashboard/visits"]')).toHaveCount(0);
    await expect(page.locator('aside a[href="/dashboard/canvas"]')).toHaveCount(0);
    await expect(page.locator('aside a[href="/dashboard/dental"]')).toHaveCount(0);
    await expect(page.locator('aside a[href="/dashboard/prescriptions"]')).toHaveCount(0);
    await expect(page.locator('aside a[href="/dashboard/ai-assistant"]')).toHaveCount(0);
    await expect(page.locator('aside a[href="/dashboard/laboratory"]')).toHaveCount(0);
    await expect(page.locator('aside a[href="/dashboard/radiology"]')).toHaveCount(0);
    await expect(page.locator('aside a[href="/dashboard/superadmin"]')).toHaveCount(0);
    await expect(page.locator('aside a[href="/dashboard/audit-logs"]')).toHaveCount(0);

    // AI & Diagnostics section header should also be omitted because all items inside are unauthorized
    await expect(page.locator('aside').getByText(/AI & Diagnostics|الذكاء الاصطناعي/i)).toHaveCount(0);
  });
});
