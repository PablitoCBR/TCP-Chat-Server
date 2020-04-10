using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Services.Encoders
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
                if (string.IsNullOrWhiteSpace(headerLine))
                    continue;

                string[] header = headerLine.Split(':');
                if (header.Length != 2)
                    throw new ArgumentException("Invalid header format.");

                headers.Add(header[0], header[1]);
            }

            return headers;
        }

        public byte[] Encode(IDictionary<string, string> headers)
        {
            if (headers == null || headers.Count == 0)
                return Array.Empty<byte>();

            StringBuilder stringBuilder = new StringBuilder();
            foreach (KeyValuePair<string, string> header in headers)
                stringBuilder.AppendFormat("{0}:{1}\n", header.Key, header.Value);

            string headersString = stringBuilder.ToString();
            return Encoding.ASCII.GetBytes(headersString);
        }
    }

    public interface IHeadersEncoder
    {
        IDictionary<string, string> Decode(byte[] headersData);

        byte[] Encode(IDictionary<string, string> headers);
    }
}
