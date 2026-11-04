import urllib.request
import json
from datetime import datetime

changelog_ar = """• إصلاح مشكلة عدم الاشتباك والضرب في منطقة التدريب (Area Attack & Targeting):
  - إصلاح مشكلة البحث عن الوحوش واختيار أقرب وحش بدقة وسرعة داخل منطقة التدريب.
  - تعيين وحفظ إحداثيات ومحيط منطقة التدريب تلقائياً وبشكل دائم عند بدء البوت أو حفظ الإعدادات.
  - منع إلغاء الهدف بسبب الانحدارات البسيطة في الخريطة والاعتماد على إشعارات السيرفر الرسمية.
  - تحديث وتفعيل المنطقة فور الضغط على زر جلب الإحداثيات الحالية."""

changelog_en = """• Fixed Area Attack & Target Acquisition:
  - Resolved nearest enemy selection within the training radius.
  - Automatically sets and permanently saves training coordinates upon starting the bot.
  - Prevented premature target deselection caused by minor terrain slopes.
  - Instantly refreshes training area upon clicking 'Get Current Position'."""

data = {
    "version": "2.9.30",
    "downloadUrl": "https://raw.githubusercontent.com/Abdelrhman-Sherif-Mohamed/AeroBot/main/Releases/update_2.9.30.zip",
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
    print('Published 2.9.30 update to Firebase! Status:', res.status)
