using MirraGames.SDK.Common;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Logger = MirraGames.SDK.Common.Logger;
using AppLovinMax;
using Firebase.Analytics;
using Io.AppMetrica;

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

        private void LogVideoAdsAvailable(string adType, string placement)
        {
            string result = "success";  // Loaded successfully
            string connection = Application.internetReachability != NetworkReachability.NotReachable ? "online" : "offline";

            string jsonParams = $@"{{
                ""ad_type"": ""{adType}"",
                ""placement"": ""{placement}"",
                ""result"": ""{result}"",
                ""connection"": ""{connection}""
            }}";

            FirebaseAnalytics.LogEvent("video_ads_available", new Parameter[]
            {
                new Parameter("ad_type", adType),
                new Parameter("placement", placement),
                new Parameter("result", result),
                new Parameter("connection", connection)
            });

            AppMetrica.ReportEvent("video_ads_available", jsonParams);
        }

        private void LogVideoAdsStarted(string adType, string placement)
        {
            string result = "started";  // Or detect if show failed later
            string connection = Application.internetReachability != NetworkReachability.NotReachable ? "online" : "offline";

            string jsonParams = $@"{{
                ""ad_type"": ""{adType}"",
                ""placement"": ""{placement}"",
                ""result"": ""{result}"",
                ""connection"": ""{connection}""
            }}";

            FirebaseAnalytics.LogEvent("video_ads_started", new Parameter[]
            {
                new Parameter("ad_type", adType),
                new Parameter("placement", placement),
                new Parameter("result", result),
                new Parameter("connection", connection)
            });

            AppMetrica.ReportEvent("video_ads_started", jsonParams);
        }

        private void LogVideoAdsWatch(string adType, string placement, string result)
        {
            string connection = Application.internetReachability != NetworkReachability.NotReachable ? "online" : "offline";

            string jsonParams = $@"{{
                ""ad_type"": ""{adType}"",
                ""placement"": ""{placement}"",
                ""result"": ""{result}"",
                ""connection"": ""{connection}""
            }}";

            FirebaseAnalytics.LogEvent("video_ads_watch", new Parameter[]
            {
                new Parameter("ad_type", adType),
                new Parameter("placement", placement),
                new Parameter("result", result),  // "completed", "skipped", etc.
                new Parameter("connection", connection)
            });

            AppMetrica.ReportEvent("video_ads_watch", jsonParams);
        }

        private void OnInterstitialAdRevenuePaidEvent(string arg1, MaxSdkBase.AdInfo info)
        {
            Logger.CreateText(nameof(AppLovinAds), "OnInterstitialAdRevenuePaidEvent", arg1, JsonUtility.ToJson(info));
        }

        private void OnInterstitialAdClosed(string arg1, MaxSdkBase.AdInfo info)
        {
            Logger.CreateText(nameof(AppLovinAds), "OnInterstitialAdClosed", arg1, JsonUtility.ToJson(info));
            onInterstitialClose?.Invoke(true);

            string adUnitId = string.Empty;
#if UNITY_ANDROID
            adUnitId = configuration.InterstitialAdUnitIdAndroid;
#elif UNITY_IOS
            adUnitId = configuration.InterstitialAdUnitIdIOS;
#endif
            MaxSdk.LoadInterstitial(adUnitId);
            LogVideoAdsWatch("interstitial", "default_placement", "completed");
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
            LogVideoAdsAvailable("interstitial", "default_placement");
        }

        private void OnRewardedAdRevenuePaidEvent(string arg1, MaxSdkBase.AdInfo info)
        {
            Logger.CreateText(nameof(AppLovinAds), "OnRewardedAdRevenuePaidEvent", arg1, JsonUtility.ToJson(info));
        }

        private void OnRewardedAdReceivedReward(string arg1, MaxSdkBase.Reward reward, MaxSdkBase.AdInfo info)
        {
            Logger.CreateText(nameof(AppLovinAds), "OnRewardedAdReceivedReward", arg1, JsonUtility.ToJson(reward), JsonUtility.ToJson(info));
            isRewardedSuccess = true;
            LogVideoAdsWatch("rewarded", rewardedTag ?? "default_placement", "completed");
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
            LogVideoAdsAvailable("rewarded", rewardedTag ?? "default_placement");
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

        protected override void InvokeInterstitialImpl(Action onOpen = null, Action<bool> onClose = null)
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
                LogVideoAdsStarted("interstitial", "default_placement");
            }
            else
            {
                Logger.CreateText(nameof(AppLovinAds), "Interstitial ad not ready");
                onClose?.Invoke(false);
            }
        }

        protected override void InvokeRewardedImpl(Action onOpen = null, Action<bool> onClose = null, string rewardTag = null)
        {
            string adUnitId = string.Empty;
#if UNITY_ANDROID
            adUnitId = configuration.RewardedAdUnitIdAndroid;
#elif UNITY_IOS
            adUnitId = configuration.RewardedAdUnitIdIOS;
#endif
            rewardedTag = rewardTag;
            onRewardedClose = onClose;

            if (MaxSdk.IsRewardedAdReady(adUnitId))
            {
                onOpen?.Invoke();
                MaxSdk.ShowRewardedAd(adUnitId);
                LogVideoAdsStarted("rewarded", rewardTag ?? "default_placement");
            }
            else
            {
                Logger.CreateText(nameof(AppLovinAds), "Rewarded ad not ready");
                onClose?.Invoke(false);
            }

        }

    }

}