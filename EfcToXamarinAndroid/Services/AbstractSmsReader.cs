// EfcToXamarinAndroid.Core/Services/AbstractSmsReader.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EfcToXamarinAndroid.Core.Configs.ManagerCore;

namespace EfcToXamarinAndroid.Core.Services
{
	public abstract class AbstractSmsReader : ISmsReader
	{
		public event EventHandler<Sms> SmsReceived;

		protected List<BankConfiguration> AddressFilter { get; private set; } = new List<BankConfiguration>();

		public abstract Task<List<Sms>> GetAllSmsAsync(List<BankConfiguration> addressFilter);

		public virtual void StartListening(List<BankConfiguration> addressFilter)
		{
			AddressFilter = addressFilter ?? new List<BankConfiguration>();
			OnStartListening();
		}

		public virtual void StopListening()
		{
			OnStopListening();
		}

		protected virtual void OnStartListening() { }
		protected virtual void OnStopListening() { }

		protected bool ShouldIncludeSms(Sms sms)
		{
			if (sms == null || string.IsNullOrEmpty(sms.Address))
				return false;
			return AddressFilter.Any(config => config.SmsNumber == sms.Address);
		}

		protected void RaiseSmsReceived(Sms sms)
		{
			SmsReceived?.Invoke(this, sms);
		}
	}
}