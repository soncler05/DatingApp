using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using CloudinaryDotNet;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Models;
using DatingApp.API.Properties.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DatingApp.API.Controllers
{
    [Authorize]
    [Route("api/users/{userId}/photos")]
    [ApiController]
    public class PhotosController : ControllerBase
    {
        private readonly IDatingRepository _datingRepository;
        private readonly IMapper _mapper;
        private readonly IOptions<CloudinarySettings> _cloudinaryConfig;
        private Cloudinary _cloudinary;

        public PhotosController(IDatingRepository datingRepository, IMapper mapper,
        IOptions<CloudinarySettings> cloudinaryConfig)
        {
            _cloudinaryConfig = cloudinaryConfig;
            _mapper = mapper;
            _datingRepository = datingRepository;
             Account account = new Account(
                 _cloudinaryConfig.Value.CloudName,
                 _cloudinaryConfig.Value.ApiKey,
                 _cloudinaryConfig.Value.ApiSecret
             );

             _cloudinary = new Cloudinary(account);

        }

        [HttpGet("{id}", Name="GetPhoto")]
        public async Task<IActionResult> GetPhoto(int id)
        {
            var photo = await _datingRepository.GetPhoto(id);
            return Ok( _mapper.Map<PhotoForReturnDto>(photo));

        }

        [HttpPost]
        public async Task<IActionResult> AddPhotoForUser(int userId, 
        [FromForm]PhotoForCreationDto photoForCreation)
        {
            if(userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var user = await _datingRepository.GetUser(userId); 
            
            var file = photoForCreation.File;
            var uploadResult = new CloudinaryDotNet.Actions.ImageUploadResult();

            if(file != null && file.Length > 0)
            {
                using (var stream = file.OpenReadStream())
                {
                    var uploadParams = new CloudinaryDotNet.Actions.ImageUploadParams()
                    {
                        File = new FileDescription(file.Name, stream),
                        Transformation = new Transformation().Width(500).Height(500).Crop("fill").Gravity("face")
                    };

                    uploadResult = _cloudinary.Upload(uploadParams);
                }


                photoForCreation.Url = uploadResult.Uri.ToString();
                photoForCreation.PublicId = uploadResult.PublicId.ToString();

                var photo = _mapper.Map<Photo>(photoForCreation);

                if (!user.Photos.Any(u => u.IsMain))
                {
                    photo.IsMain = true;
                }

                user.Photos.Add(photo);

                if(await _datingRepository.SaveAll())
                    return CreatedAtRoute("GetPhoto", new {userId = userId, id = photo.Id},
                            _mapper.Map<PhotoForReturnDto>(photo));
                
                return BadRequest("Could not add the photo");

            }

            throw new System.Exception("No file has been received");
        }

        [HttpPost("{id}/setMain")]
        public async Task<IActionResult> SetMainPhoto(int userId, int id)
        {
            if(userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();
            var photoFromRepo = await _datingRepository.GetPhoto(id);

            if(photoFromRepo.IsMain)
                        return BadRequest("This is already the main photo");
            
            var mainPhoto = await _datingRepository.GetMainPhotoForUser(userId);

            mainPhoto.IsMain = false;

            photoFromRepo.IsMain = true;

            if(await _datingRepository.SaveAll())
                return NoContent();
            return BadRequest("Could not set photo to main");
        }
    }
}