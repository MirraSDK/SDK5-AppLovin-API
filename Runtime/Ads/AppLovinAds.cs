using MirraGames.SDK.Common;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Logger = MirraGames.SDK.Common.Logger;
using AppLovinMax;

namespace MirraGames.SDK.AppLovin
{

    [Provider(typeof(IAds))]
    public class AppLovinAds : CommonAds
    {

        private readonly AppLovinAds_Configuration configuration;

        private string rewardedTag;
        private Action<bool> onRewardedClose;
        private bool isRewardedSuccess;
        private Action<bool> onInterstitialClose;

        public AppLovinAds(AppLovinAds_Configuration configuration, IEventAggregator eventAggregator) : base(eventAggregator)
        {
            this.configuration = configuration;

            MaxSdkCallbacks.OnSdkInitializedEvent += (MaxSdkBase.SdkConfiguration sdkConfiguration) =>
            {
                Logger.CreateText(nameof(AppLovinAds), "Max SDK Initialized", JsonUtility.ToJson(sdkConfiguration));
                SetInitialized();
#if UNITY_ANDROID
                MaxSdk.LoadRewardedAd(configuration.RewardedAdUnitIdAndroid);
                MaxSdk.LoadInterstitial(configuration.InterstitialAdUnitIdAndroid);
#elif UNITY_IOS
                MaxSdk.LoadRewardedAd(configuration.RewardedAdUnitIdIOS);
                MaxSdk.LoadInterstitial(configuration.InterstitialAdUnitIdIOS);
#endif
            };

            MaxSdkCallbacks.Rewarded.OnAdLoadedEvent += OnRewardedAdLoaded;
            MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent += OnRewardedAdFailedToLoad;
            MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent += OnRewardedAdFailedToShow;
            MaxSdkCallbacks.Rewarded.OnAdHiddenEvent += OnRewardedAdClosed;
            MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent += OnRewardedAdReceivedReward;
            MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent += OnRewardedAdRevenuePaidEvent;

            MaxSdkCallbacks.Interstitial.OnAdLoadedEvent += OnInterstitialAdLoaded;
            MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent += OnInterstitialAdFailedToLoad;
            MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent += OnInterstitialAdFailedToShow;
            MaxSdkCallbacks.Interstitial.OnAdHiddenEvent += OnInterstitialAdClosed;
            MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent += OnInterstitialAdRevenuePaidEvent;

            MaxSdk.InitializeSdk();
        }

        public override bool IsInterstitialAvailable => true;
        public override bool IsInterstitialReady => true;
        public override bool IsRewardedAvailable => true;
        public override bool IsRewardedReady => true;

        private void OnInterstitialAdRevenuePaidEvent(string arg1, MaxSdkBase.AdInfo info)
        {
            Logger.CreateText(nameof(AppLovinAds), "OnInterstitialAdRevenuePaidEvent", arg1, JsonUtility.ToJson(info));
        }

        private void OnInterstitialAdClosed(string arg1, MaxSdkBase.AdInfo info)
        {
            Logger.CreateText(nameof(AppLovinAds), "OnInterstitialAdClosed", arg1, JsonUtility.ToJson(info));
            onInterstitialClose?.Invoke(true);
        }

        private void OnInterstitialAdFailedToShow(string arg1, MaxSdkBase.ErrorInfo info1, MaxSdkBase.AdInfo info2)
        {
            Logger.CreateText(nameof(AppLovinAds), "OnInterstitialAdFailedToShow", arg1, JsonUtility.ToJson(info1), JsonUtility.ToJson(info2));
            onInterstitialClose?.Invoke(false);
        }

        private void OnInterstitialAdFailedToLoad(string arg1, MaxSdkBase.ErrorInfo info)
        {
            Logger.CreateText(nameof(AppLovinAds), "OnInterstitialAdFailedToLoad", arg1, JsonUtility.ToJson(info));
        }

        private void OnInterstitialAdLoaded(string arg1, MaxSdkBase.AdInfo info)
        {
            Logger.CreateText(nameof(AppLovinAds), "OnInterstitialAdLoaded", arg1, JsonUtility.ToJson(info));
        }

