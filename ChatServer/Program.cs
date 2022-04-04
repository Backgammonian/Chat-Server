﻿using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Shared;
using ChatMessages;

namespace ChatServer
{
    class Program
    {
        private static Server _server;
        private static ClientProfiles _clients;
        private static Rooms _rooms;

        private static async Task Main(string[] args)
        {
            _server = new Server(55000);
            _server.ClientConnected += OnClientConnected;

            _clients = new ClientProfiles();

            _rooms = new Rooms();

            var room1 = new Room("Room #1");
            room1.AddNewMessage(new Message("Debug message 1 in room #1", "Server", "--", DateTime.Now));
            room1.AddNewMessage(new Message("Debug message 2 in room #1", "Server", "--", DateTime.Now));
            room1.AddNewMessage(new Message("Debug message 3 in room #1", "Server", "--", DateTime.Now));
            _rooms.Add(room1);

            var room2 = new Room("Room #2");
            room2.AddNewMessage(new Message("Important message 1 in room #2", "Server", "--", DateTime.Now));
            room2.AddNewMessage(new Message("Important message 2 in room #2", "Server", "--", DateTime.Now));
            room2.AddNewMessage(new Message("Important message 3 in room #2", "Server", "--", DateTime.Now));
            _rooms.Add(room2);

            var restRoom = new Room("Rest room");
            restRoom.AddNewMessage(new Message("Hello message 1 in rest room", "Server", "--", DateTime.Now));
            restRoom.AddNewMessage(new Message("Hello message 2 in rest room", "Server", "--", DateTime.Now));
            restRoom.AddNewMessage(new Message("Hello message 3 in rest room", "Server", "--", DateTime.Now));
            _rooms.Add(restRoom);

            Console.WriteLine("Server started!");

            await _server.StartListen();
        }

        private static async void OnClientConnected(object sender, ClientConnectedEventArgs e)
        {
            var client = new Client(e.Client);
            var clientProfile = new ClientProfile(client);
            if (_clients.Add(clientProfile))
            {
                clientProfile.DataReceived += OnDataReceivedFromClient;
            }

            Console.WriteLine("Client " + client.ClientAddress + " connected!");

            var listOfRooms = new ListOfRoomsPackage(_rooms.GetListOfRooms());

            await clientProfile.Send(listOfRooms.GetByteArray());
        }

        private async static void OnDataReceivedFromClient(object sender, NetworkDataReceivedEventArgs e)
        {
            if (e.Data.Length == 0)
            {
                return;
            }

            var client = sender as ClientProfile;

            var dataReader = new SimpleReader(e.Data);
            var type = (PackageTypes)dataReader.GetByte();
            var json = dataReader.GetString();

            switch (type)
            {
                case PackageTypes.ClientsNickname:
                    Console.WriteLine("ClientsNickname");

                    var clientInfo = JsonConvert.DeserializeObject<(string, string)>(json);
                    if (client.ID == clientInfo.Item1)
                    {
                        Console.WriteLine("Nickname of client " + client.ID + "changed from " + client.Nickname + " to " + clientInfo.Item2);

                        client.SetNickname(clientInfo.Item2);
                    }
                    break;

                case PackageTypes.RequestAllMessagesInRoom:
                    Console.WriteLine("RequestAllMessagesInRoom");

                    var requestedRoomID = JsonConvert.DeserializeObject<string>(json);
                    if (_rooms.Has(requestedRoomID))
                    {
                        Console.WriteLine("Sending all messages from room " + requestedRoomID + " to client " + client.Nickname + " (" + client.ID + ")");

                        var response = new ResponseAllMessagesInRoomPackage(requestedRoomID, _rooms[requestedRoomID].GetMessages());

                        await client.Send(response.GetByteArray());
                    }
                    break;

                case PackageTypes.MessageToRoom:
                    Console.WriteLine("MessageToRoom");
                    break;

                default:
                    Console.WriteLine("Unknown type: " + type);
                    break;
            }
        }
    }
}