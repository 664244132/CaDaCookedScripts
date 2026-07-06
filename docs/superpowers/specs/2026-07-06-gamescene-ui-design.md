# GameScene UX/UI Design Spec

## Goal
Improve the UX/UI of the GameScene to look professional, cute, and unobstructive, ensuring it does not block the gameplay area.

## Global Style & Theme
- **Color Palette:** Orange (#FFA500 or similar vibrant orange) and White (#FFFFFF). White is used as the primary background for cleanliness, while Orange is used for accents, outlines, and highlights.
- **Shapes:** Rounded corners for all UI panels and buttons to emphasize a "cute" and friendly feel. No sharp edges.
- **Typography:** TextMeshPro with bold font style. Orange for headings/titles, white for button text.

## Layout (The Minimalist Edge)
To maximize the central gameplay area, all main HUD elements are anchored to the edges.

### 1. DeliveryManagerUI (Orders)
- **Position:** Top-Left corner, anchored to the top-left edge.
- **Design:** Small white cards stacking vertically downwards. Each card has rounded corners, a thin orange outline, and uses orange icons for food items.
- **Behavior:** Scales down slightly from the previous design to save screen space.

### 2. GamePlayingClockUI (Timer)
- **Position:** Top-Right corner, anchored to the top-right edge.
- **Design:** A horizontal capsule shape. White background with a vibrant orange fill that depletes from right to left as time runs out.

### 3. World-Space UI (Progress Bars & Plate Icons)
- **Position:** Floating above interactable objects (stoves, cutting boards, plates).
- **Design:** 
  - **Progress Bar:** Miniature size, white background, orange fill.
  - **Plate Icons:** Arranged neatly with a subtle white border. 
- **Goal:** Keep them small enough so they don't block the 3D models or character animations.

### 4. Pop-up Menus (GamePauseUI & GameOverUI)
- **Position:** Center screen.
- **Design:** 
  - Semi-transparent dark overlay covering the background gameplay.
  - Main panel is solid white with rounded corners.
  - Buttons use the same Orange/White theme as the Main Menu (Orange background with white text, or White background with Orange outline and text).

## Next Steps (Implementation)
Once this spec is approved, we will transition to creating a detailed Implementation Plan to modify the Unity Canvas and Prefabs accordingly.
