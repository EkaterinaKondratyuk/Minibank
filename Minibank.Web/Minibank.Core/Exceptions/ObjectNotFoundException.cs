using System;

namespace Minibank.Core
{
    public class ObjectNotFoundException : Exception
    {
        public ObjectNotFoundException()
        {
        }

        public ObjectNotFoundException(string message) : base(message)
        {
        }

        public ObjectNotFoundException(string message, Exception inner) : base(message, inner)
        {
        }

        public ObjectNotFoundException(string objectType, string objectId)
        {
        }
    }
}