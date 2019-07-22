namespace Zen.Web
{
    public enum EHttpMethod
    {
        Delete, // Represents an HTTP DELETE protocol method.
        Get, // Represents an HTTP GET protocol method.
        Head, // Represents an HTTP HEAD protocol method. The HEAD method is identical to GET except that the server only returns message-headers in the response, without a message-body.
        Options, // Represents an HTTP OPTIONS protocol method.
        Patch, // Represents an HTTP POST protocol method that is used to post a new entity as an addition to a URI.
        Post, // Represents an HTTP POST protocol method that is used to post a new entity as an addition to a URI.
        Put, // Represents an HTTP PUT protocol method that is used to replace an entity identified by a URI.
        Trace // Represents an HTTP TRACE protocol method.
    }
}