import urllib.request
import json
from datetime import datetime

changelog_ar = """• إصلاح شامل وجذري لمشكلة اللغة الإنجليزية (Pure English): عند اختيار اللغة الإنجليزية لا يظهر أي كلام عربي نهائياً في التابات أو الإعدادات أو أزرار التحكم أو الأنماط الفرعية.
• تصحيح أسماء التابات في الوضع العربي: تم ضبط تابة المهارات لتظهر 'المهارات' بدلاً من 'Skills ال'، وتابة الفريق لتظهر 'الفريق' بدلاً من 'Party ال'، وتابة المتجر لتظهر 'المتجر' بدلاً من 'Stall'.
• ترجمة شاملة بنسبة 100% لكافة إعدادات الحماية (Protection)، التدريب (Training)، تسجيل الدخول (Auto Login & Auth Window)، أوامر التحكم والتشكيلات (Control & Choreography)، تجربة الأزياء والسكنات (Avatar Tester)، والمتجر (Stall) باللغتين العربية والإنجليزية.
• تحديث نظام ترجمة الـ WinForms لإلغاء أي كاش قديم عند تغيير اللغة وتحديث الواجهة مباشرة بدون الحاجة لإعادة تشغيل البوت."""

changelog_en = """• Complete & comprehensive fix for English localization: When selecting English, all tabs, groupboxes, controls, and buttons now display 100% pure English with zero Arabic text.
• Fixed Arabic tab titles: 'المهارات' instead of mixed 'Skills ال', 'الفريق' instead of 'Party ال', and 'المتجر' instead of 'Stall'.
• 100% full localization for Protection, Training, Auto Login, Auth Window, Control & Choreography, Avatar Tester, and Stall modules.
• Enhanced UI translation cache manager to invalidate stale translations immediately upon language switch without requiring a bot restart."""

data = {
    "version": "2.9.20",
    "downloadUrl": "https://raw.githubusercontent.com/Abdelrhman-Sherif-Mohamed/AeroBot/main/Releases/update_2.9.20.zip",
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
    print('Published 2.9.20 update to Firebase! Status:', res.status)
