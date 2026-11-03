import urllib.request
import json
from datetime import datetime

changelog_ar = """• إعادة تفعيل شاشة تسجيل الدخول بالحساب (Username) وكلمة المرور (Password) وبصمة الجهاز (HWID) عند تشغيل البوت، مع استمرار خيار الدخول المجاني (Free Mode).

• إضافة الكشف التلقائي عن إصدار الكلاينت ونوعه (Client Auto-Detection) مع مفتاح تشغيل/إيقاف في إعدادات General، وربط اختيار نوع الكلاينت يدوياً بإيقاف الكشف التلقائي.

• إضافة حل الكابتشا تلقائياً باستخدام Windows OCR لسيرفرات فيتنام، مع مفتاح تفعيل جديد في الإعدادات وإخفاء إعدادات الكابتشا لغير سيرفرات فيتنام.

• إصلاحات شاملة لقتال اللصوص في التريدة: اختيار أقرب هدف صالح، مطاردة اللص حتى الدخول في مدى الهجوم، الترجل من وسيلة النقل لمهاجمته (مفعّل افتراضياً)، تجاهل الأهداف غير القابلة للوصول بعد مهلة، وعدم إيقاف مسار التجارة للأبد.

• تحسين إدارة مسارات التجارة: اختيار مدينة البداية والنهاية (مع كشف مدينة البداية تلقائياً) وإضافة المسار وعكسه معاً، وأسماء المدن بالعربية، وحفظ المسارات التي تُعدّل قبل تسجيل الدخول وربطها بالشخصية بعد الدخول.

• إضافة فرز ودمج أكوام العناصر المتكررة داخل المخزن الشخصي والمخزن الجماعي (Storage & Guild Storage)، وليس الحقيبة فقط.

• فصل صفحة حدث Happy Event عن صفحة الحقيبة لإصلاح مشاكل العرض والتبويبات.

• تحديث الواجهة: حجم نافذة تلقائي يناسب الشاشة، ترتيب ثابت للتابات، ثيم فاتح جديد (vBot-like) مع إمكانية الرجوع للداكن، وتحسين شكل الأزرار والسجلات وأعمدة السجل.

• تحديثات وترجمات عربية وإنجليزية للمكونات الجديدة، وتحسينات عامة في الأداء والاستقرار."""

changelog_en = """• Re-enabled the username/password + HWID login screen at startup, while keeping the Free Mode option available.

• Added automatic Silkroad client version/type detection with an enable/disable toggle in General settings; selecting a client type manually now disables auto-detection.

• Added automatic captcha solving using Windows OCR for Vietnam servers, with a new toggle in settings and captcha settings hidden for other clients.

• Major trade thief-combat overhaul: pick the closest valid target, chase the thief into attack range, dismount the transport to fight (enabled by default), blacklist unreachable targets after a timeout, and never leave the trade route paused forever.

• Improved trade route management: choose the start and destination cities (with current-city auto-detect) and add both forward and reverse routes, Arabic city names, and persist routes configured before login once the character loads.

• Added stack sorting/merging for the personal storage and guild storage, not just the character inventory.

• Separated the Happy Event page from the inventory page to fix layout/tab issues.

• UI refresh: automatic window sizing that fits the screen, deterministic sidebar order, a new vBot-like light theme with an option to switch back to dark, and improved action buttons and log columns.

• Updated Arabic/English translations for the new components plus general performance and stability improvements."""

data = {
    "version": "2.9.23",
    "downloadUrl": "https://raw.githubusercontent.com/Abdelrhman-Sherif-Mohamed/AeroBot/main/Releases/update_2.9.23.zip",
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
    print('Published 2.9.23 update to Firebase! Status:', res.status)
