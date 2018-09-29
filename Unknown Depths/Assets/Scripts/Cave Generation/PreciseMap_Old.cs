﻿using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

/// <summary>
/// 
///Precise Map Class (OLD VERSION)
///This class contains code references to both a tutorial located on
///the Unity site, and a tutorial found on LinkedIn Learning/Lynda by
///Sebstian Lague and Jesse Freeman Respectively
///
///https://unity3d.com/learn/tutorials/projects/procedural-cave-generation-tutorial/
///https://www.linkedin.com/learning/unity-5-2d-random-map-generation
///
/// </summary>


public enum TilePiece_OLD
{
    EMPTY = -1,
    GRASS = 15,
    TREE = 16,
    HILLS = 17,
    MOUNTAINS = 18,
    TOWNS = 19,
    CASTLE = 20,
    MONSTER = 21
}

public class PreciseMap_Old {

    public PreciseTile[] mapTiles; // Array of Tiles for the map
    public int col; //Map Size Columns/Length
    public int row; //Map Size Rows/Height

    public PreciseTile[] CoastTiles
    {
        get
        {
            return mapTiles.Where(t => t.AutoTileID < (int)TilePiece_OLD.GRASS).ToArray();
        }
    }

    public PreciseTile[] LandTiles
    {
        get
        {
            return mapTiles.Where(t => t.AutoTileID == (int)TilePiece_OLD.GRASS).ToArray();
        }
    }

    public void CreateMap(int width, int height)
    {
        row = width;// Will have variable names of r, x, i / Need to fix to be more uniform later
        col = height;// Will have variable names of c, y, j / Need to fix to be more uniform later

        mapTiles = new PreciseTile[row * col];

        CreateTiles();
    }

    public void CreateTiles()
    {
        var Total = mapTiles.Length;
        for(var i = 0; i < Total; i++)
        {
            var tile = new PreciseTile();
            tile.TileID = i;
            mapTiles[i] = tile;
        }

        FindNeighbors();
    }

    public void CreateCave(bool useSeed, string newSeed,int caveErode, int roomThresh)
    {
        var seed = "";
        if (useSeed)
        {
            seed = newSeed;
        }
        else
        {
            seed = Time.time.ToString();
        }

        System.Random psuRand = new System.Random(seed.GetHashCode());

        var total = mapTiles.Length;
        var MaxCol = col;
        var MaxRow = row;
        var Column = 0;
        var Row = 0;

        for(var i = 0; i < total; i++)
        {
            Column = i % MaxCol;
            if(Row == 0 || Row == MaxRow - 1 || Column == 0 || Column == MaxCol - 1)
            {
                Debug.Log(Column);
                mapTiles[i].ClearNeighbors();
                mapTiles[i].AutoTileID = (int)TilePiece_OLD.EMPTY;
            }
            else
            {
                if(psuRand.Next(0,100) < caveErode)
                {
                    mapTiles[i].ClearNeighbors();
                    mapTiles[i].AutoTileID = (int)TilePiece_OLD.EMPTY;
                }
            }

            if(Column == (MaxCol - 1))
            {
                Row++;
            }
        }

        DetectRegions(roomThresh);
    }

    private void FindNeighbors()
    {
        for (var x = 0; x < row; x++)
        {
            for (var y = 0; y < col; y++)
            {
                var tile = mapTiles[col * x + y];

                if (x < row - 1)
                {
                    tile.AddNeighbor(Sides.BOTTOM, mapTiles[col * (x + 1) + y]);
                }

                if (y < col - 1)
                {
                    tile.AddNeighbor(Sides.RIGHT, mapTiles[col * x + y + 1]);
                }

                if (y > 0)
                {
                    tile.AddNeighbor(Sides.LEFT, mapTiles[col * x + y - 1]);
                }

                if (x > 0)
                {
                    tile.AddNeighbor(Sides.TOP, mapTiles[col * (x - 1) + y]);
                }
            }
        }
    }

