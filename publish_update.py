import urllib.request
import json
from datetime import datetime

changelog_ar = """• تنظيف شامل لمسارات وسكربتات التجارة (Trade Scripts Deduplication):
  - حذف جميع الملفات المكررة والأسماء الخاطئة (مثل Costantinople و Downhang والملفات المتطابقة) مع الإبقاء على 20 مساراً قياسياً معتمداً يغطي كافة المدن (Jangan, Donwhang, Hotan, Samarkand, Constantinople).
  - توحيد أسماء وتنسيق ملفات المسارات بدقة لمنع تكرار أو لخبطة المسارات في القوائم.
  - إضافة تنظيف تلقائي للملفات المكررة القديمة لدى المستخدمين عبر برنامج التحديث وعبر واجهة التجارة."""

changelog_en = """• Complete Trade Route Scripts Deduplication & Cleanup:
  - Removed all duplicate/misspelled trade scripts (Downhang, Costantinople, legacy .vb files) keeping exactly 20 canonical clean routes between all cities.
  - Standardized all script filenames ensuring clean route selection in UI.
  - Added automatic cleanup of duplicate trade scripts via Updater and Trade module."""

data = {
    "version": "2.9.37",
    "downloadUrl": "https://raw.githubusercontent.com/Abdelrhman-Sherif-Mohamed/AeroBot/main/Releases/update_2.9.37.zip",
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
    print('Published 2.9.37 update to Firebase! Status:', res.status)
