# 🎨 CaDaCook - Game Design System & UI Architecture

เอกสารนี้ระบุการออกแบบเกมเพลย์ (Game Design Document), ระบบอุปสรรคและเหตุการณ์ไม่คาดฝัน (Dynamic Events & Obstacles), สถาปัตยกรรมระบบอินเทอร์เฟซผู้ใช้ (UI Architecture), ระบบ Finite State Machine (FSM), และระบบเสียง/ภาพของเกม **CaDaCook**

---

## 🌟 1. ปรัชญาการออกแบบเกมเพลย์ (Core Design Principles)

1. **Clear & Immediate Visual Feedback:** ทุกการกระทำของผู้เล่น (หยิบของ, วางของ, หั่น, ทอด, เสิร์ฟ, ฉีดดับเพลิง, ลื่นไถล) จะต้องมีการตอบสนองทางภาพและเสียงทันที
2. **Decoupled Architecture:** ระบบการแสดงผลทางสายตา (Visuals), เสียง (Audio) และหน้าจอ (UI) จะต้องไม่ผูกติดกับตรรกะของเกม (Game Logic) โดยสื่อสารกันผ่าน **C# Events** เท่านั้น
3. **Emergent Gameplay & Chaos:** เพิ่มความสนุกและเสียงหัวเราะด้วยเหตุการณ์ไม่คาดฝัน (ไฟไหม้, พื้นลื่น, ชั่วโมงเร่งด่วน, แมวขโมยวัตถุดิบ) ที่ท้าทายการวางแผนและการสื่อสารของผู้เล่น
4. **Data-Driven Scalability:** การเพิ่มสูตรอาหาร, วัตถุดิบ, หรือสูตรการปรุง ต้องทำได้ง่ายผ่านการสร้าง `ScriptableObject` โดยไม่ต้องแตะต้องโค้ดหลัก

---

## 🌪️ 2. ระบบอุปสรรคและเหตุการณ์ไม่คาดฝัน (Dynamic Events & Obstacles)

```text
                                  ┌────────────────────────────────┐
                                  │   CaDaCook Gameplay Systems    │
                                  └───────────────┬────────────────┘
                                                  │
         ┌────────────────────────┬───────────────┴────────────────┬────────────────────────┐
         ▼                        ▼                                ▼                        ▼
┌──────────────────┐    ┌──────────────────┐             ┌──────────────────┐    ┌──────────────────┐
│  Dynamic Events  │    │  Environmental   │             │ NPC Distractions │    │ Multi-Theme Maps │
├──────────────────┤    ├──────────────────┤             ├──────────────────┤    ├──────────────────┤
│ • Fire Hazard    │    │ • Slippery Floor │             │ • Neko Cat NPC   │    │ • Cozy Kitchen   │
│ • FireExtinguish │    │ • Moving Counter │             │ • Pothole Trap   │    │ • Beach Raft     │
│ • Rush Hour (2x) │    │ • Conveyor Belt  │             │   (Tripping)     │    │   (Ocean Tilt)   │
└──────────────────┘    └──────────────────┘             └──────────────────┘    └──────────────────┘
```

### 2.1 ระบบไฟไหม้เตาและถังดับเพลิง (Fire Hazard & Fire Extinguisher)
- **สเตตัสไฟไหม้ (`FireHazard.cs`):** เมื่อเตาอบทอดเนื้อจนไหม้ (`State.Burned`) เตาจะติดไฟ (`Ignite()`) พ่นเปลวไฟและควันดำออกมา ผู้เล่นจะไม่สามารถหยิบจับอาหารบนเตาได้จนกว่าจะดับไฟสำเร็จ
- **ถังดับเพลิง (`FireExtinguisher.cs`):** วัตถุเครื่องมือที่ผู้เล่นสามารถถือได้ เมื่อกดปุ่ม Alternate Interact (`F`) จะพ่นละอองโฟมดับเพลิงใส่เตาเพื่อลดระดับความร้อน (`Extinguish()`) เมื่อไฟดับสนิทจึงจะสามารถนำเนื้อไหม้ไปทิ้งถังขยะได้

### 2.2 ระบบพื้นลื่นคราบน้ำมัน (Slippery Floor)
- **`SlipperyFloor.cs`:** คราบน้ำมันบนทางเดิน เมื่อผู้เล่นเดินเหยียบจะเกิดแรงเฉื่อยไถลไปข้างหน้าตามทิศทางเดิม (`slipMomentum`) พร้อมหมุนตัวเคว้งคว้าง (`spinSpeed = 360°/s`) ควบคุมทิศทางได้ยากขึ้นชั่วขณะ

### 2.3 ระบบชั่วโมงเร่งด่วน (Rush Hour Event)
- **`RushHourManager.cs`:** เมื่อเวลาแข่งขันผ่านไป 50% ระบบจะเข้าสู่โหมดชั่วโมงเร่งด่วนเป็นเวลา 20 วินาที
- **ฟีเจอร์:** ออเดอร์จะเข้ามาเร็วขึ้น 2 เท่า (`fastSpawnTimerMax = 2s`) และจานที่ส่งสำเร็จจะได้รับคะแนนพิเศษ 2 เท่า (`scoreMultiplier = 2x`)
- **การแจ้งเตือน (`RushHourUI.cs`):** แถบป้ายกระพริบสีแดง-ทอง "⚡ RUSH HOUR 2X! ⚡" พร้อมนับเวลาถอยหลัง

