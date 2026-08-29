# 🍳 CaDaCook - Complete Project Documentation

**CaDaCook** คือเกมแนวทำอาหารและบริหารจัดการครัว 3D Fast-paced Casual Simulation (สไตล์ Kitchen Chaos / Overcooked) ที่ถูกพัฒนาขึ้นด้วย **Unity 6 (6000.3.6f1)** และเขียนโปรแกรมด้วยภาษา **C#** 

เอกสารฉบับนี้รวบรวมรายละเอียดสถาปัตยกรรมระบบ, โครงสร้างไฟล์ C#, ระบบเคาน์เตอร์ทำอาหาร, ระบบอุปสรรคและอีเวนต์ไดนามิก (Dynamic Events & Obstacles), การจัดการอีเวนต์ (C# Events) และผังโฟลเดอร์ของโปรเจกต์ทั้งหมด

---

## 🛠️ 1. สถาปัตยกรรมระบบและโครงสร้างทางเทคนิค (System Architecture)

ระบบของเกม CaDaCook ถูกออกแบบโดยยึดหลัก **Decoupled Architecture** เพื่อให้การทำงานในแต่ละส่วนแยกออกจากกันอย่างชัดเจน และง่ายต่อการขยายฟีเจอร์

### 1.1 สถาปัตยกรรม C# และรูปแบบการออกแบบ (Design Patterns)
- **Observer Pattern via C# Events:** แยกส่วน Game Logic ออกจาก Visual Effects, Animations, UI และ Audio 100%
- **Singleton Pattern:** ใช้สำหรับคลาสศูนย์กลางของเกมที่มี Instance เดียวในฉาก (`KitchenGameManager`, `DeliveryManager`, `GameInput`, `SoundManager`, `RushHourManager`, `Player`)
- **Data-Driven Architecture (ScriptableObjects):** นิยามข้อมูลวัตถุดิบและสูตรอาหารไว้ใน Assets เพื่อให้ปรับแต่งค่าได้ทันทีผ่าน Unity Inspector
- **Interface Segregation:**
  - `IKitchenObjectParent`: ใช้นิยามพฤติกรรมการเป็นเจ้าของ/ถือ/ส่งต่อวัตถุดิบ (`Player`, `BaseCounter`, `KitchenCatNPC`)
  - `IHasProgress`: ใช้นิยามพฤติกรรมของเคาน์เตอร์ที่มีหลอดแสดงความคืบหน้า (`CuttingCounter`, `StoveCounter`)

---

## 🍳 2. เจาะลึกระบบเกมเพลย์และสคริปต์หลัก (Core Game Systems)

