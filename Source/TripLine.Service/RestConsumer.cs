using System;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using TLine.Toolbox.Extensions;
using log4net;
using Newtonsoft.Json;

namespace TLine.DpSystem.Service.Client
{
    public class GoogleApiClient : RestConsumer 
    {
        static readonly string hostName = "todo";
        private static readonly int port = 999;
        static readonly string postfix = "url";

        string ex1 = @"https://maps.googleapis.com/maps/api/place/nearbysearch/json?location=-33.8670,151.1957&radius=500&types=food&name=cruise&key=AIzaSyDxUiJ2bhkHSY4SIbik5qcpMhoJba9gNAI";

        string ex2 = @"https://maps.googleapis.com/maps/api/place/nearbysearch/json?&key=AIzaSyDxUiJ2bhkHSY4SIbik5qcpMhoJba9gNAI&location=-33.8670,151.1957&radius=500";

        string ex3 = @"https://maps.googleapis.com/maps/api/place/nearbysearch/json?key=AIzaSyDxUiJ2bhkHSY4SIbik5qcpMhoJba9gNAI&location=-33.8670,151.1957&radius=100&type=locallity";
        private string nextPageTokenParameter = "next_page_token";


        public string next_page_token = "";

        public string types = "food;bank;";

        public GoogleApiClient() : base(hostName, port, postfix)
        {
            string key = "AIzaSyDxUiJ2bhkHSY4SIbik5qcpMhoJba9gNAI"; //now this give you your air distance.
        }


        public double GetAirDistance( long lat1, long long1,  long lat2, long long2)
        {
            //now this give you your air distance.
            var closest = Math.Sqrt( (lat2 - lat1) ^ 2 + (long2 - long1) ^ 2 ); 

        }
    }

    public class RestConsumer 
    {
        public const string UnableToCommunicateWithServer = "Unable to communicate with DP server";


        private int _port;

        /// <summary>
        /// The http client that will be used to connect to the REST interface.
        /// </summary>
        protected HttpClient _client;

        protected ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// The host name.
        /// </summary>
        private string _hostName;

        private readonly string _postfix;

        /// <summary>
        /// The base uri that is composed of the _hostname
        /// </summary>
        protected string _uri;


        #region Events - Public
        /// <summary>
        /// Event when there is a connection error
        /// </summary>
        //public virtual event EventHandler<string> ConnectionError;

        ///// <summary>
        ///// The connected event.
        ///// </summary>
        //public virtual event EventHandler<Version> Connected;

        ///// <summary>
        ///// This event is triggered every time the UI modifies the harware configuration.
        ///// </summary>
        //public event EventHandler<string> SetCommandsSent;

        /// <summary>
        /// Event on error receive
        /// </summary>
        public event EventHandler<Exception> ErrorReceived;

        //public event EventHandler<string> WarningReceived;

        #endregion

        #region Constructors

        protected RestConsumer(string hostName,   int port, string post)
            : this( hostName, port, post, null)
        {
        }

        protected RestConsumer(string hostName, int port, string post, HttpClient client)
        {
            _hostName = hostName;
            _port = port;
            _postfix = post;
            _client = client ?? new HttpClient();
        }

        #endregion Constructors

        #region Properties - Public
        public int Port
        {
            get { return _port; }
            set
            {
                _port = value;
                BuildUri();
            }
        }

        /// <summary>
        /// Gets or sets the host name.
        /// </summary>
        public string HostName
        {
            get
            {
                return _hostName;
            }

            set
            {
                _hostName = value;
                BuildUri();
            }
        }


        #endregion

        private void BuildUri()
        {
            _uri = $"http://{_hostName}:{_port}/{_postfix}/";
        }


        private async Task<T> ExecuteGetRequest<T>(string ressource, CancellationToken? cancellationToken = null, bool exceptionOnError = true)
        {
            HttpResponseMessage response = await ExecuteGetRequest(ressource, cancellationToken, exceptionOnError);

            try
            {
                if (response.IsSuccessStatusCode)
                {

                    T dto = JsonConvert.DeserializeObject<T>(await response.Content.ReadAsStringAsync());
                    return dto;
                }
                return default(T);
            }
            catch (Exception ex)
            {
                _log.Error("Exception while parsing server response " + typeof(T), ex);
                OnErrorReceived("Invalid response from server" + "(" + ressource + ")");
                throw;
            }
        }


        private async Task<HttpResponseMessage> ExecuteGetRequest(string ressource, CancellationToken? cancellationToken = null, bool exceptionOnError = true)
        {
            HttpResponseMessage response;

            try
            {
                response = cancellationToken.HasValue
                    ? await _client.GetAsync(_uri + ressource, cancellationToken.Value)
                    : await _client.GetAsync(_uri + ressource);
            }
            catch (Exception)
            {
                OnErrorReceived(UnableToCommunicateWithServer);
                throw;
            }

            if (!response.IsSuccessStatusCode)
            {
                OnErrorReceived($"Request failed  {response.StatusCode}");
            }
            else
            {
                LastErrorMessage = string.Empty;
            }


            return response;
        }


        //private static bool _lastCalledCompletedOK

        private async Task<T> ExecutePutRequest<T>(string ressource, HttpContent content, CancellationToken? cancelToken = null, bool exceptionOnError = false)
        {
            HttpResponseMessage response = await ExecutePutRequest(ressource, content, cancelToken, exceptionOnError);

            try
            {
                T dto = JsonConvert.DeserializeObject<T>(await response.Content.ReadAsStringAsync());

                return dto;
            }
            catch (Exception)
            {
                OnErrorReceived(UnableToCommunicateWithServer);
                throw;
            }
        }

        private async Task<HttpResponseMessage> ExecutePutRequest(string ressource, HttpContent content, CancellationToken? cancelToken = null, bool exceptionOnError = false)
        {
            HttpResponseMessage response;

            try
            {
                response = (cancelToken.HasValue)
                    ? await _client.PutAsync(_uri + ressource, content, cancelToken.Value)
                    : await _client.PutAsync(_uri + ressource, content);

            }
            catch  
            {
                //_log.Error("Exception while issuing  client request", ex);
                OnErrorReceived(UnableToCommunicateWithServer);
                throw;
            }

            if (!response.IsSuccessStatusCode)
            {
                OnErrorReceived($"Request failed  {response.StatusCode}");
            }
   
            return response;
        }

        public static string LastErrorMessage { get; set; } = string.Empty;

        /// <summary>
        /// The on error received.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        protected virtual void OnErrorReceived(string message)
        {
            // Needs to run on the main thread. 
            try
            {
                if (message.Equals(LastErrorMessage, StringComparison.InvariantCultureIgnoreCase))
                    return;

                LastErrorMessage = message;

                Dispatcher.CurrentDispatcher.Invoke(() =>
                {
                    EventHandler<Exception> handler = ErrorReceived;
                    if (handler != null)
                    {
                        _log.Error(message);
                        handler(this, new Exception(message));
                    }
                });

            }
            catch (Exception e)
            {
                _log.Error(e);
            }
        }

    }
}