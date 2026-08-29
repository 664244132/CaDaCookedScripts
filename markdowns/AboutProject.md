# 🍳 CaDaCook - Complete Project Documentation

**CaDaCook** คือเกมแนวทำอาหารและบริหารจัดการครัว 3D Fast-paced Casual Simulation (สไตล์ Kitchen Chaos / Overcooked) ที่ถูกพัฒนาขึ้นด้วย **Unity 6 (6000.3.6f1)** และเขียนโปรแกรมด้วยภาษา **C#** 

เอกสารฉบับนี้รวบรวมรายละเอียดสถาปัตยกรรมระบบ, โครงสร้างไฟล์ C#, ระบบเคาน์เตอร์ทำอาหาร, ระบบสูตรอาหาร ScriptableObjects, การจัดการอีเวนต์ (C# Events) และผังโฟลเดอร์ของโปรเจกต์ทั้งหมด

---

## 🛠️ 1. สถาปัตยกรรมระบบและโครงสร้างทางเทคนิค (System Architecture)

ระบบของเกม CaDaCook ถูกออกแบบโดยยึดหลัก **Decoupled Architecture** เพื่อให้การทำงานในแต่ละส่วนแยกออกจากกันอย่างชัดเจน และง่ายต่อการขยายฟีเจอร์ในอนาคต

### 1.1 สถาปัตยกรรม C# และรูปแบบการออกแบบ (Design Patterns)
- **Observer Pattern via C# Events:** แยกส่วน Game Logic ออกจาก Visual Effects, Animations, UI และ Audio 100% (เช่น เมื่อหั่นอาหาร `CuttingCounter` จะส่ง `OnCut` อีเวนต์ เพื่อให้ `CuttingCounterVisual` เล่นแอนิเมชัน และ `SoundManager` เล่นเสียงมีด โดยที่ `CuttingCounter` ไม่ต้องผูกติดกับ AudioSource หรือ Animator)
- **Singleton Pattern:** ใช้สำหรับคลาสศูนย์กลางของเกมที่มี Instance เดียวในฉาก ได้แก่:
  - `KitchenGameManager.Instance`: จัดการ Game State, Timers, Pause/Unpause
  - `DeliveryManager.Instance`: จัดการคิวออเดอร์, ตรวจสอบความถูกต้องของอาหาร และบันทึกคะแนน
  - `GameInput.Instance`: จัดการ Unity Input System และแปลงอินพุตเป็น C# Events
  - `SoundManager.Instance`: จัดการเล่นเสียง Sound Effects ทั้งหมดในเกม
  - `Player.Instance`: ตัวแปรอ้างอิงถึงตัวละครผู้เล่นหลัก
- **Data-Driven Architecture (ScriptableObjects):** นิยามข้อมูลวัตถุดิบและสูตรอาหารไว้ใน Assets เพื่อให้ Game Designer สามารถสร้างสูตรอาหารหรือวัตถุดิบใหม่ได้ทันทีผ่าน Unity Inspector โดยไม่ต้องเขียนโค้ดเพิ่ม
- **Interface Segregation:**
  - `IKitchenObjectParent`: ใช้นิยามพฤติกรรมการเป็นเจ้าของ/ถือ/ส่งต่อวัตถุดิบ (`Player` และ `BaseCounter` ทุกตัวนำไปใช้)
  - `IHasProgress`: ใช้นิยามพฤติกรรมของเคาน์เตอร์ที่มีหลอดแสดงความคืบหน้า (`CuttingCounter`, `StoveCounter`) เพื่อให้ `ProgressBarUI` ทำงานร่วมกันได้แบบ Universal

---

## 🍳 2. เจาะลึกระบบเกมเพลย์และสคริปต์หลัก (Core Game Systems)

