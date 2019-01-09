using AttackPrevent.Model.Cloudflare;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttackPrevent.Business
{
    public class MapperInitialize
    {
        public static void Initialize()
        {
            Mapper.Initialize(x =>
            {
                x.CreateMap<ETWPrase, CloudflareLog>()
                .ForMember(d => d.ClientRequestHost, opt =>
                {
                    opt.MapFrom(s => s.Cs_host);
                })
                .ForMember(d => d.ClientIP, opt =>
                {
                    opt.MapFrom(s => !string.IsNullOrEmpty(s.CFConnectingIP) ? s.CFConnectingIP : s.C_ip);
                })
                .ForMember(d => d.ClientRequestURI, opt =>
                {
                    opt.MapFrom(s => string.Format("{0}/{1}", s.Cs_uri_stem, s.cs_uri_query));
                })
                .ForMember(d => d.ClientRequestMethod, opt =>
                {
                    opt.MapFrom(s => s.Cs_method);
                });                
            });
        }
    }
}
