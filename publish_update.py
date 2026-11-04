import urllib.request
import json
from datetime import datetime

changelog_ar = """• إضافة نظام متكامل لترتيب وتقسيم صفحات المستودع تلقائياً (Storage Page Segregation & Order):
  - صفحة مخصصة ومستقلة داخل تبويب الشنطة (Order / Storage Order) للتحكم الكامل في خيارات المستودع وترتيبه.
  - خوارزمية ذكية لفرز العناصر وتوزيعها تلقائياً على صفحات المستودع الـ 5 (30 خانة لكل صفحة):
      * صفحة 1 [0-29]: الأسلحة، الدروع، الإكسسوارات، الأفاتار، ملابس الجوب (Equipment & Gear).
      * صفحة 2 [30-59]: إكسيرات السلاح، الدرع، الإكسسوار، الحماية (Reinforce Elixirs).
      * صفحة 3 [60-89]: أحجار الكيمياء (Magic/Attr Stones)، الأقراص (Tablets)، العناصر والمواد (Elements).
      * صفحة 4 [90-119]: الجرع (HP/MP/Vigor)، حبوب العلاج (Pills)، السكرولات، أدوات السيلك والمول.
      * صفحة 5 [120-149]: أدوات البيت والحيوانات، بضاعة التجارة، الكويستات، السهام (Pets, Trade & Misc).
  - نقل الفائض (Overflow) فوق سعة الـ 30 عنصر تلقائياً وبأمان إلى الخانات الفارغة المتبقية بدون أي فقدان أو خطأ.
  - خيارات تفعيل/تعطيل تقسيم الفئات، والترتيب التلقائي أو دمج الأكوام المتطابقة فور فتح المستودع العادي أو مستودع الجيلد."""

changelog_en = """• Dedicated Storage Page Segregation & Automatic Order System:
  - Added dedicated full-featured 'Order' page under Inventory tab for complete storage management.
  - Smart category distribution across the 5 storage pages (30 slots per page):
      * Page 1 [0-29]: Equipment, Weapons, Armor, Accessories, Avatars, Job Gear.
      * Page 2 [30-59]: Weapon, Shield, Armor, Accessory Reinforce Elixirs.
      * Page 3 [60-89]: Magic & Attribute Stones, Tablets, Elements & Alchemy Materials.
      * Page 4 [90-119]: Recovery Potions, Pills, Scrolls, Silk & Item Mall Items.
      * Page 5 [120-149]: Pet Items, Trade Goods, Quests, Ammunition & Misc.
  - Seamless overflow handling for categories exceeding 30 items without item loss.
  - Toggle options for auto-order and auto-merge upon opening storage or guild storage."""

data = {
    "version": "2.9.33",
    "downloadUrl": "https://raw.githubusercontent.com/Abdelrhman-Sherif-Mohamed/AeroBot/main/Releases/update_2.9.33.zip",
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
    print('Published 2.9.33 update to Firebase! Status:', res.status)
