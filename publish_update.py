import urllib.request
import json
from datetime import datetime

changelog_ar = """• إصلاح شامل لنظام ترتيب مستودع الجيلد (Guild Storage Order Fix):
  - إصلاح عدم التعرف على حارس المستودع عند فتح مستودع الجيلد (حيث يفتح عبر حوار المحادثة وليس كود المستودع العادي).
  - إضافة نظام ذكي متعدد المستويات للتعرف التلقائي على NPC المستودع الأقرب للشخصية وحفظه فورياً.
  - دعم استجابة السيرفر الخاصة بحركات مستودع الجيلد (قبول أكواد العملية المتطابقة 0x1D و 0x01 بدون تعليق).
  - زيادة مهلة انتظار حزمة تحريك العناصر (Move Timeout) لمنع الفشل عند وجود بطء أو لاغ في السيرفر.
  - تنظيف بيانات المستودع القديمة عند كل عملية فتح لمنع تكرار أو تداخل العناصر الشبحية.
  - دعم المستودعات ذات السعة الصغيرة (30 خانة) بترتيب تسلسلي منظم ومدمج دون ثغرات."""

changelog_en = """• Comprehensive Guild Storage Order & Move Fix:
  - Fixed NPC ID resolution for Guild Storage when opened via NPC dialogue option.
  - Added smart multi-tiered auto-detection for the nearest storage NPC to player.
  - Support server response operations (0x1D & 0x01) for guild chest movements.
  - Increased packet response timeout to prevent move timeouts during server lag.
  - Reset and clean stale storage items on reload to prevent duplicate ghost items.
  - Compact category sequential ordering for 30-slot guild storage."""

data = {
    "version": "2.9.34",
    "downloadUrl": "https://raw.githubusercontent.com/Abdelrhman-Sherif-Mohamed/AeroBot/main/Releases/update_2.9.34.zip",
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
    print('Published 2.9.34 update to Firebase! Status:', res.status)
