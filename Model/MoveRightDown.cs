﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Model
{
    public class MoveRightDown : ICommand
    {
        private Player player;
        private MapFacade facade;
        private int speed;
        public MoveRightDown(Player player, MapFacade facade)
        {
            this.player = player;
            this.speed = player.speed;
            this.facade = facade;
        }
        public void Execute()
        {
            facade.Move(player, 1, 1, speed);
        }

        public void Undo()
        {
            facade.Move(player, -1, -1, speed);
        }
    }
}
