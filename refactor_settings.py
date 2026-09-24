import os
import re

filepath = r"d:\Code\TechGearAuction\frontend\src\app\seller\settings\page.tsx"

with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

# Imports
content = content.replace("import axiosClient from '@infrastructure/api/axiosClient';", "import { useChangePassword } from '@application/hooks/useChangePassword';")

# States
content = re.sub(
    r"const \[message, setMessage\] = useState\(''\);\s*const \[error, setError\] = useState\(''\);\s*const \[isLoading, setIsLoading\] = useState\(false\);",
    "const { changePassword, isLoading, error, successMessage: message, setError } = useChangePassword();",
    content
)

# Function body
old_func = """  const handleUpdatePassword = async (e: React.FormEvent) => {
    e.preventDefault();
    if (newPassword !== confirmPassword) {
      setError('Mật khẩu xác nhận không khớp.');
      return;
    }

    setIsLoading(true);
    setError('');
    setMessage('');

    try {
      await axiosClient.put('/users/me/change-password', {
        currentPassword,
        newPassword
      });
      setMessage('Đổi mật khẩu thành công!');
      setCurrentPassword('');
      setNewPassword('');
      setConfirmPassword('');
    } catch (err: any) {
      setError(err?.response?.data?.Message || 'Đã có lỗi xảy ra.');
    } finally {
      setIsLoading(false);
    }
  };"""

new_func = """  const handleUpdatePassword = async (e: React.FormEvent) => {
    e.preventDefault();
    if (newPassword !== confirmPassword) {
      setError('Mật khẩu xác nhận không khớp.');
      return;
    }

    const success = await changePassword(currentPassword, newPassword);
    if (success) {
      setCurrentPassword('');
      setNewPassword('');
      setConfirmPassword('');
    }
  };"""

content = content.replace(old_func, new_func)

with open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
print("Done")

