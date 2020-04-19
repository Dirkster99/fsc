namespace FileSystemModels.ViewModels.Semaphores
{
	using System;
	using System.Threading;
	using System.Threading.Tasks;

	/// <summary>
	/// Slightly simplified implemenaton of AsyncSemaphore from
	/// http://blogs.msdn.com/b/pfxteam/archive/2012/02/12/10266988.aspx
	///
	/// by creating an AsyncLock type that supports interaction with the ‘using’ keyword.
	/// Our goal is to be able to achieve the same thing as in the previous code snippet
	/// but instead via code like the following:
	/// 
	/// private readonly AsyncLock m_lock = new AsyncLock(); 
	/// … 
	/// using(var releaser = await m_lock.LockAsync()) 
	/// { 
	///     … // protected code here 
	/// }
	/// </summary>
	public class AsyncLock
	{
		private readonly AsyncSemaphore m_semaphore;
		private readonly Task<Releaser> m_releaser;

		/// <summary>
		/// Class constructor.
		/// </summary>
		public AsyncLock()
		{
			m_semaphore = new AsyncSemaphore(1);
			m_releaser = Task.FromResult(new Releaser(this));
		}

		/// <summary>
		/// Implements the lock method for usage with using keyword.
		/// </summary>
		/// <returns></returns>
		public Task<Releaser> LockAsync()
		{
			var wait = m_semaphore.WaitAsync();
			return wait.IsCompleted ?
				m_releaser :
				wait.ContinueWith((_, state) => new Releaser((AsyncLock)state),
					this, CancellationToken.None,
					TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
		}

		/// <summary>
		/// Implements a releaser structure to support the lock via using functionality.
		/// </summary>
		public struct Releaser : IDisposable
		{
			private readonly AsyncLock m_toRelease;

			internal Releaser(AsyncLock toRelease) { m_toRelease = toRelease; }

			/// <summary>
			/// Disposes the releaser structure.
			/// </summary>
			public void Dispose()
			{
				if (m_toRelease != null)
					m_toRelease.m_semaphore.Release();
			}
		}
	}
}
