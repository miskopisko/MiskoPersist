using log4net;
using MiskoPersist.Core;
using MiskoPersist.Message.Request;
using MiskoPersist.Message.Response;

namespace MiskoPersist.Message
{
	public abstract class MessageWrapper
    {
        private static ILog Log = LogManager.GetLogger(typeof(MessageWrapper));

        #region Fields

        private readonly RequestMessage mRequest_;
        private readonly ResponseMessage mResponse_;

        #endregion

        #region Properties

        public RequestMessage Request { get { return mRequest_; } }
        public ResponseMessage Response { get { return mResponse_; } }

        #endregion

        #region Constructors

		protected MessageWrapper()
		{
		}

        protected MessageWrapper(RequestMessage request, ResponseMessage response)
        {
            mRequest_ = request;
            mResponse_ = response;
        }

        #endregion

        public abstract void Execute(Session session);
    }
}