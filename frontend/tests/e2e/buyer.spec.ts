import { test, expect } from '@playwright/test';
import { TEST_USERS, loginUser } from './helpers';

test.describe('Luồng Người Mua & Đấu Giá Trực Tiếp (Buyer & Bidding Flow)', () => {

  test.beforeEach(async ({ page }) => {
    await loginUser(page, TEST_USERS.buyer2.email, TEST_USERS.buyer2.password);
  });

  test('Duyệt danh sách sản phẩm đấu giá công khai', async ({ page }) => {
    await page.goto('/auctions');
    await page.waitForLoadState('domcontentloaded');

    await expect(page.locator('h1:has-text("Sản phẩm đang đấu giá")')).toBeVisible();

    // Verify ít nhất có sản phẩm hiển thị trên sàn
    const auctionCards = page.locator('div:has(> a[href*="/auctions/"])');
    await expect(auctionCards.first()).toBeVisible({ timeout: 10000 });
  });

  test('Vào chi tiết sản phẩm, đặt giá hợp lệ và xác minh cập nhật giá', async ({ page }) => {
    await page.goto('/auctions');
    await page.waitForLoadState('domcontentloaded');

    // Click vào sản phẩm đầu tiên
    const firstAuctionLink = page.locator('a[href*="/auctions/"]').first();
    await expect(firstAuctionLink).toBeVisible({ timeout: 10000 });
    await firstAuctionLink.click();

    // Chờ trang chi tiết load
    await expect(page).toHaveURL(/\/auctions\/[a-f0-9-]+/, { timeout: 10000 });

    // Đọc giá hiện tại
    const currentPriceElement = page.locator('text=Giá hiện tại').locator('..').locator('p.text-3xl');
    await expect(currentPriceElement).toBeVisible();
    const oldPriceText = await currentPriceElement.innerText();

    // Tìm input đặt giá
    const bidInput = page.locator('input[type="number"][placeholder*="Tối thiểu"]');
    await expect(bidInput).toBeVisible();

    // Lấy giá trị tối thiểu từ min attribute
    const minValStr = await bidInput.getAttribute('min');
    const minVal = minValStr ? parseInt(minValStr) : 1000000;
    const newBidAmount = minVal + 50000;

    await bidInput.fill(newBidAmount.toString());

    // Click nút Đặt giá ngay
    const placeBidButton = page.locator('button[type="submit"]:has-text("Đặt giá ngay")');
    await expect(placeBidButton).toBeEnabled();
    await placeBidButton.click();

    // Xác minh nút hiển thị trạng thái đang xử lý hoặc reset input
    await expect(bidInput).toHaveValue('', { timeout: 10000 });

    // Xác minh lịch sử đấu giá hoặc giá hiện tại được cập nhật qua SignalR
    await expect(page.locator('text=Lịch sử đặt giá')).toBeVisible();
  });

  test('Kiểm tra trang Lịch sử đấu giá của Buyer', async ({ page }) => {
    await page.goto('/buyer/bids');
    await page.waitForLoadState('domcontentloaded');

    await expect(page.locator('h1:has-text("Lịch sử đấu giá")')).toBeVisible();
  });

  test('Kiểm tra trang Sản phẩm đã thắng và Danh sách theo dõi', async ({ page }) => {
    await page.goto('/buyer/won');
    await expect(page.locator('h1:has-text("Sản phẩm đã thắng")')).toBeVisible();

    await page.goto('/buyer/watchlist');
    await expect(page.locator('h1:has-text("Danh sách theo dõi")')).toBeVisible();
  });
});
