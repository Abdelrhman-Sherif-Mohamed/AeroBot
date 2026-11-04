import urllib.request
import json
from datetime import datetime

changelog_ar = """• إصلاح نهائي لمشكلة المربع الأسود الشفاف في شريط المخزن:
  - حل مشكلة تغطية نافذة ترتيب المستودع للشريط العلوي بزر Inventory و Equipment وضبط إخفائها افتراضياً (Visible = false).
  - ضبط حجم وتصميم صفحة ترتيب المستودع (Storage Order) بكامل خياراتها والتحكم التلقائي واليدوي.
  - إظهار شريط الأزرار العلوي دائماً في المقدمة لمنع أي تداخل بين الصفحات."""

changelog_en = """• Final Storage Order Overlay Fix:
  - Fixed dark overlay box covering Inventory and Equipment buttons by properly hiding panelStorageOrder by default and setting Dock to Fill.
  - Initialized complete layout and controls for Storage Order configuration and auto-sorting options.
  - Ensured top bar is always brought to front preventing any panel overlapping."""

data = {
    "version": "2.9.36",
    "downloadUrl": "https://raw.githubusercontent.com/Abdelrhman-Sherif-Mohamed/AeroBot/main/Releases/update_2.9.36.zip",
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
    print('Published 2.9.36 update to Firebase! Status:', res.status)
