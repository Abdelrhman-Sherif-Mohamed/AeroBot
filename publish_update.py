# -*- coding: utf-8 -*-
import urllib.request
import json
from datetime import datetime

changelog_ar = """• إصلاح شامل وانتقال سلس عبر الموانئ والأنفاق في مسارات التجارة (Trade Teleportation & Ferries):
  - إصلاح انتقال العبّارات والموانئ (Constantinople / Jangan Ferries & Tunnels): النزول التلقائي من مركبة التجارة قبل إرسال أمر الانتقال (Ferry Teleport) لتفادي رفض الخادم، والاقتراب التفاعلي من الـ NPC، والتحقق الحقيقي من اكتمال النقل إلى الضفة الأخرى قبل استئناف المسار.
  - حماية محرك السكربتات (Script Engine Hardening): منع تخطي نقاط المسار بالكامل عند حدوث خطأ أو فشل أمر السير/الانتقال، وإيقاف السكربت بأمان بدلاً من حرق مئات الأسطر في ثوانٍ ومحاولة البيع الخاطئة.
  - معالجة موت اللاعب أثناء التجارة: إيقاف استدعاء الجمل في حلقة مفرغة أثناء الموت، وإلغاء المسار بأمان عند الموت والعودة للمدينة للبدء من جديد بدلاً من استئناف السير من نقطة الموت البعيدة.
  - التحقق الفعلي من وصول اللاعب لوجهة التجارة النهائية قبل إنهاء الجولة أو احتسابها ناجحة."""

changelog_en = """• Comprehensive Trade Teleportation & Ferry / Tunnel Fixes:
  - Fixed ferry and tunnel teleportation during trade routes (Constantinople, Jangan, Central Asia): automatic dismount from trade transport prior to teleportation to prevent server rejection, interactive approach to NPC, and verified completion before path resumption.
  - Hardened Script Engine: prevented runaway waypoint skipping when a teleport or move command encounters an obstacle/out-of-range error; safely aborts instead of skipping hundreds of waypoints in seconds.
  - Safe death handling during trading: stopped infinite transport summon loop while dead, cleanly resets route on death and respawn in town.
  - Verified player physical proximity to final destination before declaring trade loop complete."""

data = {
    "version": "2.9.46",
    "downloadUrl": "https://raw.githubusercontent.com/Abdelrhman-Sherif-Mohamed/AeroBot/main/Releases/update_2.9.46.zip",
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
    print('Published 2.9.46 update to Firebase! Status:', res.status)