    /// <summary>
    /// Cavern class and respective methods below
    /// </summary>


    class Cavern : IComparable<Cavern>
    {
        public List<PreciseTileChecker> tiles;
        public List<PreciseTileChecker> edges;
        public List<Cavern> connectedCaverns;
        public int caveSize;

        public bool isAccessibleFromMainCavern;
        public bool isMainCavern = false;

        public Cavern()
        {
            //Literally does nothing, Congrats this is a useless constructor
        }

        public Cavern(List<PreciseTileChecker> caveTiles, PreciseTile[] map, int mapRow, int mapCol)
        {
            //This is the real constructor
            tiles = caveTiles; //List of tiles in the cavern we are checking
            caveSize = tiles.Count; //How many tiles we have to check
            connectedCaverns = new List<Cavern>(); //Contains rooms this object is connected to

            edges = new List<PreciseTileChecker>();
            foreach (PreciseTileChecker tile in tiles)
            {
                for (var x = tile.tileX - 1; x <= tile.tileX + 1; x++)
                {
                    for (var y = tile.tileY - 1; y <= tile.tileY + 1; y++)
                    {
                        if (x == tile.tileX || y == tile.tileY)
                        {
                            if (map[mapCol * x + y].AutoTileID >= 0)
                            {
                                edges.Add(tile);
                            }
                        }
                    }
                }
            }
        }

        public void SetAccessibleFromMainCavern()
        {
            if (!isAccessibleFromMainCavern)
            {
                isAccessibleFromMainCavern = true;
                foreach(Cavern connectedCave in connectedCaverns)
                {
                    connectedCave.SetAccessibleFromMainCavern();
                }
            }
        }

        public static void ConnectCaves(Cavern caveA, Cavern caveB)
        {
            if (caveA.isAccessibleFromMainCavern)
            {
                caveB.SetAccessibleFromMainCavern();
            }
            else if (caveB.isAccessibleFromMainCavern)
            {
                caveA.SetAccessibleFromMainCavern();
            }
            caveA.connectedCaverns.Add(caveB);
            caveB.connectedCaverns.Add(caveA);
        }

        public bool IsConnected(Cavern other)
        {
            return connectedCaverns.Contains(other);
        }

        public int CompareTo(Cavern other)
        {
            return other.caveSize.CompareTo(caveSize);
        }
    }

    private void ConnectCaverns(List<Cavern> Caves, bool forceAccessibilityFromMain = false)
    {
        List<Cavern> caveListA = new List<Cavern>();
        List<Cavern> caveListB = new List<Cavern>();

        if (forceAccessibilityFromMain)
        {
            foreach(Cavern cave in Caves)
            {
                if (cave.isAccessibleFromMainCavern)
                {
                    caveListB.Add(cave);
                }
                else
                {
                    caveListA.Add(cave);
                }
            }
        }
        else
        {
            caveListA = Caves;
            caveListB = Caves;
        }
        int closestDistance = 0;
        PreciseTileChecker bestTileA = new PreciseTileChecker();
        PreciseTileChecker bestTileB = new PreciseTileChecker();
        Cavern bestCaveA = new Cavern();
        Cavern bestCaveB = new Cavern();

        bool connectonFound = false;

        foreach (Cavern caveA in caveListA)
        {
            if (!forceAccessibilityFromMain)
            {
                connectonFound = false;
                if(caveA.connectedCaverns.Count > 0)
                {
                    continue;
                }
            }

            foreach(Cavern caveB in caveListB)
            {
                if(caveA == caveB || caveA.IsConnected(caveB))
                {
                    continue;
                }

                for(int tileIndexA = 0; tileIndexA < caveA.edges.Count; tileIndexA++)
                {
                    for(int tileIndexB = 0; tileIndexB < caveB.edges.Count; tileIndexB++)
                    {
                        PreciseTileChecker tileA = caveA.edges[tileIndexA];
                        PreciseTileChecker tileB = caveB.edges[tileIndexB];

                        int distance = (int)(Mathf.Pow((tileA.tileX - tileB.tileX), 2) + Mathf.Pow((tileA.tileY - tileB.tileY), 2));

                        if (distance < closestDistance || !connectonFound)
                        {
                            closestDistance = distance;
                            connectonFound = true;
                            bestTileA = tileA;
                            bestTileB = tileB;
                            bestCaveA = caveA;
                            bestCaveB = caveB;
                        }
                    }
                }
            }

            /*if (connectonFound && !forceAccessibilityFromMain)
            {
                CreatePath(bestCaveA, bestCaveB, bestTileA, bestTileB);
            }*/
            CreatePath(bestCaveA, bestCaveB, bestTileA, bestTileB);
        }

        /*if(connectonFound && forceAccessibilityFromMain)
        {
            CreatePath(bestCaveA, bestCaveB, bestTileA, bestTileB);
            ConnectCaverns(Caves, true);
        }

        if (!forceAccessibilityFromMain)
        {
            ConnectCaverns(Caves, true);
        }*/

    }

