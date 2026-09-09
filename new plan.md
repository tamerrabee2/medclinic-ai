تمام. بما أنك اعتمدت الاتجاه السابق، فأنا أقترح أن نحول الفكرة الآن إلى **Product Blueprint + Technical Architecture + Master Prompt** واحد تعطيه للـAI Agent، بحيث لا يبدأ بإنشاء Dashboard عشوائي، بل يفهم المنتج بالكامل ويُنفذه على مراحل.

وأول قرار معماري سأثبته هو:

> **Backend: C# / ASP.NET Core / .NET 10 + PostgreSQL + Entity Framework Core**
> **Frontend: Next.js + React + TypeScript + Node.js runtime/tooling**
> **AI: طبقة مستقلة قابلة لتبديل النماذج**.

وأفضّل **PostgreSQL على SQL Server** لهذا المشروع؛ ليس لأن SQL Server سيئ، وإنما لأن PostgreSQL ممتاز لتطبيق SaaS حديث، مفتوح المصدر، قوي مع JSONB والبحث والفهارس، وسهل تشغيله مع Docker وبيئات Cloud مختلفة.

---

# 1. شكل المنتج الذي سنبنيه

اسم مؤقت:

# **MedClinic AI**

الشعار:

> **Your AI Clinical Workspace**

وبالعربي:

> **مساحة العمل الطبية الذكية للطبيب**

الفكرة ليست أن يكون البرنامج مجرد:

> مرضى + مواعيد + فواتير.

بل:

```text
                    MedClinic AI
                         │
        ┌────────────────┼────────────────┐
        │                │                │
     Clinic           Clinical            AI
   Management         Workspace         Intelligence
        │                │                │
   Appointments       EMR             AI Assistant
   Billing            Visits          Lab Analysis
   Staff              Prescriptions   Imaging AI
   Reports            Timeline        Patient Summary
                       Canvas          Voice Scribe
                       Body Map        AI Search
```

---

# 2. أهم نقطة في التصميم

لا أريد أن يكون التطبيق شبيهاً ببرنامج ERP طبي قديم.

أريد أن يشعر الطبيب أنه أمام:

**Apple/Notion/Linear + Clinical AI**

وليس:

**برنامج مستشفى قديم مليء بالجداول.**

الـDashboard الحديثة في منتجات الرعاية الصحية تميل إلى البساطة، المعلومات ذات الأولوية، الـcards والـcharts، بدلاً من ازدحام الشاشة؛ وهذه نقطة مهمة جداً في تصميم منتجك. ([Dribbble][1])

---

# 3. الشكل البصري المقترح

