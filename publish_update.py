# -*- coding: utf-8 -*-
import urllib.request
import json
from datetime import datetime

changelog_ar = """• تحديث شامل لمسار التجارة من القسطنطينية إلى هوتان (Constantinople -> Hotan) وإصلاح كشف المدن والبيع:
  - مسار Constantinople to Hotan الجديد: المرور بتاجر البضائع Tina في القسطنطينية للشراء (buy-goods)، ثم السير عبر الميناء الجنوبي والمنطاد (Harbor Manager Gale -> Blackbeard -> Shard -> Sangnia -> Ajati -> Dawari) بأرقام انتقال مُتحقَّق منها من ملفات اللعبة حتى تاجر Sanmok في هوتان للبيع (sell-goods).
  - إصلاح كشف المدينة الحالية (DetectCurrentCity) في التجارة التلقائية بين المدن: إصلاح التعرف على مدينة هوتان (Hotan region 23687 / 135,92) وسمرقند ودونهوانغ بدلاً من الخلط بينها عند بدء أو عكس جولات التجارة.
  - إضافة اقتراب تلقائي لتاجر التجارة قبل أمر البيع (sell-goods) أسوة بأمر الشراء لضمان فتح نافذة التجارة بنجاح دون توقف."""

changelog_en = """• Updated Constantinople to Hotan Trade Route & City Detection / Selling Fixes:
  - New Constantinople to Hotan trade script: buys goods from Specialty Trader Tina (345,229), navigates the southern harbor & airship route (Harbor Manager Gale -> Blackbeard -> Shard -> Sangnia -> Ajati -> Dawari) using PK2-verified teleport IDs, and delivers to Specialty Trader Sanmok (1523,896) in Hotan.
  - Fixed DetectCurrentCity() in City-to-City RouteBundle: resolved swapped/incorrect region IDs and fallback sectors so Hotan (23687), Samarkand (27244/27500), Donwhang (26265), Jangan (25000), and Constantinople (26959) are detected accurately.
  - Added automatic NPC approach before sell-goods (matching buy-goods) so trade hand-ins open reliably even if the final waypoint stops a few meters short."""

data = {
    "version": "2.9.47",
    "downloadUrl": "https://raw.githubusercontent.com/Abdelrhman-Sherif-Mohamed/AeroBot/main/Releases/update_2.9.47.zip",
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
    print('Published 2.9.47 update to Firebase! Status:', res.status)
