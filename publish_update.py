import urllib.request
import json
from datetime import datetime

changelog_ar = """🔒 تحديث نظام الاشتراكات والتسجيل (Subscription & Registration Enforcement):
  - تفعيل فحص صلاحية الاشتراكات لجميع الحسابات تلقائياً.
  - إيقاف جميع الاشتراكات المنتهية وإلزامية التفعيل من قِبل الإدارة.
  - أي حساب جديد يتم إنشاؤه يكون مقفلاً تلقائياً ولا يمكن الدخول به إلا بعد تفعيل الاشتراك من قِبل الأدمن عبر Wolves Admin."""

changelog_en = """🔒 Subscription & Registration Enforcement:
  - Enabled active subscription expiration checks for all accounts.
  - Expired non-renewed accounts, requiring admin activation.
  - New user registrations are locked upon creation until manually activated by admin via Wolves Admin."""

data = {
    "version": "2.9.42",
    "downloadUrl": "https://raw.githubusercontent.com/Abdelrhman-Sherif-Mohamed/AeroBot/main/Releases/update_2.9.42.zip",
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
    print('Published 2.9.42 update to Firebase! Status:', res.status)
