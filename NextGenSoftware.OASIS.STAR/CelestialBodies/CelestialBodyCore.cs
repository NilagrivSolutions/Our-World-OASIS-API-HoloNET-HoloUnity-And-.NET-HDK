﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mapster;
using NextGenSoftware.OASIS.API.Core.Enums;
using NextGenSoftware.OASIS.API.Core.Events;
using NextGenSoftware.OASIS.API.Core.Helpers;
using NextGenSoftware.OASIS.API.Core.Interfaces;
using NextGenSoftware.OASIS.API.Core.Interfaces.STAR;
using NextGenSoftware.OASIS.STAR.Zomes;

namespace NextGenSoftware.OASIS.STAR.CelestialBodies
{
    public abstract class CelestialBodyCore : ZomeBase, ICelestialBodyCore
    {
        public delegate void HolonsLoaded(object sender, HolonsLoadedEventArgs e);
        //  public event HolonsLoaded OnHolonsLoaded;
        public event Events.HolonsLoaded OnHolonsLoaded;

        public delegate void ZomesLoaded(object sender, ZomesLoadedEventArgs e);
        //public event ZomesLoaded OnZomesLoaded;
        public event Events.ZomesLoaded OnZomesLoaded;

        public List<IZome> Zomes { get; set; } = new List<IZome>();

        public new IEnumerable<IHolon> Holons
        {
            get
            {
                if (Zomes != null)
                {
                    List<IHolon> holons = new List<IHolon>();

                    foreach (IZome zome in Zomes)
                        holons.Add(zome);

                    //Now we need to add the base holons that are linked directly to the celestialbody.
                    holons.AddRange(base.Holons);
                    return holons;
                }

                return null;
            }
        }

        public CelestialBodyCore(Dictionary<ProviderType, string> providerKey) : base()
        {
            this.ProviderKey = providerKey;
        }

        public CelestialBodyCore(Guid id) : base()
        {
            this.Id = id;
        }

        public CelestialBodyCore() : base()
        {
        }

        public async Task<OASISResult<IEnumerable<IZome>>> LoadZomesAsync()
        {
            OASISResult<IEnumerable<IZome>> result = new OASISResult<IEnumerable<IZome>>();
            OASISResult<IEnumerable<IHolon>> holonResult = null; 

            if (Zomes == null)
                Zomes = new List<IZome>();

            if (Id != Guid.Empty)
                holonResult = await base.LoadHolonsForParentAsync(Id);

            else if (ProviderKey != null)
                holonResult = await base.LoadHolonsForParentAsync(ProviderKey);
            else
            {
                result.IsError = true;
                result.Message = "Both Id and ProviderKey are null, one of these need to be set before calling this method.";
            }

            if (holonResult.IsError)
            {
                result.IsError = true;
                result.Message = holonResult.Message;
            }

            if (holonResult.Result != null && !result.IsError)
            {
               // Zomes = SuperStar.Mapper.Map<List<Zome>(holons); //TODO: Use AutoMapper for Collection instead so is faster.
               //TODO: Want to use MapGenerator if possible so it auto-generates the code since this would be faster still.. :)
                foreach (IHolon holon in holonResult.Result)
                    Zomes.Add(SuperStar.Mapper.Map<Zome>(holon));
                    //Zomes.Add(holon.Adapt<Zome>());

                OnZomesLoaded?.Invoke(this, new ZomesLoadedEventArgs { Zomes = Zomes });
                result.Result = Zomes;
            }

            return result;
        }

