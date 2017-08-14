using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MyCodeCamp.Data;
using MyCodeCamp.Data.Entities;
using MyCodeCamp.Models;

namespace MyCodeCamp.Controllers
{
    // Association controller (see image in resources)
    [Route("api/camps/{moniker}/speakers")] // every action will have this "moniker" parameter
    public class SpeakersController : BaseController
    {
        private ICampRepository _repo;
        private readonly ILogger<SpeakersController> _logger;
        private IMapper _mapper;

        public SpeakersController(ICampRepository repo, ILogger<SpeakersController> logger, IMapper mapper)
        {
            _repo = repo;
            _logger = logger;
            _mapper = mapper;
        }

        [HttpGet]
        public IActionResult GetAllSpeakers(string moniker)
        {
            var speakers = _repo.GetSpeakersByMoniker(moniker);
            return Ok(_mapper.Map<IEnumerable<SpeakerModel>>(speakers));
        }

        [HttpGet("{id}", Name = "SpeakerGet")]
        public IActionResult GetSpeakerById(string moniker, int id)
        {
            var speaker = _repo.GetSpeaker(id);

            if (speaker == null) return NotFound();
            // check if speaker matches given camp
            if (speaker.Camp.Moniker != moniker) return BadRequest($"Speaker #{id} doesn't belong to camp {moniker}");

            return Ok(_mapper.Map<SpeakerModel>(speaker));
        }

        [HttpPost]
        public async Task<IActionResult> Post(string moniker, [FromBody] SpeakerModel model)
        {
            try
            {
                var speaker = _mapper.Map<Speaker>(model);
                // SpeakerModel doesn't have camp property, but Speaker entity requires it => find a camp
                var camp = _repo.GetCampByMoniker(moniker);
                if (camp == null)
                {
                    return BadRequest($"Could not find camp \"{moniker}\"");
                }
                speaker.Camp = camp;

                _repo.Add(speaker);

                if (await _repo.SaveAllAsync())
                {
                    var url = Url.Link("SpeakerGet", new {moniker = camp.Moniker, id = speaker.Id});
                    return Created(url, _mapper.Map<SpeakerModel>(speaker));
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"Exception thrown while adding speaker: {e}");
            }

            return BadRequest("Could not add new speaker");
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(string moniker, int id, [FromBody] SpeakerModel model)
        {
            try
            {
                var speaker = _repo.GetSpeaker(id);
                if (speaker == null) return NotFound();
                if (speaker.Camp.Moniker != moniker) return BadRequest("Speaker and camp do not match");

                // Copy the data from the model into the speaker where appropriate
                _mapper.Map(model, speaker);

                if (await _repo.SaveAllAsync())
                {
                    // again map the instance from the database (do not return model object itself, as database might change it)
                    return Ok(_mapper.Map<SpeakerModel>(speaker));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception thrown while updating speaker: {ex}");
            }

            return BadRequest(ModelState);
        }
    }
}