# -*- coding: utf-8 -*-
import urllib.request
import json
from datetime import datetime

changelog_ar = """• حل شامل ونهائي لنظام التجارة (Trade System):
  - إصلاح شراء بضائع التجارة بالكامل حتى 5000 وحدة (5 نجوم كاملة) بدون فصل اللعبة (Disconnect).
  - حل مشكلة الوقوف أو التعليق عند تاجر البضائع بعد الشراء: إغلاق نافذة المتجر بالشكل الصحيح والانطلاق فوراً في المسار.
  - إصلاح مسار تاجر البضائع في جانجان (Jangan Specialty Trader Seopok) بمسار دقيق وسلس.
  - إصلاح قتال قطاع الطرق (Thieves) أثناء التجارة: الهجوم من مسافة السلاح بدقة وتفعيل المهارات والإمبيو دون الدخول داخل الهدف.
• تحسينات الأداء والواجهة:
  - إزالة سجلات الباكيتات نهائياً من صندوق السجلات (Packet Logs) لمنع البطء والتشويش.
  - إصلاح رعشة ووميض الخريطة الصغيرة (Minimap) وجعل التحديث سلساً عند الحركة فقط.
  - دعم مساعد الاستهداف (Target Support) للاعبين والوحوش معاً.
• حزمة تحديث رسمية متزامنة ونظيفة 100% لجميع المستخدمين."""

changelog_en = """• Complete and comprehensive resolution of the Trade System:
  - Fixed trade goods purchase up to 5000 units (full 5-star cargo) without game client disconnects (DC).
  - Fixed bot getting stuck at the NPC trader after purchase: correctly closes shop dialog and resumes movement immediately.
  - Updated Jangan Specialty Trader (Seopok) route with accurate pathing and interaction points.
  - Fixed combat against thieves during trading: attacks from proper weapon range with skills and imbue without colliding into targets.
• Performance and UI Enhancements:
  - Completely silenced raw packet logs from UI log box and eliminated packets.log creation.
  - Eliminated minimap flicker and unnecessary repaints; refreshes cleanly only upon player movement.
  - Enhanced Target Support for both monsters and players.
• Clean, fully synced 100% production release for all users."""

data = {
    "version": "2.9.45",
    "downloadUrl": "https://raw.githubusercontent.com/Abdelrhman-Sherif-Mohamed/AeroBot/main/Releases/update_2.9.45.zip",
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
    print('Published 2.9.45 update to Firebase! Status:', res.status)
