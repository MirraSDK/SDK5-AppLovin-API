using System;
using MirraGames.SDK.Common;
using UnityEngine;

namespace MirraGames.SDK.AppLovin
{
    [Serializable]
    [ProviderConfiguration(typeof(AppLovinAds))]
    public class AppLovinAds_Configuration : PropertyGroup
    {
        public override string Name => nameof(AppLovinAds);

        [field: SerializeField] public string InterstitialAdUnitIdAndroid { get; private set; } = "demo-interstitial";
        [field: SerializeField] public string InterstitialAdUnitIdIOS { get; private set; } = "demo-interstitial";
        
        [field: SerializeField] public string RewardedAdUnitIdAndroid { get; private set; } = "demo-rewarded";
        [field: SerializeField] public string RewardedAdUnitIdIOS { get; private set; } = "demo-rewarded";

        [field: SerializeField] public string BannerAdUnitIdAndroid { get; private set; } = "demo-banner";
        [field: SerializeField] public string BannerAdUnitIdIOS { get; private set; } = "demo-banner";

        public override StringProperty[] GetStringProperties()
        {
            return new StringProperty[] {
                new(
                    "Interstitial UID Android",
                    getter: () => { return InterstitialAdUnitIdAndroid; },
                    setter: (value) => { InterstitialAdUnitIdAndroid = value; }
                ),
                new(
                    "Interstitial UID iOS",
                    getter: () => { return InterstitialAdUnitIdIOS; },
                    setter: (value) => { InterstitialAdUnitIdIOS = value; }
                ),
                new(
                    "Rewarded UID Android",
                    getter: () => { return RewardedAdUnitIdAndroid; },
                    setter: (value) => { RewardedAdUnitIdAndroid = value; }
                ),
                new(
                    "Rewarded UID iOS",
                    getter: () => { return RewardedAdUnitIdIOS; },
                    setter: (value) => { RewardedAdUnitIdIOS = value; }
                ),
                new(
                    "Banner UID Android",
                    getter: () => { return BannerAdUnitIdAndroid; },
                    setter: (value) => { BannerAdUnitIdAndroid = value; }
                ),
                new(
                    "Banner UID iOS",
                    getter: () => { return BannerAdUnitIdIOS; },
                    setter: (value) => { BannerAdUnitIdIOS = value; }
                )
            };
        }
    }
}
