# 🎨 CaDaCook - Game Design System & UI Architecture

เอกสารนี้ระบุการออกแบบเกมเพลย์ (Game Design Document), ระบบอุปสรรคและเหตุการณ์ไม่คาดฝัน (Dynamic Events & Obstacles), สถาปัตยกรรมระบบอินเทอร์เฟซผู้ใช้ (UI Architecture), ระบบ Finite State Machine (FSM), และระบบเสียง/ภาพของเกม **CaDaCook**

---

## 🌟 1. ปรัชญาการออกแบบเกมเพลย์ (Core Design Principles)

1. **Clear & Immediate Visual Feedback:** ทุกการกระทำของผู้เล่น (หยิบของ, วางของ, หั่น, ทอด, เสิร์ฟ, ฉีดดับเพลิง, ลื่นไถล) จะต้องมีการตอบสนองทางภาพและเสียงทันที
2. **Decoupled Architecture:** ระบบการแสดงผลทางสายตา (Visuals), เสียง (Audio) และหน้าจอ (UI) จะต้องไม่ผูกติดกับตรรกะของเกม (Game Logic) โดยสื่อสารกันผ่าน **C# Events** เท่านั้น
3. **Emergent Gameplay & Chaos:** เพิ่มความสนุกและเสียงหัวเราะด้วยเหตุการณ์ไม่คาดฝัน (ไฟไหม้, พื้นลื่น, ชั่วโมงเร่งด่วน, แมวขโมยวัตถุดิบ, เคาน์เตอร์เลื่อน 2 ตัว) ที่ท้าทายการวางแผนและการสื่อสารของผู้เล่น
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
│ • FireExtinguish │    │ • Dual Moving    │             │ • Pothole Trap   │    │ • Beach Raft     │
│ • Rush Hour (2x) │    │   Counters (X,Z) │             │   (Tripping)     │    │   (Ocean Tilt)   │
└──────────────────┘    └──────────────────┘             └──────────────────┘    └──────────────────┘
```

### 2.1 ระบบไฟไหม้เตาและถังดับเพลิง (Fire Hazard & Fire Extinguisher)
- **สเตตัสไฟไหม้ (`FireHazard.cs`):** เมื่อเตาอบทอดเนื้อจนไหม้ (`State.Burned`) เตาจะติดไฟ (`Ignite()`) พ่นเปลวไฟและควันดำออกมา ผู้เล่นจะไม่สามารถหยิบจับอาหารบนเตาได้จนกว่าจะดับไฟสำเร็จ
- **ถังดับเพลิง (`FireExtinguisher.cs`):** วัตถุเครื่องมือ 3D สมบูรณ์แบบ (ตัวถังแดง, หัวฉีดเทา, ปลายท่อดำ) พ่นละอองโฟมดับเพลิงใส่เตา (`StartSpraying()`) ผู้เล่นกด `[F]` ค้างเพื่อพ่นละออง และกด `[E]` เพื่อทิ้งถังลงพื้นอย่างปลอดภัย

### 2.2 ระบบเคาน์เตอร์เลื่อนตำแหน่งคู่ (Dual Moving Counters)
- **`MovingCounter.cs` & `GameplayEventsBootstrap.cs`:**
  - **ตัวที่ 1:** เลื่อนตามแนวนอน (ซ้าย-ขวา `X: 2.2m`, ความเร็ว `1.6f`)
  - **ตัวที่ 2:** เลื่อนตามแนวลึก (หน้า-หลัง `Z: 1.8m`, ความเร็ว `1.4f`, หน่วงเวลา `0.7s`)
  - วัตถุดิบและจานที่วางอยู่ด้านบนจะเคลื่อนที่ตามตำแหน่งเคาน์เตอร์แบบเรียลไทม์

### 2.3 ระบบพื้นลื่นคราบน้ำมัน (Slippery Floor)
- **`SlipperyFloor.cs`:** คราบน้ำมัน 4 จุดบนพื้นทางเดิน เมื่อเหยียบจะเกิดแรงเฉื่อยไถลไปข้างหน้า (`slipMomentum`) พร้อมหมุนตัวเคว้งคว้าง (`spinSpeed = 360°/s`)

### 2.4 ระบบชั่วโมงเร่งด่วน (Rush Hour Event)
- **`RushHourManager.cs`:** เมื่อเวลาแข่งขันผ่านไป 50% ระบบจะเข้าสู่โหมดชั่วโมงเร่งด่วน 20 วินาที ออเดอร์จะเข้ามาเร็วขึ้น 2 เท่า และจานที่ส่งสำเร็จจะได้รับคะแนน 2 เท่า

### 2.5 ระบบแมวป่วนครัว (Kitchen Cat NPCs)
- **`KitchenCatNPC.cs` & `CatProceduralAnimator.cs`:** แมวส้มและแมวเทา 3D ย่องเข้ามาในครัว เล็งขโมยอาหารบนเคาน์เตอร์ และอาจแอบขโมยถังดับเพลิงหากวางไว้บนเคาน์เตอร์ เชฟต้องเดินไปไล่ให้แมวตกใจหนี

---

## 🍳 3. การออกแบบระบบเคาน์เตอร์และ Finite State Machine

### 3.1 ระบบเคาน์เตอร์เตาอบ (Stove Counter State Machine)
`StoveCounter` ใช้ Finite State Machine ควบคุมลำดับการปรุงอาหาร 4 สเตต:

```text
               วางเนื้อดิบ (MeatPatty)
  [ IDLE ] ─────────────────────────────► [ FRYING ]
                                                │
                                                │ fryingTimer >= 4s
                                                ▼
  [ BURNED ] ◄─────────────────────────── [ FRIED ]
  (Fire Hazard)  burningTimer >= 3s (เนื้อสุกพร้อมเสิร์ฟ)
