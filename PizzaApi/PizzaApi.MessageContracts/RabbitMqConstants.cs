namespace PizzaApi.MessageContracts
{
    public static class RabbitMqConstants
    {
        public const string RabbitMqUri = "rabbitmq://localhost/pizzaapi/";
        public const string UserName = "guest";
        public const string Password = "guest";

        public const string RegisterOrderServiceQueue = "registerorder.service";
        public const string DomainOperationRequestQueue = "domain_operation_request";
        public const string DomainOperationRequestSchedulerQueue = "domain_operation_request_scheduler";

        //public const string NotificationServiceQueue = "notification.service";
        public const string SagaQueue = "saga.service";
    }
}