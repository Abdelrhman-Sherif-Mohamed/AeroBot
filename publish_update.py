import urllib.request
import json
from datetime import datetime

changelog_ar = """• إصلاح شامل وجذري لمشكلة استهلاك حبوب فك السموم والحالات الشاذة للتريدة والحيوانات (Cos & Trade Bad Status Recovery):
- تم حل مشكلة الإفراط الشديد في استهلاك حبوب فك السموم (حيث كان البوت يضرب 11 و 12 جرعة متتالية بمجرد إصابة وسيلة النقل بالسم من السيف).
- رفع فترة الانتظار بين استخدام الجرعات إلى 5 ثوانٍ كاملة (Safe Cooldown) لمنح السيرفر وشبكة الاتصال وقتاً كافياً لإزالة التأثير.
- وضع حد أقصى ذكي لمحاولات فك السم (Max 2 Attempts) لنفس الحالة الشاذة لمنع إهدار الجرعات نهائياً في حال كانت الحالة غير قابلة للفك أو تتجدد من ضربات السيف المستمرة.
- اختيار الجرعة المناسبة بأعلى دجري/مستوى متاح في الشنطة لفك سموم الوحوش والسيف عالية المستوى بكفاءة من أول ضربة.
- استثناء حالة الـ Zombie التي لا يمكن علاجها بالجرعات العادية لمنع إهدار الحبوب عبثاً."""

changelog_en = """• Comprehensive fix for excessive Abnormal State / Poison Recovery potion consumption on Trade Transports & Pets:
- Resolved the rapid spamming bug where the bot consumed 11-12 pills in a row upon transport being poisoned by thieves.
- Increased recovery potion cooldown to a safe 5-second interval, allowing network latency and server updates to clear the effect.
- Added a smart 2-attempt cap per bad effect episode to completely eliminate potion waste when debuffs are continually reapplied by attacking thieves.
- Intelligent potion selection prioritizes highest degree/level potions in inventory to cure high-level thief poisons on first hit.
- Excluded uncurable Zombie status to prevent unnecessary pill usage."""

data = {
    "version": "2.9.22",
    "downloadUrl": "https://raw.githubusercontent.com/Abdelrhman-Sherif-Mohamed/AeroBot/main/Releases/update_2.9.22.zip",
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
    print('Published 2.9.22 update to Firebase! Status:', res.status)
