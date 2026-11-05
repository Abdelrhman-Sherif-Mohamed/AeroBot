import urllib.request
import json
from datetime import datetime

changelog_ar = """• إضافة زر «💾 حفظ الإعدادات» في الشريط السفلي وقائمة ملف مع اختصار Ctrl+S.
• حل مشكلة إعادة ضبط الإعدادات عند إعادة تشغيل البوت وثبات ملفات التعريف وحفظها تلقائياً.
• إصلاح عرض عناصر الشنطة ومنع اختفاء وتداخل الصفوف العلوية أسفل شريط الأزرار.
• إصلاح عرض نتائج فلتر البيع والتخزين في قسم الإيتمات وتأمين استرجاع البيانات.
• إزالة أيقونة الترس من شريط العنوان العلوي بناءً على طلبكم.
• إضافة زر «⚠️ قفل اشتراك الجميع» في لوحة تحكم الأدمن."""

changelog_en = """• Added dedicated 'Save Settings' button in bottom bar and File menu (Ctrl+S).
• Fixed settings reset issue across bot restarts and ensured profile persistence.
• Fixed inventory list layout preventing top rows from clipping under buttons.
• Fixed items filter display to reliably show and populate all search results.
• Removed gear menu icon from top title bar.
• Added 'Expire All Subscriptions' button in Admin Dashboard."""

data = {
    "version": "2.9.39",
    "downloadUrl": "https://raw.githubusercontent.com/Abdelrhman-Sherif-Mohamed/AeroBot/main/Releases/update_2.9.39.zip",
    "mandatory": True,
    "changelog_ar": changelog_ar,
    "changelog_en": changelog_en,
    "releasedAt": datetime.now().strftime("%Y-%m-%d %H:%M:%S")
}

body = json.dumps(data, ensure_ascii=False).encode('utf-8')

req = urllib.request.Request(
    'https://wolves-1eb1f-default-rtdb.firebaseio.com/update.json',
    data=body,
    headers={'Content-Type': 'application/json; charset=utf-8'},
    method='PUT'
)

with urllib.request.urlopen(req) as res:
    print('Published 2.9.39 update to Firebase! Status:', res.status)