![Image](https://images.openai.com/static-rsc-4/E0g3kWHxgMsj-7_6mXoK_6FC5OMJ_pF8pFfUcQ4PpPZh5PJNxeAjRLAd9SLlGM_bmMEnFlySEJh07dOCwHDN25BSbc9V248IKudnAM04ALnEVLwPILcd4gbRsaNYencnQxY_u2OvmWAc4BaMQOqIzrDm25HradUE54l8KPb2gDcSNOfnT0JeyPZbSAvna5rx?purpose=fullsize)

![Image](https://images.openai.com/static-rsc-4/_Fwzl6UC6-d9HlgAj2v-Zts1tmOZE2MfiJRHOJc0IKJMv3W7Fcl2b8dE2xUnP_Y9qOpbVlQ84FTi-AnwhJIZCAb3dG7G32iC7wc_2b6YdZndSbbvE5S_W2sbTTcQa8cK5QYOMZXz1bsN7aTBU97jc-i-nGPb0_aSwvuEdho0d05z8X5SGjckdmVFURyU_asq?purpose=fullsize)

![Image](https://images.openai.com/static-rsc-4/SR1_rC8IUKo-n0MFPnBsyT1H9Fwe9AXHezwSRCluyyaByRwV8WhWl6WI3d7TuOX77NIDpxNw6uShoHMZdZlSPqBK3TUmRiBCBl0MW1bEBpfzfJADBAyu1nkF8o6Z9-OhfpTIjRCVTVkKfnPxfx0IWV572LSvUK_BaRPfCrj7Fs_xhr5tjdpir2mypKY73r1x?purpose=fullsize)

![Image](https://images.openai.com/static-rsc-4/od7GG5gvp9RiWL7HTQ6zxH-1_Mce953RfwqK4iRhdmyYFhZelOELIFaf8j3BDPuOQIc0rzjO2O1DI1V7EhuxPFT_SMxB0m_SSDA3qmaZIeLC0Fh-jcWwB3GX1n0kQ-qVCiohYWzM0P-pwR6VZXo7I6TT-O7y3iQ8mfMcfv-885idmeCcxph_IkuRovCiydOM?purpose=fullsize)

![Image](https://images.openai.com/static-rsc-4/_iQIOCcvfi4ZMVBQkADU74fivnx6wqhHbe9PPdO9FLQTbgKrh26HzsClzuQ5euE6h45XpkpkJA10a3s1yiNBosQIX-QkDtSHn4Hg_Vs9zLGjJyQu4pWuJjcS4k--rQpfJ3YRDdrq53s4iUpvp5EJkmxGr4z_eQKuF_ijFSDgcjus7bLJYdoRve5LDG4Y-B70?purpose=fullsize)

![Image](https://images.openai.com/static-rsc-4/yuIZNC1eIQmzEO9S98MlCFM0vz6Ht_OkTG-Wb1gcuRdnqJako25ZIk--bQwE-yqq3OFdNPjONGP6dVu-jjUzzPwJp-c72eOwBFexFwFDdDEmo1ALVFw0K57drVnNQq58vCpBJFzojdDBAkE7SpufdydcqqnZ7hwLMUPAqmpqaGH6YlVHIqboRSILWAJelRwR?purpose=fullsize)

## التصميم الذي أقترحه

### الألوان

لا تجعل التطبيق كله أزرق طبي تقليدي.

أقترح:

* Background: أبيض/رمادي فاتح جداً.
* Primary: أزرق طبي هادئ.
* Secondary: Teal.
* AI: بنفسجي/Indigo مميز.
* Success: أخضر.
* Warning: Amber.
* Danger: Red.

وفي Dark Mode:

* Background داكن جداً.
* Cards رمادية داكنة.
* AI accent بنفسجي/Indigo.
* Clinical alerts بألوان واضحة.

### الشكل

* Border radius متوسط.
* Shadows خفيفة.
* Glass effect بشكل محدود.
* Animations بسيطة.
* Micro-interactions.
* Skeleton loading.
* Command palette.
* Keyboard shortcuts.

**لا نستخدم Neon UI بشكل مبالغ فيه**؛ الطبيب يحتاج وضوحاً وليس استعراضاً.

---

# 4. الـMain Layout

أقترح:

```text
┌──────────────────────────────────────────────────────────┐
│ Logo        Search / ⌘K              Notifications Avatar│
├──────────────┬───────────────────────────────────────────┤
│              │                                           │
│ Dashboard    │                                           │
│              │                                           │
│ Patients     │              Main Content                 │
│              │                                           │
│ Appointments │                                           │
│              │                                           │
│ Clinical     │                                           │
│              │                                           │
│ AI Assistant │                                           │
│              │                                           │
│ Imaging      │                                           │
│              │                                           │
│ Laboratory   │                                           │
│              │                                           │
│ Prescriptions│                                           │
│              │                                           │
│ Billing      │                                           │
│              │                                           │
│ Reports      │                                           │
│              │                                           │
│ Settings     │                                           │
└──────────────┴───────────────────────────────────────────┘
```

وفي الموبايل يتحول الـSidebar إلى Bottom/Drawer Navigation.

---

# 5. الصفحة الرئيسية للطبيب

هذه أهم شاشة في النظام.

![Image](https://images.openai.com/static-rsc-4/Dkl76sUfC3GQeHyCwPed9zEalI7bbS6WdxiA0rCUuPAuabhvQFYGaGJCeyg_Sw8siIZvTvDy-41wmAuaiwQdpN2MU9bEN9Hw6B6x9Z5k4sn4Yggq0qlgegSXLdfVUg44olO9suJtCKDVChQbjkNX0wipGU6TEERKgD5IZbw6xUmH9DEdedkf-iecfMuTwpRh?purpose=fullsize)

![Image](https://images.openai.com/static-rsc-4/XyVYaBJ3xeqo6ysPwkOWVWvvl9ep7wOjliMvTfw2jwnDMlE_i3ZWgLfC3xHIJ2JVdWWAHuxC6XL_4i1IuwsEvJH6G_BQKLBsUtgbcRkbr-sQ-9HHnQRwga__rXXfAZY0v4P9ys_f8plnLtK5Z4MFKBay1WvVbkMN_xOLYwxge5nATRCHOkOqurh2Ot8fo8Pd?purpose=fullsize)

![Image](https://images.openai.com/static-rsc-4/_Fwzl6UC6-d9HlgAj2v-Zts1tmOZE2MfiJRHOJc0IKJMv3W7Fcl2b8dE2xUnP_Y9qOpbVlQ84FTi-AnwhJIZCAb3dG7G32iC7wc_2b6YdZndSbbvE5S_W2sbTTcQa8cK5QYOMZXz1bsN7aTBU97jc-i-nGPb0_aSwvuEdho0d05z8X5SGjckdmVFURyU_asq?purpose=fullsize)

![Image](https://images.openai.com/static-rsc-4/yuIZNC1eIQmzEO9S98MlCFM0vz6Ht_OkTG-Wb1gcuRdnqJako25ZIk--bQwE-yqq3OFdNPjONGP6dVu-jjUzzPwJp-c72eOwBFexFwFDdDEmo1ALVFw0K57drVnNQq58vCpBJFzojdDBAkE7SpufdydcqqnZ7hwLMUPAqmpqaGH6YlVHIqboRSILWAJelRwR?purpose=fullsize)

![Image](https://images.openai.com/static-rsc-4/YSDUnTP5KMNG3EHmAARG5KgLQ9TrF4tfY4aB7yAeXbDLfDaiW4Iem0eoALhMA09_r7X6Cy9jeP8-uU7UaT4aAy1kb4KF96AET-0Eqqffi_HrYnmHjLclbZ4KFi6eMrYBxIqoiiJzTohTxIjFh6Q_czdXF9tiCrCUgJnfevvQqf90VdPS6DVjKmI49siqSOWY?purpose=fullsize)

![Image](https://images.openai.com/static-rsc-4/od7GG5gvp9RiWL7HTQ6zxH-1_Mce953RfwqK4iRhdmyYFhZelOELIFaf8j3BDPuOQIc0rzjO2O1DI1V7EhuxPFT_SMxB0m_SSDA3qmaZIeLC0Fh-jcWwB3GX1n0kQ-qVCiohYWzM0P-pwR6VZXo7I6TT-O7y3iQ8mfMcfv-885idmeCcxph_IkuRovCiydOM?purpose=fullsize)

بدلاً من Dashboard فيها:

> Revenue / Patients / Doctors

فقط.

نريد:

# Good Morning, Dr. Ahmed

ثم:

```text
┌──────────────────────────────────────────────────────┐
│ Today's Overview                                     │
│                                                      │
│ 18 Patients     4 Pending      3 AI Reviews          │
│ 12 Completed    2 Follow-ups  5 Lab Results         │
└──────────────────────────────────────────────────────┘
```

ثم:

## 🧠 AI Patient Brief

```text
Ahmed Mohamed
48 years

Last visit: 3 months ago

Important history
• Diabetes
• Hypertension

Recent changes
• HbA1c: 8.2 → 7.4
• New medication added

Pending
• Follow-up laboratory test
• Radiology review

[Open Patient]
```

ثم:

## Today's Schedule

```text
09:00  Ahmed Mohamed      Consultation
09:30  Sara Ali           Follow-up
10:00  Mohamed Hassan     New Patient
10:30  Omar Khaled        Review
```

ثم:

## AI Tasks

```text
🩻 3 imaging studies waiting
🧪 5 laboratory results
📝 2 visit summaries
🎙️ 1 voice note to review
```

---

# 6. Patient Profile — أهم شاشة بعد Dashboard

أريدها مختلفة جداً عن برامج العيادات التقليدية.

```text
┌──────────────────────────────────────────────────────┐
│ ← Patients                                           │
│                                                      │
│ Ahmed Mohamed        48 y/o       Male               │
│ Diabetes • Hypertension                              │
│                                                      │
│ [New Visit] [AI Summary] [Upload] [Prescription]     │
├──────────────────────────────────────────────────────┤
│ Overview │ Timeline │ Visits │ Labs │ Imaging │ Rx    │
├──────────────────────────────────────────────────────┤
│                                                      │
│              PATIENT INTELLIGENCE                    │
│                                                      │
│ AI Summary                                            │
│ ──────────────────────────────────────────────────── │
│ Patient has a history of...                          │
│                                                      │
│ Recent Changes                                        │
│ HbA1c  8.2 → 7.4 ↓                                   │
│ BP     145 → 132 ↓                                   │
│                                                      │
└──────────────────────────────────────────────────────┘
```

---

# 7. Patient Timeline

هذه واحدة من أهم الـUSP.

```text
2024
 │
 ├── Diagnosis
 │
 ├── Lab
 │
 └── Prescription
 │
2025
 │
 ├── Visit
 ├── X-Ray
 ├── AI Analysis
 └── Medication Change
 │
2026
 │
 ├── Lab
 ├── Imaging
 ├── AI Summary
 └── Follow-up
```

والـAI يستطيع:

> **Summarize this patient's history**

ثم يعطي ملخصاً مبنياً على الـTimeline.

---

# 8. AI Assistant

لا أريد ChatGPT clone داخل التطبيق.

أريد AI موجوداً **داخل كل Workflow**.

مثلاً:

في Patient:

> 🧠 Summarize Patient

في Lab:

> 🧪 Explain Results

في Imaging:

> 🩻 Analyze Image

في Visit:

> ✨ Draft Clinical Note

في Timeline:

> 🧠 What changed?

في Prescription:

> 💊 Review Context

---

# 9. شاشة AI Assistant

![Image](https://images.openai.com/static-rsc-4/_iQIOCcvfi4ZMVBQkADU74fivnx6wqhHbe9PPdO9FLQTbgKrh26HzsClzuQ5euE6h45XpkpkJA10a3s1yiNBosQIX-QkDtSHn4Hg_Vs9zLGjJyQu4pWuJjcS4k--rQpfJ3YRDdrq53s4iUpvp5EJkmxGr4z_eQKuF_ijFSDgcjus7bLJYdoRve5LDG4Y-B70?purpose=fullsize)

![Image](https://images.openai.com/static-rsc-4/Dm0l0DPWHo8Da6QFaLOxcb-BnnGWxNFeKrn-pAeoAKFsE__aw4uaDupdeFO_3uqG0X3cWb7ScSDFZ8cYLlMQRyg-S1cU-daqxwWQ9IO3DgG0IcSfVCiwvyINvTFt77syv_ZPHFTHgI1tMh16otrqzHHDELeRRT3F57p16-D6NJyFTKNWoPCf87vsapnBkXTq?purpose=fullsize)

![Image](https://images.openai.com/static-rsc-4/gAvyIkkQ-sbDKRZHqd79hLfACqyYtz2qZG1S5Kc2GXy_djSvEmmLLSV-GJfc3yfYGsHTfJzVWUUI72lE7O6zS7bUlKowPlB9QyB-4JA3-J3nWhvEs8KzGi3ZBnoDLwMK_rXFLfoEMz_5DuaArD6wOnS1Ful367dGGhgECagraAuI1-oV60GvfkHiJQCqUhbE?purpose=fullsize)

![Image](https://images.openai.com/static-rsc-4/Dkl76sUfC3GQeHyCwPed9zEalI7bbS6WdxiA0rCUuPAuabhvQFYGaGJCeyg_Sw8siIZvTvDy-41wmAuaiwQdpN2MU9bEN9Hw6B6x9Z5k4sn4Yggq0qlgegSXLdfVUg44olO9suJtCKDVChQbjkNX0wipGU6TEERKgD5IZbw6xUmH9DEdedkf-iecfMuTwpRh?purpose=fullsize)

![Image](https://images.openai.com/static-rsc-4/E773Ae6wYgUHdUFT2DwS2RB3FmTLEYA_NAPH1DW22NZOhfbckoGyGGBigmhHSn8pH3zAUsep6Xwx7-Xkklzi6aVJJJava2YXqgQPLEUE-nkKkLBpqAOC6xngoUjW9rYlB50EUa0Nc1DsTZwAXJaXaDPHdkzg8LJEiKytZBjSJJGlGR4LuxpcL9h9AlSkvfS4?purpose=fullsize)

![Image](https://images.openai.com/static-rsc-4/MS12LiiWsKh8ERl1BHRa65SBNJNBUlK-aBnmUnxbg-R-Mrxj6eG2HNy5QbosPIGi6WJYluynUHYUqQqfa7JPcq4xRI9VFpNcB-oYV1fpTFPBoQhUpc5J5QNnNhVpfDL5Zw0nyDaPDMlCGFHQiz7kYMvyaCensTHdjpJuU2lqYz2IM6Rbe234OojANwDagwwk?purpose=fullsize)

شكلها:

```text
┌─────────────────────────────────────────────────────────┐
│ AI Clinical Assistant                              ✨  │
├──────────────────────┬──────────────────────────────────┤
│ Conversations        │                                  │
│                      │  Patient Context                 │
│ Patient Summary      │  Ahmed Mohamed                   │
│ Lab Analysis         │                                  │
│ Visit Summary        │  ─────────────────────────────   │
│ Imaging              │                                  │
│                      │  Doctor:                         │
│                      │  "Compare latest labs..."        │
│                      │                                  │
│                      │  AI:                             │
│                      │  "The HbA1c decreased..."        │
│                      │                                  │
│                      │  [Ask something...]              │
└──────────────────────┴──────────────────────────────────┘
```

---

# 10. Medical Imaging Workspace

وهذه ستكون من أقوى الشاشات في المنتج.

![Image](https://images.openai.com/static-rsc-4/-8MAZUij0_hj4ssr2JgqaYJ8UISTEwf_SMZlRahrVbbaaSsigVh1RA141z4uSjigvwS9UBPplx1NuDc7GT3Od0qGaMsSgpskzOtJ_lz1trq_KzzSGsFvuGL-zo2SfmgQh9uHBzdrLL2E9SVo13yMT5dOYYJtepsnJnVQ9kqXhqaybwqcA1-jq4cU2g4GQAkx?purpose=fullsize)

![Image](https://images.openai.com/static-rsc-4/RA_5J_zC2ka0LZDfHkVgQEfrQdL_VC4oEpeJ2X2Q9Z3gk80eiyupPzGgzJ5dOXHwQHV3HiVBVazmLEC6SHbVUsG6cZFpbI8zGPVr_y0gIb16rGmlJ5LFIyOOXtE5F6zm-s3MRkgGI8kV-jxXQxEfyOcIPaNaukzAPDF1KMRszWFNtf_0mXJphjcrRjcVX-v4?purpose=fullsize)

![Image](https://images.openai.com/static-rsc-4/dNSv_1vznmwr5u3tgwY25_JH1YF5kqwjgGcIk-Xj0u3uCMKv4tWjmWu_Ku76TRPuZt7bLPe6IEzW62n9IJHuXZq8C6sUtC6n6Rl43T_9KYUNKVhdOnBBLd-VaK7Z428nY1DL7SAQaXQSFZ8C-j3lWhhnLcwa4tB5AIjRic8nyiNuaKIXYvWu02I2Wh8mCkwI?purpose=fullsize)

![Image](https://images.openai.com/static-rsc-4/xGK3m7HHrPHNBoZGK4quGivwc339Lp82iSE7z9etdGzq30NT4GZ-iwUW_WQhFjcLDK1Oe2PqXbLTzPbehS-Ib-74BTEPSwxox9T22OCgf6iXqyx_dXzHRwxiKFwRXQvIXO2eJDZeTACP0Rc0Lyl0glT-qniVanHbxGf6lBsF-gapnomnUXqP1LbSvo2jos1R?purpose=fullsize)

![Image](https://images.openai.com/static-rsc-4/V8iA8TMgzjlOXXjpwOpm86b99TTfIFECDdlm_GreGmjOzAuZ81I-ufByJrp2ghop1cXGJVK8ebcQYkxm-FhzmOQ8_baWajJfk3f1zg0zgKuJ0G2crARqOjTx4RhDCmCPs_cmmrGUiWiXFgpeFSU2Sx4NTi_zB5DKuRMHlSv0x2JtKY8FqGNgzDLPdE3UtJJy?purpose=fullsize)

![Image](https://images.openai.com/static-rsc-4/eOWdk8r4WauKzhqUh4znGGNzc8l6SBQuDy2KFnfkI0iaB6YBMO71SXIoEq2Ad93ssU1i84h28_ih8MhUU9vA6mIXCDg4Xs4cbKmdMwEjYqECv9B0n8fagCAjU5LnKyuBoWXImvE10U_F3yKUsUUxF016Ex40GQ9diqxaCeJWWfWJ-bPK2preTNLlccubDObc?purpose=fullsize)

![Image](https://images.openai.com/static-rsc-4/5G5H_DihwwGG_3T9vLkQLDyBlQkhjg8UJdNm6io_CjPRJAR54qiZRle-lOcGQG25_zu1QaQP90WST7OTRD2TkrtiJiPobvW6bwH3adAU5v-pP1toOaxQGSfgKnVPH39wz66byg-0fgFV2tb8uGdQN-ANcSacfZHKZ7JtGCEkROCgj049A1kcow2qwgs92Tys?purpose=fullsize)

التصميم:

```text
┌────────────────────────────────────────────────────────────┐
│ Patient: Ahmed       Chest X-Ray       AI Analysis: Ready │
├────────┬──────────────────────────────────────┬────────────┤
│ Tools  │                                      │ AI Panel   │
│        │                                      │            │
│ ↖      │                                      │ Findings   │
│ Pen    │           MEDICAL IMAGE              │            │
│ Arrow  │                                      │ Summary    │
│ Circle │             X-RAY                    │            │
│ Text   │                                      │ Confidence │
│ Zoom   │                                      │            │
│ Measure│                                      │ [Review]   │
│        │                                      │            │
├────────┴──────────────────────────────────────┴────────────┤
│ Original │ Annotated │ AI Findings │ Compare │ Report      │
└────────────────────────────────────────────────────────────┘
```

هذا النوع من الـannotation له precedent واضح في أدوات التصوير الطبي؛ مثلاً واجهات مثل VinDr تعرض الصورة مع مناطق محددة وأدوات annotation وقائمة findings، لذلك يمكن أخذ **الفكرة الوظيفية** وليس نسخ التصميم.

---

# 11. Medical Canvas

يجب أن يدعم:

```text
Pen
Brush
Arrow
Line
Rectangle
Circle
Polygon
Text
Measurement
Eraser
Undo
Redo
Zoom
Pan
Rotate
Brightness
Contrast
Invert
```

والأهم:

## لا تعدل الصورة الأصلية.

احفظ:

```text
Original Image
       +
Annotation JSON
       +
Annotated Preview
```

---

# 12. AI + Canvas

هذه ستكون ميزة مميزة جداً.

الـworkflow:

```text
Upload X-Ray
       ↓
AI Analysis
       ↓
AI Findings
       ↓
Highlight / Region
       ↓
Doctor Annotation
       ↓
Doctor Notes
       ↓
AI Summary
       ↓
Doctor Review
       ↓
Final Report
```

لكن هناك قاعدة مهمة:

**لا تجعل النظام يخترع مناطق على الصورة إذا كان النموذج لا يدعم localization/segmentation فعلياً.**

---

# 13. Laboratory Workspace

```text
┌──────────────────────────────────────────────────────┐
│ Lab Results                                          │
├──────────────────────────────────────────────────────┤
│                                                      │
│ Upload Report    [PDF] [Image]                      │
│                                                      │
│ ┌──────────────┬─────────┬────────────┬──────────┐ │
│ │ Test         │ Current │ Reference  │ Status   │ │
│ ├──────────────┼─────────┼────────────┼──────────┤ │
│ │ Hemoglobin   │ 11.2    │ 13-17      │ LOW      │ │
│ │ WBC          │ 14.2    │ 4-11       │ HIGH     │ │
│ │ Platelets    │ 250     │ 150-450    │ NORMAL   │ │
│ └──────────────┴─────────┴────────────┴──────────┘ │
│                                                      │
│ [AI Explain Results]                                 │
│                                                      │
│ Trend                                                │
│ 2024 ─── 2025 ─── 2026                              │
└──────────────────────────────────────────────────────┘
```

---

# 14. Interactive Body Map

![Image](https://images.openai.com/static-rsc-4/xzievfApQCio4PkkZ-xfUQiXMGF7QBIHcdX05mt-IbP1bPKypGE8AjW_X_MyevVBvXvd4sYjsKhegB8YOtWSAHa93LkZtSj33_pz5eXgne4MCfMoRBhGDRioiJRw8--DX971nMrjwReeeXu7jBlfoeZ1q1jai0jP4jCtCy7_uVLKQ5rFL9kyn0PXg5v8Jb-K?purpose=fullsize)

![Image](https://images.openai.com/static-rsc-4/trUoqJaS_U-ceK5iTLI4VxucspB2BGsJbRm-GZQPpv3xNf-WjYn3Q4sgvkHCGg8on1pc4yCO2klrlhicaUZdduvi6kocy6VfCVgU-AzGRUq-VLWvUCMLIGlfEEcjO9sMH_z94SiQWnkDZovJtKOCqHg5HEAP8FqW07mc68vDj8eHhtru78-fOZiPenYwO2am?purpose=fullsize)

![Image](https://images.openai.com/static-rsc-4/NX7Q9NP-HVMwV3Z8fvrvKDiqacXEvOvpq48WTHR83tURTiGxIPVltTyaFDaiGXq3hurFUvQoEFR27fwz1CasrIqlu5_I6ubHnVjYfGxoOLfnsC0rqhxIOzXweFiD0XcxacAr4hk-7oWHoRoPy9tjyoeV9vPIVGCCeAPIZxYM10bm1tJDwcSqZmw_RjqXwPSY?purpose=fullsize)

![Image](https://images.openai.com/static-rsc-4/o9A9zSu99Im217HcsSGBbug2lk572QaQfJ3SDvmJCuk0onXZH2jK-BtvUq1xYfKTsBa38FNgFRfESCGLoB5n4wQ2k6kM__GQpM22ZYulOObEsOguLn2-kkLjyzIGqJRLsLW10sOXPvNJK5QN5qVy0V5Uy1JL_1XqLjBIke0FBoFermiUZ3LxqgMr6MVWXVhz?purpose=fullsize)

![Image](https://images.openai.com/static-rsc-4/lTPY9YAW13Oe-Br1L0uJ4Bjp9kBFCDfeV-n-pSldb4IJBwxVFsPinsddVp5sMOnBfQFBzLKoy_BDzgqnmoEQgPzY9sdCxPpJxRxMZ-Lk1sihu2bbJ75et4gUuROm6c9eeBaZFIoXFMNIiQK3nv4qHgGv0uzbyKc-zoJSnU5T3-285pQJR3LgylfSmzsp7r9z?purpose=fullsize)

![Image](https://images.openai.com/static-rsc-4/uEqHPFMonWNgBM7c3zOmGgin8SHD-YEkBrjOztWu0VQTyfnou0CQtdwMvs2zM0NWHMD3szcCfJ3LVheXLb4dJWyPMnQLwweBaDNfQRrhRPe7yWCgaZKCy6Yu5V29lKY8rjXUMjADSBS8pDVh9AZKyzlWILFmls8DKpXTB53wdzPHP7HTOw0BHC-ZAGnnxQmH?purpose=fullsize)

نريد SVG حقيقي وليس صورة ثابتة.

مثلاً:

```text
          HEAD
           │
          NECK
           │
       ┌── CHEST ──┐
       │            │
     ARM          ARM
       │            │
       └─ ABDOMEN ──┘
          │
        PELVIS
        /    \
      LEG    LEG
```

عند الضغط:

```text
Chest
 ↓
Add Symptom
Add Pain
Add Diagnosis
Add Note
Upload Image
```

---

# 15. Dental Module

لاحقاً:

```text
      11 12 13 14 15 16
      21 22 23 24 25 26

      31 32 33 34 35 36
      41 42 43 44 45 46
```

كل سن:

* Normal
* Caries
* Filling
* Crown
* Missing
* Implant
* Root Canal
* Extraction

---

# 16. Voice AI

هذه Feature أضعها في Phase 2، وليس MVP.

الطبيب يتحدث:

> "المريض بيشتكي من ألم في الركبة اليمين من أسبوعين والألم بيزيد مع الحركة..."

النظام:

```text
Voice
 ↓
Speech-to-Text
 ↓
Clinical NLP
 ↓
Structured Note
 ↓
Doctor Review
```

ثم:

```text
Chief Complaint
HPI
Examination
Assessment
Plan
```

---

# 17. أهم UX Feature

## Command Palette

اضغط:

`Ctrl + K`

تظهر:

```text
Search patients
New patient
New appointment
Open AI Assistant
Upload lab
Upload imaging
New prescription
Open today's schedule
```

هذه تعطي إحساس تطبيق حديث جداً.

---

# 18. Architecture

الـArchitecture النهائية:

```text
                    ┌──────────────────┐
                    │   Next.js Web    │
                    │ React + TS       │
                    └────────┬─────────┘
                             │
                         REST / SSE
                             │
                             ▼
                    ┌──────────────────┐
                    │ ASP.NET Core API │
                    │ .NET 10 / C#     │
                    └────────┬─────────┘
                             │
              ┌──────────────┼──────────────┐
              ▼              ▼              ▼
       Application        Domain      Infrastructure
              │              │              │
              └──────────────┼──────────────┘
                             │
                ┌────────────┼─────────────┐
                ▼            ▼             ▼
           PostgreSQL      Redis       Object Storage
                                          │
                                          ▼
                                    Medical Files
```

---

# 19. لماذا Next.js وليس Node.js فقط؟

مهم جداً للـAI Agent:

لا تكتب له:

> Frontend Node.js

بشكل عام.

اكتب:

> **Next.js + React + TypeScript، مع Node.js كـruntime/tooling للـfrontend.**

لأن Node.js وحده ليس UI framework.

---

# 20. Backend Project Structure

```text
backend/
│
├── src/
│   ├── MedClinic.API
│   ├── MedClinic.Application
│   ├── MedClinic.Domain
│   ├── MedClinic.Infrastructure
│   └── MedClinic.Shared
│
├── tests/
│   ├── MedClinic.UnitTests
│   ├── MedClinic.IntegrationTests
│   └── MedClinic.ArchitectureTests
│
└── MedClinic.sln
```

---

# 21. Frontend Structure

```text
frontend/
│
├── app/
│
├── components/
│   ├── ui/
│   ├── dashboard/
│   ├── patients/
│   ├── appointments/
│   ├── clinical/
│   ├── ai/
│   ├── imaging/
│   ├── laboratory/
│   ├── canvas/
│   └── body-map/
│
├── features/
│
├── hooks/
├── lib/
├── services/
├── types/
├── stores/
├── i18n/
└── public/
```

---

# 22. Database

استخدم:

# PostgreSQL

وEF Core.

الجداول الرئيسية:

```text
Users
Roles
Permissions
Clinics
ClinicMembers
Doctors
Patients
PatientContacts
MedicalRecords
Visits
Diagnoses
Medications
Prescriptions
PrescriptionItems
Appointments
LabOrders
LabResults
LabResultItems
RadiologyStudies
MedicalImages
AIAnalyses
AIReports
AIConversations
AIConversationMessages
MedicalAnnotations
BodyAnnotations
Invoices
InvoiceItems
Payments
Notifications
AuditLogs
```

---

# 23. Multi-Tenancy

هذه نقطة أساسية من البداية.

كل Clinic = Tenant.

```text
Clinic A
 ├── Doctors
 ├── Patients
 ├── Visits
 └── Billing

Clinic B
 ├── Doctors
 ├── Patients
 ├── Visits
 └── Billing
```

ولا يمكن لـClinic A رؤية بيانات Clinic B.

---

# 24. AI Architecture

```text
                AI Gateway
                     │
        ┌────────────┼─────────────┐
        │            │             │
     Vision        Text          Voice
        │            │             │
        ▼            ▼             ▼
   Imaging AI    Clinical AI    Speech AI
```

ثم:

```csharp
public interface IAIProvider
{
    Task<MedicalImageAnalysisResult>
        AnalyzeMedicalImageAsync(...);

    Task<LabAnalysisResult>
        AnalyzeLabAsync(...);

    Task<PatientSummaryResult>
        SummarizePatientAsync(...);

    Task<ClinicalNoteResult>
        GenerateClinicalNoteAsync(...);

    Task<AIChatResponse>
        ChatAsync(...);
}
```

---

# 25. لا تجعل AI Provider واحداً

أنشئ:

```text
IAIProvider
│
├── CloudAIProvider
├── OpenAIProvider
├── GeminiProvider
├── AnthropicProvider
├── LocalAIProvider
└── MockAIProvider
```

ويتم تحديده من:

```text
AI_PROVIDER
```

---

# 26. AI مجاني

هنا أريد تعديل الفكرة السابقة قليلاً.

لا تكتب للـAgent:

> "اجعل AI الطبي مجاني."

لأن هذا غير واقعي إذا استخدمت APIs مدفوعة.

اكتب:

> **The application must support a zero-cost development/demo mode using MockAIProvider and optionally local open-source models. Cloud AI must be provider-configurable and must never be falsely advertised as free.**

وبالتالي:

### Demo

مجاني.

### Local AI

يمكن تشغيله محلياً حسب الـhardware/model.

### Cloud AI

حسب API provider.

---

# 27. Security

لأن البيانات طبية:

```text
Authentication
Authorization
Tenant Isolation
Audit Logs
Encrypted Transport
Secure Storage
Signed URLs
Rate Limiting
Input Validation
File Validation
Secret Management
```

والـAI لا يرى بيانات لا يحتاج إليها.

---

# 28. AI Medical Safety

كل نتيجة AI:

```text
AI GENERATED
       ↓
Doctor Review
       ↓
Approve / Edit
       ↓
Medical Record
```

لا:

```text
AI
 ↓
Automatic Diagnosis
 ↓
Automatic Prescription
```

---

# 29. Roadmap

لا تجعل الـAgent يبني 100 Feature مرة واحدة.

## Phase 0 — Foundation

* Repository
* Architecture
* Docker
* CI
* PostgreSQL
* .NET
* Next.js
* Authentication

---

## Phase 1 — Core Clinic

* Clinics
* Users
* Roles
* Doctors
* Patients
* Appointments

---

## Phase 2 — Clinical Workspace

* Patient profile
* Medical record
* Visits
* Timeline
* Prescriptions
* Lab
* Radiology

---

## Phase 3 — AI

* AI Gateway
* Mock AI
* AI Chat
* Patient Summary
* Lab Analysis
* Imaging integration

---

## Phase 4 — Visual Clinical

* Medical Canvas
* Image Viewer
* Annotations
* Body Map
* Image comparison

---

## Phase 5 — Intelligence

* Voice Scribe
* AI Patient Brief
* AI Clinical Timeline
* AI-powered workflow
* Follow-up intelligence

---

## Phase 6 — Business

* Billing
* Payments
* Reports
* Notifications
* WhatsApp integration

---

## Phase 7 — Advanced

* Dental
* DICOM
* PACS integration
* FHIR
* External labs
* Insurance
* Patient Portal

---

# 30. Testing Strategy

يجب أن يحتوي المشروع على:

### Backend

* xUnit
* FluentAssertions
* Integration Tests
* WebApplicationFactory
* Testcontainers

### Frontend

* Vitest
* React Testing Library
* Playwright

### أهم E2E

```text
Login
 ↓
Create Clinic
 ↓
Create Doctor
 ↓
Create Patient
 ↓
Create Appointment
 ↓
Open Visit
 ↓
Upload Lab
 ↓
AI Analysis
 ↓
Upload Image
 ↓
Annotate
 ↓
AI Summary
 ↓
Doctor Approval
 ↓
Prescription
 ↓
Follow-up
```

---

# 31. GitHub Actions

```text
.github/workflows/

ci.yml
backend.yml
frontend.yml
e2e.yml
security.yml
```

كل Pull Request:

```text
Restore
Build
Lint
Test
Security Scan
Frontend Build
Backend Build
```

---

# 32. Docker

```text
docker-compose.yml

services:

  frontend
  backend
  postgres
  redis
```

ويمكن إضافة:

```text
local-ai
```

إذا كان سيتم دعم Local AI.

---

# 33. شكل الـRepository

```text
medclinic-ai/
│
├── frontend/
├── backend/
├── infrastructure/
├── docs/
│
├── .github/
│   └── workflows/
│
├── docker-compose.yml
├── README.md
├── ARCHITECTURE.md
├── SECURITY.md
├── CONTRIBUTING.md
├── LICENSE
└── .gitignore
```

---

# 34. أهم الصفحات التي يجب على الـAI Agent بناؤها

## Public

```text
/
 /features
 /pricing
 /security
 /about
 /contact
 /login
 /register
```

## Doctor

```text
/dashboard
/patients
/patients/:id
/appointments
/clinical
/clinical/:visitId
/laboratory
/radiology
/imaging/:id
/ai
/prescriptions
/billing
/notifications
/settings
```

## Admin

```text
/admin
/admin/clinics
/admin/users
/admin/doctors
/admin/reports
/admin/billing
/admin/settings
```

---

# 35. الشكل الخاص بالمريض

صفحة المريض يجب أن تكون:

```text
┌─────────────────────────────────────────────┐
│ Ahmed Mohamed                     48 Male   │
│ Diabetes • Hypertension                     │
│                                             │
│ [New Visit] [AI Summary] [Prescription]     │
├─────────────────────────────────────────────┤
│ Overview │ Timeline │ Visits │ Labs │ Images│
├─────────────────────────────────────────────┤
│                                             │
│ AI Patient Brief                            │
│                                             │
│ "3 important changes since last visit..."  │
│                                             │
├─────────────────────────────────────────────┤
│                                             │
│ Medical Timeline                             │
│                                             │
│ ● Visit                                     │
│ │                                           │
│ ● Lab                                        │
│ │                                           │
│ ● X-Ray                                      │
│ │                                           │
│ ● Prescription                               │
└─────────────────────────────────────────────┘
```

---

# 36. الشكل الخاص بالـAI Report

```text
┌──────────────────────────────────────────────┐
│ AI Medical Analysis                          │
│                                              │
│ Status: Requires Doctor Review               │
├──────────────────────────────────────────────┤
│                                              │
│ Summary                                      │
│ ───────────────────────────────────────────  │
│ ...                                          │
│                                              │
│ Findings                                     │
│                                              │
│ ● Finding 1                                  │
│ ● Finding 2                                  │
│                                              │
│ Relevant Region                               │
│ [View on Image]                              │
│                                              │
│ Confidence                                    │
│ ━━━━━━━━━━━━━━━                              │
│                                              │
│ Doctor Notes                                  │
│ [..........................................] │
│                                              │
│ [Edit Report]       [Approve]                │
└──────────────────────────────────────────────┘
```

---

# 37. الشكل الخاص بالـDashboard الإداري

![Image](https://images.openai.com/static-rsc-4/MnMGhidXOEtgJiwaN4Qhp_1TzdxWMCUr_eKQtwNliVMBYMdlURQSCP136alJsx2fvk98XUv417D29QTuOVuH3Fkcezetuu6m6P1nYbhPWeswzvq9RpEfSzO20sebCagzZlyrcSrojFawqg3BBftX3SfCuG6RlQrJjIH94Dg1kCwqTnZew4X3ti6NGB4fR7Me?purpose=fullsize)

![Image](https://images.openai.com/static-rsc-4/E-Hnpk8OQ8vUURxZ4u_wVF7VuAKKNVUhip5PTqemH3Z8wamtoTFinb39f5adiLUSGYDiRXrb1CcN1Xd0WjTjRzENyrwn_kIv53f_6mmERp-nPu3kbEOLdpTLMh-X0NMATylvBAq1fAxqjy29BBrx7aBenYRHLLhQ1qSbKLQWh_kVijDtnbbzXa3Leop2wM-y?purpose=fullsize)

![Image](https://images.openai.com/static-rsc-4/eYo8KKSUX9Ku5rFfx3J2_jpAxAk8Uwa25WNPhFSKp5mvHuo6d9jd2F2eMV4FLh23QbevNG31x1pAgOIiQNPLXKSXnOZkNFxe4r0Mb4VtHOl1LOW__hbbjW3X8jFVHMeww5WF9MU5SFrEGyBFpoKF85Y-Y45pZD74OLgF4RI3dSwJcJrVxPGdEHROjVhPd2Y1?purpose=fullsize)

![Image](https://images.openai.com/static-rsc-4/83mMNHn1eoJOMuoGDYw3hD0TkLnyNYDtKz6pER09wdm-enacPSxy_Q-6LLeoRWUhBlFTatYPEabmkK2AfTDeMc4FHeR0kTexrpQLcJGl2PbKLc2V56PXK2bp1YB8Wb8VYsuG9bHj7F-h5sXTB0KN3_tuW84lqiNeDFn-A7yCpfymiUPy6IbvIj2xnFZPJqh5?purpose=fullsize)

![Image](https://images.openai.com/static-rsc-4/GJ1mySkrMSf4lP-lOy2e3YT8cIItQj72-KuZCfu0N9pBUT1zE2WWJKn_9bqN2Sg1zYNtvmPzfeW3O39mK9FbHZReoAdBMRNbizY8Lp9dF0uwx6uPqpyRlATHnO7YY_-lVApIEQiG5i_ISaeF_8jfK-lHU90BspqIPSiTFOCsYv1uNnSw9QqXHalp07E4JZ9x?purpose=fullsize)

![Image](https://images.openai.com/static-rsc-4/yuIZNC1eIQmzEO9S98MlCFM0vz6Ht_OkTG-Wb1gcuRdnqJako25ZIk--bQwE-yqq3OFdNPjONGP6dVu-jjUzzPwJp-c72eOwBFexFwFDdDEmo1ALVFw0K57drVnNQq58vCpBJFzojdDBAkE7SpufdydcqqnZ7hwLMUPAqmpqaGH6YlVHIqboRSILWAJelRwR?purpose=fullsize)

الإدارة ترى:

```text
Patients
Appointments
Revenue
Doctors
Rooms
AI Usage
Lab
Radiology
```

مع:

* Charts
* Filters
* Export
* Date ranges

---

# 38. Design System

أنشئ Design System قبل بناء الصفحات.

```text
Button
Input
Select
DatePicker
Modal
Drawer
Tabs
Card
Table
Badge
Avatar
Tooltip
Dropdown
Toast
Skeleton
Command Palette
Timeline
Chart
Medical Card
AI Card
Patient Card
```

ثم استخدمها في كل التطبيق.

**ممنوع أن يصمم الـAgent كل صفحة بأسلوب مختلف.**

---

# 39. Typography

استخدم خطاً مناسباً للعربية والإنجليزية.

مثلاً:

* IBM Plex Sans Arabic
* أو Noto Sans Arabic

مع خط إنجليزي متوافق.

الأولوية:

**وضوح القراءة الطبية.**

---

# 40. Responsive

### Desktop

Full workspace.

### Tablet

Sidebar collapsible.

### Mobile

Bottom navigation:

```text
Home
Patients
Appointments
AI
More
```

---

# 41. Dark Mode

Dark mode ليس مجرد عكس الألوان.

يجب إعادة تصميم:

* Medical image viewer.
* Charts.
* AI panels.
* Alerts.

بحيث تظل القراءة مريحة.

---

# 42. Accessibility

يجب دعم:

* Keyboard.
* Focus.
* Screen readers.
* Contrast.
* Font scaling.

---

# 43. Performance

خصوصاً الصور الطبية:

لا تحمل الصورة الأصلية كاملة دائماً.

استخدم:

* Lazy loading.
* Thumbnail.
* Progressive loading.
* CDN.
* Signed URLs.
* Image optimization.

---

# 44. أهم شيء في الـAI Agent

الـAgent لا يبدأ بكتابة 500 ملف.

يجب أن يعمل:

```text
Analyze
 ↓
Plan
 ↓
Create Architecture
 ↓
Implement
 ↓
Build
 ↓
Test
 ↓
Fix
 ↓
Commit
 ↓
Next Phase
```

---

# 45. Master Prompt للـAI Agent

وهذه هي النسخة التي أنصحك أن تعطيها للـAgent:

```text
You are the lead engineer responsible for building MedClinic AI.

Build a production-ready AI-powered clinical workspace for modern medical clinics.

DO NOT create a prototype.
DO NOT create fake buttons.
DO NOT create empty pages.
DO NOT leave critical features as TODO.
DO NOT hardcode secrets.

==================================================
TECH STACK
==================================================

Backend:
- C#
- .NET 10
- ASP.NET Core Web API
- Entity Framework Core
- PostgreSQL
- Redis
- Serilog
- Swagger/OpenAPI
- FluentValidation
- xUnit
- FluentAssertions
- Testcontainers

Frontend:
- Next.js
- React
- TypeScript
- Node.js runtime/tooling
- Tailwind CSS
- shadcn/ui
- TanStack Query
- React Hook Form
- Zod
- Lucide
- Playwright
- Vitest

Architecture:
Clean Architecture
Modular Design
Domain Driven Design where appropriate
REST API
API Versioning
Multi-tenancy

==================================================
CORE PRODUCT
==================================================

The product is not merely a clinic management system.

It is an:

"AI Clinical Workspace"

The doctor should be able to manage:

- patients
- appointments
- visits
- EMR
- prescriptions
- laboratory
- radiology
- medical images
- medical annotations
- billing
- notifications

while AI works inside the workflow.

==================================================
AI
==================================================

Create an IAIProvider abstraction.

Support:

- MockAIProvider
- Cloud AI provider abstraction
- Local AI provider abstraction

AI capabilities:

- patient summary
- clinical note drafting
- lab analysis
- medical image analysis integration
- AI chat
- timeline summarization
- voice-to-clinical-note architecture

AI output must always require physician review.

Never automatically approve diagnosis or prescriptions.

==================================================
KEY DIFFERENTIATORS
==================================================

Build:

1. AI Patient Brief
2. AI Patient Timeline
3. AI Clinical Assistant
4. AI Lab Analyzer
5. AI Medical Imaging workflow
6. Medical Image Viewer
7. Medical Canvas
8. Image annotations
9. Interactive Body Map
10. Future Dental Module
11. Future Voice Scribe
12. Doctor + AI collaborative workflow

==================================================
MEDICAL IMAGE WORKSPACE
==================================================

Create:

- zoom
- pan
- rotate
- brightness
- contrast
- grayscale
- invert
- pen
- brush
- arrow
- rectangle
- circle
- polygon
- text
- measurement
- undo
- redo

Never modify the original image.

Store annotation data separately.

==================================================
SECURITY
==================================================

Implement:

- authentication
- authorization
- RBAC
- multi-tenancy
- tenant isolation
- audit logs
- secure file storage
- signed URLs
- rate limiting
- input validation
- secure secrets
- secure headers
- no sensitive data in logs

==================================================
DATABASE
==================================================

Use PostgreSQL.

Use EF Core migrations.

Create proper:

- indexes
- foreign keys
- constraints
- relationships
- audit fields
- soft deletion where appropriate

==================================================
UX
==================================================

The UI must feel like a premium modern SaaS product.

Do NOT build a traditional hospital ERP interface.

Use:

- clean spacing
- calm healthcare colors
- modern cards
- interactive charts
- command palette
- responsive layouts
- skeleton loading
- meaningful empty states
- excellent error states
- subtle animations

Support:

Arabic
English
RTL
LTR
Dark Mode
Light Mode

==================================================
DOCTOR DASHBOARD
==================================================

Show:

- today's patients
- upcoming appointments
- AI patient briefs
- pending lab results
- pending imaging reviews
- follow-ups
- AI tasks

==================================================
PATIENT PROFILE
==================================================

Create:

- patient overview
- AI brief
- timeline
- visits
- labs
- imaging
- prescriptions
- billing

==================================================
TESTING
==================================================

Write unit tests.

Write integration tests.

Write E2E tests.

Critical E2E flow:

Login
Create clinic
Create doctor
Create patient
Create appointment
Create visit
Upload lab
Run AI analysis
Upload image
Annotate image
Generate AI summary
Doctor review
Prescription
Follow-up

==================================================
DEVOPS
==================================================

Create:

Dockerfile
docker-compose.yml

Create GitHub Actions for:

- backend build
- frontend build
- tests
- lint
- security checks
- E2E

==================================================
DOCUMENTATION
==================================================

Create:

README.md
ARCHITECTURE.md
SECURITY.md
CONTRIBUTING.md

Document:

- setup
- environment variables
- database
- migrations
- AI configuration
- Docker
- testing
- deployment

==================================================
EXECUTION STRATEGY
==================================================

Do not implement everything in one uncontrolled step.

Work in phases:

Phase 0:
Repository and architecture.

Phase 1:
Authentication + users + roles + multi-tenancy.

Phase 2:
Patients + doctors + appointments.

Phase 3:
EMR + visits + prescriptions + timeline.

Phase 4:
Laboratory + radiology.

Phase 5:
AI Gateway + MockAI + AI Assistant.

Phase 6:
Medical Imaging + Canvas + annotations.

Phase 7:
Billing + notifications.

Phase 8:
Advanced AI.

Phase 9:
Testing.

Phase 10:
Docker + CI/CD + documentation.

After every phase:

1. Build.
2. Run tests.
3. Fix errors.
4. Run lint.
5. Run type checks.
6. Commit changes.
7. Continue.

==================================================
GITHUB
==================================================

Create repository:

medclinic-ai

Use meaningful commits.

Never commit:

.env
API keys
passwords
private keys
real patient information

==================================================
FINAL REQUIREMENT
==================================================

Do not stop at planning.

Actually implement the project.

If a dependency or external AI provider requires credentials:

- create the abstraction
- create environment variables
- create MockAIProvider
- document the requirement
- never hardcode credentials

If a medical AI capability is not safely or technically available:

do not fake the result.

Create the correct integration boundary and document the limitation.

The final result must be a real, maintainable, scalable medical SaaS application.
```

---

# 46. نقطة مهمة جداً قبل أن تبدأ

هناك منافس جديد ظهر في السوق المصري يستحق أن نراقبه تحديداً، وهو **CareFlow**؛ لأنه يقترب جداً من الاتجاه الذي اخترناه: يأخذ التاريخ المرضي بالصوت باللهجة المصرية، يقرأ التحاليل والصور الإشعاعية، ويجمعها في Dashboard واحدة قبل دخول الطبيب. لذلك **لا أنصح أن يكون تميزنا فقط "AI + Lab + Radiology"**؛ يجب أن نركز على الـ**Clinical Workspace التفاعلي، والـCanvas، والـTimeline، والتعاون بين الطبيب والـAI، وتجربة الطبيب العربية**. ([CareFlow AI][2])

كذلك يوجد بالفعل مشروع Smart Care مفتوح على GitHub يستهدف السوق المصري ويجمع React مع AI والصور والصوت والمواعيد؛ وهذا يؤكد أهمية أن يكون منتجنا مميزاً على مستوى الـworkflow والـarchitecture وليس مجرد تجميع Features. ([GitHub][3])

---

# 47. الشكل النهائي الذي أريد أن يشعر به الطبيب

الفكرة الأساسية:

```text
                    MEDCLINIC AI
                         │
             ┌───────────┴───────────┐
             │                       │
        Clinic Operations      Clinical Workspace
             │                       │
       Appointments               Patient
       Billing                    Timeline
       Staff                      EMR
       Reports                    Labs
                                  Imaging
                                  Prescription
                                       │
                                       ▼
                              ┌────────────────┐
                              │   AI COPILOT   │
                              └────────────────┘
                              │       │       │
                            Text     Vision   Voice
                              │       │       │
                              ▼       ▼       ▼
                           Summary  Imaging  Scribe
                           Labs     Canvas   Notes
```

**وهنا أرى أن لدينا منتجًا أقوى بكثير من "برنامج إدارة عيادات".**

هو يصبح:

> **نظام تشغيل رقمي للعيادة، وفي قلبه AI Copilot يعمل بجانب الطبيب.**

والتصميم المقترح ليس مجرد فكرة نظرية؛ هناك اتجاه واضح حالياً نحو واجهات Healthcare SaaS هادئة، data-driven، تجمع المواعيد والمرضى والتحليلات والـAI في مساحة واحدة، بينما واجهات التصوير الطبي الحديثة تثبت أهمية الجمع بين الصورة، الـannotations والـfindings في نفس الشاشة. ([Dribbble][1])

### القرار الذي أنصح بتثبيته للـAI Agent

**Backend:** C# / .NET 10 / ASP.NET Core
**Database:** PostgreSQL
**ORM:** EF Core
**Cache/Jobs:** Redis + Hangfire عند الحاجة
**Frontend:** Next.js + React + TypeScript
**Runtime/tooling للواجهة:** Node.js
**UI:** Tailwind + shadcn/ui
**AI:** Provider abstraction + Mock/Local/Cloud
**Architecture:** Clean Architecture + Modular + Multi-tenant
**Deployment:** Docker + GitHub Actions
**Repository:** `medclinic-ai`

وبهذا الشكل، الـAgent عنده الآن **الرؤية التجارية + الـUX + الـfeatures + الـarchitecture + الـdatabase + الـAI + الـroadmap + تعليمات التنفيذ**، بدلاً من مجرد Prompt يطلب منه "ابني برنامج عيادات".

[1]: https://dribbble.com/shots/26533782-Clinic-Dashboard-Redefined?utm_source=chatgpt.com "Clinic Dashboard UI – Modern Healthcare SaaS Design for EMR Plat by Vijay parmar on Dribbble"
[2]: https://www.careflowai.health/?utm_source=chatgpt.com "CareFlow — A Clinical AI Ecosystem for Egypt's Clinics"
[3]: https://github.com/iknevo/smart-care?utm_source=chatgpt.com "GitHub - iknevo/smart-care · GitHub"
