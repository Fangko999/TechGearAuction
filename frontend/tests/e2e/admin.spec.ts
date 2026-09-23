import { test, expect } from '@playwright/test';
import { TEST_USERS, loginUser } from './helpers';

test.describe('Luồng Phân xử Tố cáo & Quản trị Hệ thống (Admin Disputes & Management)', () => {

  test.beforeEach(async ({ page }) => {
    // Đăng nhập quyền Admin
    await loginUser(page, TEST_USERS.admin.email, TEST_USERS.admin.password);
  });

  test('Tổng quan Dashboard: Tải thành công các chỉ số đo lường hệ thống (Overview Metrics)', async ({ page }) => {
    await page.goto('/admin');
    await page.waitForLoadState('domcontentloaded');

    await expect(page.locator('h1:has-text("Tổng quan Hệ thống")')).toBeVisible();

    // Verify 4 thẻ thống kê metrics
    await expect(page.locator('text=Người dùng')).toBeVisible();
    await expect(page.locator('text=Đấu giá đang diễn ra')).toBeVisible();
    await expect(page.locator('text=Đơn tố cáo (Pending)')).toBeVisible();
    await expect(page.locator('text=Tổng giao dịch')).toBeVisible();
  });

  test('Phân xử Tố cáo: Xác minh giao diện 2 cột (MinIO Evidences & Raw Chat History Bypass Privacy) và thực thi Direct Ban', async ({ page }) => {
    await page.goto('/admin/reports');
    await page.waitForLoadState('domcontentloaded');

    await expect(page.locator('h1:has-text("Quản lý Đơn Tố Cáo")')).toBeVisible();

    // 1. Kiểm tra danh sách đơn tố cáo tồn tại ít nhất 1 đơn
    const disputeRow = page.locator('table tbody tr').first();
    await expect(disputeRow).toBeVisible({ timeout: 15000 });

    // 2. Click nút "Phân xử" để mở Modal kiểm tra bằng chứng
    const resolveButton = disputeRow.locator('button:has-text("Phân xử")');
    await expect(resolveButton).toBeVisible();
    await resolveButton.click();

    // 3. Verify Modal mở ra thành công
    await expect(page.locator('h3:has-text("Chi tiết Tố cáo")')).toBeVisible({ timeout: 10000 });

    // 4. Verify CỘT TRÁI: Bằng chứng (Media) load từ MinIO
    const leftColumnEvidence = page.locator('h4:has-text("Bằng chứng (Media)")');
    await expect(leftColumnEvidence).toBeVisible();
    
    // Kiểm tra có container ảnh bằng chứng hoặc thông báo ảnh
    const evidenceSection = page.locator('div:has(> h4:has-text("Bằng chứng (Media)"))');
    await expect(evidenceSection).toBeVisible();

    // 5. Verify CỘT PHẢI: Lịch sử Chat Thô (Bypass Privacy) - Confidential
    const rightColumnChat = page.locator('h4:has-text("Lịch sử Chat Thô (Bypass Privacy)")');
    await expect(rightColumnChat).toBeVisible();
    await expect(page.locator('text=Confidential')).toBeVisible();

    // Xác minh có tin nhắn chat hiển thị trong khung đối chiếu
    const chatContainer = page.locator('div:has(> h4:has-text("Lịch sử Chat Thô"))');
    await expect(chatContainer).toBeVisible();

    // 6. Nhập Admin Note bắt buộc trước khi thực thi Ban
    const adminNoteInput = page.locator('input[placeholder*="Ghi chú của Admin"]');
    await expect(adminNoteInput).toBeVisible();
    await adminNoteInput.fill('Xác nhận hành vi gửi hàng lỗi và né tránh liên lạc. Thực hiện cấm tài khoản vĩnh viễn.');

    // 7. Lắng nghe hộp thoại browser confirm() khi bấm nút Ban
    page.once('dialog', async (dialog) => {
      expect(dialog.message()).toContain('Khóa (Ban)');
      await dialog.accept();
    });

    // 8. Thực thi nút Direct Ban (Khóa Tài Khoản/Thiết Bị)
    const directBanBtn = page.locator('button:has-text("Khóa Tài Khoản/Thiết Bị (Ban)")');
    await expect(directBanBtn).toBeVisible();
    await directBanBtn.click();
  });

  test('Quản lý Người dùng: Tra cứu danh sách, xem trạng thái người dùng', async ({ page }) => {
    await page.goto('/admin/users');
    await page.waitForLoadState('domcontentloaded');

    await expect(page.locator('h1:has-text("Quản lý Người dùng")')).toBeVisible();

    // Kiểm tra ô tìm kiếm người dùng
    const searchInput = page.locator('input[placeholder*="Tìm kiếm"]');
    await expect(searchInput).toBeVisible();
    await searchInput.fill('Gear Store VN');

    // Verify bảng hiển thị kết quả
    await expect(page.locator('text=seller1@techgear.com')).toBeVisible({ timeout: 10000 });
  });

  test('Danh sách Thiết bị Blacklist: Hiển thị các mã DeviceHash bị cấm', async ({ page }) => {
    await page.goto('/admin/blacklist');
    await page.waitForLoadState('domcontentloaded');

    await expect(page.locator('h1:has-text("Danh sách Thiết bị Blacklist")')).toBeVisible();
    await expect(page.locator('table')).toBeVisible();
  });
});
