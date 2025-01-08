using System.Net.WebSockets;

public static class WebSocketService
{
    public static ClientWebSocket WebSocket { get; set; }
    public static string naverId { get; set; }
    public static int roomId { get; set; }
    static WebSocketService()
    {
        WebSocket = new ClientWebSocket();
        naverId = new string("");
        roomId = new int();
    }
}