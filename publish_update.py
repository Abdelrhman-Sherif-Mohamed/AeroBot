# -*- coding: utf-8 -*-
import urllib.request
import json
from datetime import datetime

changelog_ar = """• إصلاح جذري لمشاكل توقف السير وتعطل السكربت أثناء التجارة والتنقل:
  - إصلاح حساب مهلة المشي (ActualSpeed * 0.1f) في محرك السير ونظام الالتفاف: كانت المهلة تُحسب أصغر بـ 10 مرات من الوقت الفعلي مما يسبب فشل الأوامر بعد 4-5 ثوانٍ وإيقاف الجولة بعد 3 إخفاقات متتالية.
  - ضبط كاشف العوائق (Obstacle Evasion): استبدال الفحص اللحظي الخاطئ بفحص تقدم حقيقي كل 1.5 ثانية وحساب المتجهات بالإحداثيات العالمية لمنع التوقف الوهمي في الطرق المفتوحة.
  - منع فشل أو إلغاء السكربت عند الإيقاف المؤقت (Pause/Resume): انتظار استئناف البوت بسلاسة عند القتال أو انتظار وسيلة النقل بدلاً من إنهاء الأمر بـ command=move failed، مع عدم تجميد السير داخل المدينة قبل شراء البضاعة.
  - منع تعارض استدعاء النعامة (Ostrich / COS_C) مع استدعاء الجمل/الفيل التجاري (0xB04C)، وتوحيد إشعارات الإيقاف وإصلاح تحذير الانتقال الكاذب (0xB05A)."""

changelog_en = """• Comprehensive Movement, Script Pause/Resume & Trade Stability Fixes:
  - Fixed 10x move timeout bug (ActualSpeed * 0.1f with active vehicle speed support) in MoveScriptCommand, Player, and Cos: prevented valid 60m-100m route steps from timing out in 4-5 seconds.
  - Fixed false obstacle evasion triggers: switched to a 1.5s rolling world-coordinate displacement check and cross-sector meter math so open-road travel no longer stalls or loops unnecessarily.
  - Fixed Pause vs Abort handling across ScriptManager, Move, Wait, and Teleport commands: pausing for thief combat or transport waits now pauses cleanly and resumes seamlessly without logging false command failures or aborting routes.
  - Prevented COS_C mount summoning race with Trade COS_T transport (0xB04C), avoided town startup deadlocks before buying goods, cleaned up duplicate OnFinishScript events on stop, and silenced false 0xB05A rejection warnings while teleport completes."""

data = {
    "version": "2.9.48",
    "downloadUrl": "https://raw.githubusercontent.com/Abdelrhman-Sherif-Mohamed/AeroBot/main/Releases/update_2.9.48.zip",
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
    print('Published 2.9.48 update to Firebase! Status:', res.status)
