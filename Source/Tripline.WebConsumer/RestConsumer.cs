using System;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using log4net;
using System.Diagnostics;

namespace Tripline.WebConsumer
{
    
    public class RestConsumer 
    {
        public const string UnableToCommunicateWithServer = "Unable to communicate with REST service";

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

        protected RestConsumer(string hostName, string post)
            : this( hostName, post, new HttpClient())
        {

        }

        protected RestConsumer(string hostName, string post, HttpClient client)
        {
            _hostName = hostName;
            _postfix = post;
            _client = client ?? new HttpClient();
            BuildUri();
        }

        #endregion Constructors

        #region Properties - Public
     
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
            _uri = $"{_hostName}/{_postfix}/".ToLower();
        }

        public T ExecuteGetRequest<T>(string ressource, bool exceptionOnError = true)
        {
            HttpResponseMessage response;

            try
            {
                Debug.WriteLine($"{_uri}" + $"{ressource}");


                response = _client.GetAsync(_uri + ressource).Result;

                if (!response.IsSuccessStatusCode)
                {
                    Debug.WriteLine($"Request failed");

                    OnErrorReceived($"Request failed  {response.StatusCode}");
                    var newT = Activator.CreateInstance<T>();
                    return newT;
                }

                var str = response.Content.ReadAsStringAsync().Result;
                T dto = JsonConvert.DeserializeObject<T>(str);
                return dto;
            }
            catch (Exception ex )
            {
                Debug.WriteLine($"Exception  {ex.Message}");
                OnErrorReceived(UnableToCommunicateWithServer);
                throw;
            }
        }



        //public async Task<T> AsyncExecuteGetRequest<T>(string ressource, CancellationToken? cancellationToken = null, bool exceptionOnError = true)
        //{

        //    try
        //    {
        //        HttpResponseMessage response = await ExecuteGetRequest(ressource, cancellationToken, exceptionOnError).Result;

        //        if (response.IsSuccessStatusCode)
        //        {

        //            T dto = JsonConvert.DeserializeObject<T>(await response.Content.ReadAsStringAsync());
        //            return dto;
        //        }
        //        return default(T);
        //    }
        //    catch (Exception ex)
        //    {
        //        _log.Error("Exception while parsing server response " + typeof(T), ex);
        //        OnErrorReceived("Invalid response from server" + "(" + ressource + ")");
        //        throw;
        //    }
        //}


        //protected async Task<HttpResponseMessage> ExecuteGetRequest(string ressource, 
        //    CancellationToken? cancellationToken = null, bool exceptionOnError = true)
        //{
        //    HttpResponseMessage response;

        ////    ressource = ressource.Replace(@"//", @"/");

        //    //ressource = ressource.ToLower();
        //    try
        //    {
        //        response = cancellationToken.HasValue
        //            ? await _client.GetAsync(_uri + ressource, cancellationToken.Value)
        //            : await _client.GetAsync(_uri + ressource);
        //    }
        //    catch (Exception)
        //    {
        //        OnErrorReceived(UnableToCommunicateWithServer);
        //        throw;
        //    }

        //    if (!response.IsSuccessStatusCode)
        //    {
        //        OnErrorReceived($"Request failed  {response.StatusCode}");
        //    }
        //    else
        //    {
        //        LastErrorMessage = string.Empty;
        //    }


        //    return response;
        //}


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

                // todo:
                //Dispatcher.CurrentDispatcher.Invoke(() =>
                //{
                //    EventHandler<Exception> handler = ErrorReceived;
                //    if (handler != null)
                //    {
                //        _log.Error(message);
                //        handler(this, new Exception(message));
                //    }
                //});

            }
            catch (Exception e)
            {
                _log.Error(e);
            }
        }

    }
}