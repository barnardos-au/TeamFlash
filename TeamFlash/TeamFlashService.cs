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

        private CancellationTokenSource cancellationTokenSource;
        private CancellationToken cancellationToken;
        private Task teamFlashTask;

        public TeamFlashService()
        {
            var logFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TeamFlash.log");
            logger = new TextLogger(logFile);
            buildLight = new BuildLight(logger);
        }

        public bool Start(HostControl hostControl)
        {
            var teamFlashConfig = ConfigurationManager.AppSettings.MapTo<TeamFlashConfig>();

            cancellationTokenSource = new CancellationTokenSource();
            cancellationToken = cancellationTokenSource.Token;

            teamFlashTask = new TeamFlashMonitor(buildLight, logger)
                .Run(cancellationToken, teamFlashConfig);

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
