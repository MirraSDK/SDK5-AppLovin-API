using MirraGames.SDK.Common;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Logger = MirraGames.SDK.Common.Logger;

namespace MirraGames.SDK.AppLovin
{

    [Provider(typeof(IAds))]
    public class AppLovinAds : CommonAds
    {

        public AppLovinAds(IEventAggregator eventAggregator) : base(eventAggregator)
        {

        }

        protected override void InvokeBannerImpl()
        {

        }

        protected override void RefreshBannerImpl()
        {

        }

        protected override void DisableBannerImpl()
        {

        }

        protected override void InvokeInterstitialImpl(Action onOpen = null, Action<bool> onClose = null)
        {

        }

        protected override void InvokeRewardedImpl(Action onOpen = null, Action<bool> onClose = null, string rewardTag = null)
        {

        }

    }

}