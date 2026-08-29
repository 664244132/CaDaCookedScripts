# 🎨 CaDaCook - Game Design System & UI Architecture

เอกสารนี้ระบุการออกแบบเกมเพลย์ (Game Design Document), สถาปัตยกรรมระบบอินเทอร์เฟซผู้ใช้ (UI Architecture), ระบบ Finite State Machine (FSM), และระบบเสียง/ภาพของเกม **CaDaCook**

---

## 🌟 1. ปรัชญาการออกแบบเกมเพลย์ (Core Design Principles)

1. **Clear & Immediate Visual Feedback:** ทุกการกระทำของผู้เล่น (หยิบของ, วางของ, หั่น, ทอด, เสิร์ฟ) จะต้องมีการตอบสนองทางภาพและเสียงทันที (เช่น กรอบเรืองแสงสีขาวรอบเคาน์เตอร์, แอนิเมชันเปิดตู้, มีดขยับ, ควันไฟ)
2. **Decoupled Architecture:** ระบบการแสดงผลทางสายตา (Visuals), เสียง (Audio) และหน้าจอ (UI) จะต้องไม่ผูกติดกับตรรกะของเกม (Game Logic) โดยสื่อสารกันผ่าน **C# Events** เท่านั้น
3. **Data-Driven Scalability:** การเพิ่มสูตรอาหาร, วัตถุดิบ, หรือสูตรการปรุง ต้องทำได้ง่ายผ่านการสร้าง `ScriptableObject` โดยไม่ต้องแตะต้องโค้ดหลัก
4. **Intuitive Controls & Responsiveness:** การควบคุมเชฟต้องลื่นไหล ตอบสนองฉับไว และรองรับทั้งคีย์บอร์ดและเกมแพด

---

## 🍳 2. การออกแบบระบบเคาน์เตอร์และ Finite State Machine

### 2.1 ระบบเคาน์เตอร์เตาอบ (Stove Counter State Machine)
`StoveCounter` ใช้ Finite State Machine ควบคุมลำดับการปรุงอาหาร 4 สเตต:

```text
               วางเนื้อดิบ (MeatPatty)
  [ IDLE ] ─────────────────────────────► [ FRYING ]
                                                │
                                                │ fryingTimer >= fryingTimerMax
                                                ▼
  [ BURNED ] ◄─────────────────────────── [ FRIED ]
               burningTimer >= burningTimerMax (อาหารสุกพร้อมเสิร์ฟ)
```

- **`State.Idle`:** เตาว่างเปล่า ไม่มีเสียงฉ่า ไม่มีควัน
- **`State.Frying`:** กำลังทอด วัตถุดิบดิบเปลี่ยนเป็นสุกตามเวลา `fryingTimerMax` แสดงหลอด Progress Bar สีเหลือง และเสียงกระทะทอด (`StoveCounterSound`)
- **`State.Fried`:** อาหารสุกแล้ว พร้อมหยิบขึ้นจาน หากทิ้งไว้จะเริ่มนับเวลาไหม้ `burningTimer` พร้อมไฟกระพริบและเสียงเตือน
- **`State.Burned`:** อาหารไหม้เกรียมเป็นถ่าน ไม่สามารถนำไปเสิร์ฟได้ ต้องนำไปทิ้งที่ `TrashCounter` เท่านั้น

### 2.2 ระบบเขียงหั่นอาหาร (Cutting Counter Mechanism)
- ผู้เล่นต้องกดปุ่ม Alternate Interact (`F` หรือปุ่มบน Gamepad) ซ้ำๆ
- ทุกครั้งที่กด จะเรียก `cuttingProgress++` พร้อมส่งอีเวนต์ `OnCut` ให้มีดเล่นแอนิเมชันและเล่นเสียงหั่น
- เมื่อ `cuttingProgress >= cuttingProgressMax` วัตถุดิบดิบจะถูกทำลาย และถูกแทนที่ด้วยชิ้นส่วนที่หั่นแล้ว (เช่น มะเขือเทศ ➔ มะเขือเทศหั่นแว่น)

### 2.3 ระบบการประกอบอาหารลงจาน (Plate Assembly System)
- จาน (`PlateKitchenObject`) สามารถรองรับวัตถุดิบได้หลายชนิดพร้อมกัน
- ตรวจสอบรายการวัตถุดิบที่อนุญาตผ่าน `validKitchenObjectSOList` (เช่น ขนมปังเบอร์เกอร์, ชิ้นเนื้อ, มะเขือเทศหั่น, กะหล่ำปลีหั่น, ชีส)
- เมื่อใส่วัตถุดิบสำเร็จ `PlateCompleteVisual` จะเปิดการแสดงผล 3D Mesh เฉพาะของวัตถุดิบนั้นบนจาน และ `PlateIconsUI` จะเพิ่มไอคอน 2D ลอยเหนือจาน

---

## 🖥️ 3. สถาปัตยกรรมระบบอินเทอร์เฟซผู้ใช้ (UI Architecture)

ระบบ UI ของเกม CaDaCook แบ่งออกเป็น 2 ประเภทหลัก:

