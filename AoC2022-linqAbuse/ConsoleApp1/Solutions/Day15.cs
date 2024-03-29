﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Schema;
using static ConsoleApp1.Solutions.Day14;
using static ConsoleApp1.Utils;

namespace ConsoleApp1.Solutions
{
    internal class Day15 : AbstractPuzzle
    {
        public enum State
        {
            None,
            Beacon,
            Sensor,
            Seen,
        }

        override public void Part1()
        {
            int rowOfInterestYVal = 2000000;

            Dictionary<Point64, State> tiles = new();
            Dictionary<Point64, State> canSeeTiles = new();

            //parse...
            var lines = File.ReadAllText(InputFile!).Split("\r\n").Select(x => x.Trim().Split("="));
            foreach(var line in lines)
            {
                Point64 sensor = new Point64();
                Point64 beacon = new Point64();
                sensor.X = Int64.Parse(line[1].Split(",")[0].Trim());
                sensor.Y = Int64.Parse(line[2].Split(":")[0].Trim());
                beacon.X = Int64.Parse(line[3].Split(",")[0].Trim());
                beacon.Y = Int64.Parse(line[4].Trim());

                tiles.Add(sensor, State.Sensor);

                if (!tiles.ContainsKey(beacon))
                    tiles.Add(beacon, State.Beacon);

                //basic dist check to exclude far away sensors
                Point64 sensorBeaconDistVec = sensor - beacon;
                Int64 sensorBeaconDist = Math.Abs(sensorBeaconDistVec.X) + Math.Abs(sensorBeaconDistVec.Y);

                Point64 nearestRowPoint = new Point64() { X = sensor.X, Y = rowOfInterestYVal };
                Point64 rowDistVec = sensor - nearestRowPoint;
                Int64 rowDist = Math.Abs(rowDistVec.X) + Math.Abs(rowDistVec.Y);

                //early out
                if (rowDist > sensorBeaconDist)
                {
                    Console.WriteLine("Skipping sensor " + sensor + " due to dist to beacon being " + sensorBeaconDist + " and dist to rowVal being " + rowDist);
                    continue;
                }

                Console.WriteLine("processing sensor " + sensor + " due to dist to beacon being " + sensorBeaconDist + " and dist to rowVal being " + rowDist);

                //grab a rough range via calculating circle intersection with line
                Int64 c = sensorBeaconDist * sensorBeaconDist;
                Int64 a = rowDist;
                double db = Math.Sqrt((double)(c - a));
                Int64 b = (Int64)db;

                //now add these points to our list to make sure they are all unique
                for(Int64 x = nearestRowPoint.X - (b * 2); x < nearestRowPoint.X + (b * 2); ++x)
                {
                    Point64 key = new Point64();
                    key.X = x;
                    key.Y = rowOfInterestYVal;

                    //lets just make sure it actually is manhatten dist away, since circle will reach farther than mandist..
                    Point64 checkDist = sensor - key;
                    Int64 checkManDist = Math.Abs(checkDist.X) + Math.Abs(checkDist.Y);

                    if (!(checkManDist <= sensorBeaconDist))
                        continue;

                    if (!canSeeTiles.ContainsKey(key))
                        canSeeTiles.Add(key, State.Seen);
                }
            }

            var row = canSeeTiles.Where(x => x.Key.Y == rowOfInterestYVal).ToList();

            //need to subtract the sensor and beacon on that row i guess
            var objsInRow = tiles.Where(x => x.Key.Y == rowOfInterestYVal).ToList();

            Console.WriteLine("seen tiles in row y=" + rowOfInterestYVal + ": " + ( row.Count - objsInRow.Count));
        }


        public record SensorData(Point64 sensor, Point64 beacon, State Type, Int64 distance);


        override public void Part2()
        {
            Dictionary<Point64, SensorData> tiles = new();

            //parse...
            var lines = File.ReadAllText(InputFile!).Split("\r\n").Select(x => x.Trim().Split("="));
            foreach (var line in lines)
            {
                Point64 sensor = new Point64();
                Point64 beacon = new Point64();
                sensor.X = Int64.Parse(line[1].Split(",")[0].Trim());
                sensor.Y = Int64.Parse(line[2].Split(":")[0].Trim());
                beacon.X = Int64.Parse(line[3].Split(",")[0].Trim());
                beacon.Y = Int64.Parse(line[4].Trim());

                //precal distance
                Point64 sensorBeaconDistVec = sensor - beacon;
                Int64 sensorBeaconDist = Math.Abs(sensorBeaconDistVec.X) + Math.Abs(sensorBeaconDistVec.Y);

                tiles.Add(sensor, new SensorData(sensor, beacon, State.Sensor, sensorBeaconDist));

                if (!tiles.ContainsKey(beacon))
                    tiles.Add(beacon, new SensorData(sensor, beacon, State.Beacon, sensorBeaconDist));

            }

            //grab just sensors
            var tileList = tiles.ToList().Where(x => x.Value.Type != State.Beacon).ToList();
            tileList = tileList.OrderByDescending(x => x.Value.sensor.X - x.Value.distance).Reverse().ToList();

            Scan(tileList, new Point64() { X = 0, Y = 0 }, new Point64() { X = 4000000, Y = 4000000 });
        }



        public static void Scan(List<KeyValuePair<Point64, SensorData>> tileList ,Point64 minBound, Point64 maxBound)
        {
            bool collided = false;
            Point64 curPoint = new Point64() {  X= minBound.X, Y = minBound.Y };

            for (; curPoint.Y <= maxBound.Y; curPoint.Y += 1)
            {
                curPoint.X = minBound.X;

                for (; curPoint.X <= maxBound.X;)
                {
                    collided = false;

                    foreach (var tileProps in tileList)
                    {
                        Point64 sensor = tileProps.Value.sensor;

                        Point64 pointSensorDistVec = sensor - curPoint;
                        Int64 pointSensorDist = Math.Abs(pointSensorDistVec.X) + Math.Abs(pointSensorDistVec.Y);

                        if (pointSensorDist <= tileProps.Value.distance)
                        {
                            collided = true;
                            Int64 vertDist = Math.Abs(pointSensorDistVec.Y);
                            Int64 remainingDist = tileProps.Value.distance - vertDist;
                            curPoint.X = sensor.X + remainingDist + 1;
                            break;
                        }
                    }

                    if (collided == false)
                        break;
                }

                if (collided == false)
                    break;
            }

            if (collided == false)
            {
                Console.WriteLine("Non collided point: " + curPoint);
                Int64 score = (curPoint.X * 4000000) + curPoint.Y;
                Console.WriteLine("Score: " + score);
            }
        }
    }



}
