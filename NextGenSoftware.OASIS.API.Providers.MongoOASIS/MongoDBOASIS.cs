﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using NextGenSoftware.OASIS.API.Core;
using NextGenSoftware.OASIS.API.Core.Interfaces;
using NextGenSoftware.OASIS.API.Core.Enums;
using NextGenSoftware.OASIS.API.Core.Helpers;
using NextGenSoftware.OASIS.API.Core.Interfaces.STAR;
using NextGenSoftware.OASIS.API.Providers.MongoDBOASIS.Repositories;
using Avatar = NextGenSoftware.OASIS.API.Providers.MongoDBOASIS.Entities.Avatar;
using AvatarDetail = NextGenSoftware.OASIS.API.Providers.MongoDBOASIS.Entities.AvatarDetail;
using Holon = NextGenSoftware.OASIS.API.Providers.MongoDBOASIS.Entities.Holon;

namespace NextGenSoftware.OASIS.API.Providers.MongoDBOASIS
{
    public class MongoDBOASIS : OASISStorageBase, IOASISStorage, IOASISNET, IOASISSuperStar
    {
        public MongoDbContext Database { get; set; }
        private AvatarRepository _avatarRepository = null;
        private HolonRepository _holonRepository = null;
        private SearchRepository _searchRepository = null;

        public string ConnectionString { get; set; }
        public string DBName { get; set; }

        public MongoDBOASIS(string connectionString, string dbName)
        {
            ConnectionString = connectionString;
            DBName = dbName;

            this.ProviderName = "MongoDBOASIS";
            this.ProviderDescription = "MongoDB Atlas Provider";
            this.ProviderType = new EnumValue<ProviderType>(Core.Enums.ProviderType.MongoDBOASIS);
            this.ProviderCategory = new EnumValue<ProviderCategory>(Core.Enums.ProviderCategory.StorageAndNetwork);

            /*
            ConventionRegistry.Register(
                   "DictionaryRepresentationConvention",
                   new ConventionPack { new DictionaryRepresentationConvention(DictionaryRepresentation.ArrayOfArrays) },
                   _ => true);*/
        }

        public override void ActivateProvider()
        {
            //TODO: {URGENT} Find out how to check if MongoDB is connected, etc here...
            if (Database == null)
            {
                Database = new MongoDbContext(ConnectionString, DBName);
                _avatarRepository = new AvatarRepository(Database);
                _holonRepository = new HolonRepository(Database);
            }

            base.ActivateProvider();
        }

        public override void DeActivateProvider()
        {
            //TODO: {URGENT} Disconnect, Dispose and release resources here.
            Database.MongoDB = null;
            Database.MongoClient = null;
            Database = null;

            base.DeActivateProvider();
        }

        public override async Task<IEnumerable<IAvatar>> LoadAllAvatarsAsync()
        {
            return ConvertMongoEntitysToOASISAvatars(await _avatarRepository.GetAvatarsAsync());
        }

        public override IEnumerable<IAvatar> LoadAllAvatars()
        {
            return ConvertMongoEntitysToOASISAvatars(_avatarRepository.GetAvatars());
        }

        public override IAvatar LoadAvatarByEmail(string avatarEmail)
        {
            return ConvertMongoEntityToOASISAvatar(_avatarRepository.GetAvatar(x => x.Email == avatarEmail));
        }

        public override IAvatar LoadAvatarByUsername(string avatarUsername)
        {
            return ConvertMongoEntityToOASISAvatar(_avatarRepository.GetAvatar(x => x.Username == avatarUsername));
        }

        public override async Task<IAvatar> LoadAvatarAsync(string username)
        {
            return ConvertMongoEntityToOASISAvatar(await _avatarRepository.GetAvatarAsync(username));
        }

        public override async Task<IAvatar> LoadAvatarByUsernameAsync(string avatarUsername)
        {
            return ConvertMongoEntityToOASISAvatar(await _avatarRepository.GetAvatarAsync(x => x.Username == avatarUsername));
        }

        public override IAvatar LoadAvatar(string username)
        {
            return ConvertMongoEntityToOASISAvatar(_avatarRepository.GetAvatar(username));
        }

        public override async Task<IAvatar> LoadAvatarAsync(Guid Id)
        {
            return ConvertMongoEntityToOASISAvatar(await _avatarRepository.GetAvatarAsync(Id));
        }

        public override async Task<IAvatar> LoadAvatarByEmailAsync(string avatarEmail)
        {
            return ConvertMongoEntityToOASISAvatar(await _avatarRepository.GetAvatarAsync(x => x.Email == avatarEmail));
        }

        public override IAvatar LoadAvatar(Guid Id)
        {
            return ConvertMongoEntityToOASISAvatar(_avatarRepository.GetAvatar(Id));
        }

        public override async Task<IAvatar> LoadAvatarAsync(string username, string password)
        {
            return ConvertMongoEntityToOASISAvatar(await _avatarRepository.GetAvatarAsync(username, password));
        }

        public override IAvatar LoadAvatar(string username, string password)
        {
            return ConvertMongoEntityToOASISAvatar(_avatarRepository.GetAvatar(username, password));
        }

        public override async Task<IAvatar> SaveAvatarAsync(IAvatar avatar)
        {
            return ConvertMongoEntityToOASISAvatar(avatar.Id == Guid.Empty ?
               await _avatarRepository.AddAsync(ConvertOASISAvatarToMongoEntity(avatar)) :
               await _avatarRepository.UpdateAsync(ConvertOASISAvatarToMongoEntity(avatar)));
        }

        public override IAvatarDetail SaveAvatarDetail(IAvatarDetail Avatar)
        {
            throw new NotImplementedException();
        }

        public override async Task<IAvatarDetail> SaveAvatarDetailAsync(IAvatarDetail Avatar)
        {
            throw new NotImplementedException();
        }

        public override IAvatar SaveAvatar(IAvatar avatar)
        {
            return ConvertMongoEntityToOASISAvatar(avatar.Id == Guid.Empty ?
                _avatarRepository.Add(ConvertOASISAvatarToMongoEntity(avatar)) :
                _avatarRepository.Update(ConvertOASISAvatarToMongoEntity(avatar)));
        }

        public override bool DeleteAvatarByUsername(string avatarUsername, bool softDelete = true)
        {
            return _avatarRepository.Delete(x => x.Username == avatarUsername, softDelete);
        }

        public override async Task<bool> DeleteAvatarAsync(Guid id, bool softDelete = true)
        {
            return await _avatarRepository.DeleteAsync(id, softDelete);
        }

