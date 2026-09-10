import { test, expect } from '@playwright/test';

test.describe('Phase 9.1: Super Admin Platform Console & Multi-Tenant Governance', () => {
  test.beforeEach(async ({ page }) => {
    await page.addInitScript(() => {
      localStorage.setItem('medclinic_token', 'demo_token_superadmin');
      localStorage.setItem(
        'medclinic_user',
        JSON.stringify({
          id: 'demo-superadmin-1',
          email: 'superadmin@medclinic.ai',
          firstName: 'Super',
          lastName: 'Admin',
          roles: ['SuperAdmin'],
        })
      );
    });
  });

  test('Super Admin Dashboard renders KPI cards and platform overview', async ({ page }) => {
    await page.goto('/dashboard/superadmin');

    // 1. Verify Page Title
    await expect(page.getByText('Super Admin Platform Console')).toBeVisible();

    // 2. Verify Key Metric KPI cards
    await expect(page.locator('#stat-total-clinics')).toBeVisible();
    await expect(page.locator('#stat-total-doctors')).toBeVisible();
    await expect(page.locator('#stat-ai-inferences')).toBeVisible();
    await expect(page.locator('#stat-dicom-studies')).toBeVisible();

    // 3. Verify Active Subscriptions by Tier
    await expect(page.getByText('Active Subscriptions by Tier')).toBeVisible();
    await expect(page.getByText('Basic Tier')).toBeVisible();
    await expect(page.getByText('Pro Tier')).toBeVisible();
    await expect(page.getByText('Enterprise Tier')).toBeVisible();
  });

  test('Tenant Directory allows searching and filtering by lifecycle status', async ({ page }) => {
    await page.goto('/dashboard/superadmin');

    // Switch to Tenants tab
    const tenantsTab = page.locator('#tab-superadmin-tenants');
    await expect(tenantsTab).toBeVisible();
    await tenantsTab.click();

    // Verify search input
    const searchInput = page.locator('#tenant-search-input');
    await expect(searchInput).toBeVisible();
    await searchInput.fill('Al-Amal');

    // Verify table renders filtered clinic
    await expect(page.locator('#table-tenants')).toBeVisible();
    await expect(page.locator('#table-tenants').getByText(/Al-Amal Medical Center/i)).toBeVisible();

    // Clear search
    await searchInput.fill('');

    // Filter by Suspended
    const filterSuspended = page.locator('#filter-status-suspended');
    await filterSuspended.click();
    await expect(page.locator('#table-tenants').getByText(/Oasis Dental Practice/i)).toBeVisible();
  });

  test('Provisioning Modal opens with auto-slug generation and plan options', async ({ page }) => {
    await page.goto('/dashboard/superadmin');

    // Open Provision Modal
    const openBtn = page.locator('#btn-open-provision-modal');
    await expect(openBtn).toBeVisible();
    await openBtn.click();

    // Verify Modal elements
    await expect(page.locator('#modal-provision-clinic')).toBeVisible();
    await expect(page.getByText('Provision New Clinic Tenant')).toBeVisible();

    // Test Auto-slug generation
    const nameInput = page.locator('#input-clinic-name');
    const slugInput = page.locator('#input-clinic-slug');

    await nameInput.fill('Alexandria Specialized Clinic');
    await expect(slugInput).toHaveValue('alexandria-specialized-clinic');

    // Check Plan select
    const planSelect = page.locator('#select-clinic-plan');
    await expect(planSelect).toBeVisible();
    await planSelect.selectOption('enterprise');

    // Cancel modal
    await page.getByRole('button', { name: /Cancel/i }).click();
    await expect(page.locator('#modal-provision-clinic')).not.toBeVisible();
  });

  test('Subscription Catalog tab displays tiers, quotas, and feature modules', async ({ page }) => {
    await page.goto('/dashboard/superadmin');

    // Switch to Plans tab
    const plansTab = page.locator('#tab-superadmin-plans');
    await expect(plansTab).toBeVisible();
    await plansTab.click();

    // Verify tier cards
    await expect(page.getByRole('heading', { name: 'Basic Clinic' })).toBeVisible();
    await expect(page.getByRole('heading', { name: 'Professional Clinic' })).toBeVisible();
    await expect(page.getByRole('heading', { name: 'Enterprise Hospital' })).toBeVisible();
  });
});
