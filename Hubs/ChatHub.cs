﻿using Microsoft.AspNetCore.SignalR;

namespace VierGewinnt.Hubs
{
    public class ChatHub : Hub
    {
        static readonly IList<string> players = new List<string>();
        static readonly IDictionary<string, string> onlineUsers = new Dictionary<string, string>();

        //public async Task SendNotification(string player, string message)

        // Unser Problem ist, dass wenn jemand schon drinn ist, er dann nicht sieht

        public async Task SendNotification(string player)
        {

            // Entweder wir laden hier die ApplicationUser die i´n der chatLobby drin sind.
            // Aber dann müssen wir eigentlich für jeden ApplicationUser der sich mit dem Hub connected in einer Tabelle in der DB speichern.
            await Clients.Others.SendAsync("ReceiveNewUser", player);

            ////await Clients.All.SendAsync("ReceiveMessage", player, message);
        }

        public async Task AddUser(string player)
        {
            if (players.Contains(player))
            {
                return;
            }
            else
            {
                onlineUsers.Add(player, Context.ConnectionId);
                players.Add(player);
            }
            return;
        }

        public async Task GetAvailableUsers()
        {
            await Clients.Caller.SendAsync("ReceiveAvailableUsers", players);
        }

        public async Task LeaveLobby(string userName)
        {
            if (players.Contains(userName))
            {
                players.Remove(userName);
                onlineUsers.Remove(userName);
                await Clients.Others.SendAsync("PlayerLeft", userName);
                return;
            }
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var userName = Context.User?.Identity?.Name;
            if (!string.IsNullOrEmpty(userName) && players.Contains(userName))
            {
                players.Remove(userName);
                onlineUsers.Remove(userName);
                await Clients.Others.SendAsync("PlayerLeft", userName);
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task NotificateGameStart(string playerOne, string playerTwo, string gameUrl)
        {
            string conIdP1;
            onlineUsers.TryGetValue(playerOne, out conIdP1);
            string conIdP2;
            onlineUsers.TryGetValue(playerTwo, out conIdP2);

            IList<string> conIds = new List<string>();
            conIds.Add(conIdP1);
            conIds.Add(conIdP2);

            await Clients.All.SendAsync("NavigateToGame", playerOne, playerTwo);
            //await Clients.ApplicationUser(conIdP2).SendAsync("NavigateToGame", playerOne, playerTwo);
        }
    }
}
