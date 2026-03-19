using LAIMS.Models.Premiums;
using System;
using System.Data;
namespace LAIMS.Interfaces.Premiums
{
	public interface IExchangeRateRepository
	{
		void SaveExchangeRate(ExchangeRate exchangeRate);
		decimal? GetLatestExchangeRate(int baseCurrency, int otherCurrency);
		DataTable GetLatestExchangeRates();
	}
}
