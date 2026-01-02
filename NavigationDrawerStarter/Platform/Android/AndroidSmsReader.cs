using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.Content;
using Android.OS;
using Android.Telephony;
using EfcToXamarinAndroid.Core;
using EfcToXamarinAndroid.Core.Configs.ManagerCore;
using EfcToXamarinAndroid.Core.Services;

namespace NavigationDrawerStarter.Platform.Android
{
	public class AndroidSmsReader : AbstractSmsReader
	{
		private readonly Context _context;
		private SmsBroadcastReceiver _receiver;
		private bool _listening;

		public AndroidSmsReader(Context context)
		{
			_context = context;
		}

		public override async Task<List<Sms>> GetAllSmsAsync(List<BankConfiguration> addressFilter)
		{
			var result = new List<Sms>();
			var contentResolver = _context.ContentResolver;
			var uri = Android.Net.Uri.Parse("content://sms/");

			await Task.Run(() =>
			{
				using (var c = contentResolver.Query(uri, null, null, null, null))
				{
					if (c != null && c.MoveToFirst())
					{
						do
						{
							var address = c.GetString(c.GetColumnIndexOrThrow("address"));
							if (addressFilter.Select(x => x.SmsNumber).Contains(address))
							{
								var sms = new Sms();
								sms.setId(c.GetString(c.GetColumnIndexOrThrow("_id")));
								sms.setAddress(address);
								sms.setMsg(c.GetString(c.GetColumnIndexOrThrow("body")));
								sms.setReadState(c.GetString(c.GetColumnIndex("read")));
								sms.setTime(c.GetString(c.GetColumnIndexOrThrow("date")));
								var type = c.GetString(c.GetColumnIndexOrThrow("type"));
								sms.setFolderName(type != null && type.Contains("1") ? "inbox" : "sent");
								result.Add(sms);
							}
						}
						while (c.MoveToNext());
					}
				}
			});

			return result;
		}

		protected override void OnStartListening()
		{
			if (_listening)
				return;
			_receiver = new SmsBroadcastReceiver(ShouldIncludeSms, RaiseSmsReceived);
			_context.RegisterReceiver(_receiver, new IntentFilter("android.provider.Telephony.SMS_RECEIVED"));
			_listening = true;
		}

		protected override void OnStopListening()
		{
			if (!_listening)
				return;
			_context.UnregisterReceiver(_receiver);
			_receiver?.Dispose();
			_receiver = null;
			_listening = false;
		}

		private class SmsBroadcastReceiver : BroadcastReceiver
		{
			private readonly Func<Sms, bool> _filter;
			private readonly Action<Sms> _onSms;

			public SmsBroadcastReceiver(Func<Sms, bool> filter, Action<Sms> onSms)
			{
				_filter = filter;
				_onSms = onSms;
			}

			public override void OnReceive(Context context, Intent intent)
			{
				if (intent?.Action != "android.provider.Telephony.SMS_RECEIVED")
					return;

				if (Build.VERSION.SdkInt >= BuildVersionCodes.Kitkat)
				{
					SmsMessage[] msgs = Telephony.Sms.Intents.GetMessagesFromIntent(intent);
					foreach (var msg in msgs)
					{
						var sms = new Sms();
						sms.setMsg(msg.MessageBody?.ToString());
						sms.setTime(msg.TimestampMillis.ToString());
						sms.setId("0");
						sms.setAddress(msg.OriginatingAddress?.ToString());
						if (_filter(sms))
						{
							_onSms(sms);
						}
					}
				}
			}
		}
	}
} 