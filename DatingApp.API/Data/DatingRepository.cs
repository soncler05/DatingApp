using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Data
{
    public class DatingRepository : IDatingRepository
    {
        public DataContext _context { get; }
        public DatingRepository(DataContext context)
        {
            _context = context;

        }
        public void Add<T>(T entity) where T : class
        {
            _context.Add(entity);
        }

        public void Delete<T>(T entity) where T : class
        {
            _context.Remove(entity);
        }

        public async Task<User> GetUser(int id)
        {
            var user = await _context.Users.Include(p => p.Photos).FirstOrDefaultAsync(x=>x.Id == id);
           return user;
        }

        public async Task<PagedList<User>> GetUsers(UserParams userParams)
        {
           var usersQuerry = _context.Users.Include(p => p.Photos).OrderByDescending(u => u.LastActive).AsQueryable();
           usersQuerry = usersQuerry.Where(u => u.Id != userParams.UserId);
           usersQuerry = usersQuerry.Where(u => u.Gender == userParams.Gender);
           if(userParams.MinAge != 18 || userParams.MaxAge != 99)
           {
               var minDob = DateTime.Today.AddYears(-userParams.MaxAge - 1);
               var maxDob = DateTime.Today.AddYears(-userParams.MinAge );

               usersQuerry = usersQuerry.Where(u => u.DateOfBirth >= minDob && u.DateOfBirth <= maxDob);
           }

           if(!string.IsNullOrEmpty(userParams.OrderBy)){
               switch(userParams.OrderBy)
               {
                   case "created":
                   usersQuerry = usersQuerry.OrderByDescending(u => u.Created);
                   break; 
                   default:
                   usersQuerry = usersQuerry.OrderByDescending(u => u.LastActive);
                   break; 
               }
           }

           return await PagedList<User>.CreateAsync(usersQuerry, userParams.PageNumber, userParams.PageSize);
        }

        public async Task<bool> SaveAll()
        {
           return await _context.SaveChangesAsync() > 0;
        }

        public async Task<Photo> GetPhoto(int id)
        {
            return await _context.Photos.FirstOrDefaultAsync(x=> x.Id == id);
        }

        public async Task<Photo> GetMainPhotoForUser(int userId)
        {
           return await _context.Photos.FirstOrDefaultAsync(x => x.UserId == userId && x.IsMain);
        }
    }
}