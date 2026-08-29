# 🍳 CaDaCook - Project Overview

**CaDaCook** คือเกมแนวทำอาหารและบริหารจัดการครัวแบบ 3D Fast-paced Casual Simulation (สไตล์ Kitchen Chaos / Overcooked) ที่พัฒนาขึ้นบนเอนจิน **Unity 6 (6000.3.6f1)** ด้วยภาษา **C#** 

ผู้เล่นจะได้รับบทเป็นเชฟในห้องครัวที่ต้องรับคำสั่งซื้ออาหาร (Order) นำวัตถุดิบออกจากตู้ นำไปหั่นบนเขียง ทอดบนเตาไฟ จัดวางลงในจาน และนำไปส่งที่เคาน์เตอร์ส่งอาหารให้ถูกต้องตามสูตรภายในระยะเวลาที่กำหนด พร้อมรับมือกับอุปสรรคและเหตุการณ์ไม่คาดฝันในครัวเพื่อทำคะแนนสูงสุด

---

## 🎯 เป้าหมายของระบบ (Project Goals)
- **Fast-Paced & Emergent Gameplay:** สร้างประสบการณ์เกมเพลย์ทำอาหารที่สนุกสนาน ท้าทาย ควบคุมลื่นไหล และมีเหตุการณ์ไม่คาดฝัน (ไฟไหม้, พื้นลื่น, แมวขโมยของ, เคาน์เตอร์เลื่อน 2 แกน, ชั่วโมงเร่งด่วน)
- **Clean & Modular Architecture:** ออกแบบโครงสร้างโค้ดด้วยแนวคิด Decoupled Architecture ผ่าน C# Events, Interfaces และ ScriptableObjects
- **Data-Driven Recipe Pipeline:** เพิ่มเติมวัตถุดิบและสูตรอาหารใหม่ได้อย่างง่ายดายผ่าน Unity Inspector โดยไม่ต้องแก้ไขโค้ดหลัก
- **Extensible Counter & Obstacle System:** รองรับการสร้างเคาน์เตอร์และอุปสรรครูปแบบใหม่ๆ ผ่านการสืบทอดจาก `BaseCounter` และการเชื่อมต่อกับ Interfaces

---

## 🔄 ลูปการเล่นหลัก (Core Gameplay Loop)
1. **หน้าสอนเล่นและการนับถอยหลัง (How to Play & 15s Countdown):** แสดงคู่มือปุ่มควบคุม วิธีทำอาหาร และกล่องแจ้งเตือนเด่นชัดสีส้มเรื่องแมวขโมยถังดับเพลิง เป็นเวลา 15 วินาที
2. **รับออเดอร์ (Receive Orders):** ระบบ `DeliveryManager` สร้างรายการสั่งอาหารแบบสุ่มตามช่วงเวลา (และถี่ขึ้น 2 เท่าในช่วง Rush Hour)
3. **หยิบวัตถุดิบ (Gather Ingredients):** เดินไปหยิบวัตถุดิบบน `ContainerCounter` ระวังแมว `KitchenCatNPC` แอบมาขโมยของ และระวังเคาน์เตอร์เลื่อนตำแหน่ง
4. **แปรรูปวัตถุดิบ (Process Ingredients):**
   - นำไปหั่นบน `CuttingCounter` โดยกดปุ่ม Alternate Action (`F`) ซ้ำๆ จนแถบ Progress เต็ม
   - นำเนื้อไปทอดบน `StoveCounter` รอจนสุก (Fried) ภายในเวลาที่กำหนด หากปล่อยทิ้งไว้เกิน 3 วินาที เนื้อจะไหม้ (Burned) และเกิดไฟไหม้ `FireHazard` ต้องหยิบถังดับเพลิง `FireExtinguisher` มาฉีดดับ
5. **จัดวางลงจาน (Assemble on Plate):** นำจานจาก `PlatesCounter` มาใส่ส่วนผสมที่เตรียมไว้เข้าด้วยกัน
6. **ส่งมอบอาหาร (Deliver Order):** ระวังเดินเหยียบคราบน้ำมัน `SlipperyFloor` หรือหลุมดัก `PotholeTrap` นำจานไปส่งที่ `DeliveryCounter`
7. **คำนวณคะแนนและสรุปผล (Score & Auto-Return):** แสดงผลสรุปคะแนน และพากลับสู่หน้าเมนูหลักอัตโนมัติใน 5 วินาที

---

## ⭐ ฟีเจอร์หลักของเกม (Core Features)

