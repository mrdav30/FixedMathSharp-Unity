# Fixed64 Unity Serialization

## Summary

`Fixed64` has a Unity-compatible serialized shape. Unity can persist non-zero `Fixed64`, `Vector2d`, and `Vector3d` authoring values through ScriptableObject assets and prefabs, including nested structs and list elements.

Unity's field serialization rules state: Unity serializes fields, but it does not persist `readonly` fields. Hence, `Fixed64` cannot persist its payload as `public readonly long m_rawValue` since Unity had no durable payload to write. Inspector edits could affect the live object, then reload as zero from asset or prefab YAML.

The fix was to keep FixedMathSharp engine-agnostic by using a normal public `long` field instead of Unity attributes or Unity-specific wrapper types.

Unity 6000.3 serialization rules: <https://docs.unity3d.com/6000.3/Documentation/Manual/script-serialization-rules.html>

## Intentional Public Field

`m_rawValue` is public and mutable so field-based serializers, including Unity's serializer, can persist the exact fixed-point payload without a Unity dependency in core.

Treat direct writes to `m_rawValue` as serializer/bootstrap-level access. Normal code should continue to use the existing construction and conversion APIs such as `Fixed64.FromRaw`, `Fixed64.FromDouble`, casts, operators, and `ToRawString`.

## Local Persistence Coverage

The EditMode persistence tests cover:

- `[SerializeField] Fixed64`
- `[SerializeField] Vector2d`
- `[SerializeField] Vector3d`
- a nested serializable struct containing `Fixed64`
- a `List<T>` element containing `Fixed64` and `Vector3d`
- ScriptableObject save/reload through `AssetDatabase`
- prefab save/reload through `PrefabUtility`
- editing already-created ScriptableObject and prefab assets, then saving/reloading

The tests set non-zero raw values, save the asset/prefab, reload, then compare raw values exactly.

## Verified Command

This command produced TestRunner XML reliably without `-quit`:

```powershell
$resultPath = 'F:\gamedevrepos\FixedMathSharp-Unity\TestResults\fixed-serialization-editmode.xml'
$unity = 'C:\Program Files\Unity\Hub\Editor\6000.3.9f1\Editor\Unity.exe'
$args = @(
    '-batchmode',
    '-projectPath', 'F:\gamedevrepos\FixedMathSharp-Unity',
    '-runTests',
    '-testPlatform', 'editmode',
    '-testResults', $resultPath,
    '-logFile', 'F:\gamedevrepos\FixedMathSharp-Unity\Logs\fixed-serialization-editmode.log'
)
$process = Start-Process -FilePath $unity -ArgumentList $args -WindowStyle Hidden -Wait -PassThru
exit $process.ExitCode
```
