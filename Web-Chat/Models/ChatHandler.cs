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
        // Список всех клиентов
        private static readonly List<WebSocket> Clients = new List<WebSocket>();

        // Блокировка для обеспечения потокобезопасности
        private static readonly ReaderWriterLockSlim Locker = new ReaderWriterLockSlim();

        public static async Task WebSocketRequest(WebSocket socket)
        {
            // Добавляем сокет клиента в список
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
                // Ожидаем данных от клиента
                var result = await socket.ReceiveAsync(segment, CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Connection closed", CancellationToken.None);
                }
                else
                {
                    // Передаем сообщение всем клиентам
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
                            // Удаляем сокет, если с ним возникла проблема
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

            // Удаляем сокет клиента из списка при завершении
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
