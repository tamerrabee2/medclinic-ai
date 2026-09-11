import { test, expect } from '@playwright/test';

test.describe('RBAC Anti-Tampering & Authoritative Server Verification', () => {

  test('User tampering with localStorage roles is overridden by /api/v1/auth/me', async ({ page }) => {
    // 1. Intercept /api/v1/auth/me to return authentic, non-admin user credentials
    await page.route('**/api/v1/auth/me', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          succeeded: true,
          data: {
            id: 'usr-real-doctor',
            email: 'doctor@medclinic.ai',
            fullName: 'Dr. Sarah Al-Mansoor',
            roles: ['Doctor'],
            permissions: [
              'Patients.Read',
              'MedicalRecords.Read',
              'Prescriptions.Read',
              'Prescriptions.Sign',
            ],
            clinics: [{ id: 'clinic-1', name: 'Al-Amal Clinic' }],
          },
        }),
      });
    });

    // 2. Inject tampered state into localStorage (attacker elevates their own role to SuperAdmin)
    await page.addInitScript(() => {
      // Non-demo production token
      localStorage.setItem('medclinic_token', 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.e30.fake_prod_signature');
      localStorage.setItem(
        'medclinic_user',
        JSON.stringify({
          id: 'usr-real-doctor',
          email: 'doctor@medclinic.ai',
          firstName: 'Hacker',
          lastName: 'SuperAdmin',
          roles: ['SuperAdmin'], // Forged role!
          permissions: ['SuperAdmin', 'All'], // Forged permissions!
        })
      );
    });

    // 3. Navigate to dashboard
    await page.goto('/dashboard');
    await page.waitForLoadState('networkidle');

    // 4. Verify that Super Admin link is NOT rendered in the sidebar
    await expect(page.getByText('Super Admin')).not.toBeVisible();

    // 5. Attempt direct navigation to SuperAdmin screen -> Must redirect to /unauthorized
    await page.goto('/dashboard/superadmin');
    await page.waitForURL('**/unauthorized');
    await expect(page.getByText('Access Restricted')).toBeVisible();
    await expect(page.getByText('HTTP 403 · FORBIDDEN')).toBeVisible();
  });

  test('Invalid or expired session returned by /api/v1/auth/me purges localStorage and redirects to /login', async ({ page }) => {
    // 1. Intercept /api/v1/auth/me to return 401 Unauthorized
    await page.route('**/api/v1/auth/me', async (route) => {
      await route.fulfill({
        status: 401,
        contentType: 'application/json',
        body: JSON.stringify({ succeeded: false, message: 'Token expired or revoked' }),
      });
    });

    // 2. Set initial token in localStorage directly on /login without re-injecting initScript
    await page.goto('/login');
    await page.evaluate(() => {
      localStorage.setItem('medclinic_token', 'expired_production_jwt_token');
      localStorage.setItem(
        'medclinic_user',
        JSON.stringify({
          id: 'usr-expired',
          email: 'expired@medclinic.ai',
          roles: ['Doctor'],
        })
      );
    });

    // 3. Navigate to dashboard (catches the client-side navigation abort caused by window.location.href = '/login')
    await page.goto('/dashboard').catch(() => {});

    // 4. Should redirect to /login
    await page.waitForURL('**/login');
    await page.waitForLoadState('networkidle');

    // 5. Verify localStorage has been purged
    const storedToken = await page.evaluate(() => localStorage.getItem('medclinic_token'));
    expect(storedToken).toBeNull();
  });

});
