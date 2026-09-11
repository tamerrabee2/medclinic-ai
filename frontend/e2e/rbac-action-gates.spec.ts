import { test, expect } from '@playwright/test';

test.describe('RBAC Action-Level Permission Gates', () => {

  test.describe('Prescriptions Page Action Gates', () => {
    test('Nurse has Read access to Prescriptions, but Sign and Add actions are hidden', async ({ page }) => {
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

      await page.goto('/dashboard/prescriptions');
      await page.waitForLoadState('networkidle');

      // The page itself must load and be visible
      await expect(page.getByText('Electronic Prescriptions & Drug Safety Studio')).toBeVisible();

      // Read-only action should be visible
      await expect(page.getByRole('button', { name: /Preview Official Rx/i })).toBeVisible();

      // Sensitive actions must NOT be rendered in DOM for Nurse
      await expect(page.getByRole('button', { name: /Sign & Transmit Rx/i })).not.toBeVisible();
      await expect(page.getByRole('button', { name: /Add to Active Prescription/i })).not.toBeVisible();
    });

    test('Doctor has full access to Sign and Add actions on Prescriptions', async ({ page }) => {
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

      await page.goto('/dashboard/prescriptions');
      await page.waitForLoadState('networkidle');

      // Both actions MUST be visible to Doctor
      await expect(page.getByRole('button', { name: /Sign & Transmit Rx/i })).toBeVisible();
      await expect(page.getByRole('button', { name: /Add to Active Prescription/i })).toBeVisible();
    });
  });

  test.describe('Patients Page Action Gates', () => {
    test('Receptionist has access to Add New Patient', async ({ page }) => {
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

      await page.goto('/dashboard/patients');
      await page.waitForLoadState('networkidle');

      await expect(page.getByRole('button', { name: /Add New Patient/i })).toBeVisible();
    });
  });

  test.describe('Clinical Visits Action Gates', () => {
    test('Doctor can see Finalize & Save Encounter action', async ({ page }) => {
      await page.addInitScript(() => {
        localStorage.setItem('medclinic_token', 'demo_token_doctor');
        localStorage.setItem(
          'medclinic_user',
          JSON.stringify({
            id: 'demo-doctor-1',
            email: 'doctor@medclinic.ai',
            roles: ['Doctor'],
          })
        );
      });

      await page.goto('/dashboard/visits');
      await page.waitForLoadState('networkidle');

      await expect(page.getByRole('button', { name: /Finalize & Save Encounter/i })).toBeVisible();
    });
  });

});