```

- **`State.Idle`:** เตาว่างเปล่า ไม่มีเสียงฉ่า ไม่มีควัน
- **`State.Frying`:** กำลังทอด วัตถุดิบดิบเปลี่ยนเป็นสุก แสดงหลอด Progress Bar สีเหลือง
- **`State.Fried`:** อาหารสุกแล้ว พร้อมหยิบขึ้นจาน หากทิ้งไว้เกิน 3 วินาที (`burningTimerMax = 3s`) เนื้อจะไหม้
- **`State.Burned`:** อาหารไหม้เกรียม และจุดติดไฟ `FireHazard.Ignite()` บังคับให้ต้องใช้ถังดับเพลิงมาฉีดดับก่อนหยิบของทิ้ง

---

## 🖥️ 4. สถาปัตยกรรมระบบอินเทอร์เฟซผู้ใช้ (UI Architecture)

1. **Screen-Space Canvas HUD:**
   - [`GameStartCountdownUI.cs`](file:///d:/unity/My%20project/Assets/Scripts/UI/GameStartCountdownUI.cs): หน้าต่างสอนเล่น 15 วินาที พร้อมกล่องเตือนเด่นชัดสีส้ม (`#FFA500`)
   - [`DeliveryManagerUI.cs`](file:///d:/unity/My%20project/Assets/Scripts/UI/DeliveryManagerUI.cs): แสดงรายการการ์ดออเดอร์
   - [`RushHourUI.cs`](file:///d:/unity/My%20project/Assets/Scripts/UI/RushHourUI.cs): แสดงป้ายแจ้งเตือนชั่วโมงเร่งด่วน
   - [`GamePlayingClockUI.cs`](file:///d:/unity/My%20project/Assets/Scripts/UI/GamePlayingClockUI.cs): นาฬิกาจับเวลากลมถอยหลัง
   - [`GamePauseUI.cs`](file:///d:/unity/My%20project/Assets/Scripts/UI/GamePauseUI.cs): เมนูหยุดเกม (Resume, Main Menu พร้อม auto-focus และ reset timeScale)
   - [`GameOverUI.cs`](file:///d:/unity/My%20project/Assets/Scripts/UI/GameOverUI.cs): สรุปยอดจานอาหารที่ส่งสำเร็จ และกลับสู่หน้าเมนูหลักอัตโนมัติใน 5 วินาที
2. **World-Space Canvas:**
   - [`ProgressBarUI.cs`](file:///d:/unity/My%20project/Assets/Scripts/UI/ProgressBarUI.cs): หลอดแสดงความคืบหน้าลอยเหนือเคาน์เตอร์
   - [`PlateIconsUI.cs`](file:///d:/unity/My%20project/Assets/Scripts/UI/PlateIconsUI.cs): แสดงไอคอนวัตถุดิบบนจาน
   - [`FireExtinguisher.cs`](file:///d:/unity/My%20project/Assets/Scripts/Obstacles/FireExtinguisher.cs): ป้ายคำสั่งลอยแบบ Billboard (`[E] PICK UP`, `[F] HOLD TO SPRAY`, `[E] DROP`)
   - [`LookAtCamera.cs`](file:///d:/unity/My%20project/Assets/Scripts/LookAtCamera.cs): หมุนระนาบ Canvas เข้าหากล้องตลอดเวลา (Billboard)

---

## 🎨 5. โทนสีและสไตล์การออกแบบ (Visual Palette)

- **Selected Highlight Color:** สีขาวเรืองแสง (`#FFFFFF`) บน `SelectedCounterVisual`
- **Tutorial Warning Color:** สีส้มเด่นชัด (`#FFA500` / `#FFD700`) สำหรับกล่องเตือนถังดับเพลิง
- **Progress Normal Color:** สีเขียวมะนาว/เหลือง (`#84CC16` / `#EAB308`) สำหรับหลอดหั่นและทอด
- **Fire & Hazard Color:** สีส้มแดงเพลิง (`#F97316` / `#EF4444`) สำหรับเปลวไฟและป้าย Rush Hour
- **Delivery Success Color:** สีเขียวมรกต (`#10B981`) แจ้งเตือนเมื่อส่งอาหารสำเร็จ
- **Delivery Failed Color:** สีแดงกุหลาบ (`#F43F5E`) แจ้งเตือนเมื่อส่งอาหารผิดสูตร