﻿
namespace NextGenSoftware.OASIS.API.Core.Interfaces.STAR
{
    public interface IFourthDimension : IDimension
    {
        //TODO: Not sure if this also has parallel universes like the third dimension does?
        IUniverse Universe { get; set; }
    }
}