        public override async Task<bool> DeleteAvatarByEmailAsync(string avatarEmail, bool softDelete = true)
        {
            return await _avatarRepository.DeleteAsync(x => x.Email == avatarEmail, softDelete);
        }

        public override async Task<bool> DeleteAvatarByUsernameAsync(string avatarUsername, bool softDelete = true)
        {
            return await _avatarRepository.DeleteAsync(x => x.Username == avatarUsername, softDelete);
        }

        public override bool DeleteAvatar(Guid id, bool softDelete = true)
        {
            return _avatarRepository.Delete(id, softDelete);
        }

        public override bool DeleteAvatarByEmail(string avatarEmail, bool softDelete = true)
        {
            return _avatarRepository.Delete(x => x.Email == avatarEmail, softDelete);
        }

        public override async Task<IAvatar> LoadAvatarForProviderKeyAsync(string providerKey)
        {
            return ConvertMongoEntityToOASISAvatar(await _avatarRepository.GetAvatarAsync(providerKey));
        }

        public override IAvatar LoadAvatarForProviderKey(string providerKey)
        {
            return ConvertMongoEntityToOASISAvatar(_avatarRepository.GetAvatar(providerKey));
        }

        public override bool DeleteAvatar(string providerKey, bool softDelete = true)
        {
            return _avatarRepository.Delete(providerKey, softDelete);
        }

        public override async Task<bool> DeleteAvatarAsync(string providerKey, bool softDelete = true)
        {
            return await _avatarRepository.DeleteAsync(providerKey, softDelete);
        }


        //TODO: {URGENT} FIX BEB SEARCH TO WORK WITH ISearchParams instead of string as it use to be!
        public override async Task<ISearchResults> SearchAsync(ISearchParams searchTerm)
        {
            return await _searchRepository.SearchAsync(searchTerm);
        }

        public override IAvatarDetail LoadAvatarDetailByUsername(string avatarUsername)
        {
            return ConvertMongoEntityToOASISAvatarDetail(_avatarRepository.GetAvatarDetail(x => x.Username == avatarUsername));
        }

        public override async Task<IAvatarDetail> LoadAvatarDetailAsync(Guid id)
        {
            return ConvertMongoEntityToOASISAvatarDetail(await _avatarRepository.GetAvatarDetailAsync(id));
        }

        public override async Task<IAvatarDetail> LoadAvatarDetailByUsernameAsync(string avatarUsername)
        {
            return ConvertMongoEntityToOASISAvatarDetail(await _avatarRepository.GetAvatarDetailAsync(x => x.Username == avatarUsername));
        }

        public override async Task<IAvatarDetail> LoadAvatarDetailByEmailAsync(string avatarEmail)
        {
            return ConvertMongoEntityToOASISAvatarDetail(await _avatarRepository.GetAvatarDetailAsync(x => x.Email == avatarEmail));
        }

        public override async Task<IEnumerable<IAvatarDetail>> LoadAllAvatarDetailsAsync()
        {
            return ConvertMongoEntitysToOASISAvatarDetails(await _avatarRepository.GetAvatarDetailsAsync());
        }

        //public override async Task<IAvatarThumbnail> LoadAvatarThumbnailAsync(Guid id)
        //{
        //    return await _avatarRepository.GetAvatarThumbnailByIdAsync(id);
        //}

        public override IAvatarDetail LoadAvatarDetail(Guid id)
        {
            return ConvertMongoEntityToOASISAvatarDetail(_avatarRepository.GetAvatarDetail(id));
        }

        public override IAvatarDetail LoadAvatarDetailByEmail(string avatarEmail)
        {
            return ConvertMongoEntityToOASISAvatarDetail(_avatarRepository.GetAvatarDetail(x => x.Email == avatarEmail));
        }

        public override IEnumerable<IAvatarDetail> LoadAllAvatarDetails()
        {
            return ConvertMongoEntitysToOASISAvatarDetails(_avatarRepository.GetAvatarDetails());
        }

        //public override IAvatarThumbnail LoadAvatarThumbnail(Guid id)
        //{
        //    return _avatarRepository.GetAvatarThumbnailById(id);
        //}

        public override async Task<IHolon> LoadHolonAsync(Guid id)
        {
            //TODO: Finish implementing OASISResult properly...
            return ConvertMongoEntityToOASISHolon(new OASISResult<Holon>(await _holonRepository.GetHolonAsync(id))).Result;
        }

        public override IHolon LoadHolon(Guid id)
        {
            //TODO: Finish implementing OASISResult properly...
            return ConvertMongoEntityToOASISHolon(new OASISResult<Holon>(_holonRepository.GetHolon(id))).Result;
        }


        //public override T LoadHolon<T>(Guid id)
        //{
        //    return ConvertMongoEntityToOASISHolon(new OASISResult<Holon>(_holonRepository.GetHolon(id))).Result;
        //}

        public override async Task<IHolon> LoadHolonAsync(string providerKey)
        {
            //TODO: Finish implementing OASISResult properly...
            return ConvertMongoEntityToOASISHolon(new OASISResult<Holon>(await _holonRepository.GetHolonAsync(providerKey))).Result;
        }

        public override IHolon LoadHolon(string providerKey)
        {
            //TODO: Finish implementing OASISResult properly...
            return ConvertMongoEntityToOASISHolon(new OASISResult<Holon>(_holonRepository.GetHolon(providerKey))).Result;
        }

        public override async Task<IEnumerable<IHolon>> LoadHolonsForParentAsync(Guid id, HolonType type = HolonType.All)
        {
            return ConvertMongoEntitysToOASISHolons(await _holonRepository.GetAllHolonsForParentAsync(id, type));
        }

        public override IEnumerable<IHolon> LoadHolonsForParent(Guid id, HolonType type = HolonType.All)
        {
            return ConvertMongoEntitysToOASISHolons(_holonRepository.GetAllHolonsForParent(id, type));
        }

        public override async Task<OASISResult<IEnumerable<IHolon>>> LoadHolonsForParentAsync(string providerKey, HolonType type = HolonType.All)
        {
            OASISResult<IEnumerable<IHolon>> result = new OASISResult<IEnumerable<IHolon>>();
            OASISResult<IEnumerable<Holon>> repoResult = await _holonRepository.GetAllHolonsForParentAsync(providerKey, type);

            if (repoResult.IsError)
            {
                result.IsError = true;
                result.Message = repoResult.Message;
            }
            else
                result.Result = ConvertMongoEntitysToOASISHolons(repoResult.Result);

            return result;
        }

