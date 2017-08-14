using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyCodeCamp.Controllers;
using MyCodeCamp.Data.Entities;
using MyCodeCamp.Models;

namespace MyCodeCamp.Mapping
{
    public class SpeakerUrlResolver : IValueResolver<Speaker, SpeakerModel, string>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SpeakerUrlResolver(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string Resolve(Speaker source, SpeakerModel destination, string destMember, ResolutionContext context)
        {
            // use current request's HtppContext to get a URL helper
            // In BaseController "context.HttpContext.Items[URL_HELPER] = this.Url;"
            var url = (IUrlHelper) _httpContextAccessor.HttpContext.Items[BaseController.URL_HELPER];
            // Use UrlHelper to generate a url for a given action
            return url.Link("SpeakerGet", new {moniker = source.Camp.Moniker, id = source.Id});
        }
    }
}