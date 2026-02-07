# Battle System Design Document: World-Domination-By-Silver-Tongue

**Version:** 1.1
**Context:** Complementary to Main GDD
**Focus:** Battle Logic, Persuasion Engine, and Data Structure

## Part 1. Battle Overview

**Concept:** "Kuchi-Kenka" (Verbal Warfare).
Instead of physical damage, the player inflicts **Sanity Damage** using **Logic** (Evidence) and **Rhetoric** (Strategy).

**Core Loop:**

1. **Preparation:** Select character & Equippable Evidence.
2. **Strategy Input:** Player types a free-form instruction (e.g., "Attack his logic about 5G").
3. **Execution (Auto-Battle):**
* **Actor (LLM):** Generates dialogue based on strategy + evidence.
* **Judge (LLM):** Evaluates the dialogue to calculate damage.


4. **Result:** Enemy Sanity  0 (Win) or Player triggers a "Lose Condition".

## Part 2. Updated Folder Structure

To maintain cohesion between the Game Logic and the AI integration, all battle-related scripts are consolidated.

**Scenes:** `Scenes/BattleScene`

**Scripts:** `Scripts/BattleSystem/`

* `Scripts/BattleSystem/BattleManager.cs` (Controls the turn flow)
* `Scripts/BattleSystem/AgentLLM.cs` (Handles API calls to `gemini-3-flash-preview`)
* `Scripts/BattleSystem/JudgeSystem.cs` (Evaluates win/loss conditions)
* `Scripts/BattleSystem/LogManager.cs` (Handles JSONL logging)
* `Scripts/BattleSystem/UI/` (Canvas controllers: `BattleCanvasController.cs`, etc.)

**Prefabs:**

* `Prefabs/BattleScene/BattlerSelectingCanvas`
* `Prefabs/BattleScene/StrategySelectingCanvas`
* `Prefabs/BattleScene/BattleCanvas`
* `Prefabs/BattleScene/BattleResultCanvas`

**Resources:**

* `Resources/Data/Characters/` (ScriptableObjects)
* `Resources/Data/Items/` (ScriptableObjects)

## Part 3. The Persuasion Engine (Architecture)

The battle is driven by two distinct LLM personas operating in `Scripts/BattleSystem/`.

### 3.1 The Actor (AgentLLM.cs)

Generates the actual dialogue text for the game.

* **Input:** System Prompt (Character Lore) + User Strategy + Evidence Injection + Dialogue History.
* **Output:** JSON containing `{ "emotion": string, "text": string }`.

### 3.2 The Judge (JudgeSystem.cs)

Acts as the Game Engine/Referee.

* **Input:** Full Dialogue History + Enemy Weakness Checkpoints.
* **Task:** It does *not* generate dialogue. It strictly analyzes logic.
* **Output:** JSON containing Damage Values and Status Effects.

## Part 4. Data Specifications

### 4.1 Character ScriptableObject

*Located in `Scripts/BattleSystem/Data/*`

| Field | Type | Description |
| --- | --- | --- |
| **CharacterName** | `string` | Display name. |
| **SystemPromptLore** | `TextArea` | The "Soul" of the character (Personality, Secrets, Speech Style). |
| **LoseConditions** | `string[]` | Specific phrases or actions that cause instant defeat (e.g., "Admitting defeat"). |
| **ThinkingEffort** | `Enum` | `Low` (Mob), `Medium` (Elite), `High` (Boss) - controls token limit. |

### 4.2 Item ScriptableObject (Evidence)

All items are factual "Evidence" used to construct arguments.

| Field | Type | Description |
| --- | --- | --- |
| **ID** | `string` | Unique key (e.g., `manual_1998`). |
| **DisplayName** | `string` | UI Name (e.g., "Old Maintenance Manual"). |
| **Icon** | `Sprite` | Inventory visual. |
| **FactInjection** | `TextArea` | **The Core Mechanic.** This text is blindly injected into the LLM context when equipped. |

## Part 5. Prompts & Game Data (Ground Truth)

The following data is hardcoded or loaded into the ScriptableObjects.

### 5.1 Worldview & Rules (Common Prompt)

> **Setting:** Neo-Tokyo. A city of "Toxic Positivity" and "Efficiency."
> **The Law:** "Persuasion Battle" (Kuchi-Kenka). Losing a debate creates a binding contract to concede.
> **Theme:** B-grade Comedy + Social Satire.

### 5.2 Evidence Database (Items)

**Category: LOGIC (Attack)**
*Used to clear specific logical checkpoints.*

* **ID:** `manual`
* **Name:** 📜 1998 Old Manual
* **Injection:** `[ITEM: Old Maintenance Manual] FACT: A dusty manual. States the train was built in 1998 using analog vacuum tubes. Cannot support 5G.`


* **ID:** `radio`
* **Name:** 📻 Portable Radio
* **Injection:** `[ITEM: Portable Radio] FACT: When turned on, it only plays static or Trot music. No strange signals detected.`


