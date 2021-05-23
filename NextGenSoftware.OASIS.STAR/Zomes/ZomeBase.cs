﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NextGenSoftware.OASIS.API.Core.Enums;
using NextGenSoftware.OASIS.API.Core.Events;
using NextGenSoftware.OASIS.API.Core.Helpers;
using NextGenSoftware.OASIS.API.Core.Holons;
using NextGenSoftware.OASIS.API.Core.Interfaces;
using NextGenSoftware.OASIS.API.Core.Interfaces.STAR;
using NextGenSoftware.OASIS.API.Core.Managers;

namespace NextGenSoftware.OASIS.STAR.Zomes
{
    public abstract class ZomeBase : Holon, IZomeBase
    {
        private HolonManager _holonManager = null;
        public List<Holon> _holons = new List<Holon>();

        public List<Holon> Holons
        {
            get
            {
                return _holons;
            }
            set
            {
                _holons = value;
            }
        }

        //public delegate void Events.HolonSaved(object sender, HolonSavedEventArgs e);
        public event Events.HolonSaved OnHolonSaved;

       // public delegate void Events.HolonLoaded(object sender, HolonLoadedEventArgs e);
        public event Events.HolonLoaded OnHolonLoaded;

       // public delegate void HolonsLoaded(object sender, HolonsLoadedEventArgs e);
        public event Events.HolonsLoaded OnHolonsLoaded;

       // public delegate void Initialized(object sender, System.EventArgs e);
        public event Events.Initialized OnInitialized;

       // public delegate void ZomeError(object sender, ZomeErrorEventArgs e);
        public event Events.ZomeError OnZomeError;

        ////TODO: Not sure if we want to expose the HoloNETClient events at this level? They can subscribe to them through the HoloNETClient property below...
        //public delegate void Disconnected(object sender, DisconnectedEventArgs e);
        //public event Disconnected OnDisconnected;

        //public delegate void DataReceived(object sender, DataReceivedEventArgs e);
        //public event DataReceived OnDataReceived;

        public ZomeBase()
        {
            OASISResult<IOASISStorage> result = OASISBootLoader.OASISBootLoader.GetAndActivateDefaultProvider();

            //TODO: Eventually want to replace all exceptions with OASISResult throughout the OASIS because then it makes sure errors are handled properly and friendly messages are shown (plus less overhead of throwing an entire stack trace!)
            if (result.IsError)
                ErrorHandling.HandleError(ref result, string.Concat("Error calling OASISDNAManager.GetAndActivateDefaultProvider(). Error details: ", result.Message), true, false, true);

            _holonManager = new HolonManager(result.Result);
        }

        public virtual async Task<IHolon> LoadHolonAsync(Guid id)
        {
            return await _holonManager.LoadHolonAsync(id);
        }

        public virtual IHolon LoadHolon(Guid id)
        {
            return _holonManager.LoadHolon(id);
        }

        public virtual async Task<IHolon> LoadHolonAsync(Dictionary<ProviderType, string> providerKey)
        {
            return await _holonManager.LoadHolonAsync(GetCurrentProviderKey(providerKey));
        }

        public virtual async Task<IEnumerable<IHolon>> LoadHolonsAsync(Guid id, HolonType type = HolonType.All)
        {
            return await _holonManager.LoadHolonsForParentAsync(id, type);
        }
        public virtual IEnumerable<IHolon> LoadHolons(Guid id, HolonType type = HolonType.All)
        {
            return _holonManager.LoadHolonsForParent(id, type);
        }

        public virtual async Task<IEnumerable<IHolon>> LoadHolonsAsync(Dictionary<ProviderType, string> providerKey, HolonType type = HolonType.All)
        {
            return await _holonManager.LoadHolonsForParentAsync(GetCurrentProviderKey(providerKey), type);
        }
        public virtual IEnumerable<IHolon> LoadHolons(Dictionary<ProviderType, string> providerKey, HolonType type = HolonType.All)
        {
            return _holonManager.LoadHolonsForParent(GetCurrentProviderKey(providerKey), type);
        }

