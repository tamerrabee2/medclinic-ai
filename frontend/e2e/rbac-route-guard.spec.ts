import { test, expect } from '@playwright/test';

test.describe('RBAC Route Guard & 403 Redirection', () => {
  test('Doctor attempting direct URL access to Super Admin console is redirected to /unauthorized', async ({ page }) => {
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

    await page.goto('/dashboard/superadmin');

    // Must be redirected to /unauthorized
    await page.waitForURL('**/unauthorized');
    await expect(page.getByText('Access Restricted')).toBeVisible();
    await expect(page.getByText('HTTP 403 · FORBIDDEN')).toBeVisible();
    await expect(page.getByText('Doctor', { exact: true })).toBeVisible();

    // Clicking "Return to Dashboard" navigates back safely to /dashboard
    await page.locator('a[href="/dashboard"]').click();
    await page.waitForURL('**/dashboard');
    await expect(page.locator('aside a[href="/dashboard/patients"]')).toBeVisible();
  });

  test('Receptionist attempting direct URL access to Audit Logs is redirected to /unauthorized', async ({ page }) => {
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

    await page.goto('/dashboard/audit-logs');

    // Must be redirected to /unauthorized
    await page.waitForURL('**/unauthorized');
    await expect(page.getByText('Access Restricted')).toBeVisible();
    await expect(page.getByText('Receptionist', { exact: true })).toBeVisible();
  });

  test('Unauthenticated user navigating directly to protected route is redirected to /login', async ({ page }) => {
    // Clear storage
    await page.addInitScript(() => {
      localStorage.clear();
    });

    await page.goto('/dashboard/superadmin');
    await page.waitForURL('**/login');
    await expect(page.locator('input[type="password"]')).toBeVisible();
  });

  test('SuperAdmin can directly access /dashboard/superadmin without redirection', async ({ page }) => {
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

    await page.goto('/dashboard/superadmin');
    await expect(page.getByText('Super Admin Platform Console')).toBeVisible();
    expect(page.url()).toContain('/dashboard/superadmin');
  });
});
