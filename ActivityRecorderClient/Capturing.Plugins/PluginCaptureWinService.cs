using JobCTRL.Plugins;
using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Tct.ActivityRecorderClient.Capturing.Core;
using Tct.ActivityRecorderClient.Capturing.Plugins.Impl;
using Tct.ActivityRecorderClient.Capturing.Plugins.Ocr;

namespace Tct.ActivityRecorderClient.Capturing.Plugins
{
	//dummy quick and dirty implementation
	public class PluginCaptureWinService : PluginCaptureService
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private static readonly Dictionary<string, Func<PluginStartInfoDetails, ICaptureExtension>> knownPlugins = new Dictionary<string, Func<PluginStartInfoDetails, ICaptureExtension>>();

		static PluginCaptureWinService()
		{
			AddPlugin((p) => new PluginMdiClient());
			AddPlugin((p) => new PluginIkr());
			AddPlugin((p) => new PluginWindowHandle());
			AddPlugin((p) => new CachingCaptureExtensionWrapper(() => new PluginDocumentInfo(), TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(15), CachingCaptureExtensionWrapper.KeySelectorTitleAndHwnd));
			AddPlugin((p) => new CachingCaptureExtensionWrapper(() => new PluginOffice(), TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(15), CachingCaptureExtensionWrapper.KeySelectorTitleAndHwnd));
			AddPlugin((p) => new CachingCaptureExtensionWrapper(() => new PluginWord(), TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(15), CachingCaptureExtensionWrapper.KeySelectorTitleAndHwnd));
			AddPlugin((p) => new CachingCaptureExtensionWrapper(() => new PluginExcel(), TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(15), CachingCaptureExtensionWrapper.KeySelectorTitleAndHwnd));
			AddPlugin((p) => new CachingCaptureExtensionWrapper(() => new PluginPowerPoint(), TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(15), CachingCaptureExtensionWrapper.KeySelectorTitleAndHwnd));
			AddPlugin((p) => new CachingCaptureExtensionWrapper(() => new PluginAcrobat(), TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(15), CachingCaptureExtensionWrapper.KeySelectorTitleAndHwnd));
			AddPlugin((p) => new CachingCaptureExtensionWrapper(() => new PluginAutoCad(), TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(15), CachingCaptureExtensionWrapper.KeySelectorTitleAndHwnd));
			AddPlugin((p) => new CachingCaptureExtensionWrapper(() => new PluginGoogleDrive(), TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(15), CachingCaptureExtensionWrapper.KeySelectorTitleAndHwnd));
			AddPluginWithId(PluginClipboard.PluginId, (p) => new PluginClipboard());
			AddPlugin((p) => new CachingCaptureExtensionWrapper(() => new PluginInternetExplorer(), TimeSpan.FromSeconds(1), TimeSpan.FromMinutes(1), CachingCaptureExtensionWrapper.KeySelectorTitleAndHwnd));
			AddPlugin((p) => new PluginInternetExplorerEmbedded());
			AddPlugin((p) => new PluginExternalText(WindowExternalTextHelper.Instance));
			AddPluginWithId(PluginFirefox.PluginId, (p) => new CachingCaptureExtensionWrapper(() => new PluginFirefox(), TimeSpan.FromSeconds(1), TimeSpan.FromMinutes(1), CachingCaptureExtensionWrapper.KeySelectorTitleAndHwnd));
			AddPluginWithId(PluginChrome.PluginId, (p) => new CachingCaptureExtensionWrapper(() => new PluginChrome(), TimeSpan.FromSeconds(1), TimeSpan.FromMinutes(1), CachingCaptureExtensionWrapper.KeySelectorTitleAndHwnd));
			AddPluginWithId(PluginEdge.PluginId, (p) => new CachingCaptureExtensionWrapper(() => new PluginEdge(), TimeSpan.FromSeconds(1), TimeSpan.FromMinutes(1), CachingCaptureExtensionWrapper.KeySelectorTitleAndHwnd));
			AddPluginWithId(PluginEdgeBlink.PluginId, (p) => new CachingCaptureExtensionWrapper(() => new PluginEdgeBlink(), TimeSpan.FromSeconds(1), TimeSpan.FromMinutes(1), CachingCaptureExtensionWrapper.KeySelectorTitleAndHwnd));
			AddPluginWithId(PluginDomCapture.PluginId, (p) => new PluginDomCapture());
			AddPluginWithId(PluginMail.PluginId, (p) => new PluginMail());
			AddPluginWithId(PluginOutlook.PluginId, (p) => new PluginOutlook());
			AddPluginWithId(PluginLotusNotes.PluginId, (p) => new CachingCaptureExtensionWrapper(() => new PluginLotusNotes(), TimeSpan.FromSeconds(PluginMail.CaptureCachingDurationInSeconds), TimeSpan.FromMinutes(5), CachingCaptureExtensionWrapper.KeySelectorTitleAndHwnd));
			AddPluginWithId(PluginGmail.PluginId, (p) => new CachingCaptureExtensionWrapper(() => new PluginGmail(), TimeSpan.FromSeconds(PluginMail.CaptureCachingDurationInSeconds), TimeSpan.FromMinutes(5), CachingCaptureExtensionWrapper.KeySelectorTitleAndHwnd));
			AddPluginWithId(PluginConference.PluginId, (p) => new PluginConference());
			AddPluginWithId(PluginMsTeams.PluginId, (p) => new CachingCaptureExtensionWrapper(() => new PluginMsTeams(), TimeSpan.FromSeconds(5), TimeSpan.FromMinutes(5), CachingCaptureExtensionWrapper.KeySelectorTitleAndHwnd));
			AddPluginWithId(PluginMeet.PluginId, (p) => new CachingCaptureExtensionWrapper(() => new PluginMeet(), TimeSpan.FromSeconds(5), TimeSpan.FromMinutes(5), CachingCaptureExtensionWrapper.KeySelectorTitleAndHwnd));
			AddPluginWithId(PluginDesktopMsTeams.PluginId, (p) => new PluginDesktopMsTeams());
			AddPlugin((p) => new PluginDateTime());
			AddPlugin((p) => new PluginPopupInfo());
			AddPlugin((p) => new PluginAutoCapture());
			AddPlugin((p) => new PluginKeszJD());
			AddPlugin((p) => new PluginKeszUreq());
			AddPlugin((p) => new PluginAlterdataContabil());
			AddPlugin((p) => new PluginAlterdataFiscal());
			AddPlugin((p) => new PluginAlterdataPessoal());
			AddPlugin((p) => new PluginKeszTherefore());
			AddPlugin((p) => new PluginTelekomJazz());
			AddPlugin((p) => new PluginTelekomJazz2());
			AddPlugin((p) => new PluginFocus());
			AddPlugin((p) => new PluginChromeUrl());
			AddPlugin((p) => new PluginFirefoxUrl());
			AddPlugin((p) => new PluginInternetExplorerUrl());
			AddPlugin((p) => new PluginEdgeUrl());
			AddPlugin((p) => new PluginEdgeBlinkUrl());
			AddPlugin((p) => new PluginBraveUrl());
			AddPlugin((p) => new PluginDragonUrl());
			AddPlugin((p) => new PluginOperaUrl());
			AddPlugin((p) => new PluginVivaldiUrl());
			AddPlugin((p) => new CachingCaptureExtensionWrapper(() => new PluginSAP(), TimeSpan.FromSeconds(5), TimeSpan.FromMinutes(5), CachingCaptureExtensionWrapper.KeySelectorTitleAndHwnd));
			AddPlugin((p) => new CachingCaptureExtensionWrapper(() => new PluginFileHandles(), TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(15), CachingCaptureExtensionWrapper.KeySelectorTitleAndHwnd));
			AddPlugin((p) => new PluginDominio());
			AddPlugin((p) => new PluginInternalWorkState());
			AddPlugin((p) => new PluginInternalHStat());
			AddPlugin((p) => new PluginCef());
			AddPlugin((p) => new CachingCaptureExtensionWrapper(() => new PluginKontakt(), TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(15), CachingCaptureExtensionWrapper.KeySelectorTitleAndHwnd));
			AddPlugin((p) => new PluginSoftphonePro());
			AddPlugin((p) => new CachingCaptureExtensionWrapper(() => new PluginBurOffice(), TimeSpan.FromSeconds(3), TimeSpan.FromMinutes(5), CachingCaptureExtensionWrapper.KeySelectorTitleAndHwnd));
			AddPlugin(p => new CachingCaptureExtensionWrapper(() => new PluginJavaAccessibility(), TimeSpan.FromSeconds(5), TimeSpan.FromMinutes(5), CachingCaptureExtensionWrapper.KeySelectorTitleAndHwnd));
			AddPlugin((p) => new PluginIAccessibility());
			AddPlugin(p => new PluginScreenAnalyst());
#if OcrPlugin
			AddPluginWithId(PluginOcr.PluginId, (p) => new CachingCaptureExtensionWrapper(() => new PluginOcr(p), TimeSpan.FromSeconds(2), TimeSpan.FromMinutes(15), CachingCaptureExtensionWrapper.KeySelectorTitleAndHwnd));
#else
			AddPlugin((p) => new PluginOcr());
#endif

			CheckIfAllCorePluginsAreAdded();
		}

