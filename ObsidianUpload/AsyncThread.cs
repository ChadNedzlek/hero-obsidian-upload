using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace ObsidianUpload
{
	public abstract class AsyncThread<T>
	{
		private TaskCompletionSource<T> _completionSource = new TaskCompletionSource<T>();

		protected abstract T Run();

		protected void Execute()
		{
			try
			{
				var result = Run();
				_completionSource.SetResult(result);
			}
			catch (Exception e)
			{
				_completionSource.SetException(e);
			}
		}

		public Task<T> ExecuteAsync()
		{
			Thread thread = new Thread(Execute);
			thread.Start();
			return _completionSource.Task;
		}

		public Task<T> ExecuteAsync(ApartmentState apartmentState)
		{
			Thread thread = new Thread(Execute);
			thread.SetApartmentState(apartmentState);
			thread.Start();
			return _completionSource.Task;
		}
	}
}