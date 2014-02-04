﻿using System;
using System.Net;

namespace ForecastIO
{
    public class CompressionEnabledWebClient : WebClient
    {
        protected override WebRequest GetWebRequest(Uri address)
        {
            var request = base.GetWebRequest(address) as HttpWebRequest;
            if (request == null) return null;
#if !WINDOWS_PHONE
            request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
#endif
            return request;
        }

    }
}
