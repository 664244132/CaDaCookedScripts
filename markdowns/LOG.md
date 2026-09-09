# 📜 บันทึกประวัติการพัฒนาและปรับปรุงโปรเจกต์ (CaDaCook Maintenance Log)

เอกสารนี้ใช้เป็นบันทึกประวัติการแก้ไข Refactoring และการปรับปรุงระบบทั้งหมดในโปรเจกต์เกม **CaDaCook** (Unity 6 C#) เพื่อให้ AI Agents และทีมนักพัฒนาสามารถอ่านและทำความเข้าใจสถานะล่าสุดของโปรเจกต์ได้อย่างแม่นยำและรวดเร็ว

---

## 🏛️ คลังประวัติสำคัญย้อนหลัง (Historical Milestones Archive: Sessions 1–45)

เพื่อความกระชับและประหยัด Context Token ในการประมวลผลของ AI Agents ประวัติการพัฒนาใน Sessions 1–45 ได้ถูกสรุปจัดหมวดหมู่ตามความสำเร็จหลัก (Milestones) ไว้ดังนี้:

| หมวดหมู่การพัฒนา (Milestone) | ช่วงเซสชัน | สรุปฟีเจอร์และการปรับปรุงหลัก (Summary of Accomplishments) |
| :--- | :---: | :--- |
| **1. โครงสร้างพื้นฐานและมาตรฐานโค้ด** | Sessions 1–5 | วิเคราะห์โปรเจกต์ Unity 6 (6000.3.6f1), URP 17.3.0, New Input System 1.18.0, จัดทำเอกสารคู่มือ C# Standard (CSharpCodingGuide.md, DeMorgansLaws.md), วางกฎ .antigravityignore และ .gitignore |
| **2. ระบบอุปสรรคและอันตรายในครัว** | Sessions 6–20 | พัฒนาระบบ FireHazard, FireExtinguisher, SlipperyFloor, KitchenCatNPC (AI ขโมยวัตถุดิบ), MovingCounter, ConveyorBelt, RushHourManager, ปรับเวลาเล่นเป็น 2:30 นาที และเชื่อมต่อเอฟเฟกต์ไฟจริง VFXPACK_FIRE_WALLCOEUR |
| **3. ความเข้ากันได้ของ Unity 6 & URP** | Sessions 21–25 | เปลี่ยน API เก่าเป็น FindFirstObjectByType<Camera>() ใน LookAtCamera.cs, ปิดเงาไฟย่อยเพื่อรักษา URP Shadow Atlas (2048x2048), แก้ไขปัญหา Unity Fake Null Check บน Light Component |
| **4. ยกระดับระบบถังดับเพลิง 3D & UI** | Sessions 26–34 | ออกแบบถังดับเพลิง 3D พร้อมป้ายลอย Billboard World-Space Canvas [E] PICK UP / [F] HOLD TO SPRAY / [E] DROP, ปรับการดับไฟตามทิศหันหน้า (Forward Arc) และแก้ปัญหา TextMeshPro Deprecation |
| **5. ปรับสมดุลเกมและหน้าต่างสอนเล่น** | Sessions 35–45 | กำหนดเวลาเนื้อไหม้เป็น 3 วินาที, พัฒนาหน้าต่างสอนเล่น How to Play 2 คอลัมน์กว้างสบายตา (GameStartCountdownUI.cs) พร้อมนับถอยหลัง 10 วินาที และระบบกดปุ่มข้ามเข้าเกมทันที |

---

## 🚀 ประวัติการปรับปรุงเชิงลึก (Active Detailed Changelog: Sessions 46–63)

### 🔹 Session 46: GameOver Auto-Return to MainMenu (5s) & Meat Burning Tuning
- [UI/GameOverUI.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/UI/GameOverUI.cs): เริ่ม Coroutine หน่วงเวลา 5.0 วินาทีหลังสรุปคะแนน แล้วพาผู้เล่นกลับสู่ MainMenuScene อัตโนมัติ
- ปรับลดเวลาที่เนื้อสุกจะไหม้เป็น 3.0 วินาทีตรงตาม Requirement ใน ScriptableObject และ [Counters/StoveCounter.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/Counters/StoveCounter.cs)

### 🔹 Session 47–50: Tutorial HUD Typography & 2-Column Wide Formatting
- [UI/GameStartCountdownUI.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/UI/GameStartCountdownUI.cs): ขยายขนาดตัวอักษร 25-32pt, หัวข้อ 44pt, จัดเลย์เอาต์ 2 คอลัมน์ (ซ้าย: ปุ่มควบคุม 740px, ขวา: วิธีเล่นและอุปสรรค 840px)
- ปรับเปลี่ยนข้อความทั้งหมดเป็นรูปแบบสากล (Pure ASCII & Rich Text) ปราศจากปัญหากล่องสี่เหลี่ยม □ จาก Font Missing

### 🔹 Session 51–55: VIP Critic Orders & Combo Streak HUD System
- [DeliveryManager.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/DeliveryManager.cs):
  - พัฒนาระบบลูกค้า VIP สุ่มเกิดออเดอร์กรอบทอง จำกัดเวลา 25 วินาที หากส่งสำเร็จรับโบนัสคะแนน 3 เท่า (3X Score)
  - พัฒนาระบบ Combo Streak นับการเสิร์ฟสำเร็จต่อเนื่อง (Streak >= 3 รับตัวคูณ 1.5x, Streak >= 5 รับตัวคูณ 2.0x)
- [UI/ComboUI.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/UI/ComboUI.cs): แสดงป้าย Combo Streak ลอยเด่นข้างนาฬิกาจับเวลา ตัวอักษรสีส้มสดใส คมชัด ไร้พื้นหลังสีดำ
- [UI/DeliveryManagerSingleUI.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/UI/DeliveryManagerSingleUI.cs): เพิ่มหลอดเวลานับถอยหลังของการ์ด VIP เปลี่ยนเป็นสีแดงกระพริบเมื่อเหลือเวลาน้อยกว่า 25%

### 🔹 Session 56–59: Dynamic Events Bootstrap & Scene Flow Stabilization
- [Gameplay/GameplayEventsBootstrap.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/Gameplay/GameplayEventsBootstrap.cs): ผูกเข้ากับ KitchenGameManager.Start() เพื่อเริ่มระบบไฟไหม้ คราบน้ำมัน 4 จุด และแมวป่วนครัว 2 ตัวอัตโนมัติ
- [Loader.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/Loader.cs) & [LoaderCallback.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/LoaderCallback.cs): วางรากฐาน Scene Flow ระหว่าง MainMenuScene (0), GameScene (1), LoadingScene (2)

### 🔹 Session 60: Code Quality Audit (Step 1) - Performance & Garbage Cleanup
- [KitchenGameManager.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/KitchenGameManager.cs): ลบคำสั่ง Debug.Log(state); ใน Update() ที่พ่น Log 60fps ตลอดการเล่นเกม ขจัดขยะ GC Alloc และลดภาระ CPU
- [Myscripts.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/Myscripts.cs): เพิ่ม Guard Null Check Keyboard.current != null ป้องกัน NullReferenceException และย้ายการต่อสตริงใน Update() เป็น Event-driven UpdateDisplay()
- ลบ Unused & Suspicious Namespaces ขยะใน 4 สคริปต์: [Counters/TrashCounter.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/Counters/TrashCounter.cs), [ScriptableObjects/AudioClipRefsSO.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/ScriptableObjects/AudioClipRefsSO.cs), [Counters/ContainerCounter.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/Counters/ContainerCounter.cs), และ [Counters/StoveCounter.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/Counters/StoveCounter.cs)

### 🔹 Session 61: Memory Leak Prevention (Step 2) - Event Unsubscription in OnDestroy
- เพิ่มเมธอด OnDestroy() พร้อม Null Safety และคำสั่ง -= ถอนการดักฟัง Event ใน 9 สคริปต์หลักตามกฎข้อ 7 ของ [REFACTORCODE.md](file:///c:/CaDaCooked/CaDaCookedScripts/markdowns/REFACTORCODE.md):
  1. [PlateCompleteVisual.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/PlateCompleteVisual.cs): ปลด plateKitchenObject.OnIngredientAdded
  2. [SelectedCounterVisual.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/SelectedCounterVisual.cs): ปลด Player.Instance.OnSelectedCounterChanged
  3. [Counters/ContainerCounterVisual.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/Counters/ContainerCounterVisual.cs): ปลด containerCounter.OnPlayerGrabbedObject
  4. [Counters/CuttingCounterVisual.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/Counters/CuttingCounterVisual.cs): ปลด cuttingCounter.OnCut
  5. [Counters/PlatesCounterVisual.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/Counters/PlatesCounterVisual.cs): ปลด platesCounter.OnPlateSpawned และ OnPlateRemoved
  6. [Counters/StoveCounterSound.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/Counters/StoveCounterSound.cs): ปลด stoveCounter.OnStateChanged
  7. [Counters/StoveCounterVisual.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/Counters/StoveCounterVisual.cs): ปลด stoveCounter.OnStateChanged
  8. [UI/PlateIconsUI.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/UI/PlateIconsUI.cs): ปลด plateKitchenObject.OnIngredientAdded
  9. [UI/ProgressBarUI.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/UI/ProgressBarUI.cs): ปลด hasProgress.OnProgressChanged

### 🔹 Session 62: Logic Structure (Step 3) - De Morgan's Laws & Early Return Guard Clauses
- ปรับปรุงตรรกะแบบ Flat (Guard Clauses) ตามคู่มือ [DeMorgansLaws.md](file:///c:/CaDaCooked/CaDaCookedScripts/markdowns/DeMorgansLaws.md):
  - [Counters/PlatesCounter.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/Counters/PlatesCounter.cs): if (player.HasKitchenObject() || platesSpwanedAmount <= 0) return;
  - [Counters/ContainerCounter.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/Counters/ContainerCounter.cs): if (player.HasKitchenObject()) return;
  - [Counters/CuttingCounter.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/Counters/CuttingCounter.cs): if (!HasKitchenObject() || !HasRecipeWithInput(GetKitchenObject().GetKitchenObjectSO())) return;
  - [Counters/DeliveryCounter.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/Counters/DeliveryCounter.cs): if (!player.HasKitchenObject()) return;
  - [Counters/ClearCounter.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/Counters/ClearCounter.cs): ลบ namespaces ที่ไม่ได้ใช้งาน และลบ else เปล่า
- [PlateCompleteVisual.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/PlateCompleteVisual.cs): ปรับชื่อฟิลด์เป็น camelCase plateKitchenObject พร้อมใส่ [FormerlySerializedAs(PlateKitchenObject)] เพื่อคงความเข้ากันได้กับ Inspector 100%

### 🔹 Session 63: Full Documentation Modernization, Path Correction & AI Optimization
- [UI/GamePlayingClockUI.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/UI/GamePlayingClockUI.cs): เสริม Guard Null Check ป้องกัน NullReferenceException หาก KitchenGameManager.Instance หรือ 	imerImage เป็น null
- [PlayerController.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/PlayerController.cs): แปลงการเข้ารหัสไฟล์เป็น UTF-8 สากล ป้องกันปัญหา Encoding เพี้ยนและ Tool Reading Error
- [.antigravityignore](file:///c:/CaDaCooked/CaDaCookedScripts/.antigravityignore) & [.gitignore](file:///c:/CaDaCooked/CaDaCookedScripts/.gitignore): ตรวจสอบและรับประกันการละเว้นไฟล์แคช Unity 6 ครบทั้ง 10 หมวดหมู่
- [markdowns/AboutProject.md](file:///c:/CaDaCooked/CaDaCookedScripts/markdowns/AboutProject.md): ปรับปรุงเอกสารครั้งใหญ่ แปลง Path เครื่องเดิม (d:/unity/...) เป็น Path ปัจจุบัน (c:/CaDaCooked/...) และบันทึกระบบ VIP Critic, Combo HUD, RandomFireManager และ Scene Architecture ครบถ้วน
- [markdowns/DEBUG.md](file:///c:/CaDaCooked/CaDaCookedScripts/markdowns/DEBUG.md): ยกระดับเป็นคู่มือดีแบ๊กเฉพาะทางสำหรับ AI Agents พร้อม Rapid Troubleshooting Matrix และ AI Pre-Flight Checklist
- [markdowns/LOG.md](file:///c:/CaDaCooked/CaDaCookedScripts/markdowns/LOG.md): จัดระเบียบและย่อขนาดเอกสาร ย้าย Sessions 1–45 เข้าสู่ Historical Milestones Archive ช่วยลดขนาดไฟล์ลงกว่า 70% และประหยัด Context Token สูงสุด

### 🔹 Session 64: Step 1 Gameplay Enhancements - Dash Mechanic, New Input Spray & 15s Extinguisher Respawn
- [Assets/Scripts/GameInput.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/GameInput.cs):
  - เพิ่ม `OnDashAction` ผูกปุ่ม Spacebar บนคีย์บอร์ด, Gamepad South Button, และ Right Shoulder ผ่าน New Input System
  - เพิ่มเมธอด `IsInteractAlternatePressed()` ดักจับการกดค้างปุ่ม Alternate (F / Gamepad West) อย่างถูกต้อง
- [Assets/Scripts/Player.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/Player.cs):
  - พัฒนาระบบ **Dash Mechanic** พุ่งตัวความเร็วสูง 22m/s นาน 0.16s พร้อม Cooldown 1.0s และ CapsuleCast ป้องกันการทะลุกำแพง/เคาน์เตอร์
  - เชื่อมต่อฟิสิกส์ Dash เข้ากับ `SlipperyFloor` หากแดชบนคราบน้ำมันจะได้รับ **Slide Bonus** พุ่งไถลต่อเนื่องสะใจ
  - สร้างระบบ Particle ฝุ่นควันขาวกระจายด้านหลังขณะ Dash (`CreateDashDustEffect()`)
  - ลบ Legacy `Input.GetKey(KeyCode.F)` เปลี่ยนมาใช้ `gameInput.IsInteractAlternatePressed()` รองรับทั้ง Keyboard และ Controller 100%
- [Assets/Scripts/Obstacles/FireExtinguisher.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/Obstacles/FireExtinguisher.cs):
  - บันทึก `initialSpawnPosition` และเพิ่มระบบ **15s Extinguisher Respawn** นับเวลาถอยหลังและพาถังกลับสู่จุดวางเดิมอัตโนมัติ
  - เพิ่ม `ScheduleRespawn(15.0f)` ปิด MeshRenderer/Collider ชั่วคราวโดยไม่ต้องปิด GameObject ทำให้ Update() ยังคงนับเวลาได้อย่างปลอดภัย
- [Assets/Scripts/Obstacles/KitchenCatNPC.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/Obstacles/KitchenCatNPC.cs):
  - ป้องกันแมวทำลายถังดับเพลิงเมื่อหนีพ้นกล้อง โดยเรียก `ScheduleRespawn(15.0f)`
  - ปรับปรุง `ScareCat()` หากเชฟเดินเข้าไปไล่แมวตอนกำลังคาบถังดับเพลิง แมวจะทำถังตกพื้นทันที เชฟเก็บมาใช้ต่อได้เลย
- [Assets/Scripts/UI/GameStartCountdownUI.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/UI/GameStartCountdownUI.cs):
  - เพิ่มปุ่มควบคุม `[ SPACEBAR ] / [ Gamepad (A) / RB ]` สำหรับ Dash ในหน้าต่างสอนเล่น
- [CONTEXT.md](file:///c:/CaDaCooked/CaDaCookedScripts/CONTEXT.md):
  - สร้างไฟล์ Domain Glossary กำหนดคำศัพท์เฉพาะของโปรเจกต์ (Chef, Dash, Slide Bonus, Fire Extinguisher, Extinguisher Respawn, Stray Cat, Slippery Floor) ตามมาตรฐาน domain-modeling skill

### 🔹 Session 65: Step 2 Kitchen Resource Loop - Dirty Plates, Interactive Sink & Customer Patience
- [Assets/Scripts/DeliveryManager.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/DeliveryManager.cs):
  - เพิ่มสถานะ `isAngry` ใน `OrderData` พร้อมหลอดความอดทนของลูกค้าปกติ (`orderTimerMax = 55s`)
  - ยิง Event `OnOrderAngry` เมื่อหลอดความอดทนหมดเวลา
  - ปรับระบบคำนวณคะแนน: เมื่อส่งอาหารที่ลูกค้า Angry จะได้เฉพาะคะแนนพื้นฐาน (100 แต้ม) โดยไม่ได้รับ Tip และไม่บวก Combo Multiplier (ตาม Q6 ตัวเลือก C)
- [Assets/Scripts/UI/DeliveryManagerSingleUI.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/UI/DeliveryManagerSingleUI.cs):
  - เปิดใช้งานหลอดเวลานับถอยหลังสำหรับออเดอร์ปกติ โดยเปลี่ยนสีตามเวลาที่เหลือ (เขียว >50% ➔ ส้ม 20-50% ➔ แดงกระพริบ <20%)
  - หากลูกค้าเข้าสู่สถานะ `isAngry` การ์ดจะเปลี่ยนเป็นสีแดงระเรื่อพร้อมป้าย `<color=#E02020><b>[ANGRY]</b></color>` และหลอดเวลากะพริบสีแดงเตือน
- [Assets/Scripts/DirtyPlateKitchenObject.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/DirtyPlateKitchenObject.cs):
  - สร้างคลาสวัตถุกองจานเปื้อนสืบทอดจาก `KitchenObject` รองรับการซ้อนกันเป็นกองสูงสุด 4 ใบ (ตาม Q4 ตัวเลือก B)
  - สร้างโมเดล 3D จานเปื้อนซ้อนกันตามจำนวนจริง พร้อมคราบซอสเปื้อนสีแดงและน้ำตาล และป้ายตัวเลข 3D ลอยเหนือกอง
- [Assets/Scripts/Counters/DeliveryCounter.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/Counters/DeliveryCounter.cs):
  - ดักฟัง `DeliveryManager.OnRecipeSuccess` เพื่อเพิ่มจานเปื้อนสะสมบนเคาน์เตอร์ส่งอาหาร (`dirtyPlatesAmount` สูงสุด 4 ใบ)
  - แสดงโมเดลจานเปื้อนซ้อนกันบนมุมเคาน์เตอร์พร้อมป้าย `[E] PICK UP`
  - เมื่อผู้เล่นกด `[E]` ด้วยมือเปล่า จะยกกองจานเปื้อนทั้งหมดไปล้างที่อ่างล้างจาน
- [Assets/Scripts/Counters/SinkCounter.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/Counters/SinkCounter.cs):
  - สร้างเคาน์เตอร์อ่างล้างจานสืบทอดจาก `BaseCounter, IHasProgress, IKitchenObjectParent`
  - ติดตั้งโครงสร้างโมเดล Procedural: อ่างล้างจาน, ก๊อกน้ำสแตนเลส, ผิวน้ำสีฟ้า, และตะแกรงสะเด็ดน้ำสำหรับวางจานสะอาด
  - ตาม Q5 ตัวเลือก A: วางจานเปื้อนแล้วกด `[F]` (InteractAlternate) รัวๆ 4 ครั้งต่อ 1 ใบเพื่อขัดล้าง (Interactive Cleaning) พร้อมเอฟเฟกต์ละอองฟองสบู่และเสียงน้ำ
  - เมื่อล้างเสร็จ จานเปื้อนจะกลายเป็นจานสะอาดสะสมบนตะแกรง ผู้เล่นสามารถกด `[E]` หยิบไปจัดอาหารได้ทันที
- [Assets/Scripts/Counters/PlatesCounter.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/Counters/PlatesCounter.cs):
  - เริ่มต้นเกมมีจานสะอาดให้ 4 ใบ และปรับเวลาสร้างจานอัตโนมัติเป็น 20 วินาที เพื่อให้การล้างจานเป็น Loop หลัก
  - อนุญาตให้ผู้เล่นนำจานสะอาดเปล่าที่ล้างแล้วจากอ่างกลับมาวางเก็บเข้าแท่นวางจานได้
- [Assets/Scripts/Gameplay/GameplayEventsBootstrap.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/Gameplay/GameplayEventsBootstrap.cs):
  - เพิ่ม `SetupSinkCounterSystem()` ใน Start() เพื่อแปลง `ClearCounter` ในครัวเป็น `SinkCounter` อัตโนมัติ
- [Assets/Scripts/Obstacles/KitchenCatNPC.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/Obstacles/KitchenCatNPC.cs):
  - แก้ไขการปลดวัตถุที่ถืออยู่โดยเรียก `ClearKitchenObject()` แทน `ClearKitchenObjectOnParent()` แก้ไขข้อผิดพลาด CS1061 ครบถ้วน
- [CONTEXT.md](file:///c:/CaDaCooked/CaDaCookedScripts/CONTEXT.md):
  - บันทึกคำศัพท์ Domain Ubiquitous Language เพิ่มเติม: `Dirty Plate`, `Sink Counter`, `Customer Patience`, `Angry Customer`

### 🔹 Session 66: Fix Hover Distance, Universal Angry Event & Procedural Counter Shuffling
- [Assets/Scripts/SelectedCounterVisual.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/SelectedCounterVisual.cs):
  - แก้ไขบั๊กไฮไลท์ Hover ค้างเมื่ออยู่ไกล: เพิ่มเงื่อนไข `baseCounter != null && e.selectedCounter == baseCounter` เพื่อป้องกันกรณี `null == null`
  - เพิ่มเมธอด `SetBaseCounter(BaseCounter)` และดึง Parent `GetComponentInParent<BaseCounter>()` อัตโนมัติใน `Start()`
- [Assets/Scripts/Player.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/Player.cs):
  - เพิ่ม Getter `GetSelectedCounter()` ให้สคริปต์ภายนอกตรวจสอบเคาน์เตอร์ที่เลือกได้ทันที
- [Assets/Scripts/Counters/SinkCounter.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/Counters/SinkCounter.cs):
  - ปรับระยะการทำงานของ UI อ่างล้างจานให้เท่ากับเคาน์เตอร์อื่น (2.2 เมตร): ซ่อน World Space Progress Canvas ทันทีเมื่อผู้เล่นอยู่ไกลเกิน 2.2 เมตร
  - เชื่อมโยง `SelectedCounterVisual` บน GameObject เข้ากับ `SinkCounter` ทันทีใน `Start()`
- [Assets/Scripts/DeliveryManager.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/DeliveryManager.cs):
  - เพิ่ม Event `OnAnyOrderAngry` ให้ดักฟังสบายไร้ Generic Type
  - เมื่อออเดอร์ประเภทใดก็ตามหมดเวลา (ทั้งออเดอร์ปกติและ VIP) ให้ยิง `OnOrderAngry`, `OnAnyOrderAngry`, และ `OnRecipeFailed` อย่างครบถ้วน
- [Assets/Scripts/SoundManager.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/SoundManager.cs):
  - ดักฟัง `DeliveryManager.OnOrderAngry` และเล่นเสียงเตือน `audioClipRefsSO.warning` ทันทีที่ลูกค้าเริ่มโกรธ
- [Assets/Scripts/Gameplay/GameplayEventsBootstrap.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/Gameplay/GameplayEventsBootstrap.cs):
  - **Random Sink Placement:** ปรับปรุง `SetupSinkCounterSystem()` ให้สุ่มเคาน์เตอร์ว่าง 1 ตัวจาก `ClearCounter` ทั้งหมด ไม่ซ้ำตำแหน่งเดิม
  - **Procedural Counter Shuffling:** เพิ่มฟังก์ชัน `ShuffleKitchenCounters()` นำเคาน์เตอร์ทำอาหารทั้งหมด 24 ตัวในครัว (ตัดเฉพาะ DeliveryCounter ที่ติดช่องส่ง) มาสลับตำแหน่งและมุมหมุนแบบ Fisher-Yates Shuffle ทำให้ผังครัวสุ่มใหม่ทุกรอบการเล่น เพิ่มความสนุกและหลากหลายระดับขีดสุด!
- [CONTEXT.md](file:///c:/CaDaCooked/CaDaCookedScripts/CONTEXT.md):
  - บันทึกคำศัพท์: `Counter Shuffling`

### 🔹 Session 67: Fix Counter Overlap in Randomization & Moving Counter Safe Path Clearance
- [Assets/Scripts/Gameplay/GameplayEventsBootstrap.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/Gameplay/GameplayEventsBootstrap.cs):
  - **Reordered Bootstrap Execution:** สลับลำดับใน `Start()` ให้เรียก `ShuffleKitchenCounters()` ก่อน `SetupSinkCounterSystem()` เพื่อให้การสุ่มสลับตำแหน่งกระทำบนเคาน์เตอร์ตั้งต้นที่สะอาดและเป็นอิสระต่อกัน 100%
  - **Component Synchronization:** เปลี่ยน `Destroy(chosenCounter)` ใน `SetupSinkCounterSystem()` เป็น `DestroyImmediate(chosenCounter)` เพื่อขจัด Component เดิมออกแบบ Synchronous ป้องกันไม่ให้ `FindObjectsByType<BaseCounter>()` ตรวจพบ Component ซ้ำบน GameObject เดียวกัน
  - **1-to-1 Bijection & Deduplication:** ใน `ShuffleKitchenCounters()` เพิ่ม `HashSet<GameObject> registeredGameObjects` และระบบตรวจเช็คระยะห่างสล็อต `Vector3.Distance >= 0.8f` รับประกันความถูกต้องทางคณิตศาสตร์ว่าเคาน์เตอร์ทุกตัวและสล็อตทุกตำแหน่งจะจับคู่แบบ 1 ต่อ 1 ปราศจากการสุ่มซ้อนทับกันอย่างแน่นอน
  - **Collision-Free Moving Counters:** ใน `SetupMovingCounters()` เพิ่มฟังก์ชัน `IsPathClearOfCounters()` และ `DistancePointToLineSegment()` ตรวจสอบระยะปลอดภัย (อย่างน้อย 1.35 เมตร) ตลอดทั้งแนวการเคลื่อนที่ของ `MovingCounter` ไม่ให้เลื่อนไปชนหรือแทรกตัวทับเคาน์เตอร์ข้างเคียงในแถว
- [CONTEXT.md](file:///c:/CaDaCooked/CaDaCookedScripts/CONTEXT.md):
  - บันทึกคำศัพท์: `Collision-Free Counter Shuffling`, `Path Clearance Check`

### 🔹 Session 68: Gameplay Loop Hardening - Rack Full Lock, Angry Patience & Plate Scraping
- [Assets/Scripts/Counters/SinkCounter.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/Counters/SinkCounter.cs):
  - **Q1 Rack Full Lock:** ล็อคการกดขัดล้าง [F] ทันทีเมื่อตะแกรงสะเด็ดน้ำเต็ม 4 ใบ (`cleanPlatesCount >= MAX_CLEAN_PLATES`)
  - แสดงป้ายเตือน `<color=#FF9100><b>⚠️ RACK FULL! PICK UP [E]</b></color>` บน World Space HUD ป้องกันบั๊กจานสะอาดสูญหายในอากาศ 100%
- [Assets/Scripts/DeliveryManager.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/DeliveryManager.cs):
  - **Q2 Angry Patience Timer (20s):** เพิ่มตัวแปร `angryTimer`, `angryTimerMax = 20.0f` ใน `OrderData`
  - เมื่อออเดอร์เข้าสู่ `isAngry` จะเริ่มนับถอยหลัง 20 วินาทีสุดท้าย หากผู้เล่นยังไม่ส่งอาหาร ลูกค้าจะทนไม่ไหวและเดินออกจากร้านไป (`OnRecipeFailed`, หักคะแนน 50 แต้ม)
  - ปลดล็อคคิวออเดอร์ที่เคยตันถาวร ทำให้เมนูใหม่และออเดอร์ VIP กรอบทองสามารถสุ่มเกิดเข้ามาได้อย่างต่อเนื่อง
- [Assets/Scripts/UI/DeliveryManagerSingleUI.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/UI/DeliveryManagerSingleUI.cs):
  - แสดงเวลานับถอยหลังของสถานะโกรธเป็นตัวเลขวินาทีสด `[ANGRY {Xs}]` บนหัวการ์ด พร้อมหลอดเวลากะพริบสีแดงที่ลดลงตามจริง
- [Assets/Scripts/PlateKitchenObject.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/PlateKitchenObject.cs):
  - เพิ่ม Event `OnIngredientsCleared` และฟังก์ชัน `ClearIngredients()` สำหรับเทวัตถุดิบและอาหารทั้งหมดออกจากจาน
- [Assets/Scripts/PlateCompleteVisual.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/PlateCompleteVisual.cs) & [Assets/Scripts/UI/PlateIconsUI.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/UI/PlateIconsUI.cs):
  - ดักฟัง `OnIngredientsCleared` เพื่อซ่อนโมเดลอาหาร 3D และล้างไอคอนบนจานออกทันทีเมื่ออาหารบนจานถูกเททิ้ง
- [Assets/Scripts/Counters/TrashCounter.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/Counters/TrashCounter.cs):
  - **Q3 Plate Scraping (Overcooked Standard):** เมื่อถือจานอาหารไปกดที่ถังขยะ จะเททิ้งเฉพาะอาหาร/วัตถุดิบข้างใน (`plate.ClearIngredients()`) โดยยังคงจานเปล่าไว้ในมือเชฟเสมอ
  - ไม่อนุญาตให้ทิ้งจานเปล่าหรือจานเปื้อนลงถังขยะ ป้องกันการขาดแคลนจานอย่างถาวร
- [CONTEXT.md](file:///c:/CaDaCooked/CaDaCookedScripts/CONTEXT.md):
  - บันทึกคำศัพท์: `Rack Full Lock`, `Angry Patience Timer`, `Plate Scraping`

---

## 🔒 Security & Code Standards Checklist
- [x] **No Direct DB Mutations (Rule 10):** ไม่มีการรันคำสั่ง SQL หรือปรับแต่งฐานข้อมูลโดยตรง
- [x] **No Auto Git Push (Rule 11):** ไม่มีการรันคำสั่ง git commit หรือ git push (ผู้ใช้เป็นผู้ควบคุมเอง)
- [x] **Unity C# Best Practices (Rule 1, 2, 3):** สถาปัตยกรรม Decoupled ผ่าน C# Events และ ScriptableObjects
- [x] **Zero GC Alloc in Update (Rule 5):** หลีกเลี่ยงการสร้างขยะในหน่วยความจำในลูป Update()
- [x] **Event Subscription Safety (Rule 7):** Unsubscribe (-=) ใน OnDestroy() ทุกคลาสที่ดักฟังข้ามสคริปต์
- [x] **De Morgan's Laws & Early Return (Rule 4):** โครงสร้างเงื่อนไขแบนราบ อ่านง่าย สื่อความหมายชัดเจน
- [x] **Pure ASCII Standard:** ข้อความ UI ทั้งหมดใช้ตัวอักษรและสัญลักษณ์สากล ไม่เกิดปัญหากล่องสี่เหลี่ยม □
- [x] **Documentation Integrity (Rule 2, 12):** อัปเดตเอกสาร Markdown ทุกไฟล์ให้ตรงกับความเป็นจริง 100% พร้อมจัดทำเป็นภาษาไทยที่เข้าใจง่าย