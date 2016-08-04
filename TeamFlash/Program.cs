using Topshelf;

namespace TeamFlash
{
    class Program
    {
        private static void Main()
        {
            HostFactory.Run(c =>
            {
                c.Service<TeamFlashService>(service =>
                {
                    service.ConstructUsing(() => new TeamFlashService());
                    service.WhenStarted((s, h) => s.Start(h));
                    service.WhenStopped((s, h) => s.Stop(h));
                });

                c.RunAsLocalSystem();
                c.SetDescription("Build light driver for TeamCity");
                c.SetDisplayName("TeamFlash");
                c.SetServiceName("TeamFlashService");

                c.EnablePauseAndContinue();

                c.EnableServiceRecovery(r => { r.RestartService(1); });
            });

        }
    }
}
