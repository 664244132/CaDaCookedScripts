# 🍳 CaDaCook - Complete Project Documentation

**CaDaCook** คือเกมแนวทำอาหารและบริหารจัดการครัว 3D Fast-paced Casual Simulation (สไตล์ Kitchen Chaos / Overcooked) ที่ถูกพัฒนาขึ้นด้วย **Unity 6 (6000.3.6f1)**, **Universal Render Pipeline (URP 17.3.0)**, **Unity New Input System (1.18.0)** และเขียนโปรแกรมด้วยภาษา **C#**

เอกสารฉบับนี้รวบรวมรายละเอียดสถาปัตยกรรมระบบ, โครงสร้างไฟล์ C#, ระบบเคาน์เตอร์ทำอาหาร, ระบบออเดอร์ VIP และคอมโบ, ระบบอุปสรรคและเหตุการณ์ไดนามิก (Dynamic Events & Obstacles), การเชื่อมต่อ Scene และผังโฟลเดอร์ของโปรเจกต์ทั้งหมด

---

## 🛠️ 1. สถาปัตยกรรมระบบและโครงสร้างทางเทคนิค (System Architecture)

ระบบของเกม CaDaCook ถูกออกแบบโดยยึดหลัก **Decoupled Architecture** ตามแนวทางใน [REFACTORCODE.md](file:///c:/CaDaCooked/CaDaCookedScripts/markdowns/REFACTORCODE.md) เพื่อให้การทำงานในแต่ละส่วนแยกออกจากกันอย่างชัดเจน ยืดหยุ่น และง่ายต่อการขยายฟีเจอร์

### 1.1 รูปแบบการออกแบบ (Design Patterns)
- **Observer Pattern via C# Events:** แยกส่วน Game Logic ออกจาก Visual Effects, Animations, UI และ Audio 100% (เช่น CuttingCounter ส่ง OnCut ➔ CuttingCounterVisual แสดงอนิเมชันมีดหั่น และ SoundManager เล่นเสียงมีด)
- **Singleton Pattern:** ใช้สำหรับคลาสศูนย์กลางของเกมที่มี Instance เดียวในฉาก ได้แก่ [KitchenGameManager](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/KitchenGameManager.cs), [DeliveryManager](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/DeliveryManager.cs), [GameInput](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/GameInput.cs), [SoundManager](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/SoundManager.cs), [RushHourManager](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/Gameplay/RushHourManager.cs), [ComboUI](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/UI/ComboUI.cs), และ [Player](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/Player.cs)
- **Data-Driven Architecture (ScriptableObjects):** นิยามข้อมูลวัตถุดิบและสูตรอาหารไว้ใน Assets เพื่อให้ Game Designer สามารถปรับแต่งค่าได้ทันทีผ่าน Inspector โดยไม่ต้องแก้ไขโค้ด C#
- **Interface Segregation:**
  - [IKitchenObjectParent](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/IKitchenObjectParent.cs): กำหนดพฤติกรรมการเป็นเจ้าของ/ถือ/ส่งต่อวัตถุดิบ (Player, BaseCounter, KitchenCatNPC)
  - [IHasProgress](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/IHasProgress.cs): กำหนดพฤติกรรมของเคาน์เตอร์ที่มีหลอดแสดงความคืบหน้า (CuttingCounter, StoveCounter)

### 1.2 สถาปัตยกรรมประสิทธิภาพสูงและการควบคุม Frame Pacing (Zero-GC & High Performance Architecture)
เพื่อให้เกมทำงานได้อย่างราบรื่น 60 FPS นิ่งสนิท ปราศจากอาการกระตุกหรือ Micro-stutter ระบบจึงได้รับการปรับปรุงประสิทธิภาพตามมาตรฐานระดับสูง:
- **Zero-GC AudioSourcePool (20 Channels):** ใน [SoundManager.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/SoundManager.cs) ปรับใช้ระบบ Object Pooling สร้าง AudioSource สำรองไว้ล่วงหน้า 20 แชนเนลเสียง เพื่อนำกลับมาใช้ซ้ำ (Recycle) แบบ Round-Robin ขจัดการเรียก `AudioSource.PlayClipAtPoint` ซึ่งเคยสร้าง/ทำลาย GameObject เสียง 10–20 ตัว/วินาที ขจัดขยะ Heap Allocations และอาการกระตุกจาก Garbage Collection (GC Spikes) ได้ 100%
- **60 FPS Target Lock & 60Hz Physics Timestep Sync:** ใน [KitchenGameManager.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/KitchenGameManager.cs) ล็อคเฟรมเรตคงที่ 60 FPS (`Application.targetFrameRate = 60`) พร้อมซิงค์รอบฟิสิกส์ `Time.fixedDeltaTime = 1f / 60f;` (~0.0166s) ให้ตรงกับรอบการเรนเดอร์ ขจัดอาการสั่นกระตุก (Micro-judder) ขณะตัวละครเดินและช่วยลดอุณหภูมิการทำงานของเครื่อง
- **Camera Transform Caching:** ใน [LookAtCamera.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/LookAtCamera.cs) แคช `targetCameraTransform` ตั้งแต่ `Start()` เพื่อขจัดการเรียกข้าม C++ Engine Property `Camera.main` ซ้ำซ้อนใน `LateUpdate()` ของ UI ลอยฟ้าทุกชิ้นในฉาก
- **Zero-GC EventArgs Notification:** ใน [CuttingCounter.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/Counters/CuttingCounter.cs) และ [SinkCounter.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/Counters/SinkCounter.cs) แคชอินสแตนซ์ `progressChangedEventArgs` และเรียกส่งผ่านฟังก์ชันกลาง `NotifyProgressChanged()` ขจัดการ `new IHasProgress.OnProgressChangedEventArgs` ในหน่วยความจำทุกจังหวะหั่นและขัดล้างจาน ตามกฎ Rule 5 และ 6 อย่างเคร่งครัด

---

## 🔄 2. สถาปัตยกรรมการเปลี่ยนฉาก (Scene Architecture & Flow)

โปรเจกต์ใช้ระบบจัดการเปลี่ยน Scene ผ่านคลาสกลาง [Loader.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/Loader.cs) ร่วมกับฉากคั่น [LoadingScene.unity](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scenes/LoadingScene.unity) และ [LoaderCallback.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/LoaderCallback.cs) เพื่อให้การโหลดและเปลี่ยนฉากเป็นไปอย่างราบรื่น

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
              └──► (จบเกม 10 วินาที พร้อมจัดอันดับดาวเชฟ ใน GameOverUI.cs) ──► [ LoadingScene ] ──► [ MainMenuScene ]
```

---

## 🍳 3. เจาะลึกระบบเกมเพลย์และสคริปต์หลัก (Core Game Systems)

### 3.1 ระบบตัวละครและการควบคุม (Player & Input)
- [Player.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/Player.cs): ควบคุมการเคลื่อนที่ 3 มิติ, การหมุนตัว, การ Raycast ตรวจจับเคาน์เตอร์ตรงหน้าด้วย `countersLayerMask`, การหยิบ-วางวัตถุดิบ, และระบบรองรับอุปสรรค:
  - **Dash Mechanic:** พุ่งตัวด้วยความเร็วสูง 22m/s นาน 0.16s (คูลดาวน์ 1.0s) พร้อมระบบ CapsuleCast ตรวจจับสิ่งกีดขวาง และละอองฝุ่นควันขาว (`CreateDashDustEffect()`) รองรับปุ่ม `[Spacebar]`, Gamepad South (A) และ Right Bumper (RB)
  - **Slide Bonus:** หากแดชบนคราบน้ำมัน `SlipperyFloor` จะได้รับแรงผลักสไลด์พุ่งตัวต่อเนื่องอย่างรวดเร็ว
  - **Fire Extinguisher Interaction:** หยิบ/วางถังดับเพลิงด้วย `[E]`, กดค้าง `[F]` (HOLD TO SPRAY) เพื่อพ่นละอองขาวดับไฟข้างหน้าผ่าน `gameInput.IsInteractAlternatePressed()`
- [PlayerAnimator.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/PlayerAnimator.cs): ส่งค่า IsWalking ไปยัง Animator Controller พร้อม Fallback Guard ป้องกัน NRE
- [PlayerSounds.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/PlayerSounds.cs): เล่นเสียงฝีเท้าตามจังหวะก้าวเดิน
- [GameInput.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/GameInput.cs): ตัวห่อหุ้ม New Input System (PlayerInputActions) กระจาย Action เป็น C# Events (`OnInteractAction`, `OnInteractAlternateAction`, `OnDashAction`, `OnPauseAction`)
- [PlayerInputActions.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/PlayerInputActions.cs): C# Wrapper ของไฟล์ Input Action Asset

### 3.2 ระบบเคาน์เตอร์ครัว (Modular Kitchen Counters)
เคาน์เตอร์ทุกชนิดสืบทอดมาจากคลาสฐาน [BaseCounter.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/Counters/BaseCounter.cs):
- **[ClearCounter.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/Counters/ClearCounter.cs):** เคาน์เตอร์ว่างสำหรับวางพักวัตถุดิบ หรือรวมวัตถุดิบลงบนจาน
- **[ContainerCounter.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/Counters/ContainerCounter.cs):** เคาน์เตอร์จ่ายวัตถุดิบดิบไม่จำกัด พร้อม [ContainerCounterVisual.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/Counters/ContainerCounterVisual.cs) แสดงอนิเมชันเปิด-ปิดฝาตู้
- **[CuttingCounter.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/Counters/CuttingCounter.cs):** เขียงหั่นอาหาร รองรับ IHasProgress และส่งอีเวนต์ OnCut ร่วมกับ [CuttingCounterVisual.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/Counters/CuttingCounterVisual.cs) มีระบบ Cutting Extrication Guard รีเซ็ตความคืบหน้าหากวัตถุดิบถูกขโมย
- **[StoveCounter.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/Counters/StoveCounter.cs):** กระทะทอด/เตาปรุงอาหาร มี Finite State Machine 4 สเตต (Idle, Frying, Fried, Burned) พร้อม Stove Extrication Recovery รีเซ็ตสถานะและเสียงฉ่าทันทีเมื่อวัตถุถูกเคลื่อนย้ายออกนอกลูป
- **[PlatesCounter.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/Counters/PlatesCounter.cs):** แท่นวางจานสะอาดเริ่มต้น 4 ใบ จำกัดเพดานสูงสุด 4 ใบ ตัดระบบเกิดจานอัตโนมัติออกเพื่อบังคับ Loop ล้างจานที่สมบูรณ์แบบ รองรับการนำกองจานสะอาดมาเติมคืน และแสดงป้าย 3D `⚠️ NO PLATES!` เมื่อจานหมด
- **[SinkCounter.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/Counters/SinkCounter.cs):** สเตชั่นล้างจานสแตนเลสเชิงพาณิชย์ (Commercial Stainless Steel Sink Station):
  - โครงสร้างสมจริง: หลุมอ่างลึก ซี่ตะแกรงสะเด็ดน้ำ ก๊อกน้ำสปริงคอห่าน ฟองสบู่ ป้ายชื่อ 3D `🧼 SINK STATION`
  - ตรรกะล้างจาน: วางจานเปื้อนลงในอ่าง แล้วกด `[F]` (InteractAlternate) 4 ครั้งต่อ 1 ใบเพื่อขัดล้าง
  - Rack Full Lock: ล็อคการล้างเมื่อตะแกรงสะเด็ดน้ำเต็ม 4 ใบ พร้อมเตือนให้ยกออก
  - Batch Retrieval: กด `[E]` เพื่อยกกองจานสะอาดทั้งหมด (1–4 ใบ) ไปเติมที่แท่นวางจานในเที่ยวเดียว
- **[DeliveryCounter.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/Counters/DeliveryCounter.cs):** เคาน์เตอร์ส่งอาหาร ตรวจสอบสูตรกับ DeliveryManager และสะสมจานเปื้อน (`dirtyPlatesAmount` สูงสุด 4 ใบ) ส่งคืนให้เชฟนำไปล้าง พร้อมระบบ Safe FireExtinguisher Handling หากผู้เล่นเผลอกดส่งถังดับเพลิง ระบบจะถือว่าส่งผิดสูตร (`DeliverIncorrectRecipe`) และสั่งให้ถังดับเพลิงเกิดใหม่ที่ตำแหน่งเดิมทันที (`ScheduleRespawn(1.0f)`) ป้องกันถังดับเพลิงสูญหาย
- **[TrashCounter.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/Counters/TrashCounter.cs):** ถังขยะสำหรับเททิ้งเฉพาะอาหารบนจาน (Plate Scraping) โดยไม่ทำลายจานเปล่าทิ้งเด็ดขาด พร้อมระบบ Safe FireExtinguisher Handling หากผู้เล่นนำถังดับเพลิงมาทิ้ง ระบบจะส่งถังกลับไปเกิดใหม่ที่ตำแหน่งเดิม (`ScheduleRespawn(1.0f)`) แทนที่จะถูกลบทิ้ง
- **[SelectedCounterVisual.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/SelectedCounterVisual.cs):** ขอบเรืองแสงสีขาวรอบเคาน์เตอร์ที่ผู้เล่นกำลังหันหน้าเข้าหา พร้อมระบบตรวจเช็ค Null ป้องกันไฮไลต์ค้าง

#### 3.3 ระบบออเดอร์, VIP Critic, Customer Patience & Combo Streak
- **[DeliveryManager.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/DeliveryManager.cs):** ตัวจัดการคิวออเดอร์ส่วนกลาง
  - **VIP Critic Orders:** มีโอกาสสุ่มเกิดออเดอร์ VIP กรอบสีทอง มีเวลานับถอยหลังจำกัด 25 วินาที หากส่งทันจะได้โบนัสคะแนน **3 เท่า (3X Score)** และต่อยอดคอมโบ
  - **Customer Patience & Angry State:**
    - ออเดอร์ปกติมีหลอดความอดทน 55 วินาที หากเวลาหมดจะเข้าสู่สถานะ **Angry** (การ์ดแดงกระพริบ + เสียง Warning)
    - เริ่มนับถอยหลัง 20 วินาทีสุดท้าย (`[ANGRY {Xs}]`) หากส่งทันในสถานะโกรธ จะได้เพียงคะแนนพื้นฐาน (100 แต้ม) โดยไม่ได้รับ Tip และไม่บวกตัวคูณคอมโบ
    - หากปล่อยให้หมดเวลา 20 วินาที ลูกค้าจะเดินออกจากร้าน (`OnRecipeFailed`, หัก 50 แต้ม) เพื่อปลดล็อคคิวให้ออเดอร์ใหม่เข้ามาได้
  - **Combo Streak System:** ส่งอาหารสำเร็จต่อเนื่องโดยไม่ผิดพลาดหรือปล่อยให้ออเดอร์หมดเวลา
    - Streak >= 3: ได้รับตัวคูณคะแนน 1.5x
    - Streak >= 5: ได้รับตัวคูณคะแนน 2.0x (Super Combo)
  - **Anti-Softlock Plate Conservation:** ไม่ว่าจะส่งออเดอร์ถูกหรือผิดสูตร เคาน์เตอร์ส่งจะคืนจานเปื้อนเข้าสู่ระบบเสมอ รับประกันจำนวนจานหมุนเวียนคงที่ 4 ใบ ปราศจากปัญหา Softlock
- **[ComboUI.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/UI/ComboUI.cs):** แสดงสถานะ Combo Streak ข้างนาฬิกาจับเวลา จัดรูปแบบผ่าน `UITheme.FormatSuperCombo()` และ `FormatStandardCombo()`
- **[DeliveryManagerUI.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/UI/DeliveryManagerUI.cs) & [DeliveryManagerSingleUI.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/UI/DeliveryManagerSingleUI.cs):** แสดงการ์ดออเดอร์ที่มุมบนซ้าย พร้อมหลอดเวลานับถอยหลังแบบไดนามิก (เขียว >50% ➔ ส้ม 20-50% ➔ แดงกระพริบ <20%) และป้ายสถานะ `[ANGRY {Xs}]`
- **[PlayerDeliveryResultUI.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/UI/PlayerDeliveryResultUI.cs):** ป๊อปอัปแจ้งผลการส่งอาหาร (Success สีเขียว / Failed สีแดง)

### 3.4 ระบบอุปสรรคและเหตุการณ์ไดนามิก (Dynamic Events & Obstacles)
- [GameplayEventsBootstrap.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/Gameplay/GameplayEventsBootstrap.cs): จุดเริ่มต้นระบบอุปสรรคทั้งหมดในครัวอัตโนมัติ:
  - **Procedural Counter Shuffling:** สลับตำแหน่งเคาน์เตอร์ 24 ตัวในครัวแบบ Fisher-Yates Shuffle พร้อมตรวจสอบความปลอดภัย ไม่ให้เคาน์เตอร์ทับซ้อนกัน
  - **Open Aisle Floor Patrol Setup:** จัดวางจุดเกิดและแนวลาดตระเวนของแมวทั้ง 2 ตัวบนพื้นทางเดินโล่งในครัว (แมวส้มขวา `(4.8f, 0f, 2.8f)` / แมวเทาซ้าย `(-4.8f, 0f, 2.8f)`) ปราศจากปัญหาการเกิดทับหรือจมในเคาน์เตอร์
- [FireHazard.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/Obstacles/FireHazard.cs): จำลองไฟลุกไหม้บนเคาน์เตอร์ พร้อมเอฟเฟกต์ไฟ/ควันดำสมจริง หากดับไม่ทันเคาน์เตอร์จะถูกล็อคกลายเป็นสีดำ 5 วินาที
- [FireExtinguisher.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/Obstacles/FireExtinguisher.cs): ถังดับเพลิง 3D พร้อมป้าย Billboard UI [E] PICK UP, [F] HOLD TO SPRAY, และ [E] DROP พร้อมระบบ **Indestructible Fail-Safe & 15s Idle Respawn** โดย Override `DestroySelf()` เพื่อสั่ง `ScheduleRespawn(1.0f)` เสมอ ทำให้ไม่สามารถถูกทำลายหรือสูญหายไปจากฉากได้ไม่ว่าจะถูกส่งที่เคาน์เตอร์ใด และดึงถังกลับจุดวางเดิมอัตโนมัติหากถูกทิ้งหรือตกหล่น
- [RandomFireManager.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/Gameplay/RandomFireManager.cs): ระบบสุ่มจุดเกิดไฟไหม้พร้อมกัน **2 แห่งทั่วครัว (Dual Simultaneous Fire Outbreak)** ทุกๆ 12-20 วินาที โดยสุ่มเลือก 2 เคาน์เตอร์ที่ไม่ซ้ำกันเพื่อเพิ่มความท้าทายในการทำงานร่วมกัน พร้อมระบบ Cache ข้อมูลแบบ Zero GC Allocation ตามกฎ Rule 5 & 6
- [SlipperyFloor.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/Obstacles/SlipperyFloor.cs): คราบน้ำมัน 4 จุดทั่วครัว เชฟเหยียบแล้วจะลื่นไถล 0.45 วินาที หาก Dash เข้าใส่จะได้รับ Slide Bonus พุ่งตัวยาว
- [KitchenCatNPC.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/Obstacles/KitchenCatNPC.cs) & [CatProceduralAnimator.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/Obstacles/CatProceduralAnimator.cs): แมวป่วนครัว 3D 2 ตัว (แมวส้ม/แมวเทา) แอบย่องเข้ามาคาบอาหารเดี่ยวๆ บนเคาน์เตอร์ และสามารถแอบขโมยเฉพาะวัตถุดิบบนจานอาหารได้ (`PlateKitchenObject.TryRemoveTopIngredient`) โดยคงจานอาหารไว้บนเคาน์เตอร์ 100% ห้ามขโมยจานไปเด็ดขาดเพื่อป้องกัน Plate Softlock แล้ววิ่งหนีด้วยความเร็วสูง สามารถเดินเข้าไปไล่ในระยะ 1.7 เมตร (`shooDistance = 1.7f`) ให้ตกใจทิ้งของได้ พร้อมระบบหน่วงเวลากลับมาเกิดใหม่ 7 วินาที (`respawnCooldown = 7.0f`) หลังจากขโมยอาหารสำเร็จและวิ่งพ้นกล้องออกไป พร้อมฟีเจอร์สำคัญ:
  - **Open Aisle Waypoint Patrol & Counter Clearance:** เดินตรวจตราตามจุด Waypoint สุ่มรอบทางเดินโล่ง พร้อมระบบ Counter Clearance ตรวจเช็คระยะห่างไม่ให้จุดหมายตกไปอยู่ในเคาน์เตอร์ ทำให้แมวยืนตรวจตราและดมกลิ่นบนพื้นโล่ง ไม่ติดคาในเคาน์เตอร์ 100%
  - **Kinematic Rigidbody & Pass-Through:** ติดตั้ง `Rigidbody` แบบ `isKinematic = true` ควบคู่กับ Trigger Colliders ทำให้แมวสามารถเดินทะลุผ่านเคาน์เตอร์และสิ่งกีดขวางได้อย่างราบรื่น ไร้แรงผลัก PhysX บล็อกเมื่อเข้าสู่โหมดขโมยของ
  - **Smooth Arrival & Steering:** ป้องกันการ Overshoot หรือสั่นกระตุกด้วยการคำนวณระยะก้าวพอดีจุดหมาย และหมุนตัวเลี้ยวอย่างนุ่มนวล
- [MovingCounter.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/Obstacles/MovingCounter.cs): เคาน์เตอร์เลื่อนตำแหน่งอัตโนมัติขวางทางเดิน พร้อมระบบ Path Clearance ตรวจเช็คแนววิ่งไม่ให้ชนเคาน์เตอร์อื่น
- [PotholeTrap.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/Obstacles/PotholeTrap.cs): หลุมดักสะดุดลดความเร็วเชฟ
- [ConveyorBelt.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/Obstacles/ConveyorBelt.cs): สายพานเลื่อนวัตถุดิบและผลักตัวละคร
- [RushHourManager.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/Gameplay/RushHourManager.cs) & [RushHourUI.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/UI/RushHourUI.cs): ชั่วโมงเร่งด่วน สุ่มออเดอร์ถี่ขึ้น 2 เท่า และให้คะแนน 2 เท่า
- [RaftKitchenTilt.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/Gameplay/RaftKitchenTilt.cs): จำลองคลื่นทะเลโยกเอียงห้องครัว

### 3.5 ระบบ UI, การจัดรูปแบบ และสไตล์ส่วนกลาง (User Interface & Centralized Theming)
- **Centralized Theming & Design Tokens:**
  - **[UITheme.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/UI/UITheme.cs):** สไตล์ชีตกลางใน C# รวม Palette สี (Cyan, Orange, Amber, Red, Green, Gold, Silver, Bronze), วัตถุ `Color`, เมธอดจัดรูปแบบข้อความ TextMeshPro (`FormatCleanPlateBadge`, `FormatDirtyPlateBadge`, `FormatRackFullPrompt`, `FormatGameOverDashboard`, `FormatTutorialHeader`), และแคตตาล็อกพาธ Local Assets
  - **[Assets/UITheme.css](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/UITheme.css):** ไฟล์ Design Tokens CSS กลางสำหรับ UI Toolkit ตามมาตรฐาน `CSSCodingGuide.md` ข้อ 9
- **UI Toolkit (UXML / USS Separation):**
  - [MainWindowFree.uxml](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/ChaosKitchen/New%20Unity%20Project/Assets/CodeMonkeyFree/Editor/MainWindowFree/MainWindowFree.uxml) & [MainWindowFree.uss](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/ChaosKitchen/New%20Unity%20Project/Assets/CodeMonkeyFree/Editor/MainWindowFree/MainWindowFree.uss): หน้าต่าง Editor แบบแยกโครงสร้างและ Stylesheet ชัดเจน 100% ปราศจาก Inline Styles
  - [VideoTemplate.uxml](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/ChaosKitchen/New%20Unity%20Project/Assets/CodeMonkeyFree/Editor/Templates/VideoTemplate.uxml) & [VideoTemplate.uss](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/ChaosKitchen/New%20Unity%20Project/Assets/CodeMonkeyFree/Editor/Templates/VideoTemplate.uss), [CodeTemplate.uxml](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/ChaosKitchen/New%20Unity%20Project/Assets/CodeMonkeyFree/Editor/Templates/CodeTemplate.uxml), [TextTemplate.uxml](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/ChaosKitchen/New%20Unity%20Project/Assets/CodeMonkeyFree/Editor/Templates/TextTemplate.uxml)
- **Screen-Space UI (UGUI & TextMeshPro):**
  - [MainMenuUI.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/UI/MainMenuUI.cs): หน้าต่างเมนูเริ่มเกม
  - [GameStartCountdownUI.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/UI/GameStartCountdownUI.cs): หน้านับถอยหลังและสอนเล่นแบบ 2 คอลัมน์กว้างสบายตา พร้อมปุ่มกดข้ามเข้าเกมทันที
  - [GamePlayingClockUI.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/UI/GamePlayingClockUI.cs): หลอดวงกลมแสดงเวลาเล่น พร้อม Null Safety ป้องกัน NRE
  - [GamePauseUI.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/UI/GamePauseUI.cs): เมนูหยุดเกมพร้อมฟังก์ชัน Resume และ กลับเมนูหลัก
  - [GameOverUI.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/UI/GameOverUI.cs): หน้าต่างสรุปผลคะแนน Dashboard แสดงการประเมินดาว (1-3 ดาว) ฉายาเชฟ (Master Executive Chef, Senior Sous Chef, Line Cook) และนับถอยหลัง 10 วินาทีเพื่อพาผู้เล่นกลับ MainMenuScene อัตโนมัติ
- **World-Space UI:**
  - [ProgressBarUI.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/UI/ProgressBarUI.cs): แถบความคืบหน้าลอยเหนือเคาน์เตอร์หั่น/ทอด/อ่างล้างจาน
  - [PlateIconsUI.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/UI/PlateIconsUI.cs) & [PlateIconsSingleUI.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/UI/PlateIconsSingleUI.cs): ไอคอนวัตถุดิบลอยเหนือจาน
  - [LookAtCamera.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/LookAtCamera.cs): จัดการมุมหัน Billboard เข้าหากล้องหลัก

---

## 📂 4. โครงสร้างโฟลเดอร์ในโปรเจกต์ (Project Directory Structure)

```text
CaDaCook (Unity Project)/
│
├── 📂 Assets/
│   ├── 📜 UITheme.css                       # CSS Design Tokens กลางตาม CSSCodingGuide.md
│   ├── 📂 Scenes/                           # MainMenuScene, GameScene, LoadingScene
│   ├── 📂 Scripts/                          # ซอร์สโค้ด C# ทั้งหมด
│   │   ├── 📂 Counters/                     # BaseCounter, ClearCounter, StoveCounter, CuttingCounter,
│   │   │                                    # PlatesCounter, SinkCounter, DeliveryCounter, TrashCounter
│   │   ├── 📂 Gameplay/                     # GameplayEventsBootstrap, RushHourManager, RaftKitchenTilt, RandomFireManager
│   │   ├── 📂 Obstacles/                    # FireHazard, FireExtinguisher, SlipperyFloor, KitchenCatNPC,
│   │   │                                    # CatProceduralAnimator, PotholeTrap, MovingCounter, ConveyorBelt
│   │   ├── 📂 ScriptableObjects/            # RecipeSO, KitchenObjectSO, AudioClipRefsSO, CuttingRecipeSO, ฯลฯ
│   │   ├── 📂 UI/                           # UITheme.cs, GamePauseUI, RushHourUI, DeliveryManagerUI, ComboUI,
│   │   │                                    # GameOverUI, GameStartCountdownUI, ProgressBarUI, ฯลฯ
│   │   ├── 📜 DeliveryManager.cs            # คิวออเดอร์, VIP Critic, Customer Patience, คอมโบและคะแนน
│   │   ├── 📜 DirtyPlateKitchenObject.cs    # วัตถุกองจานเปื้อน (Stackable Dirty Plates 1..4 ใบ)
│   │   ├── 📜 GameInput.cs                  # Unity Input System Event Wrapper (Move, Interact, Dash, Pause)
│   │   ├── 📜 KitchenGameManager.cs         # ตัวควบคุม Game State, เวลาการเล่น, ล็อค 60 FPS และซิงค์รอบฟิสิกส์ 60Hz
│   │   ├── 📜 KitchenObject.cs              # ตัวแทนวัตถุดิบอาหาร
│   │   ├── 📜 PlateKitchenObject.cs         # วัตถุจานอาหารและกองจานสะอาด (Stackable Clean Plates 1..4 ใบ)
│   │   ├── 📜 Player.cs                     # ตัวควบคุมเชฟหลัก, Dash, ดับไฟ, ลื่นน้ำมัน
│   │   ├── 📜 SoundManager.cs               # ตัวจัดการเสียง SFX ทั้งหมด พร้อมระบบ Zero-GC AudioSourcePool (20 แชนเนล)
│   │   ├── 📜 Loader.cs                     # คลาสกลางควบคุมการโหลดฉาก
│   │   ├── 📜 LoaderCallback.cs             # หน่วงเวลาสลับเฟรมการโหลดฉาก
│   │   └── 📜 LookAtCamera.cs               # Billboard Effect หัน UI เข้าหากล้อง
│   │
│   ├── 📂 ChaosKitchen/                     # แพ็กเกจสื่อและเทมเพลต UI Toolkit
│   │   └── 📂 New Unity Project/Assets/CodeMonkeyFree/
│   │       ├── 📂 Editor/MainWindowFree/    # MainWindowFree.cs, MainWindowFree.uxml, MainWindowFree.uss
│   │       └── 📂 Editor/Templates/         # VideoTemplate, CodeTemplate, TextTemplate (.uxml & .uss)
│   │
│   ├── 📂 Prefabs/                          # Counters, KitchenObjects, Neko Cat, UI Prefabs
│   └── 📜 PlayerInputActions.cs             # Input Actions C# Wrapper
│
├── 📂 Packages/                             # การตั้งค่า Dependencies ของ Unity Package Manager
│   ├── 📜 manifest.json                     # กำหนดแพ็กเกจ (Input System, URP, Cinemachine)
│   └── 📜 packages-lock.json                # Lockfile เวอร์ชันของแพ็กเกจ
│
├── 📂 markdowns/                            # เอกสารคู่มือและมาตรฐานทั้งหมด
│   ├── 📜 AboutProject.md                   # คู่มือฉบับเต็มโครงสร้างโปรเจกต์ (ไฟล์นี้)
│   ├── 📜 PROJECT.md                        # ภาพรวมเป้าหมายและฟีเจอร์ของเกม
│   ├── 📜 TECHSTACK.md                      # สเปกเทคโนโลยีและเวอร์ชัน Unity 6
│   ├── 📜 DESIGN.md                         # Game Design Document & Dynamic Events
│   ├── 📜 REFACTORCODE.md                   # กฎเหล็ก 15 ข้อสำหรับการพัฒนา C# Unity
│   ├── 📜 CSharpCodingGuide.md              # คู่มือมาตรฐานการเขียน C# Unity
│   ├── 📜 CSSCodingGuide.md                 # คู่มือมาตรฐานการเขียน CSS / USS
│   ├── 📜 DeMorgansLaws.md                  # กฎ De Morgan's Laws & Early Return
│   ├── 📜 DEBUG.md                          # คู่มือดีแบ๊กสำหรับ AI Agents
│   ├── 📜 SECURITY.md                       # มาตรฐานความปลอดภัยของเกม
│   └── 📜 LOG.md                            # บันทึกประวัติการพัฒนาและการปรับปรุงระบบ
│
├── 📜 .antigravityignore                    # การละเว้นไฟล์/โฟลเดอร์สำหรับ AI Agents
├── 📜 .gitignore                            # การละเว้นไฟล์สำหรับ Git
└── 📜 README.md                             # สารบัญนำทางโปรเจกต์ที่รูท
```