        public OASISResult<IEnumerable<IZome>> LoadZomes()
        {
            OASISResult<IEnumerable<IZome>> result = new OASISResult<IEnumerable<IZome>>();
            OASISResult<IEnumerable<IHolon>> holonResult = null;

            if (Zomes == null)
                Zomes = new List<IZome>();

            if (Id != Guid.Empty)
                holonResult = base.LoadHolonsForParent(Id); //TODO: handle OASISResult properly.

            else if (ProviderKey != null)
                holonResult = base.LoadHolonsForParent(ProviderKey); //TODO: handle OASISResult properly.
            else
            {
                result.IsError = true;
                result.Message = "Both Id and ProviderKey are null, one of these need to be set before calling this method.";
            }

            if (holonResult.IsError)
            {
                result.IsError = true;
                result.Message = holonResult.Message;
            }

            //TODO: Move this into seperate function so can be shared with async version.
            if (holonResult.Result != null && !result.IsError)
            {
                // Zomes = SuperStar.Mapper.Map<List<Zome>(holons); //TODO: Use AutoMapper for Collection instead so is faster.
                //TODO: Want to use MapGenerator if possible so it auto-generates the code since this would be faster still.. :)
                foreach (IHolon holon in holonResult.Result)
                    Zomes.Add(SuperStar.Mapper.Map<Zome>(holon));
                    //Zomes.Add(holon.Adapt<Zome>());

                OnZomesLoaded?.Invoke(this, new ZomesLoadedEventArgs { Zomes = Zomes });
                result.Result = Zomes;
            }

            return result;
        }

        public async Task<OASISResult<IZome>> AddZomeAsync(IZome zome)
        {
            OASISResult<IZome> result = new OASISResult<IZome>();

            if (zome.Id == Guid.Empty)
                result = await zome.SaveAsync();
 
            if (!result.IsError)
            {
                this.Zomes.Add(zome);
                OASISResult<IEnumerable<IHolon>> holonsResult = await base.SaveHolonsAsync(this.Zomes);

                if (holonsResult.IsError)
                {
                    result.IsError = true;
                    result.Message = holonsResult.Message;
                }
            }

            return result;
        }

        public OASISResult<IZome> AddZome(IZome zome)
        {
            OASISResult<IZome> result = new OASISResult<IZome>();

            if (zome.Id == Guid.Empty)
                result = zome.Save();

            if (!result.IsError)
            {
                this.Zomes.Add(zome);
                OASISResult<IEnumerable<IHolon>> holonsResult = base.SaveHolons(this.Zomes);

                if (holonsResult.IsError)
                {
                    result.IsError = true;
                    result.Message = holonsResult.Message;
                }
            }

            return result;
        }

        public async Task<OASISResult<IEnumerable<IHolon>>> RemoveZomeAsync(IZome zome)
        {
            this.Zomes.Remove(zome);
            return await base.SaveHolonsAsync(this.Zomes);
        }

        public OASISResult<IEnumerable<IHolon>> RemoveZome(IZome zome)
        {
            this.Zomes.Remove(zome);
            return base.SaveHolons(this.Zomes);
        }