* **ID:** `inspection_report`
* **Name:** 📑 Failed Safety Report
* **Injection:** `[ITEM: Failed Report] FACT: Document proves safety checks failed due to budget cuts, not a conspiracy.`



**Category: EMOTION (Critical Hit)**
*Used to bypass logic and attack the human element.*

* **ID:** `mom_letter`
* **Name:** 📩 Mom's Letter
* **Injection:** `[ITEM: Mom's Letter] FACT: A crumpled letter from home. "My son... please come home. We miss you." (Triggers Guilt).`


* **ID:** `dismissal_notice`
* **Name:** 📄 Dismissal Notice
* **Injection:** `[ITEM: Dismissal Notice] FACT: Official email stating you were fired by an AI emoji. You are a victim of the system, not an agent.`



**Category: TRAP (Self-Damage)**
*High risk items that can backfire.*

* **ID:** `smartphone`
* **Name:** 📱 5G Smartphone
* **Injection:** `[ITEM: 5G Smartphone] FACT: Your phone has full 5G bars and is receiving cloud notifications.`
* **Risk:** Enemy points out **Hypocrisy** (You use 5G too!).


* **ID:** `coupon`
* **Name:** 🎫 500-won Coupon
* **Injection:** `[ITEM: 500-won Coupon] FACT: Expired beef bowl coupon. It is your most valuable possession.`
* **Risk:** Enemy mocks your **Poverty/Worthlessness**.



### 5.3 Character Prompts

**Protagonist: Kenta Yabuno**

```text
**Role:** You are Yabuno Kenta (The Protagonist).
**Archetype:** "Country Bumpkin in Tokyo" (Kappe).
**Age:** 24. Unemployed.

**[VISIBLE INFO]:**
- Fashion: Green jersey tucked into suit pants. Cheap sneakers.
- Vibe: Desperate, loud, sweaty.
- Item: Clutches a crumpled 'Beef Bowl Coupon' like it's gold.

**[HIDDEN INFO]:**
- Secret: Fired via a LINE stamp ("You're Fired! ⭐") by an AI.
- Fear: Terrified of returning to the village as a failure.
- Strength: Infinite stamina (Shameless).

**Speech Style:**
- Heavy Country Dialect (ends sentences with "~dabe", "~zura").
- Calls opponent "Oji-san" (Old man) or "Weirdo".

**Directives:**
1. Speak ONLY 1-2 sentences.
2. Read the [DIALOGUE HISTORY].
3. STRICTLY follow the [PLAYER STRATEGY].
4. Use [EQUIPPED ITEMS] to prove your point.

```

**Judge System (The Arbiter)**

```text
**Role:** Game Engine & Referee.
**Task:** Analyze debate, calculate 'Sanity Damage', and check 'Victory/Defeat Conditions'.

**INPUT:**
[DIALOGUE HISTORY]
{history}

**CURRENT OBJECTIVES (UNCLEARED):**
{uncleared_checkpoints}

**EVALUATION RULES:**
1. **LOGIC CHECK:** Does Kenta's argument + Evidence directly satisfy a Current Objective?
   - IF YES: Output `cleared_checkpoint_id`.
2. **RISK CHECK:** Did the Prophet successfully trap Kenta (Hypocrisy/Worthlessness)?
   - IF YES: Output `triggered_defeat_id`.

**SCORING:**
- Ineffective (-0): Insults without logic.
- Normal Hit (-20): Logical argument.
- Critical Hit (-50): Emotional Item (Mom's Letter).
- Trap Trigger (+20 Heal to Enemy): Kenta falls for a trap.

**OUTPUT JSON ONLY:**
{
  "reasoning": "Step-by-step analysis.",
  "damage_type": "Ineffective" | "Normal Hit" | "Critical Hit" | "Trap Trigger",
  "damage_dealt": [Integer],
  "prophet_current_sanity": [Calculated Sanity],
  "status": "ONGOING" | "KENTA_WINS" | "PROPHET_WINS"
}

```

## Part 6. UI/UX Specifications

All UI logic is handled by scripts in `Scripts/BattleSystem/UI/`.

1. **BattlerSelectingCanvas:**
* Scrollable list of characters.
* Selection instantiates the `AgentLLM` with the chosen character's `SystemPromptLore`.


2. **StrategySelectingCanvas:**
* **Input Field:** "Strategy Text" (e.g., "Attack his delusion").
* **Inventory Grid:** Drag & Drop `ItemScriptableObject` into the "Evidence Slot".
* **Effect:** This combines `{Strategy Text} + {Item.FactInjection}` into the prompt for the next turn.


3. **BattleCanvas:**
* **Visuals:** 1v1 View. Characters darken when not speaking.
* **Dialogue Box:** Updates dynamically with streaming text (if supported) or full text.
* **Sanity Bar:** Updates based on `JudgeSystem` response.


4. **BattleResultCanvas:**
* Simple overlay.
* **Win:** Shows "CONFIRMED: PROPHET BROKEN".
* **Lose:** Shows "CONTRACT SIGNED: YOU LOSE".