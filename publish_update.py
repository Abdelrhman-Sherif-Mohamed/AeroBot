# -*- coding: utf-8 -*-
import urllib.request
import json
from datetime import datetime

changelog_ar = """• تحديث شامل v2.9.50 (إصلاحات التجارة وقتال اللصوص واللانشر وبدء التشغيل):
  - التعرف التلقائي على لاعبي الثيف (Player Thieves بأسمائهم وبدلاتهم) والترجل لقتالهم وردع أي هجوم على وسيلة النقل.
  - إصلاح ركوب وسيلة النقل (الفيل والجمال) بعد 20 ثانية من قتل اللصوص بزيادة نطاق الركوب إلى 6.0 أمتار ومعايرة عقوبة القتال.
  - إصلاح خطأ تشغيل RSBot.CommandCenter، وضمان نسخ ملف Client.Library.dll والتحقق المسبق قبل تشغيل الكلاينت.
  - استقرار تشغيل اللعبة وتخفيف تحذيرات السوكيت غير الضرورية، مع كامل تحسينات الواجهة والتجارة والتحكم وWolves.Notify."""

changelog_en = """• Comprehensive Update v2.9.50 (Trade Thief Combat, Remount Fix, Startup & Client Launch):
  - Fixed automatic detection & high-threat counter-attack for Player Thieves (*alias & job suits) and transport protection.
  - Fixed 20s remount after combat for large trade mounts (White Elephant / Camels with 6.0m radius and exact 20s cooldown).
  - Fixed RSBot.CommandCenter startup exception, Client.Library.dll build copy & pre-launch checks.
  - Improved client process stability, plus all v2.9.49 UI/RTL, Trade live stats, Wolves.Control, and Wolves.Notify."""

data = {
    "version": "2.9.50",
    "downloadUrl": "https://raw.githubusercontent.com/Abdelrhman-Sherif-Mohamed/AeroBot/main/Releases/update_2.9.50.zip",
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
    print('Published 2.9.50 update to Firebase! Status:', res.status)