        public override IEnumerable<IHolon> LoadHolonsForParent(string providerKey, HolonType type = HolonType.All)
        {
            return ConvertMongoEntitysToOASISHolons(_holonRepository.GetAllHolonsForParent(providerKey, type));
        }

        public override async Task<IEnumerable<IHolon>> LoadAllHolonsAsync(HolonType type = HolonType.All)
        {
            return ConvertMongoEntitysToOASISHolons(await _holonRepository.GetAllHolonsAsync(type));
        }

        public override IEnumerable<IHolon> LoadAllHolons(HolonType type = HolonType.All)
        {
            return ConvertMongoEntitysToOASISHolons(_holonRepository.GetAllHolons(type));
        }

        public override async Task<OASISResult<IHolon>> SaveHolonAsync(IHolon holon, bool saveChildrenRecursive = true)
        {
            OASISResult<IHolon> result =  holon.IsNewHolon
                ? ConvertMongoEntityToOASISHolon(await _holonRepository.AddAsync(ConvertOASISHolonToMongoEntity(holon)))
                : ConvertMongoEntityToOASISHolon(await _holonRepository.UpdateAsync(ConvertOASISHolonToMongoEntity(holon)));

            if (!result.IsError && result.Result != null && saveChildrenRecursive && result.Result.Children != null && result.Result.Children.Count() > 0)
            {
                OASISResult<IEnumerable<IHolon>> saveChildrenResult = SaveHolons(result.Result.Children);

                if (!saveChildrenResult.IsError && saveChildrenResult.Result != null)
                    result.Result.Children = saveChildrenResult.Result;
                else
                {
                    result.IsError = true;
                    result.Message = $"Holon with id {holon.Id} and name {holon.Name} saved but it's children failed to save. Reason: {saveChildrenResult.Message}";
                }
            }

            return result;
        }

        public override OASISResult<IHolon> SaveHolon(IHolon holon, bool saveChildrenRecursive = true)
        {
            OASISResult<IHolon> result = holon.IsNewHolon
                ? ConvertMongoEntityToOASISHolon(_holonRepository.Add(ConvertOASISHolonToMongoEntity(holon)))
                : ConvertMongoEntityToOASISHolon(_holonRepository.Update(ConvertOASISHolonToMongoEntity(holon)));

            if (!result.IsError && result.Result != null && saveChildrenRecursive && result.Result.Children != null && result.Result.Children.Count() > 0)
            {
                OASISResult<IEnumerable<IHolon>> saveChildrenResult = SaveHolons(result.Result.Children);

                if (!saveChildrenResult.IsError && saveChildrenResult.Result != null)
                    result.Result.Children = saveChildrenResult.Result;
                else
                {
                    result.IsError = true;
                    result.Message = $"Holon with id {holon.Id} and name {holon.Name} saved but it's children failed to save. Reason: {saveChildrenResult.Message}";
                }
            }

            return result;
        }

        public override OASISResult<IEnumerable<IHolon>> SaveHolons(IEnumerable<IHolon> holons, bool saveChildrenRecursive = true)
        {
            OASISResult<IEnumerable<IHolon>> result = new OASISResult<IEnumerable<IHolon>>();
            List<IHolon> savedHolons = new List<IHolon>();

            if (holons.Count() == 0)
            {
                result.Message = "No holons found to save.";
                result.IsWarning = true;
                result.IsSaved = false;
                return result;
            }

            // Recursively save all child holons.
            foreach (IHolon holon in holons)
            {
                OASISResult<IHolon> holonResult = SaveHolon(holon);

                if (!holonResult.IsError && holonResult.Result != null)
                {
                    if (saveChildrenRecursive)
                    {
                        OASISResult<IEnumerable<IHolon>> saveChildrenResult = SaveHolons(holonResult.Result.Children);

                        if (!saveChildrenResult.IsError && saveChildrenResult.Result != null)
                            holonResult.Result.Children = saveChildrenResult.Result;
                        else
                        {
                            result.IsError = true;
                            result.InnerMessages.Add($"Holon with id {holon.Id} and name {holon.Name} saved but it's children failed to save. Reason: {saveChildrenResult.Message}");
                        }
                    }

                    savedHolons.Add(holonResult.Result);
                }
                else
                {
                    result.IsError = true;
                    result.InnerMessages.Add($"Holon with id {holon.Id} and name {holon.Name} faild to save. Reason: {holonResult.Message}");
                }
            }

            if (result.IsError)
                result.Message = "One or more errors occured saving the holons in the SQLLiteOASIS Provider. Please check the InnerMessages property for more infomration.";

            return result;
        }

        public override async Task<OASISResult<IEnumerable<IHolon>>> SaveHolonsAsync(IEnumerable<IHolon> holons, bool saveChildrenRecursive = true)
        {
            OASISResult<IEnumerable<IHolon>> result = new OASISResult<IEnumerable<IHolon>>();
            List<IHolon> savedHolons = new List<IHolon>();

            if (holons.Count() == 0)
            {
                result.Message = "No holons found to save.";
                result.IsWarning = true;
                result.IsSaved = false;
                return result;
            }

            // Recursively save all child holons.
            foreach (IHolon holon in holons)
            {
                OASISResult<IHolon> holonResult = await SaveHolonAsync(holon);

                if (!holonResult.IsError && holonResult.Result != null)
                {
                    if (saveChildrenRecursive)
                    {
                        OASISResult<IEnumerable<IHolon>> saveChildrenResult = await SaveHolonsAsync(holonResult.Result.Children);

                        if (!saveChildrenResult.IsError && saveChildrenResult.Result != null)
                            holonResult.Result.Children = saveChildrenResult.Result;
                        else
                        {
                            result.IsError = true;
                            result.InnerMessages.Add($"Holon with id {holon.Id} and name {holon.Name} saved but it's children failed to save. Reason: {saveChildrenResult.Message}");
                        }
                    }

                    savedHolons.Add(holonResult.Result);
                }
                else
                {
                    result.IsError = true;
                    result.InnerMessages.Add($"Holon with id {holon.Id} and name {holon.Name} faild to save. Reason: {holonResult.Message}");
                }
            }

            if (result.IsError)
                result.Message = "One or more errors occured saving the holons in the SQLLiteOASIS Provider. Please check the InnerMessages property for more infomration.";

            return result;
        }

        public override bool DeleteHolon(Guid id, bool softDelete = true)
        {
            return _holonRepository.Delete(id, softDelete);
        }

