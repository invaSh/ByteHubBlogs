using Main.Data;
using Main.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace Main.Repos
{
    public class TagRepo : ITagRepo
    {
        private readonly ApplicationDbContext _db;
        public TagRepo(ApplicationDbContext dbContext)
        {
            _db = dbContext;
        }
        public async Task<Tag> AddAsync(Tag tag)
        {
            await _db.Tags.AddAsync(tag);
            await _db.SaveChangesAsync();

            return tag;
        }


        public async Task<Tag?> DeleteAsync(Guid id)
        {
            var expectedTag = await _db.Tags.FindAsync(id); 
            
            if(expectedTag != null)
            {
                _db.Tags.Remove(expectedTag);
                await _db.SaveChangesAsync();
                return expectedTag;
            }

            return null;
        }

        public async Task<IEnumerable<Tag>> GetAllAsync()
        {
           return await _db.Tags.ToListAsync();
        }

        public async Task<Tag?> GetAsync(Guid id)
        {
            return await _db.Tags.FirstOrDefaultAsync(x=> x.Id == id);
        }

        public async Task<Tag?> UpdateAsync(Tag tag)
        {
            var tag1 = await _db.Tags.FindAsync(tag.Id);
            if(tag1 != null)
            {
                tag1.Name = tag.Name;
                tag1.DisplayName = tag.DisplayName;

                await _db.SaveChangesAsync();

                return tag;
            }

            return null;
        }
    }
}
