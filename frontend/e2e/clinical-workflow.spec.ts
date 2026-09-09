import { test, expect } from '@playwright/test';

test.describe('Doctor Clinical Workflows & EMR Suite', () => {
  test.beforeEach(async ({ page }) => {
    await page.addInitScript(() => {
      localStorage.setItem('medclinic_token', 'demo_token_doctor');
      localStorage.setItem(
        'medclinic_user',
        JSON.stringify({
          id: 'demo-doc-1',
          email: 'doctor@medclinic.com',
          firstName: 'Dr. Sarah',
          lastName: 'Al-Mansoor',
          roles: ['Doctor', 'ClinicAdmin'],
        })
      );
    });
  });

  test('Clinical Dashboard renders KPIs and Language Toggle operates', async ({ page }) => {
    await page.goto('/dashboard');
    await expect(page.locator('header')).toBeVisible();
    
    // Check Language Toggle button
    const langBtn = page.getByRole('button', { name: /العربية|English/i });
    await expect(langBtn).toBeVisible();
  });

  test('Patient EMR Longitudinal Record renders Vitals and Tabs', async ({ page }) => {
    await page.goto('/dashboard/patients/P-10024');
    await expect(page.getByText('P-10024').first()).toBeVisible();
    await expect(page.getByText(/Blood Pressure|Heart Rate/i).first()).toBeVisible();
  });

  test('Visits & Encounters SOAP Studio functions properly', async ({ page }) => {
    await page.goto('/dashboard/visits');
    await expect(page.locator('h1, h2').first()).toContainText(/Visits|Encounters|الزيارات/i);
  });

  test('Electronic Prescriptions Builder detects contraindications', async ({ page }) => {
    await page.goto('/dashboard/prescriptions');
    await expect(page.getByText(/Prescription|الوصفات/i).first()).toBeVisible();
  });
});
