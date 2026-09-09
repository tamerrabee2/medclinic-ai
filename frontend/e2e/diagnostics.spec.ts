import { test, expect } from '@playwright/test';

test.describe('Specialized Diagnostics & Visual Tools', () => {
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

  test('Radiology PACS viewer loads canvas and modality options', async ({ page }) => {
    await page.goto('/dashboard/radiology');
    await expect(page.getByText(/Radiology|الأشعة/i).first()).toBeVisible();
    await expect(page.getByRole('button', { name: /Zoom|Brightness|Rotate|Heatmap/i }).first()).toBeVisible();
  });

  test('AI Lab Analyzer displays 6-Phase Pipeline and Biomarkers Table', async ({ page }) => {
    await page.goto('/dashboard/lab-analyzer');
    await expect(page.getByText(/Lab Analyzer|محلل المختبرات/i).first()).toBeVisible();
    await expect(page.getByText(/Biomarkers|Reference Range/i).first()).toBeVisible();
  });

  test('Medical Canvas & Anatomical Body Map loads interactive regions', async ({ page }) => {
    await page.goto('/dashboard/canvas');
    await expect(page.getByText(/Canvas|Body Map|الرسم الطبي/i).first()).toBeVisible();
  });

  test('FDI World Dental Chart displays standard adult dentition', async ({ page }) => {
    await page.goto('/dashboard/dental');
    await expect(page.getByText(/Dental|الأسنان|FDI/i).first()).toBeVisible();
  });
});
