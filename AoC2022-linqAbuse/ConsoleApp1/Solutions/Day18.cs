using System.Collections;
using System.Collections.Concurrent;
using System.Numerics;
using static ConsoleApp1.Solutions.Day16;
using static ConsoleApp1.Utils;

namespace ConsoleApp1.Solutions
{
    internal class Day18 : AbstractPuzzle
    {
        struct Vec3
        {
            public Int64 X;
            public Int64 Y;
            public Int64 Z;

            public Vec3() { }
            public Vec3(Int64 _x, Int64 _y, Int64 _z)
            {
                X = _x;
                Y = _y;
                Z = _z;
            }

            public Vec3(List<Int64> vals)
            {
                X = vals[0];
                Y = vals[1];
                Z = vals[2];
            }

            public static bool operator ==(Vec3 x, Vec3 y)
            {
                return x.X == y.X && x.Y == y.Y && x.Z == y.Z;
            }

            public static bool operator !=(Vec3 x, Vec3 y)
            {
                return !(x == y);
            }

            public override string ToString()
            {
                return "(" + X + ", " + Y + ", " + Z + ")";
            }
        }

        class SolidCubeGridEntry
        {
            Vec3 point;
            public ref Vec3 Point { get { return ref point; } }

            public List<SolidCubeGridEntry> touching = new();
        }

        Vec3[] GetNeighborPoints(ref Vec3 point)
        {
            Vec3[] n = new Vec3[6] { point, point, point, point, point, point };
            n[0].X += 1;
            n[1].X -= 1;
            n[2].Y += 1;
            n[3].Y -= 1;
            n[4].Z += 1;
            n[5].Z -= 1;
            return n;
        }

        override public void Part1()
        {
            Both();
        }

        override public void Part2()
        {
            Both();
        }

        void Both()
        {
            var lavaPoints = File.ReadAllText(InputFile!)
                .Split("\r\n").Select(x => 
                    new Vec3(x.Trim().Split(',').Select(x => Int64.Parse(x.Trim())).ToList())
                    ).ToList();

            Vec3 originalMinBounds = new(999999,999999,99999);
            Vec3 originalMaxBounds = new();

            var cubes = CreateMap(lavaPoints, ref originalMinBounds, ref originalMaxBounds);

            //now calculate faces, which is 6 - touching neighbors for each cube
            
            int nonTouchingFaces = cubes.Select(x => 6 - x.Value.touching.Count()).Sum();
            Console.WriteLine("num cubes: " + cubes.Count() + " (grid space: " + CalcMaxCubes(originalMinBounds, originalMaxBounds) + ")  with Free Faces: " + nonTouchingFaces + " out of max faces " + (6 * cubes.Count));

            //create hull -
            //possible way: flood fill from an edge point thats empty to create a mold
            //flood fill from internal with lava cubes to fill cavity
            //get number of exposed faces

            //increase minbounds/maxbounds by 1 and we'll have empty space to make a mold of the external faces
            Vec3 minBounds = originalMinBounds;
            Vec3 maxBounds = originalMaxBounds;
            minBounds.X -= 1;
            minBounds.Y -= 1;
            minBounds.Z -= 1;

            maxBounds.X += 1;
            maxBounds.Y += 1;
            maxBounds.Z += 1;

            List<Vec3> moldPoints = FloodFill(cubes, minBounds, minBounds, maxBounds, true);
            Vec3 moldMinBounds = new(999999, 999999, 99999);
            Vec3 moldMaxBounds = new();
            var mold = CreateMap(moldPoints, ref moldMinBounds, ref moldMaxBounds);
            Console.WriteLine("creating mold: flood fill filled " + moldPoints.Count);

            //now floodfill from *every* lava point, since diaganols dont connect
            //alterantive, could just invert the mold...
            HashSet<Vec3> alreadyFilled = new HashSet<Vec3>();
            for (int lavaPointInd = 0; lavaPointInd < lavaPoints.Count; lavaPointInd++)
            {
                Vec3 start = lavaPoints[lavaPointInd];
                List<Vec3> refill = FloodFill(mold, start, moldMinBounds, moldMaxBounds, true, alreadyFilled);

                if (refill.Count > 0)
                {
                    foreach (var v in refill)
                        alreadyFilled.Add(v);
                }
            }

            List<Vec3> filledLava = alreadyFilled.ToList();
            Vec3 filledMinBounds = new(999999, 999999, 99999);
            Vec3 filledMaxBounds = new();
            var filled = CreateMap(filledLava, ref filledMinBounds, ref filledMaxBounds);

            //now calculate faces, which is 6 - touching neighbors for each cube
            int exteriorSurfaceArea = filled.Select(x => 6 - x.Value.touching.Count()).Sum();
            Console.WriteLine("num cubes: " + filled.Count() + " (grid space: " + CalcMaxCubes(filledMinBounds, filledMaxBounds) + ")   with exterior faces: " + exteriorSurfaceArea + " out of max " + (6 * filled.Count));

            //something is off...lets sort and try to figure out what...
            var exteriorCubes = filled.Where(x => 6 - x.Value.touching.Count() > 0).ToList();
            int exteriorCubesFree = exteriorCubes.Select(x => 6 - x.Value.touching.Count()).Sum();
            Console.WriteLine("exterior cubes: " + exteriorCubes.Count() + " all faces = " + (exteriorCubes.Count * 6) + " free faces = " + exteriorCubesFree);

            //perhaps there are multiple bodies...
            //lets do a horrible search to see...
            //flood fill at every point in the grid, look to see if any flood fills dont match, and count them.
            //if this was all done properly, there should only two types of fills - 1: external space, 2: internal spaces
            //increase bounds so we dont catch pockets on the outside edges
            //debug output...
            
            Console.WriteLine("\nScanning for unique clusters of filled space in original grid...");
            FindUniqueFills(cubes, originalMinBounds, originalMaxBounds, false, false); //only find lava

            Console.WriteLine("\nScanning for unique clusters of empty space in original grid...");
            FindUniqueFills(cubes, originalMinBounds, originalMaxBounds, true, true); //only find nonlava

            Console.WriteLine("\nScanning for unique clusters of filled space in mold...");
            FindUniqueFills(mold, moldMinBounds, moldMaxBounds, false, false); //only find lava

            Console.WriteLine("\nScanning for unique clusters of empty space in mold...");
            FindUniqueFills(mold, moldMinBounds, moldMaxBounds, true, false); //only find nonlava

            Console.WriteLine("\nScanning for unique clusters of filled space in final lavalfill...");
            FindUniqueFills(filled, filledMinBounds, filledMaxBounds, false, false); //only find lava

            Console.WriteLine("\nScanning for unique clusters of empty space in final lavalfill...");
            FindUniqueFills(filled, filledMinBounds, filledMaxBounds, true, true); //only find nonlava

        }

