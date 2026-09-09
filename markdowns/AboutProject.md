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
              └──► (จบเกม 5 วินาที ใน GameOverUI.cs) ──► [ LoadingScene ] ──► [ MainMenuScene ]
```

---

## 🍳 3. เจาะลึกระบบเกมเพลย์และสคริปต์หลัก (Core Game Systems)

### 3.1 ระบบตัวละครและการควบคุม (Player & Input)
- [Player.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/Player.cs): ควบคุมการเคลื่อนที่ 3 มิติ, การหมุนตัว, การ Raycast ตรวจจับเคาน์เตอร์ตรงหน้าด้วย countersLayerMask, การหยิบ-วางวัตถุดิบ, และระบบรองรับอุปสรรค (ลื่นไถลน้ำมัน TriggerSlipImpulse, การฉีดถังดับเพลิง [F] HOLD TO SPRAY, และการทิ้งถังลงพื้น [E] DROP)
- [PlayerAnimator.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/PlayerAnimator.cs): ส่งค่า IsWalking ไปยัง Animator Controller
- [PlayerSounds.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/PlayerSounds.cs): เล่นเสียงฝีเท้าตามจังหวะก้าวเดิน
- [GameInput.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/GameInput.cs): ตัวห่อหุ้ม New Input System (PlayerInputActions) กระจาย Action เป็น C# Events
- [PlayerInputActions.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/PlayerInputActions.cs): C# Wrapper ของไฟล์ Input Action Asset

### 3.2 ระบบเคาน์เตอร์ครัว (Modular Kitchen Counters)
เคาน์เตอร์ทุกชนิดสืบทอดมาจากคลาสฐาน [BaseCounter.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/Counters/BaseCounter.cs):
- **[ClearCounter.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/Counters/ClearCounter.cs):** เคาน์เตอร์ว่างสำหรับวางพักวัตถุดิบ หรือรวมวัตถุดิบลงบนจาน
- **[ContainerCounter.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/Counters/ContainerCounter.cs):** เคาน์เตอร์จ่ายวัตถุดิบดิบไม่จำกัด พร้อม [ContainerCounterVisual.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/Counters/ContainerCounterVisual.cs) แสดงอนิเมชันเปิด-ปิดฝาตู้
- **[CuttingCounter.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/Counters/CuttingCounter.cs):** เขียงหั่นอาหาร รองรับ IHasProgress และส่งอีเวนต์ OnCut ร่วมกับ [CuttingCounterVisual.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/Counters/CuttingCounterVisual.cs)
- **[StoveCounter.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/Counters/StoveCounter.cs):** กระทะทอด/เตาปรุงอาหาร มี Finite State Machine 4 สเตต (Idle, Frying, Fried, Burned) และเชื่อมต่อกับ [StoveCounterVisual.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/Counters/StoveCounterVisual.cs) และ [StoveCounterSound.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/Counters/StoveCounterSound.cs)
- **[PlatesCounter.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/Counters/PlatesCounter.cs):** เคาน์เตอร์ผลิตจานอัตโนมัติตามช่วงเวลา พร้อม [PlatesCounterVisual.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/Counters/PlatesCounterVisual.cs) แสดงโมเดลจานซ้อนทับกัน
- **[DeliveryCounter.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/Counters/DeliveryCounter.cs):** เคาน์เตอร์ส่งอาหาร ตรวจสอบสูตรกับ DeliveryManager
- **[TrashCounter.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/Counters/TrashCounter.cs):** ถังขยะสำหรับทำลายวัตถุดิบทิ้ง
- **[SelectedCounterVisual.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/SelectedCounterVisual.cs):** ขอบเรืองแสงสีขาวรอบเคาน์เตอร์ที่ผู้เล่นกำลังหันหน้าเข้าหา

### 3.3 ระบบออเดอร์, VIP Critic & Combo Streak
- **[DeliveryManager.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/DeliveryManager.cs):** ตัวจัดการคิวออเดอร์ส่วนกลาง
  - **VIP Critic Orders:** มีโอกาสสุ่มเกิดออเดอร์ VIP กรอบสีทอง มีเวลานับถอยหลังจำกัด 25 วินาที หากส่งทันจะได้โบนัสคะแนน **3 เท่า (3X Score)** และต่อยอดคอมโบ
  - **Combo Streak System:** ส่งอาหารสำเร็จต่อเนื่องโดยไม่ผิดพลาดหรือปล่อยให้ VIP หมดเวลา
    - Streak >= 3: ได้รับตัวคูณคะแนน 1.5x
    - Streak >= 5: ได้รับตัวคูณคะแนน 2.0x (Super Combo)
- **[ComboUI.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/UI/ComboUI.cs):** แสดงสถานะ Combo Streak ข้างนาฬิกาจับเวลา ตัวอักษรสีส้มสดใส คมชัด ไร้พื้นหลังสีดำ
- **[DeliveryManagerUI.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/UI/DeliveryManagerUI.cs) & [DeliveryManagerSingleUI.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/UI/DeliveryManagerSingleUI.cs):** แสดงการ์ดออเดอร์ที่มุมบนซ้าย พร้อมหลอดเวลานับถอยหลังของการ์ด VIP
- **[PlayerDeliveryResultUI.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/UI/PlayerDeliveryResultUI.cs):** ป๊อปอัปแจ้งผลการส่งอาหาร (Success สีเขียว / Failed สีแดง)

### 3.4 ระบบอุปสรรคและเหตุการณ์ไดนามิก (Dynamic Events & Obstacles)
- [GameplayEventsBootstrap.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/Gameplay/GameplayEventsBootstrap.cs): จุดเริ่มต้นระบบอุปสรรคทั้งหมดในครัวอัตโนมัติ
- [FireHazard.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/Obstacles/FireHazard.cs): จำลองไฟลุกไหม้บนเคาน์เตอร์ พร้อมเอฟเฟกต์ไฟ/ควันดำสมจริง หากดับไม่ทันเคาน์เตอร์จะถูกล็อคกลายเป็นสีดำ 5 วินาที
- [FireExtinguisher.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/Obstacles/FireExtinguisher.cs): ถังดับเพลิง 3D พร้อมป้าย Billboard UI คมชัด [E] PICK UP, ถือแล้วกด [F] HOLD TO SPRAY พ่นละอองขาวดับไฟข้างหน้า, และ [E] DROP ทิ้งลงพื้น
- [RandomFireManager.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/Gameplay/RandomFireManager.cs): สุ่มจุดเกิดไฟไหม้ตามเคาน์เตอร์ต่างๆ ทุกๆ 18-28 วินาที
- [SlipperyFloor.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/Obstacles/SlipperyFloor.cs): คราบน้ำมัน 4 จุดทั่วครัว เชฟเหยียบแล้วจะลื่นไถล 0.45 วินาที พร้อมคูลดาวน์ป้องกันการติดลูป
- [KitchenCatNPC.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/Obstacles/KitchenCatNPC.cs) & [CatProceduralAnimator.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/Obstacles/CatProceduralAnimator.cs): แมวป่วนครัว 3D 2 ตัว (แมวส้ม/แมวเทา) แอบย่องเข้ามาคาบอาหารบนเคาน์เตอร์ และวิ่งหนีด้วยความเร็วสูง
- [MovingCounter.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/Obstacles/MovingCounter.cs): เคาน์เตอร์เลื่อนตำแหน่งอัตโนมัติขวางทางเดิน
- [PotholeTrap.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/Obstacles/PotholeTrap.cs): หลุมดักสะดุดลดความเร็วเชฟ
- [ConveyorBelt.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/Obstacles/ConveyorBelt.cs): สายพานเลื่อนวัตถุดิบและผลักตัวละคร
- [RushHourManager.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/Gameplay/RushHourManager.cs) & [RushHourUI.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/UI/RushHourUI.cs): ชั่วโมงเร่งด่วน สุ่มออเดอร์ถี่ขึ้น 2 เท่า และให้คะแนน 2 เท่า
- [RaftKitchenTilt.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/Gameplay/RaftKitchenTilt.cs): จำลองคลื่นทะเลโยกเอียงห้องครัว

### 3.5 ระบบ UI และการแสดงผล (User Interface)
- **Screen-Space UI:**
  - [MainMenuUI.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/UI/MainMenuUI.cs): หน้าต่างเมนูเริ่มเกม
  - [GameStartCountdownUI.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/UI/GameStartCountdownUI.cs): หน้านับถอยหลังและสอนเล่นแบบ 2 คอลัมน์กว้างสบายตา
  - [GamePlayingClockUI.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/UI/GamePlayingClockUI.cs): หลอดวงกลมแสดงเวลาเล่น พร้อม Null Safety ป้องกัน NRE
  - [GamePauseUI.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/UI/GamePauseUI.cs): เมนูหยุดเกมพร้อมฟังก์ชัน Resume และ กลับเมนูหลัก
  - [GameOverUI.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/UI/GameOverUI.cs): หน้าต่างสรุปคะแนนหลังจบเกม พร้อมกลับเมนูอัตโนมัติใน 5 วินาที
- **World-Space UI:**
  - [ProgressBarUI.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/UI/ProgressBarUI.cs): แถบความคืบหน้าลอยเหนือเคาน์เตอร์หั่น/ทอด
  - [PlateIconsUI.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/UI/PlateIconsUI.cs) & [PlateIconsSingleUI.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/UI/PlateIconsSingleUI.cs): ไอคอนวัตถุดิบลอยเหนือจาน
  - [LookAtCamera.cs](file:///c:/CaDaCooked/CaDaCookedScripts/Assets/Scripts/LookAtCamera.cs): จัดการมุมหัน Billboard เข้าหากล้องหลัก

---

## 📂 4. โครงสร้างโฟลเดอร์ในโปรเจกต์ (Project Directory Structure)

```text
CaDaCook (Unity Project)/
│
├── 📂 Assets/
│   ├── 📂 Scenes/                           # MainMenuScene, GameScene, LoadingScene
│   ├── 📂 Scripts/                          # ซอร์สโค้ด C# ทั้งหมด
│   │   ├── 📂 Counters/                     # BaseCounter, ClearCounter, StoveCounter, CuttingCounter, ฯลฯ
│   │   ├── 📂 Gameplay/                     # GameplayEventsBootstrap, RushHourManager, RaftKitchenTilt, RandomFireManager
│   │   ├── 📂 Obstacles/                    # FireHazard, FireExtinguisher, SlipperyFloor, KitchenCatNPC,
│   │   │                                    # CatProceduralAnimator, PotholeTrap, MovingCounter, ConveyorBelt
│   │   ├── 📂 ScriptableObjects/            # RecipeSO, KitchenObjectSO, AudioClipRefsSO, CuttingRecipeSO, ฯลฯ
│   │   ├── 📂 UI/                           # GamePauseUI, RushHourUI, DeliveryManagerUI, ComboUI, ProgressBarUI, ฯลฯ
│   │   ├── 📜 DeliveryManager.cs            # คิวออเดอร์, VIP Critic, ระบบคอมโบและคะแนน
│   │   ├── 📜 GameInput.cs                  # Unity Input System Event Wrapper
│   │   ├── 📜 KitchenGameManager.cs         # ตัวควบคุม Game State และเวลาการเล่น
│   │   ├── 📜 KitchenObject.cs              # ตัวแทนวัตถุดิบอาหาร
│   │   ├── 📜 PlateKitchenObject.cs         # จัดการรายการวัตถุดิบบนจาน
│   │   ├── 📜 Player.cs                     # ตัวควบคุมเชฟหลักและอุปสรรค
│   │   ├── 📜 SoundManager.cs               # ตัวจัดการเสียง SFX ทั้งหมด
│   │   ├── 📜 Loader.cs                     # คลาสกลางควบคุมการโหลดฉาก
│   │   ├── 📜 LoaderCallback.cs             # หน่วงเวลาสลับเฟรมการโหลดฉาก
│   │   └── 📜 LookAtCamera.cs               # Billboard Effect หัน UI เข้าหากล้อง
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
│   ├── 📜 DeMorgansLaws.md                  # กฎ De Morgan's Laws & Early Return
│   ├── 📜 DEBUG.md                          # คู่มือดีแบ๊กสำหรับ AI Agents
│   ├── 📜 SECURITY.md                       # มาตรฐานความปลอดภัยของเกม
│   └── 📜 LOG.md                            # บันทึกประวัติการพัฒนาและการปรับปรุงระบบ
│
├── 📜 .antigravityignore                    # การละเว้นไฟล์/โฟลเดอร์สำหรับ AI Agents
├── 📜 .gitignore                            # การละเว้นไฟล์สำหรับ Git
└── 📜 README.md                             # สารบัญนำทางโปรเจกต์ที่รูท
```