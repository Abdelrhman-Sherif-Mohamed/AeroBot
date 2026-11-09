import urllib.request
import json
from datetime import datetime

changelog_ar = """⚔️ إصلاح قتال الثيف في التريدة ومسافة الاشتباك (Trade Combat Fix):
  - إصلاح مشكلة عدم ضرب الثيف وتخطي شرط فحص الـ Health الخاطئ للأهداف المستهدفة.
  - ضبط مسافة القتال ديناميكياً حسب نوع السلاح (الأسلحة بعيدة المدى كالقوس والعصا تضرب من مسافة 14 متراً دون الدخول داخل الثيف).
  - تفعيل استخدام مهارات الدامج والإمبيو (Imbue) تلقائياً ضد الثيف."""

changelog_en = """⚔️ Trade Thief Combat & Weapon Range Fix:
  - Fixed combat bug where bot would walk inside thief without attacking due to uninitialized health check.
  - Dynamic weapon range calculation (bow/staff/crossbow engages at 14m instead of walking into target).
  - Enabled automatic weapon imbue and skill casting against thieves."""

data = {
    "version": "2.9.43",
    "downloadUrl": "https://raw.githubusercontent.com/Abdelrhman-Sherif-Mohamed/AeroBot/main/Releases/update_2.9.43.zip",
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
    print('Published 2.9.43 update to Firebase! Status:', res.status)
