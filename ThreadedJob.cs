using System.Collections;
using System.Threading;

public abstract class ThreadJob
{

    /// <summary>
    /// Thread on which the conversion process runs
    /// </summary>
    Thread m_Thread;
    /// <summary>
    /// Whether the conversion process is done
    /// </summary>
    bool m_IsDone;

    /// <summary>
    /// Creates and starts the thread on which the conversion process runs.
    /// </summary>
    public void Start()
    {
        m_Thread = new Thread(Run);
        m_Thread.Start();
    }

    /// <summary>
    /// Waits for the conversion process to finish.
    /// </summary>
    public IEnumerator WaitFor()
    {
        while (!m_IsDone)
        {
            yield return null;
        }
    }

    /// <summary>
    /// Runs the job.
    /// </summary>
    void Run()
    {
        ThreadFunction();
        m_IsDone = true;
    }

    /// <summary>
    /// Specific thread function to run.
    /// </summary>
    protected abstract void ThreadFunction();
}
