using System;
using System.Collections.Generic;
using HexagonalLib.Coordinates;
using static System.Math;

namespace HexagonalLib
{
    /// <summary>
    /// Represent geometry logic for infinity hexagonal grid
    /// </summary>
    public readonly partial struct HexagonalGrid
    {
        /// <summary>
        /// Total count of edges in one Hex
        /// </summary>
        public const int EdgesCount = 6;

        public static readonly float Sqrt3 = (float) Sqrt(3);

        /// <summary>
        /// Inscribed radius of the hex
        /// </summary>
        public readonly float InscribedRadius;

        /// <summary>
        /// Described radius of hex
        /// </summary>
        public readonly float DescribedRadius;

        /// <summary>
        /// Hexagon side length
        /// </summary>
        public float Side => DescribedRadius;

        /// <summary>
        /// Inscribed diameter of hex
        /// </summary>
        public float InscribedDiameter => InscribedRadius * 2;

        /// <summary>
        /// Described diameter of hex
        /// </summary>
        public float DescribedDiameter => DescribedRadius * 2;

        /// <summary>
        /// Orientation and layout of this grid
        /// </summary>
        public readonly HexagonalGridType Type;

        /// <summary>
        /// Offset between hex and its right-side neighbour on X axis
        /// </summary>
        public float HorizontalOffset
        {
            get
            {
                switch (Type)
                {
                    case HexagonalGridType.PointyOdd:
                    case HexagonalGridType.PointyEven:
                        return InscribedRadius * 2.0f;
                    case HexagonalGridType.FlatOdd:
                    case HexagonalGridType.FlatEven:
                        return DescribedRadius * 1.5f;
                    default:
                        throw new HexagonalException($"Can't get {nameof(HorizontalOffset)} with unexpected {nameof(Type)}", this);
                }
            }
        }

        /// <summary>
        /// Offset between hex and its up-side neighbour on Y axis
        /// </summary>
        public float VerticalOffset
        {
            get
            {
                switch (Type)
                {
                    case HexagonalGridType.PointyOdd:
                    case HexagonalGridType.PointyEven:
                        return DescribedRadius * 1.5f;
                    case HexagonalGridType.FlatOdd:
                    case HexagonalGridType.FlatEven:
                        return InscribedRadius * 2.0f;
                    default:
                        throw new HexagonalException($"Can't get {nameof(VerticalOffset)} with unexpected {nameof(Type)}", this);
                }
            }
        }

        /// <summary>
        /// The angle between the centers of any hex and its first neighbor relative to the vector (0, 1) clockwise
        /// </summary>
        /// <exception cref="HexagonalException"></exception>
        public float AngleToFirstNeighbor
        {
            get
            {
                switch (Type)
                {
                    case HexagonalGridType.PointyOdd:
                    case HexagonalGridType.PointyEven:
                        return 0.0f;
                    case HexagonalGridType.FlatOdd:
                    case HexagonalGridType.FlatEven:
                        return 30.0f;
                    default:
                        throw new HexagonalException($"Can't get {nameof(AngleToFirstNeighbor)} with unexpected {nameof(Type)}", this);
                }
            }
        }

        /// <summary>
        /// Base constructor for hexagonal grid
        /// </summary>
        /// <param name="type">Orientation and layout of the grid</param>
        /// <param name="radius">Inscribed radius</param>
        public HexagonalGrid(HexagonalGridType type, float radius)
        {
            Type = type;
            InscribedRadius = radius;
            DescribedRadius = (float) (radius / Cos(PI / EdgesCount));
        }

        #region ToOffset

        /// <summary>
        /// Convert cubic coordinate to offset
        /// </summary>
        public Offset ToOffset(Cubic coord)
        {
            switch (Type)
            {
                case HexagonalGridType.PointyOdd:
                {
                    var col = coord.X + (coord.Z - (coord.Z & 1)) / 2;
                    var row = coord.Z;
                    return new Offset(col, row);
                }
                case HexagonalGridType.PointyEven:
                {
                    var col = coord.X + (coord.Z + (coord.Z & 1)) / 2;
                    var row = coord.Z;
                    return new Offset(col, row);
                }
                case HexagonalGridType.FlatOdd:
                {
                    var col = coord.X;
                    var row = coord.Z + (coord.X - (coord.X & 1)) / 2;
                    return new Offset(col, row);
                }
                case HexagonalGridType.FlatEven:
                {
                    var col = coord.X;
                    var row = coord.Z + (coord.X + (coord.X & 1)) / 2;
                    return new Offset(col, row);
                }
                default:
                    throw new HexagonalException($"{nameof(ToOffset)} failed with unexpected {nameof(Type)}", this, (nameof(coord), coord));
            }
        }

        /// <summary>
        /// Convert axial coordinate to offset
        /// </summary>
        public Offset ToOffset(Axial axial)
        {
            return ToOffset(ToCubic(axial));
        }

        /// <summary>
        /// Returns the offset coordinate of the hex which contains a point
        /// </summary>
        public Offset ToOffset(float x, float y)
        {
            return ToOffset(ToCubic(x, y));
        }

        /// <summary>
        /// Returns the offset coordinate of the hex which contains a point
        /// </summary>
        public Offset ToOffset((float X, float Y) point)
        {
            return ToOffset(ToCubic(point.X, point.Y));
        }

        #endregion

        #region ToAxial

        /// <summary>
        /// Convert cubic coordinate to axial
        /// </summary>
        public Axial ToAxial(Cubic cubic)
        {
            return new Axial(cubic.X, cubic.Z);
        }

        /// <summary>
        /// Convert offset coordinate to axial
        /// </summary>
        public Axial ToAxial(Offset offset)
        {
            return ToAxial(ToCubic(offset));
        }

        /// <summary>
        /// Returns the axial coordinate of the hex which contains a point
        /// </summary>
        public Axial ToAxial(float x, float y)
        {
            return ToAxial(ToCubic(x, y));
        }

        /// <summary>
        /// Returns the axial coordinate of the hex which contains a point
        /// </summary>
        public Axial ToAxial((float X, float Y) point)
        {
            return ToAxial(ToCubic(point.X, point.Y));
        }

        #endregion

        #region ToCubic

        /// <summary>
        /// Convert offset coordinate to cubic
        /// </summary>
        public Cubic ToCubic(Offset coord)
        {
            switch (Type)
            {
                case HexagonalGridType.PointyOdd:
                {
                    var x = coord.X - (coord.Y - (coord.Y & 1)) / 2;
                    var z = coord.Y;
                    var y = -x - z;
                    return new Cubic(x, y, z);
                }
                case HexagonalGridType.PointyEven:
                {
                    var x = coord.X - (coord.Y + (coord.Y & 1)) / 2;
                    var z = coord.Y;
                    var y = -x - z;
                    return new Cubic(x, y, z);
                }
                case HexagonalGridType.FlatOdd:
                {
                    var x = coord.X;
                    var z = coord.Y - (coord.X - (coord.X & 1)) / 2;
                    var y = -x - z;
                    return new Cubic(x, y, z);
                }
                case HexagonalGridType.FlatEven:
                {
                    var x = coord.X;
                    var z = coord.Y - (coord.X + (coord.X & 1)) / 2;
                    var y = -x - z;
                    return new Cubic(x, y, z);
                }
                default:
                    throw new HexagonalException($"{nameof(ToCubic)} failed with unexpected {nameof(Type)}", this, (nameof(coord), coord));
            }
        }

        /// <summary>
        /// Convert axial coordinate to cubic
        /// </summary>
        public Cubic ToCubic(Axial axial)
        {
            return new Cubic(axial.Q, -axial.Q - axial.R, axial.R);
        }

        /// <summary>
        /// Returns the cubic coordinate of the hex which contains a point
        /// </summary>
        public Cubic ToCubic(float x, float y)
        {
            switch (Type)
            {
                case HexagonalGridType.PointyOdd:
                case HexagonalGridType.PointyEven:
                {
                    var q = (x * Sqrt3 / 3.0f - y / 3.0f) / Side;
                    var r = y * 2.0f / 3.0f / Side;
                    return new Cubic(q, -q - r, r);
                }
                case HexagonalGridType.FlatOdd:
                case HexagonalGridType.FlatEven:
                {
                    var q = x * 2.0f / 3.0f / Side;
                    var r = (-x / 3.0f + Sqrt3 / 3.0f * y) / Side;
                    return new Cubic(q, -q - r, r);
                }
                default:
                    throw new HexagonalException($"{nameof(ToCubic)} failed with unexpected {nameof(Type)}", this, (nameof(x), x), (nameof(y), y));
            }
        }

        /// <summary>
        /// Returns the cubic coordinate of the hex which contains a point
        /// </summary>
        public Cubic ToCubic((float X, float Y) point)
        {
            return ToCubic(point.X, point.Y);
        }

        #endregion

        #region ToPoint2

        /// <summary>
        /// Convert hex based on its offset coordinate to it center position in 2d space
        /// </summary>
        public (float X, float Y) ToPoint2(Offset coord)
        {
            return ToPoint2(ToAxial(coord));
        }

        /// <summary>
        /// Convert hex based on its axial coordinate to it center position in 2d space
        /// </summary>
        public (float X, float Y) ToPoint2(Axial coord)
        {
            switch (Type)
            {
                case HexagonalGridType.PointyOdd:
                case HexagonalGridType.PointyEven:
                {
                    var x = Side * (Sqrt3 * coord.Q + Sqrt3 / 2 * coord.R);
                    var y = Side * (3.0f / 2.0f * coord.R);
                    return (x, y);
                }
                case HexagonalGridType.FlatOdd:
                case HexagonalGridType.FlatEven:
                {
                    var x = Side * (3.0f / 2.0f * coord.Q);
                    var y = Side * (Sqrt3 / 2 * coord.Q + Sqrt3 * coord.R);
                    return (x, y);
                }
                default:
                    throw new HexagonalException($"{nameof(ToPoint2)} failed with unexpected {nameof(Type)}", this, (nameof(coord), coord));
            }
        }

        /// <summary>
        /// Convert hex based on its cubic coordinate to it center position in 2d space
        /// </summary>
        public (float X, float Y) ToPoint2(Cubic coord)
        {
            return ToPoint2(ToAxial(coord));
        }

        #endregion

        #region GetCornerPoint

        /// <summary>
        /// Returns corner point in 2d space  of given coordinate
        /// </summary>
        public (float X, float Y) GetCornerPoint(Offset coord, int edge)
        {
            return GetCornerPoint(ToPoint2(coord), edge);
        }

        /// <summary>
        /// Returns corner point in 2d space  of given coordinate
        /// </summary>
        public (float X, float Y) GetCornerPoint(Axial coord, int edge)
        {
            return GetCornerPoint(ToPoint2(coord), edge);
        }

        /// <summary>
        /// Returns corner point in 2d space  of given coordinate
        /// </summary>
        public (float X, float Y) GetCornerPoint(Cubic coord, int edge)
        {
            return GetCornerPoint(ToPoint2(coord), edge);
        }

        /// <summary>
        /// Returns corner point in 2d space  of given coordinate
        /// </summary>
        private (float X, float Y) GetCornerPoint((float X, float Y) center, int edge)
        {
            edge = NormalizeIndex(edge);
            var angleDeg = 60 * edge;
            if (Type == HexagonalGridType.PointyEven || Type == HexagonalGridType.PointyOdd)
            {
                angleDeg -= 30;
            }

            var angleRad = PI / 180 * angleDeg;
            var x = (float) (center.X + DescribedRadius * Cos(angleRad));
            var y = (float) (center.Y + DescribedRadius * Sin(angleRad));
            return (x, y);
        }

        #endregion

        #region GetNeighbor

        /// <summary>
        /// Returns the neighbor at the specified index.
        /// </summary>
        public Offset GetNeighbor(Offset coord, int neighborIndex)
        {
            return coord + GetNeighborsOffsets(coord)[NormalizeIndex(neighborIndex)];
        }

        /// <summary>
        /// Returns the neighbor at the specified index.
        /// </summary>
        public Axial GetNeighbor(Axial coord, int neighborIndex)
        {
            return coord + _axialNeighbors[NormalizeIndex(neighborIndex)];
        }

        /// <summary>
        /// Returns the neighbor at the specified index.
        /// </summary>
        public Cubic GetNeighbor(Cubic coord, int neighborIndex)
        {
            return coord + _cubicNeighbors[NormalizeIndex(neighborIndex)];
        }

        #endregion

        #region GetNeighbors

        /// <summary>
        /// Return all neighbors of the hex
        /// </summary>
        public IEnumerable<Offset> GetNeighbors(Offset hex)
        {
            foreach (var offset in GetNeighborsOffsets(hex))
            {
                yield return offset + hex;
            }
        }

        /// <summary>
        /// Return all neighbors of the hex
        /// </summary>
        public IEnumerable<Axial> GetNeighbors(Axial hex)
        {
            foreach (var offset in _axialNeighbors)
            {
                yield return offset + hex;
            }
        }

        /// <summary>
        /// Return all neighbors of the hex
        /// </summary>
        public IEnumerable<Cubic> GetNeighbors(Cubic hex)
        {
            foreach (var offset in _cubicNeighbors)
            {
                yield return offset + hex;
            }
        }

        /// <summary>
        /// Writes all six neighbors into a caller-provided buffer without allocating memory.
        /// The buffer must contain at least <see cref="EdgesCount"/> elements.
        /// </summary>
        public void GetNeighborsNonAlloc(Offset hex, Offset[] buffer)
        {
            ValidateNeighborsBuffer(buffer);
            var offsets = GetNeighborsOffsets(hex);
            for (var i = 0; i < EdgesCount; i++)
            {
                buffer[i] = offsets[i] + hex;
            }
        }

        /// <summary>
        /// Writes all six neighbors into a caller-provided buffer without allocating memory.
        /// The buffer must contain at least <see cref="EdgesCount"/> elements.
        /// </summary>
        public void GetNeighborsNonAlloc(Axial hex, Axial[] buffer)
        {
            ValidateNeighborsBuffer(buffer);
            for (var i = 0; i < EdgesCount; i++)
            {
                buffer[i] = _axialNeighbors[i] + hex;
            }
        }

        /// <summary>
        /// Writes all six neighbors into a caller-provided buffer without allocating memory.
        /// The buffer must contain at least <see cref="EdgesCount"/> elements.
        /// </summary>
        public void GetNeighborsNonAlloc(Cubic hex, Cubic[] buffer)
        {
            ValidateNeighborsBuffer(buffer);
            for (var i = 0; i < EdgesCount; i++)
            {
                buffer[i] = _cubicNeighbors[i] + hex;
            }
        }

        #endregion

        #region IsNeighbors

        /// <summary>
        /// Checks whether the two hexes are neighbors or no
        /// </summary>
        public bool IsNeighbors(Offset coord1, Offset coord2)
        {
            var offsets = GetNeighborsOffsets(coord1);
            for (var i = 0; i < EdgesCount; i++)
            {
                if (offsets[i] + coord1 == coord2)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Checks whether the two hexes are neighbors or no
        /// </summary>
        public bool IsNeighbors(Axial coord1, Axial coord2)
        {
            for (var i = 0; i < EdgesCount; i++)
            {
                if (_axialNeighbors[i] + coord1 == coord2)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Checks whether the two hexes are neighbors or no
        /// </summary>
        public bool IsNeighbors(Cubic coord1, Cubic coord2)
        {
            for (var i = 0; i < EdgesCount; i++)
            {
                if (_cubicNeighbors[i] + coord1 == coord2)
                {
                    return true;
                }
            }

            return false;
        }

        #endregion

        #region GetNeighborsRing

        /// <summary>
        /// Returns a ring with a radius of <see cref="radius"/> hexes around the given <see cref="center"/>.
        /// </summary>
        public IEnumerable<Offset> GetNeighborsRing(Offset center, int radius)
        {
            return GetNeighborsRing(center, radius, GetNeighbor);
        }

        /// <summary>
        /// Returns a ring with a radius of <see cref="radius"/> hexes around the given <see cref="center"/>.
        /// </summary>
        public IEnumerable<Axial> GetNeighborsRing(Axial center, int radius)
        {
            return GetNeighborsRing(center, radius, GetNeighbor);
        }

        /// <summary>
        /// Returns a ring with a radius of <see cref="radius"/> hexes around the given <see cref="center"/>.
        /// </summary>
        public IEnumerable<Cubic> GetNeighborsRing(Cubic center, int radius)
        {
            return GetNeighborsRing(center, radius, GetNeighbor);
        }

        /// <summary>
        /// Returns a ring with a radius of <see cref="radius"/> hexes around the given <see cref="center"/>.
        /// </summary>
        private static IEnumerable<T> GetNeighborsRing<T>(T center, int radius, Func<T, int, T> getNeighbor)
            where T : struct
        {
            if (radius == 0)
            {
                yield return center;
                yield break;
            }

            for (var i = 0; i < radius; i++)
            {
                center = getNeighbor(center, 4);
            }

            for (var i = 0; i < 6; i++)
            {
                for (var j = 0; j < radius; j++)
                {
                    yield return center;
                    center = getNeighbor(center, i);
                }
            }
        }

        #endregion

        #region GetNeighborsAround

        /// <summary>
        /// Returns a all hexes in the ring with a radius of <see cref="radius"/> hexes around the given <see cref="center"/>.
        /// </summary>
        public IEnumerable<Offset> GetNeighborsAround(Offset center, int radius)
        {
            return GetNeighborsAround(center, radius, GetNeighborsRing);
        }

        /// <summary>
        /// Returns a all hexes in the ring with a radius of <see cref="radius"/> hexes around the given <see cref="center"/>.
        /// </summary>
        public IEnumerable<Axial> GetNeighborsAround(Axial center, int radius)
        {
            return GetNeighborsAround(center, radius, GetNeighborsRing);
        }

        /// <summary>
        /// Returns a all hexes in the ring with a radius of <see cref="radius"/> hexes around the given <see cref="center"/>.
        /// </summary>
        public IEnumerable<Cubic> GetNeighborsAround(Cubic center, int radius)
        {
            return GetNeighborsAround(center, radius, GetNeighborsRing);
        }

        /// <summary>
        /// Returns a all hexes in the ring with a radius of <see cref="radius"/> hexes around the given <see cref="center"/>.
        /// </summary>
        private static IEnumerable<T> GetNeighborsAround<T>(T center, int radius, Func<T, int, IEnumerable<T>> getNeighborRing)
            where T : struct
        {
            for (var i = 0; i < radius; i++)
            {
                foreach (var hex in getNeighborRing(center, i))
                {
                    yield return hex;
                }
            }
        }

        #endregion

        #region GetNeighborIndex

        /// <summary>
        /// Returns the bypass index to the specified neighbor
        /// </summary>
        public byte GetNeighborIndex(Offset center, Offset neighbor)
        {
            var offsets = GetNeighborsOffsets(center);
            for (byte i = 0; i < EdgesCount; i++)
            {
                if (offsets[i] + center == neighbor)
                {
                    return i;
                }
            }

            throw NeighborIndexNotFound(center, neighbor);
        }

        /// <summary>
        /// Returns the bypass index to the specified neighbor
        /// </summary>
        public byte GetNeighborIndex(Axial center, Axial neighbor)
        {
            for (byte i = 0; i < EdgesCount; i++)
            {
                if (_axialNeighbors[i] + center == neighbor)
                {
                    return i;
                }
            }

            throw NeighborIndexNotFound(center, neighbor);
        }

        /// <summary>
        /// Returns the bypass index to the specified neighbor
        /// </summary>
        public byte GetNeighborIndex(Cubic center, Cubic neighbor)
        {
            for (byte i = 0; i < EdgesCount; i++)
            {
                if (_cubicNeighbors[i] + center == neighbor)
                {
                    return i;
                }
            }

            throw NeighborIndexNotFound(center, neighbor);
        }

        private HexagonalException NeighborIndexNotFound<T>(T center, T neighbor)
            where T : struct
        {
            return new HexagonalException($"Can't find bypass index", this, (nameof(center), center), (nameof(neighbor), neighbor));
        }

        #endregion

        #region GetPointBetweenTwoNeighbours

        /// <summary>
        /// Returns the midpoint of the boundary segment of two neighbors
        /// </summary>
        public (float x, float y) GetPointBetweenTwoNeighbours(Offset coord1, Offset coord2)
        {
            if (!IsNeighbors(coord1, coord2))
            {
                throw NotNeighbors(coord1, coord2);
            }

            return GetPointBetween(ToPoint2(coord1), ToPoint2(coord2));
        }

        /// <summary>
        /// Returns the midpoint of the boundary segment of two neighbors
        /// </summary>
        public (float x, float y) GetPointBetweenTwoNeighbours(Axial coord1, Axial coord2)
        {
            if (!IsNeighbors(coord1, coord2))
            {
                throw NotNeighbors(coord1, coord2);
            }

            return GetPointBetween(ToPoint2(coord1), ToPoint2(coord2));
        }

        /// <summary>
        /// Returns the midpoint of the boundary segment of two neighbors
        /// </summary>
        public (float x, float y) GetPointBetweenTwoNeighbours(Cubic coord1, Cubic coord2)
        {
            if (!IsNeighbors(coord1, coord2))
            {
                throw NotNeighbors(coord1, coord2);
            }

            return GetPointBetween(ToPoint2(coord1), ToPoint2(coord2));
        }

        private HexagonalException NotNeighbors<T>(T coord1, T coord2)
            where T : struct
        {
            return new HexagonalException($"Can't calculate point between not neighbors", this, (nameof(coord1), coord1), (nameof(coord2), coord2));
        }

        private static (float x, float y) GetPointBetween((float X, float Y) c1, (float X, float Y) c2)
        {
            return ((c1.X + c2.X) / 2, (c1.Y + c2.Y) / 2);
        }

        #endregion

        #region CubeDistance

        /// <summary>
        /// Manhattan distance between two hexes
        /// </summary>
        public int CubeDistance(Offset h1, Offset h2)
        {
            var cubicFrom = ToCubic(h1);
            var cubicTo = ToCubic(h2);
            return CubeDistance(cubicFrom, cubicTo);
        }

        /// <summary>
        /// Manhattan distance between two hexes
        /// </summary>
        public int CubeDistance(Axial h1, Axial h2)
        {
            var cubicFrom = ToCubic(h1);
            var cubicTo = ToCubic(h2);
            return CubeDistance(cubicFrom, cubicTo);
        }

        /// <summary>
        /// Manhattan distance between two hexes
        /// </summary>
        public static int CubeDistance(Cubic h1, Cubic h2)
        {
            return (Abs(h1.X - h2.X) + Abs(h1.Y - h2.Y) + Abs(h1.Z - h2.Z)) / 2;
        }

        #endregion

        #region Neighbors

        /// <summary>
        /// Return all neighbors offsets of the hex
        /// </summary>
        private Offset[] GetNeighborsOffsets(Offset coord)
        {
            switch (Type)
            {
                case HexagonalGridType.PointyOdd:
                    return Abs(coord.Y % 2) == 0 ? _pointyEvenNeighbors : _pointyOddNeighbors;
                case HexagonalGridType.PointyEven:
                    return Abs(coord.Y % 2) == 1 ? _pointyEvenNeighbors : _pointyOddNeighbors;
                case HexagonalGridType.FlatOdd:
                    return Abs(coord.X % 2) == 0 ? _flatEvenNeighbors : _flatOddNeighbors;
                case HexagonalGridType.FlatEven:
                    return Abs(coord.X % 2) == 1 ? _flatEvenNeighbors : _flatOddNeighbors;
                default:
                    throw new HexagonalException($"{nameof(GetNeighborsOffsets)} failed with unexpected {nameof(Type)}", this, (nameof(coord), coord));
            }
        }

        private static readonly Offset[] _pointyOddNeighbors =
        {
            new Offset(+1, 0), new Offset(+1, -1), new Offset(0, -1),
            new Offset(-1, 0), new Offset(0, +1), new Offset(+1, +1),
        };

        private static readonly Offset[] _pointyEvenNeighbors =
        {
            new Offset(+1, 0), new Offset(0, -1), new Offset(-1, -1),
            new Offset(-1, 0), new Offset(-1, +1), new Offset(0, +1),
        };

        private static readonly Offset[] _flatOddNeighbors =
        {
            new Offset(+1, +1), new Offset(+1, 0), new Offset(0, -1),
            new Offset(-1, 0), new Offset(-1, +1), new Offset(0, +1),
        };

        private static readonly Offset[] _flatEvenNeighbors =
        {
            new Offset(+1, 0), new Offset(+1, -1), new Offset(0, -1),
            new Offset(-1, -1), new Offset(-1, 0), new Offset(0, +1),
        };

        private static readonly Axial[] _axialNeighbors =
        {
            new Axial(+1, 0), new Axial(+1, -1), new Axial(0, -1),
            new Axial(-1, 0), new Axial(-1, +1), new Axial(0, +1),
        };

        private static readonly Cubic[] _cubicNeighbors =
        {
            new Cubic(+1, -1, 0), new Cubic(+1, 0, -1), new Cubic(0, +1, -1),
            new Cubic(-1, +1, 0), new Cubic(-1, 0, +1), new Cubic(0, -1, +1),
        };

        #endregion

        private static int NormalizeIndex(int index)
        {
            index = index % EdgesCount;
            if (index < 0)
            {
                index += EdgesCount;
            }

            return index;
        }

        private static void ValidateNeighborsBuffer<T>(T[] buffer)
        {
            if (buffer == null || buffer.Length < EdgesCount)
            {
                throw new ArgumentException($"The buffer must contain at least {EdgesCount} elements.", nameof(buffer));
            }
        }
    }
}
