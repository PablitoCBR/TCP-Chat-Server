using AutoMapper;
using Host.Builder.Models;
using Host.Listeners;

namespace Host.Helpers
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<HostBuilderSettings, ListennerSettings>();
        }
    }
}
