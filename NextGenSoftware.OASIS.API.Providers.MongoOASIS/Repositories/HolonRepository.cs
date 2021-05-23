﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using NextGenSoftware.OASIS.API.Core.Enums;
using NextGenSoftware.OASIS.API.Core.Managers;
using NextGenSoftware.OASIS.API.Providers.MongoDBOASIS.Entities;
using NextGenSoftware.OASIS.API.Providers.MongoDBOASIS.Interfaces;

namespace NextGenSoftware.OASIS.API.Providers.MongoDBOASIS.Repositories
{
    public class HolonRepository : IHolonRepository
    {
        private MongoDbContext _dbContext;

        public HolonRepository(MongoDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Holon> AddAsync(Holon holon)
        {
            try
            {
                holon.HolonId = Guid.NewGuid();
                await _dbContext.Holon.InsertOneAsync(holon);
                return holon;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public Holon Add(Holon holon)
        {
            try
            {
                holon.HolonId = Guid.NewGuid();
                _dbContext.Holon.InsertOne(holon);
                return holon;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<Holon> GetHolonAsync(Guid id)
        {
            try
            {
                FilterDefinition<Holon> filter = Builders<Holon>.Filter.Where(x => x.HolonId == id);
                return await _dbContext.Holon.FindAsync(filter).Result.FirstOrDefaultAsync();
            }
            catch
            {
                throw;
            }
        }

        public Holon GetHolon(Guid id)
        {
            try
            {
                FilterDefinition<Holon> filter = Builders<Holon>.Filter.Where(x => x.HolonId == id);
                return _dbContext.Holon.Find(filter).FirstOrDefault();
            }
            catch
            {
                throw;
            }
        }

        public async Task<Holon> GetHolonAsync(string providerKey)
        {
            try
            {
                FilterDefinition<Holon> filter = Builders<Holon>.Filter.Where(x => x.ProviderKey[ProviderType.MongoDBOASIS] == providerKey);
                return await _dbContext.Holon.FindAsync(filter).Result.FirstOrDefaultAsync();
            }
            catch
            {
                throw;
            }
        }

        public Holon GetHolon(string providerKey)
        {
            try
            {
                FilterDefinition<Holon> filter = Builders<Holon>.Filter.Where(x => x.ProviderKey[ProviderType.MongoDBOASIS] == providerKey);
                return _dbContext.Holon.Find(filter).FirstOrDefault();
            }
            catch
            {
                throw;
            }
        }

        public async Task<IEnumerable<Holon>> GetAllHolonsAsync(HolonType holonType = HolonType.All)
        {
            try
            {
                if (holonType != HolonType.All)
                {
                    return await _dbContext.Holon.FindAsync(_ => true).Result.ToListAsync();
                }
                else
                {
                    FilterDefinition<Holon> filter = Builders<Holon>.Filter.Where(x => x.HolonType == holonType);
                    return await _dbContext.Holon.FindAsync(filter).Result.ToListAsync();
                }
            }
            catch
            {
                throw;
            }
        }

        public IEnumerable<Holon> GetAllHolons(HolonType holonType = HolonType.All)
        {
            try
            {
                if (holonType != HolonType.All)
                {
                    return _dbContext.Holon.Find(_ => true).ToList();
                }
                else
                {
                    FilterDefinition<Holon> filter = Builders<Holon>.Filter.Where(x => x.HolonType == holonType);
                    return _dbContext.Holon.Find(filter).ToList();
                }
            }
            catch
            {
                throw;
            }
        }

        public async Task<IEnumerable<Holon>> GetAllHolonsForParentAsync(Guid id, HolonType holonType)
        {
            try
            {
                return await _dbContext.Holon.FindAsync(BuildFilter(id, holonType)).Result.ToListAsync();
            }
            catch
            {
                throw;
            }
        }

        public IEnumerable<Holon> GetAllHolonsForParent(Guid id, HolonType holonType)
        {
            try
            {
                return _dbContext.Holon.Find(BuildFilter(id, holonType)).ToList();
            }
            catch
            {
                throw;
            }
        }

        public async Task<IEnumerable<Holon>> GetAllHolonsForParentAsync(string providerKey, HolonType holonType)
        {
            try
            {
                return await _dbContext.Holon.FindAsync(BuildFilter(providerKey, holonType)).Result.ToListAsync();
            }
            catch
            {
                throw;
            }
        }

        public IEnumerable<Holon> GetAllHolonsForParent(string providerKey, HolonType holonType)
        {
            try
            {
                return _dbContext.Holon.Find(BuildFilter(providerKey, holonType)).ToList();
            }
            catch
            {
                throw;
            }
        }

        public async Task<Holon> UpdateAsync(Holon holon)
        {
            try
            {
                if (holon.Id == null)
                {
                    Holon originalHolon = await GetHolonAsync(holon.HolonId);

                    if (originalHolon != null)
                    {
                        holon.Id = originalHolon.Id;
                        holon.CreatedByAvatarId = originalHolon.CreatedByAvatarId;
                        holon.CreatedDate = originalHolon.CreatedDate;
                        holon.HolonType = originalHolon.HolonType;
                        holon.ParentZome = originalHolon.ParentZome;
                        holon.ParentZomeId = originalHolon.ParentZomeId;
                        holon.ParentMoon = originalHolon.ParentMoon;
                        holon.ParentPlanet = originalHolon.ParentPlanet;
                        holon.ParentMoonId = originalHolon.ParentMoonId;
                        holon.ParentPlanetId = originalHolon.ParentPlanetId;
                        holon.Children = originalHolon.Children;
                        holon.DeletedByAvatarId = originalHolon.DeletedByAvatarId;
                        holon.DeletedDate = originalHolon.DeletedDate;

                        //TODO: Needs more thought!
                    }
                }

                await _dbContext.Holon.ReplaceOneAsync(filter: g => g.HolonId == holon.HolonId, replacement: holon);
                return holon;
            }
            catch
            {
                throw;
            }
        }

        public Holon Update(Holon holon)
        {
            try
            {
                if (holon.Id == null)
                {
                    Holon originalHolon = GetHolon(holon.HolonId);

                    if (originalHolon != null)
                    {
                        holon.Id = originalHolon.Id;
                        holon.CreatedByAvatarId = originalHolon.CreatedByAvatarId;
                        holon.CreatedDate = originalHolon.CreatedDate;
                        holon.HolonType = originalHolon.HolonType;
                        holon.ParentZome = originalHolon.ParentZome;
                        holon.ParentZomeId = originalHolon.ParentZomeId;
                        holon.ParentMoon = originalHolon.ParentMoon;
                        holon.ParentPlanet = originalHolon.ParentPlanet;
                        holon.ParentMoonId = originalHolon.ParentMoonId;
                        holon.ParentPlanetId = originalHolon.ParentPlanetId;
                        holon.Children = originalHolon.Children;
                        holon.DeletedByAvatarId = originalHolon.DeletedByAvatarId;
                        holon.DeletedDate = originalHolon.DeletedDate;

                        //TODO: Needs more thought!
                    }
                }

                _dbContext.Holon.ReplaceOne(filter: g => g.HolonId == holon.HolonId, replacement: holon);
                return holon;
            }
            catch
            {
                throw;
            }
        }

        public async Task<bool> DeleteAsync(Guid id, bool softDelete = true)
        {
            try
            {
                if (softDelete)
                {
                    Holon holon = await GetHolonAsync(id);
                    return await SoftDeleteAsync(holon);
                }
                else
                {
                    FilterDefinition<Holon> data = Builders<Holon>.Filter.Where(x => x.HolonId == id);
                    await _dbContext.Holon.DeleteOneAsync(data);
                    return true;
                }
            }
            catch
            {
                throw;
            }
        }

        public bool Delete(Guid id, bool softDelete = true)
        {
            try
            {
                if (softDelete)
                {
                    Holon holon = GetHolon(id);
                    return SoftDelete(holon);
                }
                else
                {
                    FilterDefinition<Holon> data = Builders<Holon>.Filter.Where(x => x.HolonId == id);
                    _dbContext.Holon.DeleteOne(data);
                    return true;
                }
            }
            catch
            {
                throw;
            }
        }

        public async Task<bool> DeleteAsync(string providerKey, bool softDelete = true)
        {
            try
            {
                if (softDelete)
                {
                    Holon holon = await GetHolonAsync(providerKey);
                    return await SoftDeleteAsync(holon);
                }
                else
                {
                    FilterDefinition<Holon> data = Builders<Holon>.Filter.Where(x => x.ProviderKey[ProviderType.MongoDBOASIS] == providerKey);
                    await _dbContext.Holon.DeleteOneAsync(data);
                    return true;
                }
            }
            catch
            {
                throw;
            }
        }

        public bool Delete(string providerKey, bool softDelete = true)
        {
            try
            {
                if (softDelete)
                {
                    Holon holon = GetHolon(providerKey);
                    return SoftDelete(holon);
                }
                else
                {
                    FilterDefinition<Holon> data = Builders<Holon>.Filter.Where(x => x.ProviderKey[ProviderType.MongoDBOASIS] == providerKey);
                    _dbContext.Holon.DeleteOne(data);
                    return true;
                }
            }
            catch
            {
                throw;
            }
        }

        private async Task<bool> SoftDeleteAsync(Holon holon)
        {
            try
            {
                if (AvatarManager.LoggedInAvatar != null)
                    holon.DeletedByAvatarId = AvatarManager.LoggedInAvatar.Id.ToString();

                holon.DeletedDate = DateTime.Now;
                await _dbContext.Holon.ReplaceOneAsync(filter: g => g.Id == holon.Id, replacement: holon);
                return true;
            }
            catch
            {
                throw;
            }
        }

        private bool SoftDelete(Holon holon)
        {
            try
            {
                if (AvatarManager.LoggedInAvatar != null)
                    holon.DeletedByAvatarId = AvatarManager.LoggedInAvatar.Id.ToString();

                holon.DeletedDate = DateTime.Now;
                _dbContext.Holon.ReplaceOne(filter: g => g.Id == holon.Id, replacement: holon);
                return true;
            }
            catch
            {
                throw;
            }
        }

        private FilterDefinition<Holon> BuildFilter(string providerKey, HolonType holonType)
        {
            FilterDefinition<Holon> filter = null;

            if (holonType != HolonType.All)
            {
                filter = Builders<Holon>.Filter.And(
                Builders<Holon>.Filter.Where(p => p.ProviderKey[ProviderType.MongoDBOASIS] == providerKey),
                Builders<Holon>.Filter.Where(p => p.HolonType == holonType));
            }
            else
            {
                filter = Builders<Holon>.Filter.And(
                Builders<Holon>.Filter.Where(p => p.ProviderKey[ProviderType.MongoDBOASIS] == providerKey));
            }

            return filter;
        }

        private FilterDefinition<Holon> BuildFilter(Guid id, HolonType holonType)
        {
            FilterDefinition<Holon> filter = null;

            if (holonType != HolonType.All)
            {
                filter = Builders<Holon>.Filter.And(
                Builders<Holon>.Filter.Where(p => p.HolonId == id),
                Builders<Holon>.Filter.Where(p => p.HolonType == holonType));
            }
            else
            {
                filter = Builders<Holon>.Filter.And(
                Builders<Holon>.Filter.Where(p => p.HolonId == id));
            }

            return filter;
        }
    }
}