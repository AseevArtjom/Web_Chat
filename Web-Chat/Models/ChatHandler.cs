using Microsoft.AspNetCore.Http;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

namespace Web_Chat.Models
{
    public class ChatHandler
    {
        private static readonly List<WebSocket> Clients = new List<WebSocket>();

        private static readonly ReaderWriterLockSlim Locker = new ReaderWriterLockSlim();

        public static async Task WebSocketRequest(WebSocket socket)
        {
            Locker.EnterWriteLock();
            try
            {
                Clients.Add(socket);
            }
            finally
            {
                Locker.ExitWriteLock();
            }

            var buffer = new byte[1024 * 4];
            var segment = new ArraySegment<byte>(buffer);

            while (socket.State == WebSocketState.Open)
            {
                var result = await socket.ReceiveAsync(segment, CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Connection closed", CancellationToken.None);
                }
                else
                {
                    for (int i = 0; i < Clients.Count; i++)
                    {
                        WebSocket client = Clients[i];

                        try
                        {
                            if (client.State == WebSocketState.Open)
                            {
                                await client.SendAsync(new ArraySegment<byte>(buffer, 0, result.Count), WebSocketMessageType.Text, true, CancellationToken.None);
                            }
                        }
                        catch (WebSocketException)
                        {
                            Locker.EnterWriteLock();
                            try
                            {
                                Clients.Remove(client);
                                i--;
                            }
                            finally
                            {
                                Locker.ExitWriteLock();
                            }
                        }
                    }
                }
            }
            Locker.EnterWriteLock();
            try
            {
                Clients.Remove(socket);
            }
            finally
            {
                Locker.ExitWriteLock();
            }
        }
    }
}
