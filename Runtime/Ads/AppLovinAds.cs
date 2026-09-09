using MirraGames.SDK.Common;
using System;
using System.Collections;
using UnityEngine;
using Logger = MirraGames.SDK.Common.Logger;

namespace MirraGames.SDK.AppLovin
{
    [Provider(typeof(IAds))]
    public class AppLovinAds : CommonAds
    {
        private readonly AppLovinAds_Configuration configuration;
        private readonly IEventDispatcher eventDispatcher;

        private string interstitialAdUnitId;
        private string rewardedAdUnitId;
        private string bannerAdUnitId;

        public AppLovinAds(AppLovinAds_Configuration configuration, IEventAggregator eventAggregator, IEventDispatcher eventDispatcher) : base(eventAggregator)
        {
            this.configuration = configuration;
            this.eventDispatcher = eventDispatcher;
            
            MaxSdkCallbacks.OnSdkInitializedEvent += (MaxSdkBase.SdkConfiguration sdkConfiguration) =>
            {
                Logger.CreateText(nameof(AppLovinAds), "Max SDK Initialized", JsonUtility.ToJson(sdkConfiguration));
#if UNITY_ANDROID
                interstitialAdUnitId = configuration.InterstitialAdUnitIdAndroid;
                rewardedAdUnitId = configuration.RewardedAdUnitIdAndroid;
                bannerAdUnitId = configuration.BannerAdUnitIdAndroid;
#elif UNITY_IOS
                interstitialAdUnitId = configuration.InterstitialAdUnitIdIOS;
                rewardedAdUnitId = configuration.RewardedAdUnitIdIOS;
                bannerAdUnitId = configuration.BannerAdUnitIdIOS;
#endif
                LoadInterstitial();
                LoadRewarded();
                SetInitialized();
            };
            
            MaxSdkCallbacks.Rewarded.OnAdLoadedEvent += OnRewardedAdLoaded;
            MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent += OnRewardedAdFailedToLoad;
            MaxSdkCallbacks.Rewarded.OnAdDisplayedEvent += OnRewardedAdOpen;
            MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent += OnRewardedAdFailedToShow;
            MaxSdkCallbacks.Rewarded.OnAdHiddenEvent += OnRewardedAdClosed;
            
            MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent += OnRewardedAdReceivedReward;

            MaxSdkCallbacks.Interstitial.OnAdLoadedEvent += OnInterstitialAdLoaded;
            MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent += OnInterstitialAdFailedToLoad;
            MaxSdkCallbacks.Interstitial.OnAdDisplayedEvent += OnInterstitialAdOpen;
            MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent += OnInterstitialAdFailedToShow;
            MaxSdkCallbacks.Interstitial.OnAdHiddenEvent += OnInterstitialAdClosed;
            
            MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent += OnRewardedAdRevenuePaidEvent;
            MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent += OnInterstitialAdRevenuePaidEvent;
            MaxSdkCallbacks.Banner.OnAdLoadedEvent += OnBannerAdLoaded;
            MaxSdkCallbacks.Banner.OnAdLoadFailedEvent += OnBannerAdFailedToLoad;
            MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent += OnBannerAdRevenuePaidEvent;

            MaxSdk.InitializeSdk();
        }
        
        #region InterstitialAd

        public override bool IsInterstitialAvailable => true;
        public override bool IsInterstitialReady => MaxSdk.IsInterstitialReady(interstitialAdUnitId);
        
        private Action onInterstitialOpen;
        private Action<bool> onInterstitialClose;
        private int interstitialRetryAttempt;

        private void LoadInterstitial()
        {
            MaxSdk.LoadInterstitial(interstitialAdUnitId);
        }
        
        private void OnInterstitialAdLoaded(string arg1, MaxSdkBase.AdInfo info)
        {
            Logger.CreateText(nameof(AppLovinAds), "OnInterstitialAdLoaded", arg1, JsonUtility.ToJson(info));
            interstitialRetryAttempt = 0;
        }

        private void OnInterstitialAdFailedToLoad(string arg1, MaxSdkBase.ErrorInfo info)
        {
            Logger.CreateText(nameof(AppLovinAds), "OnInterstitialAdFailedToLoad", arg1, JsonUtility.ToJson(info));
            DelayedInvoke(LoadInterstitial, (float) Math.Pow(2, Math.Min(6, ++interstitialRetryAttempt)));
        }

        private void OnInterstitialAdFailedToShow(string arg1, MaxSdkBase.ErrorInfo info1, MaxSdkBase.AdInfo info2)
        {
            Logger.CreateText(nameof(AppLovinAds), "OnInterstitialAdFailedToShow", arg1, JsonUtility.ToJson(info1), JsonUtility.ToJson(info2));
            onInterstitialClose?.Invoke(false);
            LoadInterstitial();
        }

        private void OnInterstitialAdOpen(string arg1, MaxSdkBase.AdInfo info)
        {
            Logger.CreateText(nameof(AppLovinAds), "OnInterstitialAdOpen", arg1, JsonUtility.ToJson(info));
            onInterstitialOpen?.Invoke();
        }

        private void OnInterstitialAdClosed(string arg1, MaxSdkBase.AdInfo info)
        {
            Logger.CreateText(nameof(AppLovinAds), "OnInterstitialAdClosed", arg1, JsonUtility.ToJson(info));
            onInterstitialClose?.Invoke(true);
            LoadInterstitial();
        }

        private void OnInterstitialAdRevenuePaidEvent(string arg1, MaxSdkBase.AdInfo info)
        {
            Logger.CreateText(nameof(AppLovinAds), "OnInterstitialAdRevenuePaidEvent", arg1, JsonUtility.ToJson(info));
        }

