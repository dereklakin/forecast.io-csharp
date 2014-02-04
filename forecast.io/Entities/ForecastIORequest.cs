﻿using System;
using System.Globalization;
#if WINDOWS_PHONE
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
#else
using System.Web.Script.Serialization
#endif
using ForecastIO.Extensions;

namespace ForecastIO
{
    public class ForecastIORequest
    {
        private readonly string _apiKey;
        private readonly string _latitude;
        private readonly string _longitude;
        private readonly string _unit;
        private readonly string _exclude;
        private readonly string _extend;
        private readonly string _time;
        //
        private string _apiCallsMade;
        private string _apiResponseTime;
        //

        private const string CurrentForecastUrl = "https://api.forecast.io/forecast/{0}/{1},{2}?units={3}&extend={4}&exclude={5}";
        private const string PeriodForecastUrl = "https://api.forecast.io/forecast/{0}/{1},{2},{3}?units={4}&extend={5}&exclude={6}";

#if WINDOWS_PHONE
        public async Task<ForecastIOResponse> Get()
        {
            var url = (_time == null) ? String.Format(CurrentForecastUrl, _apiKey, _latitude, _longitude, _unit, _extend, _exclude) :
                String.Format(PeriodForecastUrl, _apiKey, _latitude, _longitude, _time, _unit, _extend, _exclude);

            var client = new CompressionEnabledWebClient { Encoding = Encoding.UTF8 };
            var tcs = new TaskCompletionSource<DownloadStringCompletedEventArgs>();
            DownloadStringCompletedEventHandler handler = (o, e) => tcs.TrySetResult(e);
            client.DownloadStringCompleted += handler;
            client.DownloadStringAsync(new Uri(url));
            var args = await tcs.Task;
            client.DownloadStringCompleted -= handler;
            string result = RequestHelpers.FormatResponse(args.Result);
            // Set response values.
            _apiResponseTime = client.ResponseHeaders["X-Response-Time"];
            _apiCallsMade = client.ResponseHeaders["X-Forecast-API-Calls"];

            var dataObject = JsonConvert.DeserializeObject<ForecastIOResponse>(result);

            return dataObject;
        }
#else
        public ForecastIOResponse Get()
        {
            var url = (_time == null) ? String.Format(CurrentForecastUrl, _apiKey, _latitude, _longitude, _unit, _extend, _exclude) :
                String.Format(PeriodForecastUrl, _apiKey, _latitude, _longitude, _time, _unit, _extend, _exclude);

            string result;
            using (var client = new CompressionEnabledWebClient())
            {
                client.Encoding = Encoding.UTF8;
                result = RequestHelpers.FormatResponse(client.DownloadString(url));
                // Set response values.
                _apiResponseTime = client.ResponseHeaders["X-Response-Time"];
                _apiCallsMade = client.ResponseHeaders["X-Forecast-API-Calls"];
            }

            var serializer = new JavaScriptSerializer();
            var dataObject = serializer.Deserialize<ForecastIOResponse>(result);

            return dataObject;
        }
#endif


        public ForecastIORequest(string apiKey, float latF, float longF, Unit unit, Extend[] extend = null, Exclude[] exclude = null)
        {
            _apiKey = apiKey;
            _latitude = latF.ToString(CultureInfo.InvariantCulture);
            _longitude = longF.ToString(CultureInfo.InvariantCulture);
            _unit = Enum.GetName(typeof(Unit), unit);
            _extend = (extend != null) ? RequestHelpers.FormatExtendString(extend) : "";
            _exclude = (exclude != null) ? RequestHelpers.FormatExcludeString(exclude) : "";
        }

        public ForecastIORequest(string apiKey, float latF, float longF, DateTime time, Unit unit, Extend[] extend = null, Exclude[] exclude = null)
        {
            _apiKey = apiKey;
            _latitude = latF.ToString(CultureInfo.InvariantCulture);
            _longitude = longF.ToString(CultureInfo.InvariantCulture);
            _time = time.ToUTCString();
            _unit = Enum.GetName(typeof(Unit), unit);
            _extend = (extend != null) ? RequestHelpers.FormatExtendString(extend) : "";
            _exclude = (exclude != null) ? RequestHelpers.FormatExcludeString(exclude) : "";
        }

        public string ApiCallsMade
        {
            get
            {
                if (_apiCallsMade != null)
                {
                    return _apiCallsMade;
                }
                throw new Exception("Cannot retrieve API Calls Made. No calls have been made to the API yet.");
            }
        }

        public string ApiResponseTime
        {
            get
            {
                if (_apiResponseTime != null)
                {
                    return _apiResponseTime;
                }
                throw new Exception("Cannot retrieve API Reponse Time. No calls have been made to the API yet.");
            }
        }
    }
}
