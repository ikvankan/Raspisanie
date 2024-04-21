using System.Collections.ObjectModel;

namespace Raspisanie
{
    public static class WC
    {
        public const string ImagePath = @"\images\Item\";
        public const string SessionCart = "ShopingCartSession";

        public const string AdminRole = "Admin";
        public const string CustomerRole = "Customer";

        public const string EmailAdmin = "testpochtadlyavsego@gmail.com";

        public const string Success = "Success";
        public const string Error = "Error";

        public const string StatusPending = "Создан";
        public const string StatusApproved = "Подтверждён";
        public const string StatusInProcess = "Собирается";
        public const string StatusShipped = "Отправлен";
        public const string StatusCancelled = "Отменён";
        public const string StatusRefunded = "Возвращение средств";



        public static readonly IEnumerable<string> ListStatus = new ReadOnlyCollection<string>(
            new List<string>
            {
                StatusApproved,StatusCancelled,StatusInProcess,StatusPending,StatusRefunded,StatusShipped
            });
    }
}