        public virtual async Task<OASISResult<IHolon>> SaveHolonAsync(IHolon savingHolon)
        {
            return await _holonManager.SaveHolonAsync(savingHolon);
        }

        public virtual async Task<OASISResult<IEnumerable<IHolon>>> SaveHolonsAsync(IEnumerable<IHolon> savingHolons)
        {
            return await _holonManager.SaveHolonsAsync(savingHolons);
        }

        public virtual async Task<OASISResult<IZome>> Save()
        {
            OASISResult<IZome> zomeResult = new OASISResult<IZome>((IZome)this);

            //First save the zome.
            OASISResult<IHolon> holonResult = await _holonManager.SaveHolonAsync(this);

            if (!zomeResult.IsError)
            {
                this.Id = holonResult.Result.Id;
                this.ProviderKey = holonResult.Result.ProviderKey;
                this.CreatedByAvatar = holonResult.Result.CreatedByAvatar;
                this.CreatedByAvatarId = holonResult.Result.CreatedByAvatarId;
                this.CreatedDate = holonResult.Result.CreatedDate;
                this.ModifiedByAvatar = holonResult.Result.ModifiedByAvatar;
                this.ModifiedByAvatarId = holonResult.Result.ModifiedByAvatarId;
                this.ModifiedDate = holonResult.Result.ModifiedDate;
                this.Children = holonResult.Result.Children;

                ZomeHelper.SetParentIdsForZome(this.ParentStar, this.ParentPlanet, this.ParentMoon, (IZome)this);

                // Now save the zome child holons (each OASIS Provider will recursively save each child holon, could do the recursion here and just save each holon indivudally with SaveHolonAsync but this way each OASIS Provider can optimise the the way it saves (batches, etc), which would be quicker than making multiple calls...)
                OASISResult<IEnumerable<IHolon>> holonsResult = await _holonManager.SaveHolonsAsync(this.Holons);

                if (holonsResult.IsError)
                {
                    zomeResult.IsError = true;
                    zomeResult.Message = holonsResult.Message;
                }
                else
                {
                    this.Holons = (List<Holon>)holonsResult.Result; // Update the holons collection now the holons will have their id's set.

                    // Now we need to save the zome again so its child holons have their ids set.
                    // TODO: We may not need to do this save again in future since when we load the zome we could lazy load its child holons seperatley from their parentZomeIds.
                    // But loading the zome with all its child holons will be faster than loading them seperatley (but only if the current OASIS Provider supports this, so far MongoDBOASIS does).
                    holonResult = await _holonManager.SaveHolonAsync(this);

                    if (holonsResult.IsError)
                    {
                        zomeResult.IsError = true;
                        zomeResult.Message = holonsResult.Message;
                    }
                }
            }
            else
            {
                zomeResult.IsError = true;
                zomeResult.Message = holonResult.Message;
            }

            return zomeResult;
        }

        public async Task<OASISResult<IEnumerable<IHolon>>> AddHolon(IHolon holon)
        {
            //return await base.SaveHolonAsync(string.Concat(this.Name, HOLONS_ADD), zome);
            this.Holons.Add((Holon)holon);
            return await SaveHolonsAsync(this.Holons);
        }

        public async Task<OASISResult<IEnumerable<IHolon>>> RemoveHolon(IHolon holon)
        {
            //return await base.SaveHolonAsync(string.Concat(this.Name, HOLONS_REMOVE), zome);
            this.Holons.Remove((Holon)holon);
            return await SaveHolonsAsync(this.Holons);
        }

        private string GetCurrentProviderKey(Dictionary<ProviderType, string> providerKey)
        {
            if (ProviderKey.ContainsKey(ProviderManager.CurrentStorageProviderType.Value) && !string.IsNullOrEmpty(ProviderKey[ProviderManager.CurrentStorageProviderType.Value]))
                return providerKey[ProviderManager.CurrentStorageProviderType.Value];
            else
                throw new Exception(string.Concat("ProviderKey not found for CurrentStorageProviderType ", ProviderManager.CurrentStorageProviderType.Name));

            //TODO: Return OASISResult instead of throwing exceptions for ALL OASIS methods!
        }
    }
}
