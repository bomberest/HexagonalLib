# HexagonalLib for Unity

`com.bomberest.hexagonal-lib` provides hexagonal-grid geometry, coordinate conversion, mesh generation, and Unity `Vector2`/`Vector3` helpers.

## Installation

### Git URL

In Unity, open **Window > Package Manager**, select **+ > Add package from git URL**, and enter:

```
https://github.com/bomberest/HexagonalLib.git?path=Packages/com.bomberest.hexagonal-lib
```

For a pinned revision, append `#<tag-or-commit>` to the URL.

### Local package

Use **+ > Add package from disk** and select this package's `package.json`, or add the following dependency to the consuming project's `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.bomberest.hexagonal-lib": "file:../HexagonalLib/Packages/com.bomberest.hexagonal-lib"
  }
}
```

## Quick start

```csharp
using HexagonalLib;
using HexagonalLib.Coordinates;
using UnityEngine;

var grid = new HexagonalGrid(HexagonalGridType.PointyEven, 1f);
var position = grid.ToVector3(new Offset(2, 3));
var coordinate = grid.ToOffset(new Vector3(2f, 0f, 3f));
```

For the complete coordinate-system and grid API overview, see the [repository README](https://github.com/bomberest/HexagonalLib/blob/master/README.md).
