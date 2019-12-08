using System.Collections.Generic;

namespace Core.Services.Encoders.Interfaces
{
    public interface IHeadersEncoder
    {
        IDictionary<string, string> Decode(byte[] headersData);

        byte[] Encode(IDictionary<string, string> headers);
    }
}
