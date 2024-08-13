﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.Json;
using NextGenSoftware.OASIS.API.DNA;
using NextGenSoftware.OASIS.API.Core.Helpers;
using NextGenSoftware.OASIS.API.Core.Managers;
using NextGenSoftware.OASIS.API.Core.Enums;
using NextGenSoftware.OASIS.API.Core.Holons;
using NextGenSoftware.OASIS.API.Core.Interfaces;
using NextGenSoftware.OASIS.API.Core.Interfaces.NFT;
using NextGenSoftware.OASIS.API.Core.Interfaces.NFT.GeoSpatialNFT;
using NextGenSoftware.OASIS.API.Core.Interfaces.NFT.GeoSpatialNFT.Request;
using NextGenSoftware.OASIS.API.Core.Interfaces.NFT.Request;
using NextGenSoftware.OASIS.API.Core.Interfaces.NFT.Response;
using NextGenSoftware.OASIS.API.Core.Objects.NFT.Request;
using NextGenSoftware.OASIS.API.Core.Objects.Wallets.Response;
using NextGenSoftware.OASIS.API.ONode.Core.Objects;
using NextGenSoftware.OASIS.API.ONode.Core.Interfaces.Managers;
using Nethereum.Contracts.Standards.ERC721;
using NextGenSoftware.OASIS.Common;
using NextGenSoftware.OASIS.API.Core.Objects.NFT;
using System.Linq;
using NextGenSoftware.Utilities;

namespace NextGenSoftware.OASIS.API.ONode.Core.Managers
{
    public class NFTManager : OASISManager, INFTManager
    {
        private static NFTManager _instance = null;

       // public Guid AvatarId { get; set; }

        //public static NFTManager Instance
        //{
        //    get
        //    {
        //        if (_instance == null)
        //            _instance = new NFTManager(ProviderManager.Instance.CurrentStorageProvider, AvatarManager.LoggedInAvatar.Id); //TODO: Need to remove LoggedInAvatar ASAP! Not sure how to pass the avatarId to the instance prop?

        //        return _instance;
        //    }
        //}

        public NFTManager(IOASISStorageProvider OASISStorageProvider, Guid avatarId, OASISDNA OASISDNA = null) : base(OASISStorageProvider, avatarId, OASISDNA)
        {

        }

        public NFTManager(Guid avatarId, OASISDNA OASISDNA = null) : base(avatarId, OASISDNA)
        {

        }

        //TODO: This method may become obsolete if ProviderType changes to NFTProviderType on INFTWalletTransaction
        //public async Task<OASISResult<INFTTransactionRespone>> SendNFTAsync(INFTWalletTransactionRequest request)
        //{
        //    return await SendNFTAsync(new NFTWalletTransactionRequest()
        //    {
        //        Amount = request.Amount,
        //        //Date = DateTime.Now,
        //        FromWalletAddress = request.FromWalletAddress,
        //        MemoText = request.MemoText,
        //        MintWalletAddress = request.MintWalletAddress,
        //        ToWalletAddress = request.ToWalletAddress,
        //        //Token = request.Token,
        //        FromProviderType = GetProviderTypeFromNFTProviderType(request.NFTProviderType)
        //    });
        //}

        ////TODO: This method may become obsolete if ProviderType changes to NFTProviderType on INFTWalletTransaction
        //public OASISResult<INFTTransactionRespone> SendNFT(INFTWalletTransactionRequest request)
        //{
        //    return SendNFT(new NFTWalletTransactionRequest()
        //    {
        //        Amount = request.Amount,
        //        //Date = DateTime.Now,
        //        FromWalletAddress = request.FromWalletAddress,
        //        MemoText = request.MemoText,
        //        MintWalletAddress = request.MintWalletAddress,
        //        ToWalletAddress = request.ToWalletAddress,
        //        //Token = request.Token,
        //        FromProviderType = GetProviderTypeFromNFTProviderType(request.NFTProviderType)
        //    });
        //}

        public async Task<OASISResult<INFTTransactionRespone>> SendNFTAsync(INFTWalletTransactionRequest request)
        {
            OASISResult<INFTTransactionRespone> result = new OASISResult<INFTTransactionRespone>();
            string errorMessage = "Error occured in SendNFTAsync in NFTManager. Reason:";

            //if (request.Date == DateTime.MinValue)
            //    request.Date = DateTime.Now;

            try
            {
                OASISResult<IOASISNFTProvider> nftProviderResult = GetNFTProvider(request.FromProviderType, errorMessage);

                if (nftProviderResult != null && nftProviderResult.Result != null && !nftProviderResult.IsError)
                    result = await nftProviderResult.Result.SendNFTAsync(request);
                else
                {
                    result.Message = nftProviderResult.Message;
                    result.IsError = true;
                }
            }
            catch (Exception e)
            {
                OASISErrorHandling.HandleError(ref result, $"{errorMessage} Unknown error occured: {e.Message}", e);
            }

            return result;
        }

        public OASISResult<INFTTransactionRespone> SendNFT(INFTWalletTransactionRequest request)
        {
            OASISResult<INFTTransactionRespone> result = new OASISResult<INFTTransactionRespone>();
            string errorMessage = "Error occured in SendNFT in NFTManager. Reason:";

            //if (request.Date == DateTime.MinValue)
            //    request.Date = DateTime.Now;

            try
            {
                OASISResult<IOASISNFTProvider> nftProviderResult = GetNFTProvider(request.FromProviderType, errorMessage);

                if (nftProviderResult != null && nftProviderResult.Result != null && !nftProviderResult.IsError)
                    result = nftProviderResult.Result.SendNFT(request);
                else
                {
                    result.Message = nftProviderResult.Message;
                    result.IsError = true;
                }
            }
            catch (Exception e)
            {
                OASISErrorHandling.HandleError(ref result, $"{errorMessage} Unknown error occured: {e.Message}", e);
            }

            return result;
        }


