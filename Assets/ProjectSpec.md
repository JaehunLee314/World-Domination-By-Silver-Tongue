# Game Design Document: World-Domination-By-Silver-Tongue

## Part 1. Overview

### 1.1 Project Context

**Project Name:** World-Domination-By-Silver-Tongue  
**Genre:** Lighthearted JRPG-parody / Persuasion-based Combat  
**Concept:** A comedic take on the "Talk-no-Jutsu" trope. The protagonist eschews physical weaponry in favor of a mix of **real-world persuasion tactics** and **absurd anime logic** to convert hostile enemies into loyal allies (*Nakama*).

### 1.2 Tech Stack

**Engine:** Unity 6 (Version: 6000.3.6f1)  
**LLM Integration:** gemini-3-flash-preview

- **Data Handling:** ScriptableObjects for characters and items; JSONL for turn-based logging.

### 1.3 Core Gameplay Loop

The game operates on a structured ssequence of exploration, preparation, and verbal combat:

1. **Start Scene:** The initial entry point.
2. **Info Gathering Scene:** Exploration and evidence collection via movement and interaction.
3. **Battle Scene:** A unified scene containing multiple canvas-based UI phases:
   - **BattlerSelectingCanvas:** Choosing the active negotiator based on character profiles.
   - **StrategySelectingCanvas:** Mapping out the debate agenda, with an option to loop back to Battler Selection.
   - **BattleCanvas:** 1v1 AI-driven auto-battle utilizing gathered evidence and skills.
   - **BattleResultCanvas:** Outcome evaluation and reward processing.
4. **Loop/Ending:** Return to gathering or proceed to the final conclusion.

## Part 2. Folder Structure

To ensure a highly organized LLM-readable project, the directory must strictly follow this hierarchical structure:

**Scenes:** `Scene/<scene-name>`

**Scripts:**

- Scene-specific: `Scripts/<scene-name>/<script-name>`
- General functions: `Scripts/<general-function-name>/<script-name>` (e.g., `Scripts/LLM/ILLM.cs`)

**Prefabs:**

- Scene-specific: `Prefabs/<scene-name>/<prefab-name>`
- Canvas prefabs for BattleScene: `Prefabs/BattleScene/<canvas-name>Canvas` (e.g., `BattlerSelectingCanvas`, `StrategySelectingCanvas`, `BattleCanvas`, `BattleResultCanvas`)
- General functions: `Prefabs/<general-function-name>/<prefab-name>`

**Resources/Assets:** Apply similar logic for `Materials/`, `Textures/`, and `Audio/`.

## Part 3. UI Specifications

### 3.1 Start Scene

**Visuals:** A full-screen background image.  
**Title:** Positioned at the screen's center.  
**Interaction:** A "Start" button located directly below the title text to trigger gameplay.

### 3.2 Info Gathering Scene [NOT THE SCOPE OF IMPLEMENTATION]

**Environment:** Visual space constructed with 2D assets and a skybox.  
**Navigation:** Movement is handled via **WASD** keys on floor assets.  
**Interaction:**

- **Items:** Users can collect items from various locations in the environment.
- **NPCs:** Clicking a character enables interaction and displays dialogue text.

### 3.3 Inventory Prefab

**Structure:** A grid-like overlay containing multiple item images.  
**Item Cards:** Each cell contains an item image with its name bounded below it.  
**Popup Details:** Selecting an item triggers a popup with:

- The item image.
- Item Type (Evidence or Skill Book).
- Detailed Description.

**Close Button:** Located at the bottom middle.  
**Visual Effect:** The surrounding area of the popup must be blurred.  
**Functionality:** Supports item filtering in the controller code. Close buttons for the main inventory are at the bottom.

### 3.4 ConversationHistoryCanvas Prefab

**Structure:** A scrollable overlay displaying conversation history.
**History Entries:** Each entry shows:

- Speaker name (left-aligned for player, right-aligned for opponent).
- Speech text with timestamp.
- Optional indicators for evidence/skills used.

**Visual Layout:** Chat-bubble style with alternating alignment based on speaker.  
**Close Button:** Located at the bottom middle.  
**Visual Effect:** The surrounding area of the popup must be blurred.  
**Functionality:** Displays the full turn-by-turn dialogue from the current battle session.

