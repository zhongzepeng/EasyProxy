namespace EasyProxy.HttpServer.Middleware
{
    public class MiddlewareChainBuilder
    {
        private IMiddleware first;
        private IMiddleware current;

        public void Use(IMiddleware middleware)
        {
            if (first == null)
            {
                first = middleware;
                current = first;
            }
            else
            {
                current.Next = middleware;
                current = current.Next;
            }
        }

        public IMiddleware Build()
        {
            return first;
        }
    }
}
