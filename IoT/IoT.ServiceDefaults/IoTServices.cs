namespace IoT.ServiceDefaults
{
    public enum IoTServices
    {
        DeviceManagementApi,
        AlertManagementApi,
        DeviceDataCollector,
        AlertDispatcher
    }

    public static class ExtentionMethods
    {
        public static string ToUniqueId(this IoTServices service)
        {
            return service switch
            {
                IoTServices.DeviceManagementApi => "iot-devicemanagementapi",
                IoTServices.AlertManagementApi => "iot-alertmanagementapi",
                IoTServices.DeviceDataCollector => "iot-devicedatacollector",
                IoTServices.AlertDispatcher => "iot-alertdispatcher",
                _ => throw new NotImplementedException()
            };
        }
    }
}
