using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TeamFlash
{
    public class TeamFlashMonitor
    {
        private readonly IBuildLight buildLight;
        private readonly ILogger logger;

        public TeamFlashMonitor(
            IBuildLight buildLight, 
            ILogger logger)
        {
            this.buildLight = buildLight;
            this.logger = logger;
        }

        public async Task Run(CancellationToken token, TeamFlashConfig teamFlashConfig)
        {
            logger.VerboseEnabled = teamFlashConfig.Verbose;

            var interval = TimeSpan.FromSeconds(teamFlashConfig.IntervalSeconds);

            var buildTypeIds = ConvertBuildTypeIdsToArray(teamFlashConfig.BuildTypeIds);
            var buildTypeIdsExcluded = ConvertBuildTypeIdsToArray(teamFlashConfig.BuildTypeIdsExcluded);

            buildLight.Off();

            while (!token.IsCancellationRequested)
            {
                var lastBuildStatus = RetrieveBuildStatus(
                    teamFlashConfig.ServerUrl,
                    teamFlashConfig.Username,
                    teamFlashConfig.Password,
                    buildTypeIds,
                    buildTypeIdsExcluded);
                switch (lastBuildStatus)
                {
                    case BuildStatus.Unavailable:
                        buildLight.Off();
                        logger.WriteLine("Build status not available");
                        break;
                    case BuildStatus.Passed:
                        buildLight.Success();
                        logger.WriteLine("Passed");
                        break;
                    case BuildStatus.Investigating:
                        buildLight.Warning();
                        logger.WriteLine("Investigating");
                        break;
                    case BuildStatus.Failed:
                        buildLight.Fail();
                        logger.WriteLine("Failed");
                        break;
                }

                logger.Verbose(string.Format("Waiting for {0} seconds.", interval.Seconds));

                await Task.Delay(interval, token);
            }
        }

        private static string[] ConvertBuildTypeIdsToArray(string buildTypeIds)
        {
            return buildTypeIds == "*"
                ? new string[0]
                : buildTypeIds.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToArray();
        }

        private BuildStatus RetrieveBuildStatus(
            string serverUrl, string username, string password,
            IEnumerable<string> buildTypeIds, IEnumerable<string> buildTypeIdsExcluded)
        {
            logger.Verbose("Checking build status.");

            buildTypeIds = buildTypeIds.ToArray();
            buildTypeIdsExcluded = buildTypeIdsExcluded.ToArray();

            dynamic query = new Query(logger, serverUrl, username, password);

            var buildStatus = BuildStatus.Passed;

            try
            {
                var couldFindProjects = false;
                foreach (var project in query.Projects)
                {
                    couldFindProjects = true;
                    logger.Verbose("Checking Project '{0}'.", project.Name);
                    if (!project.BuildTypesExists)
                    {
                        logger.Verbose("Bypassing Project '{0}' because it has no 'BuiltTypes' property defined.", project.Name);
                        continue;
                    }

                    foreach (var buildType in project.BuildTypes)
                    {
                        logger.Verbose("Checking Built Type '{0}\\{1}'.", project.Name, buildType.Name);
                        if ((buildTypeIds.Any() &&
                            buildTypeIds.All(id => id != buildType.Id)) ||
                            (buildTypeIdsExcluded.Any() &&
                            buildTypeIdsExcluded.All(id => id == buildType.id)))
                        {
                            logger.Verbose("Bypassing Built Type '{0}\\{1}' because it does NOT match configured built-type list to monitor.", project.Name, buildType.Name);
                            continue;
                        }

                        if (buildType.PausedExists && "true".Equals(buildType.Paused, StringComparison.CurrentCultureIgnoreCase))
                        {
                            logger.Verbose("Bypassing Built Type '{0}\\{1}' because it has 'Paused' property set to 'true'.", project.Name, buildType.Name);
                            continue;
                        }

                        var builds = buildType.Builds;
                        var latestBuild = builds.First;
                        if (latestBuild == null)
                        {
                            logger.Verbose("Bypassing Built Type '{0}\\{1}' because no built history is available to it yet.", project.Name, buildType.Name);
                            continue;
                        }

                        if ("success".Equals(latestBuild.Status, StringComparison.CurrentCultureIgnoreCase))
                        {
                            dynamic runningBuild = new Query(logger, serverUrl, username, password) { RestBasePath = string.Format("/httpAuth/app/rest/buildTypes/id:{0}/builds/running:any", buildType.Id) };

                            runningBuild.Load();
                            if ("success".Equals(runningBuild.Status, StringComparison.CurrentCultureIgnoreCase))
                            {
                                logger.Verbose("Bypassing Built Type '{0}\\{1}' because status of last build and all running builds are 'success'.", project.Name, buildType.Name);
                                continue;
                            }
                        }

                        if (latestBuild.PropertiesExists)
                        {
                            var isUnstableBuild = false;
                            foreach (var property in latestBuild.Properties)
                            {
                                if (
                                    "system.BuildState".Equals(property.Name, StringComparison.CurrentCultureIgnoreCase) &&
                                    "unstable".Equals(property.Value, StringComparison.CurrentCultureIgnoreCase))
                                {
                                    isUnstableBuild = true;
                                }

                                if ("BuildState".Equals(property.Name, StringComparison.CurrentCultureIgnoreCase) &&
                                    "unstable".Equals(property.Value, StringComparison.CurrentCultureIgnoreCase))
                                {
                                    isUnstableBuild = true;

                                }
                            }

                            if (isUnstableBuild)
                            {
                                logger.Verbose("Bypassing Built Type '{0}\\{1}' because it is marked as 'unstable'.", project.Name, buildType.Name);
                                continue;
                            }
                        }

                        logger.Verbose("Now checking investigation status of Built Type '{0}\\{1}'.", project.Name, buildType.Name);
                        var buildId = buildType.Id;
                        dynamic investigationQuery = new Query(logger, serverUrl, username, password);
                        investigationQuery.RestBasePath = @"/httpAuth/app/rest/buildTypes/id:" + buildId + @"/";
                        buildStatus = BuildStatus.Failed;

                        foreach (var investigation in investigationQuery.Investigations)
                        {
                            var investigationState = investigation.State;
                            if ("taken".Equals(investigationState, StringComparison.CurrentCultureIgnoreCase) ||
                                "fixed".Equals(investigationState, StringComparison.CurrentCultureIgnoreCase))
                            {
                                logger.Verbose("Investigation status of Built Type '{0}\\{1}' detected as either 'taken' or 'fixed'.", project.Name, buildType.Name);
                                buildStatus = BuildStatus.Investigating;
                            }
                        }

                        if (buildStatus == BuildStatus.Failed)
                        {
                            logger.Verbose("Concluding status of Built Type '{0}\\{1}' as FAIL.", project.Name, buildType.Name);
                            return BuildStatus.Failed;
                        }
                    }

                }

                if (!couldFindProjects)
                {
                    logger.Verbose("No Projects found! Please ensure if TeamCity URL is valid and also TeamCity setup and credentials are correct.");
                    return BuildStatus.Unavailable;
                }
            }
            catch (Exception exception)
            {
                logger.Error(exception);
                return BuildStatus.Unavailable;
            }

            return buildStatus;
        }
    }
}
