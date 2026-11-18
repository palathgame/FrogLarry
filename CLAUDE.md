# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

"Frog Larry" — a 2D mobile (Android/iOS) visual-programming puzzle game in Unity **6000.3.6f1**. The player drags command blocks (Move, Jump, Left, Right, Eat, Repeater) into slots, presses Play, and the frog executes them on a tile grid to reach the pond, ideally catching every fly within the move budget.

There is no CLI build, lint or test setup and no test assemblies. Builds are made from the Unity Editor (Build Profiles). The `com.unity.pipeline` package is installed: with the Editor open, the `unity-editor-mcp` tools (or the `unity-cli` / `unity-pipeline` skills) can compile, read the console, enter Play Mode, take screenshots and edit scenes. `Assets/post-build.bash` is a Unity Cloud Build hook that uploads the iOS IPA.

All game code lives in `Assets/GameScripts/` (default `Assembly-CSharp`, no asmdefs). Two scenes: `Assets/Scenes/Menu.unity` and `Assets/Scenes/Game.unity`.

## Architecture

**Scene handoff through PlayerPrefs.** The Menu writes `PlayerPrefs["LastLevel"]` (e.g. `"level7"`) and loads `Game`. `GridManagerScript` reads it in `Start`. `ResultScoreScript` sets it to the next level on a win, so "Next" just reloads `Game`. Other PlayerPrefs keys: `Flies`, `FrogSkin`, `FrogSkinOwned_<id>` (shop, see below), `OfferRestart`, `Vibration`, `LevelLayoutVersion`, `GameMusic`, `GameSounds` (volume, read on every frog sound), `SavedVolume`, `Language`, `RemoveDuplicateLevels` (a one-time migration flag).

**Levels are tab-separated text files** in `Assets/Resources/LevelFiles/levelN.txt`, loaded with `Resources.Load`. Names must stay `level<N>` with consecutive N: the level count is `Resources.LoadAll("LevelFiles")`, and the next level is worked out by parsing the number after `"level"`. A file must **not** end with a newline: the parser splits on `
` and an empty last row throws. The first line of the file is the **top** row (lines are reversed so y=0 is the bottom). Each cell is a code, parsed in `GridManagerScript.MadeGenerateObject`:
- `T<n>` → prefab `Tree<n>` (blocks movement)
- `S<m><n>` → prefab `Stone<n>`, adds m to the move budget (blocks jump landing)
- `N<m>` → empty cell, adds m to the move budget
- `F<m>` → Fly, counts toward catch total and adds m to the budget. `FF` is a fly that adds neither
- `R<U|L|R|B>` → Frog, facing that way. `P<…>` → Pond (the goal); `PV` rotates it 90°

Prefabs are loaded by name from `Assets/Resources/Prefabs/`, so a new obstacle variant means a new prefab named to match the code. The sum of the budget digits is `maxMove`. Stars: 1 for winning, +1 if moves ≤ maxMove, +1 if every fly was caught. Budgets are set to exactly the cheapest 3-star solution (Move 1, Jump 2, turn 1, Eat 0; the tongue reaches only the next cell; walking or jumping onto a fly scares it away; the pond covers 3 cells). Each page of 10 levels in the level select is shown as a chapter, with the total of stars earned.

**Command execution** (`ControllerScripts/PlayScript.cs`): `ReadCommands` finds the GameObjects tagged `SlotCreator` and walks the children of each slot. The **tag** of the command block in each slot picks the action (`Move`, `Jump`, `Eat`, `Left`, `Right`, `Repeater`). It runs as nested coroutines. Collision checks use `Physics2D.OverlapCircleAll` on layer `GameElements` and read object tags (`Tree`, `Stone`, `Fly`, `Pond`). Facing comes from the frog's z-rotation snapped to 0/90/180/270. Stop/pause works by setting a `pause` flag that the loops check.

