using System;
using System.Threading;
using System.Threading.Tasks;

namespace ObsidianUpload
{
	public static class CallbackAsyncThread
	{
		public static Task<T> RunAsync<T>(Func<T> callback)
		{
			return new CallbackAsyncThread<T>(callback).ExecuteAsync();
		}

		public static Task<T> RunAsync<T>(Func<T> callback, ApartmentState apartmentState)
		{
			return new CallbackAsyncThread<T>(callback).ExecuteAsync(apartmentState);
		}
	}

	public class CallbackAsyncThread<T> : AsyncThread<T>
	{
		private Func<T> _callback;

		public CallbackAsyncThread(Func<T> callback)
		{
			_callback = callback;
		}

		protected override T Run()
		{
			return _callback();
		}
	}
}