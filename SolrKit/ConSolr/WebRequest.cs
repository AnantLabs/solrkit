using System;
using System.IO;
using System.Net;
using System.Text;

namespace SolrKit
{
    /// <summary>
    /// Fetches a Web Page
    /// </summary>
    class WebFetcher
    {
        public static string GetSchema(string url)
        {
            // used to build entire input
            StringBuilder sb = new StringBuilder();

            byte[] buf = new byte[8192];
            if (!url.StartsWith("http://")) url = "http://" + url;
            HttpWebRequest request = (HttpWebRequest) WebRequest.Create(url);

            HttpWebResponse response = (HttpWebResponse) request.GetResponse();

            // read via the response stream
            Stream resStream = response.GetResponseStream();

            string tempString = null;
            int count = 0;

            do
            {
                // fill the buffer with data
                count = resStream.Read(buf, 0, buf.Length);

                // make sure we read some data
                if (count != 0)
                {
                    // translate from bytes to ASCII text
                    tempString = Encoding.ASCII.GetString(buf, 0, count);

                    // continue building the string
                    sb.Append(tempString);
                }
            }
            while (count > 0); // any more data to read?

            //return our schema contents
            return(sb.ToString());
        }
    }

}