        public override async Task<bool> DeleteHolonAsync(Guid id, bool softDelete = true)
        {
            return await _holonRepository.DeleteAsync(id, softDelete);
        }

        public override bool DeleteHolon(string providerKey, bool softDelete = true)
        {
            return _holonRepository.Delete(providerKey, softDelete);
        }

        public override async Task<bool> DeleteHolonAsync(string providerKey, bool softDelete = true)
        {
            return await _holonRepository.DeleteAsync(providerKey, softDelete);
        }

        public IEnumerable<IHolon> GetHolonsNearMe(HolonType Type)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IPlayer> GetPlayersNearMe()
        {
            throw new NotImplementedException();
        }


        //IOASISSuperStar Interface Implementation

        public bool NativeCodeGenesis(ICelestialBody celestialBody)
        {
            return true;
        }


        private IEnumerable<IAvatar> ConvertMongoEntitysToOASISAvatars(IEnumerable<Avatar> avatars)
        {
            List<IAvatar> oasisAvatars = new List<IAvatar>();

            foreach (Avatar avatar in avatars)
                oasisAvatars.Add(ConvertMongoEntityToOASISAvatar(avatar));

            return oasisAvatars;
        }

        private IEnumerable<IAvatarDetail> ConvertMongoEntitysToOASISAvatarDetails(IEnumerable<AvatarDetail> avatars)
        {
            List<IAvatarDetail> oasisAvatars = new List<IAvatarDetail>();

            foreach (AvatarDetail avatar in avatars)
                oasisAvatars.Add(ConvertMongoEntityToOASISAvatarDetail(avatar));

            return oasisAvatars;
        }

        private IEnumerable<IHolon> ConvertMongoEntitysToOASISHolons(IEnumerable<Holon> holons)
        {
            List<IHolon> oasisHolons = new List<IHolon>();

            foreach (Holon holon in holons)
            {
                OASISResult<IHolon> convertedResult = ConvertMongoEntityToOASISHolon(new OASISResult<Holon>(holon));
                
                if (!convertedResult.IsError && convertedResult.Result != null)
                    oasisHolons.Add(convertedResult.Result);
            }

            return oasisHolons;
        }

        private IAvatar ConvertMongoEntityToOASISAvatar(Avatar avatar)
        {
            if (avatar == null)
                return null;

            Core.Holons.Avatar oasisAvatar = new Core.Holons.Avatar();

            oasisAvatar.Id = avatar.HolonId;
            oasisAvatar.ProviderKey = avatar.ProviderKey;
            oasisAvatar.PreviousVersionId = avatar.PreviousVersionId;
            oasisAvatar.PreviousVersionProviderKey = avatar.PreviousVersionProviderKey;
            oasisAvatar.ProviderMetaData = avatar.ProviderMetaData;
            oasisAvatar.Description = avatar.Description;
            oasisAvatar.Title = avatar.Title;
            oasisAvatar.FirstName = avatar.FirstName;
            oasisAvatar.LastName = avatar.LastName;
            oasisAvatar.Email = avatar.Email;
            oasisAvatar.Password = avatar.Password;
            oasisAvatar.Username = avatar.Username;
            oasisAvatar.CreatedOASISType = avatar.CreatedOASISType;
            //oasisAvatar.CreatedProviderType = new EnumValue<ProviderType>(avatar.CreatedProviderType);
            oasisAvatar.CreatedProviderType = avatar.CreatedProviderType;
            oasisAvatar.AvatarType = avatar.AvatarType;
            oasisAvatar.HolonType = avatar.HolonType;
            oasisAvatar.Image2D = avatar.Image2D;
            //oasisAvatar.UmaJson = avatar.UmaJson; //TODO: Not sure whether to include UmaJson or not? I think Unity guys said is it pretty large?
            oasisAvatar.Karma = avatar.Karma;
            oasisAvatar.XP = avatar.XP;
            oasisAvatar.IsChanged = avatar.IsChanged;
            oasisAvatar.AcceptTerms = avatar.AcceptTerms;
            oasisAvatar.JwtToken = avatar.JwtToken;
            oasisAvatar.PasswordReset = avatar.PasswordReset;
            oasisAvatar.RefreshToken = avatar.RefreshToken;
            oasisAvatar.RefreshTokens = avatar.RefreshTokens;
            oasisAvatar.ResetToken = avatar.ResetToken;
            oasisAvatar.ResetTokenExpires = avatar.ResetTokenExpires;
            oasisAvatar.VerificationToken = avatar.VerificationToken;
            oasisAvatar.Verified = avatar.Verified;
            oasisAvatar.CreatedByAvatarId = Guid.Parse(avatar.CreatedByAvatarId);
            oasisAvatar.CreatedDate = avatar.CreatedDate;
            oasisAvatar.DeletedByAvatarId = Guid.Parse(avatar.DeletedByAvatarId);
            oasisAvatar.DeletedDate = avatar.DeletedDate;
            oasisAvatar.ModifiedByAvatarId = Guid.Parse(avatar.ModifiedByAvatarId);
            oasisAvatar.ModifiedDate = avatar.ModifiedDate;
            oasisAvatar.DeletedDate = avatar.DeletedDate;
            oasisAvatar.LastBeamedIn = avatar.LastBeamedIn;
            oasisAvatar.LastBeamedOut = avatar.LastBeamedOut;
            oasisAvatar.IsBeamedIn = avatar.IsBeamedIn;
            oasisAvatar.Version = avatar.Version;
            oasisAvatar.IsActive = avatar.IsActive;

            return oasisAvatar;
        }

