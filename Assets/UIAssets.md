# UI Assets List

## Shared / Reusable

| Placeholder Name | Type | Where Used |
|---|---|---|
| `ui_button_bg` | 9-slice Sprite | Every button across all scenes (Start, Select, Strategy, Battle, Result, History) |
| `ui_panel_bg` | 9-slice Sprite | CharacterCard bg, popup panel, agenda slot bg, dialogue area bg, chat content panel |
| `ui_input_field_bg` | 9-slice Sprite | Agenda slot point text input fields (x3) |
| `ui_slot_bg` | Sprite | Skill/Item drop target slots in agenda panel (x6 total) |
| `ui_scroll_bg` | 9-slice Sprite | Character selection scroll, inventory scroll, conversation history scroll |
| `ui_chat_bubble_bg` | 9-slice Sprite | Chat bubble background in conversation history view |

## Character Portraits

| Placeholder Name | Where Used |
|---|---|
| `char_<CHARACTER_NAME>` | CharacterCard (selection), Strategy player panel, Battle stage player panel, Confirmation popup |

Assigned via `CharacterSO.profileImage` field. Same sprite reused in all locations for a given character.

## Item & Skill Icons

| Placeholder Name | Item Type | Where Used |
|---|---|---|
| `item_evidence_<EVIDENCE_NAME>` | Evidence | Inventory grid, Agenda slot item drop |
| `item_skill_<SKILL_NAME>` | SkillBook | Inventory grid, Agenda slot skill drop |

Assigned via `ItemSO.itemImage` field. Same sprite reused in inventory and agenda slots.

## Scene Backgrounds

| Placeholder Name | Where Used |
|---|---|
| `bg_start_scene` | Start Scene full-screen background |
| `bg_battler_select` | Battler Selecting Canvas full-screen background |
| `bg_strategy_select` | Strategy Selecting Canvas full-screen background |
| `bg_battle` | Battle Canvas full-screen background |
| `bg_battle_result` | Battle Result Canvas full-screen background |

Currently all solid colors. Optional to replace with illustrated backgrounds.

## Start Scene

| Placeholder Name | Where Used |
|---|---|
| `ui_title_logo` | Title text area (currently TextMeshPro text, could be a logo graphic) |