        void FindUniqueFills(Dictionary<Vec3, SolidCubeGridEntry> grid, Vec3 minBounds, Vec3 maxBounds, bool excludeLava, bool extendBounds)
        {
            List<List<Vec3>> uniqueFills = new();

            Vec3 filledMinBounds = minBounds;
            Vec3 filledMaxBounds = maxBounds;

            if(extendBounds)
            {
                filledMinBounds.X -= 1;
                filledMinBounds.Y -= 1;
                filledMinBounds.Z -= 1;

                filledMaxBounds.X += 1;
                filledMaxBounds.Y += 1;
                filledMaxBounds.Z += 1;
            }

            HashSet<Vec3> alreadyFilled = new HashSet<Vec3>();

            for (Int64 x = filledMinBounds.X; x <= filledMaxBounds.X; ++x)
            {
                for (Int64 y = filledMinBounds.Y; y <= filledMaxBounds.Y; ++y)
                {
                    for (Int64 z = filledMinBounds.Z; z <= filledMaxBounds.Z; ++z)
                    {
                        var filledSearch = FloodFill(grid, new Vec3(x, y, z), filledMinBounds, filledMaxBounds, excludeLava, alreadyFilled);
                        filledSearch = filledSearch.OrderByDescending(x =>
                        {
                            return (x.X * 100000) + (x.Y * 1000) + (x.Z);
                        }).ToList();

                        if(filledSearch.Count > 0)
                        {
                            foreach(var v in filledSearch)
                                alreadyFilled.Add(v);
                        }

                        bool matched = false;
                        for (int u = 0; u < uniqueFills.Count && matched == false; ++u)
                        {
                            if (uniqueFills[u].Count == filledSearch.Count)
                            {
                                bool matchThisList = true;
                                for (int items = 0; items < uniqueFills[u].Count && matchThisList == true; ++items)
                                {
                                    if (uniqueFills[u][items] != filledSearch[items])
                                    {
                                        matchThisList = false;
                                    }
                                }

                                if (matchThisList)
                                    matched = true;
                            }
                        }

                        if (!matched && filledSearch.Count > 0)
                        {
                            uniqueFills.Add(filledSearch);
                            Console.WriteLine("Found fill of size " + filledSearch.Count + ". total unique: " + uniqueFills.Count);
                        }
                    }
                }
            }
        }

