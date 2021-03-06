﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Model
{
    public class PlayerControlManager
    {
        int xSize;
        int ySize;
        Unit[,] units;
        Explosive[,] explosions;

        public PlayerControlManager(int xSize, int ySize, Unit[,] units, Explosive[,] explosions)
        {
            this.xSize = xSize;
            this.ySize = ySize;
            this.units = units;
            this.explosions = explosions;
        }

        public void PlaceExplosive(Player player, double placeTime)
        {
            if (player.action == "") return;

            int[] playerCenter = getCenterPlayer(new int[] { player.x, player.y });
            int[] playerTile = getTile(playerCenter[0], playerCenter[1]);

            ExplosiveAbstractFactory factory;
            Explosive toPlace = null;

            string command = player.action;
            bool placeSuper = false;
            if (command[command.Length -1] == 'S')
            {
                placeSuper = true;
                command = command.Remove(command.Length - 1);
            }

            if (units[playerTile[0], playerTile[1]] == null)
            {
                if (placeSuper && player.hasSuperbombs)
                {
                    factory = new SuperExplosiveConcreteFactory();
                }
                else
                {
                    factory = new RegularExplosiveConcreteFactory();
                }

                switch (command)
                {
                    case "placeBomb":
                        toPlace = factory.CreateBomb(playerTile[0], playerTile[1], player.explosionPower, placeTime, player);              
                        break;
                    case "placeMine":
                        toPlace = factory.CreateMine(playerTile[0], playerTile[1], player);                    
                        break;
                    default:
                        throw new Exception();
                }
                bool canPlace = true;
                switch(toPlace)
                {
                    case Bomb x:
                        if (player.bombCount <= 0)
                        {
                            canPlace = false;
                        }
                        else
                        {
                            player.bombCount--;
                        }
                        break;
                    case SuperBomb x:
                        if (player.superBombCount <= 0)
                        {
                            canPlace = false;
                        }
                        else
                        {
                            player.superBombCount--;
                        }
                        break;
                    case Mine x:
                        if (player.mineCount <= 0)
                        {
                            canPlace = false;
                        }
                        else
                        {
                            player.mineCount--;
                        }
                        break;
                    case SuperMine x:
                        if (player.superMineCount <= 0)
                        {
                            canPlace = false;
                        }
                        else
                        {
                            player.superMineCount--;
                        }
                        break;
                    default:
                        break;
                }
                if (canPlace)
                {
                    units[playerTile[0], playerTile[1]] = toPlace;
                }
            }

            player.action = "";
        }

        public void ActivateBlock(Player player, double time, ScoreboardTemplate scoreboard)
        {
            int[] playerCenter = getCenterPlayer(new int[] { player.x, player.y });
            int[] playerTile = getTile(playerCenter[0], playerCenter[1]);
            Unit playerStandsOn = units[playerTile[0], playerTile[1]];
            List<int[]> previousBlocks = player.GetPreviousBlock();
            //Explosive playerStandsOnMine = explosions[playerTile[0], playerTile[1]];
            switch (playerStandsOn)
            {
                case Boost x:
                    PickupBoost(player, x);
                    break;
                case Teleporter x:
                    Teleport(player, x);
                    break;                
                default:
                    break;
            }
            
            List<int[]> playerCorners = new List<int[]>();
            playerCorners.Add(new int[] { player.x, player.y });
            playerCorners.Add(new int[] { player.x, player.y + 14});
            playerCorners.Add(new int[] { player.x + 14, player.y });
            playerCorners.Add(new int[] { player.x + 14, player.y + 14});
            foreach (var playerCoordinate in playerCorners)
            {
                playerTile = getTile(playerCoordinate[0], playerCoordinate[1]);
                playerStandsOn = explosions[playerTile[0], playerTile[1]];
                Unit playerStandsOnMine = units[playerTile[0], playerTile[1]];
                switch (playerStandsOn)
                {
                    case Explosion x:
                        PlayerExplode(player, time, scoreboard, x.GetOwner());
                        break;
                    case SuperExplosion x:
                        PlayerExplode(player, time, scoreboard, x.GetOwner());
                        break;
                    default:
                        break;
                }
                switch (playerStandsOnMine)
                {
                    case Mine x:
                        RegularMineExplosion(player, x, time, GetStillOccupiedBlocks(GetCurrentBlocks(player), previousBlocks));
                        break;
                    case SuperMine x:
                        SuperMineExplosion(player, x, time, GetStillOccupiedBlocks(GetCurrentBlocks(player), previousBlocks));
                        break;
                    default:
                        break;
                }
            }
            player.SetPreviousBlock(GetCurrentBlocks(player));
        }

        public void PickupBoost(Player player, Boost playerStandsOn)
        {
            units[playerStandsOn.x, playerStandsOn.y] = null;
            playerStandsOn.algorithm.UseBoost(player, units);
        }
        public void RegularMineExplosion(Player player, Mine playerStandsOn, double time, List<int[]> stillOccupied)
        {
            foreach(var block in stillOccupied)
            {
                if (playerStandsOn.x == block[0] && playerStandsOn.y == block[1])
                {
                    return;
                }
            }
            units[playerStandsOn.x, playerStandsOn.y] = null;
            explosions[playerStandsOn.x, playerStandsOn.y] = new Explosion(playerStandsOn.x, playerStandsOn.y, time, playerStandsOn.GetOwner());
        }
        public void SuperMineExplosion(Player player, SuperMine playerStandsOn, double time, List<int[]> stillOccupied)
        {
            foreach (var block in stillOccupied)
            {
                if (playerStandsOn.x == block[0] && playerStandsOn.y == block[1])
                {
                    return;
                }
            }
            units[playerStandsOn.x, playerStandsOn.y] = null;
            explosions[playerStandsOn.x, playerStandsOn.y] = new SuperExplosion(playerStandsOn.x, playerStandsOn.y, time, playerStandsOn.GetOwner());
        }

        public void PlayerExplode(Player player, double time, ScoreboardTemplate scoreboard, Player bombOwner)
        {
            if (!(player.invincibleUntil > time))
            {
                player.BecomeInvincible(time);
                player.ReduceHealth();
                if (player != bombOwner)
                {
                    if (player.IsAlive())
                    {
                        scoreboard.AddScore(bombOwner, 1);
                    }
                    else
                    {
                        scoreboard.AddScore(bombOwner, 3);
                        scoreboard.ChangeStatus(player);
                    }
                }
                else
                {
                    if (player.IsAlive())
                    {
                        scoreboard.AddScore(player, -1);
                    }
                    else
                    {
                        scoreboard.AddScore(player, -1);
                        scoreboard.ChangeStatus(player);
                    }
                    
                }
            }
        }

        public void Teleport(Player player, Teleporter playerStandsOn)
        {
            if (playerStandsOn.HasDestination())
            {
                int[] destination = GetTileToCoordinates(playerStandsOn.GetDestination().x, playerStandsOn.GetDestination().y);
                player.SetPos(destination[0], destination[1]);
                player.ClearCommandHistory();
            }
        }

        public void Move(Player movingPlayer, int x, int y, int speed)
        {
            int px = x;
            int py = y;
            List<int[]> previousBlocks = movingPlayer.GetPreviousBlock();
            //movingPlayer.SetPreviousBlock(GetCurrentBlocks(movingPlayer));
            if (px == 0 && py == 0)
            {
                return;
            }

            Unit[] b = getNearbyBlocks(movingPlayer.x, movingPlayer.y, GetStillOccupiedBlocks(GetCurrentBlocks(movingPlayer), previousBlocks));
            MovementHandler full = new FullMove();
            full.SetNextChain(new HalfMove());
            full.GetNextChain().SetNextChain(new QuarterMove());
            full.GetNextChain().GetNextChain().SetNextChain(new OneMove());

            full.Calculate(movingPlayer, px, py, speed, this, b);
        }

        public bool isOccupiedSquared(int[,] edges, Unit[] b)
        {
            for (int i = 0; i < 4; i++)
            {
                if (isOccupied(new int[] { edges[i, 0], edges[i, 1] }, b))
                {
                    return false;
                }
            }
            return true;
        }

        public bool isOccupied(int[] xy, Unit[] b)
        {
            int[] topLeft = getTile(xy[0], xy[1]);

            for (int i = 0; i < 25; i++)
            {
                if (b[i] != null)
                {
                    if (b[i].x == topLeft[0] && b[i].y == topLeft[1] && b[i].isSolid == true)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public int[,] getEdges(int[] xy)
        {
            int[,] edges = new int[4, 2];
            edges[0, 0] = xy[0];
            edges[0, 1] = xy[1];

            edges[1, 0] = xy[0] + 14;
            edges[1, 1] = xy[1];

            edges[2, 0] = xy[0];
            edges[2, 1] = xy[1] + 14;

            edges[3, 0] = xy[0] + 14;
            edges[3, 1] = xy[1] + 14;

            return edges;
        }

        public int[] getCenterPlayer(int[] xy)
        {
            int[] result = new int[2];
            result[0] = xy[0] + 15 / 2;
            result[1] = xy[1] + 15 / 2;
            return result;
        }

        public int[] getTile(int x, int y)
        {
            int[] result = new int[2];

            result[0] = x / 25;
            result[1] = y / 25;

            return result;
        }

        public int[] GetTileToCoordinates(int x, int y)
        {
            int[] result = new int[2];
            result[0] = x * 25 + 5;
            result[1] = y * 25 + 5;
            return result;
        }

        public Unit[] getNearbyBlocks(int posx, int posy, List<int[]> excludedBlocks)
        {
            Unit[] b = new Unit[25];
            int[] xy = getTile(posx, posy);
            int count = 0;
            bool canAdd = true;
            for (int x = Math.Max(0, xy[0] - 2); x <= Math.Min(xy[0] + 2, xSize - 1); x++)
            {
                for (int y = Math.Max(0, xy[1] - 2); y <= Math.Min(xy[1] + 2, ySize - 1); y++)
                {
                    foreach (var block in excludedBlocks)
                    {
                        if (block[0] == x && block[1] == y)
                        {
                            canAdd = false;
                        }
                    }
                    if (canAdd)
                    {
                        b[count++] = units[x, y];
                    }
                    canAdd = true;
                }
            }
            return b;
        }

        private List<int[]> GetCurrentBlocks(Player player)
        {
            List<int[]> playerCorners = new List<int[]>();
            playerCorners.Add(new int[] { player.x, player.y });
            playerCorners.Add(new int[] { player.x, player.y + 14 });
            playerCorners.Add(new int[] { player.x + 14, player.y });
            playerCorners.Add(new int[] { player.x + 14, player.y + 14 });
            for (int i = 0; i < 4; i++)
            {
                playerCorners[i] = getTile(playerCorners[i][0], playerCorners[i][1]);
            }
            return playerCorners;
        }

        private List<int[]> GetStillOccupiedBlocks(List<int[]> currentBlocks, List<int[]> previousBlocks)
        {
            List<int[]> ignoredBlocks = new List<int[]>();
            for (int i = 0; i < 4; i++)
            {
                if (currentBlocks[i][0] == previousBlocks[i][0] && currentBlocks[i][1] == previousBlocks[i][1])
                {
                    ignoredBlocks.Add(currentBlocks[i]);
                }
            }
            return ignoredBlocks;
        }
    }
}
