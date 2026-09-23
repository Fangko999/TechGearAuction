import { Page, expect } from '@playwright/test';

export const TEST_USERS = {
  admin: {
    email: 'admin@techgear.com',
    password: 'Password123!',
    role: 'Admin',
    name: 'Admin'
  },
  seller1: {
    email: 'seller1@techgear.com',
    password: 'Password123!',
    role: 'Seller',
    name: 'Gear Store VN'
  },
  seller2: {
    email: 'seller2@techgear.com',
    password: 'Password123!',
    role: 'Seller',
    name: 'Flashlight Pro'
  },
  buyer1: {
    email: 'buyer1@techgear.com',
    password: 'Password123!',
    role: 'Buyer',
    name: 'Nguyễn Văn Mua'
  },
  buyer2: {
    email: 'buyer2@techgear.com',
    password: 'Password123!',
    role: 'Buyer',
    name: 'Trần Thị Bid'
  },
};

export async function loginUser(page: Page, email: string, password: string = 'Password123!') {
  await page.goto('/login');
  await page.waitForLoadState('domcontentloaded');

  await page.fill('input[type="email"]', email);
  await page.fill('input[type="password"]', password);
  await page.click('button[type="submit"]');

  // Wait for redirect away from /login
  await expect(page).not.toHaveURL(/\/login/, { timeout: 15000 });
}

export async function injectBannedDevice(page: Page, bannedHash: string = 'DEV-BANNED-HARDWARE-HASH') {
  // Navigate to domain first so context can set storage & cookies
  await page.goto('/login');
  
  // Set localStorage deviceHash
  await page.evaluate((hash) => {
    localStorage.setItem('deviceHash', hash);
  }, bannedHash);

  // Set isBanned cookie which triggers Next.js middleware protection
  await page.context().addCookies([
    {
      name: 'isBanned',
      value: 'true',
      domain: 'localhost',
      path: '/',
    },
    {
      name: 'banReason',
      value: 'Phần cứng bị cấm vĩnh viễn do gian lận đấu giá (Device Blacklisted)',
      domain: 'localhost',
      path: '/',
    },
    {
      name: 'bannedEmail',
      value: 'cheater@spam.com',
      domain: 'localhost',
      path: '/',
    }
  ]);
}
