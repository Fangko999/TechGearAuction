import { test, expect } from '@playwright/test';
import { TEST_USERS, loginUser, injectBannedDevice } from './helpers';

test.describe('Luồng Xác thực & Chống Spam / Kháng Cáo (Anti-Spam & Appeal)', () => {

  test('Đăng nhập thất bại khi sai mật khẩu - hiển thị thông báo lỗi', async ({ page }) => {
    await page.goto('/login');
    await page.fill('input[type="email"]', TEST_USERS.buyer1.email);
    await page.fill('input[type="password"]', 'WrongPassword999!');
    await page.click('button[type="submit"]');

    // Verify error notification is shown
    const errorMessage = page.locator('text=Email hoặc mật khẩu không chính xác');
    await expect(errorMessage).toBeVisible({ timeout: 10000 });
  });

  test('Đăng nhập thành công với tài khoản Admin - điều hướng tới /admin', async ({ page }) => {
    await loginUser(page, TEST_USERS.admin.email, TEST_USERS.admin.password);
    await expect(page).toHaveURL(/\/admin/);
    await expect(page.locator('text=Tổng quan Hệ thống')).toBeVisible();
  });

  test('Đăng nhập thành công với tài khoản Buyer - điều hướng về trang chủ', async ({ page }) => {
    await loginUser(page, TEST_USERS.buyer1.email, TEST_USERS.buyer1.password);
    await expect(page).toHaveURL('/');
  });

  test('Đăng ký tài khoản mới hợp lệ', async ({ page }) => {
    const randomId = Math.random().toString(36).substring(2, 7);
    const newEmail = `user_${randomId}@techgear-test.com`;

    await page.goto('/register');
    await page.fill('input[placeholder="Ví dụ: John Doe"]', `Test User ${randomId}`);
    await page.fill('input[placeholder="Email của bạn"]', newEmail);
    await page.fill('input[placeholder="Mật khẩu (ít nhất 6 ký tự)"]', 'Password123!');
    await page.fill('input[placeholder="Nhập lại mật khẩu"]', 'Password123!');
    
    // Listen for dialog if any alert is triggered
    page.once('dialog', dialog => dialog.dismiss());
    await page.click('button[type="submit"]');

    // Should redirect to /login after successful registration
    await expect(page).toHaveURL(/\/login/, { timeout: 15000 });
  });

  test('Chống Spam: Trình duyệt bị gắn DeviceHash Banned - Middleware chặn đứng và ép redirect về /appeal', async ({ page }) => {
    // 1. Inject banned device state & blacklist mark
    await injectBannedDevice(page, 'BANNED_MAC_DEVICE_001');

    // 2. Thử truy cập vào route bảo vệ của Buyer
    await page.goto('/buyer/bids');

    // 3. Verify Next.js Middleware chặn đứng và tự động redirect về /appeal (Trại giam)
    await expect(page).toHaveURL(/\/appeal/, { timeout: 10000 });

    // 4. Verify giao diện Trại giam hiển thị đúng lý do vi phạm
    await expect(page.locator('text=Tài khoản bị vô hiệu hóa')).toBeVisible();
    await expect(page.locator('text=Phần cứng bị cấm vĩnh viễn')).toBeVisible();

    // 5. Thử truy cập trang chủ / hoặc /seller cũng phải bị cưỡng chế quay về /appeal
    await page.goto('/seller/auctions');
    await expect(page).toHaveURL(/\/appeal/, { timeout: 10000 });
  });

  test('Kháng cáo: Giao diện form kháng cáo cho phép nhập lý do và đính kèm bằng chứng', async ({ page }) => {
    await injectBannedDevice(page, 'BANNED_MAC_DEVICE_002');
    await page.goto('/appeal');

    await expect(page.locator('textarea')).toBeVisible();
    await page.fill('textarea', 'Tôi khẳng định đây là sự nhầm lẫn thiết bị. Vui lòng unban cho tôi.');
    
    // Nút gửi kháng cáo
    const submitBtn = page.locator('button[type="submit"]');
    await expect(submitBtn).toBeVisible();
  });
});
