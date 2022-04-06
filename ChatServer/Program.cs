using System;
using System.Threading.Tasks;
using System.Net.NetworkInformation;
using System.Linq;
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
            /*var listeners = IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpListeners();
            foreach (var listener in listeners)
            {
                Console.WriteLine(listener.ToString());
            }*/

            ushort serverPort = 55000;
            var isPortChosen = false;
            while (!isPortChosen)
            {
                Console.WriteLine("Enter server port:");
                var line = Console.ReadLine();

                if (line.Length == 0)
                {
                    //default port
                    isPortChosen = true;
                }
                else
                if (ushort.TryParse(line, out ushort selectedPort))
                {
                    if (IsPortOccupied(selectedPort))
                    {
                        Console.WriteLine("Port " + selectedPort + " is already occupied, thy another");
                    }
                    else
                    {
                        serverPort = selectedPort;
                        isPortChosen = true;
                    }
                }
                else
                {
                    Console.WriteLine("Invalid port, try again");
                }
            }

            _server = new Server(serverPort);
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

            Console.WriteLine("Server started on port " + serverPort);

            await _server.StartListen();
        }

        private static bool IsPortOccupied(int port)
        {
            return IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpListeners().Any(p => p.Port == port);
        }

        private static async void OnClientConnected(object sender, ClientConnectedEventArgs e)
        {
            var client = new Client(e.Client);
            var clientProfile = new ClientProfile(client);
            if (_clients.Add(clientProfile))
            {
                clientProfile.DataReceived += OnDataReceivedFromClient;
                clientProfile.NewMessageInRoomReceived += OnNewMessageInRoomReceived;
                clientProfile.ClientLoaded += OnClientLoaded;
                clientProfile.ErrorOccured += OnClientErrorOccured;
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

            if (!dataReader.TryGetByte(out byte type) ||
                !dataReader.TryGetString(out string json))
            {
                return;
            }

            switch ((PackageTypes)type)
            {
                case PackageTypes.ClientHello:
                    Console.WriteLine("ClientHello");

                    var clientInfo = JsonConvert.DeserializeObject<(string, string)>(json);
                    client.SetInfo(clientInfo.Item1, clientInfo.Item2);

                    Console.WriteLine("Client " + client.ID + " got new infos: " + clientInfo.Item2 + " (" + clientInfo.Item1 + ")");
                    break;

                case PackageTypes.ClientsNewNickname:
                    Console.WriteLine("ClientsNewNickname");

                    var clientsNewNickname = JsonConvert.DeserializeObject<(string, string)>(json);
                    if (client.LocalID == clientsNewNickname.Item1)
                    {
                        Console.WriteLine("Nickname of client " + client.LocalID + " changed from " + client.Nickname + " to " + clientsNewNickname.Item2);

                        client.SetNickname(clientsNewNickname.Item2);
                        await NotifyClients();
                    }
                    break;

                case PackageTypes.RequestAllMessagesInRoom:
                    Console.WriteLine("RequestAllMessagesInRoom");

                    var requestedRoomID = JsonConvert.DeserializeObject<string>(json);
                    if (_rooms.Has(requestedRoomID))
                    {
                        Console.WriteLine("Sending all messages from room " + requestedRoomID + " to client " + client.Nickname + " (" + client.ID + ")");

                        client.SetRoom(_rooms[requestedRoomID]);
                        var response = new ResponseAllMessagesInRoomPackage(requestedRoomID, _rooms[requestedRoomID].GetMessages());
                        await client.Send(response.GetByteArray());
                    }
                    break;

                case PackageTypes.MessageToRoom:
                    Console.WriteLine("MessageToRoom");

                    var messageToRoom = JsonConvert.DeserializeObject<(Message, string)>(json);
                    if (messageToRoom.Item1.AuthorsID == client.LocalID &&
                        _rooms.Has(messageToRoom.Item2))
                    {
                        Console.WriteLine("Message to room " + messageToRoom.Item2 + " from client " + client.Nickname + " (" + client.ID + ")");

                        _rooms[messageToRoom.Item2].AddNewMessage(messageToRoom.Item1);
                    }
                    break;

                case PackageTypes.ClientDisconnect:
                    Console.WriteLine("ClientDisconnect");
                    Console.WriteLine("Sending info about existing clients...");

                    _clients.Remove(client);
                    await NotifyClients();
                    break;

                default:
                    Console.WriteLine("Unknown type: " + type);
                    break;
            }
        }

        private async static void OnNewMessageInRoomReceived(object sender, NewMessageInRoomEventArgs e)
        {
            Console.WriteLine("New message in room " + e.RoomID);

            var client = sender as ClientProfile;
            var message = new NewMessageInRoomPackage(e.Message, e.RoomID);

            await client.Send(message.GetByteArray());
        }

        private async static Task NotifyClients()
        {
            Console.WriteLine("(NotifyClients)");

            var listOfClients = new ClientsListUpdatedPackage(_clients.GetListOfClients());
            var data = listOfClients.GetByteArray();

            foreach (var clientProfile in _clients)
            {
                await clientProfile.Send(data);
            }
        }

        private async static void OnClientLoaded(object sender, EventArgs e)
        {
            await NotifyClients();
        }

        private async static void OnClientErrorOccured(object sender, EventArgs e)
        {
            var client = sender as ClientProfile;
            _clients.Remove(client);
            await NotifyClients();
        }
    }
}