        private IAvatarDetail ConvertMongoEntityToOASISAvatarDetail(AvatarDetail avatar)
        {
            if (avatar == null)
                return null;

            Core.Holons.AvatarDetail oasisAvatar = new Core.Holons.AvatarDetail();
            oasisAvatar.Id = avatar.HolonId;
            oasisAvatar.ProviderKey = avatar.ProviderKey;
            oasisAvatar.ProviderMetaData = avatar.ProviderMetaData;
            oasisAvatar.PreviousVersionId = avatar.PreviousVersionId;
            oasisAvatar.PreviousVersionProviderKey = avatar.PreviousVersionProviderKey;
            oasisAvatar.Title = avatar.Title;
            oasisAvatar.Description = avatar.Description;
            oasisAvatar.FirstName = avatar.FirstName;
            oasisAvatar.LastName = avatar.LastName;
            oasisAvatar.Email = avatar.Email;
            oasisAvatar.Username = avatar.Username;
            oasisAvatar.CreatedOASISType = avatar.CreatedOASISType;
            oasisAvatar.CreatedProviderType = avatar.CreatedProviderType;
            oasisAvatar.AvatarType = avatar.AvatarType;
            oasisAvatar.HolonType = avatar.HolonType;
            oasisAvatar.IsChanged = avatar.IsChanged;
            oasisAvatar.CreatedByAvatarId = Guid.Parse(avatar.CreatedByAvatarId);
            oasisAvatar.CreatedDate = avatar.CreatedDate;
            oasisAvatar.DeletedByAvatarId = Guid.Parse(avatar.DeletedByAvatarId);
            oasisAvatar.DeletedDate = avatar.DeletedDate;
            oasisAvatar.ModifiedByAvatarId = Guid.Parse(avatar.ModifiedByAvatarId);
            oasisAvatar.ModifiedDate = avatar.ModifiedDate;
            oasisAvatar.DeletedDate = avatar.DeletedDate;
            oasisAvatar.Version = avatar.Version;
            oasisAvatar.IsActive = avatar.IsActive;
            oasisAvatar.Image2D = avatar.Image2D;
            oasisAvatar.UmaJson = avatar.UmaJson;
            oasisAvatar.ProviderPrivateKey = avatar.ProviderPrivateKey;
            oasisAvatar.ProviderPublicKey = avatar.ProviderPublicKey;
            oasisAvatar.ProviderUsername = avatar.ProviderUsername;
            oasisAvatar.ProviderWalletAddress = avatar.ProviderWalletAddress;
            oasisAvatar.XP = avatar.XP;
            oasisAvatar.FavouriteColour = avatar.FavouriteColour;
            oasisAvatar.STARCLIColour = avatar.STARCLIColour;
            oasisAvatar.Skills = avatar.Skills;
            oasisAvatar.Spells = avatar.Spells;
            oasisAvatar.Stats = avatar.Stats;
            oasisAvatar.SuperPowers = avatar.SuperPowers;
            oasisAvatar.GeneKeys = avatar.GeneKeys;
            oasisAvatar.HumanDesign = avatar.HumanDesign;
            oasisAvatar.Gifts = avatar.Gifts;
            oasisAvatar.Chakras = avatar.Chakras;
            oasisAvatar.Aura = avatar.Aura;
            oasisAvatar.Achievements = avatar.Achievements;
            oasisAvatar.Inventory = avatar.Inventory;
            oasisAvatar.Address = avatar.Address;
            oasisAvatar.AvatarType = avatar.AvatarType;
            oasisAvatar.Country = avatar.Country;
            oasisAvatar.County = avatar.County;
            oasisAvatar.Address = avatar.Address;
            oasisAvatar.Country = avatar.Country;
            oasisAvatar.County = avatar.County;
            oasisAvatar.DOB = avatar.DOB;
            oasisAvatar.Landline = avatar.Landline;
            oasisAvatar.Mobile = avatar.Mobile;
            oasisAvatar.Postcode = avatar.Postcode;
            oasisAvatar.Town = avatar.Town;
            oasisAvatar.Karma = avatar.Karma;
            oasisAvatar.KarmaAkashicRecords = avatar.KarmaAkashicRecords;
            oasisAvatar.ParentHolonId = avatar.ParentHolonId;
            oasisAvatar.ParentHolon = avatar.ParentHolon;
            oasisAvatar.ParentZomeId = avatar.ParentZomeId;
            oasisAvatar.ParentZome = avatar.ParentZome;
            oasisAvatar.ParentOmiverse = avatar.ParentOmiverse;
            oasisAvatar.ParentOmiverseId = avatar.ParentOmiverseId;
            oasisAvatar.ParentDimension = avatar.ParentDimension;
            oasisAvatar.ParentDimensionId = avatar.ParentDimensionId;
            oasisAvatar.ParentMultiverse = avatar.ParentMultiverse;
            oasisAvatar.ParentMultiverseId = avatar.ParentMultiverseId;
            oasisAvatar.ParentUniverse = avatar.ParentUniverse;
            oasisAvatar.ParentUniverseId = avatar.ParentUniverseId;
            oasisAvatar.ParentGalaxyCluster = avatar.ParentGalaxyCluster;
            oasisAvatar.ParentGalaxyClusterId = avatar.ParentGalaxyClusterId;
            oasisAvatar.ParentGalaxy = avatar.ParentGalaxy;
            oasisAvatar.ParentGalaxyId = avatar.ParentGalaxyId;
            oasisAvatar.ParentSolarSystem = avatar.ParentSolarSystem;
            oasisAvatar.ParentSolarSystemId = avatar.ParentSolarSystemId;
            oasisAvatar.ParentGreatGrandSuperStar = avatar.ParentGreatGrandSuperStar;
            oasisAvatar.ParentGreatGrandSuperStarId = avatar.ParentGreatGrandSuperStarId;
            oasisAvatar.ParentGreatGrandSuperStar = avatar.ParentGreatGrandSuperStar;
            oasisAvatar.ParentGrandSuperStarId = avatar.ParentGrandSuperStarId;
            oasisAvatar.ParentGrandSuperStar = avatar.ParentGrandSuperStar;
            oasisAvatar.ParentSuperStarId = avatar.ParentSuperStarId;
            oasisAvatar.ParentSuperStar = avatar.ParentSuperStar;
            oasisAvatar.ParentStarId = avatar.ParentStarId;
            oasisAvatar.ParentStar = avatar.ParentStar;
            oasisAvatar.ParentPlanetId = avatar.ParentPlanetId;
            oasisAvatar.ParentPlanet = avatar.ParentPlanet;
            oasisAvatar.ParentMoonId = avatar.ParentMoonId;
            oasisAvatar.ParentMoon = avatar.ParentMoon;
            oasisAvatar.Children = avatar.Children;
            oasisAvatar.Nodes = avatar.Nodes;

            return oasisAvatar;
        }

