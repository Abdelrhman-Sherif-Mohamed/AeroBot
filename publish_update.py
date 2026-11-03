import urllib.request
import json
from datetime import datetime

changelog_ar = """• إضافة نظام الترتيب التلقائي الشامل (Auto Order) للمخزن الشخصي (Storage) ومخزن الجيلد (Guild Storage) والشنطة (Inventory):
  - دمج الأكوام الناقصة أولاً ثم فرز وترتيب الأدوات بالتتابع من الخانة 0 دون ترك أي خانات فارغة في المنتصف.
  - فرز وتصنيف متقدم حسب فئات اللعبة: الأسلحة، الدروع والتروس، الإكسسوارات، الأزياء والديفل، عتاد الجوب والمرافقين، الجرعات، الذخيرة، الإكسيرات، أحجار الألكيمي، الدواب، والسكرولات.
  - ترتيب العناصر داخل كل فئة حسب الدجري (Degree)، المستوى، الندرة (Sun/Moon/Star)، بلس التطوير (+)، والكمية.
  - إضافة زر 'Order' ومفتاح 'Auto order' مستقل لكل من المخزن الشخصي ومخزن الجيلد والشنطة في واجهة المستخدم.
  - حماية ذكية بفواصل زمنية بين عمليات النقل لمنع الطرد أو حظر السيرفر بسبب سرعة النقل.

• استمرار ميزة دمج الأكوام السريعة (Auto Sort) كخيار منفصل لدمج الأكوام المتشابهة فقط لمن يفضل عدم إعادة ترتيب الخانات."""

changelog_en = """• Added comprehensive Auto Order system for Personal Storage, Guild Storage, and Character Inventory:
  - Consolidates broken stacks first, then arranges all items sequentially starting from slot 0 with zero empty gaps.
  - Advanced categorization & sorting: Weapons, Shields, Armor, Accessories, Avatars, Job/Pet gear, Potions, Ammo, Elixirs, Alchemy Stones/Tablets, Transports, and Scrolls.
  - Secondary sorting within each category by Degree, Required Level, Rarity (SOS/SOM/SOSUN), Plus (+), and Amount.
  - Added dedicated 'Order' button and separate 'Auto order' checkboxes per tab (Storage, Guild Storage, Inventory).
  - Built-in safe action delays to prevent server flood kicks during rapid slot reorganizations.

• Maintained standalone Auto Sort option for players who only want to merge duplicate stacks without rearranging slots."""

data = {
    "version": "2.9.24",
    "downloadUrl": "https://raw.githubusercontent.com/Abdelrhman-Sherif-Mohamed/AeroBot/main/Releases/update_2.9.24.zip",
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
    print('Published 2.9.24 update to Firebase! Status:', res.status)