        public async Task<OASISResult<IHolon>> SaveCelestialBodyAsync(IHolon savingHolon)
        {
            ICelestialBodyCore core = null;
            IHolon parentHolon = null;
            IStar parentStar = null;
            IPlanet parentPlanet = null;
            IMoon parentMoon = null;
            //ICelestialBodyCore parentHolonCore = null;
            //ICelestialBodyCore parentStarCore = null;
            //ICelestialBodyCore parentPlanetCore = null;
            //ICelestialBodyCore parentMoonCore = null;

            ICelestialBody celestialBody = savingHolon as ICelestialBody;

            if (celestialBody != null)
            {
                core = celestialBody.CelestialBodyCore;
                celestialBody.CelestialBodyCore = null;
            }

            parentHolon = savingHolon.ParentHolon;
            parentStar = savingHolon.ParentStar;
            parentPlanet = savingHolon.ParentPlanet;
            parentMoon = savingHolon.ParentMoon;
            savingHolon.ParentHolon = null;
            savingHolon.ParentStar = null;
            savingHolon.ParentPlanet = null;
            savingHolon.ParentMoon = null;

            //ICelestialBody celestialBody = savingHolon as ICelestialBody;
            /*
            // Temp remove the cores from the celestialBody otherwise we have a infinite recursive issue in the OASIS Providers when saving/serialaizaing, etc.
            if (celestialBody != null)
            {
                core = celestialBody.CelestialBodyCore;
                celestialBody.CelestialBodyCore = null;

                parentHolon = celestialBody.ParentHolon;
                celestialBody.ParentHolon = null;

                
                //ICelestialBody parent = celestialBody.ParentHolon as ICelestialBody;

                //if (parent != null)
                //{
                //    parentHolon = parent;
                //    //parentHolonCore = parent.CelestialBodyCore;
                //}

                if (celestialBody.ParentStar != null)
                {
                    parentStar = celestialBody.ParentStar;
                    //parentStarCore = celestialBody.ParentStar.CelestialBodyCore;
                    //celestialBody.ParentStar.CelestialBodyCore = null;
                    celestialBody.ParentStar = null;
                }

                if (celestialBody.ParentPlanet != null)
                {
                    parentPlanetCore = celestialBody.ParentPlanet.CelestialBodyCore;
                    celestialBody.ParentPlanet.CelestialBodyCore = null;
                }

                if (celestialBody.ParentMoon != null)
                {
                    parentMoonCore = celestialBody.ParentMoon.CelestialBodyCore;
                    celestialBody.ParentMoon.CelestialBodyCore = null;
                }
            }*/

            // RemoveCores(savingHolon);
            OASISResult<IHolon> result = await base.SaveHolonAsync(savingHolon);

            // Restore the core.
            if (result.Result != null && core != null)
            {
                celestialBody = savingHolon as ICelestialBody;

                //Restore the cores.
                if (celestialBody != null)
                    celestialBody.CelestialBodyCore = core;

                celestialBody.ParentHolon = parentHolon;
                celestialBody.ParentStar = parentStar;
                celestialBody.ParentPlanet = parentPlanet;
                celestialBody.ParentMoon = parentMoon;

                /*
                ICelestialBody parent = celestialBody.ParentHolon as ICelestialBody;

                if (parent != null)
                    parent = parentHolon;
                    //parent.CelestialBodyCore = parentHolonCore;
                */
                /*
                if (celestialBody.ParentStar != null)
                {
                    celestialBody.ParentStar = parentStar;
                    //celestialBody.ParentStar.CelestialBodyCore = parentStarCore;
                }

                if (celestialBody.ParentPlanet != null)
                    celestialBody.ParentPlanet.CelestialBodyCore = parentPlanetCore;

                if (celestialBody.ParentMoon != null)
                    celestialBody.ParentMoon.CelestialBodyCore = parentMoonCore;*/
            }

            return result;
        }

        public OASISResult<IHolon> SaveCelestialBody(IHolon savingHolon)
        {
            //TODO: Not sure if this is a good way of doing this?
            return SaveCelestialBodyAsync(savingHolon).Result;
        }

        public async Task<OASISResult<ICelestialBody>> LoadCelestialBodyAsync()
        {
            OASISResult<ICelestialBody> result = new OASISResult<ICelestialBody>();
            OASISResult<IHolon> holonResult = new OASISResult<IHolon>();

            if (Id != Guid.Empty)
                holonResult = await base.LoadHolonAsync(Id);

            else if (ProviderKey != null)
                holonResult = await base.LoadHolonAsync(ProviderKey);

            else
            {
                result.IsError = true;
                result.Message = "Both Id and ProviderKey are null, one of these need to be set before calling this method.";
            }

            if (!result.IsError)
            {
                OASISResultHelper<IHolon, ICelestialBody>.CopyResult(holonResult, ref result);
                result.Result = (ICelestialBody)holonResult.Result; //TODO: Not sure if this cast will work? Probably not... Need to map...
            }

            return result;
        }

