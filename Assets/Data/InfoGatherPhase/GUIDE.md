# Info Gather Phase - Adding Dialogue & Items

## Creating a Dialogue Event

1. **Right-click** in the Project window inside your desired folder (e.g. `Assets/Data/InfoGatherPhase/`)
2. Select **Create > InfoGather > Dialogue Event**
3. Name it descriptively (e.g. `WebbIntroDialogue`, `LetterPickupDialogue`)
4. Select the new asset and configure it in the Inspector:

### Dialogue Lines

Each entry in the **Lines** array has three fields:

| Field | Purpose | Notes |
|---|---|---|
| **Speaker Name** | Displayed in the name text box | Use `???` for unknown characters, real names once revealed |
| **Text** | The dialogue line shown on screen | Supports multi-line via the text area |
| **Speaker Portrait** | A `Texture2D` shown on the 3D speaker panel | Optional - leave empty (None) if no portrait is needed; the panel will hide |

Click **+** on the Lines array to add entries. Each entry is one "click to advance" beat.

---

## Creating an Item

1. **Right-click** in the Project window > **Create > InfoGather > Item Data**
2. Name it (e.g. `StolenLetter`, `OldPhotograph`)
3. Configure in Inspector:

| Field | Purpose |
|---|---|
| **Item Name** | Internal ID used by GameManager to track collection (must be unique) |
| **Item Type** | `Document`, `Photo`, `Object`, or `Clue` |
| **Icon** | A `Sprite` shown in the inventory grid and detail popup (optional) |
| **Description** | Flavor text describing the item |
| **Pickup Dialogue** | Drag a **Dialogue Event** asset here - plays when the player first clicks the item |

---

## Placing an Item in the Scene

1. Drag the **ClickablePanel** prefab (`Assets/Prefabs/InfoGatherPhase/ClickablePanel.prefab`) into the scene
2. Position it where you want the clickable object to appear
3. Select it and find the **ClickableItem** component in the Inspector
4. Drag your **ItemData** asset into the **Item Data** field
5. Ensure the GameObject is on **Layer 8 ("Clickable")** - the prefab already has this set

The item can only be collected once. Clicking it again after collection does nothing (tracked by GameManager).

---

## Triggering Dialogue on Node Arrival

1. Select a **MovementNode** in the scene
2. In the **MovementNode** component, find the **Arrival Dialogue** field
3. Drag a **Dialogue Event** asset into it

This dialogue plays once each time the player arrives at that node. Movement is automatically locked during dialogue and unlocked when it ends.

---

## CommenceDemo (Auto-Start Dialogue)

The **CommenceDemo** GameObject triggers a dialogue immediately when the scene starts. To use it:

1. **Enable** the CommenceDemo GameObject to have it auto-play on scene start
2. **Disable** it to skip the intro dialogue
3. To change the dialogue, swap the **Demo Dialogue** field on the CommenceDemo component

---

## How Dialogue Flows at Runtime

1. A trigger fires (item click, node arrival, or CommenceDemo)
2. `DialogueManager.StartDialogue()` is called with a DialogueEvent
3. Movement is **locked** (OnDialogueStarted event)
4. The first line's speaker name + text appear in the dialogue panel
5. If the line has a speaker portrait, the 3D SpeakerPanel shows it (billboarded to camera)
6. **Left-click** anywhere advances to the next line
7. After the last line, the dialogue panel hides, the speaker panel hides, and movement **unlocks**

---

## Inventory System

Collected items appear in the **Inventory** panel (button in top-right corner of the screen).

- The inventory button is **disabled during dialogue** (grayed out, not clickable)
- Opening the inventory **locks movement** and blocks panel click raycasts
- The grid shows each collected item's **icon** and **name** in a 4-column layout
- Clicking an item opens a **detail popup** showing the full Name, Type, Icon, and Description
- Press **X** on the detail popup to return to the grid; press **X** on the inventory to return to gameplay

Items are stored in `GameManager` as `ItemData` references (preserving insertion order). The inventory UI reads from `GameManager.Instance.CollectedItems`.

---

## Quick Reference: File Locations

| Asset | Path |
|---|---|
| Dialogue Events | `Assets/Data/InfoGatherPhase/` |
| Item Data | `Assets/Data/InfoGatherPhase/` |
| ClickablePanel prefab | `Assets/Prefabs/InfoGatherPhase/ClickablePanel.prefab` |
| MovementNode prefab | `Assets/Prefabs/InfoGatherPhase/MovementNode.prefab` |
| InventorySlot prefab | `Assets/Prefabs/InfoGatherPhase/InventorySlot.prefab` |
| Scripts | `Assets/Scripts/InfoGatherPhase/` |
