import urllib.request
import json
from datetime import datetime

changelog_ar = """• تحسين سرعة وسلاسة القتال والضرب واختيار الوحوش بدون أي تأخير.
• إصلاح شريط أزرار الشنطة وترتيب العرض لمنع تداخل الأزرار مع عناوين وعناصر القائمة.
• إصلاح فلتر البيع والتخزين في قسم الإيتمات لإظهار كافة العناصر والنتائج فوراً.
• نقل قائمة الإعدادات (أيقونة الترس) إلى شريط العنوان العلوي وتنسيق القوائم بالثيم الداكن.
• إزالة صندوق السجل السفلي لتوسيع مساحة العمل والشاشات بالكامل."""

changelog_en = """• Combat and targeting responsiveness optimizations with zero attack delay.
• Fixed inventory top bar layout and subtab z-ordering preventing header overlap.
• Fixed item filter display to reliably show all filtered items instantly.
• Integrated gear settings menu into title bar with full dark theme rendering.
• Removed bottom log panel to provide clean, expanded full-height view."""

data = {
    "version": "2.9.38",
    "downloadUrl": "https://raw.githubusercontent.com/Abdelrhman-Sherif-Mohamed/AeroBot/main/Releases/update_2.9.38.zip",
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
    print('Published 2.9.38 update to Firebase! Status:', res.status)
