using System;
using System.Collections.Generic;
using System.Reflection;
using Ninject;
using Stylet;

namespace Server.LiteNetLib.Wpf
{
    public class NinjectBootstrapper<TRootViewModel> : BootstrapperBase where TRootViewModel : class
    {
        private IKernelConfiguration _kernelConfiguration;
        private IReadOnlyKernel _kernel;

        private object _rootViewModel;
        protected virtual object RootViewModel => _rootViewModel ?? (_rootViewModel = GetInstance(typeof(TRootViewModel)));

        protected override void ConfigureBootstrapper()
        {
            _kernelConfiguration = new KernelConfiguration();
            DefaultConfigureIoC(_kernelConfiguration);
            ConfigureIoC(_kernelConfiguration);
            _kernel = _kernelConfiguration.BuildReadonlyKernel();
        }

        /// <summary>
        /// Carries out default configuration of the IoC container. Override if you don't want to do this
        /// </summary>
        protected virtual void DefaultConfigureIoC(IKernelConfiguration kernel)
        {
            var viewManagerConfig = new ViewManagerConfig()
            {
                ViewFactory = GetInstance,
                ViewAssemblies = new List<Assembly>() { GetType().Assembly }
            };
            kernel.Bind<IViewManager>().ToConstant(new ViewManager(viewManagerConfig));

            kernel.Bind<IWindowManagerConfig>().ToConstant(this).InTransientScope();
            kernel.Bind<IWindowManager>().ToMethod(c => new WindowManager(c.Kernel.Get<IViewManager>(), () => c.Kernel.Get<IMessageBoxViewModel>(), c.Kernel.Get<IWindowManagerConfig>())).InSingletonScope();
            kernel.Bind<IEventAggregator>().To<EventAggregator>().InSingletonScope();
            kernel.Bind<IMessageBoxViewModel>().To<MessageBoxViewModel>(); // Not singleton!
        }

        /// <summary>
        /// Override to add your own types to the IoC container.
        /// </summary>
        protected virtual void ConfigureIoC(IKernelConfiguration kernel) { }

        public override object GetInstance(Type type)
        {
            return _kernel.Get(type);
        }

        protected override void Launch()
        {
            base.DisplayRootView(RootViewModel);
        }

        public override void Dispose()
        {
            base.Dispose();
            ScreenExtensions.TryDispose(_rootViewModel);
            _kernel?.Dispose();
        }
    }
}
