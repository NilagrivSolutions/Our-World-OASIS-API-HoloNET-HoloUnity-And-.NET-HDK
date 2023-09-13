﻿
using NextGenSoftware.OASIS.API.Core.Helpers;
using NextGenSoftware.OASIS.API.Core.Interfaces.NFT;
using NextGenSoftware.OASIS.API.Core.Interfaces.NFT.GeoSpatialNFT;
using NextGenSoftware.OASIS.API.Core.Objects.Wallets;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NextGenSoftware.OASIS.API.Core.Interfaces
{
    public interface IOASISNFTProvider : IOASISProvider
    {
        //TODO: More to come soon... ;-)
        public OASISResult<NFTTransactionRespone> SendNFT(INFTWalletTransaction transation);
        public Task<OASISResult<NFTTransactionRespone>> SendNFTAsync(INFTWalletTransaction transation);

        public OASISResult<NFTTransactionRespone> MintNFT(IMintNFTTransaction transation);
        public Task<OASISResult<NFTTransactionRespone>> MintNFTAsync(IMintNFTTransaction transation);

        //These load methods below will apply ONLY to the specefic provider/blockchain that they are implemented on. So will not load for ALL providers across the OASIS, but the versions implemented on NFTManger DOES...
        public OASISResult<IOASISNFT> LoadNFT(Guid id);
        public Task<OASISResult<IOASISNFT>> LoadNFTAsync(Guid id);

        public OASISResult<IOASISNFT> LoadNFT(string hash);
        public Task<OASISResult<IOASISNFT>> LoadNFTAsync(string hash);

        public OASISResult<List<IOASISNFT>> LoadAllNFTsForAvatar(Guid avatarId);
        public Task<OASISResult<List<IOASISNFT>>> LoadAllNFTsForAvatarAsync(Guid avatarId);

        public OASISResult<List<IOASISNFT>> LoadAllNFTsForMintAddress(string mintWalletAddress);
        public Task<OASISResult<List<IOASISNFT>>> LoadAllNFTsForMintAddressAsync(string mintWalletAddress);

        public OASISResult<List<IOASISGeoSpatialNFT>> LoadAllGeoNFTsForAvatar(Guid avatarId);
        public Task<OASISResult<List<IOASISGeoSpatialNFT>>> LoadAllGeoNFTsForAvatarAsync(Guid avatarId);

        public OASISResult<List<IOASISGeoSpatialNFT>> LoadAllGeoNFTsForMintAddress(string mintWalletAddress);
        public Task<OASISResult<List<IOASISGeoSpatialNFT>>> LoadAllGeoNFTsForMintAddressAsync(string mintWalletAddress);

        public OASISResult<IOASISGeoSpatialNFT> PlaceGeoNFT(IPlaceGeoSpatialNFTRequest request);
        public Task<OASISResult<IOASISGeoSpatialNFT>> PlaceGeoNFTAsync(IPlaceGeoSpatialNFTRequest request);

        public OASISResult<IOASISGeoSpatialNFT> MintAndPlaceGeoNFT(IMintAndPlaceGeoSpatialNFTRequest request);
        public Task<OASISResult<IOASISGeoSpatialNFT>> MintAndPlaceGeoNFTAsync(IMintAndPlaceGeoSpatialNFTRequest request);
    }
}