        private Avatar ConvertOASISAvatarToMongoEntity(IAvatar avatar)
        {
            if (avatar == null)
                return null;

            Avatar mongoAvatar = new Avatar();

            if (avatar.ProviderKey != null && avatar.ProviderKey.ContainsKey(Core.Enums.ProviderType.MongoDBOASIS))
                mongoAvatar.Id = avatar.ProviderKey[Core.Enums.ProviderType.MongoDBOASIS];

            //if (avatar.CreatedProviderType != null)
            //    mongoAvatar.CreatedProviderType = avatar.CreatedProviderType.Value;

            mongoAvatar.HolonId = avatar.Id;
            // mongoAvatar.AvatarId = avatar.Id;
            mongoAvatar.ProviderKey = avatar.ProviderKey;
            mongoAvatar.ProviderMetaData = avatar.ProviderMetaData;
            mongoAvatar.PreviousVersionId = avatar.PreviousVersionId;
            mongoAvatar.PreviousVersionProviderKey = avatar.PreviousVersionProviderKey;
            mongoAvatar.Name = avatar.Name;
            mongoAvatar.Description = avatar.Description;
            mongoAvatar.FirstName = avatar.FirstName;
            mongoAvatar.LastName = avatar.LastName;
            mongoAvatar.Email = avatar.Email;
            mongoAvatar.Password = avatar.Password;
            mongoAvatar.Title = avatar.Title;
            mongoAvatar.Username = avatar.Username;
            mongoAvatar.HolonType = avatar.HolonType;
            mongoAvatar.AvatarType = avatar.AvatarType;
            mongoAvatar.CreatedProviderType = avatar.CreatedProviderType;
            mongoAvatar.CreatedOASISType = avatar.CreatedOASISType;
            mongoAvatar.MetaData = avatar.MetaData;
            mongoAvatar.Image2D = avatar.Image2D;
            mongoAvatar.AcceptTerms = avatar.AcceptTerms;
            mongoAvatar.JwtToken = avatar.JwtToken;
            mongoAvatar.PasswordReset = avatar.PasswordReset;
            mongoAvatar.RefreshToken = avatar.RefreshToken;
            mongoAvatar.RefreshTokens = avatar.RefreshTokens;
            mongoAvatar.ResetToken = avatar.ResetToken;
            mongoAvatar.ResetTokenExpires = avatar.ResetTokenExpires;
            mongoAvatar.VerificationToken = avatar.VerificationToken;
            mongoAvatar.Verified = avatar.Verified;
            mongoAvatar.Karma = avatar.Karma;
            mongoAvatar.XP = avatar.XP;
            mongoAvatar.Image2D = avatar.Image2D;
            mongoAvatar.IsChanged = avatar.IsChanged;
            mongoAvatar.CreatedByAvatarId = avatar.CreatedByAvatarId.ToString();
            mongoAvatar.CreatedDate = avatar.CreatedDate;
            mongoAvatar.DeletedByAvatarId = avatar.DeletedByAvatarId.ToString();
            mongoAvatar.DeletedDate = avatar.DeletedDate;
            mongoAvatar.ModifiedByAvatarId = avatar.ModifiedByAvatarId.ToString();
            mongoAvatar.ModifiedDate = avatar.ModifiedDate;
            mongoAvatar.DeletedDate = avatar.DeletedDate;
            mongoAvatar.LastBeamedIn = avatar.LastBeamedIn;
            mongoAvatar.LastBeamedOut = avatar.LastBeamedOut;
            mongoAvatar.IsBeamedIn = avatar.IsBeamedIn;
            mongoAvatar.Version = avatar.Version;
            mongoAvatar.IsActive = avatar.IsActive;

            return mongoAvatar;
        }

