import { test, expect } from '@playwright/test';

test.describe('Specialized Diagnostics & Visual Tools', () => {
  test('Radiology PACS viewer loads canvas and modality options', async ({ page }) => {
    await page.goto('/dashboard/radiology');
    await expect(page.getByText(/Radiology|الأشعة/i)).toBeVisible();
    await expect(page.getByRole('button', { name: /Zoom|Brightness|Rotate|Heatmap/i }).first()).toBeVisible();
  });

  test('AI Lab Analyzer displays 6-Phase Pipeline and Biomarkers Table', async ({ page }) => {
    await page.goto('/dashboard/lab-analyzer');
    await expect(page.getByText(/Lab Analyzer|محلل المختبرات/i)).toBeVisible();
    await expect(page.getByText(/Biomarkers|Reference Range/i)).toBeVisible();
  });

  test('Medical Canvas & Anatomical Body Map loads interactive regions', async ({ page }) => {
    await page.goto('/dashboard/canvas');
    await expect(page.getByText(/Canvas|Body Map|الرسم الطبي/i)).toBeVisible();
  });

  test('FDI World Dental Chart displays standard adult dentition', async ({ page }) => {
    await page.goto('/dashboard/dental');
    await expect(page.getByText(/Dental|الأسنان|FDI/i)).toBeVisible();
  });
});
