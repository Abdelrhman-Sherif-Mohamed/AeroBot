import urllib.request
import json
from datetime import datetime

changelog_ar = """✨ تحديث شامل وإصلاحات AeroBot 2.9.29:
  - إصلاح مشكلة تجارة المدن (City-to-City Trade): الذهاب والعودة والتبديل التلقائي اللانهائي بين المدن (دون توقف).
  - اكتشاف المدينة الحالية تلقائياً واختيار مسار العودة الذكي وشراء البضائع فور الوصول.
  - إزالة التعليق البرمجي الناتج عن فحص الحركة بعد انتهاء السكربت وبيع البضائع.
  - استمرار الحذف التلقائي للملفات القديمة وحفظ كافة الإعدادات."""

changelog_en = """✨ AeroBot 2.9.29 Trade Round-Trip Fix & Updates:
  - Fixed City-to-City Trade round trip: seamless endless looping between cities (go and return).
  - Smart current city auto-detection to immediately pick return route and restock goods upon arrival.
  - Resolved movement check lock that prevented starting the return route after selling goods.
  - Continuous automatic legacy cleanup and persistent settings."""

data = {
    "version": "2.9.29",
    "downloadUrl": "https://raw.githubusercontent.com/Abdelrhman-Sherif-Mohamed/AeroBot/main/Releases/update_2.9.29.zip",
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
    print('Published 2.9.29 update to Firebase! Status:', res.status)
