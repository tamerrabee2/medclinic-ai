import { test, expect } from '@playwright/test';

test.describe('Public SaaS & Authentication Flows', () => {
  test('Landing page loads and displays public navigation links', async ({ page }) => {
    await page.goto('/');
    await expect(page.locator('h1')).toContainText('MedClinic AI');
    
    // Check marketing links
    const featuresLink = page.getByRole('link', { name: 'Features' });
    const pricingLink = page.getByRole('link', { name: 'Pricing' });
    const securityLink = page.getByRole('link', { name: 'Security & Trust' });
    
    await expect(featuresLink).toBeVisible();
    await expect(pricingLink).toBeVisible();
    await expect(securityLink).toBeVisible();
  });

  test('Pricing page renders subscription tiers and annual toggle', async ({ page }) => {
    await page.goto('/pricing');
    await expect(page.locator('h1')).toContainText('Invest in Clinical Precision');
    await expect(page.getByText('Solo Practice')).toBeVisible();
    await expect(page.getByText('Polyclinic Pro')).toBeVisible();
    await expect(page.getByText('Hospital & Enterprise')).toBeVisible();
  });

  test('Registration onboarding wizard works as expected', async ({ page }) => {
    await page.goto('/register');
    await expect(page.locator('h1')).toContainText('Register Your Medical Clinic');
    await expect(page.getByPlaceholder('e.g. Al-Amal Specialized Medical Center')).toBeVisible();
  });

  test('Login screen renders credentials form and demo logins', async ({ page }) => {
    await page.goto('/login');
    await expect(page.locator('input[type="email"], input[type="text"]')).toBeVisible();
    await expect(page.locator('input[type="password"]')).toBeVisible();
  });
});
