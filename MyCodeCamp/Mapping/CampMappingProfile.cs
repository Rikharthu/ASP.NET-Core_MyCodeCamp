using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using MyCodeCamp.Data.Entities;
using MyCodeCamp.Models;

namespace MyCodeCamp.Mapping
{
    // AutoMapper profile that is used to configure object mapping
    public class CampMappingProfile : Profile
    {
        // Do not use request-specific DI here, since it is not guaranteed which requests will instantiate the profile

        public CampMappingProfile()
        {
            // Create maps between classes (From Camp to CampModel)
            // This generic call will try to map fields automatically (most of the time it works)
            CreateMap<Camp, CampModel>()
                // configure mapping for specific fields
                .ForMember(
                    campModel => campModel.StartDate, // destination
                    opt => opt.MapFrom(camp => camp.EventDate)) // source
                .ForMember(campModel => campModel.EndDate,
                    opt => opt.ResolveUsing(
                        camp => camp.EventDate.AddDays(camp.Length - 1))) // calculate this property
                .ForMember(campModel => campModel.Url,
                    //opt => opt.MapFrom(camp => $"/api/camps/{camp.Moniker}")); // works, but not a good approach to hardcode urls
                    //MapCampModelUrl // our mapping method (just a shortcut) 
                    opt => opt.ResolveUsing<CampUrlResolver>() // pass a custom resolver. Enavles DI
                )
                .ReverseMap() // allow converting reverse: from CampModel to Camp
                // Configure reverse mapping
                .ForMember(camp => camp.EventDate,
                    opt => opt.MapFrom(campModel => campModel.StartDate))
                .ForMember(camp => camp.Length,
                    opt => opt.ResolveUsing(campModel => (campModel.EndDate - campModel.StartDate).Days + 1))
                // Not necessary, it seems to work by default TODO WHY it works by default?
                .ForMember(camp => camp.Location,
                    opt => opt.ResolveUsing(campModel => new Location()
                    {
                        Address1 = campModel.LocationAddress1,
                        Address2 = campModel.LocationAddress2,
                        Address3 = campModel.LocationAddress3,
                        CityTown = campModel.LocationCityTown,
                        StateProvince = campModel.LocationStateProvince,
                        PostalCode = campModel.LocationPostalCode,
                        Country = campModel.LocationCountry
                    }));

            // TODO how to use string names?

            CreateMap<Speaker, SpeakerModel>()
                .ForMember(speaker=>speaker.Url,opt=>opt.ResolveUsing<SpeakerUrlResolver>())
                .ReverseMap();
        }

        /// <summary>
        /// Generates a URL to the given Camp resource by using passed "UrlHelper" from controll methods and 
        /// controller action names such as "CampGet"
        /// </summary>
        /// <param name="opt"></param>
        private static void MapCampModelUrl(IMemberConfigurationExpression<Camp, CampModel, string> opt)
        {
            opt.ResolveUsing((camp, model, unused, context) =>
            {
                // P.S. opt.Items["UrlHelper"] = this.Url in CampsController.GetCampById()
                var url = (IUrlHelper) context.Items["UrlHelper"];
                // generate a URL to the resource
                return url.Link("CampGet", new {id = camp.Id});


                //var url = (IUrlHelper)context.HttpContext.Items["UrlHelper"];
                //// generate a URL to the resource
                //return url.Link("CampGet", new { id = camp.Id });
            });
        }
    }
}