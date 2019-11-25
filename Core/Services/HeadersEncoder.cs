using System;
using System.Collections.Generic;
using System.Text;
using Core.Services.Interfaces;

namespace Core.Services
{
    public class HeadersEncoder : IHeadersEncoder
    {
        public IDictionary<string, string> Decode(byte[] headersData)
        {
            string headersString = Encoding.ASCII.GetString(headersData);
            string[] headersLines = headersString.Split("\n");

            IDictionary<string, string> headers = new Dictionary<string, string>();
            foreach(string headerLine in headersLines)
            {
                string[] header = headerLine.Split(':');
                if (header.Length != 2)
                    throw new ArgumentException("Invalid header format.");

                headers.Add(header[0], header[1]);
            }

            return headers;
        }

        public byte[] Encode(IDictionary<string, string> headers)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (KeyValuePair<string, string> header in headers)
                stringBuilder.AppendFormat("\"{2}\":\"{1}\"\n", header.Key, header.Value);

            stringBuilder.Remove(stringBuilder.Length, 1);
            string headersString = stringBuilder.ToString();
            return Encoding.ASCII.GetBytes(headersString);
        }
    }
}
