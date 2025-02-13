﻿using Microsoft.AspNetCore.Mvc;
using MQTTnet.Client;
using MQTTnet;
using VierGewinnt.Data.Interfaces;
using VierGewinnt.Data.Models;
using VierGewinnt.ViewModels;
using System.Text;
using VierGewinnt.Services;
using VierGewinnt.Data.Model;
using System.Diagnostics;
using MQTTBroker;
using Microsoft.AspNetCore.SignalR;
using VierGewinnt.Hubs;
using Microsoft.EntityFrameworkCore;
using VierGewinnt.Data;

namespace VierGewinnt.Controllers
{
    public class GameController : Controller
    {
        private readonly IGameRepository _gameRepository;
        //private readonly IAccountRepository _accountRepository;
        private readonly IHubContext<BoardEvEHub> _hubContext;
        //private static readonly IList<GameBoard> runningGames;

        private static string connectionstring = "Server=localhost;Database=4Gewinnt;Trusted_connection=True;TrustServerCertificate=True;";

        //static GameController()
        //{
        //    runningGames = new List<GameBoard>();
        //}

        public GameController(IGameRepository gameRepository, IHubContext<BoardEvEHub> hubContext,
            IAccountRepository accountRepository)
        {
            _gameRepository = gameRepository;
            _hubContext = hubContext;
            //_accountRepository = accountRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Board(int gameId)
        {
            GameViewModel gameViewModel = new GameViewModel();
            GameBoard gameBoard = await _gameRepository.GetByIdAsync(new GameBoard() { ID = gameId });
            gameViewModel.Board = gameBoard;
            return View(gameViewModel);
        }

        [HttpGet]
        public async Task<IActionResult> BoardPvE(int gameId)
        {
            GameViewModel gameViewModel = new GameViewModel();
            GameBoard gameBoard = await _gameRepository.GetByIdAsync(new GameBoard() { ID = gameId });
            gameViewModel.Board = gameBoard;
            return View(gameViewModel);
        }

        [HttpGet]
        public async Task<IActionResult> BoardEvE(string robotOneName, string robotTwoName)
        {
            GameViewModel gameViewModel = new GameViewModel();

            GameBoard gameBoard = new GameBoard();

            Robot robotOne = new Robot() { Name = robotOneName };
            Robot robotTwo = new Robot() { Name = robotTwoName };

            RobotVsRobotManager.robotsInGame.Add(robotOneName, 1);
            RobotVsRobotManager.robotsInGame.Add(robotTwoName, 2);

            gameBoard.PlayerOneID = robotOne.Name;
            gameBoard.PlayerTwoID = robotTwo.Name;
            gameViewModel.Board = gameBoard;

            await RobotVsRobotManager.SubscribeToFeedbackTopic();
            RobotVsRobotManager.hubContext = _hubContext;
            RobotVsRobotManager.moves = new Move[7, 6];
            RobotVsRobotManager.currentGame = gameBoard;
            RobotVsRobotManager.currentRobotMove = robotOne.Name;


            RobotVsRobotManager.InitColDepth();

            return View(gameViewModel);
        }



        //public async Task SubscribeAwaitBestMoveAsync(string topic, int gameId)
        //{

        //    string broker = "localhost";
        //    int port = 1883;
        //    string clientId = Guid.NewGuid().ToString();

        //    // Create a MQTT client factory
        //    var factory = new MqttFactory();

        //    // Create a MQTT client instance
        //    IMqttClient mqttClient = factory.CreateMqttClient();

        //    // Create MQTT client options
        //    var options = new MqttClientOptionsBuilder()
        //        .WithTcpServer(broker, port)
        //        .WithClientId(clientId)
        //        .WithCleanSession(true)
        //        .Build();

        //        await ConnectToMQTTBroker(mqttClient, options, topic, gameId);
        //}

        //private async Task ConnectToMQTTBroker(IMqttClient mqttClient, MqttClientOptions options, string topic, int gameId)
        //{
        //    // Connect to MQTT broker
        //    var connectResult = await mqttClient.ConnectAsync(options);

        //    int currGameId = gameId;

        //    if (connectResult.ResultCode == MqttClientConnectResultCode.Success)
        //    {
        //        // Subscribe to a topic
        //        await mqttClient.SubscribeAsync(topic);

        //        // Callback function when a message is received
        //        mqttClient.ApplicationMessageReceivedAsync += async e =>
        //        {
        //            // Hier kommt man nur rein von Messages von /Challenge
        //            var message = e.ApplicationMessage;
        //            if (message.Retain) // Ignore retained messages
        //            {
        //                return;
        //            }

        //            string payload = Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment);

        //            string robotName = payload;
        //            int gameId = currGameId;
        //            // besten Move berechnen für Roboter
                    
        //            Robot robot = await GetRobotByName(robotName);


        //            GameBoard gb = runningGames.Where(gb => gb.PlayerOneID.Equals(robot.MacAdress) || gb.PlayerTwoID.Equals(robot.MacAdress)).Single();



        //            //Move[,] moves = CreateMoveArrFromBoard(gb.Moves);
        //            //aiService.board = moves;
        //            //aiService.currentPlayer = robotName;

        //            //int columnNR = new Random().Next(1, 7);

        //            // Publish Message mit ColumnNr und Name des Roboters.

        //            string color;

        //            if (gb.PlayerOneID.Equals(robot.Id))
        //            {
        //                color = "red";
        //            } else
        //            {
        //                color = "yellow";
        //            }

        //            //GameBoard.Board boardTest = new GameBoard.Board(7, 6);
        //            //boardTest.DropCoin



        //            //SaveMoveToDB(robotName, columnNR, gameId);
        //            //AnimateMove(robotName, columnNR, gameId, color);

        //            await mqttClient.UnsubscribeAsync(topic);
        //            await mqttClient.DisconnectAsync();
        //        };

        //    }
        //    else
        //    {
        //        Console.WriteLine($"Failed to connect to MQTT broker: {connectResult.ResultCode}");
        //    }
        //}

        private async Task<Robot> GetRobotByName(string robotName)
        {

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseSqlServer(connectionstring);


            using (AppDbContext dbContext = new AppDbContext(optionsBuilder.Options))
            {
                try
                {
                    return await dbContext.Robots.Where(r => r.Name.Equals(robotName)).SingleAsync();
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }
            }

            return null;
        }

        private void AnimateMove(string robotName, int columnNR, int gameId, string color)
        {
            _hubContext.Clients.All.SendAsync("AnimateMove", robotName, columnNR, gameId, color);
        }

        private void SaveMoveToDB(string robotName, int columnNR, int gameId)
        {
            Debug.WriteLine("Move Saved To DB");
        }

        private Move[,] CreateMoveArrFromBoard(ICollection<Move> moves)
        {
            try
            {
                Move[,] movesArr = new Move[7, 6];

                Dictionary<string, int> colDepth = new Dictionary<string, int>();
                colDepth.Add("1", 6);
                colDepth.Add("2", 6);
                colDepth.Add("3", 6);
                colDepth.Add("4", 6);
                colDepth.Add("5", 6);
                colDepth.Add("6", 6);
                colDepth.Add("7", 6);

                foreach (Move move in moves)
                {
                    int depth;
                    colDepth.TryGetValue("" + move.Column, out depth);
                    movesArr[(move.Column - 1), depth - 1] = move;
                    colDepth["" + move.Column] = depth - 1;
                }

                return movesArr;
            }
            catch (IndexOutOfRangeException e)
            {
                Debug.WriteLine(e);
            }
            return null;
        }



        private class BoardParticipants
        {
            public Robot RobotOne { get; set; }
            public Robot RobotTwo { get; set; }

            public GameBoard Board { get; set; }
        }
    }
}