### 2.1 ระบบตัวละครและการควบคุม (Player & Input)
- [`Player.cs`](file:///d:/unity/My%20project/Assets/Scripts/Player.cs): จัดการการเคลื่อนที่ 3 มิติ, การหมุนตัว, การตรวจจับการชน (Collision Detection ด้วย `Physics.CapsuleCast`), การสแกนหาเคาน์เตอร์ตรงหน้าด้วย `Physics.Raycast` เพื่อไฮไลท์และสั่ง `Interact()` หรือ `InteractAlternate()`
- [`PlayerAnimator.cs`](file:///d:/unity/My%20project/Assets/Scripts/PlayerAnimator.cs): อัปเดตพารามิเตอร์ `IsWalking` ใน Animator Controller ตามสถานะการเคลื่อนที่จริง
- [`PlayerSounds.cs`](file:///d:/unity/My%20project/Assets/Scripts/PlayerSounds.cs): เล่นเสียงฝีเท้าตามจังหวะการก้าวเดิน
- [`GameInput.cs`](file:///d:/unity/My%20project/Assets/Scripts/GameInput.cs): ดักจับ Action จาก `PlayerInputActions` (Move, Interact, InteractAlternate, Pause) และกระจายเป็น C# Events

### 2.2 ระบบเคาน์เตอร์ครัว (Modular Kitchen Counters)
สคริปต์เคาน์เตอร์ทั้งหมดสืบทอดมาจากคลาสฐาน [`BaseCounter.cs`](file:///d:/unity/My%20project/Assets/Scripts/Counters/BaseCounter.cs) ซึ่งเป็น `IKitchenObjectParent`:
- **`ClearCounter`:** เคาน์เตอร์ว่างสำหรับวางพักวัตถุดิบ หรือนำจานมารวมวัตถุดิบ
- **`ContainerCounter`:** เคาน์เตอร์หยิบวัตถุดิบดิบไม่จำกัดจำนวน (ส่งอีเวนต์ให้ `ContainerCounterVisual` เล่นแอนิเมชันเปิดฝา)
- **`CuttingCounter`:** เขียงหั่นอาหาร รองรับ `IHasProgress` เมื่อผู้เล่นกดปุ่มหั่นซ้ำๆ จะเพิ่มจำนวนครั้งจนครบ `cuttingProgressMax` และเปลี่ยนวัตถุดิบเป็นชิ้นที่หั่นแล้ว
- **`StoveCounter`:** เตาอบ/กระทะทอด มี Finite State Machine 4 สเตต (`Idle`, `Frying`, `Fried`, `Burned`) อัปเดตตัวจับเวลาแบบเรียลไทม์ และแจ้งเตือนผ่าน `StoveCounterVisual` และ `StoveCounterSound`
- **`PlatesCounter`:** เคาน์เตอร์ผลิตจานเปล่า โดยจะสร้างจานใหม่ตามช่วงเวลาจนครบขีดจำกัดสูงสุด (`platesSpawnAmountMax = 4`)
- **`DeliveryCounter`:** เคาน์เตอร์ส่งอาหาร รับจานจากผู้เล่นและส่งให้ `DeliveryManager.Instance.DeliveryRecipe(plateKitchenObject)`
- **`TrashCounter`:** ถังขยะสำหรับทำลายวัตถุดิบทิ้งเมื่อเกิดข้อผิดพลาด
- **`SelectedCounterVisual`:** แสดงผลขอบเรืองแสงสีขาวรอบเคาน์เตอร์ที่ผู้เล่นกำลังหันหน้าเข้าหา

### 2.3 ระบบวัตถุดิบ จาน และการประกอบอาหาร (Kitchen Objects & Plate System)
- [`KitchenObject.cs`](file:///d:/unity/My%20project/Assets/Scripts/KitchenObject.cs): จัดการการเกิด (Spawn), ย้ายตำแหน่งพาเรนต์ (SetKitchenObjectParent), และทำลายตัวเอง (DestroySelf)
- [`PlateKitchenObject.cs`](file:///d:/unity/My%20project/Assets/Scripts/PlateKitchenObject.cs): คลาสพิเศษของวัตถุดิบจาน จัดการรายการ `validKitchenObjectSOList` ที่อนุญาตให้วางลงบนจานได้ และส่งอีเวนต์ `OnIngredientAdded`
- [`PlateCompleteVisual.cs`](file:///d:/unity/My%20project/Assets/Scripts/PlateCompleteVisual.cs): เปิด/ปิดชิ้นส่วน 3D Mesh ของอาหารบนจานตามรายการวัตถุดิบที่ถูกใส่เข้ามาจริง

### 2.4 ระบบข้อมูลและสูตรอาหาร (ScriptableObjects Data Pipeline)
- [`KitchenObjectSO.cs`](file:///d:/unity/My%20project/Assets/Scripts/ScriptableObjects/KitchenObjectSO.cs): เก็บ Prefab, ไอคอน Sprite และชื่อของวัตถุดิบ
- [`CuttingRecipeSO.cs`](file:///d:/unity/My%20project/Assets/Scripts/ScriptableObjects/CuttingRecipeSO.cs): กำหนดคู่ Input ➔ Output และจำนวนครั้งที่ต้องหั่น
- [`FryingRecipeSO.cs`](file:///d:/unity/My%20project/Assets/Scripts/ScriptableObjects/FryingRecipeSO.cs): กำหนดคู่ Input ➔ Output และเวลาในการทอดจนสุก
- [`BurningRecipeSO.cs`](file:///d:/unity/My%20project/Assets/Scripts/ScriptableObjects/BurningRecipeSO.cs): กำหนดคู่ Input (ของสุก) ➔ Output (ของไหม้) และเวลาจนกว่าจะไหม้
- [`RecipeSO.cs`](file:///d:/unity/My%20project/Assets/Scripts/ScriptableObjects/RecipeSO.cs): นิยามสูตรอาหารสำเร็จ โดยระบุรายการ `List<KitchenObjectSO>` ที่จำเป็นต้องมีบนจาน
- [`RecipeListSO.cs`](file:///d:/unity/My%20project/Assets/Scripts/ScriptableObjects/RecipeListSO.cs): รวมรายการสูตรอาหารทั้งหมดเพื่อนำไปสุ่มออเดอร์ในเกม

### 2.5 ระบบโฟลว์เกมและคิวออเดอร์ (Game Flow & Delivery Manager)
- [`KitchenGameManager.cs`](file:///d:/unity/My%20project/Assets/Scripts/KitchenGameManager.cs): ควบคุม State Machine ของเกม:
  - `WaitingToStart` (1s) ➔ `CountdownToStart` (5s) ➔ `GamePlaying` (80s) ➔ `GameOver`
  - รองรับการหยุดเกมชั่วคราว (`TogglePauseGame()`) ด้วย `Time.timeScale`
- [`DeliveryManager.cs`](file:///d:/unity/My%20project/Assets/Scripts/DeliveryManager.cs):
  - สุ่มสร้างออเดอร์อาหารใหม่ทุกๆ 4 วินาที (เก็บในคิวได้สูงสุด 4 ออเดอร์)
  - ฟังก์ชัน `DeliveryRecipe(PlateKitchenObject)`: ตรวจสอบวัตถุดิบบนจานเทียบกับออเดอร์ในคิวแบบ Cross-Match โดยไม่สนใจลำดับการวาง
  - บันทึกจำนวนครั้งที่ส่งอาหารสำเร็จ (`successfulRecipesAmount`)
- [`SoundManager.cs`](file:///d:/unity/My%20project/Assets/Scripts/SoundManager.cs): ดักฟังอีเวนต์จากทุกระบบในเกมเพื่อเล่นเสียง SFX จาก [`AudioClipRefsSO.cs`](file:///d:/unity/My%20project/Assets/Scripts/ScriptableObjects/AudioClipRefsSO.cs)

### 2.6 ระบบอินเทอร์เฟซผู้ใช้ (User Interface)
- **Screen-Space UI:**
  - [`MainMenuUI.cs`](file:///d:/unity/My%20project/Assets/Scripts/UI/MainMenuUI.cs): ปุ่ม Play (โหลดไป `GameScene`) และปุ่ม Quit
  - [`GameStartCountdownUI.cs`](file:///d:/unity/My%20project/Assets/Scripts/UI/GameStartCountdownUI.cs): ตัวเลขนับถอยหลัง 3, 2, 1 ก่อนเริ่มเล่น
  - [`GamePlayingClockUI.cs`](file:///d:/unity/My%20project/Assets/Scripts/UI/GamePlayingClockUI.cs): หลอดวงกลมแสดงเวลาเล่นที่เหลือ
  - [`GamePauseUI.cs`](file:///d:/unity/My%20project/Assets/Scripts/UI/GamePauseUI.cs): เมนูป๊อปอัปเมื่อกด Pause (Resume, Main Menu)
  - [`GameOverUI.cs`](file:///d:/unity/My%20project/Assets/Scripts/UI/GameOverUI.cs): แสดงจำนวนออเดอร์ที่ส่งสำเร็จเมื่อจบเกม
  - [`DeliveryManagerUI.cs`](file:///d:/unity/My%20project/Assets/Scripts/UI/DeliveryManagerUI.cs) & [`DeliveryManagerSingleUI.cs`](file:///d:/unity/My%20project/Assets/Scripts/UI/DeliveryManagerSingleUI.cs): แสดงรายการการ์ดออเดอร์อาหารที่กำลังรอเสิร์ฟ
  - [`PlayerDeliveryResultUI.cs`](file:///d:/unity/My%20project/Assets/Scripts/UI/PlayerDeliveryResultUI.cs): แสดงผลป๊อปอัปแจ้งผลการส่งสำเร็จหรือล้มเหลว
- **World-Space UI:**
  - [`ProgressBarUI.cs`](file:///d:/unity/My%20project/Assets/Scripts/UI/ProgressBarUI.cs): แถบพลังลอยบนเคาน์เตอร์หั่นและเตาอบ
  - [`PlateIconsUI.cs`](file:///d:/unity/My%20project/Assets/Scripts/UI/PlateIconsUI.cs) & [`PlateIconsSingleUI.cs`](file:///d:/unity/My%20project/Assets/Scripts/UI/PlateIconsSingleUI.cs): แสดงไอคอนของวัตถุดิบที่อยู่บนจานแบบลอยเหนือจาน
  - [`LookAtCamera.cs`](file:///d:/unity/My%20project/Assets/Scripts/LookAtCamera.cs): คำนวณมุมหันของ World Space Canvas ให้หันหน้าตรงเข้าหากล้องตลอดเวลา (Billboard Effect)

---

## 📂 3. โครงสร้างโฟลเดอร์และไฟล์สำคัญในโปรเจกต์ (Project Directory Structure)

```text
CaDaCook (Unity Project)/
│
├── 📂 Assets/                               # โฟลเดอร์เก็บ Assets และซอร์สโค้ดทั้งหมดของเกม
│   │
│   ├── 📂 Scenes/                           # ซีนทั้งหมดของเกม
│   │   ├── 🎮 MainMenuScene.unity           # หน้าเมนูหลัก (Index 0)
│   │   ├── 🎮 GameScene.unity               # หน้าเล่นเกมห้องครัวหลัก (Index 1)
│   │   └── 🎮 LoadingScene.unity            # ซีนคั่นสำหรับโหลดฉาก (Index 2)
│   │
│   ├── 📂 Scripts/                          # ซอร์สโค้ด C# ทั้งหมดของเกม
│   │   ├── 📂 Counters/                     # คลาสระบบเคาน์เตอร์ครัวทั้งหมด
│   │   │   ├── 📜 BaseCounter.cs            # คลาสแม่ของเคาน์เตอร์ทุกชนิด (IKitchenObjectParent)
│   │   │   ├── 📜 ClearCounter.cs           # เคาน์เตอร์ว่างวางของอเนกประสงค์
│   │   │   ├── 📜 ContainerCounter.cs       # เคาน์เตอร์หยิบวัตถุดิบ
│   │   │   ├── 📜 ContainerCounterVisual.cs # แอนิเมชันเปิดตู้หยิบของ
│   │   │   ├── 📜 CuttingCounter.cs         # เคาน์เตอร์เขียงหั่นอาหาร (IHasProgress)
│   │   │   ├── 📜 CuttingCounterVisual.cs   # แอนิเมชันมีดหั่น
│   │   │   ├── 📜 DeliveryCounter.cs        # เคาน์เตอร์ส่งมอบอาหาร
│   │   │   ├── 📜 PlatesCounter.cs          # เคาน์เตอร์สร้างจานเปล่า
│   │   │   ├── 📜 PlatesCounterVisual.cs    # แสดงโมเดลจานเรียงซ้อนกัน
│   │   │   ├── 📜 StoveCounter.cs           # เคาน์เตอร์เตาอบทอดอาหาร (FSM 4 สเตต)
│   │   │   ├── 📜 StoveCounterSound.cs      # เสียงฉ่าและเสียงเตือนกระทะ
│   │   │   ├── 📜 StoveCounterVisual.cs     # เอฟเฟกต์ไฟและควันกระทะ
│   │   │   └── 📜 TrashCounter.cs           # เคาน์เตอร์ถังขยะ
│   │   │
│   │   ├── 📂 ScriptableObjects/            # Data Containers สำหรับสูตรอาหารและเสียง
│   │   │   ├── 📜 AudioClipRefsSO.cs        # รวม AudioClip อ้างอิง
│   │   │   ├── 📜 BurningRecipeSO.cs        # นิยามสูตรอาหารไหม้
│   │   │   ├── 📜 CuttingRecipeSO.cs        # นิยามสูตรการหั่น
│   │   │   ├── 📜 FryingRecipeSO.cs         # นิยามสูตรการทอด
│   │   │   ├── 📜 KitchenObjectSO.cs        # นิยามข้อมูลวัตถุดิบ
│   │   │   ├── 📜 RecipeListSO.cs           # รวมรายการสูตรอาหารทั้งหมด
│   │   │   └── 📜 RecipeSO.cs               # นิยามสูตรอาหารสำเร็จ
│   │   │
│   │   ├── 📂 UI/                           # ระบบอินเทอร์เฟซผู้ใช้ทั้งหมด
│   │   │   ├── 📜 DeliveryManagerSingleUI.cs # การ์ดแสดงออเดอร์เดี่ยว
│   │   │   ├── 📜 DeliveryManagerUI.cs      # คอนเทนเนอร์รายการออเดอร์
│   │   │   ├── 📜 GameOverUI.cs             # หน้าต่างสรุปผลเมื่อจบเกม
│   │   │   ├── 📜 GamePauseUI.cs            # เมนูป๊อปอัปหยุดเกม
│   │   │   ├── 📜 GamePlayingClockUI.cs     # นาฬิกาจับเวลาถอยหลัง
│   │   │   ├── 📜 GameStartCountdownUI.cs   # ตัวเลขนับถอยหลัง 3 2 1
│   │   │   ├── 📜 MainMenuUI.cs             # ปุ่มเมนูหน้าแรก
│   │   │   ├── 📜 PlateIconsSingleUI.cs     # ไอคอนเดี่ยวของวัตถุดิบบนจาน
│   │   │   ├── 📜 PlateIconsUI.cs           # คอนเทนเนอร์ไอคอนบนจาน
│   │   │   ├── 📜 PlayerDeliveryResultUI.cs # ป๊อปอัปแจ้งผลการส่ง
│   │   │   └── 📜 ProgressBarUI.cs          # แถบ Progress Bar แบบ World-Space
│   │   │
│   │   ├── 📜 DeliveryManager.cs            # ตัวจัดการคิวออเดอร์และระบบคะแนน (Singleton)
│   │   ├── 📜 GameInput.cs                  # รับอินพุตจาก Unity Input System (Singleton)
│   │   ├── 📜 IHasProgress.cs               # อินเทอร์เฟซสำหรับเคาน์เตอร์ที่มีหลอด Progress
│   │   ├── 📜 IKitchenObjectParent.cs       # อินเทอร์เฟซสำหรับวัตถุที่สามารถถือหรือวางของได้
│   │   ├── 📜 KitchenGameManager.cs         # ตัวควบคุมโฟลว์หลักของเกม (Singleton & FSM)
│   │   ├── 📜 KitchenObject.cs              # ตัวแทนวัตถุอาหารที่หยิบจับได้
│   │   ├── 📜 Loader.cs                     # คลาสสแตติกสำหรับเปลี่ยน Scene พร้อมหน้า Loading
│   │   ├── 📜 LoaderCallback.cs             # คอลแบ็กสั่งโหลดฉากเป้าหมาย
│   │   ├── 📜 LookAtCamera.cs               # สคริปต์หมุน World Space Canvas เข้าหากล้อง
│   │   ├── 📜 PlateCompleteVisual.cs        # ควบคุมการแสดงผล 3D Mesh บนจาน
│   │   ├── 📜 PlateKitchenObject.cs         # จัดการรายการวัตถุดิบบนจาน
│   │   ├── 📜 Player.cs                     # ตัวควบคุมตัวละครและปฏิสัมพันธ์ (Singleton)
│   │   ├── 📜 PlayerAnimator.cs             # ส่งสถานะการเดินเข้า Animator
│   │   ├── 📜 PlayerSounds.cs               # ควบคุมเสียงฝีเท้าตัวละคร
│   │   ├── 📜 SelectedCounterVisual.cs      # ไฮไลท์เรืองแสงรอบเคาน์เตอร์ที่เลือก
│   │   └── 📜 SoundManager.cs               # ตัวจัดการระบบเสียง SFX ทั้งหมด (Singleton)
│   │
│   ├── 📂 Prefabs/                          # Prefabs ของตัวละคร, เคาน์เตอร์, วัตถุดิบ, UI
│   ├── 📂 Settings/                         # การตั้งค่า URP Asset และ Graphics Settings
│   ├── 📜 InputSystem_Actions.inputactions  # ไฟล์นิยามการตั้งค่า Input Actions
│   └── 📜 PlayerInputActions.cs             # โค้ด C# ที่ Unity Input System สร้างขึ้นอัตโนมัติ
│
├── 📂 Packages/                             # การตั้งค่าแพ็กเกจและ Dependencies
│   └── 📜 manifest.json                     # รายการแพ็กเกจ Unity 6 (URP, Input System, Cinemachine ฯลฯ)
│
├── 📂 ProjectSettings/                      # การตั้งค่าโปรเจกต์ของ Unity Editor
│   ├── 📜 ProjectVersion.txt                # ระบุเวอร์ชัน Unity Editor (6000.3.6f1)
│   ├── 📜 EditorBuildSettings.asset         # ลำดับฉากในการ Build
│   └── 📜 ProjectSettings.asset             # ข้อมูลชื่อโปรเจกต์และการตั้งค่าทั่วไป
│
├── 📂 markdowns/                            # เอกสารและคู่มือมาตรฐานของโปรเจกต์
│   ├── 📜 AboutProject.md                   # คู่มือสรุปโครงสร้างและระบบทั้งหมดของเกม (ไฟล์นี้)
│   ├── 📜 PROJECT.md                        # ภาพรวมเป้าหมายและฟีเจอร์หลักของเกม
│   ├── 📜 TECHSTACK.md                      # สรุปเวอร์ชันและเทคโนโลยีทั้งหมดของ Unity 6
│   ├── 📜 DESIGN.md                         # Game Design Document และระบบสถาปัตยกรรม UI
│   ├── 📜 REFACTORCODE.md                   # กฎเหล็ก 15 ข้อสำหรับการเขียน C# ใน Unity
│   ├── 📜 DEBUG.md                          # คู่มือการดีแบ๊กและแก้ปัญหาบัคใน Unity สำหรับ AI Agents
│   ├── 📜 SECURITY.md                       # มาตรฐานความปลอดภัยและข้อกำหนดของโปรเจกต์
│   ├── 📜 LOG.md                            # บันทึกประวัติการพัฒนาและปรับปรุงระบบ
│   ├── 📜 CSharpCodingGuide.md              # คู่มือมาตรฐานการเขียน C# ใน Unity
│   └── 📜 DeMorgansLaws.md                  # คู่มือ De Morgan's Laws สำหรับตรรกะ Boolean
│
└── 📜 .antigravityignore                    # ไฟล์กำหนดการข้ามไฟล์/โฟลเดอร์สำหรับ AI Agents
```