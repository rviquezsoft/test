using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WebSocketsServer.Models;

namespace WebSocketsServer.Controllers
{
    public class HomeController : Controller
    {

        private readonly WebSocketConnectionManager _webSocketConnectionManager;

     

        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger, WebSocketConnectionManager webSocketConnectionManager)
        {
            _webSocketConnectionManager = webSocketConnectionManager;

            _logger = logger;
        }

        public IActionResult Index()
        {
            var webSockets = _webSocketConnectionManager.GetAllSockets();

            foreach (var item in webSockets)
            {
                ViewBag.list = ViewBag.list + "key = " + item.Key + "state==  " + item.Value.State;
            }

            // Obtener el User-Agent del cliente
            string userAgent = HttpContext.Request.Headers.UserAgent;

            // Obtener la dirección IP del cliente
            string ipAddress = $"{HttpContext.Connection.RemoteIpAddress.MapToIPv4()}";

            string conectionId = $"{HttpContext.Connection.Id}";
            string port = $"{HttpContext.Connection.RemotePort}";

            // Otros datos posibles que podrías recopilar:
            // - Idioma del navegador: Request.UserLanguages;
            // - Tipo de navegador: Extraer del User-Agent
            // - Sistema operativo: Extraer del User-Agent

            // Puedes enviar estos datos a tu vista para mostrarlos o procesarlos según sea necesario
            ViewBag.UserAgent = userAgent;
            ViewBag.IPAddress = ipAddress;
            ViewBag.Port = port;
            ViewBag.CnId = conectionId;

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