### 2.1 ระบบตัวละครและการควบคุม (Player & Input)
- [`Player.cs`](file:///d:/unity/My%20project/Assets/Scripts/Player.cs): จัดการการเคลื่อนที่ 3 มิติ, การหมุนตัว, การตรวจจับเคาน์เตอร์ตรงหน้าด้วย Raycast, การหยิบจับวัตถุดิบ, และรองรับอุปสรรค (ลื่นไถลคราบน้ำมัน `SetSlipping()`, ชะลอความเร็ว `ApplySlowEffect()`, การฉีดถังดับเพลิง `[F] HOLD TO SPRAY` และทิ้งถังลงพื้น `[E] DROP`)
- [`PlayerAnimator.cs`](file:///d:/unity/My%20project/Assets/Scripts/PlayerAnimator.cs): อัปเดตพารามิเตอร์ `IsWalking` ใน Animator Controller ตามสถานะการเคลื่อนที่จริง
- [`PlayerSounds.cs`](file:///d:/unity/My%20project/Assets/Scripts/PlayerSounds.cs): เล่นเสียงฝีเท้าตามจังหวะการก้าวเดิน
- [`GameInput.cs`](file:///d:/unity/My%20project/Assets/Scripts/GameInput.cs): ดักจับ Action จาก `PlayerInputActions` (Move, Interact, InteractAlternate, Pause) และกระจายเป็น C# Events

### 2.2 ระบบเคาน์เตอร์ครัว (Modular Kitchen Counters)
สคริปต์เคาน์เตอร์ทั้งหมดสืบทอดมาจากคลาสฐาน [`BaseCounter.cs`](file:///d:/unity/My%20project/Assets/Scripts/Counters/BaseCounter.cs):
- **`ClearCounter`:** เคาน์เตอร์ว่างสำหรับวางพักวัตถุดิบ หรือนำจานมารวมวัตถุดิบ
- **`ContainerCounter`:** เคาน์เตอร์หยิบวัตถุดิบดิบไม่จำกัดจำนวน
- **`CuttingCounter`:** เขียงหั่นอาหาร รองรับ `IHasProgress` และส่งอีเวนต์ `OnCut`
- **`StoveCounter`:** เตาอบ/กระทะทอด มี Finite State Machine 4 สเตต (`Idle`, `Frying`, `Fried`, `Burned`) และเชื่อมต่อกับ `FireHazard`
- **`PlatesCounter`:** เคาน์เตอร์ผลิตจานเปล่าอัตโนมัติตามช่วงเวลา
- **`DeliveryCounter`:** เคาน์เตอร์ส่งอาหาร ตรวจสอบสูตรกับ `DeliveryManager`
- **`TrashCounter`:** ถังขยะสำหรับทำลายวัตถุดิบทิ้ง
- **`SelectedCounterVisual`:** แสดงผลขอบเรืองแสงสีขาวรอบเคาน์เตอร์ที่เลือก

### 2.3 ระบบอุปสรรคและเหตุการณ์ไดนามิก (Dynamic Events & Obstacles)
- [`GameplayEventsBootstrap.cs`](file:///d:/unity/My%20project/Assets/Scripts/Gameplay/GameplayEventsBootstrap.cs): จุดศูนย์กลางเริ่มต้นระบบอุปสรรคทั้งหมดในครัวอัตโนมัติ (สุ่มสร้างไฟไหม้, คราบน้ำมัน 4 จุด, แมว NPC 2 ตัว, เคาน์เตอร์เลื่อน 2 ตัว, คลื่นแพโยก)
- [`FireHazard.cs`](file:///d:/unity/My%20project/Assets/Scripts/Obstacles/FireHazard.cs): ควบคุมสถานะไฟลุกไหม้บนเคาน์เตอร์ มีระดับพลังไฟ (`Extinguish()`) และแจ้งเตือนเสียงไฟไหม้
- [`FireExtinguisher.cs`](file:///d:/unity/My%20project/Assets/Scripts/Obstacles/FireExtinguisher.cs): ถังดับเพลิง 3D สมบูรณ์แบบ (Red Body, Grey Top Nozzle, Black Tip) พร้อมป้ายคำสั่ง Billboard และละอองขาวดับเพลิง
- [`SlipperyFloor.cs`](file:///d:/unity/My%20project/Assets/Scripts/Obstacles/SlipperyFloor.cs): คราบน้ำมันบนพื้น ทำให้ตัวละครเชฟลื่นไถลและหมุนตัว
- [`PotholeTrap.cs`](file:///d:/unity/My%20project/Assets/Scripts/Obstacles/PotholeTrap.cs): หลุมดักสะดุดที่ลดความเร็วของผู้เล่นและมีโอกาสทำของตก
- [`MovingCounter.cs`](file:///d:/unity/My%20project/Assets/Scripts/Obstacles/MovingCounter.cs): เคาน์เตอร์เลื่อนตำแหน่งอัตโนมัติ 2 ตัว เลื่อนสลับจังหวะทั้งแกน X และแกน Z
- [`ConveyorBelt.cs`](file:///d:/unity/My%20project/Assets/Scripts/Obstacles/ConveyorBelt.cs): สายพานลำเลียงส่งวัตถุดิบและผลักตัวละคร
- [`KitchenCatNPC.cs`](file:///d:/unity/My%20project/Assets/Scripts/Obstacles/KitchenCatNPC.cs): แมวป่วนครัวที่แอบเข้ามาขโมยวัตถุดิบบนเคาน์เตอร์ และวิ่งหนีเมื่อผู้เล่นเข้าไปไล่
- [`CatProceduralAnimator.cs`](file:///d:/unity/My%20project/Assets/Scripts/Obstacles/CatProceduralAnimator.cs): ระบบแอนิเมชัน Procedural ส่ายหางและเดินดุ๊กดิ๊กของแมว 3D
- [`RushHourManager.cs`](file:///d:/unity/My%20project/Assets/Scripts/Gameplay/RushHourManager.cs): ตัวจัดการชั่วโมงเร่งด่วน สุ่มออเดอร์เร็วขึ้น 2 เท่า และให้คะแนน 2 เท่า
- [`RaftKitchenTilt.cs`](file:///d:/unity/My%20project/Assets/Scripts/Gameplay/RaftKitchenTilt.cs): จำลองคลื่นทะเลและการโยกเอียงของแพในครัวธีมชายหาด

### 2.4 ระบบอินเทอร์เฟซผู้ใช้ (User Interface)
- **Screen-Space UI:**
  - [`MainMenuUI.cs`](file:///d:/unity/My%20project/Assets/Scripts/UI/MainMenuUI.cs): ปุ่ม Play และ Quit
  - [`GameStartCountdownUI.cs`](file:///d:/unity/My%20project/Assets/Scripts/UI/GameStartCountdownUI.cs): หน้าต่างสอนเล่น 15 วินาที แสดงปุ่มควบคุม วิธีทำอาหาร และกล่องเตือนเด่นชัดสีส้มเรื่องแมวขโมยถังดับเพลิง
  - [`GamePlayingClockUI.cs`](file:///d:/unity/My%20project/Assets/Scripts/UI/GamePlayingClockUI.cs): หลอดวงกลมแสดงเวลาเล่นที่เหลือ
  - [`RushHourUI.cs`](file:///d:/unity/My%20project/Assets/Scripts/UI/RushHourUI.cs): ป้ายแจ้งเตือนชั่วโมงเร่งด่วน 2X Points
  - [`GamePauseUI.cs`](file:///d:/unity/My%20project/Assets/Scripts/UI/GamePauseUI.cs): เมนูหยุดเกม (Resume, Main Menu พร้อม auto-focus และ reset timeScale)
  - [`GameOverUI.cs`](file:///d:/unity/My%20project/Assets/Scripts/UI/GameOverUI.cs): หน้าต่างสรุปคะแนนเมื่อหมดเวลา พร้อมระบบ Auto Return สู่หน้า Menu ใน 5 วินาที
  - [`DeliveryManagerUI.cs`](file:///d:/unity/My%20project/Assets/Scripts/UI/DeliveryManagerUI.cs): คอนเทนเนอร์รายการออเดอร์
- **World-Space UI:**
  - [`ProgressBarUI.cs`](file:///d:/unity/My%20project/Assets/Scripts/UI/ProgressBarUI.cs): แถบพลังลอยบนเคาน์เตอร์
  - [`PlateIconsUI.cs`](file:///d:/unity/My%20project/Assets/Scripts/UI/PlateIconsUI.cs): แสดงไอคอนของวัตถุดิบบนจาน
  - [`LookAtCamera.cs`](file:///d:/unity/My%20project/Assets/Scripts/LookAtCamera.cs): คำนวณมุมหัน Billboard Effect เข้าหากล้อง

---

## 📂 3. โครงสร้างโฟลเดอร์ในโปรเจกต์ (Project Directory Structure)

```text
CaDaCook (Unity Project)/
│
├── 📂 Assets/
│   ├── 📂 Scenes/                           # MainMenuScene, GameScene, LoadingScene
│   │
│   ├── 📂 Scripts/                          # ซอร์สโค้ด C# ทั้งหมด
│   │   ├── 📂 Counters/                     # BaseCounter, ClearCounter, StoveCounter, CuttingCounter, ฯลฯ
│   │   ├── 📂 Gameplay/                     # GameplayEventsBootstrap.cs, RushHourManager.cs, RaftKitchenTilt.cs
│   │   ├── 📂 Obstacles/                    # FireHazard.cs, FireExtinguisher.cs, SlipperyFloor.cs,
│   │   │                                    # KitchenCatNPC.cs, CatProceduralAnimator.cs, PotholeTrap.cs, MovingCounter.cs
│   │   ├── 📂 ScriptableObjects/            # RecipeSO, KitchenObjectSO, AudioClipRefsSO, ฯลฯ
│   │   ├── 📂 UI/                           # GamePauseUI, RushHourUI, DeliveryManagerUI, ProgressBarUI, ฯลฯ
│   │   ├── 📜 DeliveryManager.cs            # คิวออเดอร์และระบบคะแนน
│   │   ├── 📜 GameInput.cs                  # Unity Input System Event Wrapper
│   │   ├── 📜 KitchenGameManager.cs         # ตัวควบคุม Game State & Pause
│   │   ├── 📜 KitchenObject.cs              # ตัวแทนวัตถุดิบอาหาร
│   │   ├── 📜 PlateKitchenObject.cs         # จัดการรายการวัตถุดิบบนจาน
│   │   ├── 📜 Player.cs                     # ตัวควบคุมเชฟหลักและอุปสรรค
│   │   └── 📜 SoundManager.cs               # ตัวจัดการเสียง SFX ทั้งหมด
│   │
│   ├── 📂 Prefabs/                          # Counters, KitchenObjects, Neko Cat, UI Prefabs
│   └── 📜 PlayerInputActions.cs             # Input Actions C# Wrapper
│
├── 📂 markdowns/                            # เอกสารคู่มือและมาตรฐานทั้งหมด
│   ├── 📜 AboutProject.md                   # คู่มือฉบับเต็ม (ไฟล์นี้)
│   ├── 📜 PROJECT.md                        # ภาพรวมเป้าหมายและฟีเจอร์ของเกม
│   ├── 📜 TECHSTACK.md                      # สเปกเทคโนโลยีและเวอร์ชัน Unity 6
│   ├── 📜 DESIGN.md                         # Game Design Document & Dynamic Events
│   ├── 📜 REFACTORCODE.md                   # กฎเหล็ก 15 ข้อสำหรับการเขียน C# Unity
│   ├── 📜 CSharpCodingGuide.md              # คู่มือมาตรฐานการเขียน C#
│   ├── 📜 DeMorgansLaws.md                  # กฎ De Morgan's Laws & Early Return
│   ├── 📜 DEBUG.md                          # คู่มือดีแบ๊กและแก้ปัญหาบัคใน Unity
│   ├── 📜 SECURITY.md                       # มาตรฐานความปลอดภัยของเกม
│   └── 📜 LOG.md                            # บันทึกประวัติการพัฒนาและปรับปรุงระบบ
│
├── 📜 .antigravityignore                    # การละเว้นไฟล์/โฟลเดอร์สำหรับ AI Agents
├── 📜 .gitignore                            # การละเว้นไฟล์สำหรับ Git
└── 📜 README.md                             # สารบัญนำทางโปรเจกต์ที่รูท
```