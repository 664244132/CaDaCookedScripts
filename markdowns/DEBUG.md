# 🛠️ คู่มือการดีแบ๊กและแนวทางการพัฒนาเกมสำหรับ AI Agents (CaDaCook)

คู่มือนี้จัดทำขึ้นเพื่อให้ **AI Agents ทุกตัว** ที่เข้ามาร่วมวิเคราะห์ แก้ไขบัค หรือพัฒนาโปรเจกต์ **CaDaCook** (Unity 6 C#) สามารถทำงานได้อย่างมีประสิทธิภาพ ปฏิบัติตามมาตรฐานความปลอดภัยสูงสุด และวินิจฉัยปัญหาได้อย่างถูกต้อง แม่นยำ และรวดเร็ว

---

## 🚨 กฎเหล็กด้านความปลอดภัยและข้อจำกัดสำหรับ AI Agents (Hard Constraints)

1. **ห้ามใช้คำสั่ง Git Commit หรือ Git Push เด็ดขาด (Rule 11 - No Auto-Commits):**
   - ห้ามรัน git commit หรือ git push ด้วยตนเอง ผู้ใช้จะเป็นผู้ตรวจสอบความเรียบร้อยและ Commit ขึ้น GitHub เองเสมอ
2. **ห้ามแก้ไขหรือยุ่งเกี่ยวกับฐานข้อมูลโดยตรง (Rule 10 - No Direct DB Writes):**
   - หากมีการเชื่อมต่อระบบ Database หรือ Backend ในอนาคต ให้จัดเตรียมคำสั่ง SQL หรือ API Schema ให้ผู้ใช้นำไปดำเนินการเองเท่านั้น
3. **ห้ามลบไฟล์หรือดึงแพ็กเกจใหม่โดยไม่ได้รับอนุญาต (Rule 8, 13, 14):**
   - ห้ามใช้คำสั่งลบที่อันตราย (`rm -rf`) และห้ามติดตั้ง Unity Packages ใหม่ผ่าน UPM เว้นแต่จะได้รับการอนุมัติอย่างชัดเจนจากผู้ใช้
4. **ปฏิบัติตามกฎ 15 ข้อใน [`REFACTORCODE.md`](file:///c:/CaDaCooked/CaDaCookedScripts/markdowns/REFACTORCODE.md):**
   - แยก Game Logic ออกจาก Visual/Audio/UI ผ่าน C# Events
   - ปฏิบัติตามกฎ De Morgan's Laws & Early Return Guard Clauses ตามคู่มือ [`DeMorgansLaws.md`](file:///c:/CaDaCooked/CaDaCookedScripts/markdowns/DeMorgansLaws.md)
   - ป้องกัน Memory Leak ด้วยการ Unsubscribe (`-=`) ใน `OnDestroy()` เสมอ
   - Zero GC Alloc ในลูป `Update()` (ห้าม `new` หรือต่อสตริงทุกเฟรม)
5. **บันทึกประวัติการปรับปรุงลงใน [`LOG.md`](file:///c:/CaDaCooked/CaDaCookedScripts/markdowns/LOG.md) ทุกครั้ง (Rule 7):**
   - เมื่อทำการปรับปรุงโค้ดหรือเอกสาร ต้องสรุปผลลงใน `markdowns/LOG.md` เสมอ

---

## 🤖 ขั้นตอนการสืบค้นและวินิจฉัยบัคสำหรับ AI Agent (AI Agent Diagnostic Workflow)

เมื่อได้รับรายงานข้อผิดพลาด หรือคำเตือน (Errors/Warnings) ให้ AI Agent ดำเนินการตามลูป 5 ขั้นตอนดังนี้:

```text
[ 1. LOCATE ] ──► ค้นหาตำแหน่งสคริปต์/คอมโพเนนต์ที่เกี่ยวข้องจากผังโครงสร้าง
      │
      ▼
[ 2. ANALYZE ] ──► วิเคราะห์ Callstack, Null Safety, State Machine, LayerMask, Event Lifecycle
      │
      ▼
[ 3. PROPOSE ] ──► เสนอแนวทางแก้ไขและแผนงาน (Implementation Plan) ในโทน Senior Engineer (ภาษาไทย)
      │
      ▼
[ 4. IMPLEMENT ] ──► แก้ไขโค้ดเฉพาะจุดที่จำเป็น ไม่กระทบต่อ Gameplay เดิม
      │
      ▼
[ 5. VERIFY ] ──► ตรวจสอบ Syntax, Event Unsubscription, Null Guards ก่อนส่งมอบงาน
```

### รายละเอียดโฟลเดอร์หลักสำหรับค้นหาสคริปต์:
- **เคาน์เตอร์ทำอาหารทั้งหมด:** [Assets/Scripts/Counters/](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/Counters/)
- **ระบบเกมเพลย์และอีเวนต์:** [Assets/Scripts/Gameplay/](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/Gameplay/)
- **อุปสรรคและอันตรายในครัว:** [Assets/Scripts/Obstacles/](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/Obstacles/)
- **ระบบหน้าต่างผู้ใช้และ HUD:** [Assets/Scripts/UI/](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/UI/)
- **ฐานข้อมูลวัตถุดิบและสูตรอาหาร:** [Assets/Scripts/ScriptableObjects/](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/ScriptableObjects/)

---

## ⚡ ตารางวิเคราะห์สาเหตุและวิธีแก้ปัญหายอดนิยม (Rapid Troubleshooting Matrix)

| อาการของปัญหา (Symptom) | สาเหตุที่พบบ่อย (Root Cause) | แนวทางแก้ไขสำหรับ AI Agent (Solution) |
| :--- | :--- | :--- |
| **NullReferenceException ตอนเรียก Singleton** | มีการเรียกใช้ Instance ใน Awake() ก่อนที่คลาสเจ้าของจะตั้งค่า Instance = this | ย้ายโค้ดไปทำงานใน Start() หรือเพิ่ม Guard if (TargetClass.Instance != null) |
| **กดปุ่ม E หรือ F แล้วเชฟไม่ตอบสนองกับเคาน์เตอร์** | 1. เคาน์เตอร์ไม่ได้อยู่บน Layer ที่ระบุใน countersLayerMask<br>2. ตัวเคาน์เตอร์ไม่มี Collider<br>3. CapsuleCast/Raycast ติด Trigger อื่น | ตรวจสอบ Layer ของ Counter ใน Inspector และตรวจสอบว่า Physics.CapsuleCast มี QueryTriggerInteraction.Ignore ใน [Player.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/Player.cs) |
| **หั่นอาหารไม่ได้ / วัตถุดิบไม่ยอมเปลี่ยนเป็นชิ้นหั่น** | ไม่มีสูตร CuttingRecipeSO ของวัตถุดิบชิ้นนั้นอยู่ใน cuttingRecipeSOArray | ตรวจสอบ [CuttingCounter.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/Counters/CuttingCounter.cs) และตรวจสอบรายการ ScriptableObject ของเขียงหั่น |
| **ทอดเนื้อแล้วเตาไม่ทำงาน / ไม่เปลี่ยนเป็นเนื้อสุก** | ไม่มี FryingRecipeSO สำหรับวัตถุดิบชิ้นนั้น | ตรวจสอบ [StoveCounter.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/Counters/StoveCounter.cs) และตั้งค่า ryingTimerMax ใน Inspector |
| **นำวัตถุดิบวางรวมใส่จานไม่ได้** | วัตถุดิบไม่อยู่ใน alidKitchenObjectSOList ของ PlateKitchenObject | เพิ่ม KitchenObjectSO ของวัตถุดิบนั้นลงในลิสต์ของ Prefab จาน |
| **Event ถูกเรียกซ้ำซ้อน 2 ครั้งเมื่อเปลี่ยน Scene** | มีการ += ใน Start() แต่ลืม -= ใน OnDestroy() ทำให้ Delegate ค้างในหน่วยความจำ | เพิ่ม OnDestroy() และสั่ง -= ถอนการเชื่อมต่อ Event ตามกฎข้อ 7 ของ [REFACTORCODE.md](file:///c:/CaDaCooked/CaDaCookedScripts/markdowns/REFACTORCODE.md) |
| **ข้อความ UI แสดงผลเป็นกล่องสี่เหลี่ยม □** | มีการใช้อักขระ Unicode พิเศษ หรือ Emojis ที่ฟอนต์สากลไม่มี Glyph รองรับ | ปรับเปลี่ยนข้อความให้เป็น **Pure ASCII** หรือจัดรูปแบบผ่าน Rich Text <color=#FFD700><b>...</b></color> แทนการใช้อีโมจิ |
| **เปิดโปรเจกต์เครื่องใหม่แล้วแมพว่างเปล่า (Untitled Scene)** | Unity โหลด Scene เริ่มต้นว่างเปล่าเนื่องจากไม่ได้บันทึก Last Opened Scene ข้ามเครื่อง | ดับเบิ้ลคลิกเปิด [Assets/Scenes/GameScene.unity](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scenes/GameScene.unity) หรือ [MainMenuScene.unity](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scenes/MainMenuScene.unity) |
| **CS0234: The type or namespace 'InputSystem' does not exist** | โฟลเดอร์ Packages/ ถูกละเลย หรือไฟล์ manifest.json หายไป | ตรวจสอบให้แน่ใจว่า Git ติดตาม Packages/manifest.json เพื่อให้ Unity Package Manager ดาวน์โหลดแพ็กเกจให้อัตโนมัติ |

---

## 🔄 สถาปัตยกรรมฉากและลำดับการ Build (Scene Architecture & Build Flow)

โปรเจกต์ใช้ระบบจัดการเปลี่ยน Scene ผ่านคลาสกลาง [Loader.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/Loader.cs) ร่วมกับฉากคั่น [LoadingScene.unity](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scenes/LoadingScene.unity):

```text
[ Build Index 0: MainMenuScene ]
              │
              ▼  (กดปุ่ม Play ใน MainMenuUI.cs)
[ Build Index 2: LoadingScene ] (รัน LoaderCallback.cs หน่วง 1 เฟรมให้ระบบเตรียมหน่วยความจำ)
              │
              ▼
[ Build Index 1: GameScene ] (เกมเพลย์หลัก: ห้องครัว, เคาน์เตอร์, ระบบสั่งอาหาร, อุปสรรค)
              │
              ├──► (กด Pause -> Main Menu ใน GamePauseUI.cs) ──┐
              │                                                ▼
              └──► (จบเกม 5 วินาที ใน GameOverUI.cs) ──► [ LoadingScene ] ──► [ MainMenuScene ]
```

### ตารางตรวจสอบ Build Settings (ProjectSettings/EditorBuildSettings.asset):
| Build Index | Scene Path | Scene Name | หน้าที่การทำงาน |
| :---: | :--- | :--- | :--- |
| **0** | Assets/Scenes/MainMenuScene.unity | [MainMenuScene](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scenes/MainMenuScene.unity) | **ฉากเริ่มต้นเกม (Startup Scene)** เมนูหลัก ปุ่ม Play และ Quit |
| **1** | Assets/Scenes/GameScene.unity | [GameScene](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scenes/GameScene.unity) | **ฉากห้องครัวหลัก** รวมโมเดล เคาน์เตอร์ อุปสรรค และระบบเกมเพลย์ |
| **2** | Assets/Scenes/LoadingScene.unity | [LoadingScene](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scenes/LoadingScene.unity) | **ฉากหน้าต่างโหลด** สลับเปลี่ยน Scene อย่างราบรื่นไร้รอยต่อ |

> ⚠️ **ข้อบังคับสำหรับ AI Agent:**
> - ลำดับ Index 0 **ต้องเป็น MainMenuScene เสมอ**
> - ชื่อ Scene ใน Build Settings ต้องตรงกับ enum Loader.Scene ใน [Loader.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/Loader.cs) แบบ Case-sensitive

---

## 📋 เช็คลิสต์ตรวจสอบความสมบูรณ์ก่อนตอบผู้ใช้ (AI Pre-Flight Checklist)

ก่อนที่ AI Agent จะแจ้งผลการทำงานให้ผู้ใช้ทราบ ต้องตรวจสอบหัวข้อเหล่านี้ให้ครบถ้วน:
- [ ] โค้ดที่แก้ไขไม่มี Syntax Error หรือ Compilation Error
- [ ] มีการใส่ Null Check ก่อนเรียกใช้ Property/Method ของ GameObject หรือ Component เสมอ
- [ ] หากมีการใช้ += Event ต้องมี -= ใน OnDestroy() เสมอ
- [ ] ไม่มีคำสั่ง 
ew หรือการต่อสตริงในลูป Update()
- [ ] ลิงก์เอกสารทั้งหมดใช้ Path โครงสร้างปัจจุบัน (c:/CaDaCooked/CaDaCookedScripts/...)
- [ ] ไม่อนุญาตให้รัน git commit หรือ git push โดยเด็ดขาด
- [ ] บันทึกการเปลี่ยนแปลงลงใน [markdowns/LOG.md](file:///c:/CaDaCooked/CaDaCookedScripts/markdowns/LOG.md) เรียบร้อยแล้ว