        private void OnRewardedAdRevenuePaidEvent(string arg1, MaxSdkBase.AdInfo info)
        {
            Logger.CreateText(nameof(AppLovinAds), "OnRewardedAdRevenuePaidEvent", arg1, JsonUtility.ToJson(info));
        }

        private void OnRewardedAdReceivedReward(string arg1, MaxSdkBase.Reward reward, MaxSdkBase.AdInfo info)
        {
            Logger.CreateText(nameof(AppLovinAds), "OnRewardedAdReceivedReward", arg1, JsonUtility.ToJson(reward), JsonUtility.ToJson(info));
            isRewardedSuccess = true;
        }

        private void OnRewardedAdClosed(string arg1, MaxSdkBase.AdInfo info)
        {
            Logger.CreateText(nameof(AppLovinAds), "OnRewardedAdClosed", arg1, JsonUtility.ToJson(info));
            onRewardedClose?.Invoke(isRewardedSuccess);
            isRewardedSuccess = false;

            // Reload the rewarded ad
            string adUnitId = string.Empty;
#if UNITY_ANDROID
            adUnitId = configuration.RewardedAdUnitIdAndroid;
#elif UNITY_IOS
            adUnitId = configuration.RewardedAdUnitIdIOS;
#endif
            MaxSdk.LoadRewardedAd(adUnitId);
        }

        private void OnRewardedAdFailedToShow(string arg1, MaxSdkBase.ErrorInfo info1, MaxSdkBase.AdInfo info2)
        {
            Logger.CreateText(nameof(AppLovinAds), "OnRewardedAdFailedToShow", arg1, JsonUtility.ToJson(info1), JsonUtility.ToJson(info2));
            onRewardedClose?.Invoke(false);
        }

        private void OnRewardedAdFailedToLoad(string arg1, MaxSdkBase.ErrorInfo info)
        {
            Logger.CreateText(nameof(AppLovinAds), "OnRewardedAdFailedToLoad", arg1, JsonUtility.ToJson(info));
        }

        private void OnRewardedAdLoaded(string arg1, MaxSdkBase.AdInfo info)
        {
            Logger.CreateText(nameof(AppLovinAds), "OnRewardedAdLoaded", arg1, JsonUtility.ToJson(info));
        }

        protected override void InvokeBannerImpl()
        {
            Logger.NotImplementedWarning(this, nameof(InvokeBannerImpl));
        }

        protected override void RefreshBannerImpl()
        {
            Logger.NotImplementedWarning(this, nameof(RefreshBannerImpl));
        }

        protected override void DisableBannerImpl()
        {
            Logger.NotImplementedWarning(this, nameof(DisableBannerImpl));
        }

        protected override void InvokeInterstitialImpl(InterstitialParameters parameters, Action onOpen, Action<bool> onClose)
        {
            string adUnitId = string.Empty;
#if UNITY_ANDROID
            adUnitId = configuration.InterstitialAdUnitIdAndroid;
#elif UNITY_IOS
            adUnitId = configuration.InterstitialAdUnitIdIOS;
#endif
            onInterstitialClose = onClose;

            if (MaxSdk.IsInterstitialReady(adUnitId))
            {
                onOpen?.Invoke();
                MaxSdk.ShowInterstitial(adUnitId);
            }
            else
            {
                Logger.CreateText(nameof(AppLovinAds), "Interstitial ad not ready");
                onClose?.Invoke(false);
            }
        }

        protected override void InvokeRewardedImpl(RewardedParameters parameters, Action onOpen, Action<bool> onClose)
        {
            string adUnitId = string.Empty;
#if UNITY_ANDROID
            adUnitId = configuration.RewardedAdUnitIdAndroid;
#elif UNITY_IOS
            adUnitId = configuration.RewardedAdUnitIdIOS;
#endif
            rewardedTag = parameters.PlacementId;
            onRewardedClose = onClose;

            if (MaxSdk.IsRewardedAdReady(adUnitId))
            {
                onOpen?.Invoke();
                MaxSdk.ShowRewardedAd(adUnitId);
            }
            else
            {
                Logger.CreateText(nameof(AppLovinAds), "Rewarded ad not ready");
                onClose?.Invoke(false);
            }

        }

    }

}