1. **🧑‍🍳 Player Controller & Interactions:**
   - ควบคุมการเคลื่อนที่ 3 มิติอย่างนุ่มนวลผ่าน Unity New Input System
   - ระบบ Raycast Interaction ตรวจจับเคาน์เตอร์ตรงหน้า และแสดงกรอบเรืองแสงไฮไลท์ (`SelectedCounterVisual`)
   - รองรับสถานะลื่นไถลคราบน้ำมัน (`SetSlipping()`), ชะลอความเร็ว (`ApplySlowEffect()`) และการถือ/พ่นละอองถังดับเพลิง (`[F] HOLD TO SPRAY` / `[E] DROP`)

2. **🪑 Modular Kitchen Counters System:**
   - **`ClearCounter`:** เคาน์เตอร์วางพักของอเนกประสงค์ และใช้สำหรับรวมวัตถุดิบเข้ากับจาน
   - **`ContainerCounter`:** เคาน์เตอร์หยิบวัตถุดิบสดใหม่ไม่จำกัดจำนวน
   - **`CuttingCounter`:** เขียงหั่นผัก/เนื้อ มีเกจแสดงความคืบหน้า (`ProgressBarUI`)
   - **`StoveCounter`:** เตาอบ/กระทะทอด พร้อม Finite State Machine 4 สเตต (`Idle`, `Frying`, `Fried`, `Burned`) และเชื่อมต่อกับระบบไฟไหม้
   - **`PlatesCounter`:** เคาน์เตอร์สร้างจานใหม่อัตโนมัติตามช่วงเวลา
   - **`DeliveryCounter`:** เคาน์เตอร์ส่งมอบอาหาร ตรวจสอบสูตรกับ `DeliveryManager`
   - **`TrashCounter`:** ถังขยะสำหรับทิ้งวัตถุดิบที่ไหม้หรือทำผิดพลาด

3. **🌪️ Dynamic Events & Obstacles (ระบบอุปสรรคและเหตุการณ์ครัว):**
   - **Fire Hazard & Enhanced 3D Extinguisher:** ไฟลุกไหม้เตาเมื่ออาหารไหม้ ถังดับเพลิง 3D สมบูรณ์แบบ (ตัวถังแดง, หัวฉีดเทา, ท่อดำ) พ่นละอองขาวดับเพลิงได้จริง
   - **Dual Moving Counters:** เคาน์เตอร์เคลื่อนที่ไป-กลับ 2 ตัว (ตัวที่ 1 เลื่อนแกน X ซ้าย-ขวา `2.2m`, ตัวที่ 2 เลื่อนแกน Z หน้า-หลัง `1.8m`)
   - **Slippery Floor:** คราบน้ำมันบนพื้นทำให้ตัวละครลื่นไถลและหมุนตัว
   - **Rush Hour Event:** ช่วงเวลาเร่งด่วน ออเดอร์เข้าถี่ 2 เท่า ได้คะแนน 2 เท่า
   - **Kitchen Cat NPC:** แมวป่วนครัวแอบย่องมาขโมยวัตถุดิบบนเคาน์เตอร์
   - **Multi-Theme Maps:** ฉากครัวมาตรฐาน (Cozy Kitchen) และฉากครัวแพริมหาดโยกตามคลื่น (`RaftKitchenTilt`)

4. **🥗 Data-Driven Recipe & Ingredient Pipeline:**
   - ใช้วัตถุ `ScriptableObject` กำหนดข้อมูล `KitchenObjectSO`, `RecipeSO`, `CuttingRecipeSO`, `FryingRecipeSO`, `BurningRecipeSO`
   - ระบบตรวจสอบสูตรยืดหยุ่น (Ingredient matching algorithm) ที่ตรวจความถูกต้องของวัตถุดิบบนจานโดยไม่สนใจลำดับการวาง

5. **🖥️ Responsive UI & HUD System:**
   - **World-Space UI:** แถบ Progress Bar ลอยเหนือเคาน์เตอร์, ป้ายคำสั่งถังดับเพลิง Billboard และไอคอนวัตถุดิบบนจาน (`PlateIconsUI`)
   - **Screen-Space HUD:** หน้าต่างสอนเล่น 15 วินาทีพร้อมกล่องเตือนสีส้ม (`GameStartCountdownUI`), นาฬิกาจับเวลาถอยหลัง (`GamePlayingClockUI`), รายการออเดอร์ (`DeliveryManagerUI`), ป้ายแจ้งเตือนชั่วโมงเร่งด่วน (`RushHourUI`), เมนูพักเกม (`GamePauseUI`), และหน้าสรุปผลที่กลับสู่เมนูอัตโนมัติใน 5 วินาที (`GameOverUI`)
