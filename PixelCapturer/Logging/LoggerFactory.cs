using System;

namespace PixelCapturer.Logging
{
    public class LoggerFactory
    {
        private static Func<Type, ILogger> _factory = type => new ConsoleLogger();
        
        public static ILogger Create<TClass>()
        {
            return _factory(typeof(TClass));
        }

        public static void Set(Func<Type, ILogger> loggerFactory)
        {
            _factory = loggerFactory;
        }
    }
}