    private void CreatePath(Cavern caveA, Cavern caveB, PreciseTileChecker tileA, PreciseTileChecker tileB)
    {
        Cavern.ConnectCaves(caveA, caveB);
        //Debug.Log("Tile A ID:" + mapTiles[col * tileA.tileX + tileA.tileY].TileID + " Tile B ID:" + mapTiles[col * tileB.tileX + tileB.tileY].TileID);
        //Debug.DrawLine(DisplayLine(tileA), DisplayLine(tileB), Color.green, 100);
        /*List<PreciseTileChecker> line = GetPassageTiles(tileA, tileB);
        foreach (PreciseTileChecker t in line)
        {
            DrawPassage(t, 1);
        }*/
    }

    Vector3 DisplayLine(PreciseTileChecker tile)
    {
        return new Vector3((tile.tileX * 16 + .5f), (-tile.tileY * 16 + .5f), 2);
    }

    /// <summary>
    /// Precise Tile Checker structure and needed methods below
    /// </summary>

    struct PreciseTileChecker
    {
        public int tileX;
        public int tileY;

        public PreciseTileChecker(int x, int y)
        {
            tileX = x;
            tileY = y;
        }
    }

    private void DetectRegions(int roomSize)
    {
        List<List<PreciseTileChecker>> caveRegions = GetRegions();
        List<Cavern> survivingRegions = new List<Cavern>();
        foreach (List<PreciseTileChecker> cavern in caveRegions)
        {
            //Debug.Log(cavern.Count);
            if(cavern.Count <= roomSize)
            {
                foreach(PreciseTileChecker Tile in cavern)
                {
                    mapTiles[col * Tile.tileX + Tile.tileY].ClearNeighbors();
                    mapTiles[col * Tile.tileX + Tile.tileY].AutoTileID = (int)TilePiece_OLD.EMPTY;
                }
            }
            else
            {
                survivingRegions.Add(new Cavern(cavern, mapTiles, row, col));
            }
        }

        Debug.Log("Going to connect regions now");

        survivingRegions.Sort();
        survivingRegions[0].isMainCavern = true;
        survivingRegions[0].isAccessibleFromMainCavern = true;
        ConnectCaverns(survivingRegions);
    }

    private List<List<PreciseTileChecker>> GetRegions()
    {
        List<List<PreciseTileChecker>> caverns = new List<List<PreciseTileChecker>>();
        int[,] mapFlags = new int[row, col];
        for(var r = 0; r < row; r++)
        {
            for(var c = 0; c < col; c++)
            {
                if(mapFlags[r, c] == 0 && mapTiles[col * r + c].AutoTileID >= (int)TileType.EMPTY && mapTiles[col * r + c] != null)
                {
                    List<PreciseTileChecker> newCavern = GetRegionTiles(r, c);
                    caverns.Add(newCavern);

                    foreach(PreciseTileChecker tile in newCavern)
                    {
                        mapFlags[tile.tileX, tile.tileY] = 1;
                    }
                }
            }
        }
        return caverns;
    }

