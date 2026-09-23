import { NextResponse } from 'next/server';
import type { NextRequest } from 'next/server';

export function middleware(request: NextRequest) {
  const { pathname } = request.nextUrl;
  
  const token = request.cookies.get('token')?.value;
  const isBanned = request.cookies.get('isBanned')?.value === 'true';
  let userData: { role?: string } | null = null;
  
  try {
    const userCookie = request.cookies.get('user_data')?.value;
    if (userCookie) {
      userData = JSON.parse(userCookie);
    }
  } catch (e) {
    // ignore
  }

  // 1. Luồng Anti-Spam: Nếu bị Ban, cấm mọi ngả, đẩy vào Trại Giam (/appeal)
  if (isBanned) {
    // Chỉ cho phép vào trang /appeal và các route public không làm thay đổi trạng thái (như login, dù login sẽ lại redirect sang appeal)
    if (!pathname.startsWith('/appeal')) {
      return NextResponse.redirect(new URL('/appeal', request.url));
    }
    return NextResponse.next();
  }

  // Nếu người dùng không bị ban nhưng đang cố vào trang appeal -> Đẩy về trang chủ
  if (pathname.startsWith('/appeal') && !isBanned) {
    return NextResponse.redirect(new URL('/', request.url));
  }

  // 2. Bảo vệ Route chưa đăng nhập
  const isProtectedPath = pathname.startsWith('/buyer') || pathname.startsWith('/seller') || pathname.startsWith('/admin') || pathname.startsWith('/chat');
  
  if (isProtectedPath && !token) {
    return NextResponse.redirect(new URL('/login', request.url));
  }

  // 3. Phân quyền Role (Đăng nhập rồi nhưng sai Role)
  if (token && userData?.role) {
    if (pathname.startsWith('/admin') && userData.role !== 'Admin') {
      return NextResponse.redirect(new URL('/', request.url));
    }
    
    // Nếu vào /login khi đã đăng nhập
    if (pathname.startsWith('/login') || pathname.startsWith('/register')) {
      if (userData.role === 'Admin') {
        return NextResponse.redirect(new URL('/admin', request.url));
      } else {
        return NextResponse.redirect(new URL('/', request.url));
      }
    }
  }

  return NextResponse.next();
}

export const config = {
  matcher: [
    /*
     * Match all request paths except for the ones starting with:
     * - api (API routes)
     * - _next/static (static files)
     * - _next/image (image optimization files)
     * - favicon.ico (favicon file)
     */
    '/((?!api|_next/static|_next/image|favicon.ico).*)',
  ],
};

