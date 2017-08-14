using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyCodeCamp.Controllers;
using MyCodeCamp.Data.Entities;

namespace MyCodeCamp.Models
{
    public class CampUrlResolver : IValueResolver<Camp, CampModel, string> // source, destionation, type
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CampUrlResolver(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string Resolve(Camp source, CampModel destination, string destMember, ResolutionContext context)
        {
            // use current request's HtppContext to get a URL helper
            // In BaseController "context.HttpContext.Items[URL_HELPER] = this.Url;"
            var url = (IUrlHelper) _httpContextAccessor.HttpContext.Items[BaseController.URL_HELPER];
            // Use UrlHelper to generate a url for a given action
            return url.Link("CampGet", new {moniker = source.Moniker });
        }
    }
}