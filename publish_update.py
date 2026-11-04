import urllib.request
import json
from datetime import datetime

changelog_ar = """• إضافة تحكم مخصص لتأخير التحدث مع الـ NPC في نظام الهابي (Happy Event Delay):
  - إضافة زر/عداد تحديد وقت الانتظار (بالثواني) قبل فتح محادثة So-Ok وبين كل دورة تبديل وأخرى في واجهة الـ Happy.
  - حماية الحساب من الفلود (Flood Protection) والديسكونكت أو الحظر المؤقت في السيرفرات الخاصة.
  - حفظ وقت التأخير تلقائياً وبشكل دائم في إعدادات الشخصية."""

changelog_en = """• Added NPC Talk Delay Control for Happy Event (So-Ok):
  - Added delay control (seconds) in Happy tab before talking to Event So-Ok and between exchange cycles.
  - Prevents server-side packet flood disconnections and talk rate-limits.
  - Automatically persists the configured delay value."""

data = {
    "version": "2.9.32",
    "downloadUrl": "https://raw.githubusercontent.com/Abdelrhman-Sherif-Mohamed/AeroBot/main/Releases/update_2.9.32.zip",
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
    print('Published 2.9.32 update to Firebase! Status:', res.status)
