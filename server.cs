using System;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Net;
using System.Web;


namespace HttpServer
{
    class HttpServer
    {
        public static HttpListener listener;
        public static string url = "http://localhost:8000/";

        public static async Task Handle_inc_connections()
        {
            bool runServer = true;

            while (runServer)
            {

                HttpListenerContext ctx = await listener.GetContextAsync();      
                HttpListenerResponse response = ctx.Response;
                HttpListenerRequest request = ctx.Request;

                string requestedService = request.Url.AbsolutePath;
                Stream body = request.InputStream;
                Encoding encoding = request.ContentEncoding;
                StreamReader reader = new StreamReader(body, encoding);
                string Msg = reader.ReadToEnd();
               

                //imprime informações do cliente:

                Console.WriteLine(
                    "\n------------------------------------\n" +
                    "REQUEST URL: {0}\n" +
                    "REQUEST METHOD: {1}\n" +
                    "REQUEST SERVICE NAME: {2}\n" +
                    "REQUEST URI NAME: {3}\n" +
                    "REQUESTED SERVICE: {4}\n" +
                    "------------------------------------\n",
                    request.Url, request.HttpMethod, request.ServiceName, request.UrlReferrer, requestedService
                );

                //para o servidor caso o request tenha /shutdown e seja do tipo POST:

                if ((request.HttpMethod == "POST") && (requestedService == "/shutdown"))
                {
                    Console.WriteLine("Shutdown requested");
                    runServer = false;
                }

                if ((request.HttpMethod == "POST") && (requestedService == "/Echo"))
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(Msg);

                    response.ContentType = "Text";
                    response.ContentEncoding = Encoding.UTF8;
                    response.ContentLength64 = bytes.LongLength;

                    response.OutputStream.WriteAsync(bytes, 0, bytes.Length);     
                }

                //dar um jeito no CORS

                response.Close();
            }
        }

        static void Main(string[] args)
        {

            //cria um servidor e espera por conexões

            listener = new HttpListener();
            listener.Prefixes.Add(url);

            listener.Start();
            Console.WriteLine("Listening for connections on {0}\n\n", url);

            Task Listen = Handle_inc_connections();
            Listen.GetAwaiter().GetResult();

            listener.Close();

            Console.ReadLine();
        }
    }
}
