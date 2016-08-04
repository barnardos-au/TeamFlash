using System;
using System.Configuration;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Topshelf;

namespace TeamFlash
{
    public class TeamFlashService : ServiceControl
    {
        private readonly ILogger logger;
        private readonly IBuildLight buildLight;
        private readonly TeamFlashConfig teamFlashConfig;
        private readonly TimeSpan interval;

        private CancellationTokenSource cancellationTokenSource;
        private CancellationToken cancellationToken;
        private Task teamFlashTask;

        public TeamFlashService()
        {
            var logFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TeamFlash.log");
            logger = new TextLogger(logFile);
            buildLight = new BuildLight(logger);
            teamFlashConfig = ConfigurationManager.AppSettings.MapTo<TeamFlashConfig>();
            interval = TimeSpan.FromSeconds(teamFlashConfig.IntervalSeconds);
        }

        public bool Start(HostControl hostControl)
        {
            cancellationTokenSource = new CancellationTokenSource();
            cancellationToken = cancellationTokenSource.Token;

            teamFlashTask = new TeamFlashMonitor(buildLight, teamFlashConfig, logger, interval)
                .Run(cancellationToken);

            return true;
        }

        public bool Stop(HostControl hostControl)
        {
            cancellationTokenSource.Cancel();

            try
            {
                teamFlashTask.Wait(cancellationToken);
            }
            catch (OperationCanceledException) { }
            finally
            {
                cancellationTokenSource.Dispose();
                buildLight.Off();
            }

            return true;
        }
    }
}