        public OASISResult<ICelestialBody> LoadCelestialBody()
        {
            OASISResult<ICelestialBody> result = new OASISResult<ICelestialBody>();
            OASISResult<IHolon> holonResult = new OASISResult<IHolon>();

            if (Id != Guid.Empty)
                holonResult = base.LoadHolon(Id);

            else if (ProviderKey != null)
                holonResult = base.LoadHolon(ProviderKey);

            else
            {
                result.IsError = true;
                result.Message = "Both Id and ProviderKey are null, one of these need to be set before calling this method.";
            }

            if (!result.IsError)
            {
                OASISResultHelper<IHolon, ICelestialBody>.CopyResult(holonResult, ref result);
                result.Result = (ICelestialBody)holonResult.Result; //TODO: Not sure if this cast will work? Probably not... Need to map...
            }

            return result;
        }

        /*
        private void RemoveCores(IHolon holon)
        {
            // Temp remove the core from the celestialBody otherwise we have a infinite recursive issue in the OASIS Providers when saving/serialaizaing, etc.
            ICelestialBodyCore core = null;
            ICelestialBodyCore parentHolonCore = null;
            ICelestialBodyCore parentStarCore = null;
            ICelestialBodyCore parentPlanetCore = null;
            ICelestialBodyCore parentMoonCore = null;
            ICelestialBody celestialBody = holon as ICelestialBody;

            if (celestialBody != null)
            {
                core = celestialBody.CelestialBodyCore;
                celestialBody.CelestialBodyCore = null;

                foreach (Zome zome in celestialBody.CelestialBodyCore.Zomes)
                    RemoveCores(zome);

                //ICelestialBody parent = celestialBody.ParentHolon as ICelestialBody;

                //if (parent != null)
                //    parentHolonCore = parent.CelestialBodyCore;

                //parentStarCore = celestialBody.ParentStar.CelestialBodyCore;
                //parentPlanetCore = celestialBody.ParentPlanet.CelestialBodyCore;
                //parentMoonCore = celestialBody.ParentMoon.CelestialBodyCore;

                RemoveCores(celestialBody.ParentHolon);
                RemoveCores(celestialBody.ParentStar);
                RemoveCores(celestialBody.ParentPlanet);
                RemoveCores(celestialBody.ParentMoon);
            }
        }

        private void RestoreCores(IHolon holon)
        {
            // Temp remove the core from the celestialBody otherwise we have a infinite recursive issue in the OASIS Providers when saving/serialaizaing, etc.
            ICelestialBodyCore core = null;
            ICelestialBodyCore parentHolonCore = null;
            ICelestialBodyCore parentStarCore = null;
            ICelestialBodyCore parentPlanetCore = null;
            ICelestialBodyCore parentMoonCore = null;
            ICelestialBody celestialBody = holon as ICelestialBody;

            if (celestialBody != null)
            {
                switch (celestialBody.HolonType)
                {
                    case HolonType.Star:
                        celestialBody.CelestialBodyCore = new StarCore(celestialBody.Id, (IStar)celestialBody);
                        break;

                    case HolonType.Planet:
                        celestialBody.CelestialBodyCore = new PlanetCore(celestialBody.Id, (IPlanet)celestialBody);
                        break;

                    case HolonType.Moon:
                        celestialBody.CelestialBodyCore = new MoonCore(celestialBody.Id, (IMoon)celestialBody);
                        break;
                }

                foreach (Zome zome in celestialBody.CelestialBodyCore.Zomes)
                    RemoveCores(zome);

                ICelestialBody parent = celestialBody.ParentHolon as ICelestialBody;

                if (parent != null)
                    parentHolonCore = parent.CelestialBodyCore;

                parentStarCore = celestialBody.ParentStar.CelestialBodyCore;
                parentPlanetCore = celestialBody.ParentPlanet.CelestialBodyCore;
                parentMoonCore = celestialBody.ParentMoon.CelestialBodyCore;

                RestoreCores(celestialBody.ParentHolon);
                RestoreCores(celestialBody.ParentStar);
                RestoreCores(celestialBody.ParentPlanet);
                RestoreCores(celestialBody.ParentMoon);
            }
        }*/
    }
}
