import urllib.request
import json
from datetime import datetime

changelog_ar = """• إصلاح جذري لظهور كافة التبويبات والمميزات المفقودة (الخريطة، القلعة، الإضافات، المتجر، الأزياء، اللاعبون، الإحصائيات، الدردشة، الإشعارات):
- تم حل خطأ التحويل البرمجي (InvalidCastException في ToolStripSeparator) داخل محرك الترجمة والقوائم المنبثقة والذي كان يتسبب في إيقاف حلقة تحميل الإضافات عند تابة الخريطة وحرمان باقي التابات من الظهور.
- إضافة حماية استثنائية (Safe Extension Loader) تضمن عزل أخطاء أي إضافة بشكل مستقل دون التأثير على بقية المميزات.
- ظهور كافة الـ 16 تابة بالكامل في الشريط الجانبي والقوائم بدون أي نقص."""

changelog_en = """• Critical fix for missing tabs and extensions (Map, Fortress, Control, Stall, Avatar Tester, Players, Statistics, Chat, Log):
- Resolved an InvalidCastException on ToolStripSeparator inside the UI language translation traversal that was prematurely terminating the extension loading loop at the Map tab.
- Added isolated error boundary (Safe Extension Loader) to ensure any extension anomaly cannot break other tabs.
- All 16 sidebar tabs and features now load and display completely."""

data = {
    "version": "2.9.21",
    "downloadUrl": "https://raw.githubusercontent.com/Abdelrhman-Sherif-Mohamed/AeroBot/main/Releases/update_2.9.21.zip",
    "mandatory": False,
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
    print('Published 2.9.21 update to Firebase! Status:', res.status)
