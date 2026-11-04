import urllib.request
import json
from datetime import datetime

changelog_ar = """✨ تحديث شامل AeroBot 2.9.27:
  - توحيد وتسمية كافة ملفات وموديولات البوت والإضافات إلى AeroBot.
  - إزالة وتنظيف كافة الملفات والمكتبات القديمة ومخلفات التحديثات تلقائياً.
  - دعم برنامج التحديث الجديد AeroBot.Updater مع التنظيف الذكي.
  - استقرار كامل لكافة إعدادات اللغات والملفات الشخصية بدون فقدان بيانات."""

changelog_en = """✨ Comprehensive AeroBot 2.9.27 Update:
  - Unified and renamed all bot modules, plugins, and libraries to AeroBot.
  - Automatic cleanup and removal of all obsolete files and legacy leftovers.
  - Built-in new AeroBot.Updater with smart cleanup routines.
  - Full stability and backward compatibility for configurations and languages."""

data = {
    "version": "2.9.27",
    "downloadUrl": "https://raw.githubusercontent.com/Abdelrhman-Sherif-Mohamed/AeroBot/main/Releases/update_2.9.27.zip",
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
    print('Published 2.9.27 update to Firebase! Status:', res.status)
