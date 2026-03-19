using LAIMS.Models.Premiums;

namespace LAIMS.Interfaces.Premiums
{
    public interface IPaymentTypeRepository
    {
        List<PaymentType> GetAllPaymentTypes();
        PaymentType GetPaymentTypeById(int id);
        void AddPaymentType(PaymentType paymentType);
        void UpdatePaymentType(PaymentType paymentType);
        void DeletePaymentType(int id);
    }
}
