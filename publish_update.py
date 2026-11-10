# -*- coding: utf-8 -*-
import urllib.request
import json
from datetime import datetime

changelog_ar = """• إصلاح مشكلة فصل اللعبة (Disconnect) عند شراء بضائع التجارة من تاجر البضائع — الشراء بقى شغال وبيتم تحميل البضاعة على الجمل لحد 5000 وحدة بدون كراش.
• إصلاح مشكلة "البوت بيلزق في التاجر" بعد الشراء: البوت بقى يقفل نافذة المتجر صح ويبدأ يتحرك فوراً بدون ما يفضل واقف عند الـNPC.
• إصلاح مسار تاجر البضائع في جانجان (Specialty Trader Seopok) — مسار جديد أسهل وبيوصل للتاجر بالظبط.
• إزالة سجلات الباكيتات (Packet Logs) من صندوق اللوج نهائياً — مبقاش بيظهر للمستخدم أي تفاصيل باكيتات ولا بيتم إنشاء ملف packets.log.
• إصلاح رعشة ورفريش الخريطة الصغيرة (Minimap) — الخريطة بقت تترسم بس لما اللاعب يتحرك فعلاً، وبدون فلاش أو اهتزاز.
• إصلاح هجمات قطاع الطرق (Thieves) أثناء التجارة: البوت بقى يهاجم من مسافة السلاح بدل ما يمشي جوه الهدف.
• دعم مساعد الاستهداف (Target Support) على اللاعبين مش الوحوش بس.
• تحسين تثبيت سعر وكمية الشراء واختيار البضاعة المفضلة أثناء شراء بضائع التجارة."""

changelog_en = """• Fixed the game client disconnect (DC) when buying trade goods — purchasing now works and loads up to 5000 units onto the transport without crashing.
• Fixed the "bot stuck at the NPC" bug after buying: the bot now closes the shop dialog correctly and starts moving immediately.
• Fixed the Jangan Specialty Trader (Seopok) route — new cleaner path that ends exactly on the trader.
• Removed packet logs from the log window completely — no packet dumps are shown to users and packets.log is no longer created.
• Fixed the minimap flicker / constant refresh — the map now repaints only when the player actually moves, with no flicker or shaking.
• Fixed trade thief combat: the bot now engages from weapon range instead of walking inside the target.
• Target Support now works on players as well as monsters.
• Improved trade purchase stability, quantity handling and preferred goods selection."""

data = {
    "version": "2.9.44",
    "downloadUrl": "https://raw.githubusercontent.com/Abdelrhman-Sherif-Mohamed/AeroBot/main/Releases/update_2.9.44.zip",
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
    print('Published 2.9.44 update to Firebase! Status:', res.status)