```text
                                  ┌───────────────────────────────┐
                                  │      CaDaCook UI Systems      │
                                  └──────────────┬────────────────┘
                                                 │
                  ┌──────────────────────────────┴──────────────────────────────┐
                  ▼                                                             ▼
     ┌────────────────────────┐                                   ┌───────────────────────────┐
     │  Screen-Space Canvas   │                                   │    World-Space Canvas     │
     └────────────┬───────────┘                                   └─────────────┬─────────────┘
                  │                                                             │
   ├── DeliveryManagerUI (รายการออเดอร์)                            ├── ProgressBarUI (หลอดพลัง)
   ├── GamePlayingClockUI (นาฬิกาจับเวลา)                           ├── PlateIconsUI (ไอคอนบนจาน)
   ├── GameStartCountdownUI (นับ 3 2 1)                            └── LookAtCamera (Billboard)
   ├── GamePauseUI (เมนูหยุดเกม)
   └── GameOverUI (สรุปผลคะแนน)
```

### 3.1 World-Space UI & LookAtCamera Billboard System
- **`ProgressBarUI.cs`:** แถบแสดงความคืบหน้าลอยเหนือเคาน์เตอร์หั่นและเตาอบ เชื่อมต่อกับอินเทอร์เฟซ `IHasProgress` แบบ Generic ซ่อนตัวเองเมื่อหลอดอยู่ที่ 0 หรือเต็ม 1
- **`PlateIconsUI.cs`:** แสดงไอคอนวัตถุดิบที่อยู่ในจานแบบไดนามิก โดยสร้าง `PlateIconsSingleUI` ตามจำนวนวัตถุดิบจริง
- **`LookAtCamera.cs`:** จัดการให้ UI 3 มิติในฉากหันหน้าตรงเข้าหากล้องตลอดเวลา (Billboard Effect) โดยมีโหมดการทำงาน:
  - `LookAt`: หันเข้าหาตำแหน่งกล้องตรงๆ
  - `LookAtInverted`: หันหลังให้กล้อง (สำหรับ Canvas ที่พลิกด้าน)
  - `CameraForward`: วางระนาบขนานกับมุมกล้อง
  - `CameraForwardInverted`: วางระนาบขนานแบบกลับด้าน

### 3.2 Screen-Space Canvas HUD
- **`DeliveryManagerUI.cs`:** แดชบอร์ดแสดงการ์ดออเดอร์อาหารที่มุมซ้ายบน สร้างและทำลายการ์ด `DeliveryManagerSingleUI` ตามรายการ `waitingrecipeSOList`
- **`GamePlayingClockUI.cs`:** หลอดวงกลม (Radial Fill) แสดงเวลาที่เหลือของการแข่งขัน คำนวณจาก `GetGamePlayingTimerNormalized()`
- **`GameStartCountdownUI.cs`:** แสดงตัวเลขนับถอยหลังพร้อมเสียงเตือน และซ่อนตัวเมื่อเริ่มเล่นเกม
- **`GameOverUI.cs`:** สรุปยอดจานอาหารที่ส่งสำเร็จ (`successfulRecipesAmount`) พร้อมปุ่มกลับหน้าเมนู

---

## 🎨 4. โทนสีและสไตล์การออกแบบ (Visual Palette & UI Aesthetics)

- **Selected Highlight Color:** สีขาวเรืองแสง (`#FFFFFF`) ใช้บน `SelectedCounterVisual`
- **Progress Normal Color:** สีเขียวมะนาว/เหลือง (`#84CC16` / `#EAB308`) สำหรับหลอดหั่นและทอดปกติ
- **Warning / Burning Color:** สีแดงสด (`#EF4444`) และควันดำสำหรับเตาอบที่กำลังจะไหม้
- **Delivery Success Color:** สีเขียวมรกต (`#10B981`) แจ้งเตือนเมื่อส่งอาหารสำเร็จ
- **Delivery Failed Color:** สีแดงกุหลาบ (`#F43F5E`) แจ้งเตือนเมื่อส่งอาหารผิดสูตร
- **Typography:** ใช้ **TextMesh Pro** ด้วยฟอนต์สไตล์ Casual/Comic ที่อ่านง่าย สดใส และชัดเจนทุกความละเอียดหน้าจอ

---

## 🔊 5. ระบบเสียงและเสียงตอบสนอง (Audio Architecture)

- ควบคุมระบบเสียงทั้งหมดผ่าน [`SoundManager.cs`](file:///d:/unity/My%20project/Assets/Scripts/SoundManager.cs)
- เก็บรายการ AudioClip ทั้งหมดไว้ใน [`AudioClipRefsSO.cs`](file:///d:/unity/My%20project/Assets/Scripts/ScriptableObjects/AudioClipRefsSO.cs)
- ใช้ฟังก์ชัน `PlaySound(AudioClip[], Vector3, float)` สุ่มเลือกเสียงจากอาเรย์เพื่อลดความซ้ำซาก (Sound Fatigue) เช่น เสียงหั่นมีด เสียงฝีเท้า
- รองรับการเล่นเสียงแบบ 3D Positional Audio (เสียงดังตามตำแหน่งเคาน์เตอร์ในครัว)