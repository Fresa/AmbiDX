using System;

namespace PixelCapturer.Logging
{
    public class TypeLoggerDecorator : ILogger
    {
        private readonly Type _type;
        private readonly ILogger _logger;

        public TypeLoggerDecorator(Type type, ILogger logger)
        {
            _type = type;
            _logger = logger;
        }

        public void Log(string message, params object[] parameters)
        {
            _logger.Log(_type.Name + ": " + message, parameters);
        }
    }
}