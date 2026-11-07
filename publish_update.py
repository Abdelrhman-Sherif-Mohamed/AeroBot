import urllib.request
import json
from datetime import datetime

changelog_ar = """• إصلاح شراء بضاعة التجارة بالكامل: البوت يشتري حمولة 5 نجوم كاملة (5000 وحدة / 5 خانات) دون التوقف المبكر.
• إصلاح تحديث حقيبة وسيلة النقل (الفيل/الجمل) داخل اللعبة: ظهور البضاعة ومستوى الصعوبة فور شرائها في شاشة وسيلة النقل.
• منع كراش وفصل اللعبة (DC / 0xC0000005) نهائياً أثناء عمليات شراء وبيع البضائع مع تاجر التجارة الخاصة.
• تأمين الخريطة المصغرة (Minimap) وحمايتها من أخطاء النقر في حال عدم الاتصال باللعبة."""

changelog_en = """• Fixed trade cargo purchasing: Bot now buys full 5-star cargo capacity (5000 units / 5 full slots) without premature stopping.
• Fixed transport inventory sync: Goods and 5-star trade difficulty now appear immediately inside the pet/transport window in-game.
• Fixed client crash / disconnect (0xC0000005) during automated NPC specialty goods trading.
• Added minimap safety guard preventing unhandled exceptions when clicked while disconnected."""

data = {
    "version": "2.9.40",
    "downloadUrl": "https://raw.githubusercontent.com/Abdelrhman-Sherif-Mohamed/AeroBot/main/Releases/update_2.9.40.zip",
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
    print('Published 2.9.40 update to Firebase! Status:', res.status)
