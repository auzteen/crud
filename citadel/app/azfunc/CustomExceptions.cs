using System;
namespace Citadel
{
    public class ExistingAssetsException : Exception
    {
        public ExistingAssetsException()
        {

        }
        public ExistingAssetsException(string message) : base(message)
        {

        }

        public ExistingAssetsException(string message, Exception inner) : base(message, inner)
        {

        }
    }
    public class ExistingCustomerException : Exception
    {
        public ExistingCustomerException()
        {

        }
        public ExistingCustomerException(string message) : base(message)
        {

        }

        public ExistingCustomerException(string message, Exception inner) : base(message, inner)
        {

        }
    }
    public class UnauthorizedAccessException : Exception
    {
        public UnauthorizedAccessException()
        {

        }
        public UnauthorizedAccessException(string message) : base(message)
        {

        }
        public UnauthorizedAccessException(string message, Exception inner) : base(message, inner)
        {

        }
    }

    public class RangePaginationException : Exception
    {
        public RangePaginationException()
        {

        }

        public RangePaginationException(string message) : base(message)
        {

        }
    }

    public class NoRecordsFoundException : Exception
    {
        public NoRecordsFoundException()
        {

        }

        public NoRecordsFoundException(string message) : base(message)
        {

        }
    }

    public class BadRequestException : Exception
    {
        public BadRequestException()
        {

        }

        public BadRequestException(string message) : base(message)
        {

        }
    }
    public class AssetUpdateException : Exception
    {
        public AssetUpdateException()
        {

        }

        public AssetUpdateException(string message) : base(message)
        {

        }
    }

    public class FailedToUpdateAssetException : Exception
    {
        public FailedToUpdateAssetException()
        {

        }

        public FailedToUpdateAssetException(string message) : base(message)
        {

        }
    }

    public class InvalidTokenException : Exception
    {
        public InvalidTokenException()
        {

        }

        public InvalidTokenException(string message) : base(message)
        {

        }
    }
}
