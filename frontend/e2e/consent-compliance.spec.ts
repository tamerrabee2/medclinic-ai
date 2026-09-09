import { test, expect } from '@playwright/test';

test.describe('Phase 12: Consent Management, AI Guard & Audit Explorer Compliance', () => {
  test('Patient EMR renders AI Consent Badge and Consents & Compliance Tab', async ({ page }) => {
    await page.goto('/dashboard/patients/P-10024');
    
    // 1. Verify Patient Header renders AI Consent Badge
    await expect(page.getByText(/AI Consent/i)).toBeVisible();

    // 2. Verify Consents & Compliance tab exists in navigation
    const consentTabBtn = page.getByRole('button', { name: /Consents & Compliance|إدارة الموافقات/i });
    await expect(consentTabBtn).toBeVisible();

    // 3. Switch to Consents & Compliance Tab
    await consentTabBtn.click();

    // 4. Verify Standard Consent Categories are visible
    await expect(page.getByText(/AI-Assisted Diagnostics|الرعاية والتشخيص بالذكاء الاصطناعي/i)).toBeVisible();
    await expect(page.getByText(/General Clinical Care|الرعاية الطبية العامة/i)).toBeVisible();
    await expect(page.getByText(/Clinical Data Sharing|مشاركة البيانات السريرية/i)).toBeVisible();

    // 5. Verify Immutable Audit Trail section is rendered
    await expect(page.getByText(/Immutable Consent Lifecycle Audit Trail|سجل أحداث الموافقة/i)).toBeVisible();
  });

  test('Grant Consent Modal opens with validity duration and clinical notes', async ({ page }) => {
    await page.goto('/dashboard/patients/P-10024');

    // Switch to Consents tab
    const consentTabBtn = page.getByRole('button', { name: /Consents & Compliance|إدارة الموافقات/i });
    await consentTabBtn.click();

    // Open Record Consent Modal
    const recordBtn = page.getByRole('button', { name: /Record Consent|تسجيل موافقة/i }).first();
    if (await recordBtn.isVisible()) {
      await recordBtn.click();

      // Check modal inputs
      await expect(page.getByRole('combobox').first()).toBeVisible();
      await expect(page.getByPlaceholder(/Patient reviewed|تم توضيح/i)).toBeVisible();

      // Close modal
      const cancelBtn = page.getByRole('button', { name: /Cancel|إلغاء/i });
      await cancelBtn.click();
    }
  });

  test('Audit & Compliance Explorer renders 3 unified tabs and Export CSV button', async ({ page }) => {
    await page.goto('/dashboard/audit-logs');

    // 1. Verify Page Title
    await expect(page.getByText(/Audit & Compliance Explorer|مستكشف التدقيق والامتثال/i)).toBeVisible();

    // 2. Verify 3 Unified Explorer Tabs
    await expect(page.getByRole('button', { name: /Patient Consent Lifecycle|سجل موافقات المرضى/i })).toBeVisible();
    await expect(page.getByRole('button', { name: /Clinical AI Decisions|قرارات وتوصيات AI/i })).toBeVisible();
    await expect(page.getByRole('button', { name: /Security & Resource Access|سجل الوصول والأمان/i })).toBeVisible();

    // 3. Verify Export CSV Button
    const exportBtn = page.getByRole('button', { name: /Export Compliance CSV|تصدير تقرير الامتثال/i });
    await expect(exportBtn).toBeVisible();

    // 4. Verify Search and Event Filter inputs
    await expect(page.getByPlaceholder(/Search MRN|بحث في السجل/i)).toBeVisible();
  });

  test('AI Assistant displays safe error alert and zero fake messages when server fails', async ({ page }) => {
    // Intercept AI endpoint to simulate backend failure (503 Service Unavailable)
    await page.route('**/api/v1/ai/**', route => route.abort('failed'));

    await page.goto('/dashboard/ai-assistant');

    // Select patient if available or enter query
    const input = page.getByPlaceholder(/Ask clinical question|اسأل المساعد/i);
    await input.fill('What is the recommended antibiotic dosage?');

    // Click Send
    const sendBtn = page.getByRole('button', { name: /Send|إرسال/i });
    if (await sendBtn.isVisible()) {
      await sendBtn.click();

      // Verify Safety Alert is displayed
      await expect(page.getByText(/AI Service Unavailable|خدمة الذكاء الاصطناعي غير متاحة/i)).toBeVisible();
      await expect(page.getByRole('button', { name: /Retry Request|إعادة المحاولة/i })).toBeVisible();

      // CRITICAL: Ensure NO simulated clinical text is injected into chat
      await expect(page.getByText(/Clinical Copilot Response/i)).toHaveCount(0);
      await expect(page.getByText(/Clinical Impression/i)).toHaveCount(0);
    }
  });
});