		//to avoid initialization just for getting an Id
		private static void AddPluginWithId(string id, Func<PluginStartInfoDetails, ICaptureExtension> captureExtensionFactory)
		{
#if DEBUG
			new ClientSettingsManager().LoadSettings(); // we load settings to ensure working of plugins
			AddPlugin(captureExtensionFactory); //we still initialize in DEBUG builds
			return;
#endif
			try
			{
				knownPlugins.Add(id, captureExtensionFactory);
			}
			catch (Exception ex)
			{
				log.ErrorAndFail("Unable to add plugin", ex);
			}
		}

		private static void AddPluginWithId(string id, Func<PluginStartInfoDetails, PluginCompositionBase> captureExtensionFactory)
		{
			var composite = captureExtensionFactory(null);
			compositePluginIds[id] = composite.InnerPluginIds.ToList();
		}

		private static void AddPlugin(Func<PluginStartInfoDetails, ICaptureExtension> captureExtensionFactory)
		{
			try
			{
				ICaptureExtension capExt;
				using ((capExt = captureExtensionFactory(null)) as IDisposable)
				{
					if (capExt == null || capExt.Id == null) throw new Exception("Null extension or Id");
					knownPlugins.Add(capExt.Id, captureExtensionFactory);
				}
			}
			catch (Exception ex)
			{
				log.ErrorAndFail("Unable to load plugin", ex);
			}
		}

