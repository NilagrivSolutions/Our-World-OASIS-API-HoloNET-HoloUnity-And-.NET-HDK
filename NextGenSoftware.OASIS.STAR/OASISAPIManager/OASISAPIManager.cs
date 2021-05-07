﻿using System.Collections.Generic;
using NextGenSoftware.OASIS.API.DNA;
using NextGenSoftware.OASIS.API.Core.Interfaces;
using NextGenSoftware.OASIS.API.Core.Managers;
using NextGenSoftware.OASIS.API.Core.Apollo.Server;
using NextGenSoftware.OASIS.API.Providers.AcitvityPubOASIS;
using NextGenSoftware.OASIS.API.Providers.BlockStackOASIS;
using NextGenSoftware.OASIS.API.Providers.EOSIOOASIS;
using NextGenSoftware.OASIS.API.Providers.EthereumOASIS;
using NextGenSoftware.OASIS.API.Providers.HoloOASIS.Desktop;
using NextGenSoftware.OASIS.API.Providers.IPFSOASIS;
using NextGenSoftware.OASIS.API.Providers.MongoDBOASIS;
using NextGenSoftware.OASIS.API.Providers.Neo4jOASIS;
using NextGenSoftware.OASIS.API.Providers.SEEDSOASIS;
using NextGenSoftware.OASIS.API.Providers.TelosOASIS;
using NextGenSoftware.OASIS.API.Core.Enums;
using NextGenSoftware.OASIS.API.DNA.Manager;

namespace NextGenSoftware.OASIS.STAR.OASISAPIManager
{
    public class OASISProviders
    {
        SEEDSOASIS _SEEDS;
        IPFSOASIS _IPFS;
        EOSIOOASIS _EOSIO;
        TelosOASIS _Telos;
        HoloOASIS _Holochain;
        MongoDBOASIS _MongoDB;
        Neo4jOASIS _Neo4j;
        EthereumOASIS _Ethereum;
        ThreeFoldOASIS _ThreeFold;
        AcitvityPubOASIS _ActivityPub;
        OASISDNA _OASISDNA;

        public SEEDSOASIS SEEDS
        {
            get
            {
                if (_SEEDS == null)
                {
                    if (ProviderManager.IsProviderRegistered(ProviderType.SEEDSOASIS))
                        _SEEDS = (SEEDSOASIS)ProviderManager.GetStorageProvider(ProviderType.SEEDSOASIS);
                    else
                    {
                        // We could re-use the TelosOASIS Provider but it could have a different connection string to SEEDSOASIS so they need to be seperate.
                        // TODO: The only other way is to share it and have to keep disconnecting and re-connecting with the different connections (SEEDS or EOSIO may even work with any EOSIO node end point? NEED TO TEST... if so then we can use the commented out line below).
                        //_SEEDS = new SEEDSOASIS(Telos); 
                        _SEEDS = new SEEDSOASIS(new TelosOASIS(_OASISDNA.OASIS.StorageProviders.SEEDSOASIS.ConnectionString));
                        ProviderManager.RegisterProvider(_SEEDS);
                    }
                }

                return _SEEDS;
            }
        }

        public IPFSOASIS IPFS
        {
            get
            {
                if (_IPFS == null)
                    _IPFS = (IPFSOASIS)OASISDNAManager.RegisterProvider(ProviderType.IPFSOASIS);

                return _IPFS;
            }
        }

        public EOSIOOASIS EOSIO
        {
            get
            {
                if (_EOSIO == null)
                    _EOSIO = (EOSIOOASIS)OASISDNAManager.RegisterProvider(ProviderType.EOSIOOASIS);

                return _EOSIO;
            }
        }

        public TelosOASIS Telos
        {
            get
            {
                if (_Telos == null)
                    _Telos = (TelosOASIS)OASISDNAManager.RegisterProvider(ProviderType.TelosOASIS);

                return _Telos;
            }
        }

        public HoloOASIS Holochain
        {
            get
            {
                if (_Holochain == null)
                    _Holochain = (HoloOASIS)OASISDNAManager.RegisterProvider(ProviderType.HoloOASIS);

                return _Holochain;
            }
        }

        public MongoDBOASIS MongoDB
        {
            get
            {
                if (_MongoDB == null)
                    _MongoDB = (MongoDBOASIS)OASISDNAManager.RegisterProvider(ProviderType.MongoDBOASIS);

                return _MongoDB;
            }
        }

        public Neo4jOASIS Neo4j
        {
            get
            {
                if (_Neo4j == null)
                    _Neo4j = (Neo4jOASIS)OASISDNAManager.RegisterProvider(ProviderType.Neo4jOASIS);

                return _Neo4j;
            }
        }

