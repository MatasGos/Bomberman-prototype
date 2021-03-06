﻿using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Model;
using Newtonsoft.Json;
using System.Xml.Schema;

namespace Server.Hubs
{
    public class GameHub : Hub
    {
        JsonSerializerSettings settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
        //Logs the player into the game, saves his information and sends a login message to everyone else
        public async Task SendLoginMessage(string username)
        {
            //TODO: Check if the game has already started and dont allow to connect
            //TODO: Check if more than 4 players are trying to connect
            if (!Server.CheckUsernames(username))
            {
                if (Server.AddPlayer(Context.ConnectionId, username))
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(username + " has logged in.");
                    Console.ResetColor();
                    await Clients.Caller.SendAsync("SuccessfulLogin", "temp");
                    await Clients.Others.SendAsync("ReceiveLoginMessage", username);
                }
                else
                {
                    await Clients.Caller.SendAsync("ReceiveMessage", "Server", "Player limit reached");
                }                
            }
            else
            {
                await Clients.Caller.SendAsync("ReceiveMessage", "Server", "This username already exists in the game");
            }
        }

        //Sends a message to everyone
        public async Task SendMessage(string username, string message)
        {
            if (message.Length > 5)
            {
                if (message.Substring(0, 5) == "/skin")
                {
                    Expression exp = new Expression(message, Context.ConnectionId, Server.GetPlayers());
                    string returnValue = (exp.initialize());
                    Console.WriteLine(returnValue);
                    string jsonPlayers = JsonConvert.SerializeObject(Server.GetPlayers(), settings);
                    await Clients.All.SendAsync("UpdatePlayerImages", jsonPlayers);
                    if (returnValue != "")
                    {
                        await Clients.Caller.SendAsync("ReceiveMessage", "SERVER", returnValue);
                    }
                    return;
                }
            }
            Console.WriteLine(username + ": " + message);
            await Clients.All.SendAsync("ReceiveMessage", username, message);
        }

        //Sends a message to start the game to everyone after someone presses the start button
        public async Task StartMessage()
        {
            await Task.Run(() =>
            {
                Server.context = this.Clients;  //Sets the context to send messages to clients not only from the hub
                Server.StartGame();
            });
        }

        public async Task UpdateSkin(string skin)
        {
            //Server.GetPlayerById(Context.ConnectionId).pictureStructure = skin;
            Server.UpdatePlayerSkin(Context.ConnectionId, skin);
            string jsonPlayers = JsonConvert.SerializeObject(Server.GetPlayers(), settings);
            await Clients.All.SendAsync("UpdatePlayerImages", jsonPlayers);
        }

        public async Task SendMoveMessage(string moveCommand)
        {
            await Task.Run(() =>
            {
                ICommand command = null;
                Player p = Server.GetPlayerById(Context.ConnectionId);
                switch (moveCommand)
                {
                    case "moveleft":
                        command = new MoveLeft(p, Server.current);
                        break;
                    case "moveright":
                        command = new MoveRight(p, Server.current);
                        break;
                    case "moveup":
                        command = new MoveUp(p, Server.current);
                        break;
                    case "movedown":
                        command = new MoveDown(p, Server.current);
                        break;
                    case "moveleftup":
                        command = new MoveLeftUp(p, Server.current);
                        break;
                    case "moveleftdown":
                        command = new MoveLeftDown(p, Server.current);
                        break;
                    case "moverightup":
                        command = new MoveRightUp(p, Server.current);
                        break;
                    case "moverightdown":
                        command = new MoveRightDown(p, Server.current);
                        break;
                }
                p.SetCommand(command);
            });
        }

        //Sends a place bomb message
        public async Task SendActionMessage(string action)
        {
            await Task.Run(() =>
            {
                Server.GetPlayerById(Context.ConnectionId).SetAction(action);
            });
        }
    }
}