**Repeaters:** the repeater editor panel uses slots tagged `SlotCreator2`. `MakeRepeaterScript.ReadCommands` turns those blocks into a list of command strings plus a list of `InternalRepeaterScript` (plain C# objects, used for nesting). It then destroys the blocks and puts a `RepeateScript` block into the main slots. Nesting depth is capped by `GetLvl()` (more than 2 levels is refused). When running, each `"Repeater"` command in a list uses the next `InternalRepeaterScript` in order.

**Drag and drop:** `DragFromSlot`/`DragFromSlots`/`DraggableButton` reparent the block GameObject. On drop they hit-test `Physics2D.OverlapPointAll` for objects tagged `SlotPrefab`. `SlotCreator` makes up to 10 slots at runtime and stops when they would go off screen. While a block is being dragged, camera panning is turned off through `CameraController.SetChangeview`.

**Progress saving:** `FileController` reads and writes `Application.persistentDataPath/levelData.json` (`LevelDataWrapper { List<LevelScore> }`, where `score` -1 means never played and `open` means unlocked). `SetLevelScript` (Menu `Awake`) adds entries when new level files exist. `ResultScoreScript.saveScore` updates the best score and unlocks the next level. In this Editor, `Application.persistentDataPath` comes back empty (the company name `'PalAthGame  '` ends in spaces, which may be the cause). So the Editor's save is the `levelData.json` in the repo root, and Play Mode testing changes it.

**Path look:** `Gamemanager/PathDecorator.cs` sits on GridManager and is called after the grid is built. It puts trodden-path tiles under walkable cells (everything except `T*` and `FF`), adds a leafy fringe sprite where the path meets a blocked cell, and adds a small leaf corner at inner corners, and scatters loose leaves and grass tufts on the path (`Assets/Graphic/Path/Leaves`, drawn by `Tools/generate_leaves.py`, placed from a seed based on the cell so a level always looks the same). The 16 path tiles in `Assets/Graphic/Path/Tiles` are one seamless 4x4 texture made by `Tools/generate_path_tiles.py`; its arguments set how much dirt shows and how many leaf tufts there are. Draw order: GameBackground -2, tilemap -1, path fringe 0, stones/flies/pond 1, trees 2, frog 3.

**Frog skins and fly wallet:** `FrogSkins.cs` is a static class that holds the skins and their prices. It keeps the fly wallet, the bought skins and the selected skin in PlayerPrefs (`Flies`, `FrogSkinOwned_<id>`, `FrogSkin`). `ResultScoreScript.setWin` adds the flies eaten in a won run to the wallet (a lost run gives nothing; replays count again) and shows "+N" with a fly icon under the stars. `FrogLookScript` sits on the Frog prefab. It swaps in the selected skin's sprite, and at level start it blinks an arrow above the frog 3 times. The skin sprites are recoloured copies of `Graphic/GameObject/Fog.png` with its bone data, in `Resources/FrogSkins/` (made by `Tools/generate_frog_skins.py`). To make a new skin, copy the .meta with a new guid so the bones carry over. Shop icons are in `Resources/FrogSkins/Icons` (rendered from the prefab in Play Mode). `MainMenu/FrogShopScript.cs` on the Menu object builds the shop screen in code; it opens from the menu button `Menu/Canvas/Shop`.

**Back to start:** `Gamemanager/LevelResetScript.cs` sits on GridManager. When the commands run out without a win, `PlayScript` calls `OnRunEnded`, which offers "To start / Continue". To start puts the frog and the flies (saved one frame after the level is built) back where they began and resets `catchFly` and `countMoves`. The blocks in the slots stay. The offer can be switched off in Settings (`MainMenu/RestartOfferToggle.cs` on the Settings prefab, PlayerPrefs `OfferRestart`). Stopping a run with Stop does not show the offer. In tutorial levels, the retry hint arrow points at the To start button.

**Feedback effects:** `Gamemanager/FrogEffectsScript.cs` sits on the Frog prefab. `EatScript` calls `FlyEaten` to float "+1" with a fly icon above Larry. `PlayScript.BumpEffect` runs `Bump` when a Move or Jump is blocked by a tree or stone: a nudge and shake, the obstacle shakes, the `Sounds/Bump.wav` thump plays and dizzy stars appear. The sound and the star sprite are generated by `Tools/generate_effects.py` and contain no third-party content. While a new frog is affordable, the menu's Shop button pulses and shows a fly badge (`FrogShopScript`). A bump also gives a short phone vibration (`Gamemanager/Haptics.cs`, PlayerPrefs `Vibration`; a Settings row next to the restart offer, both built by `RestartOfferToggle`). On a win, `ResultStarsEffect` pops the result stars in one by one with the `Resources/Sounds/StarDing.wav` chime (also from `generate_effects.py`).

**Tutorial levels:** level1–5 are guided tutorials, and level16 teaches the nested repeater (3× [Jump 3×, Left]). `Gamemanager/TutorialScript.cs` sits on the `Tutorial` object in Game.unity and holds hard-coded steps per level: a text bubble, a ghost block flying to the slot where the block belongs or an arrow pointing from the side, and a condition that ends the step. A failed run shows that level's `tut_hint_lN` hint. Its texts are `tut_*` keys in the localization table. These levels were inserted in front of the original ones, which became level6+. `SetLevelScript.MigrateTutorialLevels` shifts old saves and `LastLevel` once, guarded by `PlayerPrefs["LevelLayoutVersion"]`. If levels are inserted again, bump that version and shift again, rather than just renaming files. Version 3 (`MigrateMovedLevel`) moved the old level18 to level25 (old 19–25 became 18–24): scores follow their maps. `UnlockAfterWins` opens every level after a won one, so levels added in an update are reachable.

**Localization:** Unity Localization package with locales en, cs, fr, de. String tables are Addressables (`Assets/AddressableAssetsData`). `Localization.cs` cycles locales and saves the choice in `PlayerPrefs["Language"]`.

**Many lookups are by string:** tags, `GameObject.Find("GridManager"/"LeftPanel"/"RunPanel"/"GameBackground")`, and child names such as `Image/Star0..2` and `LevelsGrid/Level1..10`. Renaming objects or tags in scenes or prefabs silently breaks code, so grep for the string first.

## Conventions / gotchas

- Code comments and log messages are in Czech. Several `.cs` files are **Windows-1250**-encoded with CRLF line endings, not UTF-8. Keep the existing encoding when editing, or the Czech characters get corrupted.
- The repo root holds build output and signing material (`*.apk`, `*.aab`, symbol zips, `*_DoNotShip`/`BackUpThisFolder` folders, `IOSbuild/`, `user.keystore`, `y`/`y.pub`). Never commit or modify these. `user.keystore` and `y`/`y.pub` are **not** covered by `.gitignore`, and `y`/`y.pub` are already committed.
- `Assets/TextMesh Pro/Examples & Extras` and the asset-store art/sound folders are third-party content. Leave them alone unless asked.
