# -*- coding: utf-8 -*-
import urllib.request
import json
from datetime import datetime

changelog_ar = """• تحديث شامل v2.9.49: تحسينات الواجهة والسرعة والتجارة والتحكم والإشعارات:
  - تسريع فتح البوت واللانشر بشكل ملحوظ (تأجيل بناء الواجهة لما بعد تسجيل الدخول، تقليل مهلة فحص التحديث، تحميل PK2 بالتوازي، وتسريع بدء تشغيل اللعبة).
  - إصلاحات الواجهة والخطوط والقوائم (الشنطة، المهارات، الأيتمات، التدريب، أزرار الإضافات، معلومات السيرفر، والدردشة بدون ترجمة).
  - تطوير نظام التجارة وإحصائيات الجولات المباشرة (عدد الجولات، المتوسط، الأفضل، صافي الربح، ذهب/ساعة) وتثبيت استقرار القوافل وقتال اللصوص.
  - إضافة نظام تنبيهات تيليجرام وديسكورد (Wolves.Notify) لمتابعة الموت، اليونيك، الجرعات، التجارة، الدروب النادر، والرسائل مع منع التكرار.
  - ترقية أوامر التحكم عن بُعد (Wolves.Control) بأوامر EQUIP / UNEQUIP / TERMINATE / SORT / TP / RECALL / REVERSE / CHAT / INJECT."""

changelog_en = """• Comprehensive Update v2.9.49 — Performance, UI Fixes, Trade Stats, Control & Notifications:
  - Faster startup & launcher opening (deferred main UI creation until login, bounded update check timeout, parallel PK2 loading, and async client start).
  - UI & RTL fixes across Skills, Inventory, Items, Training, Extensions Hub, Server Info, and Chat (with chat translator removed).
  - Upgraded Trade botbase with live caravan stats (Runs, Avg, Best, Net Profit, Gold/h) and thief-hunting / transport stability improvements.
  - Added Wolves.Notify plugin (Telegram & Discord alerts for Death, Stop, Attacks, Trade, HP/MP Pots, Uniques, Rare Drops, Level Up, and Chat).
  - Expanded Wolves.Control remote commands with EQUIP, UNEQUIP, TERMINATE, SORT, TP, RECALL, REVERSE, CHAT, and INJECT."""

data = {
    "version": "2.9.49",
    "downloadUrl": "https://raw.githubusercontent.com/Abdelrhman-Sherif-Mohamed/AeroBot/main/Releases/update_2.9.49.zip",
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
    print('Published 2.9.49 update to Firebase! Status:', res.status)
