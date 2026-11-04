import urllib.request
import json
from datetime import datetime

changelog_ar = """• إصلاح ميزة استخدام العناصر في مكان التدريب (Use item at training place):
  - تجديد وإعادة استخدام السكرولات والبفات والأدوية فور انتهاء مفعولها مباشرة وبدون توقف.
  - إصلاح فحص حالة مفعول السكرول (ItemSkillInUse) لمنع التضارب مع البفات العادية.
  - الانتقال التلقائي لاستخدام الرزمة (Stack) التالية في الشنطة عند نفاد الخانة الحالية.
  - إزالة الحظر التلقائي المؤقت وإضافة إعادة المحاولة الذكية بعد 3 ثوانٍ في حال وجود تأخير.
  - حفظ الإعدادات تلقائياً وبشكل دائم فور تفعيل الخيار من قائمة الشنطة."""

changelog_en = """• Fixed 'Use Item at Training Place' Re-application & Stacking:
  - Automatically and continuously re-casts/uses scrolls, buffs, and items as soon as they expire.
  - Fixed active item skill detection (ItemSkillInUse) to avoid false-positive overlaps.
  - Automatically switches to the next inventory stack when the current stack is depleted.
  - Replaced 5-minute blacklisting with smart 3-second retry throttling.
  - Automatically persists item settings immediately upon toggling."""

data = {
    "version": "2.9.31",
    "downloadUrl": "https://raw.githubusercontent.com/Abdelrhman-Sherif-Mohamed/AeroBot/main/Releases/update_2.9.31.zip",
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
    print('Published 2.9.31 update to Firebase! Status:', res.status)
