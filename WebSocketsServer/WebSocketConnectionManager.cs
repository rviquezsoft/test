using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;

namespace WebSocketsServer
{
    public class WebSocketConnectionManager
    {
        private readonly ConcurrentDictionary<string, WebSocket> _sockets = new ConcurrentDictionary<string, WebSocket>();


        public WebSocketConnectionManager()
        {
            _sockets = new ConcurrentDictionary<string, WebSocket>();
        }


        public void AddSocket(string id, WebSocket socket)
        {
            _sockets.TryAdd(id, socket);
        }

        public void RemoveSocket(string id)
        {
            _sockets.TryRemove(id, out _);
        }

        public WebSocket GetSocketById(string id)
        {
            _sockets.TryGetValue(id, out WebSocket socket);
            return socket;
        }

        public ConcurrentDictionary<string, WebSocket> GetAllSockets()
        {
            return _sockets;
        }

        public async Task SendMessageToClient(WebSocket socket, string message)
        {
            if (socket != null && socket.State == WebSocketState.Open)
            {
                var buffer = Encoding.UTF8.GetBytes(message);
                await socket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
            }
            else
            {
                throw new InvalidOperationException("NO HAY CLIENTES DISPOIBLES O EL CLIENTE NO ESTA DISPONIBLE");
            }
        }


        public async Task Echo(HttpContext context, WebSocket webSocket, WebSocketConnectionManager manager)
        {
            var buffer = new byte[1024 * 4];
            WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            while (!result.CloseStatus.HasValue)
            {
                foreach (var socket in manager.GetAllSockets())
                {
                    WebSocket socketdic = socket.Value;

                    if (socketdic.State == WebSocketState.Open)
                    {
                        await socketdic.SendAsync(new ArraySegment<byte>(buffer, 0, result.Count), result.MessageType, result.EndOfMessage, CancellationToken.None);
                    }
                }

                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }

            string socketId = manager.GetAllSockets().FirstOrDefault(s => s.Value == webSocket).ToString();
            manager.RemoveSocket(socketId);
            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        }
    }

}