        public EthereumOASIS Ethereum
        {
            get
            {
                if (_Ethereum == null)
                    _Ethereum = (EthereumOASIS)OASISDNAManager.RegisterProvider(ProviderType.EthereumOASIS);

                return _Ethereum;
            }
        }

        public ThreeFoldOASIS ThreeFold
        {
            get
            {
                if (_ThreeFold == null)
                    _ThreeFold = (ThreeFoldOASIS)OASISDNAManager.RegisterProvider(ProviderType.ThreeFoldOASIS);

                return _ThreeFold;
            }
        }

        public AcitvityPubOASIS ActivityPub
        {
            get
            {
                if (_ActivityPub == null)
                    _ActivityPub = (AcitvityPubOASIS)OASISDNAManager.RegisterProvider(ProviderType.ActivityPubOASIS);

                return _ActivityPub;
            }
        }

        public OASISProviders(OASISDNA OASISDNA)
        {
            _OASISDNA = OASISDNA;
        }
    }

    public class OASISAPI
    {
        public AvatarManager Avatar { get; set; }
        public HolonManager Data { get; set; }
        public MapManager Map { get; set; }
        public OASISProviders Providers { get; private set; }

        public void Init(InitOptions options, OASISDNA OASISDNA, bool startApolloServer = true)
        {
            switch (options)
            {
                case InitOptions.InitWithAllProviders:
                    Init(ProviderManager.GetAllRegisteredProviders(), OASISDNA, startApolloServer);
                    break;

                case InitOptions.InitWithCurrentDefaultProvider:
                    Init(new List<IOASISProvider>() { ProviderManager.CurrentStorageProvider }, OASISDNA, startApolloServer);
                    break;
            }
        }

        public void Init(List<IOASISProvider> OASISProviders, OASISDNA OASISDNA, bool startApolloServer = true)
        {
            ProviderManager.RegisterProviders(OASISProviders); //TODO: Soon you will not need to pass these in since MEF will taKe care of this for us.

            // TODO: Soon you will not need to inject in a provider because the mappings below will be used instead...
            Map = new MapManager((IOASISStorage)OASISProviders[0]); 
            Avatar = new AvatarManager((IOASISStorage)OASISProviders[0], OASISDNA);
            Data = new HolonManager((IOASISStorage)OASISProviders[0]);
            Providers = new OASISProviders(OASISDNA);

            if (startApolloServer)
                ApolloServer.StartServer();

            ////TODO: Move the mappings to an external config wrapper than is injected into the OASISAPIManager constructor above...
            //// Give HoloOASIS Store permission for the Name field (the field will only be stored on Holochain).
            //Avatar.Config.FieldToProviderMappings.Name.Add(new ProviderManagerConfig.FieldToProviderMappingAccess { Access = ProviderManagerConfig.ProviderAccess.Store, Provider = ProviderType.HoloOASIS });

            //// Give all providers read/write access to the Karma field (will allow them to read and write to the field but it will only be stored on Holochain).
            //// You could choose to store it on more than one provider if you wanted the extra redundancy (but not normally needed since Holochain has a lot of redundancy built in).
            //Avatar.Config.FieldToProviderMappings.Karma.Add(new ProviderManagerConfig.FieldToProviderMappingAccess { Access = ProviderManagerConfig.ProviderAccess.ReadWrite, Provider = ProviderType.All });
            ////this.AvatarManager.Config.FieldToProviderMappings.Name.Add(new AvatarManagerConfig.FieldToProviderMappingAccess { Access = AvatarManagerConfig.ProviderAccess.ReadWrite, Provider = ProviderType.EthereumOASIS });
            ////this.AvatarManager.Config.FieldToProviderMappings.Name.Add(new AvatarManagerConfig.FieldToProviderMappingAccess { Access = AvatarManagerConfig.ProviderAccess.ReadWrite, Provider = ProviderType.IPFSOASIS });
            ////this.AvatarManager.Config.FieldToProviderMappings.DOB.Add(new AvatarManagerConfig.FieldToProviderMappingAccess { Access = AvatarManagerConfig.ProviderAccess.Store, Provider = ProviderType.HoloOASIS });

            ////Give Ethereum read-only access to the DOB field.
            //Avatar.Config.FieldToProviderMappings.DOB.Add(new ProviderManagerConfig.FieldToProviderMappingAccess { Access = ProviderManagerConfig.ProviderAccess.ReadOnly, Provider = ProviderType.EthereumOASIS });
        }
    }
}