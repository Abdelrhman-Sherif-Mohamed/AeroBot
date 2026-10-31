import urllib.request
import json
from datetime import datetime

changelog_ar = """• إصلاح مشكلة حرق واستهلاك حبوب فك السموم للتريدة (حيث كان يضرب 11 أو 12 حبة في نفس اللحظة) عند هجوم الثيف وتسميم دابة التجارة.

• إضافة كول داون (Cooldown) زمني ذكي بمقدار 1.5 ثانية لفك السموم والحالات الشاذة لمركبات التجارة والدواب لمنع الإرسال المتكرر لحين استجابة السيرفر.

• حماية استهلاك جرعات الصحة (HP) للدواب ومركبات التجارة بكول داون (1.05 ثانية) لمنع إهدار الجرعات.

• ضبط فك الحالات الشاذة للناقلة على التأثيرات الشاملة (Universal Effects) فقط لأن اللعبة لا تدعم فك تأثيرات الـ Purification للمركبات، لمنع الحرق المتواصل للحبوب."""

changelog_en = """• Fixed excessive trade transport bad status pills spam (using 11-12 pills simultaneously) when attacked or poisoned by thieves.

• Added intelligent 1.5s cooldown for vehicle and pet abnormal status recovery pills to allow server response before re-firing.

• Added 1.05s cooldown for vehicle and pet HP potion usage to prevent wasting potions.

• Restricted transport recovery pills to Universal status effects only (pets/vehicles do not have purification pills in Silkroad), preventing infinite pill consumption."""

data = {
    "version": "2.9.19",
    "downloadUrl": "https://raw.githubusercontent.com/Abdelrhman-Sherif-Mohamed/AeroBot/main/Releases/update_2.9.19.zip",
    "mandatory": False,
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
    print('Published 2.9.19 update to Firebase! Status:', res.status)
