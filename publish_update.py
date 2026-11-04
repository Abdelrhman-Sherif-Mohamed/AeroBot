import urllib.request
import json
from datetime import datetime

changelog_ar = """• إصلاح ظهور زر Order في الشريط العلوي للمخزن:
  - حل مشكلة المربع الشفاف الفارغ فوق زر Inventory وتحديد إحداثيات ومكان وحجم زر Order (ترتيب المستودع) بدقة ليكون بعد زر Happy.
  - إضافة واجهة مخصصة بالكامل لترتيب المستودع (Storage Order) بتقسيم الأغراض تلقائياً على 5 صفحات (أسلحة وعتاد / إكسيرات وتطوير / أحجار سحرية وتثبيت / جرعات واسترجاع / بطاقات ومخطوطات وأخرى).
  - خيارات تفعيل الترتيب التلقائي وأزرار للترتيب الفوري لكل من المستودع الشخصي ومستودع الجيلد."""

changelog_en = """• Storage Order Button & Layout UI Fix:
  - Fixed ghost empty box appearing over Inventory button by assigning proper bounds and text to the Order button after Happy tab.
  - Dedicated Storage Order view categorizing items across 5 distinct pages (Equipment / Elixirs / Stones / Potions / Scrolls & Others).
  - Toggle options for auto-ordering and instant manual order buttons for Storage and Guild Storage."""

data = {
    "version": "2.9.35",
    "downloadUrl": "https://raw.githubusercontent.com/Abdelrhman-Sherif-Mohamed/AeroBot/main/Releases/update_2.9.35.zip",
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
    print('Published 2.9.35 update to Firebase! Status:', res.status)
