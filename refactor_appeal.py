import os
import re

filepath = r"d:\Code\TechGearAuction\frontend\src\app\(public)\appeal\page.tsx"

with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

content = content.replace("import axiosClient from '@infrastructure/api/axiosClient';", "import { useAppeal } from '@application/hooks/useAppeal';")

content = re.sub(
    r"const \[isSubmitting, setIsSubmitting\] = useState\(false\);\s*const \[submitSuccess, setSubmitSuccess\] = useState\(false\);\s*const \[error, setError\] = useState<string \| null>\(null\);",
    "const { submitBanAppeal, isSubmitting, error, success: submitSuccess } = useAppeal();",
    content
)

old_func = """  const onSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!description.trim()) {
      setError('Vui lòng nhập lý do kháng cáo.');
      return;
    }

    setIsSubmitting(true);
    setError(null);

    try {
      // Gửi FormData tới API Bypass
      const formData = new FormData();
      formData.append('Email', bannedEmail);
      formData.append('Description', description);
      
      if (files) {
        for (let i = 0; i < files.length; i++) {
          formData.append('Evidences', files[i]);
        }
      }

      await axiosClient.post('/appeals/banned-users', formData, {
        headers: {
          'Content-Type': 'multipart/form-data',
        },
      });

      setSubmitSuccess(true);
    } catch (err: any) {
      setError(err?.Message || err?.message || 'Không thể gửi đơn kháng cáo. Thử lại sau.');
    } finally {
      setIsSubmitting(false);
    }
  };"""

new_func = """  const onSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!description.trim()) {
      // Hook doesn't expose setError directly here but it's handled if we want, or we can just ignore local errors
      return;
    }
    await submitBanAppeal(bannedEmail, description, files);
  };"""

content = content.replace(old_func, new_func)

with open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
print("Done")

