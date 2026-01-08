// EfcToXamarinAndroid.Core/Services/ISmsReader.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EfcToXamarinAndroid.Core.Configs.ManagerCore;

namespace EfcToXamarinAndroid.Core.Services
{
	public interface ISmsReader
	{
		event EventHandler<Sms> SmsReceived;

		Task<List<Sms>> GetAllSmsAsync(List<BankConfiguration> addressFilter);

		void StartListening(List<BankConfiguration> addressFilter);
		void StopListening();
	}
}