### 2.4 ระบบสัตว์ป่วนครัว (Kitchen Distractions & Traps)
- **แมวขโมยของ (`KitchenCatNPC.cs`):** แมวจะแอบย่องเข้ามาในครัว เล็งเคาน์เตอร์ที่มีอาหารวางอยู่เพื่อขโมยไปกิน หากผู้เล่นเดินเข้าไปใกล้จะส่งเสียงขู่ไล่ (`ScareCat()`) ให้แมวทิ้งของและวิ่งหนีออกนอกครัวไป
- **หลุมดักสะดุด (`PotholeTrap.cs`):** หลุมหรือฝาท่อระบายน้ำที่ทำให้เชฟเดินช้าลง 60% ชั่วคราวเมื่อก้าวเหยียบ

### 2.5 ระบบแผนที่หลายธีมและคลื่นทะเล (Multi-Theme Maps & Raft Tilt)
- **Map 1: Cozy Kitchen:** ห้องครัวมาตรฐาน สว่างสดใส เหมาะสำหรับเรียนรู้ระบบ
- **Map 2: Beach Raft Kitchen (`RaftKitchenTilt.cs`):** ห้องครัวบนแพริมหาดที่มีพื้นที่จำกัด และตัวแพจะโยกเอียงตามระลอกคลื่นทะเล (Pitch & Roll Sine Wave) อย่างสมจริง

---

## 🍳 3. การออกแบบระบบเคาน์เตอร์และ Finite State Machine

### 3.1 ระบบเคาน์เตอร์เตาอบ (Stove Counter State Machine)
`StoveCounter` ใช้ Finite State Machine ควบคุมลำดับการปรุงอาหาร 4 สเตต:

```text
               วางเนื้อดิบ (MeatPatty)
  [ IDLE ] ─────────────────────────────► [ FRYING ]
                                                │
                                                │ fryingTimer >= fryingTimerMax
                                                ▼
  [ BURNED ] ◄─────────────────────────── [ FRIED ]
  (Fire Hazard)  burningTimer >= burningTimerMax (อาหารสุกพร้อมเสิร์ฟ)
```

- **`State.Idle`:** เตาว่างเปล่า ไม่มีเสียงฉ่า ไม่มีควัน
- **`State.Frying`:** กำลังทอด วัตถุดิบดิบเปลี่ยนเป็นสุกตามเวลา `fryingTimerMax` แสดงหลอด Progress Bar สีเหลือง และเสียงกระทะทอด
- **`State.Fried`:** อาหารสุกแล้ว พร้อมหยิบขึ้นจาน หากทิ้งไว้จะเริ่มนับเวลาไหม้ `burningTimer` พร้อมไฟกระพริบและเสียงเตือน
- **`State.Burned`:** อาหารไหม้เกรียม และจุดติดไฟ `FireHazard.Ignite()` บังคับให้ต้องใช้ถังดับเพลิงมาฉีดดับก่อนหยิบของทิ้ง

---

## 🖥️ 4. สถาปัตยกรรมระบบอินเทอร์เฟซผู้ใช้ (UI Architecture)

ระบบ UI ของเกม CaDaCook แบ่งออกเป็น 2 ประเภทหลัก:

1. **Screen-Space Canvas HUD:**
   - [`DeliveryManagerUI.cs`](file:///d:/unity/My%20project/Assets/Scripts/UI/DeliveryManagerUI.cs): แสดงรายการการ์ดออเดอร์
   - [`RushHourUI.cs`](file:///d:/unity/My%20project/Assets/Scripts/UI/RushHourUI.cs): แสดงป้ายแจ้งเตือนชั่วโมงเร่งด่วน
   - [`GamePlayingClockUI.cs`](file:///d:/unity/My%20project/Assets/Scripts/UI/GamePlayingClockUI.cs): นาฬิกาจับเวลากลมถอยหลัง
   - [`GameStartCountdownUI.cs`](file:///d:/unity/My%20project/Assets/Scripts/UI/GameStartCountdownUI.cs): ตัวเลขนับถอยหลัง 3 2 1
   - [`GamePauseUI.cs`](file:///d:/unity/My%20project/Assets/Scripts/UI/GamePauseUI.cs): เมนูหยุดเกม (Resume, Main Menu พร้อม auto-focus)
   - [`GameOverUI.cs`](file:///d:/unity/My%20project/Assets/Scripts/UI/GameOverUI.cs): สรุปยอดจานอาหารที่ส่งสำเร็จ
2. **World-Space Canvas:**
   - [`ProgressBarUI.cs`](file:///d:/unity/My%20project/Assets/Scripts/UI/ProgressBarUI.cs): หลอดแสดงความคืบหน้าลอยเหนือเคาน์เตอร์
   - [`PlateIconsUI.cs`](file:///d:/unity/My%20project/Assets/Scripts/UI/PlateIconsUI.cs): แสดงไอคอนวัตถุดิบบนจาน
   - [`LookAtCamera.cs`](file:///d:/unity/My%20project/Assets/Scripts/LookAtCamera.cs): หมุนระนาบ Canvas เข้าหากล้องตลอดเวลา (Billboard)

---

## 🎨 5. โทนสีและสไตล์การออกแบบ (Visual Palette)

- **Selected Highlight Color:** สีขาวเรืองแสง (`#FFFFFF`) บน `SelectedCounterVisual`
- **Progress Normal Color:** สีเขียวมะนาว/เหลือง (`#84CC16` / `#EAB308`) สำหรับหลอดหั่นและทอด
- **Fire & Hazard Color:** สีส้มแดงเพลิง (`#F97316` / `#EF4444`) สำหรับเปลวไฟและป้าย Rush Hour
- **Delivery Success Color:** สีเขียวมรกต (`#10B981`) แจ้งเตือนเมื่อส่งอาหารสำเร็จ
- **Delivery Failed Color:** สีแดงกุหลาบ (`#F43F5E`) แจ้งเตือนเมื่อส่งอาหารผิดสูตร