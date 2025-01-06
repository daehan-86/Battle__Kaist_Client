using System.Net.WebSockets;

public static class WebSocketService
{
    public static ClientWebSocket WebSocket { get; private set; }
    public static string naverId {  get; set; }
    static WebSocketService()
    {
        WebSocket = new ClientWebSocket();
        naverId = new string("");
    }
}