### 3.5 BattlerSelectingCanvas (within BattleScene)

**UI Layout:** A horizontally scrollable table for choosing participants.  
**Columns per Character:**

- **Top 2/3:** Character profile image.
- **Bottom 1/3:** Name, Personality, Intelligence, and must-lose conditions (typically 3).

**Interaction:** A selection button associated with each character entry at the bottom.  
**Verification:** Choosing a character triggers a confirmation popup.

### 3.6 StrategySelectingCanvas (within BattleScene)

**Top Bar:** A button to return to character selection (top right), a turn counter (e.g., Turn 3/7) at top middle, and a log button (top left) to open the conversation history.  
**Middle Section:** Displays the user's selected character (left) and opponent (right). Clicking a character toggles their must-lose conditions.  
**Bottom Section (Split):**

- **Left (Agenda):** Three vertical slots formatted as `#N [Point Text (≤ 30 words)] [Skill Placeholder] [Item Placeholder]`.
- **Right (Inventory):** A version of the inventory prefab with skill and item filters.

**Interaction:** Placeholders support **Drag & Drop** from the inventory.

### 3.7 BattleCanvas (within BattleScene)

**Top UI:** Turn tracker (top middle), a pause button below it, and a log button (top left) to open the conversation history.  
*Note:* Pausing must nullify any ongoing LLM calls.  
**Stage Area:** User character (left) and opponent (right).  
**Animation:** Characters move to the middle when speaking and return after finishing.  
**Visual State:** While not talking, characters should be darkened.  
**Dialogue Area:** Bottom 1/3 of the screen displaying speaker name and speech text.  
**Action:** A text field is provided for specific player-driven actions.

### 3.8 BattleResultCanvas & Ending

**BattleResultCanvas (within BattleScene):** Displays win/lose text at the top and a close button at the bottom to transition to the next info-gathering phase.  
**Ending Scene:** Features final messages, a prominent "Thank You" text, and a close button to exit the game.

## Part 4. Data & Architecture

### 4.1 Character ScriptableObject

**Identity:** Name and profile image.  
**Stats:** Personality traits and intelligence levels (determines LLM `thinking_effort`).  
**Lore:** `SystemPromptLore` (Personality, secrets, and speaking style).  
**Voice Tone:** String descriptors (e.g., "Arrogant," "Shy").  
**Conditions:** `LoseConditionRule`: Phrases or behaviors that cause immediate failure. Regular expression or validation function name.

### 4.2 Item ScriptableObject

**Non-Skill (Evidence):**

- `ID` (string) for identification.
- `Description` of the fact or object.

**Skill (Skill Book):**

- `SkillName` (e.g., "Tsundere Logic" or "Logical Deduction").
- `PromptModifier`: Text injected into the LLM context to steer persuasion style.
- `ThinkingEffort`: Determines inference depth (Low, Medium, High).

### 4.3 The Persuasion Engine (AI Logic)

**AgentLLM Class:** Constructs payloads for `gemini-3-flash-preview` and parses responses.  
**Payload Components:** Role definition, world state, character lore, gameplay rules, and skill/evidence injections.  
**Output Format:** Requires `[Thought Process] Dialogue <tags>` (e.g., `<evidence_used=ID>`).  
**The Judge:**

- **Fast Check:** Local Regex/rule-based check for win/lose conditions every turn.
- **Final Verdict:** An LLM call reviews the full history to confirm if persuasion was successful based on context (Win/Lose/Draw).

## Part 5. Technical Implementation Rules

**Management:**

- `GameManager`: Persistent singleton for overall state and user items.
- `SceneManager`: Handles logic for each specific scene.
- `Controller`: Used for UI-specific controlling code (e.g., `InventoryController`).

**View-Controller Pattern:** Managers must use "View" scripts as an abstraction layer for UI actions rather than accessing UI elements directly.

**UI Constraints:**

- **No Code-Based Generation:** All UI must be created from **Prefabs**.
- **Default Transforms:** Use standard transforms; unconventional transforms are prohibited unless absolutely necessary.

**Logging:** Turn-by-turn `jsonl` logs saved to reasonable path to capture full context prompts and raw LLM responses for debugging and optimization.