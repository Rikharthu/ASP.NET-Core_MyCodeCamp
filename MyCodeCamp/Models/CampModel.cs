using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MyCodeCamp.Models
{
    /// <summary>
    /// Model for class Camp, which encapsulates only required fields (no EF navigation properties)
    /// and provides simpler access to the camp's location. 
    /// Model mapping is used for conversions 
    /// (we are using AutoMapper extension "AutoMapper.Extensions.Microsoft.DependencyIn" NuGet dependency)
    /// </summary>
    public class CampModel
    {
        //public int Id { get; set; }
        public string Url { get; set; } // needs mapping too
        // Same field names are mapped by default
        [Required]
        [MinLength(3)]
        [MaxLength(20)]
        public string Moniker { get; set; }
        [Required]
        [MinLength(5)]
        [MaxLength(100)]
        public string Name { get; set; }
        // these fields differ from original Camp class
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Length { get; set; }
        [Required]
        [MinLength(25)]
        [MaxLength(4096)]
        public string Description { get; set; }

        // Instead of using Location object as in Entities folder, use it's fields directly
        // To support AutoMapping, we added Location<FIELD_NAME> prefix, so that automapper by convention
        // knows that we derive these fields from a "Location" class
        // (it expects prefix of a class name that we get these fields from)
        // TODO is it possible to ammend this prefix and configure manually? (yes, see how we map the reverse location)
        public string LocationAddress1 { get; set; }
        public string LocationAddress2 { get; set; }
        public string LocationAddress3 { get; set; }
        public string LocationCityTown { get; set; }
        public string LocationStateProvince { get; set; }
        public string LocationPostalCode { get; set; }
        public string LocationCountry { get; set; }
    }
}
