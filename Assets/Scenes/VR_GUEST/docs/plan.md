 ## Plan: Fix SpawnManager and MapGapBehaviour

TL;DR: Normalize gap/scene name casing, replace DB-only gap spawn with a static, inspector-editable gap list (or JSON/ScriptableObject), and fix timing/race conditions and color-fade bugs in `SpawnManager` and `MapGapBehaviour` so gates always appear and click-feedback works reliably.

**Steps**
1. Replace `GetGapsFromDatabase()` in `Assets/My_Scripts/MapManagement/SpawnManager.cs` with a static source loader:
   - Add a serializable `GapInfo` class (fields: `string gap_name`, `double utm_north`, `double utm_east`) and a public `List<GapInfo> staticGaps` on `SpawnManager` so gates can be edited in the Inspector.
   - Implement `LoadGapsFromStaticList()` that uses `staticGaps` instead of `DataUsage`.
   - Keep a small helper to optionally load from `DataUsage` if present (runtime toggle).
   - Ensure spawned GameObject names are stored uppercased: `obj.name = gapName.ToUpper()`.
   - Use `Quaternion.identity` when instantiating and check for duplicate names before adding to `varchi`.
   - Change `GetGapInScene()` to return an empty dictionary instead of null when none found (or document the choice).

2. Harden UTM → Unity conversion & instantiation in `SpawnManager`:
   - Validate `city` and `varcoObj` and early-return with clear logs if missing.
   - Use `obj.transform.SetParent(city.transform, false);` then `obj.transform.localPosition = posizione_varco;` and `obj.transform.localRotation = Quaternion.identity;` to avoid transient world-space issues.
   - Make spawn coroutine deterministic: do not rely on other coroutines; ensure `fattore_scala` is computed in `Start()` and reused.

3. Fix `MapGapBehaviour.cs` race conditions and color reset:
   - Replace the coroutine `GetScenesInBuild()` (async, yielding) with a synchronous method executed in `Start()` that fills `scenesInBuild` immediately using:
     `for (int i=0; i<sceneCount; i++) { scenesInBuild.Add(Path.GetFileNameWithoutExtension(scenePath).ToUpper()); }`
   - In `ClickVisual()`, remove the early-exit when `scenesInBuild` is empty; instead, rely on the synchronous fill so the check `!scenesInBuild.Contains(gameObject.name.ToUpper())` is reliable.
   - Improve `ResetGapColor()` fade: replace fixed-step t+=0.1 with `t += Time.deltaTime/duration`, make `duration` configurable (e.g., 0.5s), wait initial hold using `yield return new WaitForSeconds(holdBeforeFade)` then lerp until t>=1, then explicitly set material color to `toRestore`.
   - Align `ReportPanelInfo()` wait-time comment and actual value (decide 0.1s or 1s) — change to `0.1f` or update comment.

4. Normalize casing across pipeline (important):
   - Ensure any database reads (`ReadLocalDatabase` / `DataRequest` / `DataUsage`) normalize gap names to uppercase as soon as data is ingested (recommended but optional).
   - Ensure `SpawnManager` and `MapGapBehaviour` always compare uppercase names: call `.ToUpperInvariant()` for comparisons and storage.

5. Minor cleanups & Unity best-practices:
   - Use `Quaternion.identity` instead of `new Quaternion(0,0,0,0)`.
   - Cache `meshRenderer.material` color only when needed; consider using a cached `Material instanceMaterial = meshRenderer.material;` so runtime instantiation is explicit.
   - Use `Debug.LogWarning`/`Debug.LogError` with the same `LogFormat` prefix if available.

**Relevant files**
- [Assets/My_Scripts/MapManagement/SpawnManager.cs](Assets/My_Scripts/MapManagement/SpawnManager.cs)
- [Assets/My_Scripts/Gap/MapGapBehaviour.cs](Assets/My_Scripts/Gap/MapGapBehaviour.cs)
- [Assets/My_Scripts/Michele_Scripts/Data/DataUsage.cs](Assets/My_Scripts/Michele_Scripts/Data/DataUsage.cs) — read-only reference
- [Assets/My_Scripts/Michele_Scripts/Data/GapData.cs](Assets/My_Scripts/Michele_Scripts/Data/GapData.cs) — optional to reuse types
- [Assets/My_Scripts/DateMenuManager.cs](Assets/My_Scripts/DateMenuManager.cs) — verification consumer of `GetGapInScene()`

**Verification**
1. In the Editor, populate `SpawnManager.staticGaps` with 2-3 entries; open `VR_GUEST_Home` and run Play: gates should appear at correct round-map positions.
2. Click on a gate whose scene is not in build → mesh should change to light-gray then fade back to original.
3. Click on a gate whose scene IS in build → mesh should change to white (alpha 0.6) and scene-load logic (existing `EnterGapAnimBehaviour`/`MySceneLoader`) should be triggered when wired.
4. Run `DateMenuManager` flow to compute colors via `ChangeColorOnAverageAccess()` and ensure colors are applied per gradient.
5. Check console for no race-condition warnings related to `scenesInBuild` being empty.

**Decisions / Assumptions**
- I will not remove `DataUsage` or other DB code immediately; the plan replaces DB-based spawning with a static inspector-editable list and leaves DB-based analytics (avg accesses) in place.
- For name normalization, we'll use `ToUpperInvariant()` everywhere to avoid culture issues.
- Storing static gaps: prefer `List<GapInfo>` serialized in `SpawnManager` or a `ScriptableObject` list if you want one shared asset. I'll ask which you prefer before implementing.

**Further Considerations**
1. Telemetry & WebSocket (Phase 2/3 in AS-IS): add a separate `TelemetryManager` to send entrance/exit JSON events via WebSocket; keep this as a follow-up task.
2. If you want offline editing by non-dev users, use a `ScriptableObject` assets list; if you want to update via file, use a `Resources` JSON file.