		[Conditional("DEBUG")]
		private static void CheckIfAllCorePluginsAreAdded()
		{
			var typePlugin = typeof(ICaptureExtension);
			var typeComposition = typeof(PluginCompositionBase);
			var types = AppDomain.CurrentDomain.GetAssemblies().ToList()
				.SelectMany(n => n.GetTypes())
				.Where(n => !n.IsInterface)
				.Where(n => !n.IsAbstract)
				.Where(n => typePlugin.IsAssignableFrom(n) && !typeComposition.IsAssignableFrom(n))
				.Where(n => !n.IsNested || n.IsPublic)
				.Where(n => n.Name != "StaCaptureExtensionWrapper"
					&& n.Name != "StaSharedCaptureExtensionWrapper`1"
					&& n.Name != "CachingCaptureExtensionWrapper")
				;
			Debug.Assert(knownPlugins.Count == types.Count(), "Some ICaptureExtensions are missing");
		}

		protected override ICaptureExtensionAdapter GetCaptureExtensionWithId(PluginStartInfo pluginStartInfo)
		{
			Func<PluginStartInfoDetails, ICaptureExtension> captureExtensionFactory;
			if (knownPlugins.TryGetValue(pluginStartInfo.PluginId, out captureExtensionFactory))
			{
				return new CaptureExtensionWinAdapter(captureExtensionFactory, pluginStartInfo);
			}
			log.Warn("Unable to find plugin " + pluginStartInfo);
			return null; //todo proper loading / searching for external plugins
		}

		protected override IEnumerable<PluginStartInfo> GetInternalCaptureExtensionSettings()
		{
			if (ConfigManager.AsyncPluginsEnabled)
			{
				yield return new PluginStartInfo { PluginId = PluginChromeUrl.PluginId };
				yield return new PluginStartInfo { PluginId = PluginFirefoxUrl.PluginId };
				yield return new PluginStartInfo { PluginId = PluginInternetExplorerUrl.PluginId };
				yield return new PluginStartInfo { PluginId = PluginEdgeUrl.PluginId };
				yield return new PluginStartInfo { PluginId = PluginEdgeBlinkUrl.PluginId };
				yield return new PluginStartInfo { PluginId = PluginBraveUrl.PluginId };
				yield return new PluginStartInfo { PluginId = PluginDragonUrl.PluginId };
				yield return new PluginStartInfo { PluginId = PluginOperaUrl.PluginId };
				yield return new PluginStartInfo { PluginId = PluginVivaldiUrl.PluginId };
			}
		}
	}
}
