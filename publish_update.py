import urllib.request
import json
from datetime import datetime

changelog_ar = """• تحديث شامل وإضافة الإضافات الكاملة: مساعد الاستهداف (Target Support)، مسابقات الأسئلة (Trivia)، محدد الأهداف (Mob Selector).
• تعريب وتفعيل تسجيل الدخول التلقائي (Automated Login) مع دعم حل الكابتشا التلقائي (OCR).
• حزمة تحديث نظيفة 100% ومجردة من أي بيانات مستخدم أو حسابات لضمان الخصوصية التامة."""

changelog_en = """• Comprehensive update including Target Support, Trivia solver, and Mob Selector plugins.
• Full Automated Login support with OCR captcha solving and complete localization.
• 100% clean system package with zero user data or credentials for complete privacy."""

data = {
    "version": "2.9.41",
    "downloadUrl": "https://raw.githubusercontent.com/Abdelrhman-Sherif-Mohamed/AeroBot/main/Releases/update_2.9.41.zip",
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
    print('Published 2.9.41 update to Firebase! Status:', res.status)
