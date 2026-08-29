# 🍳 CaDaCook - Project Overview

**CaDaCook** คือเกมแนวทำอาหารและบริหารจัดการครัวแบบ 3D Fast-paced Casual Simulation (สไตล์ Kitchen Chaos / Overcooked) ที่พัฒนาขึ้นบนเอนจิน **Unity 6** ด้วยภาษา **C#** 

ผู้เล่นจะได้รับบทเป็นเชฟในห้องครัวที่ต้องรับคำสั่งซื้ออาหาร (Order) นำวัตถุดิบออกจากตู้ นำไปหั่นบนเขียง ทอดบนเตาไฟ จัดวางลงในจาน และนำไปส่งที่เคาน์เตอร์ส่งอาหารให้ถูกต้องตามสูตรภายในระยะเวลาที่กำหนดเพื่อทำคะแนนสูงสุด

---

## 🎯 เป้าหมายของระบบ (Project Goals)
- **Fast-Paced Gameplay:** สร้างประสบการณ์เกมเพลย์ทำอาหารที่สนุกสนาน ท้าทาย และควบคุมได้อย่างลื่นไหล
- **Clean & Modular Architecture:** ออกแบบโครงสร้างโค้ดด้วยแนวคิด Decoupled Architecture ผ่าน C# Events, Interfaces และ ScriptableObjects
- **Data-Driven Recipe Pipeline:** เพิ่มเติมวัตถุดิบและสูตรอาหารใหม่ได้อย่างง่ายดายผ่าน Unity Inspector โดยไม่ต้องแก้ไขโค้ดหลัก
- **Extensible Counter System:** รองรับการสร้างเคาน์เตอร์รูปแบบใหม่ๆ ผ่านการสืบทอดจาก `BaseCounter` และการเชื่อมต่อกับ Interfaces

---

## 🔄 ลูปการเล่นหลัก (Core Gameplay Loop)
1. **รับออเดอร์ (Receive Orders):** ระบบ `DeliveryManager` สร้างรายการสั่งอาหารแบบสุ่มตามช่วงเวลา (เช่น เบอร์เกอร์, สลัด)
2. **หยิบวัตถุดิบ (Gather Ingredients):** เดินไปหยิบวัตถุดิบ (มะเขือเทศ, กะหล่ำปลี, เนื้อ, ขนมปัง, ชีส) จาก `ContainerCounter`
3. **แปรรูปวัตถุดิบ (Process Ingredients):**
   - นำไปหั่นบน `CuttingCounter` โดยกดปุ่ม Alternate Action ซ้ำๆ จนแถบ Progress เต็ม
   - นำเนื้อไปทอดบน `StoveCounter` รอจนสุก (Fried) และระวังอย่าปล่อยทิ้งไว้นานจนไหม้ (Burned)
4. **จัดวางลงจาน (Assemble on Plate):** นำจานจาก `PlatesCounter` มาใส่ส่วนผสมที่เตรียมไว้เข้าด้วยกัน
5. **ส่งมอบอาหาร (Deliver Order):** นำจานอาหารไปวางที่ `DeliveryCounter` เพื่อตรวจเช็คความถูกต้อง
6. **คำนวณคะแนนและสรุปผล (Score & Game Over):** สะสมจำนวนจานที่ส่งสำเร็จ และสรุปผลเมื่อหมดเวลาการเล่น (`gamePlayingTimerMax = 80s`)

---

## ⭐ ฟีเจอร์หลักของเกม (Core Features)

1. **🧑‍🍳 Player Controller & Interaction System:**
   - ควบคุมการเคลื่อนที่ 3 มิติอย่างนุ่มนวลผ่าน Unity New Input System
   - ระบบ Raycast Interaction ตรวจจับเคาน์เตอร์ตรงหน้า และแสดงกรอบเรืองแสงไฮไลท์ (`SelectedCounterVisual`)
   - ระบบถือและส่งต่อวัตถุดิบ (`IKitchenObjectParent`) ระหว่างผู้เล่นและเคาน์เตอร์

2. **🪑 Modular Kitchen Counters System:**
   - **`ClearCounter`:** เคาน์เตอร์วางพักของอเนกประสงค์ และใช้สำหรับรวมวัตถุดิบเข้ากับจาน
   - **`ContainerCounter`:** เคาน์เตอร์หยิบวัตถุดิบสดใหม่ไม่จำกัดจำนวน
   - **`CuttingCounter`:** เขียงหั่นผัก/เนื้อ มีเกจแสดงความคืบหน้า (`ProgressBarUI`) และแอนิเมชันมีดหั่น
   - **`StoveCounter`:** เตาอบ/กระทะทอด พร้อม Finite State Machine 4 สเตต (`Idle`, `Frying`, `Fried`, `Burned`), เอฟเฟกต์เสียงฉ่า และไฟ/ควันเตือน
   - **`PlatesCounter`:** เคาน์เตอร์สร้างจานใหม่อัตโนมัติทีละใบจนครบขีดจำกัด
   - **`DeliveryCounter`:** เคาน์เตอร์ส่งมอบอาหาร ตรวจสอบสูตรกับ `DeliveryManager`
   - **`TrashCounter`:** ถังขยะสำหรับทิ้งวัตถุดิบที่ไหม้หรือทำผิดพลาด

3. **🥗 Data-Driven Recipe & Ingredient Pipeline:**
   - ใช้วัตถุ `ScriptableObject` กำหนดข้อมูล `KitchenObjectSO`, `RecipeSO`, `CuttingRecipeSO`, `FryingRecipeSO`, `BurningRecipeSO`
   - ระบบตรวจสอบสูตรยืดหยุ่น (Ingredient matching algorithm) ที่ตรวจความถูกต้องของวัตถุดิบบนจานโดยไม่สนใจลำดับการวาง

4. **🖥️ Responsive UI & HUD System:**
   - **World-Space UI:** แถบ Progress Bar ลอยเหนือเคาน์เตอร์ และไอคอนวัตถุดิบบนจาน (`PlateIconsUI`) พร้อมระบบ `LookAtCamera` ปรับมุมมองหันหาผู้เล่นเสมอ
   - **Screen-Space HUD:** นาฬิกาจับเวลาถอยหลัง (`GamePlayingClockUI`), ตัวนับถอยหลังเริ่มเกม (`GameStartCountdownUI`), รายการออเดอร์ที่กำลังรอ (`DeliveryManagerUI`), เมนูพักเกม (`GamePauseUI`), และหน้าสรุปผล (`GameOverUI`)

5. **🔊 Dynamic Sound & Visual FX:**
   - ระบบจัดการเสียง `SoundManager` เล่นเสียงตาม C# Events (เสียงฝีเท้า, เสียงมีดหั่น, เสียงทอดกระทะ, เสียงส่งสำเร็จ/ล้มเหลว, เสียงเตือนไฟไหม้)