        private AvatarDetail ConvertOASISAvatarDetailToMongoEntity(IAvatarDetail avatar)
        {
            if (avatar == null)
                return null;

            AvatarDetail mongoAvatar = new AvatarDetail();

            if (avatar.ProviderKey != null && avatar.ProviderKey.ContainsKey(Core.Enums.ProviderType.MongoDBOASIS))
                mongoAvatar.Id = avatar.ProviderKey[Core.Enums.ProviderType.MongoDBOASIS];

            // if (avatar.CreatedProviderType != null)
            //     mongoAvatar.CreatedProviderType = avatar.CreatedProviderType.Value;

            //Avatar Properties
            mongoAvatar.HolonId = avatar.Id;
            mongoAvatar.ProviderKey = avatar.ProviderKey;
            mongoAvatar.ProviderMetaData = avatar.ProviderMetaData;
            mongoAvatar.PreviousVersionId = avatar.PreviousVersionId;
            mongoAvatar.PreviousVersionProviderKey = avatar.PreviousVersionProviderKey;
            mongoAvatar.Name = avatar.Name;
            mongoAvatar.Description = avatar.Description;
            mongoAvatar.FirstName = avatar.FirstName;
            mongoAvatar.LastName = avatar.LastName;
            mongoAvatar.Email = avatar.Email;
            mongoAvatar.Title = avatar.Title;
            mongoAvatar.Username = avatar.Username;
            mongoAvatar.HolonType = avatar.HolonType;
            mongoAvatar.AvatarType = avatar.AvatarType;
            mongoAvatar.CreatedProviderType = avatar.CreatedProviderType;
            mongoAvatar.CreatedOASISType = avatar.CreatedOASISType;
            mongoAvatar.MetaData = avatar.MetaData;
            mongoAvatar.Image2D = avatar.Image2D;
            mongoAvatar.Karma = avatar.Karma;
            mongoAvatar.XP = avatar.XP;
            mongoAvatar.Image2D = avatar.Image2D;
            mongoAvatar.IsChanged = avatar.IsChanged;
            mongoAvatar.CreatedByAvatarId = avatar.CreatedByAvatarId.ToString();
            mongoAvatar.CreatedDate = avatar.CreatedDate;
            mongoAvatar.DeletedByAvatarId = avatar.DeletedByAvatarId.ToString();
            mongoAvatar.DeletedDate = avatar.DeletedDate;
            mongoAvatar.ModifiedByAvatarId = avatar.ModifiedByAvatarId.ToString();
            mongoAvatar.ModifiedDate = avatar.ModifiedDate;
            mongoAvatar.DeletedDate = avatar.DeletedDate;
            mongoAvatar.Version = avatar.Version;
            mongoAvatar.IsActive = avatar.IsActive;

            //AvatarDetail Properties
            mongoAvatar.UmaJson = avatar.UmaJson;
            mongoAvatar.ProviderPrivateKey = avatar.ProviderPrivateKey;
            mongoAvatar.ProviderPublicKey = avatar.ProviderPublicKey;
            mongoAvatar.ProviderUsername = avatar.ProviderUsername;
            mongoAvatar.ProviderWalletAddress = avatar.ProviderWalletAddress;
            mongoAvatar.FavouriteColour = avatar.FavouriteColour;
            mongoAvatar.STARCLIColour = avatar.STARCLIColour;
            mongoAvatar.Skills = avatar.Skills;
            mongoAvatar.Spells = avatar.Spells;
            mongoAvatar.Stats = avatar.Stats;
            mongoAvatar.SuperPowers = avatar.SuperPowers;
            mongoAvatar.GeneKeys = avatar.GeneKeys;
            mongoAvatar.HumanDesign = avatar.HumanDesign;
            mongoAvatar.Gifts = avatar.Gifts;
            mongoAvatar.Chakras = avatar.Chakras;
            mongoAvatar.Aura = avatar.Aura;
            mongoAvatar.Achievements = avatar.Achievements;
            mongoAvatar.Inventory = avatar.Inventory;
            mongoAvatar.Address = avatar.Address;
            mongoAvatar.Country = avatar.Country;
            mongoAvatar.County = avatar.County;
            mongoAvatar.Address = avatar.Address;
            mongoAvatar.Country = avatar.Country;
            mongoAvatar.County = avatar.County;
            mongoAvatar.DOB = avatar.DOB;
            mongoAvatar.Landline = avatar.Landline;
            mongoAvatar.Mobile = avatar.Mobile;
            mongoAvatar.Postcode = avatar.Postcode;
            mongoAvatar.Town = avatar.Town;
            mongoAvatar.KarmaAkashicRecords = avatar.KarmaAkashicRecords;
            mongoAvatar.MetaData = avatar.MetaData;
            mongoAvatar.ParentHolonId = avatar.ParentHolonId;
            mongoAvatar.ParentHolon = avatar.ParentHolon;
            mongoAvatar.ParentZomeId = avatar.ParentZomeId;
            mongoAvatar.ParentZome = avatar.ParentZome;
            mongoAvatar.ParentOmiverse = avatar.ParentOmiverse;
            mongoAvatar.ParentOmiverseId = avatar.ParentOmiverseId;
            mongoAvatar.ParentDimension = avatar.ParentDimension;
            mongoAvatar.ParentDimensionId = avatar.ParentDimensionId;
            mongoAvatar.ParentMultiverse = avatar.ParentMultiverse;
            mongoAvatar.ParentMultiverseId = avatar.ParentMultiverseId;
            mongoAvatar.ParentUniverse = avatar.ParentUniverse;
            mongoAvatar.ParentUniverseId = avatar.ParentUniverseId;
            mongoAvatar.ParentGalaxyCluster = avatar.ParentGalaxyCluster;
            mongoAvatar.ParentGalaxyClusterId = avatar.ParentGalaxyClusterId;
            mongoAvatar.ParentGalaxy = avatar.ParentGalaxy;
            mongoAvatar.ParentGalaxyId = avatar.ParentGalaxyId;
            mongoAvatar.ParentSolarSystem = avatar.ParentSolarSystem;
            mongoAvatar.ParentSolarSystemId = avatar.ParentSolarSystemId;
            mongoAvatar.ParentGreatGrandSuperStar = avatar.ParentGreatGrandSuperStar;
            mongoAvatar.ParentGreatGrandSuperStarId = avatar.ParentGreatGrandSuperStarId;
            mongoAvatar.ParentGreatGrandSuperStar = avatar.ParentGreatGrandSuperStar;
            mongoAvatar.ParentGrandSuperStarId = avatar.ParentGrandSuperStarId;
            mongoAvatar.ParentGrandSuperStar = avatar.ParentGrandSuperStar;
            mongoAvatar.ParentSuperStarId = avatar.ParentSuperStarId;
            mongoAvatar.ParentSuperStar = avatar.ParentSuperStar;
            mongoAvatar.ParentStarId = avatar.ParentStarId;
            mongoAvatar.ParentStar = avatar.ParentStar;
            mongoAvatar.ParentPlanetId = avatar.ParentPlanetId;
            mongoAvatar.ParentPlanet = avatar.ParentPlanet;
            mongoAvatar.ParentMoonId = avatar.ParentMoonId;
            mongoAvatar.ParentMoon = avatar.ParentMoon;
            mongoAvatar.Children = avatar.Children;
            mongoAvatar.Nodes = avatar.Nodes;

            return mongoAvatar;
        }

        private OASISResult<IHolon> ConvertMongoEntityToOASISHolon(OASISResult<Holon> holon)
        {
            OASISResult<IHolon> result = new OASISResult<IHolon>(new Core.Holons.Holon());

            if (holon.Result == null || holon.IsError)
            {
                result.IsError = true;
                result.Message = holon.Message;
                return result;
            }

            result.Result.Id = holon.Result.HolonId;
            result.Result.ProviderKey = holon.Result.ProviderKey;
            result.Result.PreviousVersionId = holon.Result.PreviousVersionId;
            result.Result.PreviousVersionProviderKey = holon.Result.PreviousVersionProviderKey;
            result.Result.MetaData = holon.Result.MetaData;
            result.Result.ProviderMetaData = holon.Result.ProviderMetaData;
            result.Result.Name = holon.Result.Name;
            result.Result.Description = holon.Result.Description;
            result.Result.HolonType = holon.Result.HolonType;
            // oasisHolon.CreatedProviderType = new EnumValue<ProviderType>(holon.CreatedProviderType);
            result.Result.CreatedProviderType = holon.Result.CreatedProviderType;
            //oasisHolon.CreatedProviderType.Value = Core.Enums.ProviderType.MongoDBOASIS;
            result.Result.CreatedProviderType = holon.Result.CreatedProviderType;
            result.Result.IsChanged = holon.Result.IsChanged;
            result.Result.ParentHolonId = holon.Result.ParentHolonId;
            result.Result.ParentHolon = holon.Result.ParentHolon;
            result.Result.ParentZomeId = holon.Result.ParentZomeId;
            result.Result.ParentZome = holon.Result.ParentZome;
            result.Result.ParentOmiverse = holon.Result.ParentOmiverse;
            result.Result.ParentOmiverseId = holon.Result.ParentOmiverseId;
            result.Result.ParentDimension = holon.Result.ParentDimension;
            result.Result.ParentDimensionId = holon.Result.ParentDimensionId;
            result.Result.ParentMultiverse = holon.Result.ParentMultiverse;
            result.Result.ParentMultiverseId = holon.Result.ParentMultiverseId;
            result.Result.ParentUniverse = holon.Result.ParentUniverse;
            result.Result.ParentUniverseId = holon.Result.ParentUniverseId;
            result.Result.ParentGalaxyCluster = holon.Result.ParentGalaxyCluster;
            result.Result.ParentGalaxyClusterId = holon.Result.ParentGalaxyClusterId;
            result.Result.ParentGalaxy = holon.Result.ParentGalaxy;
            result.Result.ParentGalaxyId = holon.Result.ParentGalaxyId;
            result.Result.ParentSolarSystem = holon.Result.ParentSolarSystem;
            result.Result.ParentSolarSystemId = holon.Result.ParentSolarSystemId;
            result.Result.ParentGreatGrandSuperStar = holon.Result.ParentGreatGrandSuperStar;
            result.Result.ParentGreatGrandSuperStarId = holon.Result.ParentGreatGrandSuperStarId;
            result.Result.ParentGreatGrandSuperStar = holon.Result.ParentGreatGrandSuperStar;
            result.Result.ParentGrandSuperStarId = holon.Result.ParentGrandSuperStarId;
            result.Result.ParentGrandSuperStar = holon.Result.ParentGrandSuperStar;
            result.Result.ParentSuperStarId = holon.Result.ParentSuperStarId;
            result.Result.ParentSuperStar = holon.Result.ParentSuperStar;
            result.Result.ParentStarId = holon.Result.ParentStarId;
            result.Result.ParentStar = holon.Result.ParentStar;
            result.Result.ParentPlanetId = holon.Result.ParentPlanetId;
            result.Result.ParentPlanet = holon.Result.ParentPlanet;
            result.Result.ParentMoonId = holon.Result.ParentMoonId;
            result.Result.ParentMoon = holon.Result.ParentMoon;
            result.Result.Children = holon.Result.Children;
            result.Result.Nodes = holon.Result.Nodes;
            result.Result.CreatedByAvatarId = Guid.Parse(holon.Result.CreatedByAvatarId);
            result.Result.CreatedDate = holon.Result.CreatedDate;
            result.Result.DeletedByAvatarId = Guid.Parse(holon.Result.DeletedByAvatarId);
            result.Result.DeletedDate = holon.Result.DeletedDate;
            result.Result.ModifiedByAvatarId = Guid.Parse(holon.Result.ModifiedByAvatarId);
            result.Result.ModifiedDate = holon.Result.ModifiedDate;
            result.Result.DeletedDate = holon.Result.DeletedDate;
            result.Result.Version = holon.Result.Version;
            result.Result.IsActive = holon.Result.IsActive;

            return result;
        }

