﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace prototype.Classes
{
    public class Game
    {
        private Bitmap background, bombPic, wallPic;
        private LinkedList<Player> players;
        private LinkedList<Bomb> bombs;
        private Map map;
        private const int playerSize = 15;
        const int xsize = 20;
        const int ysize = 20;
        public static readonly int[] backcolor = { 98, 65, 8 };
        private Bitmap[] playerPictures;

        //placeholder kol nera explosion klases
        const int explosionTime = 200;

        public Game()
        {
            this.players = new LinkedList<Player>();
            this.map = new Map(xsize, ysize, backcolor, 0);
            makeMap();
            getPlayerPics();
            this.bombPic = new Bitmap("bomb.jpg");
            this.wallPic = new Bitmap("wall.png");
        }
        // Pakeicia zemelapi
        public void setMap(Map map)
        {
            this.map = map;
        }
        // Atnaujina zemelapi pagal turima player, wall sarasa
        public void uploadGame()
        {
            makeMap();
        }
        // Nuklonuoja Zemelapi su jo sienomis
        public Bitmap getMap()
        {
            RectangleF cloneRect = new RectangleF(0, 0, 25 * xsize + xsize * 2, 25 * ysize + ysize * 2);
            System.Drawing.Imaging.PixelFormat format = background.PixelFormat;
            return background.Clone(cloneRect, format);
        }
        // Atnaujina players, bombs ir boxes sarasa
        public void update(List<string> playersUpdated)
        {
            this.players.Clear();
            foreach (string pp in playersUpdated)
            {
                string[] playerInfo = pp.Split('+');
                Player temp = new Player(playerInfo[0], Int32.Parse(playerInfo[1]), Int32.Parse(playerInfo[2]));
                this.players.AddFirst(temp);
            }
        }
        // zemelapio atnaujinimas
        public void makeMap()
        {
            this.background = new Bitmap(25 * xsize + xsize * 2, 25 * ysize + ysize * 2);
            this.bombPic = new Bitmap("bomb.jpg");
            this.wallPic = new Bitmap("wall.png");

            Color black = Color.FromArgb(0, 0, 0);
            Color newColor = Color.FromArgb(backcolor[0], backcolor[1], backcolor[2]);
            for (int x = 0; x < background.Width; x++)
            {
                for (int y = 0; y < background.Height; y++)
                {
                    background.SetPixel(x, y, newColor);
                }
            }
            for (int x = 27; x < background.Width; x += 27)
            {
                for (int y = 0; y < background.Height; y++)
                {
                    background.SetPixel(x - 2, y, black);
                    background.SetPixel(x - 1, y, black);
                }
            }
            for (int y = 27; y < background.Height; y += 27)
            {
                for (int x = 0; x < background.Height; x++)
                {
                    background.SetPixel(x, y - 1, black);
                    background.SetPixel(x, y - 2, black);
                }
            }
            Wall[] walls = map.getWalls();
            for (int i = 0; i < map.getWallsCount(); i++)
            {
                int[] xy = walls[i].getPos();
                for (int x = 0; x < 25; x++)
                {
                    for (int y = 0; y < 25; y++)
                    {
                        background.SetPixel(x + xy[0], y + xy[1], wallPic.GetPixel(x, y));
                    }
                }
            }
        }
        // Ant zemelapio su walls, nupiesia players, bombs ir t.t., ir ji grazina
        public Bitmap getGame()
        {
            bombs = map.getBombs();
            Bitmap newMap = getMap();
            foreach (Player player in this.players)
            {
                int[] xy = player.getPos();
                for (int x = 0; x < playerSize; x++)
                {
                    for (int y = 0; y < playerSize; y++)
                    {
                        newMap.SetPixel(x + xy[0], y + xy[1], playerPictures[0].GetPixel(x, y));
                    }
                }
            }
            LinkedList<Bomb> bombsRemove = new LinkedList<Bomb>();
            foreach (Bomb bomb in bombs)
            {
                int tick = bomb.tick();
                if (tick > 0)
                {
                    int[] xy = bomb.getPos();
                    for (int x = 0; x < 25; x++)
                    {
                        for (int y = 0; y < 25; y++)
                        {
                            newMap.SetPixel(x + xy[0], y + xy[1], bombPic.GetPixel(x, y));
                        }
                    }
                }
                else if (tick > -explosionTime)
                {
                    int[] xy = bomb.getPos();
                    int power = bomb.getPower();
                    for (int x = 0 - 27 * power; x < 25 + 27 * power; x++)
                    {
                        if (x > 0 && x < 25)
                        {
                            for (int y = 0 - 27 * power; y < 25 + 27 * power; y++)
                            {
                                Color explosionColor = Color.FromArgb(230, 114, 56);
                                newMap.SetPixel(Math.Max(x + xy[0], 1), Math.Max(Math.Min(y + xy[1], background.Height - 1), 0), explosionColor);
                            }
                        }
                        else
                        {
                            for (int y = 0; y < 25; y++)
                            {
                                Color explosionColor = Color.FromArgb(230, 114, 56);
                                newMap.SetPixel(Math.Max(Math.Min(x + xy[0], background.Height - 1), 0), y + xy[1], explosionColor);
                            }
                        }
                    }
                }
                else
                {
                    bombsRemove.AddFirst(bomb);
                }
            }
            foreach (Bomb bomb in bombsRemove)
            {
                bombs.Remove(bomb);
            }
            return newMap;
        }
        public void addBomb(string playerId)
        {
            map.addBomb(playerId);
        }
        // testavimui. Dont delete pls.
        public Player getPlayer(int id)
        {
            foreach (Player player in players)
            {
                return player;
            }
            return null;
        }
        // Zaideju nuotrauku pakrovimo placeholderis
        private void getPlayerPics()
        {
            this.playerPictures = new Bitmap[4];
            for (int i = 0; i < 4; i++)
            {
                this.playerPictures[i] = new Bitmap("p1.png");
            }
        }
    }
}
