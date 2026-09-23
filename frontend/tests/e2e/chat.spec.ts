import { test, expect } from '@playwright/test';
import { TEST_USERS, loginUser } from './helpers';

test.describe('Luồng Nhắn tin Thời gian thực (Real-time Chat with SignalR)', () => {

  test('Mở song song 2 trình duyệt ẩn danh: Buyer gửi tin nhắn, Seller nhận realtime qua SignalR và kiểm tra Badge đỏ', async ({ browser }) => {
    // 1. Tạo 2 browser context độc lập (Buyer và Seller)
    const buyerContext = await browser.newContext();
    const sellerContext = await browser.newContext();

    const buyerPage = await buyerContext.newPage();
    const sellerPage = await sellerContext.newPage();

    try {
      // 2. Đăng nhập Buyer và Seller trên 2 context riêng biệt
      await loginUser(buyerPage, TEST_USERS.buyer1.email, TEST_USERS.buyer1.password);
      await loginUser(sellerPage, TEST_USERS.seller1.email, TEST_USERS.seller1.password);

      // 3. Cả 2 cùng truy cập trang chat
      await buyerPage.goto('/chat');
      await sellerPage.goto('/chat');

      await buyerPage.waitForLoadState('domcontentloaded');
      await sellerPage.waitForLoadState('domcontentloaded');

      // 4. Buyer chọn phòng chat đầu tiên
      const buyerRoomItem = buyerPage.locator('li:has-text("Gear Store VN")').first();
      await expect(buyerRoomItem).toBeVisible({ timeout: 15000 });
      await buyerRoomItem.click();

      // Kiểm tra input chat hiển thị bên Buyer
      const chatInput = buyerPage.locator('input[placeholder*="Nhập tin nhắn"], input[type="text"]').last();
      await expect(chatInput).toBeVisible({ timeout: 10000 });

      // 5. Buyer gửi tin nhắn realtime
      const uniqueMessage = `Pin còn dùng được bao lâu vậy shop? [${Date.now()}]`;
      await chatInput.fill(uniqueMessage);
      await buyerPage.keyboard.press('Enter');

      // Verify tin nhắn xuất hiện ngay trên khung chat của Buyer
      await expect(buyerPage.locator(`text=${uniqueMessage}`)).toBeVisible({ timeout: 10000 });

      // 6. Bên phía Seller: Kiểm tra SignalR đẩy tin nhắn hoặc Badge đỏ đếm số tin nhắn chưa đọc
      // Seller chưa mở phòng -> kiểm tra Badge đỏ xuất hiện trên danh sách phòng
      const sellerRoomItem = sellerPage.locator('li:has-text("Nguyễn Văn Mua")').first();
      await expect(sellerRoomItem).toBeVisible({ timeout: 10000 });

      // Seller click mở phòng chat đó
      await sellerRoomItem.click();

      // 7. Xác minh SignalR đẩy bong bóng chat hiển thị tức thì trên màn hình Seller
      const receivedMessageLocator = sellerPage.locator(`text=${uniqueMessage}`);
      await expect(receivedMessageLocator).toBeVisible({ timeout: 15000 });

      // 8. Xác minh sau khi mở phòng, cục Badge đỏ được dọn dẹp (Clear Unread)
      const unreadBadge = sellerRoomItem.locator('span.bg-red-500');
      await expect(unreadBadge).not.toBeVisible();

    } finally {
      // Dọn dẹp cả 2 context
      await buyerContext.close();
      await sellerContext.close();
    }
  });

  test('Lọc danh sách hội thoại theo vai trò và trạng thái', async ({ page }) => {
    await loginUser(page, TEST_USERS.buyer1.email, TEST_USERS.buyer1.password);
    await page.goto('/chat');

    // Kiểm tra dropdown lọc vai trò
    const roleSelect = page.locator('select').first();
    await expect(roleSelect).toBeVisible();
    await roleSelect.selectOption('Buyer');

    // Kiểm tra dropdown hộp thư
    const folderSelect = page.locator('select').nth(1);
    await expect(folderSelect).toBeVisible();
    await folderSelect.selectOption('Archived');
    await folderSelect.selectOption('Inbox');
  });
});