        public async Task<OASISResult<INFTTransactionRespone>> MintNftAsync(IMintNFTTransactionRequest request, bool isGeoNFT = false)
        {
            OASISResult<INFTTransactionRespone> result = new OASISResult<INFTTransactionRespone>();
            string errorMessage = "Error occured in MintNftAsync in NFTManager. Reason:";
            IAvatar currentAvatar = null;

            try
            {
                if (string.IsNullOrEmpty(request.MintWalletAddress) && request.MintedByAvatarId == Guid.Empty)
                {
                    OASISErrorHandling.HandleError(ref result, $"{errorMessage} Both MintWalletAddress and MintedByAvatarId are empty, please specify at least one, thank you!");
                    return result;
                }

                //If the wallet Address hasn't been set then set it now by looking up the relevant wallet address for this avatar and provider type.
                if (string.IsNullOrEmpty(request.MintWalletAddress) && request.MintedByAvatarId != Guid.Empty)
                {
                    if (currentAvatar == null)
                    {
                        OASISResult<IAvatar> avatarResult = await AvatarManager.Instance.LoadAvatarAsync(request.MintedByAvatarId);

                        if (avatarResult != null && !avatarResult.IsError && avatarResult.Result != null)
                        {
                            currentAvatar = avatarResult.Result;

                            foreach (ProviderType providerType in currentAvatar.ProviderWallets.Keys)
                            {
                                if (providerType == request.OnChainProvider.Value)
                                {
                                    request.MintWalletAddress = currentAvatar.ProviderWallets[request.OnChainProvider.Value][0].WalletAddress;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            OASISErrorHandling.HandleError(ref result, $"{errorMessage} An error occured attempting to load the avatar details for the MintedByAvatarId {request.MintedByAvatarId}. Reason: {avatarResult.Message}");
                            return result;
                        }
                    }
                }

                OASISResult<IOASISNFTProvider> nftProviderResult = GetNFTProvider(request.OnChainProvider.Value, errorMessage);

                if (nftProviderResult != null && nftProviderResult.Result != null && !nftProviderResult.IsError)
                {
                    string geoNFTMemoText = "";

                    if (isGeoNFT)
                        geoNFTMemoText = "Geo";

                    request.MemoText = $"{request.OnChainProvider.Name} {geoNFTMemoText}NFT minted on The OASIS with title '{request.Title}' by avatar with id {request.MintedByAvatarId} for the price of {request.Price}. {request.MemoText}";
                    result = await nftProviderResult.Result.MintNFTAsync(request);
                    ProviderType NFTMetaDataProviderType = ProviderType.None;
                    string NFTMetaDataProviderTypeType = "";

                    if (request.StoreNFTMetaDataOnChain)
                    {
                        NFTMetaDataProviderType = request.OnChainProvider.Value;
                        NFTMetaDataProviderTypeType = "OnChainProvider";
                    }
                    else
                    {
                        NFTMetaDataProviderType = request.OffChainProvider.Value;
                        NFTMetaDataProviderTypeType = "OffChainProvider";
                    }

                    //TODO: TEMP! Remove once MintNFTAsync above is working (with Solaba for example!)
                    result.Result = new NFTTransactionRespone(); //TODO: COMMENT!
                    result.Result.TransactionResult = "19GSYRYNoWTcKeqHsoBVjiDXCvtWpbj7wUE6zKnFhmpot1aL3CatMyLuyfvcRzhQvScRMCfpAWsQtWhgHsGC7pf"; //TODO:TEMP!
                    result.IsError = false;

                    //TODO UNCOMMENT! if (result != null && !result.IsError && result.Result != null)
                    if (result != null) //Temp!
                    {
                        result.Result.OASISNFT = CreateOASISNFT(request, result.Result);
                        OASISResult<IHolon> saveHolonResult = await Data.SaveHolonAsync(CreateNFTMetaDataHolon(result.Result.OASISNFT, request), request.MintedByAvatarId, true, true, 0, true, false, NFTMetaDataProviderType);

                        if ((saveHolonResult != null && (saveHolonResult.IsError || saveHolonResult.Result == null)) || saveHolonResult == null)
                        {
                            result.Result = null;
                            OASISErrorHandling.HandleError(ref result, $"{errorMessage} Error occured saving metadata holon to the {NFTMetaDataProviderTypeType} {Enum.GetName(typeof(ProviderType), NFTMetaDataProviderType)}. Reason: {saveHolonResult.Message}");
                        }
                        else
                            result.Message = $"Successfully minted the NFT on the {request.OnChainProvider.Name} provider with hash {result.Result.TransactionResult} and title '{request.Title}' by AvatarId {request.MintedByAvatarId} for price {request.Price}. The OASIS metadata is stored on the {Enum.GetName(typeof(ProviderType), NFTMetaDataProviderType)} provider with the id {result.Result.OASISNFT.Id}.";
                    }
                    else
                        OASISErrorHandling.HandleError(ref result, $"{errorMessage} An eror occured minting the OASISNFT: Reason: {result.Message}");
                }
                else
                {
                    result.Result = null;
                    result.Message = nftProviderResult.Message;
                    result.IsError = true;
                }
            }
            catch (Exception e)
            {
                OASISErrorHandling.HandleError(ref result, $"{errorMessage} Unknown error occured: {e.Message}", e);
            }

            return result;
        }

        public OASISResult<INFTTransactionRespone> MintNft(IMintNFTTransactionRequest request, bool isGeoNFT = false)
        {
            OASISResult<INFTTransactionRespone> result = new OASISResult<INFTTransactionRespone>();
            string errorMessage = "Error occured in MintNft in NFTManager. Reason:";
            IAvatar currentAvatar = null;

            try
            {
                if (string.IsNullOrEmpty(request.MintWalletAddress) && request.MintedByAvatarId == Guid.Empty)
                {
                    OASISErrorHandling.HandleError(ref result, $"{errorMessage} Both MintWalletAddress and MintedByAvatarId are empty, please specify at least one, thank you!");
                    return result;
                }

                //If the wallet Address hasn't been set then set it now by looking up the relevant wallet address for this avatar and provider type.
                if (string.IsNullOrEmpty(request.MintWalletAddress) && request.MintedByAvatarId != Guid.Empty)
                {
                    if (currentAvatar == null)
                    {
                        OASISResult<IAvatar> avatarResult = AvatarManager.Instance.LoadAvatar(request.MintedByAvatarId);

                        if (avatarResult != null && !avatarResult.IsError && avatarResult.Result != null)
                        {
                            currentAvatar = avatarResult.Result;

                            foreach (ProviderType providerType in currentAvatar.ProviderWallets.Keys)
                            {
                                if (providerType == request.OnChainProvider.Value)
                                {
                                    request.MintWalletAddress = currentAvatar.ProviderWallets[request.OnChainProvider.Value][0].WalletAddress;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            OASISErrorHandling.HandleError(ref result, $"{errorMessage} An error occured attempting to load the avatar details for the MintedByAvatarId {request.MintedByAvatarId}. Reason: {avatarResult.Message}");
                            return result;
                        }
                    }
                }

                OASISResult<IOASISNFTProvider> nftProviderResult = GetNFTProvider(request.OnChainProvider.Value, errorMessage);

                if (nftProviderResult != null && nftProviderResult.Result != null && !nftProviderResult.IsError)
                {
                    string geoNFTMemoText = "";

                    if (isGeoNFT)
                        geoNFTMemoText = "Geo";

                    request.MemoText = $"{request.OnChainProvider.Name} {geoNFTMemoText}NFT minted on The OASIS with title '{request.Title}' by avatar with id {request.MintedByAvatarId} for the price of {request.Price}. {request.MemoText}";

                    result = nftProviderResult.Result.MintNFT(request);
                    ProviderType NFTMetaDataProviderType = ProviderType.None;
                    string NFTMetaDataProviderTypeType = "";

                    if (request.StoreNFTMetaDataOnChain)
                    {
                        NFTMetaDataProviderType = request.OnChainProvider.Value;
                        NFTMetaDataProviderTypeType = "OnChainProvider";
                    }
                    else
                    {
                        NFTMetaDataProviderType = request.OffChainProvider.Value;
                        NFTMetaDataProviderTypeType = "OffChainProvider";
                    }

                    //TODO: TEMP! Remove once MintNFTAsync above is working (with Solaba for example!)
                    result.Result = new NFTTransactionRespone(); //TODO: COMMENT!
                    result.Result.TransactionResult = "19GSYRYNoWTcKeqHsoBVjiDXCvtWpbj7wUE6zKnFhmpot1aL3CatMyLuyfvcRzhQvScRMCfpAWsQtWhgHsGC7pf"; //TODO:TEMP!

                    //TODO UNCOMMENT! if (result != null && !result.IsError && result.Result != null)
                    if (result != null) //Temp!
                    {
                        result.Result.OASISNFT = CreateOASISNFT(request, result.Result);
                        OASISResult<IHolon> saveHolonResult = Data.SaveHolon(CreateNFTMetaDataHolon(result.Result.OASISNFT, request), request.MintedByAvatarId, true, true, 0, true, false, NFTMetaDataProviderType);

                        if ((saveHolonResult != null && (saveHolonResult.IsError || saveHolonResult.Result == null)) || saveHolonResult == null)
                        {
                            result.Result = null;
                            OASISErrorHandling.HandleError(ref result, $"{errorMessage} Error occured saving metadata holon to the {NFTMetaDataProviderTypeType} {Enum.GetName(typeof(ProviderType), NFTMetaDataProviderType)}. Reason: {saveHolonResult.Message}");
                        }
                        else
                            result.Message = $"Successfully minted the NFT on the {request.OnChainProvider.Name} provider with hash {result.Result.TransactionResult} and title '{request.Title}' by AvatarId {request.MintedByAvatarId} for price {request.Price}. The OASIS metadata is stored on the {Enum.GetName(typeof(ProviderType), NFTMetaDataProviderType)} provider with the id {result.Result.OASISNFT.Id}.";
                    }
                    else
                        OASISErrorHandling.HandleError(ref result, $"{errorMessage} An eror occured minting the OASISNFT: Reason: {result.Message}");
                }
                else
                {
                    result.Result = null;
                    result.Message = nftProviderResult.Message;
                    result.IsError = true;
                }
            }
            catch (Exception e)
            {
                OASISErrorHandling.HandleError(ref result, $"{errorMessage} Unknown error occured: {e.Message}", e);
            }

            return result;
        }

        public async Task<OASISResult<IOASISNFT>> LoadNftAsync(Guid id, ProviderType providerType = ProviderType.Default)
        {
            OASISResult<IOASISNFT> result = new OASISResult<IOASISNFT>();
            string errorMessage = "Error occured in LoadNftAsync in NFTManager. Reason:";

            try
            {
                result = DecodeNFTMetaData(await Data.LoadHolonAsync(id, true, true, 0, true, false, HolonType.All, 0, providerType), result, errorMessage);
            }
            catch (Exception e)
            {
                OASISErrorHandling.HandleError(ref result, $"{errorMessage} Unknown error occured: {e.Message}", e);
            }

            return result;
        }

        public OASISResult<IOASISNFT> LoadNft(Guid id, ProviderType providerType = ProviderType.Default)
        {
            OASISResult<IOASISNFT> result = new OASISResult<IOASISNFT>();
            string errorMessage = "Error occured in LoadNft in NFTManager. Reason:";

            try
            {
                result = DecodeNFTMetaData(Data.LoadHolon(id, true, true, 0, true, false, HolonType.All, 0, providerType), result, errorMessage);
            }
            catch (Exception e)
            {
                OASISErrorHandling.HandleError(ref result, $"{errorMessage} Unknown error occured: {e.Message}", e);
            }

            return result;
        }

        public async Task<OASISResult<IOASISNFT>> LoadNftAsync(string onChainNftHash, ProviderType providerType = ProviderType.Default)
        {
            OASISResult<IOASISNFT> result = new OASISResult<IOASISNFT>();
            string errorMessage = "Error occured in LoadNftAsync in NFTManager. Reason:";

            try
            {
                result = DecodeNFTMetaData(await Data.LoadHolonByCustomKeyAsync(onChainNftHash, true, true, 0, true, false, HolonType.All, 0, providerType), result, errorMessage);

                //TODO: It may be more efficient and faster to add a custom/metadata field to IHolonBase that can used to Load holons by? Just means having to add additional LoadHolon methods...
                //OASISResult<ISearchResults> searchResult = await SearchManager.Instance.SearchAsync(new SearchParams()
                //{
                //    SearchGroups = new List<ISearchGroupBase>() 
                //    { 
                //        new SearchTextGroup() 
                //        {
                //            SearchQuery = hash, 
                //            SearchHolons = true,
                //            HolonSearchParams = new SearchHolonParams()
                //            {
                //                 MetaData = true
                //            }
                //        },
                //        new SearchTextGroup()
                //        {
                //            PreviousSearchGroupOperator = SearchParamGroupOperator.And, //This wll currently not work in MongoDBOASIS Provider because it currently only supports OR and not AND...
                //            SearchQuery = "NFT",
                //            SearchHolons = true,
                //            HolonSearchParams = new SearchHolonParams()
                //            {
                //                Name = true
                //            }
                //        }
                //    }
                //});

                //if (searchResult != null && !searchResult.IsError && searchResult.Result != null && searchResult.Result.SearchResultHolons.Count > 0)
                //    result.Result = (IOASISNFT)JsonSerializer.Deserialize(searchResult.Result.SearchResultHolons[0].MetaData["OASISNFT"].ToString(), typeof(IOASISNFT));
                //else
                //    OASISErrorHandling.HandleError(ref result, $"{errorMessage} Error occured loading/searching for the holon metadata. Reason: {searchResult.Message}");
            }
            catch (Exception e)
            {
                OASISErrorHandling.HandleError(ref result, $"{errorMessage} Unknown error occured: {e.Message}", e);
            }

            return result;
        }

        public OASISResult<IOASISNFT> LoadNft(string onChainNftHash, ProviderType providerType = ProviderType.Default)
        {
            OASISResult<IOASISNFT> result = new OASISResult<IOASISNFT>();
            string errorMessage = "Error occured in LoadNft in NFTManager. Reason:";

            try
            {
                result = DecodeNFTMetaData(Data.LoadHolonByCustomKey(onChainNftHash, true, true, 0, true, false, HolonType.All, 0, providerType), result, errorMessage);
            }
            catch (Exception e)
            {
                OASISErrorHandling.HandleError(ref result, $"{errorMessage} Unknown error occured: {e.Message}", e);
            }

            return result;
        }

        public async Task<OASISResult<IOASISGeoSpatialNFT>> LoadGeoNftAsync(Guid id, ProviderType providerType = ProviderType.Default)
        {
            OASISResult<IOASISGeoSpatialNFT> result = new OASISResult<IOASISGeoSpatialNFT>();
            string errorMessage = "Error occured in LoadGeoNftAsync in NFTManager. Reason:";

            try
            {
                result = DecodeGeoNFTMetaData(await Data.LoadHolonAsync(id, true, true, 0, true, false, HolonType.All, 0, providerType), result, errorMessage);
            }
            catch (Exception e)
            {
                OASISErrorHandling.HandleError(ref result, $"{errorMessage} Unknown error occured: {e.Message}", e);
            }

            return result;
        }

        public OASISResult<IOASISGeoSpatialNFT> LoadGeoNft(Guid id, ProviderType providerType = ProviderType.Default)
        {
            OASISResult<IOASISGeoSpatialNFT> result = new OASISResult<IOASISGeoSpatialNFT>();
            string errorMessage = "Error occured in LoadGeoNft in NFTManager. Reason:";

            try
            {
                result = DecodeGeoNFTMetaData(Data.LoadHolon(id, true, true, 0, true, false, HolonType.All, 0, providerType), result, errorMessage);
            }
            catch (Exception e)
            {
                OASISErrorHandling.HandleError(ref result, $"{errorMessage} Unknown error occured: {e.Message}", e);
            }

            return result;
        }

        public async Task<OASISResult<IOASISGeoSpatialNFT>> LoadGeoNftAsync(string onChainNftHash, ProviderType providerType = ProviderType.Default)
        {
            OASISResult<IOASISGeoSpatialNFT> result = new OASISResult<IOASISGeoSpatialNFT>();
            string errorMessage = "Error occured in LoadGeoNftAsync in NFTManager. Reason:";

            try
            {
                result = DecodeGeoNFTMetaData(await Data.LoadHolonByCustomKeyAsync(onChainNftHash, true, true, 0, true, false, HolonType.All, 0, providerType), result, errorMessage);
            }
            catch (Exception e)
            {
                OASISErrorHandling.HandleError(ref result, $"{errorMessage} Unknown error occured: {e.Message}", e);
            }

            return result;
        }

        public OASISResult<IOASISGeoSpatialNFT> LoadGeoNft(string onChainNftHash, ProviderType providerType = ProviderType.Default)
        {
            OASISResult<IOASISGeoSpatialNFT> result = new OASISResult<IOASISGeoSpatialNFT>();
            string errorMessage = "Error occured in LoadGeoNft in NFTManager. Reason:";

            try
            {
                result = DecodeGeoNFTMetaData(Data.LoadHolonByCustomKey(onChainNftHash, true, true, 0, true, false, HolonType.All, 0, providerType), result, errorMessage);
            }
            catch (Exception e)
            {
                OASISErrorHandling.HandleError(ref result, $"{errorMessage} Unknown error occured: {e.Message}", e);
            }

            return result;
        }

        public async Task<OASISResult<IEnumerable<IOASISNFT>>> LoadAllNFTsForAvatarAsync(Guid avatarId, ProviderType providerType = ProviderType.Default)
        {
            OASISResult<IEnumerable<IOASISNFT>> result = new OASISResult<IEnumerable<IOASISNFT>>();
            string errorMessage = "Error occured in LoadAllNFTsForAvatarAsync in NFTManager. Reason:";

            try
            {
                result = DecodeNFTMetaData(await Data.LoadHolonsForParentByMetaDataAsync("NFT.MintedByAvatarId", avatarId.ToString(), HolonType.NFT, true, true, 0, true, false, 0, HolonType.All, 0, providerType), result, errorMessage);

                //TODO: Want to add new LoadHolonsForAvatar methods to HolonManager eventually, which we would use here instead. It would load all Holons that had CreatedByAvatarId = avatarId. But for now we can just set the ParentId on the holons to the AvatarId.
                // OASISResult<IEnumerable<IHolon>> holonsResult = await Data.LoadHolonsForParentAsync(avatarId, HolonType.NFT, true, true, 0, true, 0, providerType); //This line would also work because by default all holons created have their parent set to the avatar that created them in the HolonManger.
            }
            catch (Exception e)
            {
                OASISErrorHandling.HandleError(ref result, $"{errorMessage} Unknown error occured: {e.Message}", e);
            }

            return result;
        }

        public OASISResult<IEnumerable<IOASISNFT>> LoadAllNFTsForAvatar(Guid avatarId, ProviderType providerType = ProviderType.Default)
        {
            OASISResult<IEnumerable<IOASISNFT>> result = new OASISResult<IEnumerable<IOASISNFT>>();
            string errorMessage = "Error occured in LoadAllNFTsForAvatar in NFTManager. Reason:";

            try
            {
                result = DecodeNFTMetaData(Data.LoadHolonsForParentByMetaData("NFT.MintedByAvatarId", avatarId.ToString(), HolonType.NFT, true, true, 0, true, false, 0, HolonType.All, 0, providerType), result, errorMessage);
            }
            catch (Exception e)
            {
                OASISErrorHandling.HandleError(ref result, $"{errorMessage} Unknown error occured: {e.Message}", e);
            }

            return result;
        }

        public async Task<OASISResult<IEnumerable<IOASISNFT>>> LoadAllNFTsForMintAddressAsync(string mintWalletAddress, ProviderType providerType = ProviderType.Default)
        {
            OASISResult<IEnumerable<IOASISNFT>> result = new OASISResult<IEnumerable<IOASISNFT>>();
            string errorMessage = "Error occured in LoadAllNFTsForMintAddressAsync in NFTManager. Reason:";

            try
            {
                result = DecodeNFTMetaData(await Data.LoadHolonsForParentByMetaDataAsync("NFT.MintWalletAddress", mintWalletAddress, HolonType.NFT, true, true, 0, true, false, 0, HolonType.All, 0, providerType), result, errorMessage);

                //TODO: We could possibly add a CustomKey2 property to Holons to load by but not sure how far we go with this? I think eventually we may have 3 custom keys you can load by but for now Search will do... ;-)
                //OASISResult<ISearchResults> searchResult = await SearchManager.Instance.SearchAsync(new SearchParams()
                //{
                //    SearchGroups = new List<ISearchGroupBase>()
                //    {
                //        new SearchTextGroup()
                //        {
                //            SearchQuery = mintWalletAddress,
                //            SearchHolons = true,
                //            HolonSearchParams = new SearchHolonParams()
                //            {
                //                 MetaData = true,
                //                 MetaDataKey = "NFT.MintWalletAddress"
                //            }
                //        },
                //        new SearchTextGroup()
                //        {
                //            PreviousSearchGroupOperator = SearchParamGroupOperator.And, //This wll currently not work in MongoDBOASIS Provider because it currently only supports OR and not AND...
                //            SearchQuery = "NFT",
                //            SearchHolons = true,
                //            HolonSearchParams = new SearchHolonParams()
                //            {
                //                Name = true
                //            }
                //        }
                //    }
                //});

                //if (searchResult != null && !searchResult.IsError && searchResult.Result != null)
                //{
                //    foreach (IHolon holon in searchResult.Result.SearchResultHolons)
                //        result.Result.Add((IOASISNFT)JsonSerializer.Deserialize(holon.MetaData["OASISNFT"].ToString(), typeof(IOASISNFT)));
                //}
                //else
                //    OASISErrorHandling.HandleError(ref result, $"{errorMessage} Error occured loading/searching the holon metadata. Reason: {searchResult.Message}");
            }
            catch (Exception e)
            {
                OASISErrorHandling.HandleError(ref result, $"{errorMessage} Unknown error occured: {e.Message}", e);
            }

            return result;
        }

        public OASISResult<IEnumerable<IOASISNFT>> LoadAllNFTsForMintAddress(string mintWalletAddress, ProviderType providerType = ProviderType.Default)
        {
            OASISResult<IEnumerable<IOASISNFT>> result = new OASISResult<IEnumerable<IOASISNFT>>();
            string errorMessage = "Error occured in LoadAllNFTsForMintAddress in NFTManager. Reason:";

            try
            {
                result = DecodeNFTMetaData(Data.LoadHolonsForParentByMetaData("GEONFT.MintedByAvatarId", mintWalletAddress, HolonType.NFT, true, true, 0, true, false, 0, HolonType.All, 0, providerType), result, errorMessage);
            }
            catch (Exception e)
            {
                OASISErrorHandling.HandleError(ref result, $"{errorMessage} Unknown error occured: {e.Message}", e);
            }

            return result;
        }

        public async Task<OASISResult<IEnumerable<IOASISGeoSpatialNFT>>> LoadAllGeoNFTsForAvatarAsync(Guid avatarId, ProviderType providerType = ProviderType.Default)
        {
            OASISResult<IEnumerable<IOASISGeoSpatialNFT>> result = new OASISResult<IEnumerable<IOASISGeoSpatialNFT>>();
            string errorMessage = "Error occured in LoadAllGeoNFTsForAvatarAsync in NFTManager. Reason:";

            try
            {
                result = DecodeGeoNFTMetaData(await Data.LoadHolonsForParentByMetaDataAsync("GEONFT.PlacedByAvatarId", avatarId.ToString(), HolonType.GEONFT, true, true, 0, true, false, 0, HolonType.All, 0, providerType), result, errorMessage);
            }
            catch (Exception e)
            {
                OASISErrorHandling.HandleError(ref result, $"{errorMessage} Unknown error occured: {e.Message}", e);
            }

            return result;
        }

        public OASISResult<IEnumerable<IOASISGeoSpatialNFT>> LoadAllGeoNFTsForAvatar(Guid avatarId, ProviderType providerType = ProviderType.Default)
        {
            OASISResult<IEnumerable<IOASISGeoSpatialNFT>> result = new OASISResult<IEnumerable<IOASISGeoSpatialNFT>>();
            string errorMessage = "Error occured in LoadAllGeoNFTsForAvatar in NFTManager. Reason:";

            try
            {
                result = DecodeGeoNFTMetaData(Data.LoadHolonsForParentByMetaData("GEONFT.MintedByAvatarId", avatarId.ToString(), HolonType.GEONFT, true, true, 0, true, false, 0, HolonType.All, 0, providerType), result, errorMessage);
            }
            catch (Exception e)
            {
                OASISErrorHandling.HandleError(ref result, $"{errorMessage} Unknown error occured: {e.Message}", e);
            }

            return result;
        }

        public async Task<OASISResult<IEnumerable<IOASISGeoSpatialNFT>>> LoadAllGeoNFTsForMintAddressAsync(string mintWalletAddress, ProviderType providerType = ProviderType.Default)
        {
            OASISResult<IEnumerable<IOASISGeoSpatialNFT>> result = new OASISResult<IEnumerable<IOASISGeoSpatialNFT>>();
            string errorMessage = "Error occured in LoadAllGeoNFTsForMintAddressAsync in NFTManager. Reason:";

            try
            {
                result = DecodeGeoNFTMetaData(await Data.LoadHolonsForParentByMetaDataAsync("GEONFT.OriginalOASISNFT.MintWalletAddress", mintWalletAddress, HolonType.GEONFT, true, true, 0, true, false, 0, HolonType.All, 0, providerType), result, errorMessage);
            }
            catch (Exception e)
            {
                OASISErrorHandling.HandleError(ref result, $"{errorMessage} Unknown error occured: {e.Message}", e);
            }

            return result;
        }

        public OASISResult<IEnumerable<IOASISGeoSpatialNFT>> LoadAllGeoNFTsForMintAddress(string mintWalletAddress, ProviderType providerType = ProviderType.Default)
        {
            OASISResult<IEnumerable<IOASISGeoSpatialNFT>> result = new OASISResult<IEnumerable<IOASISGeoSpatialNFT>>();
            string errorMessage = "Error occured in LoadAllGeoNFTsForMintAddress in NFTManager. Reason:";

            try
            {
                result = DecodeGeoNFTMetaData(Data.LoadHolonsForParentByMetaData("GEONFT.OriginalOASISNFT.MintWalletAddress", mintWalletAddress, HolonType.GEONFT, true, true, 0, true, false, 0, HolonType.All, 0, providerType), result, errorMessage);
            }
            catch (Exception e)
            {
                OASISErrorHandling.HandleError(ref result, $"{errorMessage} Unknown error occured: {e.Message}", e);
            }

            return result;
        }

        public async Task<OASISResult<IEnumerable<IOASISNFT>>> LoadAllNFTsAsync(ProviderType providerType = ProviderType.Default)
        {
            OASISResult<IEnumerable<IOASISNFT>> result = new OASISResult<IEnumerable<IOASISNFT>>();
            string errorMessage = "Error occured in LoadAllNFTsAsync in NFTManager. Reason:";

            try
            {
                result = DecodeNFTMetaData(await Data.LoadAllHolonsAsync(HolonType.NFT, true, true, 0, true, false, HolonType.All, 0, providerType), result, errorMessage);
            }
            catch (Exception e)
            {
                OASISErrorHandling.HandleError(ref result, $"{errorMessage} Unknown error occured: {e.Message}", e);
            }

            return result;
        }

        public OASISResult<IEnumerable<IOASISNFT>> LoadAllNFTs(ProviderType providerType = ProviderType.Default)
        {
            OASISResult<IEnumerable<IOASISNFT>> result = new OASISResult<IEnumerable<IOASISNFT>>();
            string errorMessage = "Error occured in LoadAllNFTs in NFTManager. Reason:";

            try
            {
                result = DecodeNFTMetaData(Data.LoadAllHolons(HolonType.NFT, true, true, 0, true, false, HolonType.All, 0, providerType), result, errorMessage);
            }
            catch (Exception e)
            {
                OASISErrorHandling.HandleError(ref result, $"{errorMessage} Unknown error occured: {e.Message}", e);
            }

            return result;
        }

        public async Task<OASISResult<IEnumerable<IOASISGeoSpatialNFT>>> LoadAllGeoNFTsAsync(ProviderType providerType = ProviderType.Default)
        {
            OASISResult<IEnumerable<IOASISGeoSpatialNFT>> result = new OASISResult<IEnumerable<IOASISGeoSpatialNFT>>();
            string errorMessage = "Error occured in LoadAllGeoNFTsAsync in NFTManager. Reason:";

            try
            {
                result = DecodeGeoNFTMetaData(await Data.LoadAllHolonsAsync(HolonType.GEONFT, true, true, 0, true, false, HolonType.All, 0, providerType), result, errorMessage);
            }
            catch (Exception e)
            {
                OASISErrorHandling.HandleError(ref result, $"{errorMessage} Unknown error occured: {e.Message}", e);
            }

            return result;
        }

        public OASISResult<IEnumerable<IOASISGeoSpatialNFT>> LoadAllGeoNFTs(ProviderType providerType = ProviderType.Default)
        {
            OASISResult<IEnumerable<IOASISGeoSpatialNFT>> result = new OASISResult<IEnumerable<IOASISGeoSpatialNFT>>();
            string errorMessage = "Error occured in LoadAllGeoNFTs in NFTManager. Reason:";

            try
            {
                result = DecodeGeoNFTMetaData(Data.LoadAllHolons(HolonType.GEONFT, true, true, 0, true, false, HolonType.All, 0, providerType), result, errorMessage);
            }
            catch (Exception e)
            {
                OASISErrorHandling.HandleError(ref result, $"{errorMessage} Unknown error occured: {e.Message}", e);
            }

            return result;
        }

        public async Task<OASISResult<IOASISGeoSpatialNFT>> PlaceGeoNFTAsync(IPlaceGeoSpatialNFTRequest request)
        {
            OASISResult<IOASISGeoSpatialNFT> result = new OASISResult<IOASISGeoSpatialNFT>();
            string errorMessage = "Error occured in PlaceGeoNFTAsync in NFTManager. Reason:";

            try
            {
                OASISResult<IOASISNFT> loadNftResult = await LoadNftAsync(request.OriginalOASISNFTId, request.OriginalOASISNFTOffChainProviderType);

                if (loadNftResult != null && !loadNftResult.IsError && loadNftResult.Result != null) 
                {
                    result.Result = CreateGeoSpatialNFT(request, loadNftResult.Result);
                    OASISResult<IHolon> saveHolonResult = Data.SaveHolon(CreateGeoSpatialNFTMetaDataHolon(result.Result), request.PlacedByAvatarId, true, true, 0, true, false, request.ProviderType);

                    if ((saveHolonResult != null && (saveHolonResult.IsError || saveHolonResult.Result == null)) || saveHolonResult == null)
                    {
                        result.Result = null;
                        OASISErrorHandling.HandleError(ref result, $"{errorMessage} Error occured saving metadata holon to the OffChainProvider {Enum.GetName(typeof(ProviderType), request.ProviderType)}. Reason: {saveHolonResult.Message}");
                    }
                    else
                        result.Message = $"Successfully created and placed the new OASIS GeoNFT. The meta data is stored on the offchain provider {Enum.GetName(typeof(ProviderType), request.ProviderType)} with id {result.Result.Id} and was placed by the avatar with id {result.Result.PlacedByAvatarId}. The NFT was originally minted on the {result.Result.OnChainProvider.Name} onchain provider with hash {result.Result.Hash} and title '{result.Result.Title}' by the avatar with id {result.Result.MintedByAvatarId} for the price of {result.Result.Price}. The OASIS metadata for the original NFT is stored on the {result.Result.OffChainProvider.Name} offchain provider with the id {result.Result.OriginalOASISNFTId}.";
                }
                else
                    OASISErrorHandling.HandleError(ref result, $"{errorMessage} Error occured loading original OASIS NFT with id {request.OriginalOASISNFTId}. Reason: {loadNftResult.Message}");
            }
            catch (Exception e)
            {
                OASISErrorHandling.HandleError(ref result, $"{errorMessage} Unknown error occured: {e.Message}", e);
            }

            return result;
        }

        public OASISResult<IOASISGeoSpatialNFT> PlaceGeoNFT(IPlaceGeoSpatialNFTRequest request)
        {
            OASISResult<IOASISGeoSpatialNFT> result = new OASISResult<IOASISGeoSpatialNFT>();
            string errorMessage = "Error occured in PlaceGeoNFT in NFTManager. Reason:";

            try
            {
                OASISResult<IOASISNFT> loadNftResult = LoadNft(request.OriginalOASISNFTId, request.OriginalOASISNFTOffChainProviderType);

                if (loadNftResult != null && !loadNftResult.IsError && loadNftResult.Result != null)
                {
                    result.Result = CreateGeoSpatialNFT(request, loadNftResult.Result);
                    OASISResult<IHolon> saveHolonResult = Data.SaveHolon(CreateGeoSpatialNFTMetaDataHolon(result.Result), request.PlacedByAvatarId, true, true, 0, true, false, request.ProviderType);

                    if ((saveHolonResult != null && (saveHolonResult.IsError || saveHolonResult.Result == null)) || saveHolonResult == null)
                    {
                        result.Result = null;
                        OASISErrorHandling.HandleError(ref result, $"{errorMessage} Error occured saving metadata holon to the OffChainProvider {Enum.GetName(typeof(ProviderType), request.ProviderType)}. Reason: {saveHolonResult.Message}");
                    }
                    else
                        result.Message = $"Successfully created and placed the new OASIS GeoNFT meta data on the offchain provider {Enum.GetName(typeof(ProviderType), request.ProviderType)} with id {result.Result.Id}, which was placed by avatar id {result.Result.PlacedByAvatarId} and was originally minted on the {Enum.GetName(typeof(ProviderType), result.Result.OnChainProvider)} onchain provider with hash {result.Result.Hash} and title '{result.Result.Title}' by AvatarId {result.Result.MintedByAvatarId} for price {result.Result.Price}. The OASIS metadata for the original NFT is stored on the {Enum.GetName(typeof(ProviderType), result.Result.OffChainProvider)} offchain provider with the id {result.Result.OriginalOASISNFTId}.";
                }
                else
                    OASISErrorHandling.HandleError(ref result, $"{errorMessage} Error occured loading original OASIS NFT with id {request.OriginalOASISNFTId}. Reason: {loadNftResult.Message}");
            }
            catch (Exception e)
            {
                OASISErrorHandling.HandleError(ref result, $"{errorMessage} Unknown error occured: {e.Message}", e);
            }

            return result;
        }

        public async Task<OASISResult<IOASISGeoSpatialNFT>> MintAndPlaceGeoNFTAsync(IMintAndPlaceGeoSpatialNFTRequest request)
        {
            OASISResult<IOASISGeoSpatialNFT> result = new OASISResult<IOASISGeoSpatialNFT>();
            string errorMessage = "Error occured in MintAndPlaceGeoNFTAsync in NFTManager. Reason:";

            try
            {
                OASISResult<INFTTransactionRespone> mintNftResult = await MintNftAsync(CreateMintNFTTransactionRequest(request), true);

                if (mintNftResult != null && mintNftResult.Result != null && !mintNftResult.IsError)
                {
                    PlaceGeoSpatialNFTRequest placeGeoSpatialNFTRequest = new PlaceGeoSpatialNFTRequest()
                    {
                        OriginalOASISNFTId = mintNftResult.Result.OASISNFT.Id,
                        OriginalOASISNFTOffChainProviderType = request.OffChainProvider.Value,
                        ProviderType = request.OffChainProvider.Value,
                        PlacedByAvatarId = request.MintedByAvatarId,
                        Lat = request.Lat,
                        Long = request.Long,
                        AllowOtherPlayersToAlsoCollect = request.AllowOtherPlayersToAlsoCollect,
                        PermSpawn = request.PermSpawn,
                        GlobalSpawnQuantity = request.GlobalSpawnQuantity,
                        PlayerSpawnQuantity = request.PlayerSpawnQuantity
                    };

                    result.Result = CreateGeoSpatialNFT(placeGeoSpatialNFTRequest, mintNftResult.Result.OASISNFT);
                    OASISResult<IHolon> saveHolonResult = await Data.SaveHolonAsync(CreateGeoSpatialNFTMetaDataHolon(result.Result), request.MintedByAvatarId, true, true, 0, true, false, request.OffChainProvider.Value);

                    if (saveHolonResult != null && (saveHolonResult.IsError || saveHolonResult.Result == null) || saveHolonResult == null)
                    {
                        result.Result = null;
                        OASISErrorHandling.HandleError(ref result, $"{errorMessage} Error occured saving metadata holon to the OffChainProvider {Enum.GetName(typeof(ProviderType), request.OffChainProvider.Value)}. Reason: {saveHolonResult.Message}");
                    }
                    else
                        result.Message = $"Successfully minted and placed the new OASIS GeoNFT meta data on the offchain provider {request.OffChainProvider.Name} with id {result.Result.Id}, which was placed and minted by avatar with id {result.Result.PlacedByAvatarId}. The NFT was minted on the {result.Result.OnChainProvider.Name} onchain provider with hash {result.Result.Hash} and title '{result.Result.Title}' for the price of {result.Result.Price}. The OASISNFT id is {result.Result.OriginalOASISNFTId}.";
                }
                else
                    OASISErrorHandling.HandleError(ref result, $"{errorMessage} Error occured minting the GEONFT in function MintNftAsync. Reason: {mintNftResult.Message}");
            }
            catch (Exception e)
            {
                OASISErrorHandling.HandleError(ref result, $"{errorMessage} Unknown error occured: {e.Message}", e);
            }

            return result;
        }

        public OASISResult<IOASISGeoSpatialNFT> MintAndPlaceGeoNFT(IMintAndPlaceGeoSpatialNFTRequest request)
        {
            OASISResult<IOASISGeoSpatialNFT> result = new OASISResult<IOASISGeoSpatialNFT>();
            string errorMessage = "Error occured in MintAndPlaceGeoNFT in NFTManager. Reason:";

            try
            {
                OASISResult<INFTTransactionRespone> mintNftResult = MintNft(CreateMintNFTTransactionRequest(request), true);

                if (mintNftResult != null && mintNftResult.Result != null && !mintNftResult.IsError)
                {
                    PlaceGeoSpatialNFTRequest placeGeoSpatialNFTRequest = new PlaceGeoSpatialNFTRequest()
                    {
                        OriginalOASISNFTId = mintNftResult.Result.OASISNFT.Id,
                        OriginalOASISNFTOffChainProviderType = request.OffChainProvider.Value,
                        ProviderType = request.OffChainProvider.Value,
                        PlacedByAvatarId = request.MintedByAvatarId,
                        Lat = request.Lat,
                        Long = request.Long,
                        AllowOtherPlayersToAlsoCollect = request.AllowOtherPlayersToAlsoCollect,
                        PermSpawn = request.PermSpawn,
                        GlobalSpawnQuantity = request.GlobalSpawnQuantity,
                        PlayerSpawnQuantity = request.PlayerSpawnQuantity
                    };

                    result.Result = CreateGeoSpatialNFT(placeGeoSpatialNFTRequest, mintNftResult.Result.OASISNFT);
                    OASISResult<IHolon> saveHolonResult = Data.SaveHolon(CreateGeoSpatialNFTMetaDataHolon(result.Result), request.MintedByAvatarId, true, true, 0, true, false, request.OffChainProvider.Value);

                    if (saveHolonResult != null && (saveHolonResult.IsError || saveHolonResult.Result == null) || saveHolonResult == null)
                    {
                        result.Result = null;
                        OASISErrorHandling.HandleError(ref result, $"{errorMessage} Error occured saving metadata holon to the OffChainProvider {Enum.GetName(typeof(ProviderType), request.OffChainProvider.Value)}. Reason: {saveHolonResult.Message}");
                    }
                    else
                        result.Message = $"Successfully minted and placed the new OASIS GeoNFT meta data on the offchain provider {request.OffChainProvider.Name} with id {result.Result.Id}, which was placed and minted by avatar with id {result.Result.PlacedByAvatarId}. The NFT was minted on the {result.Result.OnChainProvider.Name} onchain provider with hash {result.Result.Hash} and title '{result.Result.Title}' for the price of {result.Result.Price}. The OASISNFT id is {result.Result.OriginalOASISNFTId}.";
                }
                else
                    OASISErrorHandling.HandleError(ref result, $"{errorMessage} Error occured minting the GEONFT in function MintNft. Reason: {mintNftResult.Message}");
            }
            catch (Exception e)
            {
                OASISErrorHandling.HandleError(ref result, $"{errorMessage} Unknown error occured: {e.Message}", e);
            }

            return result;
        }

        public ProviderType GetProviderTypeFromNFTProviderType(NFTProviderType nftProviderType)
        {
            ProviderType providerType = ProviderType.None;

            switch (nftProviderType)
            {
                case NFTProviderType.Solana:
                    providerType = ProviderType.SolanaOASIS;
                    break;

                case NFTProviderType.EOS:
                    providerType = ProviderType.EOSIOOASIS;
                    break;

                case NFTProviderType.Ethereum:
                    providerType = ProviderType.EthereumOASIS;
                    break;
            }

            return providerType;
        }

        public NFTProviderType GetNFTProviderTypeFromProviderType(ProviderType providerType)
        {
            NFTProviderType nftProviderType = NFTProviderType.None;

            switch (providerType)
            {
                case ProviderType.SolanaOASIS:
                    nftProviderType = NFTProviderType.Solana;
                    break;

                case ProviderType.EOSIOOASIS:
                    nftProviderType = NFTProviderType.EOS;
                    break;

                case ProviderType.EthereumOASIS:
                    nftProviderType = NFTProviderType.Ethereum;
                    break;
            }

            return nftProviderType;
        }

        //public IOASISNFTProvider GetNFTProvider<T>(NFTProviderType NFTProviderType, ref OASISResult<T> result, string errorMessage)
        //{
        //    return GetNFTProvider(GetProviderTypeFromNFTProviderType(NFTProviderType), ref result, errorMessage);
        //}

        public OASISResult<IOASISNFTProvider> GetNFTProvider(NFTProviderType NFTProviderType, string errorMessage = "")
        {
            return GetNFTProvider(GetProviderTypeFromNFTProviderType(NFTProviderType), errorMessage);
        }

        public OASISResult<IOASISNFTProvider> GetNFTProvider(ProviderType providerType, string errorMessage = "")
        {
            OASISResult<IOASISNFTProvider> result = new OASISResult<IOASISNFTProvider>();
            IOASISProvider OASISProvider = ProviderManager.Instance.GetProvider(providerType);

            if (OASISProvider != null)
            {
                if (!OASISProvider.IsProviderActivated)
                {
                    OASISResult<bool> activateProviderResult = OASISProvider.ActivateProvider();

                    if (activateProviderResult.IsError)
                        OASISErrorHandling.HandleError(ref result, $"{errorMessage} Error occured activating provider. Reason: {activateProviderResult.Message}");
                }
            }
            else
                OASISErrorHandling.HandleError(ref result, $"{errorMessage} The {Enum.GetName(typeof(ProviderType), providerType)} provider was not found.");

            if (!result.IsError)
            {
                result.Result = OASISProvider as IOASISNFTProvider;

                if (result.Result == null)
                    OASISErrorHandling.HandleError(ref result, $"{errorMessage} The {Enum.GetName(typeof(ProviderType), providerType)} provider is not a valid OASISNFTProvider.");
            }

            return result;
        }

        //public IOASISNFTProvider GetNFTProvider<T>(ProviderType providerType, ref OASISResult<T> result, string errorMessage)
        //{
        //    OASISResult<IOASISNFTProvider> getNFTProviderResult = GetNFTProvider(providerType);

        //    if (getNFTProviderResult == null || getNFTProviderResult != null && getNFTProviderResult.IsError ) 
        //    { 

        //    }


        //    //IOASISNFTProvider nftProvider = null;
        //    //IOASISProvider OASISProvider = ProviderManager.Instance.GetProvider(providerType);

        //    //if (OASISProvider != null)
        //    //{
        //    //    if (!OASISProvider.IsProviderActivated)
        //    //    {
        //    //        OASISResult<bool> activateProviderResult = OASISProvider.ActivateProvider();

        //    //        if (activateProviderResult.IsError)
        //    //            OASISErrorHandling.HandleError(ref result, $"{errorMessage} Error occured activating provider. Reason: {activateProviderResult.Message}");
        //    //    }
        //    //}
        //    //else
        //    //    OASISErrorHandling.HandleError(ref result, $"{errorMessage} The {Enum.GetName(typeof(ProviderType), providerType)} provider was not found.");

        //    //if (!result.IsError)
        //    //{
        //    //    nftProvider = OASISProvider as IOASISNFTProvider;

        //    //    if (nftProvider == null)
        //    //        OASISErrorHandling.HandleError(ref result, $"{errorMessage} The {Enum.GetName(typeof(ProviderType), providerType)} provider is not a valid OASISNFTProvider.");
        //    //}

        //    //return nftProvider;
        //}


        private OASISNFT CreateOASISNFT(IMintNFTTransactionRequest request, INFTTransactionRespone mintNFTResponse)
        {
            return new OASISNFT()
            {
                Id = Guid.NewGuid(),
                Hash = mintNFTResponse.TransactionResult,
                MintedByAddress = request.MintWalletAddress,
                MintedByAvatarId = request.MintedByAvatarId,
                Title = request.Title,
                Description = request.Description,
                Price = request.Price,
                Discount = request.Discount,
                Image = request.Image,
                ImageUrl = request.ImageUrl,
                Thumbnail = request.Thumbnail,
                ThumbnailUrl = request.ThumbnailUrl,
                OnChainProvider = request.OnChainProvider,
                OffChainProvider = request.OffChainProvider,
                MintedOn = DateTime.Now,
                MemoText = request.MemoText
                //OffChainProviderHolonId = Guid.NewGuid(),
                //Token= request.Token
            };
        }

        private OASISGeoSpatialNFT CreateGeoSpatialNFT(IPlaceGeoSpatialNFTRequest request, IOASISNFT originalNftMetaData)
        {
            return new OASISGeoSpatialNFT()
            {
                //Id = request.OASISNFTId != Guid.Empty ? request.OASISNFTId : Guid.NewGuid(),
                //Id = request.OASISNFTId,
                Id = Guid.NewGuid(),  //The NFT could be placed many times so we need a new ID for each time
                OriginalOASISNFTId = request.OriginalOASISNFTId, //We need to link back to the orignal NFT (but we copy across the NFT properties making it quicker and easier to get at the data). TODO: Do we want to copy the data across? Pros and Cons? Need to think about this... for now it's fine... ;-)
                GeoNFTMetaDataOffChainProvider = new EnumValue<ProviderType>(request.ProviderType),
                //OriginalOASISNFTProviderType = request.OriginalOASISNFTOffChainProviderType,
                Hash = originalNftMetaData.Hash,
                URL = originalNftMetaData.URL,
                MintedByAddress = originalNftMetaData.MintedByAddress,
                MintedByAvatarId = originalNftMetaData.MintedByAvatarId,
                Title = originalNftMetaData.Title,
                Description = originalNftMetaData.Description,
                Price = originalNftMetaData.Price,
                Discount = originalNftMetaData.Discount,
                Image = originalNftMetaData.Image,
                ImageUrl = originalNftMetaData.ImageUrl,
                Thumbnail = originalNftMetaData.Thumbnail,
                ThumbnailUrl = originalNftMetaData.ThumbnailUrl,
                MetaData = originalNftMetaData.MetaData,
                OnChainProvider = originalNftMetaData.OnChainProvider,
                OffChainProvider = originalNftMetaData.OffChainProvider,
                MintedOn = originalNftMetaData.MintedOn,
                MemoText = originalNftMetaData.MemoText,
                PlacedByAvatarId = request.PlacedByAvatarId,
                Lat = request.Lat,
                Long = request.Long,
                PermSpawn = request.PermSpawn,
                PlayerSpawnQuantity = request.PlayerSpawnQuantity,
                AllowOtherPlayersToAlsoCollect = request.AllowOtherPlayersToAlsoCollect,
                GlobalSpawnQuantity = request.GlobalSpawnQuantity,
                PlacedOn = DateTime.Now
            };
        }

        private IHolon CreateNFTMetaDataHolon(IOASISNFT nftMetaData, IMintNFTTransactionRequest request)
        {
            IHolon holonNFT = new Holon(HolonType.NFT);
            //holonNFT.Id = result.Result.OASISNFT.OffChainProviderHolonId;
            holonNFT.Id = nftMetaData.Id;
            holonNFT.CustomKey = nftMetaData.Hash;
            holonNFT.Name = $"{nftMetaData.OnChainProvider.Name} NFT Minted On The OASIS with title {nftMetaData.Title}";
            holonNFT.Description = nftMetaData.MemoText;
            holonNFT.MetaData["NFT.OASISNFT"] = JsonSerializer.Serialize(nftMetaData);
            holonNFT.MetaData["NFT.Hash"] = nftMetaData.Hash;
            holonNFT.MetaData["NFT.Id"] = nftMetaData.Id;
            holonNFT.MetaData["NFT.MintedByAvatarId"] = nftMetaData.MintedByAvatarId.ToString();
            holonNFT.MetaData["NFT.MintWalletAddress"] = nftMetaData.MintedByAddress;
            holonNFT.MetaData["NFT.MemoText"] = nftMetaData.MemoText;
            holonNFT.MetaData["NFT.Title"] = nftMetaData.Title;
            holonNFT.MetaData["NFT.Description"] = nftMetaData.Description;
            holonNFT.MetaData["NFT.Price"] = nftMetaData.Price.ToString();
            holonNFT.MetaData["NFT.NumberToMint"] = request.NumberToMint.ToString();
            holonNFT.MetaData["NFT.OnChainProvider"] = nftMetaData.OnChainProvider.Name;
            holonNFT.MetaData["NFT.OffChainProvider"] = nftMetaData.OffChainProvider.Name;
            holonNFT.MetaData["NFT.Image"] = nftMetaData.Image;
            holonNFT.MetaData["NFT.ImageUrl"] = nftMetaData.ImageUrl;
            holonNFT.MetaData["NFT.Thumbnail"] = nftMetaData.Thumbnail;
            holonNFT.MetaData["NFT.ThumbnailUrl"] = nftMetaData.ThumbnailUrl;
            holonNFT.MetaData["NFT.MintedOn"] = nftMetaData.MintedOn.ToShortDateString();
            holonNFT.ParentHolonId = nftMetaData.MintedByAvatarId;

            return holonNFT;
        }

        private IHolon CreateGeoSpatialNFTMetaDataHolon(IOASISGeoSpatialNFT geoNFTMetaData)
        {
            IHolon holonNFT = new Holon(HolonType.GEONFT);
            //holonNFT.Id = result.Result.OASISNFT.OffChainProviderHolonId;
            holonNFT.Id = geoNFTMetaData.Id;
            holonNFT.Name = "OASIS GEO NFT"; // $"{Enum.GetName(typeof(ProviderType), request.OnChainProvider)} NFT Minted On The OASIS with title {request.Title}";
            holonNFT.Description = "OASIS GEO NFT";
            holonNFT.MetaData["GEONFT.OASISGEONFT"] = JsonSerializer.Serialize(geoNFTMetaData);
            holonNFT.MetaData["GEONFT.Id"] = geoNFTMetaData.Id;
            holonNFT.MetaData["GEONFT.GeoNFTMetaDataOffChainProvider"] = geoNFTMetaData.GeoNFTMetaDataOffChainProvider.Name;
            holonNFT.MetaData["GEONFT.PlacedByAvatarId"] = geoNFTMetaData.PlacedByAvatarId.ToString();
            holonNFT.MetaData["GEONFT.PlacedOn"] = geoNFTMetaData.PlacedOn.ToShortDateString();
            holonNFT.MetaData["GEONFT.Lat"] = geoNFTMetaData.Lat;
            holonNFT.MetaData["GEONFT.Long"] = geoNFTMetaData.Long;
            holonNFT.MetaData["GEONFT.PermSpawn"] = geoNFTMetaData.PermSpawn;
            holonNFT.MetaData["GEONFT.PlayerSpawnQuantity"] = geoNFTMetaData.PlayerSpawnQuantity;
            holonNFT.MetaData["GEONFT.AllowOtherPlayersToAlsoCollect"] = geoNFTMetaData.AllowOtherPlayersToAlsoCollect;
            holonNFT.MetaData["GEONFT.GlobalSpawnQuantity"] = geoNFTMetaData.GlobalSpawnQuantity;
            holonNFT.MetaData["GEONFT.OriginalOASISNFT.Id"] = geoNFTMetaData.OriginalOASISNFTId;
            //holonNFT.MetaData["GEONFT.OriginalOASISNFT.ProviderType"] = Enum.GetName(typeof(ProviderType), geoNFTMetaData.OriginalOASISNFTProviderType);
            //holonNFT.MetaData["GEONFT.OriginalOASISNFT.ProviderType"] = geoNFTMetaData.OffChainProvider.Name;
            holonNFT.MetaData["GEONFT.OriginalOASISNFT.Hash"] = geoNFTMetaData.Hash;
            //holonNFT.MetaData["GEONFT.OriginalOASISNFT.MemoText"] = geoNFTMetaData.MetaData["MemoText"];
            holonNFT.MetaData["GEONFT.OriginalOASISNFT.MemoText"] = geoNFTMetaData.MemoText;
            holonNFT.MetaData["GEONFT.OriginalOASISNFT.Title"] = geoNFTMetaData.Title;
            holonNFT.MetaData["GEONFT.OriginalOASISNFT.Description"] = geoNFTMetaData.Description;
            holonNFT.MetaData["GEONFT.OriginalOASISNFT.MintedByAvatarId"] = geoNFTMetaData.MintedByAvatarId.ToString();
            holonNFT.MetaData["GEONFT.OriginalOASISNFT.MintWalletAddress"] = geoNFTMetaData.MintedByAddress; //geoNFTMetaData.MintWalletAddress;
            holonNFT.MetaData["GEONFT.OriginalOASISNFT.Price"] = geoNFTMetaData.Price.ToString();
            //holonNFT.MetaData["GEONFT.NumberToMint"] = geoNFTMetaData.NumberToMint.ToString();
            holonNFT.MetaData["GEONFT.OriginalOASISNFT.OnChainProvider"] = geoNFTMetaData.OnChainProvider.Name;
            holonNFT.MetaData["GEONFT.OriginalOASISNFT.OffChainProvider"] = geoNFTMetaData.OffChainProvider.Name;
            holonNFT.MetaData["GEONFT.OriginalOASISNFT.Image"] = geoNFTMetaData.Image;
            holonNFT.MetaData["GEONFT.OriginalOASISNFT.ImageUrl"] = geoNFTMetaData.ImageUrl;
            holonNFT.MetaData["GEONFT.OriginalOASISNFT.Thumbnail"] = geoNFTMetaData.Thumbnail;
            holonNFT.MetaData["GEONFT.OriginalOASISNFT.ThumbnailUrl"] = geoNFTMetaData.ThumbnailUrl;
            holonNFT.MetaData["GEONFT.OriginalOASISNFT.MintedOn"] = geoNFTMetaData.MintedOn.ToShortDateString();

            return holonNFT;
        }

        private IMintNFTTransactionRequest CreateMintNFTTransactionRequest(IMintAndPlaceGeoSpatialNFTRequest mintAndPlaceGeoSpatialNFTRequest)
        {
            return new MintNFTTransactionRequest()
            {
                MintWalletAddress = mintAndPlaceGeoSpatialNFTRequest.MintWalletAddress,
                MintedByAvatarId = mintAndPlaceGeoSpatialNFTRequest.MintedByAvatarId,
                Title = mintAndPlaceGeoSpatialNFTRequest.Title,
                Description = mintAndPlaceGeoSpatialNFTRequest.Description,
                Image = mintAndPlaceGeoSpatialNFTRequest.Image,
                ImageUrl = mintAndPlaceGeoSpatialNFTRequest.ImageUrl,
                Thumbnail = mintAndPlaceGeoSpatialNFTRequest.Thumbnail,
                ThumbnailUrl = mintAndPlaceGeoSpatialNFTRequest.ThumbnailUrl,
                Price = mintAndPlaceGeoSpatialNFTRequest.Price,
                Discount = mintAndPlaceGeoSpatialNFTRequest.Discount,
                MemoText = mintAndPlaceGeoSpatialNFTRequest.MemoText,
                NumberToMint = mintAndPlaceGeoSpatialNFTRequest.NumberToMint,
                MetaData = mintAndPlaceGeoSpatialNFTRequest.MetaData,
                OffChainProvider = mintAndPlaceGeoSpatialNFTRequest.OffChainProvider,
                OnChainProvider = mintAndPlaceGeoSpatialNFTRequest.OnChainProvider
            };
        }

        private OASISResult<IOASISNFT> DecodeNFTMetaData(OASISResult<IHolon> holonResult, OASISResult<IOASISNFT> result, string errorMessage)
        {
            if (holonResult != null && !holonResult.IsError && holonResult.Result != null)
            {
                result.Result = (IOASISNFT)JsonSerializer.Deserialize(holonResult.Result.MetaData["NFT.OASISNFT"].ToString(), typeof(OASISNFT));
            }
            else
                OASISErrorHandling.HandleError(ref result, $"{errorMessage} Error occured loading holon metadata. Reason: {holonResult.Message}");

            return result;
        }

        private OASISResult<IOASISGeoSpatialNFT> DecodeGeoNFTMetaData(OASISResult<IHolon> holonResult, OASISResult<IOASISGeoSpatialNFT> result, string errorMessage)
        {
            if (holonResult != null && !holonResult.IsError && holonResult.Result != null)
            {
                result.Result = (OASISGeoSpatialNFT)JsonSerializer.Deserialize(holonResult.Result.MetaData["GEONFT.OASISGEONFT"].ToString(), typeof(OASISGeoSpatialNFT));
            }
            else
                OASISErrorHandling.HandleError(ref result, $"{errorMessage} Error occured loading holon metadata. Reason: {holonResult.Message}");

            return result;
        }

        private OASISResult<IEnumerable<IOASISNFT>> DecodeNFTMetaData(OASISResult<IEnumerable<IHolon>> holonsResult, OASISResult<IEnumerable<IOASISNFT>> result, string errorMessage)
        {
            List<IOASISNFT> nfts = new List<IOASISNFT>();

            if (holonsResult != null && !holonsResult.IsError && holonsResult.Result != null)
            {
                if (holonsResult.Result.Count() > 0)
                {
                    foreach (IHolon holon in holonsResult.Result)
                        nfts.Add((IOASISNFT)JsonSerializer.Deserialize(holon.MetaData["NFT.OASISNFT"].ToString(), typeof(OASISNFT)));

                    result.Result = nfts;
                }
                else
                    result.Message = "No NFT's Found.";
            }
            else
                OASISErrorHandling.HandleError(ref result, $"{errorMessage} Error occured loading holon metadata. Reason: {holonsResult.Message}");

            return result;
        }

        private OASISResult<IEnumerable<IOASISGeoSpatialNFT>> DecodeGeoNFTMetaData(OASISResult<IEnumerable<IHolon>> holonsResult, OASISResult<IEnumerable<IOASISGeoSpatialNFT>> result, string errorMessage)
        {
            List<IOASISGeoSpatialNFT> nfts = new List<IOASISGeoSpatialNFT>();

            if (holonsResult != null && !holonsResult.IsError && holonsResult.Result != null)
            {
                if (holonsResult.Result.Count() > 0)
                {
                    foreach (IHolon holon in holonsResult.Result)
                        nfts.Add((IOASISGeoSpatialNFT)JsonSerializer.Deserialize(holon.MetaData["GEONFT.OASISGEONFT"].ToString(), typeof(OASISGeoSpatialNFT)));

                    result.Result = nfts;
                }
                else
                    result.Message = "No GeoNFT's Found.";
            }
            else
                OASISErrorHandling.HandleError(ref result, $"{errorMessage} Error occured loading holon metadata. Reason: {holonsResult.Message}");

            return result;
        }

        //TODO: Lots more coming soon! ;-)
    }
}