        Int64 CalcMaxCubes(Vec3 min, Vec3 max)
        {
            return (max.X - min.X) * (max.Y - min.Y) * (max.Z - min.Z);
        }

        Dictionary<Vec3, SolidCubeGridEntry> CreateMap(List<Vec3> lavaPoints, ref Vec3 minBounds, ref Vec3 maxBounds)
        {
            Dictionary<Vec3, SolidCubeGridEntry> cubes = new();

            for (int i = 0; i < lavaPoints.Count; ++i)
            {
                //insert and update neighbors
                SolidCubeGridEntry cube = new();
                cube.Point = lavaPoints[i];

                minBounds.X = Math.Min(minBounds.X, cube.Point.X);
                minBounds.Y = Math.Min(minBounds.Y, cube.Point.Y);
                minBounds.Z = Math.Min(minBounds.Z, cube.Point.Z);

                maxBounds.X = Math.Max(maxBounds.X, cube.Point.X);
                maxBounds.Y = Math.Max(maxBounds.Y, cube.Point.Y);
                maxBounds.Z = Math.Max(maxBounds.Z, cube.Point.Z);

                //fill with neighbors
                Vec3[] neighbors = GetNeighborPoints(ref cube.Point);
                foreach (var n in neighbors)
                {
                    if (cubes.TryGetValue(n, out SolidCubeGridEntry? nCube))
                    {
                        nCube.touching.Add(cube);
                        cube.touching.Add(nCube);
                    }
                }

                cubes.Add(lavaPoints[i], cube);
            }

            return cubes;
        }

        List<Vec3> FloodFill(Dictionary<Vec3, SolidCubeGridEntry> cubes, Vec3 start, Vec3 minBounds, Vec3 maxBounds, bool excludeLavaCubes, HashSet<Vec3>? exclude = null)
        {
            List<Vec3> path= new();
            path.Add(start);

            List<List<Vec3>> search = new();
            search.Add(path);

            //if start violates lava/non lava, stop already...
            //if we hit a lavacube, stop
            if (cubes.TryGetValue(start, out _))
            {
                if (excludeLavaCubes)
                    return new List<Vec3>();
            }
            else
            {
                if (!excludeLavaCubes)
                    return new List<Vec3>();
            }

            //check bounds
            if (start.X < minBounds.X ||
                start.Y < minBounds.Y ||
                start.Z < minBounds.Z ||

                start.X > maxBounds.X ||
                start.Y > maxBounds.Y ||
                start.Z > maxBounds.Z)
            {
                return new List<Vec3>();
            }

            //exclude
            if (exclude != null && exclude.Contains(start))
                return new List<Vec3>();

            HashSet<Vec3> visited = new HashSet<Vec3>();
            visited.Add(start);

            while (search.Count > 0)
            {
                List<Vec3> curPath = search[0];
                search.RemoveAt(0);

                Vec3 curPoint = curPath.Last();
                Vec3[] neighbors = GetNeighborPoints(ref curPoint);

                foreach (Vec3 n in neighbors)
                {
                    //if we already explored, stop
                    if (exclude != null && exclude.Contains(start))
                        continue;
                    
                    if (visited.Contains(n)) 
                        continue;

                    //if we hit a lavacube, stop
                    if (cubes.TryGetValue(n, out _))
                    {
                        if (excludeLavaCubes)
                            continue;
                    }
                    else
                    {
                        if (!excludeLavaCubes)
                            continue;
                    }

                    //check bounds
                    if( n.X < minBounds.X ||
                        n.Y < minBounds.Y ||
                        n.Z < minBounds.Z ||

                        n.X > maxBounds.X ||
                        n.Y > maxBounds.Y ||
                        n.Z > maxBounds.Z )
                    {
                        continue;
                    }

                    //search new path
                    List<Vec3> newPath = new(curPath);
                    newPath.Add(n);
                    visited.Add(n);
                    search.Add(newPath);
                }
            }

            return visited.ToList();
        }




    }
}