        private Holon ConvertOASISHolonToMongoEntity(IHolon holon)
        {
            if (holon == null)
                return null;

            Holon mongoHolon = new Holon();

           // if (holon.CreatedProviderType != null)
           //     mongoHolon.CreatedProviderType = holon.CreatedProviderType.Value;

            if (holon.ProviderKey != null && holon.ProviderKey.ContainsKey(Core.Enums.ProviderType.MongoDBOASIS))
                mongoHolon.Id = holon.ProviderKey[Core.Enums.ProviderType.MongoDBOASIS];

            mongoHolon.HolonId = holon.Id;
            mongoHolon.ProviderKey = holon.ProviderKey;
            mongoHolon.PreviousVersionId = holon.PreviousVersionId;
            mongoHolon.PreviousVersionProviderKey = holon.PreviousVersionProviderKey;
            mongoHolon.ProviderMetaData = holon.ProviderMetaData;
            mongoHolon.MetaData = holon.MetaData;
            mongoHolon.CreatedProviderType = holon.CreatedProviderType;
            mongoHolon.HolonType = holon.HolonType;
            mongoHolon.Name = holon.Name;
            mongoHolon.Description = holon.Description;
            mongoHolon.IsChanged = holon.IsChanged;
            mongoHolon.ParentHolonId = holon.ParentHolonId;
            mongoHolon.ParentHolon = holon.ParentHolon;
            mongoHolon.ParentZomeId = holon.ParentZomeId;
            mongoHolon.ParentZome = holon.ParentZome;
            mongoHolon.ParentOmiverse = holon.ParentOmiverse;
            mongoHolon.ParentOmiverseId = holon.ParentOmiverseId;
            mongoHolon.ParentDimension = holon.ParentDimension;
            mongoHolon.ParentDimensionId = holon.ParentDimensionId;
            mongoHolon.ParentMultiverse = holon.ParentMultiverse;
            mongoHolon.ParentMultiverseId = holon.ParentMultiverseId;
            mongoHolon.ParentUniverse = holon.ParentUniverse;
            mongoHolon.ParentUniverseId = holon.ParentUniverseId;
            mongoHolon.ParentGalaxyCluster = holon.ParentGalaxyCluster;
            mongoHolon.ParentGalaxyClusterId = holon.ParentGalaxyClusterId;
            mongoHolon.ParentGalaxy = holon.ParentGalaxy;
            mongoHolon.ParentGalaxyId = holon.ParentGalaxyId;
            mongoHolon.ParentSolarSystem = holon.ParentSolarSystem;
            mongoHolon.ParentSolarSystemId = holon.ParentSolarSystemId;
            mongoHolon.ParentGreatGrandSuperStar = holon.ParentGreatGrandSuperStar;
            mongoHolon.ParentGreatGrandSuperStarId = holon.ParentGreatGrandSuperStarId;
            mongoHolon.ParentGreatGrandSuperStar = holon.ParentGreatGrandSuperStar;
            mongoHolon.ParentGrandSuperStarId = holon.ParentGrandSuperStarId;
            mongoHolon.ParentGrandSuperStar = holon.ParentGrandSuperStar;
            mongoHolon.ParentSuperStarId = holon.ParentSuperStarId;
            mongoHolon.ParentSuperStar = holon.ParentSuperStar;
            mongoHolon.ParentStarId = holon.ParentStarId;
            mongoHolon.ParentStar = holon.ParentStar;
            mongoHolon.ParentPlanetId = holon.ParentPlanetId;
            mongoHolon.ParentPlanet = holon.ParentPlanet;
            mongoHolon.ParentMoonId = holon.ParentMoonId;
            mongoHolon.ParentMoon = holon.ParentMoon;
            mongoHolon.Children = holon.Children;
            mongoHolon.Nodes = holon.Nodes;
            mongoHolon.CreatedByAvatarId = holon.CreatedByAvatarId.ToString();
            mongoHolon.CreatedDate = holon.CreatedDate;
            mongoHolon.DeletedByAvatarId = holon.DeletedByAvatarId.ToString();
            mongoHolon.DeletedDate = holon.DeletedDate;
            mongoHolon.ModifiedByAvatarId = holon.ModifiedByAvatarId.ToString();
            mongoHolon.ModifiedDate = holon.ModifiedDate;
            mongoHolon.DeletedDate = holon.DeletedDate;
            mongoHolon.Version = holon.Version;
            mongoHolon.IsActive = holon.IsActive;
            
            return mongoHolon;
        }
    }
}