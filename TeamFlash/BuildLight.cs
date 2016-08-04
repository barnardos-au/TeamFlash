namespace TeamFlash
{
    public class BuildLight : IBuildLight
    {
        private readonly ILogger logger;

        public BuildLight(ILogger logger)
        {
            this.logger = logger;
        }

        readonly Monitor monitor = new Monitor();

        public void Success()
        {
            VerboseThemeChange("GREEN");
            monitor.SetLed(DelcomBuildIndicator.REDLED, false, false);
            monitor.SetLed(DelcomBuildIndicator.GREENLED, true, false);
            monitor.SetLed(DelcomBuildIndicator.BLUELED, false, false);
        }

        public void Warning()
        {
            VerboseThemeChange("AMBER");
            monitor.SetLed(DelcomBuildIndicator.REDLED, false, false);
            monitor.SetLed(DelcomBuildIndicator.GREENLED, false, false);
            monitor.SetLed(DelcomBuildIndicator.BLUELED, true, false);
        }

        public void Fail()
        {
            VerboseThemeChange("RED");
            monitor.SetLed(DelcomBuildIndicator.REDLED, true, false);
            monitor.SetLed(DelcomBuildIndicator.GREENLED, false, false);
            monitor.SetLed(DelcomBuildIndicator.BLUELED, false, false);
        }

        public void Off()
        {
            VerboseThemeChange("OFF");
            monitor.SetLed(DelcomBuildIndicator.REDLED, false, false);
            monitor.SetLed(DelcomBuildIndicator.GREENLED, false, false);
            monitor.SetLed(DelcomBuildIndicator.BLUELED, false, false);
        }

        private void VerboseThemeChange(string newTheme)
        {
            logger.Verbose("Switching LED to {0}.", newTheme);
        }
    }
}