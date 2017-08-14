using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Castle.Core.Logging;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using MyCodeCamp.Data;
using MyCodeCamp.Data.Entities;
using MyCodeCamp.Models;

namespace MyCodeCamp.Controllers
{
    [Route("api/[controller]")]
    public class CampsController : BaseController
    {
        private readonly ICampRepository _repo;
        private readonly ILogger<CampsController> _logger;
        private IMapper _mapper;

        public CampsController(ICampRepository repo, ILogger<CampsController> logger, IMapper mapper)
        {
            _repo = repo;
            _logger = logger;
            _mapper = mapper;
        }

        // Gets all camps
        // GET: api/camps
        [HttpGet]
        public IActionResult Get()
        {
            // wrap our result into Ok (200) response
            //            return Ok(new { Name = "Shawn", FavoriteColor = "Blue" });
            //return Ok(_repo.GetAllCamps());
            return Ok(_mapper.Map<IEnumerable<CampModel>>(_repo.GetAllCamps()));
        }

        /*
        // GET: api/camps/4
        [HttpGet("{id}", Name = "CampGet")] // api/camps/{id}
        public IActionResult GetCampById(int id, bool includeSpeakers = false)
        {
            // since includeSpeakers is not part of the route, then it will be used as query paremeter
            // such as api/camps/4?includeSpeakers=true

            try
            {
                Camp camp = null;

                if (includeSpeakers)
                {
                    camp = _repo.GetCampWithSpeakers(id);
                }
                else
                {
                    camp = _repo.GetCamp(id);
                }

                if (camp == null)
                {
                    // camp with such id doesn't exist
                    //return NotFound($"Camp #{id} was not found :("); // 404 NOT FOUND
                    return NotFound(new
                    {
                        errorCode = "UV_404",
                        message = $"Camp #{id} was not found"
                    });
                }

                //var url = this.Url.Link("CampGet", new {id = camp.Id});

                //return Ok(camp); // 200 OK
                // Use AutoMapper to map Camp to CampModel
                // We configure mapping in TODO add location to mapping instructions

                //return Ok(_mapper.Map<CampModel>(camp,
                //    opt => opt.Items["UrlHelper"] = this.Url)); // pass additional data to mapper
                return Ok(_mapper.Map<CampModel>(camp));
            }
            catch
            {
                // TODO log an error

                return BadRequest(); // something bad happened
            }
        }
        */

        // Instead use "Moniker" property as a surrogate key, to avoid passing users interals of our database such as entity id's
        // P.S. it should also be a unique identifier that doesnt leak information
        // P.P.S. nevertheless, using ID PK is also OK
        [HttpGet("{moniker}", Name = "CampGet")] // api/camps/{id}
        public IActionResult GetCampByMoniker(string moniker, bool includeSpeakers = false)
        {
            try
            {
                Camp camp = null;

                if (includeSpeakers)
                {
                    camp = _repo.GetCampByMonikerWithSpeakers(moniker);
                }
                else
                {
                    camp = _repo.GetCampByMoniker(moniker);
                }

                if (camp == null)
                {
                    return NotFound(new
                    {
                        errorCode = "UV_404",
                        message = $"Camp #{moniker} was not found"
                    });
                }

                return Ok(_mapper.Map<CampModel>(camp));
            }
            catch
            {
                return BadRequest();
            }
        }

        // POST: api/camps
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CampModel model)
        {
            // Model Binder gets the data from the uri query parameters/request body form-data/json 
            // and tries to map it to our specified Camp model
            // [FromBody] indicates that Model Binder should use request body (json or etc)

            try
            {
                // Validate model
                // TODO is it possible to check for validity only those fields that were passed
                // TODO for instance I do not want to update description, but if I dont pass it, validation will fail
                if (!ModelState.IsValid) // check upon validation attributes such as [Required]
                {
                    _logger.LogInformation($"Received invalid model {model}");
                    // return the message with problems about the model
                    return BadRequest(ModelState);
                }

                _logger.LogInformation("Creating a new Code Camp");

                // Do not forget to map CampModel back to Camp in order to persist in a database
                // mapping in an opposite direction
                var camp = _mapper.Map<Camp>(model);

                _repo.Add(camp);
                if (await _repo.SaveAllAsync())
                {
                    // URI to a newly created object
                    // Point to our Get() method with correct Camp id
                    //var newUri = $"/api/camps/{model.Id}"; // will work, but not really a good way to do that
                    // Use Route name
                    var newUri = Url.Link("CampGet", new {moniker = camp.Moniker}); // pass as anonymous class


                    _logger.LogInformation($"New camp created successfully at url {newUri}");

                    // 201 CREATED: adds newUri to "Location" header and returns model in response body
                    return
                        Created(newUri,
                            _mapper
                                .Map<CampModel>(camp)); // convert back saved instance as it may have been changed too
                }
                else
                {
                    _logger.LogError($"Failed saving camp {model}");
                }
            }
            catch (Exception e)
            {
                _logger.LogCritical($"Threw exception while saving Camp: {e}");
            }

            _logger.LogError("Could not create new camp, returning bad request 401");
            return BadRequest();
        }

        // Handle both PUT and PATCH with a single method
        [HttpPatch("{moniker}")]
        [HttpPut("{moniker}")]
        public async Task<IActionResult> UpdateCampById(string moniker, [FromBody] CampModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var oldCamp = _repo.GetCampByMoniker(moniker);
                if (oldCamp == null)
                {
                    return NotFound($"Could not find a camp with moniker {moniker}");
                }

                // map fields from model to oldCamp, modifying it
                _mapper.Map(model, oldCamp);

                if (await _repo.SaveAllAsync())
                {
                    return Ok(_mapper.Map<CampModel>(oldCamp));
                }
            }
            catch (Exception e)
            {
                _logger.LogCritical($"Threw exception while updating Camp: {e}");
            }

            return BadRequest("Couldn't update Camp");
        }

        // DELTE: api/camps/4
        [HttpDelete("{moniker}")]
        public async Task<IActionResult> DeleteCampByMoniker(string moniker)
        {
            try
            {
                var oldCamp = _repo.GetCampByMoniker(moniker);
                if (oldCamp == null)
                {
                    return NotFound($"Could not find a camp with ID of {moniker}");
                }

                _repo.Delete(oldCamp);
                if (await _repo.SaveAllAsync())
                {
                    return Ok(); // no sense to return any data, since it doesn't exist anymore
                }
            }
            catch (Exception e)
            {
                _logger.LogCritical($"Threw exception while deleting Camp: {e}");
            }

            return BadRequest("Couldn't delete Camp");
        }

        /* ACTHUNG! Note that posting to a camp with ID is pointless and should not be implemented!         
        [HttpPost("{id}")]
        public IActionResult PostToCampById(int id){ ... }
        * As well as deleting whole collection should not be possible too:
        * [HttpDelete()]
        public IActionResult DeleteAllCampts(){ ... }
        */
    }
}