        protected override void InvokeInterstitialImpl(InterstitialParameters parameters, Action onOpen, Action<bool> onClose)
        {
            if (IsInterstitialReady)
            {
                onInterstitialClose = onClose;
                onInterstitialOpen = onOpen;
                MaxSdk.ShowInterstitial(interstitialAdUnitId);
            }
            else
            {
                Logger.CreateText(nameof(AppLovinAds), "Interstitial ad not ready");
            }
        }

        #endregion

        #region RewardedAd

        private string rewardedTag;
        private bool isRewardedSuccess;
        
        private Action onRewardedOpen;
        private Action<bool> onRewardedClose;
        private int rewardedRetryAttempt;
        public override bool IsRewardedReady => MaxSdk.IsRewardedAdReady(rewardedAdUnitId);
        public override bool IsRewardedAvailable => true;

        private void OnRewardedAdOpen(string arg1, MaxSdkBase.AdInfo info)
        {
            Logger.CreateText(nameof(AppLovinAds), "OnRewardedAdOpen", arg1, JsonUtility.ToJson(info));
            onRewardedOpen?.Invoke();
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
            LoadRewarded();
        }

        private void OnRewardedAdFailedToShow(string arg1, MaxSdkBase.ErrorInfo info1, MaxSdkBase.AdInfo info2)
        {
            Logger.CreateText(nameof(AppLovinAds), "OnRewardedAdFailedToShow", arg1, JsonUtility.ToJson(info1), JsonUtility.ToJson(info2));
            onRewardedClose?.Invoke(false);
            LoadRewarded();
        }

        private void OnRewardedAdFailedToLoad(string arg1, MaxSdkBase.ErrorInfo info)
        {
            Logger.CreateText(nameof(AppLovinAds), "OnRewardedAdFailedToLoad", arg1, JsonUtility.ToJson(info));
            DelayedInvoke(LoadRewarded, (float) Math.Pow(2, Math.Min(6, ++rewardedRetryAttempt)));
        }

        private void OnRewardedAdLoaded(string arg1, MaxSdkBase.AdInfo info)
        {
            Logger.CreateText(nameof(AppLovinAds), "OnRewardedAdLoaded", arg1, JsonUtility.ToJson(info));
            rewardedRetryAttempt = 0;
        }

        private void LoadRewarded()
        {
            MaxSdk.LoadRewardedAd(rewardedAdUnitId);
        }

        protected override void InvokeRewardedImpl(RewardedParameters parameters, Action onOpen, Action<bool> onClose)
        {
            if (IsRewardedReady)
            {
                rewardedTag = parameters.PlacementId;
                onRewardedClose = onClose;
                onRewardedOpen = onOpen;
                MaxSdk.ShowRewardedAd(rewardedAdUnitId, rewardedTag);
            }
            else
            {
                Logger.CreateText(nameof(AppLovinAds), "Rewarded ad not ready");
            }
        }
        
        #endregion

        #region BannerAd

        private bool isBannerCreated;
        private bool isBannerVisible;
        private int bannerRetryAttempt;

        public override bool IsBannerAvailable => true;
        public override bool IsBannerReady { get; protected set; }
        public override bool IsBannerVisible => isBannerVisible;

        private void CreateBanner()
        {
            MaxSdk.CreateBanner(bannerAdUnitId, MaxSdkBase.BannerPosition.BottomCenter);
            isBannerCreated = true;
        }

        private void LoadBanner()
        {
            if (!isBannerCreated)
            {
                CreateBanner();
                return;
            }
            MaxSdk.LoadBanner(bannerAdUnitId);
        }

        private void OnBannerAdLoaded(string arg1, MaxSdkBase.AdInfo info)
        {
            Logger.CreateText(nameof(AppLovinAds), "OnBannerAdLoaded", arg1, JsonUtility.ToJson(info));
            IsBannerReady = true;
            bannerRetryAttempt = 0;
        }

        private void OnBannerAdFailedToLoad(string arg1, MaxSdkBase.ErrorInfo info)
        {
            Logger.CreateText(nameof(AppLovinAds), "OnBannerAdFailedToLoad", arg1, JsonUtility.ToJson(info));
            IsBannerReady = false;
            isBannerCreated = false;
            DelayedInvoke(LoadBanner, (float) Math.Pow(2, Math.Min(6, ++bannerRetryAttempt)));
        }

        private void OnBannerAdRevenuePaidEvent(string arg1, MaxSdkBase.AdInfo info)
        {
            Logger.CreateText(nameof(AppLovinAds), "OnBannerAdRevenuePaidEvent", arg1, JsonUtility.ToJson(info));
        }

        protected override void InvokeBannerImpl()
        {
            LoadBanner();
            MaxSdk.ShowBanner(bannerAdUnitId);
            isBannerVisible = true;
        }

        protected override void RefreshBannerImpl()
        {
            if (isBannerCreated)
            {
                MaxSdk.DestroyBanner(bannerAdUnitId);
            }
            isBannerCreated = false;
            IsBannerReady = false;
            isBannerVisible = false;
            LoadBanner();
            MaxSdk.ShowBanner(bannerAdUnitId);
            isBannerVisible = true;
        }

        protected override void DisableBannerImpl()
        {
            if (!isBannerCreated)
            {
                return;
            }
            MaxSdk.HideBanner(bannerAdUnitId);
            isBannerVisible = false;
        }

        #endregion

        private void DelayedInvoke(Action func, float delay)
        {
            eventDispatcher.StartCoroutine(CorutineFunction());
            IEnumerator CorutineFunction()
            {
                yield return new WaitForSecondsRealtime(delay);
                func();
            }
        }
    }
}
