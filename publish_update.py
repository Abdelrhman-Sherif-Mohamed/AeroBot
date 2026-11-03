import urllib.request
import json
from datetime import datetime

changelog_ar = """• تحديث أمني إلزامي ومهم جداً:
  - تفعيل نظام التحقق الأمني وإلزام تسجيل الدخول بالاسم وكلمة المرور لجميع المستخدمين.
  - إلغاء وإيقاف الوضع المجاني (Free Mode) بالكامل ومنع تخطي شاشة الدخول نهائياً.
  - إيقاف الدخول التلقائي وحذف أي بيانات محفوظة سابقاً لضمان عدم فتح البوت إلا بحساب مفعل ومشترك.
  - تفعيل الفحص الفوري والمباشر لصلاحية وانتهاء الاشتراكات على السيرفر."""

changelog_en = """• Mandatory Security & Authentication Update:
  - Enforced strict login with valid username & password credentials for all users.
  - Completely disabled and blocked Free Mode bypass.
  - Disabled automatic login and cleared cached credentials to guarantee bot locks on startup.
  - Instant server-side subscription expiration checks."""

data = {
    "version": "2.9.25",
    "downloadUrl": "https://raw.githubusercontent.com/Abdelrhman-Sherif-Mohamed/AeroBot/main/Releases/update_2.9.25.zip",
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
    print('Published 2.9.25 update to Firebase! Status:', res.status)
