import { test, expect } from '@playwright/test';
import path from 'path';
import { TEST_USERS, loginUser } from './helpers';

test.describe('Luồng Người Bán & Upload Ảnh MinIO Thật (Seller Flow)', () => {

  test.beforeEach(async ({ page }) => {
    // Đăng nhập với tài khoản Seller
    await loginUser(page, TEST_USERS.seller2.email, TEST_USERS.seller2.password);
  });

  test('Tạo phiên đấu giá cho Đèn pin Loop Gear SK05 Pro đính kèm ảnh thật (MinIO Multipart)', async ({ page }) => {
    await page.goto('/seller/auctions/create');
    await page.waitForLoadState('domcontentloaded');

    // 1. Điền thông tin chi tiết phần cứng thực tế
    const itemTitle = `Đèn pin Loop Gear SK05 Pro Titanium - ${Date.now()}`;
    await page.fill('input[placeholder*="Đèn pin EDC"]', itemTitle);
    
    await page.fill('textarea[placeholder*="Cung cấp chi tiết"]', 
      'Đèn pin EDC Loop Gear SK05 Pro bản Titanium đa nguồn sáng (Flood, Spot, Sidelight RGB). Mới 99% fullbox đầy đủ phụ kiện pin 18650.'
    );

    // Điền giá tiền
    await page.locator('input[type="number"]').nth(0).fill('1500000'); // Giá khởi điểm: 1.500.000đ
    await page.locator('input[type="number"]').nth(1).fill('50000');   // Bước giá: 50.000đ
    await page.locator('input[type="number"]').nth(2).fill('2200000'); // Mua ngay: 2.200.000đ

    // 2. Upload file ảnh thật (MinIO Multipart)
    const fixturePath = path.resolve(__dirname, '../fixtures/sk05-pro.png');
    const fileChooserPromise = page.waitForEvent('filechooser').catch(() => null);
    
    // Set file input trực tiếp
    const fileInput = page.locator('input[type="file"]');
    await fileInput.setInputFiles(fixturePath);

    // Verify ảnh preview hiển thị trên giao diện
    await expect(page.locator('text=Ảnh chính')).toBeVisible({ timeout: 5000 });

    // 3. Submit form và ép hệ thống xử lý multipart upload lên MinIO
    await page.click('button[type="submit"]');

    // 4. Verify thông báo tạo thành công
    await expect(page.locator('text=Tạo phiên đấu giá thành công!')).toBeVisible({ timeout: 20000 });
  });

  test('Tạo phiên đấu giá cho Bộ tua vít điện Xiaomi Mijia 24-in-1', async ({ page }) => {
    await page.goto('/seller/auctions/create');
    await page.waitForLoadState('domcontentloaded');

    const itemTitle = `Bộ tua vít điện Xiaomi Mijia Precision 24-in-1 - ${Date.now()}`;
    await page.fill('input[placeholder*="Đèn pin EDC"]', itemTitle);
    
    await page.fill('textarea[placeholder*="Cung cấp chi tiết"]', 
      'Bộ tua vít điện mini Xiaomi Mijia gồm 24 đầu vít thép S2 cao cấp, 2 nấc mô-men xoắn, pin sạc Type-C, hộp nhôm nguyên khối sang trọng.'
    );

    await page.locator('input[type="number"]').nth(0).fill('450000');
    await page.locator('input[type="number"]').nth(1).fill('20000');
    await page.locator('input[type="number"]').nth(2).fill('700000');

    // Upload file ảnh thật
    const fixturePath = path.resolve(__dirname, '../fixtures/mijia-screwdriver.png');
    await page.locator('input[type="file"]').setInputFiles(fixturePath);

    await expect(page.locator('text=Ảnh chính')).toBeVisible({ timeout: 5000 });

    await page.click('button[type="submit"]');
    await expect(page.locator('text=Tạo phiên đấu giá thành công!')).toBeVisible({ timeout: 20000 });
  });

  test('Xem kho hàng của Seller - hiển thị danh sách sản phẩm', async ({ page }) => {
    await page.goto('/seller/auctions');
    await page.waitForLoadState('domcontentloaded');

    // Kiểm tra tiêu đề trang
    await expect(page.locator('h1:has-text("Kho hàng của tôi")')).toBeVisible();
    
    // Kiểm tra có nút điều hướng đăng sản phẩm mới
    const createBtn = page.locator('a[href="/seller/auctions/create"]');
    await expect(createBtn).toBeVisible();
  });
});