    private List<PreciseTileChecker> GetRegionTiles(int r, int c)
    {
        List<PreciseTileChecker> caveTiles = new List<PreciseTileChecker>();
        int[,] mapFlags = new int[row , col];

        Queue<PreciseTileChecker> queue = new Queue<PreciseTileChecker>();
        queue.Enqueue(new PreciseTileChecker(r, c));
        while (queue.Count > 0)
        {
            PreciseTileChecker tile = queue.Dequeue();
            if(mapTiles[col* r + c].AutoTileID > (int)TilePiece_OLD.EMPTY && mapTiles[col * r + c] != null)
            {
                caveTiles.Add(tile);
                mapFlags[r, c] = 1;
                for (var x = tile.tileX - 1; x <= tile.tileX + 1; x++)
                {
                    for (var y = tile.tileY - 1; y <= tile.tileY + 1; y++)
                    {

                        if (InsideMap(x, y) && (y == tile.tileY || x == tile.tileX))
                        {
                            if (mapFlags[x, y] == 0 && mapTiles[col * x + y].AutoTileID > -1)
                            {
                                mapFlags[x, y] = 1;
                                queue.Enqueue(new PreciseTileChecker(x, y));
                            }
                        }
                    }
                }
            }
        }
        return caveTiles;
    }

    private bool InsideMap(int x, int y)
    {
        return x >= 0 && x < row && y >= 0 && y < col;
    }

    /// <summary>
    /// Methods below are used to draw the paths connecting the caverns across the map
    /// </summary>

    private void DrawPassage(PreciseTileChecker t, int r)
    {
        for(var x = -r; x <= r; x++)
        {
            for(var y = -r; y <= r; y++)
            {
                if(x*x + y*y <= r * r)
                {
                    int drawX = t.tileX + x;
                    int drawY = t.tileY + y;
                    if(InsideMap(drawX, drawY))
                    {
                        PreciseTile tile = mapTiles[col * drawX + drawY];

                        if (drawX < row - 1)
                        {
                            tile.AddNeighbor(Sides.BOTTOM, mapTiles[col * (drawX + 1) + drawY]);
                        }

                        if (drawY < col - 1)
                        {
                            tile.AddNeighbor(Sides.RIGHT, mapTiles[col * drawX + drawY + 1]);
                        }

                        if (drawY > 0)
                        {
                            tile.AddNeighbor(Sides.LEFT, mapTiles[col * drawX + drawY - 1]);
                        }

                        if (drawX > 0)
                        {
                            tile.AddNeighbor(Sides.TOP, mapTiles[col * (drawX - 1) + drawY]);
                        }
                    }
                }
            }
        }
    }

    private List<PreciseTileChecker> GetPassageTiles(PreciseTileChecker from, PreciseTileChecker to)
    {
        List<PreciseTileChecker> line = new List<PreciseTileChecker>();

        int x = from.tileX;
        int y = from.tileY;

        int dx = to.tileX;
        int dy = to.tileY;

        bool inverted = false;
        int step = Math.Sign(dx);
        int gradient = Math.Sign(dy);

        int longest = Mathf.Abs(dx);
        int shortest = Mathf.Abs(dy);

        if(longest < shortest)
        {
            inverted = true;
            longest = Mathf.Abs(dy);
            shortest = Mathf.Abs(dx);

            step = Math.Sign(dy);
            gradient = Math.Sign(dx);
        }

        int gradientAcc = longest / 2;
        for(var i = 0; i < longest; i++)
        {
            line.Add(new PreciseTileChecker(x, y));

            if (inverted)
            {
                y += step;
            }
            else
            {
                x += step;
            }

            gradientAcc += shortest;
            if(gradientAcc >= longest)
            {
                if (inverted)
                {
                    x += gradient;
                }
                else
                {
                    y += gradient;
                }

                gradientAcc -= longest;
            }
        }

        return line;
    }
}