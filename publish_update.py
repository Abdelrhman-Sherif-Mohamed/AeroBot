import urllib.request
import json
from datetime import datetime

changelog_ar = """✨ تحديث شامل وإصلاحات AeroBot 2.9.28:
  - إصلاح مشكلة إغلاق البوت المفاجئ عند التنقل بين تبويبات الشنطة (Inventory) و Happy.
  - استقرار كامل لواجهة المستخدم وحفظ تلقائي لكافة الإعدادات.
  - استمرار الحذف التلقائي للملفات القديمة ومخلفات النسخ السابقة."""

changelog_en = """✨ AeroBot 2.9.28 Update & Bug Fixes:
  - Fixed crash/stack overflow when switching between Inventory and Happy tabs.
  - Complete UI stability and seamless auto-configuration.
  - Continuous automatic legacy file cleanup."""

data = {
    "version": "2.9.28",
    "downloadUrl": "https://raw.githubusercontent.com/Abdelrhman-Sherif-Mohamed/AeroBot/main/Releases/update_2.9.28.zip",
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
    print('Published 2.9.28 update to Firebase! Status:', res.status)
